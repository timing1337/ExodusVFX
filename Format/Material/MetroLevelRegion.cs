using System;
using System.Numerics;
using System.Text;
using Cast;
using Cast.NET;
using Cast.NET.Nodes;
using ExodusVFX.Format.Mesh;
using Serilog;

namespace ExodusVFX.Format.Material
{
    public class MetroLevelRegion
    {
        public string name;
        public List<MetroMesh> meshes = new List<MetroMesh>();
        public MetroLevelRegion(string name)
        {
            this.name = name;
        }

        public void ExportToCast(string path)
        {
            Log.Information($"Exporting {this.name} to {path}");
            var root = new CastNode(CastNodeIdentifier.Root);
            root.AddNode(ExportToCastModel());
            CastWriter.Save(Path.Join(path, $"{this.name}.cast"), root);
        }

        public ModelNode ExportToCastModel()
        {
            var model = new ModelNode();
            model.AddString("n", this.name);

            for (var i = 0; i < this.meshes.Count; i++) {
                MeshNode mesh = new MeshNode();
                MetroMesh metroMesh = this.meshes[i];

                // Faces
                var faceIndices = mesh.AddArray<uint>("f", new(metroMesh.faces.Count * 3));
                var positions = mesh.AddArray<Vector3>("vp", new(metroMesh.vertices.Count));
                var normals = mesh.AddArray<Vector3>("vn", new(metroMesh.vertices.Count));
                var uv0 = mesh.AddArray<Vector2>("u0", new(metroMesh.vertices.Count));

                foreach(var vertice in metroMesh.vertices)
                {
                    positions.Add(vertice.pos);
                    normals.Add(new Vector3(vertice.normal.X, vertice.normal.Y, vertice.normal.Z));
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
            File.WriteAllText(Path.Join(path, $"{this.name}.obj"), $"# {this.name}\n{vPos.ToString()}{vUv.ToString()}{vNorm.ToString()} {f.ToString()}");
        }
    }
}
