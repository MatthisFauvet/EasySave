using System.Windows;

namespace EasySave
{
    public partial class App : Application
    { protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                var service = new BackupService();

                try
                {
                    service.ExecuteFromFlag(e.Args[0]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "CLI Error");
                }

                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
