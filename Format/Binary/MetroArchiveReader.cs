using ExodusVFX.Database;
using ExodusVFX.Utils;
using Serilog;

namespace ExodusVFX.Format.Binary
{
    public class MetroArchiveReader
    {
        public int flags;

        public string path;

        public string[]? stringTables;

        public Dictionary<uint, MetroArchive> archives = new Dictionary<uint, MetroArchive>();

        public MetroArchiveReader(string path, BinaryReader mainChunk, int flags)
        {
            this.path = path;
            //Preload chunks :D

            Log.Information($"Loading bin archives from {path}");

            if(!MetroFlags.HasFlag(flags, MetroFlags.NoSections))
            {
                while (mainChunk.BaseStream.Position < mainChunk.BaseStream.Length)
                {
                    uint idx = mainChunk.ReadUInt32();
                    uint size = mainChunk.ReadUInt32();
                    byte[] content = mainChunk.ReadBytes((int)size);
                    archives.Add(idx, new MetroArchive(this, content));
                    Log.Information($"Loaded archive {idx} with flags");
                }
            }

            this.flags = flags;
        }

        public void PopulateStringTables(int count)
        {
            this.stringTables = new string[count];
        }

        public MetroArchive GetArchive(string name)
        {
            return archives[CRC32.Calculate(name)];
        }
        public MetroArchive GetArchive(uint idx)
        {
            return archives[idx];
        }

        public static MetroArchiveReader LoadFromFile(MetroFile file)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(MetroDatabase.vfx.GetFileContent(file)));
            reader.ReadBytes(4); // Header
            var bitFlags = reader.ReadByte();
            reader.ReadBytes(8); // I have no idea lol
            MetroArchiveReader bin = new MetroArchiveReader(file.GetFullPath(), BinaryUtils.chunkOpenAtCurrentPosition(reader, CRC32.Calculate("arch_chunk_0")), bitFlags);
            
            if(MetroFlags.HasFlag(bitFlags, MetroFlags.StringsTable))
            {
                Log.Information("Loading string tables");
                var stringTable = BinaryUtils.chunkOpenAtCurrentPosition(reader, 2);
                int count = stringTable.ReadInt32();
                bin.PopulateStringTables(count);
                for(int i = 0; i < count; i++)
                {
                    bin.stringTables![i] = stringTable.ReadStringZ();
                }
            }
            return bin;
        }
    }
}
