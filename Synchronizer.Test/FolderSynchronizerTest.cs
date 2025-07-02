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
            var result = folderSynchronizer.SyncronizeFolders(@"c:\demo", @"c:\backup");

            // THEN
            Assert.IsTrue(result);

            var files = fileSystem.Directory.GetFiles(@"c:\backup");
            Assert.AreEqual(2, files.Length);

            var file1 = fileSystem.GetFile(@"c:\backup\jQuery.js");
            var file2 = fileSystem.GetFile(@"c:\backup\image.gif");

            Assert.IsNotNull(file1);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("some js"), file1.Contents);

            Assert.IsNotNull(file2);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56, 0xd2 }, file2.Contents);
        }

        [TestMethod]
        public void GivenFlatInputFolderAndOutputWithExtraFilesWhenSyncedThenExtraFilesAreDeleted()
        {
            // GIVEN
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing testing.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\backup\image.gif", new MockFileData([0x12, 0x34, 0x56, 0xd2]) }
            });

            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem);

            // WHEN
            var result = folderSynchronizer.SyncronizeFolders(@"c:\demo", @"c:\backup");

            // THEN
            Assert.IsTrue(result);

            var files = fileSystem.Directory.GetFiles(@"c:\backup");
            Assert.AreEqual(1, files.Length);

            var file1 = fileSystem.GetFile(@"c:\backup\jQuery.js");

            Assert.IsNotNull(file1);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("some js"), file1.Contents);
        }

        [TestMethod]
        public void GivenInputFolderWithSubDirsAndEmptyOutputWhenSyncedThenSyncedWithSubdirs()
        {
            // GIVEN
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing testing.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData([0x12, 0x34, 0x56, 0xd2]) },
                { @"c:\demo\some files\New Text Document.txt", new MockFileData("some text") },
                { @"c:\backup", new MockDirectoryData() }
            });

            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem);

            // WHEN
            var result = folderSynchronizer.SyncronizeFolders(@"c:\demo", @"c:\backup");

            // THEN
            Assert.IsTrue(result);

            var files = fileSystem.Directory.GetFiles(@"c:\backup");
            Assert.AreEqual(2, files.Length);

            var subdirFiles = fileSystem.Directory.GetFiles(@"c:\backup\some files");
            Assert.AreEqual(1, subdirFiles.Length);

            var file1 = fileSystem.GetFile(@"c:\backup\jQuery.js");
            var file2 = fileSystem.GetFile(@"c:\backup\image.gif");
            var file3 = fileSystem.GetFile(@"c:\backup\some files\New Text Document.txt");

            Assert.IsNotNull(file1);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("some js"), file1.Contents);

            Assert.IsNotNull(file2);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56, 0xd2 }, file2.Contents);

            Assert.IsNotNull(file3);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("some text"), file3.Contents);
        }

        [TestMethod]
        public void GivenInputFolderWithSubDirsAndOutputWithExtraFilesWhenSyncedThenExtraFilesAreDeleted()
        {
            // GIVEN
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing testing.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData([0x12, 0x34, 0x56, 0xd2]) },
                { @"c:\demo\some files\New Text Document.txt", new MockFileData("some text") },
                { @"c:\backup\some files\image2.gif", new MockFileData([0x21, 0x43, 0x65, 0x2d]) }
            });

            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem);

            // WHEN
            var result = folderSynchronizer.SyncronizeFolders(@"c:\demo", @"c:\backup");

            // THEN
            Assert.IsTrue(result);

            var files = fileSystem.Directory.GetFiles(@"c:\backup");
            Assert.AreEqual(2, files.Length);

            var subdirFiles = fileSystem.Directory.GetFiles(@"c:\backup\some files");
            Assert.AreEqual(1, subdirFiles.Length);

            var file1 = fileSystem.GetFile(@"c:\backup\jQuery.js");
            var file2 = fileSystem.GetFile(@"c:\backup\image.gif");
            var file3 = fileSystem.GetFile(@"c:\backup\some files\New Text Document.txt");

            Assert.IsNotNull(file1);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("some js"), file1.Contents);

            Assert.IsNotNull(file2);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56, 0xd2 }, file2.Contents);

            Assert.IsNotNull(file3);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("some text"), file3.Contents);
        }
    }
}
