using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAZ_Installer.IO;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.Core;

namespace DAZ_Installer.TestingSuiteWindows
{
    internal static class ResultCompiler
    {
        public struct ObjectResult
        {
            public string ArchiveName { get; set; }
            public Dictionary<string, string> Mappings;
        }

        private static readonly JsonSerializerOptions options = new()
        {
            IncludeFields = true,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = false,
            IgnoreReadOnlyFields = false,
        };

        public static string CompileResults(IEnumerable<DPExtractionReport> extractionReports, DPProcessSettings settings)
        {
            var a = new List<ObjectResult>();
            foreach (var report in extractionReports)
            {
                a.Add(new ObjectResult
                {
                    ArchiveName = report.Settings.Archive.FileName,
                    Mappings = report.ExtractedFiles.ToDictionary(x => x.Path, x => x.RelativePathToContentFolder)
                }) ;
            }

            return JsonSerializer.Serialize(new { ProcessSettings = settings, Results = a }, options);
        }

        public static string CompileResults(TreeNodeCollection rootNodes, DPProcessSettings settings)
        {
            var l = new List<ObjectResult>();
            foreach (TreeNode rootNode in rootNodes) 
            {
                l.Add(new ObjectResult
                {
                    ArchiveName = rootNode.Text,
                    Mappings = makeMappings(rootNode)
                });
            }

            return JsonSerializer.Serialize(new { ProcessSettings = settings, Results = l }, options);
        }

        private static Dictionary<string, string> makeMappings(TreeNode node)
        {
            if (node.Nodes.Count == 0) return new Dictionary<string, string>();
            var d = new Dictionary<string, string>(((DPAbstractNode) node.Nodes[0].Tag).AssociatedArchive!.Contents.Count);

            // Create an iterative approach to this.
            var stack = new Stack<TreeNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var currentNode = stack.Pop();
                if (currentNode.ForeColor != Color.Green || currentNode.Tag is not DPFile file) continue;
                d[file.Path] = file.RelativePathToContentFolder;
                foreach (TreeNode childNode in currentNode.Nodes)
                {
                    stack.Push(childNode);
                }
            }

            return d;
        }
    }
}
