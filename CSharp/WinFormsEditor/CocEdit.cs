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
            textLog.ScrollBars = ScrollBars.Vertical;
            loadSampleTxt();
            parse();
            textSource.TextChanged += sourceChanged;
            textSource.SelectionChanged += sourceSelectionChanged;
            listAutocomplete.DoubleClick += acDoubleClick; 
        }

        private void acDoubleClick(object sender, EventArgs e)
        {
            ListViewItem sel = listAutocomplete.SelectedItems[0];
            if (sel == null) return;
            string txt = sel.Text;
            string typ = sel.SubItems[1].Text;
            handle(txt, typ);
        }

        private void sourceChanged(object sender, EventArgs e) {
            parse();
            listAlternativesAtSelection();
        }

        private void sourceSelectionChanged(object sender, EventArgs e) {            
            listAlternativesAtSelection();
        }

        void handle(string txt, string typ)
        {
            switch(typ) {
                case "*decl":
                    string t = txt.Substring(1, txt.Length - 2);
                    textSource.Select(int.Parse(t), 0);
                    break;
                case "*tclass":
                    break;
                default:
                    textSource.SelectedText = txt;
                    break;
            }
            textSource.Focus();
        }

        void listAlternativesAtSelection() {
            int pos = textSource.SelectionStart;
            listAutocomplete.Items.Clear();
            System.Console.Write("pos {0,-6}", pos);
            if (parser == null) return;
            Alternative a = findAlternative(pos);
            if (a == null) return;

            string s = a.t.val;
            if (s.Length > 60) s = s.Substring(0, 60) + " ...";
            System.Console.WriteLine("token \"{0}\"", s);

            if (a.declaration != null)
                addAC(string.Format("({0})", a.declaration.charPos + 1), "*decl");

            for (int k = 0; k <= Parser.maxT; k++)
            {
                if (a.alt[k]) {
                    string name = Parser.tName[k];
                    if (a.st[k] == null) {
                        string t = name[0] == '"' ? "*keyword" : "*tclass";
                        addAC(name.Replace("\"", string.Empty), t);    
                    } else {
                        foreach(Token tok in a.st[k].items)
                            addAC(tok.val, a.st[k].name);
                    }
                }
            }

            listAutocomplete.Columns[0].Text = a.t.val;
            listAutocomplete.Columns[1].Text = describeParsed(a);
            foreach(ColumnHeader column in listAutocomplete.Columns)
            {
                column.Width = -2;
            }
        }

        string describeParsed(Alternative a) {
            string tname = Parser.tName[a.t.kind];
            if (a.declared != null) return string.Format("{0}:{1}", tname, a.declared);
            if (a.declares != null) return string.Format("{0}>{1}", tname, a.declares);
            return tname;
        }

        ListViewItem addAC(string s, string t) {
            ListViewItem i = new ListViewItem(new string[] {s, t});
            listAutocomplete.Items.Add(i);
            return i;
        }

        Alternative findAlternative(int pos) {
            int lastEnd = 0;
            foreach (Alternative a in parser.tokens)
            {
                if (lastEnd < pos) {
                    lastEnd = a.t.charPos + a.t.val.Length;
                    if (pos <= lastEnd)
                        return a;
                }
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
                Scanner scanner = new Scanner(s, true); // it's BOM free but UTF8
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