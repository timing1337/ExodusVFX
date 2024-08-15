using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format
{
    public enum MetroFlags
    {
        None = 0,
        HasDebugInfo = 1,
        Editor = 2,
        StringsTable = 4,
        Plain = 8,
        NoSections = 16,
        MultiChunk = 32,
    }
}
