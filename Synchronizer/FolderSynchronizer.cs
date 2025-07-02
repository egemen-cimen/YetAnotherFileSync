using Microsoft.Extensions.Logging;

namespace Synchronizer
{
    public class FolderSynchronizer : IFolderSynchronizer
    {
        private readonly ILogger _logger;
        private readonly System.IO.Abstractions.IFileSystem _fileSystem;

        public FolderSynchronizer(ILogger logger, System.IO.Abstractions.IFileSystem fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public bool SyncronizeFolders(string sourcePath, string destinationPath)
        {
            var fullSourcePath = CheckAndGetFullPath(sourcePath);
            var fullDestinationPath = CheckAndGetFullPath(destinationPath);

            if (fullSourcePath == null || fullDestinationPath == null)
            {
                return false;
            }

            var sourcefiles = _fileSystem.Directory.GetFiles(fullSourcePath, "*", SearchOption.AllDirectories);
            var destinationFiles = _fileSystem.Directory.GetFiles(fullDestinationPath, "*", SearchOption.AllDirectories);

            foreach (var file in sourcefiles)
            {
                _logger.LogDebug("Source file detected: {File}", file);
                var fileName = Path.GetFileName(file);
                _fileSystem.File.Copy(file, Path.Combine(fullDestinationPath, fileName), true);
            }

            return true;
        }

        private string? CheckAndGetFullPath(string path)
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
