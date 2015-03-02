using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using MediaBrowser.Common.Net;
using Octokit;

namespace MediaBrowser.Theater.Mpdn
{
    public class LavFiltersInstaller
    {
        private static readonly Regex VersionRegex = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)(\.(?<build>\d+))?.*$");

        private async Task<Release> GetLatestRelease()
        {
            var github = new GitHubClient(new ProductHeaderValue("Media Browser Theater"));
            var releases = await github.Release.GetAll("Nevcairiel", "LAVFilters").ConfigureAwait(false);
            var versionedReleases = releases.Select(r => new { Version = ParseVersion(r.TagName), Release = r }).ToList();
            var latest = versionedReleases
                .Where(r => r.Version != null)
                .OrderByDescending(r => r.Version)
                .FirstOrDefault();

            if (latest == null) {
                return null;
            }

            var assets = await github.Release.GetAssets("Nevcairiel", "LAVFilters", latest.Release.Id).ConfigureAwait(false);
            var installer = assets.FirstOrDefault(a => a.ContentType == "application/x-msdownload");
            var zip = assets.FirstOrDefault(a => a.Name.Contains("x86") && a.ContentType == "application/x-zip-compressed");

            return new Release(latest.Version,
                               installer != null ? installer.BrowserDownloadUrl : null,
                               zip != null ? zip.BrowserDownloadUrl : null);
        }

        private Version ParseVersion(string tagName)
        {
            var match = VersionRegex.Match(tagName);
            if (match.Success) {
                return new Version(int.Parse(match.Groups["major"].Value),
                                   int.Parse(match.Groups["minor"].Value),
                                   match.Groups["build"].Success ? int.Parse(match.Groups["build"].Value) : 0);
            }

            return null;
        }

        private Install GetCurrentInstall()
        {
            return null;
        }

        public async Task InstallOrUpdate(IProgress<double> progress, IHttpClient httpClient)
        {
            var latestRelease = await GetLatestRelease().ConfigureAwait(false);
            var currentInstall = GetCurrentInstall();

            if (latestRelease == null) {
                return;
            }

            if (currentInstall == null) {
                await latestRelease.NewInstall(progress, httpClient).ConfigureAwait(false);
            }
            else if (currentInstall.Version < latestRelease.Version) {
                await latestRelease.Update(progress, httpClient, currentInstall.Location).ConfigureAwait(false);
            }
        }
        
        private class Release
        {
            private readonly Version _version;
            private readonly string _installerUrl;
            private readonly string _zipUrl;

            public Release(Version version, string installerUrl, string zipUrl)
            {
                _version = version;
                _installerUrl = installerUrl;
                _zipUrl = zipUrl;
            }

            public Version Version
            {
                get { return _version; }
            }

            public async Task NewInstall(IProgress<double> progress, IHttpClient httpClient)
            {
                // todo log error cases

                string tempFile = await httpClient.GetTempFile(new HttpRequestOptions {
                    Url = _installerUrl,
                    Progress = progress
                }).ConfigureAwait(false);

                string exePath = Path.ChangeExtension(tempFile, ".exe");
                File.Move(tempFile, exePath);

                try {
                    using (Process process = Process.Start(new ProcessStartInfo {
                        FileName = exePath,
                        Arguments = "/VERYSILENT",
                        UseShellExecute = false
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
            }

            public async Task Update(IProgress<double> progress, IHttpClient httpClient, string directory)
            {
                string tempFile = await httpClient.GetTempFile(new HttpRequestOptions {
                    Url = _zipUrl,
                    Progress = progress
                }).ConfigureAwait(false);

                string zipPath = Path.ChangeExtension(tempFile, ".zip");
                File.Move(tempFile, zipPath);

                var zip = new FastZip();
                zip.ExtractZip(zipPath, directory, @"-\.bat$");
            }
        }

        private class Install
        {
            private readonly string _location;
            private readonly Version _version;

            public Install(string location, Version version)
            {
                _location = location;
                _version = version;
            }

            public string Location
            {
                get { return _location; }
            }

            public Version Version
            {
                get { return _version; }
            }
        }
    }
}
