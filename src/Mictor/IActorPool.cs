namespace Mictor
{
    public interface IActorPool
    {
        /// <summary>
        /// Gets or creates an <see cref="IActor"/> with a key
        /// </summary>
        /// <param name="key">The id of the actor</param>
        /// <returns></returns>
        IActor GetOrCreate(string key);
    }
}