using ExodusVFX.Format.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format.Mesh
{
    public class MetroMesh
    {
        public bool skinned;
        public float vscale;
        public Vector3 aa;
        public Vector3 bb;
        public uint type;
        public uint shaderId;
        public string material;
        public string geometrySettings;
        public List<MetroFace> faces = new List<MetroFace>();
        public List<MetroVertex> vertices = new List<MetroVertex>();
        public byte[] bonesRemap;

        public GeomObjectInfo info;
    }
}