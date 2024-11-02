using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format.Mesh
{
    public class MetroLevelVertex
    {
        public const float uvDequant = 1.0f / 1024.0f;

        public Vector3 pos;
        public uint normal;
        public uint aux0;
        public uint aux1;
        public short[] uv0;
        public short[] uv1;
    }
}
