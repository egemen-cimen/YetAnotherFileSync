using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Synchronizer
{
    public class FolderSynchronizer(ILogger logger, System.IO.Abstractions.IFileSystem fileSystem, MD5 md5) : IFolderSynchronizer
    {
        private readonly ILogger _logger = logger;
        private readonly System.IO.Abstractions.IFileSystem _fileSystem = fileSystem;
        private readonly MD5 _md5 = md5;

        public bool SyncronizeFolders(string sourcePath, string destinationPath)
        {
            var sourceFullPath = CheckDirectoryAndGetFullPath(sourcePath);
            var destinationFullPath = CheckDirectoryAndGetFullPath(destinationPath);

            if (sourceFullPath == null || destinationFullPath == null)
            {
                return false;
            }

            try
            {
                var sourceAllFiles = _fileSystem.Directory.GetFiles(sourceFullPath, "*", SearchOption.AllDirectories);
                var destinationAllFiles = _fileSystem.Directory.GetFiles(destinationFullPath, "*", SearchOption.AllDirectories);
                var sourceAllDirectories = _fileSystem.Directory.GetDirectories(sourceFullPath, "*", SearchOption.AllDirectories);
                var destinationAllDirectories = _fileSystem.Directory.GetDirectories(destinationFullPath, "*", SearchOption.AllDirectories);

                var sourceAllFilesRelativePaths = sourceAllFiles.Select(f => Path.GetRelativePath(sourceFullPath, f)).ToList();
                var destinationAllFilesRelativePaths = destinationAllFiles.Select(f => Path.GetRelativePath(destinationFullPath, f)).ToList();
                var sourceAllDirectoriesRelativePaths = sourceAllDirectories.Select(f => Path.GetRelativePath(sourceFullPath, f)).ToList();
                var destinationAllDirectoriesRelativePaths = destinationAllDirectories.Select(f => Path.GetRelativePath(destinationFullPath, f)).ToList();

                var sourceOnlyFilesRelativePaths = sourceAllFilesRelativePaths.Except(destinationAllFilesRelativePaths).ToList();
                var destinationOnlyFilesRelativePaths = destinationAllFilesRelativePaths.Except(sourceAllFilesRelativePaths).ToList();
                var sourceOnlyDirectoriesRelativePaths = sourceAllDirectoriesRelativePaths.Except(destinationAllDirectoriesRelativePaths).ToList();
                var destinationOnlyDirectoriesRelativePaths = destinationAllDirectoriesRelativePaths.Except(sourceAllDirectoriesRelativePaths).ToList();

                var commonFilesRelativePaths = sourceAllFilesRelativePaths.Intersect(destinationAllFilesRelativePaths).ToList();

                foreach (var directoryRelativePath in sourceOnlyDirectoriesRelativePaths)
                {
                    var destinationDirectoryName = Path.Combine(destinationFullPath, directoryRelativePath);
                    _logger.LogDebug("Creating directory `{DestinationDirectoryName}`.", destinationDirectoryName);
                    _fileSystem.Directory.CreateDirectory(destinationDirectoryName);
                }

                foreach (var fileRelativePath in sourceOnlyFilesRelativePaths)
                {
                    var sourceFileName = Path.Combine(sourceFullPath, fileRelativePath);
                    var destinationFileName = Path.Combine(destinationFullPath, fileRelativePath);
                    _logger.LogInformation("Copying `{FileRelativePath}` from `{Source}` to `{Destination}`.", fileRelativePath, sourceFileName, destinationFileName);
                    _fileSystem.File.Copy(sourceFileName, destinationFileName, true);
                }

                foreach (var fileRelativePath in commonFilesRelativePaths)
                {
                    var sourceFileName = Path.Combine(sourceFullPath, fileRelativePath);
                    var destinationFileName = Path.Combine(destinationFullPath, fileRelativePath);

                    var sourceFileHash = CalculateMd5(sourceFileName);
                    var destinationFileHash = CalculateMd5(destinationFileName);
                    if (!sourceFileHash.SequenceEqual(destinationFileHash))
                    {
                        _logger.LogInformation("Copying `{FileRelativePath}` from `{Source}` to `{Destination}`.", fileRelativePath, sourceFileName, destinationFileName);
                        _fileSystem.File.Copy(sourceFileName, destinationFileName, true);
                    }
                }

                foreach (var fileRelativePath in destinationOnlyFilesRelativePaths)
                {
                    var destinationFileName = Path.Combine(destinationFullPath, fileRelativePath);
                    _logger.LogInformation("Deleting {DestinationFileName} because it doesn't exist in source directory.", destinationFileName);
                    _fileSystem.File.Delete(destinationFileName);
                }

                foreach (var directoryRelativePath in destinationOnlyDirectoriesRelativePaths)
                {
                    var destinationDirectoryName = Path.Combine(destinationFullPath, directoryRelativePath);
                    _logger.LogInformation("Deleting `{DestinationDirectoryName}` because it doesn't exist in source directory.", destinationDirectoryName);
                    _fileSystem.Directory.Delete(destinationDirectoryName, true);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured during sync. Stopping sync.");
                return false;
            }

            _logger.LogInformation("Sync is complete");
            return true;
        }

        private byte[] CalculateMd5(string filename)
        {
            using var stream = _fileSystem.File.OpenRead(filename);
            return _md5.ComputeHash(stream);
        }

        private string? CheckDirectoryAndGetFullPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (!_fileSystem.Directory.Exists(fullPath))
            {
                _logger.LogError("Folder {Path} does not exists as {FullPath}", path, fullPath);
                return null;
            }

            _logger.LogDebug("Folder {Path} exists as {FullPath}", path, fullPath);
            return fullPath;
        }
    }
}
