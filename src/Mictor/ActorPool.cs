using System;
using System.Collections.Concurrent;
using System.Linq;
using Serilog;

namespace Mictor
{
    /// <summary>
    /// Represents a pool of actors managed by keys
    /// </summary>
    public class ActorPool : IActorPool
    {
        private readonly ConcurrentDictionary<string, Actor> _actors = new ConcurrentDictionary<string, Actor>();

        /// <summary>
        /// A global shared pool of actors
        /// </summary>
        public static ActorPool Shared { get; } = new ActorPool();

        /// <inheritdoc />
        public IActor GetOrCreate(string key)
        {
            while (true)
            {
                Log.Verbose("Trying to get actor {Key}", key);
                if (_actors.TryGetValue(key, out var actor))
                {
                    if (actor.TryCreateReference(out ActorReference? actorReference))
                    {
                        Log.Verbose("Created a reference for actor {Key}", key);
                        return actorReference!;
                    }
                }

                Log.Verbose("Creating actor {Key}", key);
                actor = Actor.Create(Return, key, out var reference);

                // 2 threads can reach this point, but only one can enter the "if"

                if (_actors.TryAdd(key, actor))
                {
                    Log.Verbose("Added actor {Key}", key);
                    actor.Start();
                    return reference;
                }

                Log.Verbose("Could not add actor {Key} to pool. trying to get existing one", key);
            }
        }

        /// <summary>
        /// Takes a state snapshot of all active actors
        /// </summary>
        /// <returns>A snapshot of all active actors</returns>
        public ActorPoolSnapshot TakeSnapshot()
        {
            // must use .ToArray() because it is the consistent way to get a moment in time of the dictionary
            var dict = _actors
                .ToArray()
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.TakeSnapshot());

            return new ActorPoolSnapshot(dict);
        }

        private void Return(Actor actor)
        {
            if (!_actors.TryRemove(actor.Key, out _))
            {
                throw new InvalidOperationException(); // should never happen
            }
        }
    }
}
