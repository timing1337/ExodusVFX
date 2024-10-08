﻿using ExodusVFX.Utils;
using Newtonsoft.Json;
using Serilog;
using System.Numerics;

namespace ExodusVFX.Format
{
    public class MetroReflectionReader
    {
        public static T Read<T>(byte[] buffer) where T : MetroHandleClass, new()
        {
            var reader = new BinaryReader(new MemoryStream(buffer));

            var handleObject = new T();

            handleObject.name = BinaryUtils.ReadString(reader);
            handleObject.flags = reader.ReadByte();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var propertyName = BinaryUtils.ReadString(reader);
                var propertyType = BinaryUtils.ReadString(reader);
                var value = MetroReflectionReader.ReadPropertyValue(reader, propertyType);
                var field = typeof(T).GetField(propertyName);
                if(field == null)
                {
                    Log.Warning($"Can't find property {propertyName} in class {typeof(T).Name}");
                    continue;
                }
                field.SetValue(handleObject, value);
            }
            var path = "D:/materials_data/" + handleObject.name + ".json";
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonConvert.SerializeObject(handleObject));

            return handleObject;
        }

        private static object ReadPropertyValue(BinaryReader reader, string propertyType)
        {
            var genericTypes = propertyType.Split(", ");
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
                    value = BinaryUtils.ReadString(reader);
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
                    var chooseName = BinaryUtils.ReadString(reader);
                    var chooseType = BinaryUtils.ReadString(reader);
                    value = MetroReflectionReader.ReadPropertyValue(reader, chooseType);
                    break;
                default:
                    throw new Exception($"Unhandled type {propertyType}");
            }
            return value;
        }

        private static float[] ReadFloatArrayPropertyValue(BinaryReader reader)
        {
            var num = reader.ReadUInt32();
            var array = new float[num];
            for (int i = 0; i < num; i++) array[i] = reader.ReadSingle();
            return array;
        }

        private static byte[] ReadU8ArrayPropertyValue(BinaryReader reader)
        {
            var num = reader.ReadUInt32();
            var array = new byte[num];
            for (int i = 0; i < num; i++) array[i] = reader.ReadByte();
            return array;
        }
    }
}
