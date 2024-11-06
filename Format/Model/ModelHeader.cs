using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Format.Model
{
    public class ModelHeader
    {
        public byte version;
        public byte type;
        public ushort shaderId;
        public Vector3 bbMin;
        public Vector3 bbMax;
        public Vector4 bsphere;
        public uint checkSum;
        public float invLod;
        public uint flags;
        public float vscale;
        public float texelDensity;

        public static ModelHeader ReadFromBinary(BinaryReader reader)
        {
            ModelHeader modelHeader = new ModelHeader();
            modelHeader.version = reader.ReadByte();
            modelHeader.type = reader.ReadByte();
            modelHeader.shaderId = reader.ReadUInt16();
            modelHeader.bbMin = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            modelHeader.bbMax = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            modelHeader.bsphere = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            modelHeader.checkSum = reader.ReadUInt32();
            modelHeader.invLod = reader.ReadSingle();
            modelHeader.flags = reader.ReadUInt32();
            modelHeader.vscale = reader.ReadSingle();
            modelHeader.texelDensity = reader.ReadSingle();

            if (modelHeader.vscale <= 1.192092896e-07f) modelHeader.vscale = 1.0f;

            return modelHeader;
        }
    }
}