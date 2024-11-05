using ExodusVFX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format.Binary
{
    public class MetroArchive
    {
        public MetroArchiveReader mainArchive;

        public byte[] content;

        public BinaryReader reader;

        public MetroArchive(MetroArchiveReader reader, byte[] content)
        {
            this.mainArchive = reader;
            this.content = content;
            this.reader = new BinaryReader(new MemoryStream(content));
        }

        public string ReadString()
        {
            if(MetroFlags.HasFlag(this.mainArchive.flags, MetroFlags.StringsTable))
            {
                var idx = this.reader.ReadUInt32();
                return this.mainArchive.stringTables![idx] ?? "";
            }
            else {
                return this.reader.ReadStringZ();
            }
        }


        public MetroArchive[] ReadArray()
        {
            uint count = this.reader.ReadUInt32();
            MetroArchive[] arr = new MetroArchive[count];
            for (int i = 0; i < count; i++)
            {
                uint idx = this.reader.ReadUInt32();
                uint size = this.reader.ReadUInt32();
                arr[i] = new MetroArchive(this.mainArchive, this.reader.ReadBytes((int)size));
            }
            return arr;
        }
    }
}
