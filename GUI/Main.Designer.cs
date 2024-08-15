namespace ExodusVFX
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            filesHierarchy = new TreeView();
            toolbar = new ToolStrip();
            fileOptions = new ToolStripSplitButton();
            fileLoad = new ToolStripMenuItem();
            toolbar.SuspendLayout();
            SuspendLayout();
            // 
            // filesHierarchy
            // 
            filesHierarchy.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            filesHierarchy.Location = new Point(12, 30);
            filesHierarchy.Name = "filesHierarchy";
            filesHierarchy.Size = new Size(466, 589);
            filesHierarchy.TabIndex = 0;
            // 
            // toolbar
            // 
            toolbar.GripStyle = ToolStripGripStyle.Hidden;
            toolbar.ImageScalingSize = new Size(20, 20);
            toolbar.Items.AddRange(new ToolStripItem[] { fileOptions });
            toolbar.Location = new Point(0, 0);
            toolbar.Name = "toolbar";
            toolbar.Size = new Size(1413, 27);
            toolbar.TabIndex = 1;
            toolbar.Text = "toolbar";
            // 
            // fileOptions
            // 
            fileOptions.DisplayStyle = ToolStripItemDisplayStyle.Text;
            fileOptions.DropDownItems.AddRange(new ToolStripItem[] { fileLoad });
            fileOptions.Image = (Image)resources.GetObject("fileOptions.Image");
            fileOptions.ImageTransparentColor = Color.Magenta;
            fileOptions.Name = "fileOptions";
            fileOptions.Size = new Size(51, 24);
            fileOptions.Text = "File";
            // 
            // fileLoad
            // 
            fileLoad.Name = "fileLoad";
            fileLoad.Size = new Size(125, 26);
            fileLoad.Text = "Load";
            fileLoad.Click += fileLoadToolStripMenuItem_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1413, 631);
            Controls.Add(toolbar);
            Controls.Add(filesHierarchy);
            HelpButton = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Main";
            Text = "ExodusVFX";
            toolbar.ResumeLayout(false);
            toolbar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeView filesHierarchy;
        private ToolStrip toolbar;
        private ToolStripSplitButton fileOptions;
        private ToolStripMenuItem fileLoad;
    }
}
