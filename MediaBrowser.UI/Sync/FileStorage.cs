using MediaBrowser.ApiInteraction.Data;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.IO;
using MediaBrowser.Model.Sync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.UI.Sync
{
    public class FileStorage : IFileRepository
    {
        private readonly IFileSystem _fileSystem;
        private readonly IApplicationPaths _appPaths;

        private string SyncRootPath
        {
            get { return Path.Combine(_appPaths.ProgramDataPath, "sync", "files"); }
        }

        public FileStorage(IFileSystem fileSystem, IApplicationPaths appPaths)
        {
            _fileSystem = fileSystem;
            _appPaths = appPaths;
        }

        public string GetFullLocalPath(IEnumerable<string> path)
        {
            var paths = path.ToList();
            paths.Insert(0, SyncRootPath);
            return Path.Combine(paths.ToArray());
        }

        public Task DeleteFile(string path)
        {
            File.Delete(path);
            return Task.FromResult(true);
        }

        public Task DeleteFolder(string path)
        {
            Directory.Delete(path, true);
            return Task.FromResult(true);
        }

        public Task<List<DeviceFileInfo>> GetFileSystemEntries(string path)
        {
            var entries = new DirectoryInfo(path)
                .EnumerateFileSystemInfos()
                .Select(i => new DeviceFileInfo
                {
                    Name = i.Name,
                    Path = i.FullName
                })
                .ToList();

            return Task.FromResult(entries);
        }

        public string GetValidFileName(string name)
        {
            return _fileSystem.GetValidFilename(name);
        }

        public async Task SaveFile(Stream stream, IEnumerable<string> path)
        {
            var fullPath = GetFullLocalPath(path);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
        }

        private string[] GetPathParts(string path)
        {
            path = path.Substring(SyncRootPath.Length);
            
            return path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
        }


        public Task<bool> FileExists(string path)
        {
            return Task.FromResult(File.Exists(path));
        }

        public async Task SaveFile(Stream stream, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
        }

        public string GetParentDirectoryPath(string path)
        {
            return Path.GetDirectoryName(path);
        }
    }
}
