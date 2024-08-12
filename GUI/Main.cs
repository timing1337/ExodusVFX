using ExodusVFX.Database;
using ExodusVFX.Format;

namespace ExodusVFX
{
    public partial class Main : Form
    {
        private ImageList fileHierarchyImageList;
        public Main()
        {
            InitializeComponent();
        }

        private void ReloadFileHierarchy()
        {
            this.filesHierarchy.Nodes.Clear();

            foreach(var folder in MetroDatabase.vfx.folders)
            {
                var currentNode = new TreeNode(folder.name);

                foreach (var subFolder in folder.subFolders.OrderBy(subFolder => subFolder.name))
                {
                    var subNode = new TreeNode(subFolder.name);
                    this.HandleSubFolderTree(subFolder, subNode);
                    currentNode.Nodes.Add(subNode);
                }

                foreach (var file in folder.files.OrderBy(file => file.name))
                {
                    currentNode.Nodes.Add(new TreeNode(file.name));
                }

                this.filesHierarchy.Nodes.Add(currentNode);
                
            }
        }

        private void HandleSubFolderTree(MetroFolder folder, TreeNode parentNode)
        {
            foreach (var subFolder in folder.subFolders.OrderBy(subFolder => subFolder.name))
            {
                var subNode = new TreeNode(subFolder.name);
                this.HandleSubFolderTree(subFolder, subNode);
                parentNode.Nodes.Add(subNode);
            }

            foreach (var file in folder.files.OrderBy(file => file.name))
            {
                parentNode.Nodes.Add(new TreeNode(file.name));
            }
        }

        private void toolbar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void fileToolbar_Click(object sender, EventArgs e)
        {

        }
        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {

        }

        private void fileLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Metro Exodus containers (*.vfx)|*.vfx";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var path = openFileDialog1.FileName;
                MetroDatabase.loadFromFile(path);

                //reload tree
                this.ReloadFileHierarchy();
            }
        }
    }
}
