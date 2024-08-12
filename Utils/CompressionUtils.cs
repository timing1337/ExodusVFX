using System.Runtime.InteropServices;

namespace ExodusVFX.Utils
{
    public class CompressionUtils
    {
        [DllImport("msys-lz4-1.dll")]
        public static extern unsafe int LZ4_decompress_fast_withPrefix64k(byte* source, byte* destination, int originalSize);

        public static unsafe byte[] DecompressMetroBlock(byte[] block, uint actualSize)
        {
            var reader = new BinaryReader(new MemoryStream(block));
            var merged = new List<byte>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var blockSize = reader.ReadUInt32();
                var blockUncompressedSize = reader.ReadInt32();
                var compressedBlock = reader.ReadBytes((int)blockSize - 8);
                var decompressedBlock = new byte[blockUncompressedSize];
                fixed (byte* compressedBlockP = compressedBlock)
                {
                    fixed (byte* decompressedBlockP = decompressedBlock)
                    {
                        var result = LZ4_decompress_fast_withPrefix64k(compressedBlockP, decompressedBlockP, blockUncompressedSize);
                    }
                }
                merged.AddRange(decompressedBlock);
            }
            return merged.ToArray();
        }
    }
}
