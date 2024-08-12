using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format
{
    public class MetroPackage
    {
        public uint packageIdx;
        public string name;
        public string[] levels;
        public uint chunkIdx;

        public List<MetroFile> files = new List<MetroFile>();

        public MetroPackage(uint packageIdx, string name, string[] levels, uint chunkIdx)
        {
            this.packageIdx = packageIdx;
            this.name = name;
            this.levels = levels;
            this.chunkIdx = chunkIdx;
        }
    }
}
