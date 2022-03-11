using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Mictor
{
    /// <summary>
    /// Represents an actor with an id
    /// </summary>
    internal class Actor
    {
        private readonly Action<Actor> _onTerminated;
        private readonly SemaphoreSlim _workSemaphore = new SemaphoreSlim(0); // used to signal work/dispose events
        private readonly ConcurrentQueue<Func<Task>> _workQueue = new ConcurrentQueue<Func<Task>>();

        private bool _disposed;
        private int _references;

        private Actor(string key, Action<Actor> onTerminated)
        {
            Key = key;
            _onTerminated = onTerminated;
        }

        internal static Actor Create(Action<Actor> onTerminated, string key, out ActorReference actorReference)
        {
            var actor = new Actor(key, onTerminated);
            actorReference = new ActorReference(actor);

            return actor;
        }

        public string Key { get; }

        public void Enqueue(Func<Task> work)
        {
            Log.Verbose("Enqueue work for actor {Key}", Key);
            _workQueue.Enqueue(work);
            _workSemaphore.Release();
        }

        public Task<TResult> Enqueue<TResult>(Func<Task<TResult>> work)
        {
            var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            Enqueue(async () =>
            {
                try
                {
                    TResult result = await work();
                    tcs.TrySetResult(result);
                }
                catch(Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        internal ActorSnapshot TakeSnapshot()
        {
            return new ActorSnapshot(_references, _workQueue.Count);
        }

        internal void Start()
        {
            _ = RunAsync();
        }

        internal bool TryCreateReference(out ActorReference? actorReference)
        {
            lock (this)
            {
                if (_disposed)
                {
                    actorReference = default;
                    return false;
                }

                actorReference = new ActorReference(this);
                return true;
            }
        }

        private async Task RunAsync()
        {
            while (true)
            {
                Log.Verbose("Waiting for work in actor {Key}", Key);
                await _workSemaphore.WaitAsync();

                // we lock in order to prevent others from acquiring a reference to this actor, while checking if needs termination.
                lock (this)
                {
                    if (_workQueue.Count == 0 && _references == 0)
                    {
                        // at this point:
                        // 1. there are no consumers - meaning no more work can be queued while inside this lock
                        // 2. no more consumers can be added, since we have taking the
                        // 3. no more work can be added (because of lock)

                        Log.Verbose("Terminating actor {Key}", Key);
                        _onTerminated(this);
                        _disposed = true;
                        return;
                    }
                }

                if (!_workQueue.TryDequeue(out var work))
                {
                    Log.Verbose("Could not dequeue work for actor {Key}", Key);
                    continue;
                }

                try
                {
                    Log.Verbose("Executing work for actor {Key}", Key);
                    await work();
                }
                catch(Exception ex)
                {
                    Log.Warning(ex, "Caught exception for actor {Key}", Key);
                }
            }
        }

        internal void IncrementReferences()
        {
            _references++;
            Log.Verbose("Incremented references to {Refs} for actor {Key}", _references, Key);
        }

        internal void DecrementReferences()
        {
            lock (this)
            {
                _references--;
                _workSemaphore.Release();
                Log.Verbose("Decremented references to {Refs} for actor {Key}", _references, Key);
            }
        }
    }
}
