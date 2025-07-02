using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace Synchronizer.Test
{
    [TestClass]
    public sealed class FolderSynchronizerTest
    {
        [TestMethod]
        public void GivenFlatInputFolderAndEmptyOutputWhenSyncedThenSyncedWithoutIssues()
        {
            // GIVEN
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing testing.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData([0x12, 0x34, 0x56, 0xd2]) },
                { @"c:\backup", new MockDirectoryData() }
            });

            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem);

            // WHEN
            folderSynchronizer.SyncronizeFolders(@"c:\demo", @"c:\backup");

            // THEN
            var files = fileSystem.Directory.GetFiles(@"c:\backup");
            Assert.AreEqual(2, files.Length);

            var file1 = fileSystem.GetFile(@"c:\backup\jQuery.js");
            var file2 = fileSystem.GetFile(@"c:\backup\image.gif");
            
            Assert.IsNotNull(file1);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("some js"), file1.Contents);

            Assert.IsNotNull(file2);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56, 0xd2 }, file2.Contents);
        }
    }
}
