using System.Numerics;

namespace ExodusVFX.Utils
{
    public class MathUtils
    {
        private const float div = 1.0f / 127.0f;
        private const float div255 = 1.0f / 255.0f;

        public static Vector3 InverseVector(Vector3 v)
        {
            return new Vector3(v.Z, v.Y, v.X);
        }
        public static Vector4 InverseVector(Vector4 v)
        {
            return new Vector4(v.Z, v.Y, v.X, v.W);
        }

        public static Vector4 DecodeNormal(uint n)
        {
            float x = ((n & 0x00FF0000) >> 16) * div - 1.0f;
            float y = ((n & 0x0000FF00) >> 8) * div - 1.0f;
            float z = ((n & 0x000000FF) >> 0) * div - 1.0f;
            float w = ((n & 0xFF000000) >> 24) * div255;
            return new Vector4(x, y, z, w);
        }

    }
}
