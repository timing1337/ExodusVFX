using ExodusVFX.Database;
using ExodusVFX.Utils;
using Serilog;
using System.Numerics;
using ExodusVFX.Format.Mesh;
using System.Text;
using ExodusVFX.Format.Binary;
using Cast;
using System;
using Cast.NET;
using Cast.NET.Nodes;
using ExodusVFX.Format.Model;

namespace ExodusVFX.Format.Map
{
    public class MetroLevel
    {
        public bool isInitialized = false;

        public string name;
        public Dictionary<string, MetroSector> regions = new Dictionary<string, MetroSector>();

        public static readonly uint STARTUP_ARCHIVE = CRC32.Calculate("startup");
        public static readonly uint ENTITIES_PARAMS_ARCHIVE = CRC32.Calculate("entities_params");
        public static readonly uint ENTITIES_ARCHIVE = CRC32.Calculate("entities");

        public List<MetroEntity> entities = new List<MetroEntity>();

        public MetroFile binFile;

        public MetroLevel(string name, string path, MetroFile binFile)
        {
            this.name = name;
            this.binFile = binFile;
        }

        public void Initialize()
        {
            foreach (var region in this.regions.Values) region.Initialize();
            this.LoadEntities(this.binFile);
            isInitialized = true;
        }

        public void ExportToCast(string path)
        {
            if (!isInitialized) this.Initialize();

            Log.Information($"Exporting {this.name} to {path}");

            var root = new CastNode(CastNodeIdentifier.Root);

            var entities = this.entities;
            foreach(var region in this.regions.Values) {
                root.AddNode(region.ExportToCastModel());
                entities.AddRange(region.entities);
            }

            foreach (var entity in entities.DistinctBy(entity => entity.name)) root.AddNode(entity.ExportToCastModel());

            CastWriter.Save(path, root);
        }

        public static MetroLevel? LoadFromFile(MetroFile file)
        {
            string path = file.GetFullPath();
            string folderPath = path.Substring(0, path.LastIndexOf('/'));
            MetroFolder folder = MetroDatabase.vfx.GetFolder(folderPath);
            MetroLevel level = new MetroLevel(folder.name, folderPath, file);

            MetroFolder levelStaticFolder = folder.subFolders.Where(folder => folder.name == "static").First();

            level.LoadRegions(levelStaticFolder.files.Where(file => file.name == "level.lightmaps").First());
            return level;
        }

        private void LoadEntities(MetroFile file)
        {
            MetroArchiveReader archive = MetroArchiveReader.LoadFromFile(file);
            MetroArchive entitiesArchive = archive.GetArchive(ENTITIES_ARCHIVE);
            MetroArchive paramsArchive = archive.GetArchive(ENTITIES_PARAMS_ARCHIVE);

            ushort version = paramsArchive.reader.ReadUInt16();

            Log.Information($"Loading entities from {file.GetFullPath()} / version: {version}");

            MetroArchive[] entitiesRawData = entitiesArchive.ReadArray();

            for(var i = 0; i < entitiesRawData.Length; i++)
            {
                MetroArchive entityData = entitiesRawData[i];
                MetroEntity entity = MetroEntity.LoadFromArchiveChunk(entityData, version);
                //we only cares about static props, thats all
                if (entity.visual != "" && entity.name != "" && entity.visual.Contains("static"))
                {
                    var found = false;
                    foreach (var region in this.regions.Values)
                    {
                        if (MathUtils.IsInBox(entity.position, region.boundingBoxMin, region.boundingBoxMax))
                        {
                            region.entities.Add(entity);
                            found = true;
                        }
                    }
                    if (!found) this.entities.Add(entity);
                }
            }
        }

        private void LoadRegions(MetroFile file)
        {
            BinaryReader lightmapsReader = new BinaryReader(new MemoryStream(MetroDatabase.vfx.GetFileContent(file)));

            uint version = lightmapsReader.ReadUInt32();
            uint numRegion = lightmapsReader.ReadUInt32();

            Log.Information($"Loading level {this.name} with {numRegion} regions");

            for (var i = 0; i < numRegion; i++)
            {
                string regionName = lightmapsReader.ReadStringZ();
                MetroSector region = new MetroSector(this, regionName);
                this.regions.Add(regionName, region);
            }
        }
    }
}
