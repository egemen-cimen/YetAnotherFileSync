using Microsoft.Extensions.Logging;
using Synchronizer;
using System.Security.Cryptography;
using Serilog;
using System.Globalization;

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
                    .AddConsole()
                    .AddSerilog();
            });
            var programLogger = factory.CreateLogger<Program>();
            programLogger.LogInformation("Starting YetAnotherFileSync program");
            programLogger.LogDebug("{Length} arguments given: {ArgsString}", args.Length, string.Join(", ", args));

            var fileSystem = new System.IO.Abstractions.FileSystem();

            if (args.Length != 4)
            {
                programLogger.LogError("Needed arguments: <source folder path> <destination folder path> <synchronization interval in seconds> <log file path>");
                return;
            }

            var correctArguments = true;
            correctArguments &= CheckArgExistingDirectory(args[0], programLogger, fileSystem);
            correctArguments &= CheckArgExistingDirectory(args[1], programLogger, fileSystem);
            var isSyncIntervalArgumentValidInteger = int.TryParse(args[2], NumberStyles.Integer, CultureInfo.CurrentCulture, out int syncInterval);
            if (!isSyncIntervalArgumentValidInteger || syncInterval < 1)
            {
                programLogger.LogError("The synchronization interval ({Arg}) needs to be a valid positive integer.", args[2]);
                correctArguments = false;
            }

            if (!correctArguments)
            {
                programLogger.LogWarning("Arguments are not correct. Exiting.");
                return;
            }

            programLogger.LogInformation("SyncInterval: `{SyncInterval}`.", syncInterval);

            var logPath = Path.GetFullPath(args[3]);
            programLogger.LogInformation("Log Path: `{LogPath}`.", logPath);

            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .File(logPath)
                .CreateLogger();
            var folderSynchronizerLogger = factory.CreateLogger<FolderSynchronizer>();

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(folderSynchronizerLogger, fileSystem, md5);
            folderSynchronizer.SyncronizeFolders(args[0], args[1]);
        }

        private static bool CheckArgExistingDirectory(string arg, ILogger<Program> programLogger, System.IO.Abstractions.FileSystem fileSystem)
        {
            if (fileSystem.File.Exists(arg) || !fileSystem.Directory.Exists(arg))
            {
                programLogger.LogError("The argument {Arg} is not a directory or does not exist.", arg);
                return false;
            }

            return true;
        }
    }
}
