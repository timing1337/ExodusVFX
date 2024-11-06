using ExodusVFX.Format;
using ExodusVFX.Format.Map;
using ExodusVFX.Format.Material;
using ExodusVFX.Format.Model;
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

        public static Dictionary<string, MetroLevel> levels = new Dictionary<string, MetroLevel>();
        public static Dictionary<string, MetroModel> models = new Dictionary<string, MetroModel>();

        public static void loadFromFile(string filePath)
        {
            MetroDatabase.luaScripts.Clear();
            MetroDatabase.textureHandles.Clear();

            MetroDatabase.vfx = MetroVFX.Read(filePath);
            //LoadScripts();
            //LoadTexture();
        }

        private static void LoadScripts()
        {
            MetroDatabase.configBinCache = MetroDatabase.vfx.GetFileContent("content/config.bin");
            BinaryReader reader = new BinaryReader(new MemoryStream(MetroDatabase.configBinCache));
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint crc32 = reader.ReadUInt32();
                uint size = reader.ReadUInt32();
                Log.Information($"Loading script_crc32_0x{crc32.ToString("X")}.bin, length: {size}, offset 0x{reader.BaseStream.Position.ToString("X")}");
                MetroDatabase.luaScripts.Add(new MetroLuaScript(crc32, (int)size, (int)reader.BaseStream.Position));
                reader.BaseStream.Position += size;
            }
        }

        private static void LoadTexture()
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(MetroDatabase.vfx.GetFileContent("content/textures_handles_storage.bin")));
            reader.BaseStream.Seek(6, SeekOrigin.Begin); //Skip the first 6 headers bytes, aka AVER..
            uint entriesCount = reader.ReadUInt32();
            for (int i = 0; i < entriesCount; i++)
            {
                uint idx = reader.ReadUInt32();
                uint size = reader.ReadUInt32();
                MetroTextureHandle textureData = MetroReflectionReader.Read<MetroTextureHandle>(reader.ReadBytes((int)size));
                MetroDatabase.textureHandles.Add(textureData.name, textureData);
                Log.Information($"Loading texture data for {textureData.name}");
            }
        }
    }
}
