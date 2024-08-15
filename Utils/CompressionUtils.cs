using System.Runtime.InteropServices;

namespace ExodusVFX.Utils
{
    public class CompressionUtils
    {
        [DllImport("liblz4.dll")]
        public static extern unsafe int LZ4_decompress_fast_withPrefix64k(byte* source, byte* destination, int originalSize);

        //Actual mental illness
        public static unsafe byte[] DecompressMetroBlock(byte[] block, uint actualSize)
        {
            var decompressedData = new byte[actualSize];
            var decompressedDataPointerOffset = 0;
            fixed (byte* blockP = block)
            {
                fixed (byte* decompressedDataP = decompressedData)
                {
                    var reader = new BinaryReader(new MemoryStream(block));
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var blockSize = reader.ReadUInt32();
                        var blockUncompressedSize = reader.ReadUInt32();
                        var result = LZ4_decompress_fast_withPrefix64k(blockP + reader.BaseStream.Position, decompressedDataP + decompressedDataPointerOffset, (int)blockUncompressedSize);
                        reader.BaseStream.Position += (int)blockSize - 8;
                        decompressedDataPointerOffset += (int)blockUncompressedSize;
                    }
                }
            }
            return decompressedData;
        }
    }
}
