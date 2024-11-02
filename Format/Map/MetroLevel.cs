using ExodusVFX.Database;
using ExodusVFX.Utils;
using Serilog;
using System.Numerics;
using ExodusVFX.Format.Mesh;
using System.Text;

namespace ExodusVFX.Format.Map
{
    public class MetroLevel
    {
        public string name;
        public Dictionary<string, MetroLevelRegion> regions = new Dictionary<string, MetroLevelRegion>();

        public static readonly uint STARTUP_ARCHIVE = CRC32.Calculate("startup");
        public static readonly uint ENTITIES_ARCHIVE = CRC32.Calculate("entities_params");

        public MetroLevel(string name, string path)
        {
            this.name = name;
        }

        public void ExportToOBJ(string regionName)
        {
            MetroLevelRegion region = regions[regionName];
            if(region == null)
            {
                Log.Error($"Region {region} not found");
                return;
            }
            int lastIdx = 0;
            StringBuilder vPos = new StringBuilder();
            StringBuilder vUv = new StringBuilder();
            StringBuilder vNorm = new StringBuilder();
            StringBuilder f = new StringBuilder();
            foreach (var mesh in region.meshes)
            {
                if (mesh.vertices.Count > 0 && mesh.faces.Count > 0)
                {
                    foreach (var vertex in mesh.vertices)
                    {
                        vPos.Append($"v {vertex.pos.X} {vertex.pos.Y} {vertex.pos.Z}\n");
                        vUv.Append($"vt {vertex.uv0.X} {1.0f - vertex.uv0.Y}\n");
                        vNorm.Append($"vn {vertex.normal.X} {vertex.normal.Y} {vertex.normal.Z}\n");
                    }

                    foreach (var face in mesh.faces)
                    {
                        f.Append($"f {face.a + 1 + lastIdx}/{face.a + 1 + lastIdx}/{face.a + 1 + lastIdx} {face.b + 1 + lastIdx}/{face.b + 1 + lastIdx}/{face.b + 1 + lastIdx} {face.c + 1 + lastIdx}/{face.c + 1 + lastIdx}/{face.c + 1 + lastIdx}\n");
                    }

                    lastIdx += mesh.vertices.Count;
                }
            }
            string obj = $"# {regionName}\n{vPos.ToString()}{vUv.ToString()}{vNorm.ToString()} {f.ToString()}";
            System.IO.File.WriteAllText($"{regionName}.obj", obj);
        }

        public static MetroLevel? LoadFromPath(MetroFile file)
        {
            string path = file.GetFullPath();
            string folderPath = path.Substring(0, path.LastIndexOf('/'));
            MetroFolder folder = MetroDatabase.vfx.GetFolder(folderPath);
            MetroLevel level = new MetroLevel(folder.name, folderPath);

            MetroFolder levelStaticFolder = folder.subFolders.Where(folder => folder.name == "static").First();

            MetroFile levelLightmaps = levelStaticFolder.files.Where(file => file.name == "level.lightmaps").First();
            BinaryReader lightmapsReader = new BinaryReader(new MemoryStream(MetroDatabase.vfx.GetFileContent(levelLightmaps)));

            uint version = lightmapsReader.ReadUInt32();
            uint numRegion = lightmapsReader.ReadUInt32();

            Log.Information($"Loading level {level.name} with {numRegion} regions");

            for (var i = 0; i < numRegion; i++)
            {
                string regionName = lightmapsReader.ReadStringZ();
                MetroLevelRegion region = new MetroLevelRegion(regionName);

                MetroFile descriptionFile = levelStaticFolder.files.Where(file => file.name == regionName).First();
                BinaryReader descriptionReader = new BinaryReader(new MemoryStream(MetroDatabase.vfx.GetFileContent(descriptionFile)));

                MetroFile geometryFile = levelStaticFolder.files.Where(file => file.name == $"{regionName}.geom_pc").First();
                BinaryReader geometryReader = new BinaryReader(new MemoryStream(MetroDatabase.vfx.GetFileContent(geometryFile)));

                ReadGeometryDescription(descriptionReader, region);
                ReadGeometryData(geometryReader, region);
                
                level.regions.Add(regionName, region);
                level.ExportToOBJ(regionName);
            }

            return level;
        }

        private static void ReadGeometryData(BinaryReader reader, MetroLevelRegion region)
        {
            reader.ReadBytes(24);
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint chunkType = reader.ReadUInt32();
                uint chunkSize = reader.ReadUInt32();
                BinaryReader chunkReader = new BinaryReader(new MemoryStream(reader.ReadBytes((int)chunkSize)));
                long baseLocation;
                switch (chunkType)
                {
                    case 0x00000009: // GC_Vertices
                        uint numVertices = chunkReader.ReadUInt32();
                        uint numSmallVertices = chunkReader.ReadUInt32();
                        baseLocation = chunkReader.BaseStream.Position;
                        foreach (var mesh in region.meshes)
                        {
                            GeomObjectInfo info = mesh.info;
                            //jump to the base location
                            chunkReader.BaseStream.Position += 32 * info.vbOffset;
                            for (var i = 0; i < info.numVertices; i++)
                            {
                                var pos = new Vector3(chunkReader.ReadSingle(), chunkReader.ReadSingle(), chunkReader.ReadSingle());
                                var normal = chunkReader.ReadUInt32();
                                var aux0 = chunkReader.ReadUInt32();
                                var aux1 = chunkReader.ReadUInt32();
                                var uv0 = new short[2] { chunkReader.ReadInt16(), chunkReader.ReadInt16() };
                                var uv1 = new short[2] { chunkReader.ReadInt16(), chunkReader.ReadInt16() };
                                MetroVertex vertex = new MetroVertex();
                                vertex.pos = MathUtils.InverseVector(pos);
                                vertex.normal = MathUtils.InverseVector(MathUtils.DecodeNormal(normal));
                                vertex.uv0 = new Vector2(uv0[0] * MetroLevelVertex.uvDequant, uv0[1] * MetroLevelVertex.uvDequant);
                                vertex.uv1 = new Vector2(uv1[0] * MetroLevelVertex.uvDequant, uv1[1] * MetroLevelVertex.uvDequant);
                                mesh.vertices.Add(vertex);
                            }
                            //reset the position
                            chunkReader.BaseStream.Position = baseLocation;
                        }
                        break;
                    case 0x0000000A: // GC_Indices
                        uint numIndicess = chunkReader.ReadUInt32();
                        uint numSmallIndicess = chunkReader.ReadUInt32();
                        baseLocation = chunkReader.BaseStream.Position;

                        foreach (var mesh in region.meshes)
                        {
                            GeomObjectInfo info = mesh.info;
                            //jump to the base location 
                            chunkReader.BaseStream.Position += 2 * info.ibOffset;
                            for (var j = 0; j < info.numIndices / 3; j++)
                            {
                                MetroFace face = new MetroFace();
                                face.a = chunkReader.ReadUInt16();
                                face.b = chunkReader.ReadUInt16();
                                face.c = chunkReader.ReadUInt16();
                                mesh.faces.Add(face);
                            }
                            //reset the position
                            chunkReader.BaseStream.Position = baseLocation;
                        }
                        break;
                    default:
                        Log.Warning($"ReadGeometryData // Unknown chunk type {chunkType}");
                        break;
                }
            }
        }
        
        private static void ReadGeometryDescription(BinaryReader reader, MetroLevelRegion region)
        {
            var header = BinaryUtils.chunkOpenAtCurrentPosition(reader, 1);
            var version = header.ReadUInt16();
            var isDraft = header.ReadUInt16();
            if(version != 19)
            {
                throw new Exception("Invalid sector version.");
            }

            var materialChunk = BinaryUtils.chunkOpenAtCurrentPosition(reader, 3);

            uint expectedIdx = 0;
            try{
                while (true)
                {
                    BinaryReader chunkReader = BinaryUtils.chunkOpenAtCurrentPosition(materialChunk, expectedIdx);

                    //ReadGeometryInfo
                    BinaryReader geometryInfo = BinaryUtils.chunkOpenAtCurrentPosition(chunkReader, 21);
                    GeomObjectInfo objectInfo = new GeomObjectInfo();
                    objectInfo.vertexType = geometryInfo.ReadUInt32();
                    objectInfo.vbOffset = geometryInfo.ReadUInt32();
                    objectInfo.numVertices = geometryInfo.ReadUInt32();
                    objectInfo.numShadowVertices = geometryInfo.ReadUInt32();
                    objectInfo.ibOffset = geometryInfo.ReadUInt32();
                    objectInfo.numIndices = geometryInfo.ReadUInt32();
                    objectInfo.numShadowIndices = geometryInfo.ReadUInt32();

                    MetroMesh mesh = new MetroMesh();
                    mesh.vscale = 1.0f;
                    mesh.info = objectInfo;
                    region.meshes.Add(mesh);

                    expectedIdx++;

                    BinaryReader headerData = BinaryUtils.chunkOpenAtCurrentPosition(chunkReader, 2);

                    mesh.material = headerData.ReadStringZ();
                    mesh.geometrySettings = headerData.ReadStringZ();
                    Console.WriteLine($"Material data: {mesh.material}, Geometry data: {mesh.geometrySettings}");

                    //AABB and stuff like that :fire:, i think idk, 64 bytes
                    BinaryReader sideInformationChunk = BinaryUtils.chunkOpenAtCurrentPosition(chunkReader, 1);

                }
            }catch(Exception ex) { }
        }
    }
}
