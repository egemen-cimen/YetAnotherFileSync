using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Synchronizer.Test
{
    [TestClass]
    public sealed class FolderSynchronizerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");
            var folderSynchronizer = new FolderSynchronizer(logger);
            //folderSynchronizer.SyncronizeFolders("a", "b");
        }
    }
}
