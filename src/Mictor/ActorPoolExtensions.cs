using System;
using System.Threading.Tasks;

namespace Mictor
{
    public static class ActorPoolExtensions
    {
        public static void Enqueue(this IActorPool actorPool, string key, Func<Task> work)
        {
            using IActor actor = actorPool.GetOrCreate(key);

            actor.Enqueue(work);
        }
    }
}
