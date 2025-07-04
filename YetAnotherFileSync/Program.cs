using Microsoft.Extensions.Logging;
using Synchronizer;
using System.Security.Cryptography;
using Serilog;
using System.Globalization;

namespace YetAnotherFileSync
{
    public class Program
    {
        private static bool _isSyncInProgress = false;
        private static IFolderSynchronizer? _folderSynchronizer;
        private static string _sourceDirectory = string.Empty;
        private static string _destinationDirectory = string.Empty;
        private static ILogger<Program>? _programLogger;

        static async Task Main(string[] args)
        {
            using var factory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("YetAnotherFileSync.Program", LogLevel.Information)
                    .AddFilter("Synchronizer.FolderSynchronizer", LogLevel.Information)
                    .AddConsole()
                    .AddSerilog();
            });
            _programLogger = factory.CreateLogger<Program>();
            _programLogger.LogInformation("Starting YetAnotherFileSync program");
            _programLogger.LogDebug("{Length} arguments given: {ArgsString}", args.Length, string.Join(", ", args));

            var fileSystem = new System.IO.Abstractions.FileSystem();

            if (args.Length != 4)
            {
                _programLogger.LogError("Needed arguments: <source folder path> <destination folder path> <synchronization interval in seconds> <log file path>");
                return;
            }

            var correctArguments = true;
            correctArguments &= CheckArgExistingDirectory(args[0], fileSystem);
            correctArguments &= CheckArgExistingDirectory(args[1], fileSystem);
            var isSyncIntervalArgumentValidInteger = int.TryParse(args[2], NumberStyles.Integer, CultureInfo.CurrentCulture, out int syncInterval);
            if (!isSyncIntervalArgumentValidInteger || syncInterval < 1)
            {
                _programLogger.LogError("The synchronization interval ({Arg}) needs to be a valid positive integer.", args[2]);
                correctArguments = false;
            }

            if (!correctArguments)
            {
                _programLogger.LogWarning("Arguments are not correct. Exiting.");
                return;
            }

            _sourceDirectory = args[0];
            _destinationDirectory = args[1];

            _programLogger.LogInformation("SyncInterval: `{SyncInterval}`.", syncInterval);

            var logPath = Path.GetFullPath(args[3]);
            _programLogger.LogInformation("Log Path: `{LogPath}`.", logPath);

            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .File(logPath)
                .CreateLogger();
            var folderSynchronizerLogger = factory.CreateLogger<FolderSynchronizer>();

            using var md5 = MD5.Create();
            _folderSynchronizer = new FolderSynchronizer(folderSynchronizerLogger, fileSystem, md5);

            System.Timers.Timer timer = new(interval: syncInterval * 1_000);
            timer.Elapsed += (sender, e) => HandleTimer();
            timer.Start();

            await Task.Delay(Timeout.Infinite);
        }

        private static void HandleTimer()
        {
            if (!_isSyncInProgress)
            {
                _isSyncInProgress = true;
                _folderSynchronizer?.SyncronizeFolders(_sourceDirectory, _destinationDirectory);
                _isSyncInProgress = false;
            }
            else
            {
                _programLogger?.LogWarning("Timer triggered but sync is already in progress.");
            }
        }

        private static bool CheckArgExistingDirectory(string arg, System.IO.Abstractions.FileSystem fileSystem)
        {
            if (fileSystem.File.Exists(arg) || !fileSystem.Directory.Exists(arg))
            {
                _programLogger?.LogError("The argument {Arg} is not a directory or does not exist.", arg);
                return false;
            }

            return true;
        }
    }
}
