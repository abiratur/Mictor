using System;
using System.Threading.Tasks;

namespace Mictor
{
    /// <summary>
    /// Represents an actor with a key
    /// </summary>
    public interface IActor : IDisposable
    {
        /// <summary>
        /// Enqueue a work to be done by the actor
        /// </summary>
        /// <param name="work"></param>
        void Enqueue(Func<Task> work);

        /// <summary>
        /// Enqueue the work immediately (synchronously) and returns a <see cref="Task{TResult}"/> that will complete once
        /// the actor has completed its processing.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="work"></param>
        Task<TResult> Enqueue<TResult>(Func<Task<TResult>> work);
    }
}
