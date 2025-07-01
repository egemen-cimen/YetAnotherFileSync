using Synchronizer;

namespace YetAnotherFileSync
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var folderSynchronizer = new FolderSynchronizer();
            folderSynchronizer.SyncronizeFolders("C:\\Users\\ecime\\Desktop\\source", "C:\\Users\\ecime\\Desktop\\destination");
        }
    }
}
