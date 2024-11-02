using System.Text;

namespace ExodusVFX.Utils
{
    public static class BinaryUtils
    {
        public static string ReadStringZ(this BinaryReader input)
        {
            StringBuilder builder = new StringBuilder();
            while (input.BaseStream.Position < input.BaseStream.Length)
            {
                char c = input.ReadChar();
                if (c == '\0') break;
                builder.Append(c);
            }
            // Convert the buffer to a string
            return builder.ToString();
        }

        public static string ReadEncryptedString(this BinaryReader input)
        {
            var stringHeader = input.ReadUInt16();
            var stringLength = stringHeader & 0xFF;
            var xorMask = (stringHeader >> 8) & 0xFF;
            StringBuilder builder = new StringBuilder();
            for(int i = 1; i < stringLength; i++)
            {
                builder.Append(Convert.ToChar(input.ReadByte() ^ xorMask));
            }
            input.BaseStream.Position++; //ignore random
            return builder.ToString();
        }

        public static BinaryReader chunkOpenAtCurrentPosition(BinaryReader reader, uint expectedIdx = 0)
        {
            uint idx = reader.ReadUInt32();
            if (idx != expectedIdx)
            {
                throw new Exception($"Invalid chunk index. Got {idx}, expected {expectedIdx}");
            }
            uint size = reader.ReadUInt32();
            return new BinaryReader(new MemoryStream(reader.ReadBytes((int)size)));
        }
    }
}
