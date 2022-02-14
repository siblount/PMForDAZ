// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer
{
    struct DPRange
    {
        uint start { get; set; }
        uint end { get; set; }
        DPRange(uint _start, uint _end)
        {
            start = _start;
            end = _end;
        }

    }
}
