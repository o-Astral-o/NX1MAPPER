using System.Runtime.InteropServices;
namespace C2M
{
    public class C2M_2
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 8)]
        public class C2MapHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public char[] Magic = new char[] { 'C', '2', 'M' };
            public byte Version = 2;
            public CoDVersion MapVersion;
            public string Name;
            public string SkyBoxInfo;
            public uint ObjectsCount;
            public ulong ObjectsOffset;
            public uint InstancesCount;
            public ulong InstanceOffset;
            public uint ImagesCount;
            public ulong ImageOffset;
            public uint MaterialsCount;
            public ulong MaterialsOffset;
            public uint LightsCount;
            public ulong LightsOffset;
            public ulong MapEntsOffset;
        };

        public static C2MapHeader Header = new C2MapHeader();

        public static void ExportC2M_2(string Path, CoDMap map)
        {

            // Create writer for our file
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Path)))
            {
                // Save space for header
                ushort baseHeaderSize = 69 + 8; // no mapName & skybox strings
                writer.BaseStream.Position += baseHeaderSize + map.SkyBoxInfo.Length + map.Name.Length;
                // Header setup
                Header.MapVersion = map.Version;
                Header.Name = map.Name;
                Header.SkyBoxInfo = map.SkyBoxInfo;
                Header.ObjectsCount = (uint)map.Objects.Count();
                Header.ObjectsOffset = (ulong)writer.BaseStream.Position;
                // Write objects
                foreach (var obj in map.Objects)
                {
                    // Write object
                    obj.WriteBin(writer);
                }
                // Header update
                Header.MaterialsCount = (uint)map.Materials.Count();
                Header.MaterialsOffset = (ulong)writer.BaseStream.Position;
                // Write materials
                foreach (var material in map.Materials.Values)
                {
                    // Write material header
                    writer.WriteString(material.Name);
                    writer.WriteString(material.TechSet);
                    writer.WriteString(material.SurfType);
                    writer.Write((byte)material.Blending);
                    writer.Write((byte)material.SortKey);
                    writer.Write((byte)material.Textures.Count);
                    writer.Write((byte)material.Constants.Count); 
                    foreach (var texture in material.Textures.Values)
                    {
                        writer.WriteString(texture.Name);
                        writer.WriteString(texture.Type);
                    }
                    foreach (var constant in material.Constants.Values)
                    {
                        writer.WriteMaterialConstant(constant);
                    }
                }
                // Header update
                Header.InstancesCount = (uint)map.ModelInstances.Count();
                Header.InstanceOffset = (ulong)writer.BaseStream.Position;
                // Write model instances
                foreach (var model_instance in map.ModelInstances)
                {
                    // Write name
                    writer.WriteString(model_instance.Name);
                    // Write position
                    writer.Write((float)model_instance.Position.X);
                    writer.Write((float)model_instance.Position.Y);
                    writer.Write((float)model_instance.Position.Z);
                    writer.Write((byte)model_instance.RotationMode);
                    // Write rotation
                    switch (model_instance.RotationMode)
                    {
                        default:
                        case 0:
                            writer.Write((float)model_instance.RotationQuat.X);
                            writer.Write((float)model_instance.RotationQuat.Y);
                            writer.Write((float)model_instance.RotationQuat.Z);
                            writer.Write((float)model_instance.RotationQuat.W);
                            break;
                        case 1:
                            writer.Write((float)model_instance.RotationDegrees.X);
                            writer.Write((float)model_instance.RotationDegrees.Y);
                            writer.Write((float)model_instance.RotationDegrees.Z);
                            break;
                    }

                    // Write scale
                    writer.Write((float)model_instance.ModelScale.X);
                    writer.Write((float)model_instance.ModelScale.Y);
                    writer.Write((float)model_instance.ModelScale.Z);
                    
                    // Write Type
                    writer.Write((byte)model_instance.Type);
                }

                Header.ImagesCount = 0;//(uint)map.Images.Count();
                Header.ImageOffset = (ulong)writer.BaseStream.Position;
                // foreach (var image in map.Images)
                // {
                //     string imageName = image.Key.Split(new[] { "::" }, StringSplitOptions.None)[1];
                //     writer.WriteString(imageName);
                // }
                // Header update
                Header.LightsCount = (uint)map.Lights.Count();
                Header.LightsOffset = (ulong)writer.BaseStream.Position;
                // Write lights
                foreach (var light in map.Lights)
                {
                    writer.Write((byte)light.Type);
                    writer.Write((float)light.Origin.X);
                    writer.Write((float)light.Origin.Y);
                    writer.Write((float)light.Origin.Z);
                    writer.Write((float)light.Direction.X);
                    writer.Write((float)light.Direction.Y);
                    writer.Write((float)light.Direction.Z);
                    writer.Write((float)light.Angles.X);
                    writer.Write((float)light.Angles.Y);
                    writer.Write((float)light.Angles.Z);
                    writer.Write((float)light.Color.X);
                    writer.Write((float)light.Color.Y);
                    writer.Write((float)light.Color.Z);
                    writer.Write((float)light.Color.W);
                    writer.Write((float)light.Radius);
                    writer.Write((float)light.CosHalfFovOuter);
                    writer.Write((float)light.CosHalfFovInner);
                    writer.Write((float)light.DAttenuation);
                }

                // Header update
                Header.MapEntsOffset = (ulong)writer.BaseStream.Position;
                writer.WriteString(map.MapEnts);

                // Go back to write header
                writer.BaseStream.Position = 0;
                writer.WriteC2MHeader(Header);
            }
        }
    }
}
