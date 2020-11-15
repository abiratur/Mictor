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
                // ReSharper disable once InconsistentlySynchronizedField
                if (_actors.TryGetValue(key, out var worker))
                {
                    lock (worker)
                    {
                        // check if the actor has been disposed since we took the value
                        // if it was, then create it again
                        if (_actors.ContainsKey(key))
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

        public ActorPoolSnapshot TakeSnapshot()
        {
            Dictionary<string, ActorSnapshot> dict = _actors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.TakeSnapshot()
            );

            return new ActorPoolSnapshot(dict);
        }

        internal int EstimatedCount => _actors.Count;

        internal void Return(Actor actor)
        {
            if (!_actors.TryRemove(actor.Key, out _))
            {
                throw new InvalidOperationException(); // should never happen
            }
        }
    }
}
