using ExodusVFX.Format;
using ExodusVFX.Utils;
using Serilog;

namespace ExodusVFX.Vfx
{
    public class MetroVFX
    {
        public string filePath;
        public uint version;
        public bool isCompressed;
        public string revision;
        public uint packagesCount;
        public uint filesCount;

        public List<MetroPackage> packages = new List<MetroPackage>();
        public List<MetroFolder> folders = new List<MetroFolder>();

        public MetroVFX(uint version, bool isCompressed, string revision, uint packagesCount, uint filesCount, string filePath)
        {
            this.version = version;
            this.isCompressed = isCompressed;
            this.revision = revision;
            this.packagesCount = packagesCount;
            this.filesCount = filesCount;
            this.filePath = filePath;
        }

        public MetroFolder GetFolder(string path)
        {
            var filePaths = path.Split('/');
            var parentFolder = this.folders.Where(folder => folder.name == filePaths[0]).First();
            for (int i = 1; i < filePaths.Length; i++)
            {
                var subFolderName = filePaths.Skip(i).First();
                var subFolderSearch = parentFolder.subFolders.Where(folder => folder.name == subFolderName);
                parentFolder = subFolderSearch.First();
            }
            return parentFolder;
        }

        public byte[] GetFileContent(string path)
        {
            var lastIndex = path.LastIndexOf('/');
            var fileName = path.Substring(lastIndex + 1);
            var parentFolder = GetFolder(path.Substring(0, lastIndex));
            var fileSearch = parentFolder.files.Where(file => file.name == fileName);
            var file = fileSearch.First();
            return GetFileContent(file);
        }

        public byte[] GetFileContent(MetroFile file)
        {
            var package = this.packages[file.packageIdx];

            using (BinaryReader reader = new BinaryReader(new FileStream(Path.Join(this.filePath, package.name), FileMode.Open)))
            {
                reader.BaseStream.Seek(file.offset, SeekOrigin.Begin);
                var content = new byte[file.compressedSize];
                reader.Read(content, 0, (int)file.compressedSize);
                if (file.compressedSize != file.size) content = CompressionUtils.DecompressMetroBlock(content, file.size);
                return content;
            }
        }

        public static MetroVFX Read(string filePath)
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var binaryReader = new BinaryReader(fs);
            var version = binaryReader.ReadUInt32();
            var isCompressed = Convert.ToBoolean(binaryReader.ReadUInt32());
            var revision = binaryReader.ReadStringZ();
            var unk0 = binaryReader.ReadBytes(16); //according to metroex seems like its some guid shit, don't care though :D, marking it as unknown

            var packagesCount = binaryReader.ReadUInt32();
            var filesCount = binaryReader.ReadUInt32();
            var unk1 = binaryReader.ReadUInt32();

            var metroVfx = new MetroVFX(version, isCompressed, revision, packagesCount, filesCount, Path.GetDirectoryName(filePath));

            for (uint i = 0; i < packagesCount; i++)
            {
                var packageName = binaryReader.ReadStringZ();
                var levelsCount = binaryReader.ReadUInt32();
                var levelNames = new string[levelsCount];
                for(int j = 0; j < levelsCount; j++)
                {
                    levelNames[j] = binaryReader.ReadStringZ();
                }
                var chunkIdx = binaryReader.ReadUInt32();
                metroVfx.packages.Add(new MetroPackage(i, packageName, levelNames, chunkIdx));
                Log.Information($"Loading package {packageName}");
            }

            var files = new MetroFile[filesCount];
            var folders = new List<MetroFolder>(); //this is for caching, basically sorting stuff :D
            for (int fileIdx = 0; fileIdx < filesCount; fileIdx++)
            {
                string name;
                var flags = binaryReader.ReadUInt16();
                if ((flags & 8) == 0)
                {
                    var packageIdx = binaryReader.ReadUInt16();
                    var offset = binaryReader.ReadUInt32();
                    var sizeUncompressed = binaryReader.ReadUInt32();
                    var sizeCompressed = binaryReader.ReadUInt32();
                    name = binaryReader.ReadEncryptedString();
                    var metroFile = new MetroFile(packageIdx, offset, sizeCompressed, sizeUncompressed, name);
                    var folder = folders.Where(folder => fileIdx >= folder.firstFileIdx && fileIdx < (folder.firstFileIdx + folder.count)).First();
                    metroFile.parent = folder;
                    folder.files.Add(metroFile);
                }
                else
                {
                    var folderFilesCount = binaryReader.ReadUInt16();
                    var firstFileIdx = binaryReader.ReadUInt32();
                    name = binaryReader.ReadEncryptedString();
                    if (name == "") continue; //ignore
                    var metroFolder = new MetroFolder(name, folderFilesCount, firstFileIdx);
                    var parentFolder = folders.Where(folder => fileIdx >= folder.firstFileIdx && fileIdx < (folder.firstFileIdx + folder.count));
                    if (parentFolder.Any())
                    {
                        parentFolder.First().subFolders.Add(metroFolder);
                        metroFolder.parent = parentFolder.First();
                    }
                    else
                    {
                        metroVfx.folders.Add(metroFolder);
                    }
                    folders.Add(metroFolder);
                }
            }
            return metroVfx;
        }
    }
}
