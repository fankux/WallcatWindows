using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Wallcat.fe;

namespace Wallcat
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if DEBUG
            Console.WriteLine($@"Current Channel: {Properties.Settings.Default.CurrentChannel}");
            Console.WriteLine($@"Current Wallpaper: {Properties.Settings.Default.CurrentWallpaper}");
            Console.WriteLine($@"Last Checked: {Properties.Settings.Default.LastChecked}");
            Console.WriteLine(
                $@"Storage: {System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath}");
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, error) => true;
            Application.Run(new Entry());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
#if DEBUG
            MessageBox.Show(e.Exception.ToString(), @"Thread Exception!");
#endif
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG
            MessageBox.Show(e.ToString(), @"Unhandled Exception!");
#endif
        }
    }
}