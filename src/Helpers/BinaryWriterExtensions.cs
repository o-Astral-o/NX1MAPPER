namespace C2M
{
    public static class BinaryReaderExtensions
    {
        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            string str = "";
            char ch;
            while (reader.BaseStream.Length > reader.BaseStream.Position && (int)(ch = reader.ReadChar()) != 0)
                str = str + ch;
            return str;
        }

    }

    public static class BinaryWriterExtensions
    {
        public static void WriteString(this BinaryWriter writer, string String)
        {
            writer.Write(String + '\x00');
        }

        public static void WriteMaterialConstant(this BinaryWriter writer, CoDConstant constant)
        {
            writer.WriteString(constant.Name);
            writer.Write((uint)constant.Hash);
            writer.Write((byte)constant.DataType);
            writer.Write((float)constant.Value.X);
            writer.Write((float)constant.Value.Y);
            writer.Write((float)constant.Value.Z);
            writer.Write((float)constant.Value.W);
        }

        public static void WriteC2MHeader(this BinaryWriter writer, C2M_2.C2MapHeader header)
        {
            writer.Write(header.Magic);
            writer.Write(header.Version);
            writer.Write((byte)header.MapVersion);
            writer.WriteString(header.Name);
            writer.WriteString(header.SkyBoxInfo);
            writer.Write(header.ObjectsCount);
            writer.Write(header.ObjectsOffset);
            writer.Write(header.InstancesCount);
            writer.Write(header.InstanceOffset);
            writer.Write(header.ImagesCount);
            writer.Write(header.ImageOffset);
            writer.Write(header.MaterialsCount);
            writer.Write(header.MaterialsOffset);
            writer.Write(header.LightsCount);
            writer.Write(header.LightsOffset);
            writer.Write(header.MapEntsOffset);
        }
    }


}
