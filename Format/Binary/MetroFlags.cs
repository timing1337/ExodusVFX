using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format.Binary
{
    public class MetroFlags
    {
        public static readonly int None = 0;
        public static readonly int HasDebugInfo = 1;
        public static readonly int Editor = 2;
        public static readonly int StringsTable = 4;
        public static readonly int Plain = 8;
        public static readonly int NoSections = 16;
        public static readonly int MultiChunk = 32;

        public static bool HasFlag(int flags, int flag)
        {
            return (flags & flag) != 0;
        }
    }
}
