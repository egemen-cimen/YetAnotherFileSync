using Synchronizer;
using Microsoft.Extensions.Logging;

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
            programLogger.LogDebug("{Length} arguments given: {Args}", args.Length, string.Join(", ", args));

            var folderSynchronizer = new FolderSynchronizer(folderSynchronizerLogger);
            folderSynchronizer.SyncronizeFolders("C:\\Users\\ecime\\Desktop\\source", "C:\\Users\\ecime\\Desktop\\destination");
        }
    }
}
