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
            textSource.AcceptsTab = true;
            textLog.ScrollBars = RichTextBoxScrollBars.Vertical;
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
        }

        private void sourceSelectionChanged(object sender, EventArgs e) {            
            listAlternativesAtSelection();
        }

        private void logSelectionChanged(object sender, EventArgs e) {
            try {
                int pos = textLog.SelectionStart;
                string text = textLog.Text;
                string t1 = text.Substring(0, pos);
                string t2 = text.Substring(pos);
                string[] a1 = t1.Split(new Char[] {'\n'});
                string[] a2 = t2.Split(new Char[] {'\n'}, 2);
                string s = a1[a1.Length - 1] + a2[0];
                System.Console.Write(s);  // -- line 29 col 5: EOF expected
                int lp = s.IndexOf("line ");
                int cp = s.IndexOf("col ");
                int cc = s.IndexOf(":");
                if (lp > 0 && cp > 0 && cc > 0) {
                    int line = int.Parse(s.Substring(lp + 5, cp - lp - 5)); line--;
                    int col = int.Parse(s.Substring(cp + 4, cc - cp - 4)); col--;
                    Console.WriteLine("({0},{1})", line, col);
                    textSource.SuspendLayout();
                    textSource.SelectAll();
                    textSource.SelectionColor = textSource.ForeColor;
                    int start = textSource.GetFirstCharIndexFromLine(line);
                    textSource.Select(start, textSource.Lines[line].Length);
                    textSource.SelectionColor = Color.DarkRed;
                    textSource.Select(start + col, 0);
                    textSource.ScrollToCaret();
                    textSource.ResumeLayout(true);
                }
                System.Console.WriteLine();
            } finally {}
        }

        void handle(string txt, string typ)
        {
            switch(typ) {
                case "*decl":
                case "*ref":
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
                addAC(string.Format("({0})", a.declaration.charPos), "*decl");

            for (int k = 0; k <= Parser.maxT; k++)
            {
                if (a.alt[k]) {
                    string name = Parser.tName[k];
                    if (a.st[k] == null) {
                        string t = name[0] == '"' ? "*keyword" : "*tclass";
                        if (name.StartsWith("\""))
                            name = name.Substring(1, name.Length - 2);
                        addAC(name, t);    
                    } else {
                        foreach(Token tok in a.st[k].items)
                            addAC(tok.val, a.st[k].name);
                    }
                }
            }

            if (a.declares != null) { // we declare something so we have references
                foreach(Token reft in findReferences(a.t))
                    addAC(string.Format("({0})", reft.charPos), "*ref");
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

        IEnumerable<Token> findReferences(Token t) {
            List<Token> found = new List<Token>();
            foreach (Alternative a in parser.tokens)
                if (a.declaration == t)
                    found.Add(a.t);
            return found;
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

        private string orig = null;
        void parse() {
            string txt = textSource.Text;
            if (txt == orig) return;
            orig = txt;
            byte[] b = System.Text.Encoding.UTF8.GetBytes(txt);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            textLog.SelectionChanged -= logSelectionChanged;
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
            textLog.SelectionChanged += logSelectionChanged;            
            listAlternativesAtSelection();
       }
    }
}