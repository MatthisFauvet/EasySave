using System.Threading;

namespace EasySave.Repository
{
    /// <summary>
    /// Generates unique incremental IDs for backups.
    ///
    /// Strategy:
    /// - The factory starts from the current maximum ID found in storage.
    /// - Each call to NextId() returns a strictly increasing value.
    ///
    /// Thread-safety:
    /// - Uses Interlocked.Increment to ensure atomic ID generation
    ///   in multi-threaded scenarios within a single process.
    /// </summary>
    public sealed class IncrementalBackupIdFactory : IBackupIdFactory
    {
        /// <summary>
        /// Stores the last issued ID.
        /// </summary>
        private int _current;

        /// <summary>
        /// Initializes the factory with the last existing ID.
        /// </summary>
        /// <param name="startingId">
        /// The highest ID currently present in storage (or 0 if none).
        /// </param>
        public IncrementalBackupIdFactory(int startingId)
        {
            _current = startingId;
        }

        /// <summary>
        /// Returns the next unique ID.
        /// </summary>
        public int NextId()
        {
            return Interlocked.Increment(ref _current);
        }
    }
}
