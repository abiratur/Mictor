using System;
using System.Collections.Concurrent;

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
                // ReSharper disable once InconsistentlySynchronizedField
                if (_actors.TryGetValue(key, out var worker))
                {
                    lock (worker)
                    {
                        // check if the actor has been disposed since we took the value
                        // if it was, then create it again
                        if (worker.Consumers > 0 && _actors.ContainsKey(key))
                        {
                            worker.Consumers++;
                            return worker;
                        }
                    }
                }

                worker = new Actor(this, key) {Consumers = 1};

                // 2 threads can reach this point, but only one can enter the "if"

                // ReSharper disable once InconsistentlySynchronizedField
                if (_actors.TryAdd(key, worker))
                {
                    worker.Start();
                    return worker;
                }
            }
        }

        internal int EstimatedCount => _actors.Count;

        internal void Return(Actor actor)
        {
            // there are no consumers - meaning no more work can be queued while inside this lock
            // check the number of queued work

            if (!_actors.TryRemove(actor.Key, out _))
            {
                throw new InvalidOperationException(); // should never happen
            }

            // at this point there are not more reference to the actor, so no one can enqueue.

        }
    }
}
