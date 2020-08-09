using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using Wallcat.Util;

namespace Wallcat.service
{
    public class SystemService
    {
        public static void UpdateSystemWallpaper(string filePath)
        {
#if DEBUG
            MessageBox.Show(@"set wall parpaer: " + filePath, @"Update System Wallpaper");
            return;
#endif
            if (Environment.OSVersion.Version.Major >= 8)
            {
                SetWallpaper.Apply(null, filePath, DesktopWallpaperPosition.Fill);
            }
            else
            {
                SetWallpaperLegacy.Apply(filePath, DesktopWallpaperPosition.Fill);
            }
        }

        public static void CreateStartupShortcut()
        {
            var pathToExe = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var pathToShortcut =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Wallcat.lnk");

            if (IsEnabledAtStartup())
            {
                System.IO.File.Delete(pathToShortcut);
            }
            else
            {
                var shortcut = (IWshShortcut) new WshShell().CreateShortcut(pathToShortcut);

                shortcut.Description = "Enjoy a new, beautiful wallpaper, every day.";
                shortcut.TargetPath = pathToExe;
                shortcut.Save();
            }
        }

        public static bool IsEnabledAtStartup()
        {
            return System.IO.File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                "Wallcat.lnk"));
        }

        public void MidnightUpdate(TimerCallback cb)
        {
            var updateTime = new TimeSpan(24, 1, 0) - DateTime.Now.TimeOfDay;
            var timer = new System.Threading.Timer(x =>
            {
                cb(x);
                MidnightUpdate(cb);
            }, null, updateTime, Timeout.InfiniteTimeSpan);
        }
    }
}