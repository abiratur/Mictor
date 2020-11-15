using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mictor
{
    /// <summary>
    /// When does the worker stops:
    /// The Actor.Dispose() is called, The worker has 0 consumers AND has no more messages to consume AND worker is blocked on semaphore.Wait()
    /// in this case we need to make sure that no more handles are being created
    /// </summary>
    internal class Actor
    {
        private readonly ActorPool _owner;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<Func<Task>> _workQueue = new ConcurrentQueue<Func<Task>>();

        private bool _disposed;
        private int _handles;

        private Actor(ActorPool owner, string key)
        {
            _owner = owner;
            Key = key;
        }

        internal static Actor Create(ActorPool owner, string key, out ActorHandle actorHandle)
        {
            var actor = new Actor(owner, key);
            actorHandle = new ActorHandle(actor);
            
            return actor;
        }

        public string Key { get; }

        public void Enqueue(Func<Task> work)
        {
            _workQueue.Enqueue(work);
            _semaphoreSlim.Release();
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
            return new ActorSnapshot(_handles, _workQueue.Count);
        }

        internal void Start()
        {
            _ = RunAsync();
        }

        internal bool TryCreateHandle(out ActorHandle? actorHandle)
        {
            lock (this)
            {
                if (_disposed)
                {
                    actorHandle = default;
                    return false;
                }

                actorHandle = new ActorHandle(this);
                return true;
            }
        }

        private async Task RunAsync()
        {
            while (true)
            {
                await _semaphoreSlim.WaitAsync();

                lock (this)
                {
                    if (_workQueue.Count == 0 && _handles == 0)
                    {
                        // at this point:
                        // 1. there are no consumers - meaning no more work can be queued while inside this lock
                        // 2. no more consumers can be added, since we have taking the
                        // 3. no more work can be added (because of lock)

                        _owner.Return(this);
                        _disposed = true;
                        return;
                    }
                }

                if (!_workQueue.TryDequeue(out var work))
                {
                    continue;
                }

                try
                {
                    await work();
                }
                catch
                {
                    // ignored
                }
            }
        }

        internal void IncrementHandles()
        {
            _handles++;
        }

        internal void DecrementHandles()
        {
            lock (this)
            {
                _handles--;
                _semaphoreSlim.Release();
            }
        }
    }
}
