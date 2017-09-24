using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MediaBrowser.Model.IO;

namespace Emby.Theater.IO
{
    public class SharpCifsFileSystem
    {
        private readonly MediaBrowser.Model.System.OperatingSystem _operatingSystem;

        public SharpCifsFileSystem(MediaBrowser.Model.System.OperatingSystem operatingSystem)
        {
            _operatingSystem = operatingSystem;
        }

        public bool IsEnabledForPath(string path)
        {
            if (_operatingSystem == MediaBrowser.Model.System.OperatingSystem.Windows)
            {
                return false;
            }

            return path.StartsWith("smb://", StringComparison.OrdinalIgnoreCase) || IsUncPath(path);
        }

        public string NormalizePath(string path)
        {
            if (path.StartsWith("smb://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            if (IsUncPath(path))
            {
                return ConvertUncToSmb(path);
            }

            return path;
        }

        public string GetDirectoryName(string path)
        {
            var separator = GetDirectorySeparatorChar(path);
            var result = Path.GetDirectoryName(path);

            if (separator == '/')
            {
                result = result.Replace('\\', '/');

                if (result.StartsWith("smb:/", StringComparison.OrdinalIgnoreCase) && !result.StartsWith("smb://", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Replace("smb:/", "smb://");
                }
            }

            return result;
        }

        public char GetDirectorySeparatorChar(string path)
        {
            if (path.IndexOf('/') != -1)
            {
                return '/';
            }

            return '\\';
        }

        public FileSystemMetadata GetFileSystemInfo(string path)
        {
            throw new NotImplementedException();
        }

        public FileSystemMetadata GetFileInfo(string path)
        {
            throw new NotImplementedException();
        }

        public FileSystemMetadata GetDirectoryInfo(string path)
        {
            throw new NotImplementedException();
        }

        private bool IsUncPath(string path)
        {
            return path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase);
        }

        private string ConvertUncToSmb(string path)
        {
            if (IsUncPath(path))
            {
                path = path.Replace('\\', '/');
                path = "smb:" + path;
            }
            return path;
        }

        private string AddAuthentication(string path)
        {
            return path;
        }

        public void SetHidden(string path, bool isHidden)
        {
            
        }

        public void SetReadOnly(string path, bool isReadOnly)
        {
        }

        public void SetAttributes(string path, bool isHidden, bool isReadOnly)
        {
        }

        public void DeleteFile(string path)
        {
        }

        public void DeleteDirectory(string path, bool recursive)
        {
        }

        public void CreateDirectory(string path)
        {
        }

        public string[] ReadAllLines(string path)
        {
            var lines = new List<string>();

            using (var stream = OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        lines.Add(reader.ReadLine());
                    }
                }
            }

            return lines.ToArray();
        }

        public void WriteAllLines(string path, IEnumerable<string> lines)
        {
            using (var stream = GetFileStream(path, FileOpenMode.Create, FileAccessMode.Write, FileShareMode.None))
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var line in lines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }

        public Stream OpenRead(string path)
        {
            throw new NotImplementedException();
        }

        private Stream OpenWrite(string path)
        {
            throw new NotImplementedException();
        }

        public void CopyFile(string source, string target, bool overwrite)
        {
            if (string.Equals(source, target, StringComparison.Ordinal))
            {
                throw new ArgumentException("Cannot CopyFile when source and target are the same");
            }

            using (var input = OpenRead(source))
            {
                using (var output = GetFileStream(target, FileOpenMode.Create, FileAccessMode.Write, FileShareMode.None))
                {
                    input.CopyTo(output);
                }
            }
        }

        public void MoveFile(string source, string target)
        {
            if (string.Equals(source, target, StringComparison.Ordinal))
            {
                throw new ArgumentException("Cannot MoveFile when source and target are the same");
            }

            using (var input = OpenRead(source))
            {
                using (var output = GetFileStream(target, FileOpenMode.Create, FileAccessMode.Write, FileShareMode.None))
                {
                    input.CopyTo(output);
                }
            }

            DeleteFile(source);
        }

        public void MoveDirectory(string source, string target)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public string ReadAllText(string path, Encoding encoding)
        {
            using (var stream = OpenRead(path))
            {
                using (var reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public Stream GetFileStream(string path, FileOpenMode mode, FileAccessMode access, FileShareMode share)
        {
            throw new NotImplementedException();
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            using (var stream = GetFileStream(path, FileOpenMode.Create, FileAccessMode.Write, FileShareMode.None))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void WriteAllText(string path, string text)
        {
            using (var stream = GetFileStream(path, FileOpenMode.Create, FileAccessMode.Write, FileShareMode.None))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(text);
                }
            }
        }

        public void WriteAllText(string path, string text, Encoding encoding)
        {
            using (var stream = GetFileStream(path, FileOpenMode.Create, FileAccessMode.Write, FileShareMode.None))
            {
                using (var writer = new StreamWriter(stream, encoding))
                {
                    writer.Write(text);
                }
            }
        }

        public string ReadAllText(string path)
        {
            using (var stream = OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public byte[] ReadAllBytes(string path)
        {
            using (var stream = OpenRead(path))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }

        public IEnumerable<FileSystemMetadata> GetDirectories(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileSystemMetadata> GetFiles(string path, string[] extensions, bool enableCaseSensitiveExtensions, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileSystemMetadata> GetFileSystemEntries(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFileSystemEntryPaths(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFilePaths(string path, string[] extensions, bool enableCaseSensitiveExtensions, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirectoryPaths(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }
    }
}
