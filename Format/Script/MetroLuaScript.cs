using ExodusVFX.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format.Script
{
    public class MetroLuaScript
    {
        public uint crc32;
        public int length;
        public int offset;

        public MetroLuaScript(uint crc32, int length, int offset)
        {
            this.crc32 = crc32;
            this.length = length;
            this.offset = offset;
        }

        public byte[] ReadScriptContent()
        {
            var reader = new BinaryReader(new MemoryStream(MetroDatabase.configBinCache));
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return reader.ReadBytes(length);
        }
    }
}
