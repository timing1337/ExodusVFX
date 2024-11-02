using ExodusVFX.Database;
using ExodusVFX.Utils;
using Serilog;

namespace ExodusVFX.Format.Binary
{
    public class MetroArchiveReader
    {
        public string path;

        public string[]? stringTables;

        public Dictionary<uint, MetroArchive> archives = new Dictionary<uint, MetroArchive>();

        public MetroArchiveReader(string path, BinaryReader mainChunk)
        {
            this.path = path;
            //Preload chunks :D

            Log.Information($"Loading bin archives from {path}");

            while (mainChunk.BaseStream.Position < mainChunk.BaseStream.Length)
            {
                uint idx = mainChunk.ReadUInt32();
                uint size = mainChunk.ReadUInt32() - 1;
                byte flags = mainChunk.ReadByte();
                byte[] content = mainChunk.ReadBytes((int)size);
                archives.Add(idx, new MetroArchive(flags, content));
                Log.Information($"Loaded archive {idx} with flags {flags}");
            }
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
            reader.ReadBytes(4);
            var bitFlags = reader.ReadByte();
            reader.ReadBytes(8);
            MetroArchiveReader bin = new MetroArchiveReader(file.GetFullPath(), BinaryUtils.chunkOpenAtCurrentPosition(reader, CRC32.Calculate("arch_chunk_0")));
            
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
