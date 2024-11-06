using System;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Cast;
using Cast.NET;
using Cast.NET.Nodes;
using ExodusVFX.Database;
using ExodusVFX.Format.Mesh;
using ExodusVFX.Format.Model;
using ExodusVFX.Utils;
using Serilog;

namespace ExodusVFX.Format.Map
{
    public class MetroSector
    {
        public bool isInitialized = false;

        public MetroLevel level;

        public string name;
        public List<MetroMesh> meshes = new List<MetroMesh>();

        public Vector3 boundingBoxMin;
        public Vector3 boundingBoxMax;

        public List<MetroEntity> entities = new List<MetroEntity>();

        public MetroSector(MetroLevel level, string name)
        {
            this.level = level;
            this.name = name;
        }

        public void Initialize()
        {
            
            MetroFolder folder = MetroDatabase.vfx.GetFolder($"content/maps/m3/{level.name}/static");

            MetroFile descriptionFile = folder.files.Where(file => file.name == this.name).First();
            BinaryReader descriptionReader = new BinaryReader(new MemoryStream(MetroDatabase.vfx.GetFileContent(descriptionFile)));

            MetroFile geometryFile = folder.files.Where(file => file.name == $"{this.name}.geom_pc").First();
            BinaryReader geometryReader = new BinaryReader(new MemoryStream(MetroDatabase.vfx.GetFileContent(geometryFile)));

            ReadGeometryDescription(descriptionReader);
            ReadGeometryData(geometryReader);

            isInitialized = true;
        }

        private void ReadGeometryData(BinaryReader reader)
        {
            reader.ReadBytes(24);

            BinaryReader verticesData = BinaryUtils.chunkOpenAtCurrentPosition(reader, 9);
            uint numVertices = verticesData.ReadUInt32();
            uint numSmallVertices = verticesData.ReadUInt32();
            long baseLocation = verticesData.BaseStream.Position;

            foreach (var mesh in this.meshes)
            {
                GeomObjectInfo info = mesh.info!.Value;
                //jump to the base location
                verticesData.BaseStream.Position += 32 * info.vbOffset;
                for (var i = 0; i < info.numVertices; i++)
                {
                    var pos = new Vector3(verticesData.ReadSingle(), verticesData.ReadSingle(), verticesData.ReadSingle());
                    var normal = verticesData.ReadUInt32();
                    var aux0 = verticesData.ReadUInt32();
                    var aux1 = verticesData.ReadUInt32();
                    var uv0 = new short[2] { verticesData.ReadInt16(), verticesData.ReadInt16() };
                    var uv1 = new short[2] { verticesData.ReadInt16(), verticesData.ReadInt16() };
                    MetroVertex vertex = new MetroVertex();
                    vertex.pos = pos * mesh.header.vscale;
                    vertex.normal = MathUtils.DecodeNormal(normal);
                    vertex.uv0 = new Vector2(uv0[0] * MetroLevelVertex.uvDequant, uv0[1] * MetroLevelVertex.uvDequant);
                    vertex.uv1 = new Vector2(uv1[0] * MetroLevelVertex.uvDequant, uv1[1] * MetroLevelVertex.uvDequant);

                    mesh.vertices[i] = vertex;
                }
                //reset the position
                verticesData.BaseStream.Position = baseLocation;
            }

            BinaryReader indicesData = BinaryUtils.chunkOpenAtCurrentPosition(reader, 10);
            uint numIndicess = indicesData.ReadUInt32();
            uint numSmallIndicess = indicesData.ReadUInt32();
            baseLocation = indicesData.BaseStream.Position;

            foreach (var mesh in this.meshes)
            {
                GeomObjectInfo info = mesh.info!.Value;
                //jump to the base location 
                indicesData.BaseStream.Position += 2 * info.ibOffset;
                for (var j = 0; j < info.numIndices / 3; j++)
                {
                    MetroFace face = new MetroFace();
                    face.a = indicesData.ReadUInt16();
                    face.b = indicesData.ReadUInt16();
                    face.c = indicesData.ReadUInt16();
                    mesh.faces[j] = face;
                }
                //reset the position
                indicesData.BaseStream.Position = baseLocation;
            }

            var allPos = this.meshes.SelectMany(mesh => mesh.vertices).Select(x => x.pos);

            var allX = allPos.Select(x => x.X);
            var allY = allPos.Select(x => x.Y);
            var allZ = allPos.Select(x => x.Z);

            this.boundingBoxMin = new Vector3(allX.Min(), allY.Min(), allZ.Min());
            this.boundingBoxMax = new Vector3(allX.Max(), allY.Max(), allZ.Max());
        }

        private void ReadGeometryDescription(BinaryReader reader)
        {
            var header = BinaryUtils.chunkOpenAtCurrentPosition(reader, 1);
            var version = header.ReadUInt16();
            var isDraft = header.ReadUInt16();
            if (version != 19) throw new Exception("Invalid sector version.");

            var materialChunk = BinaryUtils.chunkOpenAtCurrentPosition(reader, 3);

            uint expectedIdx = 0;
            try
            {
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
                    mesh.info = objectInfo;
                    mesh.faces = new MetroFace[objectInfo.numIndices / 3];
                    mesh.vertices = new MetroVertex[objectInfo.numVertices];

                    BinaryReader materialData = BinaryUtils.chunkOpenAtCurrentPosition(chunkReader, 2);

                    mesh.materialsInfo.Append(materialData.ReadStringZ());
                    mesh.materialsInfo.Append(materialData.ReadStringZ());

                    mesh.header = ModelHeader.ReadFromBinary(BinaryUtils.chunkOpenAtCurrentPosition(chunkReader, 1));

                    this.meshes.Add(mesh);

                    expectedIdx++;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ExportToCast(string path)
        {
            if (!isInitialized) this.level.Initialize();
            Log.Information($"Exporting {name} to {path}");
            var root = new CastNode(CastNodeIdentifier.Root);
            root.AddNode(ExportToCastModel());
            this.entities.ForEach(entity => root.AddNode(entity.ExportToCastModel()));
            CastWriter.Save(path, root);
        }

        public ModelNode ExportToCastModel()
        {
            var model = new ModelNode();
            model.AddString("n", name);
            model.AddNode(new SkeletonNode());

            for (var i = 0; i < meshes.Count; i++)
            {
                MeshNode mesh = new MeshNode();
                MetroMesh metroMesh = meshes[i];

                // Faces
                var faceIndices = mesh.AddArray<uint>("f", new(metroMesh.faces.Length * 3));
                var positions = mesh.AddArray<Vector3>("vp", new(metroMesh.vertices.Length));
                var normals = mesh.AddArray<Vector3>("vn", new(metroMesh.vertices.Length));
                var uv0 = mesh.AddArray<Vector2>("u0", new(metroMesh.vertices.Length));

                foreach (var vertice in metroMesh.vertices)
                {
                    positions.Add(vertice.pos);
                    normals.Add(new Vector3(vertice.normal.X, vertice.normal.Y, vertice.normal.Z));
                    uv0.Add(vertice.uv0);
                }

                foreach (var metroFace in metroMesh.faces)
                {
                    faceIndices.Add(metroFace.a);
                    faceIndices.Add(metroFace.b);
                    faceIndices.Add(metroFace.c);
                }

                model.AddNode(mesh);
            }
            return model;
        }
    }
}
