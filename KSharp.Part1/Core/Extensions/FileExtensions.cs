using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KSharp.Part1.Core
{
    public static class FileExtensions
    {
        public static IEnumerable<string> GetFiles(string directoryPath, Func<string,bool> searchPattern)
        {
            IEnumerable<string> GetFilesRec(DirectoryInfo info)
            {
                foreach (var t in info.GetFiles().Where(f => searchPattern(f.Name)).Select(f => f.FullName))
                    yield return t;
                foreach (var t in info.GetDirectories())
                    foreach (var b in GetFilesRec(t))
                        yield return b;
            }

            return GetFilesRec(new DirectoryInfo(directoryPath));
        }

        public const string ParentDir = "..";
        public static string GetProjectDir(this string assemblyDir)
        {
            return Path.GetFullPath(Path.Combine(assemblyDir, ParentDir, ParentDir, ParentDir));
        }
        public static string GetSolutionDir(this string assemblyDir)
        {
            return Path.GetFullPath(Path.Combine(assemblyDir, ParentDir, ParentDir, ParentDir, ParentDir));
        }
    }
}
