using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Build
{
    public interface IPathFinder
    {
        string FindPath(string path);
    }

    internal class PathFinder : IPathFinder
    {
        public string WorkingDirectory;
        public byte MaxDepth = 6;
        public PathFinder(string cwd)
        {
            WorkingDirectory = cwd;
        }
        public string FindPath(string path)
        {
            var components = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var result = FindPathRecursive(WorkingDirectory, components, 0, 0);
            return result ?? throw new FileNotFoundException($"Could not find path: {path} in {WorkingDirectory}");
        }

        private string? FindPathRecursive(string currentPath, string[] components, int index, int depth)
        {
            if (depth > MaxDepth) return null;

            if (index == components.Length - 1)
            {
                // Check if it's a file or directory
                var fullPath = Path.Combine(currentPath, components[index]);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            var searchPattern = $"*{components[index]}*";
            foreach (var dir in Directory.EnumerateDirectories(currentPath, searchPattern, SearchOption.TopDirectoryOnly))
            {
                if (index == components.Length - 1)
                {
                    return dir;
                }

                var result = FindPathRecursive(dir, components, index + 1, depth + 1);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

    }
}
