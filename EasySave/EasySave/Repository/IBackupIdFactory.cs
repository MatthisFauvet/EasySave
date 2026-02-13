namespace EasySave.Repository
{
    /// <summary>
    /// Defines a contract for generating unique Backup identifiers.
    ///
    /// The implementation is responsible for ensuring that
    /// each generated ID is unique within the repository context.
    /// </summary>
    public interface IBackupIdFactory
    {
        /// <summary>
        /// Returns the next available unique identifier.
        /// </summary>
        int NextId();
    }
}
