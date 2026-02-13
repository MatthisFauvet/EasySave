using System.Threading;

namespace EasySave.Repository
{
    /// <summary>
    /// Thread-safe implementation of <see cref="IBackupIdFactory"/>.
    ///
    /// Generates unique incremental identifiers using an atomic counter.
    /// The counter is initialized with the current maximum ID found in storage,
    /// ensuring continuity across application restarts.
    /// </summary>
    public sealed class IncrementalBackupIdFactory : IBackupIdFactory
    {
        /// <summary>
        /// Internal counter storing the last issued ID.
        /// </summary>
        private int _current;

        /// <summary>
        /// Initializes the factory with a starting ID.
        /// Typically set to the maximum existing ID in the repository.
        /// </summary>
        /// <param name="startingId">The last existing ID.</param>
        public IncrementalBackupIdFactory(int startingId)
        {
            _current = startingId;
        }

        /// <summary>
        /// Returns the next unique ID.
        ///
        /// Uses <see cref="Interlocked.Increment(ref int)"/> to ensure
        /// atomicity and prevent race conditions in multi-threaded scenarios.
        /// </summary>
        public int NextId()
        {
            return Interlocked.Increment(ref _current);
        }
    }
}
