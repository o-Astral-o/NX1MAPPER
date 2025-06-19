using Cast.NET;
using RedFox.Graphics3D.Skeletal;
using System.Collections.Concurrent;
using System.Text;

namespace RedFox.Graphics3D.Cast
{
    /// <summary>
    /// A class to handle translating data from Cast files.
    /// </summary>
    public sealed class CastTranslator : Graphics3DTranslator
    {
        /// <summary>
        /// SEAnim Magic
        /// </summary>
        public static readonly byte[] Magic = [0x53, 0x45, 0x41, 0x6E, 0x69, 0x6D];

        /// <inheritdoc/>
        public override string Name => "SEAnimTranslator";

        /// <inheritdoc/>
        public override string[] Extensions { get; } =
        [
            ".cast"
        ];

        /// <inheritdoc/>
        public override bool SupportsReading => false;

        /// <inheritdoc/>
        public override bool SupportsWriting => false;

        /// <inheritdoc/>
        public override void Read(Stream stream, string filePath, Graphics3DScene scene)
        {
        }

        /// <inheritdoc/>
        public override void Write(Stream stream, string filePath, Graphics3DScene scene)
        {
        }

        /// <inheritdoc/>
        public override bool IsValid(Graphics3DScene scene, Span<byte> startOfFile, Stream stream, string? filePath, string? ext)
        {
            return !string.IsNullOrWhiteSpace(ext) && Extensions.Contains(ext);
        }

        /// <summary>
        /// Reads a UTF8 string from the file
        /// </summary>
        internal static string ReadUTF8String(BinaryReader reader)
        {
            var output = new StringBuilder(32);

            while (true)
            {
                var c = reader.ReadByte();
                if (c == 0)
                    break;
                output.Append(Convert.ToChar(c));
            }

            return output.ToString();
        }
    }
}
