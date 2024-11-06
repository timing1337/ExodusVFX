using ExodusVFX.Database;
using ExodusVFX.Format;
using ExodusVFX.Format.Binary;
using ExodusVFX.Format.Map;
using ExodusVFX.Format.Model;
using Serilog;
using System.Windows.Forms;

namespace ExodusVFX
{
    public partial class Main : Form
    {
        private ImageList fileHierarchyImageList;
        private static ContextMenuStrip fileOptionCtxMenu = new ContextMenuStrip();
        private static ContextMenuStrip folderOptionCtxMenu = new ContextMenuStrip();
        private static ContextMenuStrip modelOptionCtxMenu = new ContextMenuStrip();
        private static ContextMenuStrip mapOptionCtxMenu = new ContextMenuStrip();
        private static ContextMenuStrip regionOptionCtxMenu = new ContextMenuStrip();

        public Main()
        {
            InitializeComponent();

            this.filesHierarchy.NodeMouseClick += fileHierarchy_MouseClick;

            ToolStripButton exportFolderAsRaw = new ToolStripButton("Export folder as raw data");
            exportFolderAsRaw.Click += FileOptionCtxMenu_Click;

            ToolStripButton exportAsModel = new ToolStripButton("Export model");
            exportAsModel.Click += ExportModelCtxMenu_Click;

            ToolStripButton exportMap = new ToolStripButton("Export map");
            exportMap.Click += ExportMapCtxMenu_Click;

            ToolStripButton exportRegion = new ToolStripButton("Export region");
            exportRegion.Click += ExportRegionCtxMenu_Click;

            fileOptionCtxMenu = new ContextMenuStrip();
            AddDefaultMember(fileOptionCtxMenu);

            folderOptionCtxMenu = new ContextMenuStrip();
            folderOptionCtxMenu.Items.Add(exportFolderAsRaw);

            modelOptionCtxMenu = new ContextMenuStrip();
            modelOptionCtxMenu.Items.Add(exportAsModel);
            AddDefaultMember(modelOptionCtxMenu);

            mapOptionCtxMenu = new ContextMenuStrip();
            mapOptionCtxMenu.Items.Add(exportMap);
            AddDefaultMember(mapOptionCtxMenu);

            regionOptionCtxMenu = new ContextMenuStrip();
            regionOptionCtxMenu.Items.Add(exportRegion);
            AddDefaultMember(regionOptionCtxMenu);
        }

        private void AddDefaultMember(ContextMenuStrip strip)
        {
            ToolStripButton exportAsRaw = new ToolStripButton("Export as raw data");
            exportAsRaw.Click += FileOptionCtxMenu_Click;
            strip.Items.Add(exportAsRaw);
        }
        private void ExportRegionCtxMenu_Click(object? sender, EventArgs e)
        {
            TreeNode node = this.filesHierarchy.SelectedNode;
            node.ContextMenuStrip.Close();

            Console.WriteLine(node.Parent.Parent.Name);

            MetroLevel level = MetroDatabase.levels[node.Parent.Parent.Text];
            MetroSector region = level.regions[node.Text];

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Save region";
            dialog.FileName = region.name + ".cast";
            dialog.ShowDialog();
            if (dialog.FileName != node.Text) region.ExportToCast(dialog.FileName);
        }

        private void ExportMapCtxMenu_Click(object? sender, EventArgs e)
        {
            TreeNode node = this.filesHierarchy.SelectedNode;
            node.ContextMenuStrip.Close();

            MetroFile file = MetroDatabase.vfx.GetFileFromPath(node.FullPath.Replace("\\", "/"));
            MetroLevel level = MetroLevel.LoadFromFile(file);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Save map";
            dialog.FileName = level.name + ".cast";
            dialog.ShowDialog();
            if (dialog.FileName != node.Text) level.ExportToCast(dialog.FileName);
        }

        private void ExportModelCtxMenu_Click(object? sender, EventArgs e)
        {
            TreeNode node = this.filesHierarchy.SelectedNode;
            node.ContextMenuStrip.Close();

            SaveFileDialog dialog = new SaveFileDialog();
            MetroFile file = MetroDatabase.vfx.GetFileFromPath(node.FullPath.Replace("\\", "/"));
            MetroModel model = MetroModel.LoadFromFile(file);
            dialog.Title = "Save model";
            dialog.FileName = model.name + ".cast";
            dialog.ShowDialog();
            if (dialog.FileName != node.Text) model.ExportToCast(dialog.FileName);
        }

        private void FileOptionCtxMenu_Click(object? sender, EventArgs e)
        {
            var node = this.filesHierarchy.SelectedNode;
            node.ContextMenuStrip.Close();
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Save raw data";
            dialog.FileName = node.Text;
            dialog.ShowDialog();
            if (dialog.FileName != node.Text)
            {
                File.WriteAllBytes(dialog.FileName, MetroDatabase.vfx.GetFileContent(node.FullPath.Replace("\\", "/")));
            }
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
                    subNode.ContextMenuStrip = folderOptionCtxMenu;
                    subNode.ContextMenuStrip.PerformLayout();
                    currentNode.Nodes.Add(subNode);
                }

                foreach (var file in folder.files.OrderBy(file => file.name))
                {
                    var treeNode = new TreeNode(file.name);
                    treeNode.ContextMenuStrip = fileOptionCtxMenu;
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
                subNode.ContextMenuStrip = folderOptionCtxMenu;
                subNode.ContextMenuStrip.PerformLayout();
                parentNode.Nodes.Add(subNode);
            }

            foreach (var file in folder.files.OrderBy(file => file.name))
            {
                var treeNode = new TreeNode(file.name);
                treeNode.ContextMenuStrip = fileOptionCtxMenu;
                if (file.name.EndsWith(".model")) treeNode.ContextMenuStrip = modelOptionCtxMenu;
                if (file.name == "level.bin")
                {
                    treeNode.ContextMenuStrip = mapOptionCtxMenu;
                    MetroLevel level = MetroLevel.LoadFromFile(file);
                    MetroDatabase.levels.Add(level.name, level);

                    foreach (var region in level.regions)
                    {
                        var subRegionNode = new TreeNode(region.Key);
                        subRegionNode.ContextMenuStrip = regionOptionCtxMenu;
                        subRegionNode.ContextMenuStrip.PerformLayout();
                        treeNode.Nodes.Add(subRegionNode);
                    }
                }
                treeNode.ContextMenuStrip.PerformLayout();
                parentNode.Nodes.Add(treeNode);
            }

            if(folder.name == "scripts")
            {
                foreach(var luaScript in MetroDatabase.luaScripts)
                {
                    var treeNode = new TreeNode($"script_crc32_{luaScript.crc32.ToString("X")}.bin");
                    treeNode.ContextMenuStrip = fileOptionCtxMenu;
                    treeNode.ContextMenuStrip.PerformLayout();
                    parentNode.Nodes.Add(treeNode);
                }
            }
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
                MetroDatabase.vfx = null;
                MetroDatabase.loadFromFile(path);

                //reload tree
                this.ReloadFileHierarchy();
            }
        }
    }
}
