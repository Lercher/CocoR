using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsEditor
{
    public partial class CocEdit : Form
    {
        public CocEdit()
        {
            InitializeComponent();
            this.loadValuesInto();
        }

        private void loadValuesInto()
        {
            TreeNode tnRoot = treeView1.Nodes.Add("root","/");
            treeView1.Nodes[
            treeView1.Nodes.IndexOfKey("root")].Nodes.Add("Node-1", "Node-1");
            TreeNode [] nodes = treeView1.Nodes.Find("Node-1", true);
            addChildTo(nodes[0]);
           

            treeView1.Nodes[
            treeView1.Nodes.IndexOfKey("root")].Nodes.Add("Node-2", "Node-2");
             nodes = treeView1.Nodes.Find("Node-2", true);
            addChildTo(nodes[0]);

            treeView1.Nodes[
            treeView1.Nodes.IndexOfKey("root")].Nodes.Add("Node-3", "Node-3");
            nodes = treeView1.Nodes.Find("Node-3", true);
            addChildTo(nodes[0]);

        }

        private void addChildTo(TreeNode tn)
        {
            for (int i = 1; i <= 10; i++)
                tn.Nodes.Add(tn.Name + ".Child-"+i, "Child-"+i);
        }

        private void rdoLeft_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            splitContainer1.Panel2Collapsed = false;
            
        }

        private void rdoRight_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = true;
            splitContainer1.Panel1Collapsed = false;

        }

        private void rdoDefault_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = false;
            splitContainer1.Panel2Collapsed = false;
            
        }

        private void rdoHorizantal_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer1.Orientation = Orientation.Horizontal;
        }

        private void rdoVertical_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer1.Orientation = Orientation.Vertical;
        }

        private void rdoFix3d_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer1.BorderStyle = BorderStyle.Fixed3D;
        }

        private void rdoFixSingle_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
        }

        private void rdoNone_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer1.BorderStyle = BorderStyle.None;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listView1.Items.Clear();
            for(int i =0;i<e.Node.Nodes.Count;i++) 
              listView1.Items.Add(e.Node.Nodes[i].Name );            
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown nm = (NumericUpDown)sender;
            splitContainer1.SplitterWidth =(int) nm.Value ;
        }

        private void splitContainer1_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            if(splitContainer1.Orientation == Orientation.Vertical )
             Cursor.Current = Cursors.NoMoveVert;
            else
             Cursor.Current = Cursors.NoMoveHoriz;   
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Cursor.Current = Cursors.Default;
        }


    }



}