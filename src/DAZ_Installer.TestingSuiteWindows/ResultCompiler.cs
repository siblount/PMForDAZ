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
        public static string CompileResults(IEnumerable<DPExtractionReport> extractionReports, DPProcessSettings settings, DPArchive arc)
        {
            var l = new List<DPArchiveMap>();
            foreach (var report in extractionReports)
            {
                l.Add(new DPArchiveMap
                {
                    ArchiveName = report.Settings.Archive.FileName,
                    Mappings = report.ExtractedFiles.ToDictionary(x => x.Path, x => x.RelativePathToContentFolder)
                });
            }

            return new DPProcessorTestManifest(settings, arc.FileName, l).ToJson();
        }

        public static string CompileResults(TreeNodeCollection rootNodes, DPProcessSettings settings, DPArchive arc)
        {
            var l = new List<DPArchiveMap>();
            foreach (TreeNode rootNode in rootNodes)
            {
                l.Add(new DPArchiveMap
                {
                    ArchiveName = rootNode.Text,
                    Mappings = makeMappings(rootNode)
                });
            }

            return new DPProcessorTestManifest(settings, arc.FileName, l).ToJson();
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
