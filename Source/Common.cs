using System.IO;

namespace SqlTool
{
    public class Common
    {
        public static string CreateDirectory(string parentDirectoryName, string subDirectoryName)
        {
            string subDirectoryPath = Path.Combine(parentDirectoryName, subDirectoryName);
            Directory.CreateDirectory(subDirectoryPath);
            return subDirectoryPath;
        }
    }
}
