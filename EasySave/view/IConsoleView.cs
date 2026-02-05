using EasySave.Entity;

namespace EasySave.view
{
    public interface IConsoleView
    {
        /// <summary>
        /// Ask the user to select a language for the UI.
        /// </summary>
        /// <returns>The selected UI language.</returns>
        UiLanguage AskLanguage();

        /// <summary>
        /// Display the main menu and ask the user to choose an option.
        /// </summary>
        /// <returns>
        ///     An integer representing the user's choice:
        ///     0 - Create backup
        ///     1 - List backup
        ///     2 - Delete backup
        ///     3 - Run backup
        ///     4 - Rename backup
        /// </returns>
        int ShowMenu();

        /// <summary>
        /// Ask the user to provide the ID of a backup for a specific purpose.
        /// </summary>
        /// <param name="purposeKey">A string indicating the purpose for which the backup ID is requested (e.g., "delete", "execute").</param>
        /// <returns>The ID of the backup as an integer.</returns>
        int AskBackupId(string purposeKey);

        /// <summary>
        /// Prompt the user to provide information to create a new backup.
        /// </summary>
        /// <returns>
        /// A <see cref="CreateBackupRequest"/> object containing the backup information.
        /// </returns>
        // TODO: Consider renaming this method to CreateBackup()
        CreateBackupRequest AskCreateBackupInfo();

        /// <summary>
        /// Allow the user to select one or more backups to execute.
        /// </summary>
        /// <returns>A list of integers representing the IDs of the selected backups.</returns>
        List<int> SelectBackupToExecute();

        /// <summary>
        /// Ask the user to provide a new name for an existing backup.
        /// </summary>
        /// <returns>The new backup name as a string.</returns>
        string AskNewBackupName();

        /// <summary>
        /// Display a message to the user.
        /// </summary>
        /// <param name="messageKey">A key representing the message to show (usually mapped to a localized string).</param>
        void ShowMessage(string messageKey);

        /// <summary>
        /// Display an error message to the user.
        /// </summary>
        /// <param name="messageKey">A key representing the error message to show.</param>
        void ShowError(string messageKey);

        /// <summary>
        /// Display a list of existing backups to the user.
        /// </summary>
        /// <param name="backups">A list of <see cref="Backup"/> objects to display.</param>
        void ShowBackupList(List<Backup> backups);

        /// <summary>
        /// Pause the console and wait for user input to continue.
        /// </summary>
        void Pause();
    }
}
