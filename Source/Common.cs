using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
