using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace POE1Tools.Utilities
{
    /// <summary>
    /// .NET keeps user settings in a folder named after the exe's path, so the Debug build, the Release
    /// build and any copy of the exe each get their own settings. This keeps one shared copy that every
    /// build reads at startup and writes after each save.
    /// </summary>
    public static class SharedSettings
    {
        private static readonly string AppDataDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "POE1Tools");

        private static readonly string SharedPath = Path.Combine(AppDataDir, "shared-user.config");

        private static string OwnPath =>
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;

        /// <summary>Call before reading settings: take the shared copy if it is newer than this exe's own.</summary>
        public static void Import()
        {
            try
            {
                string own = OwnPath;
                string source = File.Exists(SharedPath) ? SharedPath : NewestOtherConfig(own);
                if (source == null) return;
                if (File.Exists(own) && File.GetLastWriteTimeUtc(own) >= File.GetLastWriteTimeUtc(source)) return;

                Directory.CreateDirectory(Path.GetDirectoryName(own));
                File.Copy(source, own, true);
                Properties.Settings.Default.Reload();
            }
            catch (Exception) { } // settings are a convenience, never block startup
        }

        /// <summary>Call after Settings.Default.Save(): publish this exe's settings to the other builds.</summary>
        public static void Export()
        {
            try
            {
                string own = OwnPath;
                if (!File.Exists(own)) return;
                Directory.CreateDirectory(AppDataDir);
                File.Copy(own, SharedPath, true);
            }
            catch (Exception) { }
        }

        // First run with sharing: start from whichever build saved most recently
        private static string NewestOtherConfig(string own)
        {
            if (!Directory.Exists(AppDataDir)) return null;
            return Directory.GetFiles(AppDataDir, "user.config", SearchOption.AllDirectories)
                .Where(p => !string.Equals(p, own, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }
    }
}
