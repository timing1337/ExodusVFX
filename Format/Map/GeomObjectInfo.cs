
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format.Map
{
    public struct GeomObjectInfo
    {
        public uint vertexType;
        public uint vbOffset;
        public uint numVertices;
        public uint numShadowVertices;
        public uint ibOffset;
        public uint numIndices;
        public uint numShadowIndices;
    }
}
