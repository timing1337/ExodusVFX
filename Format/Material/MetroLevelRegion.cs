using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExodusVFX.Format.Mesh;

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

        public string ExportToOBJ()
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
            string obj = $"# {this.name}\n{vPos.ToString()}{vUv.ToString()}{vNorm.ToString()} {f.ToString()}";
            return obj;
        }
    }
}
