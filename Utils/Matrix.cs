using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Utils
{
    public class Matrix
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Vector3 d;

        public Matrix(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public static Matrix ReadFromBinary(BinaryReader reader)
        {
            Vector3 a = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 b = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 c = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 d = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            return new Matrix(a, b, c, d);
        }
        public void Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            Matrix4x4 res = new Matrix4x4(this.a.X, a.Y, a.Z, 0, a.X, b.Y, b.Z, 0, c.X, c.Y, c.Z, 0, d.X, d.Y, d.Z, 0);
            Matrix4x4.Decompose(res, out scale, out rotation, out translation);
        }
    }
}
