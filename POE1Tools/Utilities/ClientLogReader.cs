using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace POE1Tools.Utilities
{
    /// <summary>
    /// Finds the Path of Exile 1 install through the running game process, then tails logs\Client.txt
    /// to track whether the player is currently in a hideout.
    /// </summary>
    public class ClientLogReader
    {
        // PoE 1 executables (standalone, Steam, Kakao, 32/64 bit). PoE 2 is "PathOfExile2*" and is skipped.
        private static readonly string[] ExeNames =
        {
            "PathOfExile", "PathOfExile_x64", "PathOfExileSteam", "PathOfExile_x64Steam",
            "PathOfExile_KG", "PathOfExile_x64_KG", "PathOfExile_x64EGS"
        };

        // [DEBUG Client 1234] Generating level 68 area "1_1_town" with seed 1
        private static readonly Regex GeneratingRegex = new Regex(@"Generating level \d+ area ""([^""]+)""", RegexOptions.Compiled);
        // [INFO Client 1234] : You have entered Celestial Hideout.
        private static readonly Regex EnteredRegex = new Regex(@"You have entered (.+?)\.\s*$", RegexOptions.Compiled);

        private string _path;
        private long _position;
        private DateTime _lastPathSearch = DateTime.MinValue;

        /// <summary>True when the last area change seen in the log was a hideout.</summary>
        public bool InHideout { get; private set; }

        /// <summary>Last area id / name seen in the log (for display).</summary>
        public string CurrentArea { get; private set; } = "";

        /// <summary>Full path of the Client.txt being read, or empty if not found.</summary>
        public string LogPath => _path ?? "";

        /// <summary>Call periodically; cheap when nothing changed.</summary>
        public void Poll()
        {
            if (_path == null || !File.Exists(_path))
            {
                // Don't scan processes every tick
                if ((DateTime.Now - _lastPathSearch).TotalSeconds < 5) return;
                _lastPathSearch = DateTime.Now;

                string found = FindClientTxt();
                if (found == null) return;
                OpenAtEnd(found);
                return;
            }

            try
            {
                ReadNewLines(_position, false);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        // Start at the end of the file for speed, but scan the tail so the current area is known immediately.
        private void OpenAtEnd(string path)
        {
            _path = path;
            InHideout = false;
            CurrentArea = "";
            try
            {
                long length = new FileInfo(path).Length;
                const int tailBytes = 256 * 1024;
                long start = Math.Max(0, length - tailBytes);
                ReadNewLines(start, start > 0);
            }
            catch (IOException) { _path = null; }
            catch (UnauthorizedAccessException) { _path = null; }
        }

        // Parse every complete line from 'from' to the end of the file. A trailing line the game is still
        // writing (no '\n' yet) is left for the next poll, so _position always sits on a line boundary.
        private void ReadNewLines(long from, bool skipFirstPartialLine)
        {
            using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                long length = fs.Length;
                if (length < from) from = 0; // log was rotated/truncated
                if (length == from) { _position = from; return; }

                byte[] buffer = new byte[length - from];
                fs.Seek(from, SeekOrigin.Begin);
                int read = 0;
                while (read < buffer.Length)
                {
                    int n = fs.Read(buffer, read, buffer.Length - read);
                    if (n <= 0) break;
                    read += n;
                }

                int lineStart = 0;
                if (skipFirstPartialLine)
                {
                    int firstNl = Array.IndexOf(buffer, (byte)'\n', 0, read);
                    if (firstNl < 0) { _position = from; return; }
                    lineStart = firstNl + 1;
                }

                for (int i = lineStart; i < read; i++)
                {
                    if (buffer[i] != (byte)'\n') continue;
                    ParseLine(Encoding.UTF8.GetString(buffer, lineStart, i - lineStart).TrimEnd('\r'));
                    lineStart = i + 1;
                }
                _position = from + lineStart;
            }
        }

        private void ParseLine(string line)
        {
            Match m = GeneratingRegex.Match(line);
            if (m.Success)
            {
                CurrentArea = m.Groups[1].Value;
                InHideout = CurrentArea.IndexOf("Hideout", StringComparison.OrdinalIgnoreCase) >= 0;
                return;
            }

            // Fallback for logs without "Generating level" lines (English client only)
            m = EnteredRegex.Match(line);
            if (m.Success)
            {
                CurrentArea = m.Groups[1].Value;
                InHideout = CurrentArea.IndexOf("Hideout", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        /// <summary>Locate Client.txt from the running PoE 1 process, falling back to common install folders.</summary>
        public static string FindClientTxt()
        {
            foreach (string dir in GetRunningGameDirs())
            {
                string p = Path.Combine(dir, "logs", "Client.txt");
                if (File.Exists(p)) return p;
            }

            foreach (string dir in GetCommonInstallDirs())
            {
                string p = Path.Combine(dir, "logs", "Client.txt");
                if (File.Exists(p)) return p;
            }
            return null;
        }

        private static IEnumerable<string> GetRunningGameDirs()
        {
            foreach (string name in ExeNames)
            {
                Process[] procs;
                try { procs = Process.GetProcessesByName(name); }
                catch { continue; }

                foreach (Process proc in procs)
                {
                    string dir = null;
                    try { dir = Path.GetDirectoryName(proc.MainModule?.FileName); }
                    catch { } // access denied / bitness mismatch
                    finally { proc.Dispose(); }
                    if (!string.IsNullOrEmpty(dir)) yield return dir;
                }
            }
        }

        private static IEnumerable<string> GetCommonInstallDirs()
        {
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            var dirs = new List<string>
            {
                Path.Combine(pf86, "Grinding Gear Games", "Path of Exile"),
                Path.Combine(pf, "Grinding Gear Games", "Path of Exile"),
                Path.Combine(pf86, "Steam", "steamapps", "common", "Path of Exile"),
                Path.Combine(pf, "Steam", "steamapps", "common", "Path of Exile"),
            };
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                if (d.DriveType != DriveType.Fixed) continue;
                dirs.Add(Path.Combine(d.Name, "Steam", "steamapps", "common", "Path of Exile"));
                dirs.Add(Path.Combine(d.Name, "SteamLibrary", "steamapps", "common", "Path of Exile"));
                dirs.Add(Path.Combine(d.Name, "Games", "Path of Exile"));
                dirs.Add(Path.Combine(d.Name, "Grinding Gear Games", "Path of Exile"));
            }
            return dirs;
        }
    }
}
