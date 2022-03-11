namespace Mictor
{
    /// <summary>
    /// Represents a pool of actors managed by keys
    /// </summary>
    public interface IActorPool
    {
        /// <summary>
        /// Gets or creates an actor
        /// </summary>
        /// <remarks>The actor must be disposed at the end of use</remarks>
        /// <param name="key">The actor id</param>
        /// <returns>An actor reference</returns>
        IActor GetOrCreate(string key);
    }
}