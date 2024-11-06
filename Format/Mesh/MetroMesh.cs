using ExodusVFX.Format.Map;
using ExodusVFX.Format.Model;
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

        public ModelHeader header;

        public MetroFace[] faces;
        public MetroVertex[] vertices;

        public string[] materialsInfo = new string[4];

        public GeomObjectInfo? info;
    }
}