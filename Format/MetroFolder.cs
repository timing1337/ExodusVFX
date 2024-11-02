namespace ExodusVFX.Format
{
    public class MetroFolder
    {
        public MetroFolder parent;

        public string name;
        public uint count;
        public uint firstFileIdx;
        public List<MetroFolder> subFolders = new List<MetroFolder>();
        public List<MetroFile> files = new List<MetroFile>();

        public MetroFolder(string name, uint count, uint firstFileIdx)
        {
            this.name = name;
            this.count = count;
            this.firstFileIdx = firstFileIdx;
        }
    }
}
