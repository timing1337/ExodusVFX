using ExodusVFX.Database;
using ExodusVFX.Format;
using System.Windows.Forms;

namespace ExodusVFX
{
    public partial class Main : Form
    {
        private ImageList fileHierarchyImageList;
        private ContextMenuStrip fileOptionCtxMenu;
        private ContextMenuStrip folderOptionCtxMenu;

        public Main()
        {
            InitializeComponent();

            this.filesHierarchy.NodeMouseClick += fileHierarchy_MouseClick;
            
            this.fileOptionCtxMenu = new ContextMenuStrip();
            this.fileOptionCtxMenu.Items.Add(new ToolStripButton("Export as raw data"));

            this.folderOptionCtxMenu = new ContextMenuStrip();
            this.folderOptionCtxMenu.Items.Add(new ToolStripButton("Export folder as raw data"));
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
                    subNode.ContextMenuStrip = this.folderOptionCtxMenu;
                    subNode.ContextMenuStrip.PerformLayout();
                    currentNode.Nodes.Add(subNode);
                }

                foreach (var file in folder.files.OrderBy(file => file.name))
                {
                    var treeNode = new TreeNode(file.name);
                    treeNode.ContextMenuStrip = this.fileOptionCtxMenu;
                    treeNode.ContextMenuStrip.PerformLayout();
                    currentNode.Nodes.Add(treeNode);
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
                subNode.ContextMenuStrip = this.folderOptionCtxMenu;
                subNode.ContextMenuStrip.PerformLayout();
                parentNode.Nodes.Add(subNode);
            }

            foreach (var file in folder.files.OrderBy(file => file.name))
            {
                var treeNode = new TreeNode(file.name);
                treeNode.ContextMenuStrip = this.fileOptionCtxMenu;
                treeNode.ContextMenuStrip.PerformLayout();
                parentNode.Nodes.Add(treeNode);
            }

            if(folder.name == "scripts")
            {
                foreach(var luaScript in MetroDatabase.luaScripts)
                {
                    var treeNode = new TreeNode($"script_crc32_{luaScript.crc32.ToString("X")}.bin");
                    treeNode.ContextMenuStrip = this.fileOptionCtxMenu;
                    treeNode.ContextMenuStrip.PerformLayout();
                    parentNode.Nodes.Add(treeNode);
                }
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

        private void fileHierarchy_MouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            this.filesHierarchy.SelectedNode = e.Node;
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
