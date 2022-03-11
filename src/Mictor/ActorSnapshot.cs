namespace Mictor
{
    /// <summary>
    /// Represents a state snapshot of an actor
    /// </summary>
    public readonly struct ActorSnapshot
    {
        /// <summary>
        /// Gets the number of open references to the actor
        /// </summary>
        public int References { get; }

        /// <summary>
        /// Gets the number of work items in the actor's queue
        /// </summary>
        public int QueuedWork { get; }

        /// <summary>
        /// Creates a new <see cref="ActorSnapshot"/>
        /// </summary>
        /// <param name="references">Number of active references for this actor</param>
        /// <param name="queuedWork">Number of work items in the actors queue</param>
        public ActorSnapshot(int references, int queuedWork)
        {
            References = references;
            QueuedWork = queuedWork;
        }

        /// <summary>
        /// Returns a string representation of this <see cref="ActorSnapshot"/>
        /// </summary>
        public override string ToString()
        {
            return $"References: {References}, QueuedWork: {QueuedWork}";
        }
    }
}