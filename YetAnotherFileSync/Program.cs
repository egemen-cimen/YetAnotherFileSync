using Microsoft.Extensions.Logging;
using Synchronizer;
using System.Security.Cryptography;

namespace YetAnotherFileSync
{
    public class Program
    {
        static void Main(string[] args)
        {
            using var factory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("YetAnotherFileSync.Program", LogLevel.Debug)
                    .AddFilter("Synchronizer.FolderSynchronizer", LogLevel.Debug)
                    .AddConsole();
            });
            var programLogger = factory.CreateLogger<Program>();
            var folderSynchronizerLogger = factory.CreateLogger<FolderSynchronizer>();

            programLogger.LogInformation("Starting YetAnotherFileSync program");
            programLogger.LogDebug("{Length} arguments given: {ArgsString}", args.Length, string.Join(", ", args));

            var fileSystem = new System.IO.Abstractions.FileSystem();

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(folderSynchronizerLogger, fileSystem, md5);
            folderSynchronizer.SyncronizeFolders("C:\\Users\\ecime\\Desktop\\source 1", "C:\\Users\\ecime\\Desktop\\destination 2");
        }
    }
}
