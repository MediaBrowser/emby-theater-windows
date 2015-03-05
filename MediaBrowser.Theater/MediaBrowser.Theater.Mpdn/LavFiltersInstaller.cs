using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using MediaBrowser.Common.Net;
using MediaBrowser.Theater.Api.System;
using Microsoft.Win32;
using Octokit;

namespace MediaBrowser.Theater.Mpdn
{
    public interface IUpdate
    {
        Version Version { get; }
        UpdateType Type { get; }
        Task Install(IProgress<double> progress, IHttpClient httpClient);
    }

    public enum UpdateType
    {
        NewInstall,
        NewRelease,
        UpToDate,
        Unavailable
    }

    public class LavFiltersInstaller
    {
        private const string Clsid = "{171252A0-8820-4AFE-9DF8-5C92B2D66B04}";
        private const string CodecsRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\CLSID\{083863F1-70DE-11D0-BD40-00A0C911CE86}\Instance\";
        private static readonly Regex VersionRegex = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)(\.(?<build>\d+))?.*$");

        public bool IsInstalled
        {
            get { return IsLavSplitterInstalled() && IsLavAudioInstalled() && IsLavVideoInstalled(); }
        }

        public bool CanInstall
        {
            get { return UacHelper.IsProcessElevated; }
        }

        private async Task<Release> GetLatestRelease()
        {
            var github = new GitHubClient(new ProductHeaderValue("MediaBrowserTheater"));
            IReadOnlyList<Octokit.Release> releases = await github.Release.GetAll("Nevcairiel", "LAVFilters").ConfigureAwait(false);
            var versionedReleases = releases.Select(r => new { Version = ParseVersion(r.TagName), ReleaseDate = IgnoreTime(r.CreatedAt), Release = r }).ToList();
            var latest = versionedReleases
                .Where(r => r.ReleaseDate != null)
                .OrderByDescending(r => r.ReleaseDate)
                .FirstOrDefault();

            if (latest == null) {
                return null;
            }

            IReadOnlyList<ReleaseAsset> assets = await github.Release.GetAssets("Nevcairiel", "LAVFilters", latest.Release.Id).ConfigureAwait(false);
            string installer = assets.Where(a => a.ContentType == "application/x-msdownload").Select(a => a.BrowserDownloadUrl).FirstOrDefault();
            string x86Zip = assets.Where(a => a.Name.Contains("x86") && a.ContentType == "application/x-zip-compressed").Select(a => a.BrowserDownloadUrl).FirstOrDefault();
            string x64Zip = assets.Where(a => a.Name.Contains("x64") && a.ContentType == "application/x-zip-compressed").Select(a => a.BrowserDownloadUrl).FirstOrDefault();

            return new Release(latest.Version, latest.ReleaseDate, installer, x86Zip, x64Zip);
        }

        private Version ParseVersion(string tagName)
        {
            Match match = VersionRegex.Match(tagName);
            if (match.Success) {
                return new Version(int.Parse(match.Groups["major"].Value),
                                   int.Parse(match.Groups["minor"].Value),
                                   match.Groups["build"].Success ? int.Parse(match.Groups["build"].Value) : 0);
            }

            return null;
        }

        private ExistingInstall GetCurrentInstall()
        {
            if (!IsLavSplitterInstalled() || !IsLavAudioInstalled() || !IsLavVideoInstalled()) {
                return null;
            }

            string path = GetInstalledPath();
            if (string.IsNullOrEmpty(path)) {
                return null;
            }

            string x86, x64;
            SearchForBinaryPaths(Directory.GetParent(path).FullName, out x86, out x64);
            DateTimeOffset date = FindSplitterFileDate(x86 ?? x64);

            return new ExistingInstall(date, x86, x64);
        }

        private DateTimeOffset FindSplitterFileDate(string path)
        {
            string file = Directory.GetFiles(path, "LAVSplitter.ax", SearchOption.AllDirectories).First();
            DateTime created = File.GetCreationTimeUtc(file);
            DateTime modified = File.GetLastWriteTimeUtc(file);

            DateTime oldest = created < modified ? created : modified;
            return IgnoreTime(oldest);
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
            List<string> directories = Directory.GetFiles(root, "LAVSplitter.ax", SearchOption.AllDirectories)
                                                .Select(Path.GetDirectoryName)
                                                .ToList();

            x64 = directories.FirstOrDefault(path => path.Substring(root.Length).Contains("64"));
            x86 = directories.FirstOrDefault(path => path.Substring(root.Length).Contains("86")) ?? directories.First();
        }

        private static string FindProgramFiles(string path)
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string directory = Path.Combine(programFiles, path);
            if (Directory.Exists(directory)) {
                return directory;
            }

            return null;
        }

        private static bool IsLavSplitterInstalled()
        {
            // Returns true if 32-bit splitter is installed
            object id = Registry.GetValue(CodecsRegistryKey + "{B98D13E7-55DB-4385-A33D-09FD1BA26338}", "CLSID", "id");
            return id != null;
        }

        private static bool IsLavAudioInstalled()
        {
            // Returns true if 32-bit audio is installed
            object id = Registry.GetValue(CodecsRegistryKey + "{E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491}", "CLSID", "id");
            return id != null;
        }

        private static bool IsLavVideoInstalled()
        {
            // Returns true if 32-bit video is installed
            object id = Registry.GetValue(CodecsRegistryKey + "{EE30215D-164F-4A92-A4EB-9D4C13390F9F}", "CLSID", "id");
            return id != null;
        }

        public async Task<IUpdate> FindUpdate()
        {
            if (!UacHelper.IsProcessElevated) {
                throw new InvalidOperationException("LAV Filters installation or updates requires elevated privileges.");
            }

            Release latestRelease = await GetLatestRelease().ConfigureAwait(false);
            ExistingInstall currentInstall = GetCurrentInstall();

            if (latestRelease == null) {
                return Update.Unavailable;
            }

            return new Update(currentInstall, latestRelease);
        }

        private class ExistingInstall
        {
            private readonly DateTimeOffset _releaseDate;
            private readonly string _x64Location;
            private readonly string _x86Location;

            public ExistingInstall(DateTimeOffset releaseDate, string x86Location, string x64Location)
            {
                _x86Location = x86Location;
                _releaseDate = releaseDate;
                _x64Location = x64Location;
            }

            public string X86Location
            {
                get { return _x86Location; }
            }

            public DateTimeOffset ReleaseDate
            {
                get { return _releaseDate; }
            }

            public string X64Location
            {
                get { return _x64Location; }
            }
        }

        private class Release
        {
            private readonly string _installerUrl;
            private readonly DateTimeOffset _releaseDate;
            private readonly Version _version;
            private readonly string _zipUrlx64;
            private readonly string _zipUrlx86;

            public Release(Version version, DateTimeOffset releaseDate, string installerUrl, string zipUrlx86, string zipUrlx64)
            {
                _version = version;
                _releaseDate = releaseDate;
                _installerUrl = installerUrl;
                _zipUrlx86 = zipUrlx86;
                _zipUrlx64 = zipUrlx64;
            }

            public Version Version
            {
                get { return _version; }
            }

            public DateTimeOffset ReleaseDate
            {
                get { return _releaseDate; }
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
                        if (process != null) {
                            process.WaitForExit();
                        }
                    }
                }
                finally {
                    try {
                        File.Delete(exePath);
                    }
                    catch (Exception ex) {
                        //_logger.ErrorException("Error deleting {0}", ex, exePath);
                    }
                }

                progress.Report(1);
            }

            public async Task InstallUpdate(IProgress<double> progress, IHttpClient httpClient, string x86Directory, string x64Directory)
            {
                string defaultInstallLocation = FindProgramFiles(@"LAV Filters");
                string dir = x86Directory ?? x64Directory;
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

        private class Update : IUpdate
        {
            private readonly ExistingInstall _installed;
            private readonly Release _newRelease;

            public static readonly Update Unavailable = new Update();

            private Update()
            {
            }
            
            public Update(ExistingInstall installed, Release newRelease)
            {
                _installed = installed;
                _newRelease = newRelease;
            }

            public Version Version
            {
                get { return _newRelease != null ? _newRelease.Version : new Version(0, 0, 0); }
            }

            public UpdateType Type
            {
                get
                {
                    if (_newRelease == null) {
                        return UpdateType.Unavailable;
                    }

                    if (_installed == null) {
                        return UpdateType.NewInstall;
                    }

                    return (_installed.ReleaseDate < _newRelease.ReleaseDate) ? UpdateType.NewRelease : UpdateType.UpToDate;
                }
            }

            public async Task Install(IProgress<double> progress, IHttpClient httpClient)
            {
                switch (Type) {
                    case UpdateType.NewInstall:
                        await _newRelease.NewInstall(progress, httpClient).ConfigureAwait(false);
                        break;
                    case UpdateType.NewRelease:
                        await _newRelease.InstallUpdate(progress, httpClient, _installed.X86Location, _installed.X64Location).ConfigureAwait(false);
                        break;
                    case UpdateType.UpToDate:
                    case UpdateType.Unavailable:
                        progress.Report(1);
                        break;
                }
            }
        }
    }
}