// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.DP
{
    // TO DO: Install ID.
    /// <summary>
    /// DPIDManager should be used to generate IDs for identifying DP Objects and nothing more. Should not be used for product IDs. 
    /// When objects are deleted, they are removed from dpObjects.
    /// </summary>
    internal struct DPIDManager
    {
        internal static uint lastID = 0;

        /// <summary>
        ///
        /// </summary>
        /// <returns>A unique new tag.</returns>
        internal static uint GetNewID()
        {
            uint id = lastID;
            while (true)
            {
                if (DPGlobal.dpObjects.ContainsKey(id)) id++;
                else
                {
                    lastID = id;
                    return id;
                }
            }
        }

        internal static void RemoveID(uint id)
        {
            DPGlobal.dpObjects.Remove(id);
        }


    }
}
