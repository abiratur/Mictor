using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mictor
{
    /// <summary>
    /// Represents a reference to an actor
    /// </summary>
    internal class ActorReference : IActor
    {
        private readonly Actor _actor;
        private int _disposed;

        public ActorReference(Actor actor)
        {
            _actor = actor;
            actor.IncrementReferences();
        }

        /// <inheritdoc />    
        public void Enqueue(Func<Task> work)
        {
            ThrowIfDisposed();

            _actor.Enqueue(work);
        }

        /// <inheritdoc />
        public Task<TResult> Enqueue<TResult>(Func<Task<TResult>> work)
        {
            ThrowIfDisposed();

            return _actor.Enqueue(work);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
            {
                return;
            }

            _actor.DecrementReferences();
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
