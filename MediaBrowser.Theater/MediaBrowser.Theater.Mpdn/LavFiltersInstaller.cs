using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using MediaBrowser.Common.Net;
using MediaBrowser.Theater.Api.System;
using Microsoft.Win32;
using Octokit;

namespace MediaBrowser.Theater.Mpdn
{
    public class LavFiltersInstaller
    {
//        private static readonly Regex VersionRegex = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)(\.(?<build>\d+))?.*$");
        private const string Clsid = "{171252A0-8820-4AFE-9DF8-5C92B2D66B04}";
        private const string CodecsRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\CLSID\{083863F1-70DE-11D0-BD40-00A0C911CE86}\Instance\";

        private async Task<Release> GetLatestRelease()
        {
            var github = new GitHubClient(new ProductHeaderValue("MediaBrowserTheater"));
            var releases = await github.Release.GetAll("Nevcairiel", "LAVFilters").ConfigureAwait(false);
            var versionedReleases = releases.Select(r => new { Version = IgnoreTime(r.CreatedAt), Release = r }).ToList();
            var latest = versionedReleases
                .Where(r => r.Version != null)
                .OrderByDescending(r => r.Version)
                .FirstOrDefault();

            if (latest == null) {
                return null;
            }

            var assets = await github.Release.GetAssets("Nevcairiel", "LAVFilters", latest.Release.Id).ConfigureAwait(false);
            var installer = assets.Where(a => a.ContentType == "application/x-msdownload").Select(a => a.BrowserDownloadUrl).FirstOrDefault();
            var x86Zip = assets.Where(a => a.Name.Contains("x86") && a.ContentType == "application/x-zip-compressed").Select(a => a.BrowserDownloadUrl).FirstOrDefault();
            var x64Zip = assets.Where(a => a.Name.Contains("x64") && a.ContentType == "application/x-zip-compressed").Select(a => a.BrowserDownloadUrl).FirstOrDefault();

            return new Release(latest.Version, installer, x86Zip, x64Zip);
        }

        private Install GetCurrentInstall()
        {
            if (!IsLavSplitterInstalled() || !IsLavAudioInstalled() || !IsLavVideoInstalled()) {
                return null;
            }

            var path = GetInstalledPath();
            if (string.IsNullOrEmpty(path)) {
                return null;
            }

            string x86, x64;
            SearchForBinaryPaths(Directory.GetParent(path).FullName, out x86, out x64);
            var date = FindSplitterFileDate(x86 ?? x64);

            return new Install(date, x86, x64);
        }

        private DateTimeOffset FindSplitterFileDate(string path)
        {
            var file = Directory.GetFiles(path, "LAVSplitter.ax", SearchOption.AllDirectories).First();
            var created = File.GetCreationTimeUtc(file);
            var modified = File.GetLastWriteTimeUtc(file);

            var oldest = created < modified ? created : modified;
            return  IgnoreTime(oldest);
        }

        private DateTimeOffset IgnoreTime(DateTimeOffset date)
        {
            date = date.ToUniversalTime();
            return new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, date.Offset);
        }

        private static string GetInstalledPath()
        {
            return FindProgramFiles(@"K-Lite Codec Pack\Filters\LAV") ??
                   FindProgramFiles(@"KCP\LAV Filters") ??
                   GetRegistryLocation();
        }

        private static string GetRegistryLocation()
        {
            var path = Registry.GetValue(string.Format(@"HKEY_CLASSES_ROOT\CLSID\{0}\InprocServer32", Clsid), null, null) as string;
            if (path != null) {
                return Path.GetDirectoryName(path);
            }

            return null;
        }

        private void SearchForBinaryPaths(string root, out string x86, out string x64)
        {
            var directories = Directory.GetFiles(root, "LAVSplitter.ax", SearchOption.AllDirectories)
                                       .Select(Path.GetDirectoryName)
                                       .ToList();

            x64 = directories.FirstOrDefault(path => path.Substring(root.Length).Contains("64"));
            x86 = directories.FirstOrDefault(path => path.Substring(root.Length).Contains("86")) ?? directories.First();
        }

        private static string FindProgramFiles(string path)
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var directory = Path.Combine(programFiles, path);
            if (Directory.Exists(directory)) {
                return directory;
            }

            return null;
        }

        public static bool IsLavSplitterInstalled()
        {
            // Returns true if 32-bit splitter is installed
            var id = Registry.GetValue(CodecsRegistryKey + "{B98D13E7-55DB-4385-A33D-09FD1BA26338}", "CLSID", "id");
            return id != null;
        }

        public static bool IsLavAudioInstalled()
        {
            // Returns true if 32-bit audio is installed
            var id = Registry.GetValue(CodecsRegistryKey + "{E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491}", "CLSID", "id");
            return id != null;
        }

        public static bool IsLavVideoInstalled()
        {
            // Returns true if 32-bit video is installed
            var id = Registry.GetValue(CodecsRegistryKey + "{EE30215D-164F-4A92-A4EB-9D4C13390F9F}", "CLSID", "id");
            return id != null;
        }

        public async Task InstallOrUpdate(IProgress<double> progress, IHttpClient httpClient)
        {
            if (!UacHelper.IsProcessElevated) {
                throw new InvalidOperationException("LAV Filters installation or updates requires elevated privileges.");
            }

            try {
                progress.Report(0);

                var latestRelease = await GetLatestRelease().ConfigureAwait(false);
                var currentInstall = GetCurrentInstall();

                if (latestRelease == null) {
                    return;
                }

                progress.Report(0.25);

                if (currentInstall == null) {
                    await latestRelease.NewInstall(progress.Slice(0.25, 1), httpClient).ConfigureAwait(false);
                } else if (currentInstall.Version < latestRelease.Version) {
                    await latestRelease.Update(progress.Slice(0.25, 1), httpClient, currentInstall.X86Location, currentInstall.X64Location).ConfigureAwait(false);
                }
            }
            finally {
                progress.Report(1);
            }
        }
        
        private class Release
        {
            private readonly DateTimeOffset _version;
            private readonly string _installerUrl;
            private readonly string _zipUrlx86;
            private readonly string _zipUrlx64;

            public Release(DateTimeOffset version, string installerUrl, string zipUrlx86, string zipUrlx64)
            {
                _version = version;
                _installerUrl = installerUrl;
                _zipUrlx86 = zipUrlx86;
                _zipUrlx64 = zipUrlx64;
            }

            public DateTimeOffset Version
            {
                get { return _version; }
            }

            public async Task NewInstall(IProgress<double> progress, IHttpClient httpClient)
            {
                // todo log error cases

                string tempFile = await httpClient.GetTempFile(new HttpRequestOptions {
                    Url = _installerUrl,
                    Progress = progress.Slice(0, 0.75),
                }).ConfigureAwait(false);

                string exePath = Path.ChangeExtension(tempFile, ".exe");
                File.Move(tempFile, exePath);

                try {
                    using (Process process = Process.Start(new ProcessStartInfo {
                        FileName = exePath,
                        Arguments = "/VERYSILENT"
                    })) {
                        process.WaitForExit();
                    }
                } finally {
                    try {
                        File.Delete(exePath);
                    } catch (Exception ex) {
                        //_logger.ErrorException("Error deleting {0}", ex, exePath);
                    }
                }

                progress.Report(1);
            }

            public async Task Update(IProgress<double> progress, IHttpClient httpClient, string x86Directory, string x64Directory)
            {
                var defaultInstallLocation = FindProgramFiles(@"LAV Filters");
                var dir = x86Directory ?? x64Directory;
                if (dir != null && defaultInstallLocation != null && dir.StartsWith(defaultInstallLocation + @"\")) {
                    // if LAV is installed in its default standalone location, then just re-run the installer
                    await NewInstall(progress, httpClient).ConfigureAwait(false);
                    return;
                }

                // unzip x86 and x64 versions of LAV Filters into their installed locations
                if (x86Directory != null && Directory.Exists(x86Directory)) {
                    await Extract(_zipUrlx86, x86Directory, progress.Slice(0, 0.5), httpClient).ConfigureAwait(false);
                }

                if (x64Directory != null && Directory.Exists(x64Directory)) {
                    await Extract(_zipUrlx64, x64Directory, progress.Slice(0.5, 1), httpClient).ConfigureAwait(false);
                }
            }

            private async Task Extract(string zipUrl, string directory, IProgress<double> progress, IHttpClient httpClient)
            {
                string tempFile = await httpClient.GetTempFile(new HttpRequestOptions {
                    Url = zipUrl,
                    Progress = progress.Slice(0, 0.5)
                }).ConfigureAwait(false);

                string zipPath = Path.ChangeExtension(tempFile, ".zip");
                File.Move(tempFile, zipPath);

                try {
                    var zip = new FastZip();
                    zip.ExtractZip(zipPath, directory, @"-\.bat$");
                }
                finally {
                    try {
                        File.Delete(zipPath);
                    }
                    catch (Exception e) {
                        // todo log
                    }
                }

                progress.Report(1);
            }
        }

        private class Install
        {
            private readonly string _x86Location;
            private readonly string _x64Location;
            private readonly DateTimeOffset _version;

            public Install(DateTimeOffset version, string x86Location, string x64Location)
            {
                _x86Location = x86Location;
                _version = version;
                _x64Location = x64Location;
            }

            public string X86Location
            {
                get { return _x86Location; }
            }

            public DateTimeOffset Version
            {
                get { return _version; }
            }

            public string X64Location
            {
                get { return _x64Location; }
            }
        }
    }
}
