using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediaBrowser.Theater.Mpdn
{
    public static class FilterInstallation
    {
        private static readonly Regex VersionRegex = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)(\.(?<build>\d+))?(\.(?<revision>\d+))?.*$");

        public static Version ParseVersion(string tagName)
        {
            Match match = VersionRegex.Match(tagName);
            if (match.Success) {
                return new Version(int.Parse(match.Groups["major"].Value),
                                   int.Parse(match.Groups["minor"].Value),
                                   match.Groups["build"].Success ? int.Parse(match.Groups["build"].Value) : 0);
            }

            return null;
        }

        public static string FindProgramFiles(string path)
        {
            string directory = GetProgramFiles(path);
            if (Directory.Exists(directory)) {
                return directory;
            }

            return null;
        }

        public static string GetProgramFiles(string path)
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string directory = Path.Combine(programFiles, path);
            return directory;
        }

        public static void SearchForBinaryPaths(string root, string file, out string x86, out string x64)
        {
            List<string> directories = Directory.GetFiles(root, file, SearchOption.AllDirectories)
                                                .Select(Path.GetDirectoryName)
                                                .ToList();

            x64 = directories.FirstOrDefault(path => path.Substring(root.Length).Contains("64"));
            x86 = directories.FirstOrDefault(path => path.Substring(root.Length).Contains("86")) ?? directories.First();
        }
    }
}