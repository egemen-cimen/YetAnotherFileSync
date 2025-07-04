using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Security.Cryptography;

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

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem, md5);

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
        public void GivenSameInputAndOutputFoldersWhenSyncedThenNothingIsCopied()
        {
            // GIVEN
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");

            var sourceFileData = new MockFileData([0x12, 0x34, 0x56, 0xd2])
            {
                LastWriteTime = new DateTimeOffset(2022, 2, 22, 22, 22, 22, TimeSpan.Zero)
            };
            var destinationFileData = new MockFileData([0x12, 0x34, 0x56, 0xd2])
            {
                LastWriteTime = new DateTimeOffset(2023, 3, 23, 23, 23, 23, TimeSpan.Zero)
            };
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\demo\image.gif", sourceFileData },
                { @"c:\backup\image.gif", destinationFileData }
            });

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem, md5);

            // WHEN
            var result = folderSynchronizer.SyncronizeFolders(@"c:\demo", @"c:\backup");

            // THEN
            Assert.IsTrue(result);

            var files = fileSystem.Directory.GetFiles(@"c:\backup");
            Assert.AreEqual(1, files.Length);

            var file = fileSystem.GetFile(@"c:\backup\image.gif");

            Assert.IsNotNull(file);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56, 0xd2 }, file.Contents);

            Assert.AreEqual(destinationFileData.LastWriteTime, file?.LastWriteTime);
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

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem, md5);

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

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem, md5);

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

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem, md5);

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
        public void GivenInputFolderWithSubDirsAndOutputWithExtraSubdirsWhenSyncedThenExtraSubdirsAreDeleted()
        {
            // GIVEN
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing testing.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData([0x12, 0x34, 0x56, 0xd2]) },
                { @"c:\backup\some files\image2.gif", new MockFileData([0x21, 0x43, 0x65, 0x2d]) }
            });

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem, md5);

            // WHEN
            var result = folderSynchronizer.SyncronizeFolders(@"c:\demo", @"c:\backup");

            // THEN
            Assert.IsTrue(result);

            var files = fileSystem.Directory.GetFiles(@"c:\backup");
            Assert.AreEqual(2, files.Length);

            var subdirExists = fileSystem.Directory.Exists(@"c:\backup\some files");
            Assert.IsFalse(subdirExists);

            var file1 = fileSystem.GetFile(@"c:\backup\jQuery.js");
            var file2 = fileSystem.GetFile(@"c:\backup\image.gif");

            Assert.IsNotNull(file1);
            Assert.IsNotNull(file2);
        }

        [TestMethod]
        public void GivenInputFolderWithSubDirsAndOutputWithRecursiveExtraSubdirsWhenSyncedThenExtraSubdirsAreDeleted()
        {
            // GIVEN
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing testing.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData([0x12, 0x34, 0x56, 0xd2]) },
                { @"c:\demo\some files\abc.gif", new MockFileData([0x21, 0x43, 0x65, 0x2d]) },
                { @"c:\backup\some files\directory\image2.gif", new MockFileData([0x21, 0x43, 0x65, 0x2d]) },
                { @"c:\backup\some files\directory 2", new MockDirectoryData() }
            });

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem, md5);

            // WHEN
            var result = folderSynchronizer.SyncronizeFolders(@"c:\demo", @"c:\backup");

            // THEN
            Assert.IsTrue(result);

            var files = fileSystem.Directory.GetFiles(@"c:\backup");
            Assert.AreEqual(2, files.Length);

            var subdir1Exists = fileSystem.Directory.Exists(@"c:\backup\some files");
            Assert.IsTrue(subdir1Exists);

            var subdir2Exists = fileSystem.Directory.Exists(@"c:\backup\some files\directory");
            Assert.IsFalse(subdir2Exists);

            var subdir3Exists = fileSystem.Directory.Exists(@"c:\backup\some files\directory 2");
            Assert.IsFalse(subdir3Exists);

            var file1 = fileSystem.GetFile(@"c:\backup\jQuery.js");
            var file2 = fileSystem.GetFile(@"c:\backup\image.gif");

            Assert.IsNotNull(file1);
            Assert.IsNotNull(file2);
        }

        [TestMethod]
        public void GivenFileWithTheSameNameInInputAndOutputDirectoriesWhenSyncedThenFileContentIsUpdated()
        {
            // GIVEN
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\backup\jQuery.js", new MockFileData("some other js") }
            });

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem, md5);

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
        public void GivenEmptyInputFolderAndOutputWithRecursiveDirectoriesWhenSyncedThenOutputIsEmpty()
        {
            // GIVEN
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger("test");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing testing.") },
                { @"c:\demo", new MockDirectoryData() },
                { @"c:\backup\jQuery.js", new MockFileData("some js") },
                { @"c:\backup\some files\images\image.gif", new MockFileData([0x12, 0x34, 0x56, 0xd2]) },
                { @"c:\backup\some files\New Text Document.txt", new MockFileData("some text") }
            });

            using var md5 = MD5.Create();
            var folderSynchronizer = new FolderSynchronizer(logger, fileSystem, md5);

            // WHEN
            var result = folderSynchronizer.SyncronizeFolders(@"c:\demo", @"c:\backup");

            // THEN
            Assert.IsTrue(result);

            var files = fileSystem.Directory.GetFiles(@"c:\backup");
            Assert.AreEqual(0, files.Length);
        }
    }
}
