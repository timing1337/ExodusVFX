namespace ExodusVFX.Format
{
    public class MetroFile
    {
        public MetroFolder parent;

        public int packageIdx;
        public uint offset;
        public uint compressedSize;
        public uint size;
        public string name;

        private string fullPath;

        public MetroFile(int packageIdx, uint offset, uint compressedSize, uint size, string name)
        {
            this.packageIdx = packageIdx;
            this.offset = offset;
            this.compressedSize = compressedSize;
            this.size = size;
            this.name = name;
        }

        public string GetFullPath()
        {
            if(this.fullPath != null) return this.fullPath;
            var parentFolder = this.parent;
            var paths = new List<string>();
            paths.Add(this.name);
            while (parentFolder != null && parentFolder.name != "")
            {
                paths.Add(parentFolder.name);
                parentFolder = parentFolder.parent;
            }
            paths.Reverse();
            this.fullPath = string.Join("/", paths);
            return this.fullPath;
        }
    }
}
