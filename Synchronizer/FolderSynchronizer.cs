using Microsoft.Extensions.Logging;

namespace Synchronizer
{
    public class FolderSynchronizer(ILogger logger, System.IO.Abstractions.IFileSystem fileSystem) : IFolderSynchronizer
    {
        private readonly ILogger _logger = logger;
        private readonly System.IO.Abstractions.IFileSystem _fileSystem = fileSystem;

        public bool SyncronizeFolders(string sourcePath, string destinationPath)
        {
            var fullSourcePath = CheckDirectoryAndGetFullPath(sourcePath);
            var fullDestinationPath = CheckDirectoryAndGetFullPath(destinationPath);

            if (fullSourcePath == null || fullDestinationPath == null)
            {
                return false;
            }

            var sourcefiles = _fileSystem.Directory.GetFiles(fullSourcePath, "*", SearchOption.AllDirectories);
            var destinationFiles = _fileSystem.Directory.GetFiles(fullDestinationPath, "*", SearchOption.AllDirectories);

            foreach (var file in sourcefiles)
            {
                var relativePath = Path.GetRelativePath(fullSourcePath, file);
                var sourceFileName = Path.Combine(fullSourcePath, relativePath);
                var destFileName = Path.Combine(fullDestinationPath, relativePath);

                _logger.LogInformation("Copying `{Relative}` from `{Source}` to `{Destination}`.", relativePath, sourceFileName, fullDestinationPath);

                var destDirectoryName = Path.GetDirectoryName(destFileName);
                if (destDirectoryName != null)
                {
                    _logger.LogDebug("Trying to create directory `{DestinationDirectoryName}`.", destDirectoryName);
                    _fileSystem.Directory.CreateDirectory(destDirectoryName);
                    _fileSystem.File.Copy(sourceFileName, destFileName, true);
                }
                else
                {
                    _logger.LogError("Unexpected state");
                }
            }

            foreach (var file in destinationFiles)
            {
                var relativePath = Path.GetRelativePath(fullDestinationPath, file);
                var sourceFileName = Path.Combine(fullSourcePath, relativePath);
                var destFileName = Path.Combine(fullDestinationPath, relativePath);

                _logger.LogDebug("Checking if `{Relative}` file in `{Destination}` exists as `{source}`.", relativePath, fullDestinationPath, sourceFileName);
                if (!_fileSystem.File.Exists(sourceFileName))
                {
                    _logger.LogInformation("Deleting {DestFileName} because it doesn't exist in source directory.", destFileName);
                    _fileSystem.File.Delete(destFileName);
                }
            }

            var destinationDirectories = _fileSystem.Directory.GetDirectories(fullDestinationPath, "*", SearchOption.AllDirectories);

            foreach (var directory in destinationDirectories)
            {
                var relativePath = Path.GetRelativePath(fullDestinationPath, directory);
                var sourceDirectoryName = Path.Combine(fullSourcePath, relativePath);
                var destDirectoryName = Path.Combine(fullDestinationPath, relativePath);

                _logger.LogDebug("Checking if `{Relative}` directory in `{Destination}` exists as `{source}`.", relativePath, fullDestinationPath, sourceDirectoryName);
                if (!_fileSystem.Directory.Exists(sourceDirectoryName))
                {
                    _logger.LogInformation("Deleting `{DestFileName}` because it doesn't exist in source directory.", destDirectoryName);
                    _fileSystem.Directory.Delete(destDirectoryName, true);
                }
            }

            return true;
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
