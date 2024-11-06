using ExodusVFX.Utils;
using Newtonsoft.Json;
using Serilog;
using System.Numerics;
using System.Reflection;

namespace ExodusVFX.Format
{
    public class MetroReflectionReader
    {
        public static T Read<T>(byte[] buffer) where T : MetroHandleClass, new()
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(buffer));

            T handleObject = new T();

            handleObject.name = reader.ReadStringZ();
            handleObject.flags = reader.ReadByte();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                string propertyName = reader.ReadStringZ();
                string propertyType = reader.ReadStringZ();
                var value = MetroReflectionReader.ReadPropertyValue(reader, propertyType);
                FieldInfo field = typeof(T).GetField(propertyName);
                if(field == null)
                {
                    Log.Warning($"Can't find property {propertyName} in class {typeof(T).Name}");
                    continue;
                }
                field.SetValue(handleObject, value);
            }
            return handleObject;
        }

        private static object ReadPropertyValue(BinaryReader reader, string propertyType)
        {
            string[] genericTypes = propertyType.Split(", ");
            object value = null;
            switch (genericTypes[0])
            {
                case "u8":
                    value = reader.ReadByte();
                    break;
                case "u8_array":
                    value = ReadU8ArrayPropertyValue(reader);
                    break;
                case "u16":
                    value = reader.ReadUInt16();
                    break;
                case "u32":
                    value = reader.ReadUInt32();
                    break;
                case "stringz":
                    value = reader.ReadStringZ();
                    break;
                case "bool":
                    value = reader.ReadBoolean();
                    break;
                case "fp32":
                    value = reader.ReadSingle();
                    break;
                case "fp32_array":
                    value = ReadFloatArrayPropertyValue(reader);
                    break;
                case "vec4f":
                    value = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    break;
                case "color":
                    value = MetroReflectionReader.ReadPropertyValue(reader, genericTypes[1]);
                    break;
                case "choose": //unknown...?
                case "choose_array": //weird lol
                    string chooseName = reader.ReadStringZ();
                    string chooseType = reader.ReadStringZ();
                    value = MetroReflectionReader.ReadPropertyValue(reader, chooseType);
                    break;
                default:
                    throw new Exception($"Unhandled type {propertyType}");
            }
            return value;
        }

        private static float[] ReadFloatArrayPropertyValue(BinaryReader reader)
        {
            uint num = reader.ReadUInt32();
            var array = new float[num];
            for (int i = 0; i < num; i++) array[i] = reader.ReadSingle();
            return array;
        }

        private static byte[] ReadU8ArrayPropertyValue(BinaryReader reader)
        {
            uint num = reader.ReadUInt32();
            byte[] array = new byte[num];
            for (int i = 0; i < num; i++) array[i] = reader.ReadByte();
            return array;
        }
    }
}
