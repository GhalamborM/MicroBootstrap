namespace MicroBootstrap.Persistence.EventStoreDB.Subscriptions;

public class SubscribeToAllBackgroundWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventStoreClient _eventStoreClient;
    private readonly ISubscriptionCheckpointRepository _checkpointRepository;
    private readonly ILogger<SubscribeToAllBackgroundWorker> _logger;
    private readonly string _subscriptionId;
    private readonly SubscriptionFilterOptions? _filterOptions;
    private readonly Action<EventStoreClientOperationOptions>? _configureOperation;
    private readonly UserCredentials? _credentials;
    private readonly object _resubscribeLock = new();
    private Task? _executingTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public SubscribeToAllBackgroundWorker(
        IServiceProvider serviceProvider,
        EventStoreClient eventStoreClient,
        ISubscriptionCheckpointRepository checkpointRepository,
        ILogger<SubscribeToAllBackgroundWorker> logger,
        string subscriptionId,
        SubscriptionFilterOptions? filterOptions = null,
        Action<EventStoreClientOperationOptions>? configureOperation = null,
        UserCredentials? credentials = null
    )
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
        _checkpointRepository =
            checkpointRepository ?? throw new ArgumentNullException(nameof(checkpointRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscriptionId = subscriptionId;
        _configureOperation = configureOperation;
        _credentials = credentials;
        _filterOptions = filterOptions ?? new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents());
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Create a linked token so we can trigger cancellation outside of this token's cancellation
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _executingTask = SubscribeToAll(_cancellationTokenSource.Token);

        return _executingTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stop called without start
        if (_executingTask == null)
        {
            return;
        }

        // Signal cancellation to the executing method
        _cancellationTokenSource?.Cancel();

        // Wait until the issue completes or the stop token triggers
        await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

        // Throw if cancellation triggered
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Subscription to all '{SubscriptionId}' stopped", _subscriptionId);
        _logger.LogInformation("External Event Consumer stopped");
    }

    private async Task SubscribeToAll(CancellationToken ct)
    {
        _logger.LogInformation("Subscription to all '{SubscriptionId}'", _subscriptionId);

        var checkpoint = await _checkpointRepository.Load(_subscriptionId, ct);

        if (checkpoint != null)
        {
            await _eventStoreClient.SubscribeToAllAsync(
                new Position(checkpoint.Value, checkpoint.Value),
                HandleEvent,
                false,
                HandleDrop,
                _filterOptions,
                _configureOperation,
                _credentials,
                ct
            );
        }
        else
        {
            await _eventStoreClient.SubscribeToAllAsync(
                HandleEvent,
                false,
                HandleDrop,
                _filterOptions,
                _configureOperation,
                _credentials,
                ct
            );
        }

        _logger.LogInformation("Subscription to all '{SubscriptionId}' started", _subscriptionId);
    }

    private async Task HandleEvent(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken ct)
    {
        try
        {
            if (IsEventWithEmptyData(resolvedEvent) || IsCheckpointEvent(resolvedEvent)) return;

            // Create scope to have proper handling of scoped services
            using var scope = _serviceProvider.CreateScope();

            var eventProcessor =
                scope.ServiceProvider.GetRequiredService<IEventProcessor>();

            // publish event to internal event bus
            await eventProcessor.PublishAsync((IEvent)resolvedEvent.Deserialize(), ct);

            await _checkpointRepository.Store(_subscriptionId, resolvedEvent.Event.Position.CommitPosition, ct);
        }
        catch (System.Exception e)
        {
            _logger.LogError(
                "Error consuming message: {ExceptionMessage}{ExceptionStackTrace}",
                e.Message,
                e.StackTrace);
        }
    }

    private bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
    {
        if (resolvedEvent.Event.Data.Length != 0) return false;

        _logger.LogInformation("Event without data received");
        return true;
    }

    private bool IsCheckpointEvent(ResolvedEvent resolvedEvent)
    {
        if (resolvedEvent.Event.EventType != EventTypeMapper.ToName<CheckpointStored>()) return false;

        _logger.LogInformation("Checkpoint event - ignoring");
        return true;
    }

    private void HandleDrop(StreamSubscription _, SubscriptionDroppedReason reason, System.Exception? exception)
    {
        _logger.LogWarning(
            exception,
            "Subscription to all '{SubscriptionId}' dropped with '{Reason}'",
            _subscriptionId,
            reason
        );

        Resubscribe();
    }

    private void Resubscribe()
    {
        while (true)
        {
            var resubscribed = false;
            try
            {
                Monitor.Enter(_resubscribeLock);

                using (NoSynchronizationContextScope.Enter())
                {
                    SubscribeToAll(_cancellationTokenSource!.Token).Wait();
                }

                resubscribed = true;
            }
            catch (System.Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to resubscribe to all '{SubscriptionId}' dropped with '{ExceptionMessage}{ExceptionStackTrace}'",
                    _subscriptionId,
                    exception.Message,
                    exception.StackTrace);
            }
            finally
            {
                Monitor.Exit(_resubscribeLock);
            }

            if (resubscribed)
                break;

            Thread.Sleep(1000);
        }
    }
}
