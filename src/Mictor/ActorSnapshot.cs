namespace Mictor
{
    public readonly struct ActorSnapshot
    {
        /// <summary>
        /// Gets the number of open handles to the actor
        /// </summary>
        public int Handles { get; }

        /// <summary>
        /// Gets the number of work items in the actor's queue
        /// </summary>
        public int QueuedWork { get; }

        public ActorSnapshot(int handles, int queuedWork)
        {
            Handles = handles;
            QueuedWork = queuedWork;
        }

        public override string ToString()
        {
            return $"Handles: {Handles}, QueuedWork: {QueuedWork}";
        }
    }
}