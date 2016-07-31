namespace WinFormsEditor
{
    partial class CocEdit
    {

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rdoFix3d = new System.Windows.Forms.RadioButton();
            this.rdoNone = new System.Windows.Forms.RadioButton();
            this.rdoFixSingle = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rdoVertical = new System.Windows.Forms.RadioButton();
            this.rdoHorizantal = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdoDefault = new System.Windows.Forms.RadioButton();
            this.rdoRight = new System.Windows.Forms.RadioButton();
            this.rdoLeft = new System.Windows.Forms.RadioButton();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Location = new System.Drawing.Point(13, 140);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            this.splitContainer1.Size = new System.Drawing.Size(582, 270);
            this.splitContainer1.SplitterDistance = 194;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.Text = "splitContainer1";
            this.splitContainer1.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitContainer1_SplitterMoving);
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(192, 268);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(382, 268);
            this.listView1.TabIndex = 0;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "NodeName";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Location = new System.Drawing.Point(13, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(582, 122);
            this.panel1.TabIndex = 1;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.numericUpDown1);
            this.groupBox4.Location = new System.Drawing.Point(341, 15);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(99, 91);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Splitter Width";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(6, 37);
            this.numericUpDown1.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(89, 20);
            this.numericUpDown1.TabIndex = 0;
            this.numericUpDown1.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rdoFix3d);
            this.groupBox3.Controls.Add(this.rdoNone);
            this.groupBox3.Controls.Add(this.rdoFixSingle);
            this.groupBox3.Location = new System.Drawing.Point(229, 14);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(106, 91);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Border Style";
            // 
            // rdoFix3d
            // 
            this.rdoFix3d.AutoSize = true;
            this.rdoFix3d.Checked = true;
            this.rdoFix3d.Location = new System.Drawing.Point(15, 23);
            this.rdoFix3d.Name = "rdoFix3d";
            this.rdoFix3d.Size = new System.Drawing.Size(48, 17);
            this.rdoFix3d.TabIndex = 5;
            this.rdoFix3d.Text = "Fix3&D";
            this.rdoFix3d.CheckedChanged += new System.EventHandler(this.rdoFix3d_CheckedChanged);
            // 
            // rdoNone
            // 
            this.rdoNone.AutoSize = true;
            this.rdoNone.Location = new System.Drawing.Point(15, 66);
            this.rdoNone.Name = "rdoNone";
            this.rdoNone.Size = new System.Drawing.Size(47, 17);
            this.rdoNone.TabIndex = 4;
            this.rdoNone.TabStop = false;
            this.rdoNone.Text = "&None";
            this.rdoNone.CheckedChanged += new System.EventHandler(this.rdoNone_CheckedChanged);
            // 
            // rdoFixSingle
            // 
            this.rdoFixSingle.AutoSize = true;
            this.rdoFixSingle.Location = new System.Drawing.Point(15, 45);
            this.rdoFixSingle.Name = "rdoFixSingle";
            this.rdoFixSingle.Size = new System.Drawing.Size(66, 17);
            this.rdoFixSingle.TabIndex = 3;
            this.rdoFixSingle.TabStop = false;
            this.rdoFixSingle.Text = "&Fix Single";
            this.rdoFixSingle.CheckedChanged += new System.EventHandler(this.rdoFixSingle_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rdoVertical);
            this.groupBox2.Controls.Add(this.rdoHorizantal);
            this.groupBox2.Location = new System.Drawing.Point(128, 14);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(95, 92);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Orientation";
            // 
            // rdoVertical
            // 
            this.rdoVertical.AutoSize = true;
            this.rdoVertical.Checked = true;
            this.rdoVertical.Location = new System.Drawing.Point(6, 56);
            this.rdoVertical.Name = "rdoVertical";
            this.rdoVertical.Size = new System.Drawing.Size(56, 17);
            this.rdoVertical.TabIndex = 1;
            this.rdoVertical.Text = "&Vertical";
            this.rdoVertical.CheckedChanged += new System.EventHandler(this.rdoVertical_CheckedChanged);
            // 
            // rdoHorizantal
            // 
            this.rdoHorizantal.AutoSize = true;
            this.rdoHorizantal.Location = new System.Drawing.Point(6, 27);
            this.rdoHorizantal.Name = "rdoHorizantal";
            this.rdoHorizantal.Size = new System.Drawing.Size(68, 17);
            this.rdoHorizantal.TabIndex = 0;
            this.rdoHorizantal.TabStop = false;
            this.rdoHorizantal.Text = "&Horizantal";
            this.rdoHorizantal.CheckedChanged += new System.EventHandler(this.rdoHorizantal_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoDefault);
            this.groupBox1.Controls.Add(this.rdoRight);
            this.groupBox1.Controls.Add(this.rdoLeft);
            this.groupBox1.Location = new System.Drawing.Point(15, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(107, 92);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PanelCollapse";
            // 
            // rdoDefault
            // 
            this.rdoDefault.AutoSize = true;
            this.rdoDefault.Checked = true;
            this.rdoDefault.Location = new System.Drawing.Point(19, 19);
            this.rdoDefault.Name = "rdoDefault";
            this.rdoDefault.Size = new System.Drawing.Size(55, 17);
            this.rdoDefault.TabIndex = 2;
            this.rdoDefault.Text = "&Default";
            this.rdoDefault.CheckedChanged += new System.EventHandler(this.rdoDefault_CheckedChanged);
            // 
            // rdoRight
            // 
            this.rdoRight.AutoSize = true;
            this.rdoRight.Location = new System.Drawing.Point(19, 67);
            this.rdoRight.Name = "rdoRight";
            this.rdoRight.Size = new System.Drawing.Size(76, 17);
            this.rdoRight.TabIndex = 1;
            this.rdoRight.TabStop = false;
            this.rdoRight.Text = "&Right Panel";
            this.rdoRight.CheckedChanged += new System.EventHandler(this.rdoRight_CheckedChanged);
            // 
            // rdoLeft
            // 
            this.rdoLeft.AutoSize = true;
            this.rdoLeft.Location = new System.Drawing.Point(19, 41);
            this.rdoLeft.Name = "rdoLeft";
            this.rdoLeft.Size = new System.Drawing.Size(69, 17);
            this.rdoLeft.TabIndex = 0;
            this.rdoLeft.TabStop = false;
            this.rdoLeft.Text = "&Left Panel";
            this.rdoLeft.CheckedChanged += new System.EventHandler(this.rdoLeft_CheckedChanged);
            // 
            // MySplitContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 422);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.Name = "MySplitContainer";
            this.Text = "SplitContainer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdoRight;
        private System.Windows.Forms.RadioButton rdoLeft;
        private System.Windows.Forms.RadioButton rdoDefault;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rdoHorizantal;
        private System.Windows.Forms.RadioButton rdoVertical;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton rdoFix3d;
        private System.Windows.Forms.RadioButton rdoNone;
        private System.Windows.Forms.RadioButton rdoFixSingle;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
    }
}

