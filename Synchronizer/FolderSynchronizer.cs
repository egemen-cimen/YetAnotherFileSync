using Microsoft.Extensions.Logging;
using System.IO;

namespace Synchronizer
{
    public class FolderSynchronizer : IFolderSynchronizer
    {
        private readonly ILogger _logger;
        public FolderSynchronizer(ILogger logger)
        {
            _logger = logger;
        }

        public bool SyncronizeFolders(string sourcePath, string destinationPath)
        {
            var fullSourcePath = CheckAndGetFullPath(sourcePath);
            var fullDestinationPath = CheckAndGetFullPath(destinationPath);

            if (fullSourcePath == null || fullDestinationPath == null)
            {
                return false;
            }

            var sourcefiles = Directory.GetFiles(fullSourcePath, "*", SearchOption.AllDirectories);
            var destinationFiles = Directory.GetFiles(fullDestinationPath, "*", SearchOption.AllDirectories);

            foreach (var file in sourcefiles)
            {
                _logger.LogDebug("Source file: {File}", file);
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(fullDestinationPath, fileName), true);
            }

            return true;
        }

        private string? CheckAndGetFullPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
            {
                _logger.LogError("Folder {Path} does not exists as {FullPath}", path, fullPath);
                return null;
            }

            _logger.LogDebug("Folder {path} exists as {fullPath}", path, fullPath);
            return fullPath;
        }
    }
}
