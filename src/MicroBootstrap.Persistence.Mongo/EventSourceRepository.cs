namespace MicroBootstrap.Persistence.Mongo
{
    public class EventSourceRepository<TContext, TEvent> : IEventRepository<TContext, TEvent>
        where TContext : MongoDbContext
        where TEvent : IEvent
    {
        protected readonly IMongoCollection<TEvent> DbSet;

        public EventSourceRepository(TContext context)
        {
            DbSet = context.GetCollection<TEvent>();
        }

        public Task DeleteEvent(TEvent @event, CancellationToken cancellationToken)
            => DbSet.DeleteOneAsync(Builders<TEvent>.Filter.Eq("_id", @event.EventId), cancellationToken);

        public Task DeleteRangeEvent(List<TEvent> events, CancellationToken cancellationToken)
            => DbSet.DeleteManyAsync(
                Builders<TEvent>.Filter.In("_id", events.Select(x => x.EventId)),
                cancellationToken);

        public Task InsertEvent(TEvent @event, CancellationToken cancellationToken)
            => DbSet.InsertOneAsync(@event, new InsertOneOptions { BypassDocumentValidation = false },
                cancellationToken);

        public Task InsertRangeEvent(List<TEvent> events, CancellationToken cancellationToken)
            => DbSet.InsertManyAsync(events, new InsertManyOptions { BypassDocumentValidation = false },
                cancellationToken);

        public Task UpdateEvent(TEvent @event, CancellationToken cancellationToken)
            => DbSet.ReplaceOneAsync(
                Builders<TEvent>.Filter.Eq("_id", @event.EventId),
                @event,
                new UpdateOptions
                {
                    IsUpsert = false
                },
                cancellationToken);

        public Task UpdateRangeEvent(List<TEvent> events, CancellationToken cancellationToken)
        {
            List<Func<Task>> updatedTasks = new List<Func<Task>>();

            events.ForEach(item =>
            {
                updatedTasks.Add(() => DbSet.ReplaceOneAsync(
                    Builders<TEvent>.Filter.Eq("_id", item.EventId),
                    item,
                    new UpdateOptions { IsUpsert = false },
                    cancellationToken));
            });

            var commandTasks = updatedTasks.Select(c => c());

            return Task.WhenAll(commandTasks);
        }
    }
}
