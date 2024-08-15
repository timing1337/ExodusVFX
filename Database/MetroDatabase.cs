using ExodusVFX.Format;
using ExodusVFX.Format.Material;
using ExodusVFX.Format.Script;
using ExodusVFX.Vfx;
using Serilog;
using System.Runtime.Intrinsics.Arm;

namespace ExodusVFX.Database
{
    public class MetroDatabase
    {
        public static byte[] configBinCache;

        public static MetroVFX vfx;

        public static List<MetroLuaScript> luaScripts = new List<MetroLuaScript>();
        public static Dictionary<string, MetroTextureHandle> textureHandles = new Dictionary<string, MetroTextureHandle>();

        public static void loadFromFile(string filePath)
        {
            MetroDatabase.luaScripts.Clear();
            MetroDatabase.textureHandles.Clear();

            MetroDatabase.vfx = MetroVFX.Read(filePath);
            MetroDatabase.LoadScripts();
            MetroDatabase.LoadTexture();
        }

        private static void LoadScripts()
        {
            MetroDatabase.configBinCache = MetroDatabase.vfx.GetFileContent("content/config.bin");
            var reader = new BinaryReader(new MemoryStream(MetroDatabase.configBinCache));
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var crc32 = reader.ReadUInt32();
                var size = reader.ReadUInt32();
                Log.Information($"Loading script_crc32_0x{crc32.ToString("X")}.bin, length: {size}, offset 0x{reader.BaseStream.Position.ToString("X")}");
                MetroDatabase.luaScripts.Add(new MetroLuaScript(crc32, (int)size, (int)reader.BaseStream.Position));
                reader.BaseStream.Position += size;
            }
        }

        private static void LoadTexture()
        {
            var texturesHandleStorage = MetroDatabase.vfx.GetFileContent("content/textures_handles_storage.bin");
            var reader = new BinaryReader(new MemoryStream(texturesHandleStorage));
            reader.BaseStream.Seek(6, SeekOrigin.Begin); //Skip the first 6 headers bytes, aka AVER..
            var entriesCount = reader.ReadUInt32();
            for (int i = 0; i < entriesCount; i++)
            {
                var idx = reader.ReadUInt32();
                var size = reader.ReadUInt32();
                var textureData = MetroReflectionReader.Read<MetroTextureHandle>(reader.ReadBytes((int)size));
                MetroDatabase.textureHandles.Add(textureData.name, textureData);
                Log.Information($"Loading texture data for {textureData.name}");
            }
        }
    }
}
