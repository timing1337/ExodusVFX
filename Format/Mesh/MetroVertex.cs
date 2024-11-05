using System.Numerics;

namespace ExodusVFX.Format.Mesh
{
    public class MetroVertex
    {
        public Vector3 pos;
        public byte[] bones; // 4
        public Vector4 normal;
        public byte[] weights; // 4 
        public Vector2 uv0;
        public Vector2 uv1;
    }
}
