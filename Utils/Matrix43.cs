using System.Numerics;

namespace ExodusVFX.Utils
{
    public class Matrix43
    {
        public Vector4 a;
        public Vector4 b;
        public Vector4 c;
        public Matrix43(Vector4 a, Vector4 b, Vector4 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        
        public static Matrix43 ReadFromBinary(BinaryReader reader)
        {
            Vector4 a = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector4 b = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector4 c = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            return new Matrix43(a, b, c);
        }
    }
}
