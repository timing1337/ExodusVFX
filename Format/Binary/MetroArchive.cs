using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format.Binary
{
    public class MetroArchive
    {
        public byte flags;
        public byte[] content;

        public MetroArchive(byte flags, byte[] content)
        {
            this.flags = flags;
            this.content = content;
        }
    }
}
