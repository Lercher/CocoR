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
        Parser parser;

        public CocEdit()
        {
            InitializeComponent();
            textSource.WordWrap = false;
            textSource.ScrollBars = RichTextBoxScrollBars.Both;
            loadSampleTxt();
            parse();
            textSource.TextChanged += sourceChanged;
            textSource.SelectionChanged += sourceSelectionChanged;
        }

        private void sourceChanged(object sender, EventArgs e) {
            parse();
        }

        private void sourceSelectionChanged(object sender, EventArgs e) {
            int pos = textSource.SelectionStart;
            listAlternativesAt(pos);
        }

        void listAlternativesAt(int pos) {
            listAutocomplete.Items.Clear();
            System.Console.Write("pos {0,-5}", pos);
            if (parser == null) return;
            Alternative a = find(pos);
            if (a == null) return;
            System.Console.WriteLine("token \"{0}\"", a.t.val);
        }

        Alternative find(int pos) {
            foreach (Alternative a in parser.tokens)
            {
                if (a.t.charPos <= pos && pos <= a.t.charPos + a.t.val.Length)
                    return a;
            }
            return null;
        }


        void loadSampleTxt() {
            string fn = @"..\test\sample.txt";
            string s = System.IO.File.ReadAllText(fn);
            textSource.Text = s;
            this.Text += " - " + fn;
            textSource.Select(0, 0);
        }

        void parse() {
            string txt = textSource.Text;
            byte[] b = System.Text.Encoding.UTF8.GetBytes(txt);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            using (System.IO.StringWriter w = new System.IO.StringWriter(sb))
            using (System.IO.MemoryStream s = new System.IO.MemoryStream(b)) {
                Scanner scanner = new Scanner(s);
                parser = new Parser(scanner);
                parser.errors.errorStream = w;
                parser.Parse();
                w.WriteLine("\n{0:n0} error(s) detected", parser.errors.count);
            }
            textLog.Text = sb.ToString();
            textLog.Select(0, 0);            
        }
    }
}