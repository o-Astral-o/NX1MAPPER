using System.Collections.Generic;
using System.IO;

namespace C2M
{
    public class WraithNameIndex
    {
        public WraithNameIndex()
        {
             List<string> wniFiles = new List<string>() { "CoDConstants", "CoDSemantics"};
             foreach (var wniFile in wniFiles)
             {
                 string wniPath = Path.Combine("PackageIndex", wniFile + ".wni");
                 if (File.Exists(wniPath))
                     ParseWNI(wniPath);
             }
        }

        public Dictionary<ulong, string> assetNames = new Dictionary<ulong, string>();

        public void ParseWNI(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                // Create reader
                BinaryReader binReader = new BinaryReader(fs);

                // Verify magic ('WNI ')
                if (binReader.ReadUInt32() == 0x20494E57)
                {
                    // Skip over version (Should be 0x1)
                    binReader.BaseStream.Seek(2, SeekOrigin.Current);
                    // Read entry count
                    var entryCount = binReader.ReadUInt32();

                    // Read packed sizes
                    var packed = binReader.ReadUInt32();
                    var unpacked = binReader.ReadUInt32();

                    // Result size
                    // Read the compressed data
                    var compressedBuffer = binReader.ReadBytes((int)packed);
                    byte[] decompressed = new byte[unpacked];
                    if (K4os.Compression.LZ4.LZ4Codec.Decode(compressedBuffer, 0, (int)packed, decompressed, 0, (int)unpacked) > 0)
                    {
                        using (var chunkReader = new BinaryReader(new MemoryStream(decompressed)))
                        {
                            for (int i = 0; i < entryCount; i++)
                            {
                                var key = chunkReader.ReadUInt64();
                                var value = chunkReader.ReadNullTerminatedString().Replace("*", "").Split('.')[0];

                                if (!assetNames.ContainsKey(key))
                                    assetNames.Add(key, value);
                            }
                        }
                    }
                }
            }
        }

    }
}
