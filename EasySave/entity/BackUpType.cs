using System;

namespace EasySave.Entity
{
    /// <summary>
    /// Type de sauvegarde pour un travail de backup.
    /// Permet de définir comment les fichiers seront copiés.
    /// </summary>
    public enum BackupType
    {
        /// <summary>
        /// Type inconnu ou non défini.
        /// Peut servir pour initialisation ou validation.
        /// </summary>
        Unknown,

        /// <summary>
        /// Sauvegarde complète : tous les fichiers sont copiés à chaque exécution.
        /// </summary>
        Full,

        /// <summary>
        /// Sauvegarde séquentielle (différentielle) :
        /// seuls les fichiers nouveaux ou modifiés depuis la dernière sauvegarde sont copiés.
        /// </summary>
        Sequential
    }

    /// <summary>
    /// Classe utilitaire pour convertir une string en BackupType
    /// </summary>
    public static class BackupTypeHelper
    {
        public static BackupType FromString(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return BackupType.Unknown;

            return type.ToLower() switch
            {
                "full" => BackupType.Full,
                "1" => BackupType.Full,
                "sequential" => BackupType.Sequential,
                "2" => BackupType.Sequential,
                _ => BackupType.Unknown
            };
        }
    }
}
