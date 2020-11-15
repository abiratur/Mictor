using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mictor
{
    internal class ActorHandle : IActor
    {
        private readonly Actor _actor;
        private int _disposed;

        public ActorHandle(Actor actor)
        {
            _actor = actor;
            actor.IncrementHandles();
        }

        public void Enqueue(Func<Task> work)
        {
            ThrowIfDisposed();

            _actor.Enqueue(work);
        }

        public Task<TResult> Enqueue<TResult>(Func<Task<TResult>> work)
        {
            ThrowIfDisposed();

            return _actor.Enqueue(work);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
            {
                return;
            }

            _actor.DecrementHandles();
        }
        
        private void ThrowIfDisposed()
        {
            if (_disposed == 1)
            {
                throw new ObjectDisposedException("Actor");
            }
        }
    }
}
