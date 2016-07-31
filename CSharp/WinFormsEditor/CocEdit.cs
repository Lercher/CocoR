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
            loadSampleTxt();            
        }

        void loadSampleTxt() {
            string fn = @"..\test\sample.txt";
            string s = System.IO.File.ReadAllText(fn);
            textSource.Text = s;
            this.Text += " - " + fn;
            textSource.Select(0, 0);
        }
    }
}