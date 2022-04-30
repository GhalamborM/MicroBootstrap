namespace MicroBootstrap.Abstractions.Core.Domain.Model;

public interface IHaveVersion
{
    /// <summary>
    /// Gets the original version is the aggregate version we got from the store.
    /// It is used for optimistic concurrency, to check if there were no changes made to the
    /// aggregate state between load and save for the current operation.
    /// </summary>
    long OriginalVersion { get; }

    /// <summary>
    /// Gets the current version is set to the original version when the aggregate is loaded from the store.
    /// It should increase for each state transition performed within the scope of the current operation.
    /// </summary>
    long CurrentVersion { get; }
}