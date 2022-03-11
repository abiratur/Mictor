using System;
using System.Threading.Tasks;

namespace Mictor
{
    /// <summary>
    /// Extensions for <see cref="IActorPool"/>
    /// </summary>
    public static class ActorPoolExtensions
    {
        /// <summary>
        /// Enqueue work to an actor with the specified key
        /// </summary>
        public static void Enqueue(this IActorPool actorPool, string key, Func<Task> work)
        {
            using IActor actor = actorPool.GetOrCreate(key);

            actor.Enqueue(work);
        }
    }
}
