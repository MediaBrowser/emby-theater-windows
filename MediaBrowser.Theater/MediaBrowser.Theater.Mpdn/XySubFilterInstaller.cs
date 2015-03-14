using System;
using System.Collections.Generic;
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
    public class XySubFilterInstaller
    {
        private const string Clsid = "{2DFCB782-EC20-4A7C-B530-4577ADB33F21}";
        private const string CodecsRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\CLSID\{083863F1-70DE-11D0-BD40-00A0C911CE86}\Instance\";

        public bool IsInstalled
        {
            get
            {
                var id = Registry.GetValue(CodecsRegistryKey + "{2DFCB782-EC20-4A7C-B530-4577ADB33F21}", "CLSID", "id");
                return id != null;
            }
        }

        public bool CanInstall
        {
            get { return UacHelper.IsProcessElevated; }
        }

        private async Task<Release> GetLatestRelease()
        {
            var github = new GitHubClient(new ProductHeaderValue("MediaBrowserTheater"));
            IReadOnlyList<Octokit.Release> releases = await github.Release.GetAll("Cyberbeing", "xy-VSFilter").ConfigureAwait(false);
            var versionedReleases = releases.Select(r => new { Version = FilterInstallation.ParseVersion(r.TagName), Release = r }).ToList();
            var latest = versionedReleases
                .Where(r => r.Version != null)
                .OrderByDescending(r => r.Version)
                .FirstOrDefault();

            if (latest == null) {
                return null;
            }

            IReadOnlyList<ReleaseAsset> assets = await github.Release.GetAssets("Cyberbeing", "xy-VSFilter", latest.Release.Id).ConfigureAwait(false);
            string x86Zip = assets.Where(a => a.Name.Contains("x86") && a.ContentType == "application/x-stuffit").Select(a => a.BrowserDownloadUrl).FirstOrDefault();
            string x64Zip = assets.Where(a => a.Name.Contains("x64") && a.ContentType == "application/x-stuffit").Select(a => a.BrowserDownloadUrl).FirstOrDefault();

            return new Release(latest.Version, x86Zip, x64Zip);
        }

        private ExistingInstall GetCurrentInstall()
        {
            if (!IsInstalled) {
                return null;
            }

            string path = GetInstallePath();
            if (string.IsNullOrEmpty(path)) {
                return null;
            }

            try {
                string x86, x64;
                FilterInstallation.SearchForBinaryPaths(Directory.GetParent(path).FullName, "XySubFilter.dll", out x86, out x64);

                var version = GetFileVersion(Path.Combine(x86 ?? x64, "XySubFilter.dll"));

                return new ExistingInstall(version, x86, x64);
            }
            catch {
                return null;
            }
        }

        private string GetInstallePath()
        {
            return FilterInstallation.FindProgramFiles(@"KCP\xy-subfilter") ??
                   GetRegistryLocation();

        }

        private static string GetRegistryLocation()
        {
            var path = Registry.GetValue(string.Format(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\CLSID\{0}\InprocServer32", Clsid), null, null) as string;
            if (path != null) {
                return Path.GetDirectoryName(path);
            }

            return null;
        }

        private Version GetFileVersion(string path)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(path);
            return new Version(versionInfo.ProductMajorPart, versionInfo.ProductMinorPart, versionInfo.ProductBuildPart, versionInfo.ProductPrivatePart);
        }
        public async Task<IUpdate> FindUpdate()
        {
            if (!UacHelper.IsProcessElevated)
            {
                throw new InvalidOperationException("XySubFilter installation or update requires elevated privileges.");
            }

            Release latestRelease = await GetLatestRelease().ConfigureAwait(false);
            ExistingInstall currentInstall = GetCurrentInstall();

            if (latestRelease == null) {
                return currentInstall == null ? Update.Unavailable : Update.UpToDate;
            }

            return new UpdateInstaller(currentInstall, latestRelease);
        }
        
        private class Release
        {
            private readonly Version _version;
            private readonly string _zipUrlx64;
            private readonly string _zipUrlx86;

            public Release(Version version, string zipUrlx64, string zipUrlx86)
            {
                _version = version;
                _zipUrlx64 = zipUrlx64;
                _zipUrlx86 = zipUrlx86;
            }

            public Version Version
            {
                get { return _version; }
            }
            
            public async Task NewInstall(IProgress<double> progress, IHttpClient httpClient)
            {
                await NewInstall(progress.SlicePercent(0, 50), httpClient, _zipUrlx86, FilterInstallation.GetProgramFiles(@"XySubFilter\x86")).ConfigureAwait(false);
                await NewInstall(progress.SlicePercent(50, 100), httpClient, _zipUrlx64, FilterInstallation.GetProgramFiles(@"XySubFilter\x64")).ConfigureAwait(false);
            }

            private async Task NewInstall(IProgress<double> progress, IHttpClient httpClient, string zip, string directory)
            {
                if (!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                await Extract(zip, directory, progress, httpClient).ConfigureAwait(false);

                using (Process process = Process.Start(new ProcessStartInfo {
                    FileName = "regsvr32.exe",
                    Arguments = string.Format(@"/s ""{0}""", Path.Combine(directory, "XySubFilter.dll"))
                })) {
                    if (process != null) {
                        process.WaitForExit();
                    }
                }
            }

            public async Task InstallUpdate(IProgress<double> progress, IHttpClient httpClient, string x86Directory, string x64Directory)
            {
                if (x86Directory != null && Directory.Exists(x86Directory)) {
                    await Extract(_zipUrlx86, x86Directory, progress.SlicePercent(0, 50), httpClient, "XySubFilter.dll").ConfigureAwait(false);
                }

                if (x64Directory != null && Directory.Exists(x64Directory)) {
                    await Extract(_zipUrlx64, x64Directory, progress.SlicePercent(50, 100), httpClient, "XySubFilter.dll").ConfigureAwait(false);
                }
            }

            private async Task Extract(string zipUrl, string directory, IProgress<double> progress, IHttpClient httpClient, string filter = null)
            {
                string tempFile = await httpClient.GetTempFile(new HttpRequestOptions {
                    Url = zipUrl,
                    Progress = progress.SlicePercent(0, 75)
                }).ConfigureAwait(false);

                string zipPath = Path.ChangeExtension(tempFile, ".zip");
                File.Move(tempFile, zipPath);

                try {
                    var zip = new FastZip();
                    zip.ExtractZip(zipPath, directory, filter);
                }
                finally {
                    try {
                        File.Delete(zipPath);
                    }
                    catch (Exception e) {
                        // todo log
                    }

                    progress.Report(100);
                }
            }
        }

        private class ExistingInstall
        {
            private readonly string _x64Location;
            private readonly Version _version;
            private readonly string _x86Location;

            public ExistingInstall(Version version, string x86Location, string x64Location)
            {
                _version = version;
                _x86Location = x86Location;
                _x64Location = x64Location;
            }

            public string X86Location
            {
                get { return _x86Location; }
            }
            
            public string X64Location
            {
                get { return _x64Location; }
            }

            public Version Version
            {
                get { return _version; }
            }
        }

        private class UpdateInstaller : IUpdate
        {
            private readonly ExistingInstall _installed;
            private readonly Release _newRelease;

            public UpdateInstaller(ExistingInstall installed, Release newRelease)
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
                    if (_newRelease == null && _installed == null)
                    {
                        return UpdateType.Unavailable;
                    }

                    if (_newRelease == null)
                    {
                        return UpdateType.UpToDate;
                    }

                    if (_installed == null)
                    {
                        return UpdateType.NewInstall;
                    }

                    return (_installed.Version < _newRelease.Version) ? UpdateType.NewRelease : UpdateType.UpToDate;
                }
            }

            public async Task Install(IProgress<double> progress, IHttpClient httpClient)
            {
                switch (Type)
                {
                    case UpdateType.NewInstall:
                        await _newRelease.NewInstall(progress, httpClient).ConfigureAwait(false);
                        break;
                    case UpdateType.NewRelease:
                        await _newRelease.InstallUpdate(progress, httpClient, _installed.X86Location, _installed.X64Location).ConfigureAwait(false);
                        break;
                    case UpdateType.UpToDate:
                    case UpdateType.Unavailable:
                        progress.Report(100);
                        break;
                }
            }
        }
    }
}
