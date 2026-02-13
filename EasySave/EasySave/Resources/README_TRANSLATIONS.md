# Guide des Traductions - EasySave v.2

## Structure des fichiers de ressources

Les fichiers de ressources sont situés dans le dossier `Resources/`:
- `fr.json` - Traductions françaises
- `en.json` - Traductions anglaises

## Hiérarchie des clés de traduction

### App
- `App.Title` - Titre de l'application
- `App.Version` - Numéro de version

### Navigation
- `Navigation.Title` - Titre de la section navigation
- `Navigation.Home` - Label menu Accueil
- `Navigation.Saves` - Label menu Sauvegardes
- `Navigation.History` - Label menu Historique
- `Navigation.Settings` - Label menu Paramètres

### Storage (Stockage)
- `Storage.Title` - Titre section stockage
- `Storage.Calculating` - Message pendant le calcul
- `Storage.NotAvailable` - Message si non disponible
- `Storage.ReadError` - Message d'erreur de lecture

### Home (Accueil)
- `Home.Title` - Titre de la page d'accueil
- `Home.Welcome` - Message de bienvenue
- `Home.ActiveJobs` - Label travaux actifs
- `Home.Completed` - Label complétés
- `Home.Pending` - Label en attente
- `Home.Errors` - Label erreurs
- `Home.RecentActivity` - Titre activité récente
- `Home.QuickActions` - Titre actions rapides
- `Home.StartBackup` - Label démarrer sauvegarde
- `Home.ViewLogs` - Label voir les logs
- `Home.Settings` - Label paramètres

### Saves (Sauvegardes)
- `Saves.Title` - Titre de la page
- `Saves.Subtitle` - Sous-titre descriptif
- `Saves.CreateWork` - Bouton créer un travail
- `Saves.ExecuteWorks` - Bouton exécuter les travaux
- `Saves.WorksList` - Titre liste des travaux
- `Saves.Work` - Label travail
- `Saves.Pending` - Statut en attente
- `Saves.InProgress` - Statut en cours
- `Saves.Completed` - Statut terminé

### History (Historique)
- `History.Title` - Titre de la page
- `History.Description` - Description de la page
- `History.Success` - Statut succès
- `History.Fail` - Statut échec
- `History.Abort` - Statut annulé

### Settings (Paramètres)
- `Settings.Title` - Titre de la page
- `Settings.Subtitle` - Sous-titre descriptif
- `Settings.General` - Section général
- `Settings.TemplateApp` - Label template
- `Settings.TemplateDescription` - Description template
- `Settings.Template1` - Option template 1
- `Settings.Template2` - Option template 2
- `Settings.ThemeApp` - Label thème
- `Settings.ThemeDescription` - Description thème
- `Settings.Light` - Option thème clair
- `Settings.Dark` - Option thème sombre
- `Settings.Language` - Label langue
- `Settings.LanguageDescription` - Description langue
- `Settings.French` - Option français
- `Settings.English` - Option anglais
- `Settings.Saves` - Section sauvegardes
- `Settings.ExecutionMode` - Label mode exécution
- `Settings.ExecutionDescription` - Description mode exécution
- `Settings.Manual` - Option manuel
- `Settings.Auto` - Option automatique
- `Settings.Logs` - Section logs
- `Settings.FileType` - Label type de fichier
- `Settings.FileTypeDescription` - Description type fichier
- `Settings.JSON` - Option JSON
- `Settings.XML` - Option XML

### CreateWork (Dialogue création travail)
- `CreateWork.Title` - Titre du dialogue
- `CreateWork.WorkName` - Label nom du travail
- `CreateWork.SourcePath` - Label chemin source
- `CreateWork.DestinationPath` - Label chemin destination
- `CreateWork.SaveType` - Label type de sauvegarde
- `CreateWork.Complete` - Option complète
- `CreateWork.Incremental` - Option incrémentale
- `CreateWork.Browse` - Bouton parcourir
- `CreateWork.Create` - Bouton créer
- `CreateWork.Cancel` - Bouton annuler
- `CreateWork.ErrorRequired` - Message erreur champ requis
- `CreateWork.ErrorNotExists` - Message erreur n'existe pas
- `CreateWork.ErrorSameFolder` - Message erreur même dossier
- `CreateWork.ErrorWorkName` - Préfixe erreur nom travail
- `CreateWork.ErrorSourcePath` - Préfixe erreur chemin source
- `CreateWork.ErrorSourceNotExists` - Préfixe erreur dossier source
- `CreateWork.ErrorDestPath` - Préfixe erreur chemin destination
- `CreateWork.ErrorDestNotExists` - Préfixe erreur dossier destination

## Comment utiliser

Dans le code C#:
```csharp
string translated = LanguageManager.Get("Navigation.Home");
```

Pour les propriétés composées:
```csharp
TextBlock.Text = $"{LanguageManager.Get("CreateWork.ErrorWorkName")} {LanguageManager.Get("CreateWork.ErrorRequired")}";
```

## Ajout de nouvelles traductions

1. Ajouter la clé dans `fr.json`
2. Ajouter la même clé dans `en.json`
3. Utiliser `LanguageManager.Get("Votre.Cle")` dans le code

## Notes importantes

- Tous les textes affichés à l'utilisateur doivent utiliser le système de traduction
- Ne jamais mettre de texte en dur dans le code ou XAML
- Les fichiers JSON doivent avoir la même structure dans toutes les langues
- Le changement de langue rafraîchit automatiquement l'interface via `UpdateUILanguage()`
