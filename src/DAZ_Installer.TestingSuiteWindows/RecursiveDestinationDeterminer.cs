using DAZ_Installer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.TestingSuiteWindows
{
    /// <summary>
    /// A destination determiner that uses <see cref="DPDestinationDeterminerEx"/> to determine the destination of each file in the archive.
    /// This is a recursive destination determiner, meaning that it will also determine the destinations of subarchives/archives within the given archive.
    /// </summary>
    internal class RecursiveDestinationDeterminer : AbstractDestinationDeterminer
    {
        /// <inheritdoc/>
        public override HashSet<DPFile> DetermineDestinations(DPArchive arc, DPProcessSettings settings)
        {
            var hash = new DPDestinationDeterminerEx().DetermineDestinations(arc, settings);
            foreach (var subarc in arc.Subarchives.Where(x => x.Extracted))
            {
                hash.UnionWith(new DPDestinationDeterminerEx().DetermineDestinations(subarc, settings));
            }
            return hash;
        }
    }
}
