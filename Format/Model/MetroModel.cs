using Cast.NET;
using Cast.NET.Nodes;
using ExodusVFX.Database;
using ExodusVFX.Format.Mesh;
using ExodusVFX.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format.Model
{
    public class MetroModel
    {

        public string name;

        public ModelHeader header;

        public List<MetroMesh> meshes = new List<MetroMesh>();

        public MetroModel() { }

        public void ExportToCast(string path)
        {
            Log.Information($"Exporting {this.name} to {path}");
            var root = new CastNode(CastNodeIdentifier.Root);
            root.AddNode(ExportToCastModel());
            CastWriter.Save(Path.Join(path, $"{this.name}.cast"), root);
        }

        public ModelNode ExportToCastModel(string? name = null, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null)
        {
            var model = new ModelNode();
            model.AddString("n", name == null ? this.name : name);
            model.AddNode(new SkeletonNode());

            for (var i = 0; i < this.meshes.Count; i++)
            {
                MeshNode mesh = new MeshNode();
                MetroMesh metroMesh = this.meshes[i];

                // Faces
                var faceIndices = mesh.AddArray<uint>("f", new(metroMesh.faces.Length * 3));
                var positions = mesh.AddArray<Vector3>("vp", new(metroMesh.vertices.Length));
                var normals = mesh.AddArray<Vector3>("vn", new(metroMesh.vertices.Length));
                var uv0 = mesh.AddArray<Vector2>("u0", new(metroMesh.vertices.Length));

                foreach (var vertice in metroMesh.vertices)
                {
                    Vector3 newPos = vertice.pos;
                    Vector3 newNorm = new Vector3(vertice.normal.X, vertice.normal.Y, vertice.normal.Z);

                    if (scale.HasValue)
                    {
                        newPos = newPos * scale.Value;
                        newNorm = newNorm * new Vector3(1 / scale.Value.X, 1 / scale.Value.Y, 1 / scale.Value.Z);
                    }
                    if (rotation.HasValue)
                    {
                        newPos = Vector3.Transform(newPos, rotation.Value);
                        newNorm = Vector3.Transform(newNorm, rotation.Value);
                    }
                    if (position.HasValue) newPos = newPos + position.Value;

                    positions.Add(newPos);
                    normals.Add(newNorm);
                    uv0.Add(vertice.uv0);
                }

                foreach (var metroFace in metroMesh.faces)
                {
                    faceIndices.Add((uint)(metroFace.a));
                    faceIndices.Add((uint)(metroFace.b));
                    faceIndices.Add((uint)(metroFace.c));
                }

                model.AddNode(mesh);
            }
            return model;
        }

        public void ExportToOBJ(string path)
        {
            int lastIdx = 0;
            StringBuilder vPos = new StringBuilder();
            StringBuilder vUv = new StringBuilder();
            StringBuilder vNorm = new StringBuilder();
            StringBuilder f = new StringBuilder();
            foreach (var mesh in this.meshes)
            {
                if (mesh.vertices.Length > 0 && mesh.faces.Length > 0)
                {
                    foreach (var vertex in mesh.vertices)
                    {
                        vPos.Append($"v {vertex.pos.X} {vertex.pos.Y} {vertex.pos.Z}\n");
                        vUv.Append($"vt {vertex.uv0.X} {1.0f - vertex.uv0.Y}\n");
                        vNorm.Append($"vn {vertex.normal.X} {vertex.normal.Y} {vertex.normal.Z}\n");
                    }

                    foreach (var face in mesh.faces)
                    {
                        f.Append($"f {face.c + 1 + lastIdx}/{face.c + 1 + lastIdx}/{face.c + 1 + lastIdx} {face.b + 1 + lastIdx}/{face.b + 1 + lastIdx}/{face.b + 1 + lastIdx} {face.a + 1 + lastIdx}/{face.a + 1 + lastIdx}/{face.a + 1 + lastIdx}\n");
                    }

                    lastIdx += mesh.vertices.Length;
                }
            }
            File.WriteAllText(Path.Join(path, $"{this.name}.obj"), $"# {this.name}\n{vPos.ToString()}{vNorm.ToString()}{vUv.ToString()} {f.ToString()}");
        }
        public static MetroModel LoadFromFile(MetroFile file)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(MetroDatabase.vfx.GetFileContent(file)));

            MetroModel model = new MetroModel();
            model.name = file.name.Split(".")[0];

            // Read subchunks
            ModelHeader modelHeader = ModelHeader.ReadFromBinary(BinaryUtils.chunkOpenAtCurrentPosition(reader, 1));

            model.header = modelHeader;

            BinaryReader submeshesReader = BinaryUtils.chunkOpenAtCurrentPosition(reader, 9);

            uint expectedIdx = 0;
            while (submeshesReader.BaseStream.Position < submeshesReader.BaseStream.Length)
            {
                MetroMesh mesh = new MetroMesh();

                BinaryReader submeshReader = BinaryUtils.chunkOpenAtCurrentPosition(submeshesReader, expectedIdx);

                mesh.header = ModelHeader.ReadFromBinary(BinaryUtils.chunkOpenAtCurrentPosition(submeshReader, 1));

                BinaryReader materialsReader = BinaryUtils.chunkOpenAtCurrentPosition(submeshReader, 2);
                for (var i = 0; i < 4; i++) mesh.materialsInfo[i] = materialsReader.ReadStringZ();

                BinaryReader verticesData = BinaryUtils.chunkOpenAtCurrentPosition(submeshReader, 3);
                uint vertexType = verticesData.ReadUInt32();
                uint numVertices = verticesData.ReadUInt32();
                uint numShadowVertices = verticesData.ReadUInt16();

                mesh.vertices = new MetroVertex[numVertices];

                for (var i = 0; i < numVertices; i++)
                {
                    MetroVertex vertex = new MetroVertex();
                    var pos = new Vector3(verticesData.ReadSingle(), verticesData.ReadSingle(), verticesData.ReadSingle());
                    var normal = verticesData.ReadUInt32();
                    var aux0 = verticesData.ReadUInt32();
                    var aux1 = verticesData.ReadUInt32();
                    var uv0 = new Vector2(verticesData.ReadSingle(), verticesData.ReadSingle());

                    //workaround for making object stop facing x axis
                    vertex.pos = pos;
                    vertex.normal = MathUtils.DecodeNormal(normal);
                    vertex.uv0 = uv0;

                    mesh.vertices[i] = vertex;
                }

                //Defaulting to non-skinned vertices
                //Todo: Properly handle this
                BinaryReader indicesData = BinaryUtils.chunkOpenAtCurrentPosition(submeshReader, 4);
                uint numIndices = indicesData.ReadUInt32();
                uint numShadowIndices = indicesData.ReadUInt16();
                mesh.faces = new MetroFace[numIndices];

                for (var j = 0; j < numIndices; j++)
                {
                    MetroFace face = new MetroFace();
                    face.a = indicesData.ReadUInt16();
                    face.b = indicesData.ReadUInt16();
                    face.c = indicesData.ReadUInt16();
                    mesh.faces[j] = face;
                }
                model.meshes.Add(mesh);

                expectedIdx++;
            }
            return model;
        }
    }
}
