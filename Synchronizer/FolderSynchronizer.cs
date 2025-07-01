using System.IO;

namespace Synchronizer
{
    public class FolderSynchronizer : IFolderSynchronizer
    {
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
                Console.WriteLine(file);
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(fullDestinationPath, fileName), true);
            }

            return true;
        }

        private static string? CheckAndGetFullPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
            {
                Console.WriteLine($"Folder {path} does not exists as {fullPath}");
                return null;
            }

            Console.WriteLine($"Folder {path} exists as {fullPath}");
            return fullPath;
        }
    }
}
