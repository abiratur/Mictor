using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mictor
{
    public class ActorPool : IActorPool
    {
        private readonly ConcurrentDictionary<string, Actor> _actors = new ConcurrentDictionary<string, Actor>();

        public static ActorPool Shared { get; } = new ActorPool();

        public IActor GetOrCreate(string key)
        {
            while (true)
            {
                if (_actors.TryGetValue(key, out var actor))
                {
                    if (actor.TryCreateHandle(out ActorHandle? actorHandle))
                    {
                        return actorHandle!;
                    }
                }

                actor = Actor.Create(this, key, out var handle);

                // 2 threads can reach this point, but only one can enter the "if"

                if (_actors.TryAdd(key, actor))
                {
                    actor.Start();
                    return handle;
                }
            }
        }

        public ActorPoolSnapshot TakeSnapshot()
        {
            Dictionary<string, ActorSnapshot> dict = _actors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.TakeSnapshot()
            );

            return new ActorPoolSnapshot(dict);
        }

        internal void Return(Actor actor)
        {
            if (!_actors.TryRemove(actor.Key, out _))
            {
                throw new InvalidOperationException(); // should never happen
            }
        }
    }
}
