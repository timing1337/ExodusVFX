﻿using ExodusVFX.Format.Binary;
using ExodusVFX.Utils;
using Serilog;
using System.Numerics;


namespace ExodusVFX.Format.Map
{
    public class MetroEntity
    {
        public uint classIdx;
        public uint staticDataKey;
        public string bone_att_id;
        public uint idx;
        public uint parentIdx;
        public Matrix43 att_offset;
        public bool att_root;

        public string name = "";
        public byte? oflags;
        public byte? sflags;
        public float? culling_distance;
        public Matrix unk;
        public string visual = "";
        public ushort? dao_val;
        public Vector4? render_aux_val;

        public MetroArchive[]? unk2;

        public bool? vs_active;
        public ushort? spatial_sector;
        public byte qsave_chunk;

        public MetroArchive[]? commons_vs;
        public MetroArchive[]? removed_vs;

        public MetroEntity(uint classIdx, uint staticDataKey, string bone_att_id, uint idx, uint parentIdx, Matrix43 att_offset, bool att_root)
        {
            this.classIdx = classIdx;
            this.staticDataKey = staticDataKey;
            this.bone_att_id = bone_att_id;
            this.idx = idx;
            this.parentIdx = parentIdx;
            this.att_offset = att_offset;
            this.att_root = att_root;
        }

        public static MetroEntity LoadFromArchiveChunk(MetroArchive archive, uint version)
        {
            BinaryReader reader = archive.reader;

            uint classIdx = reader.ReadUInt32();
            uint staticDataKey = reader.ReadUInt32();
            string bone_att_id = archive.ReadString();
            uint idx = reader.ReadUInt16();
            uint parentIdx = reader.ReadUInt16();
            Matrix43 att_offset = Matrix43.ReadFromBinary(reader);
            bool att_root = reader.ReadBoolean();

            MetroEntity entity = new MetroEntity(classIdx, staticDataKey, bone_att_id, idx, parentIdx, att_offset, att_root);
            //External data
            //TODO: Find a way to properly parse this shit
            try
            {
                LoadSubData(archive, entity);
            }
            catch(Exception ex) {}

            return entity;
        }
        
        private static void LoadSubData(MetroArchive archive, MetroEntity entity)
        {
            BinaryReader reader = archive.reader;

            entity.name = archive.ReadString();
            entity.oflags = reader.ReadByte();
            entity.sflags = reader.ReadByte();
            entity.culling_distance = reader.ReadSingle();
            entity.unk = Matrix.ReadFromBinary(reader);
            entity.visual = archive.ReadString();
            entity.dao_val = reader.ReadUInt16();
            entity.render_aux_val = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            reader.ReadUInt32();
            entity.unk2 = (new MetroArchive(archive.mainArchive, reader.ReadBytes((int)(reader.ReadUInt32())))).ReadArray();

            entity.vs_active = reader.ReadBoolean();
            entity.spatial_sector = reader.ReadUInt16();
            entity.qsave_chunk = reader.ReadByte();

            reader.ReadUInt32();
            entity.commons_vs = (new MetroArchive(archive.mainArchive, reader.ReadBytes((int)(reader.ReadUInt32())))).ReadArray();

            reader.ReadUInt32();
            entity.removed_vs = (new MetroArchive(archive.mainArchive, reader.ReadBytes((int)(reader.ReadUInt32())))).ReadArray();
        }
    }
}