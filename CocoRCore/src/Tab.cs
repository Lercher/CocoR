using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{

    //=====================================================================
    // Tab
    //=====================================================================

    public class Tab
    {
        public Range semDeclPos;       // position of global semantic declarations
        public CharSet ignored;           // characters ignored by the scanner
        public bool[] ddt = new bool[10]; // debug and test switches
        public Symbol gramSy;             // root nonterminal; filled by ATG
        public Symbol eofSy;              // end of file symbol
        public Symbol noSym;              // used in case of an error
        public BitArray allSyncSets;      // union of all synchronisation sets
        public IDictionary<string, Symbol> literals;        // symbols that are used as literals
        public List<SymTab> symtabs = new List<SymTab>();

        public string srcName;            // name of the atg file (including path)
        public string srcDir;             // directory path of the atg file
        public string nsName;             // namespace for generated files
        public string frameDir;           // directory containing the frame files
        public string outDir;             // directory for generated files
        public bool checkEOF = true;      // should coco generate a check for EOF at
                                          //   the end of Parser.Parse():
        public bool emitLines;            // emit #line pragmas for semantic actions
                                          //   in the generated parser

        BitArray visited;                 // mark list for graph traversals
        Symbol curSy;                     // current symbol in computation of sets

        Parser parser;                    // other Coco objects
        TextWriter trace;
        ErrorsBase errors;

        public Tab(CocoRCore.CSharp.Parser parser)
        {
            this.parser = parser;
            trace = parser.trace;
            errors = parser.errors;
            eofSy = NewSym(NodeKind.t, "EOF", Position.Zero);
            dummyNode = NewNode(NodeKind.eps, null, 0);
            literals = new Dictionary<string, Symbol>();
        }

        //---------------------------------------------------------------------
        //  Symbol list management
        //---------------------------------------------------------------------

        public List<Symbol> terminals = new List<Symbol>();
        public List<Symbol> pragmas = new List<Symbol>();
        public List<Symbol> nonterminals = new List<Symbol>();

        string[] tKind = { "fixedToken", "classToken", "litToken", "classLitToken" };

        public Symbol NewSym(NodeKind typ, string name, Position pos)
        {
            if (name.Length == 2 && name[0] == '"')
            {
                parser.SemErr(81, "empty token not allowed"); name = "???";
            }
            Symbol sym = new Symbol(typ, name, pos);
            sym.definedAs = name;
            switch (typ)
            {
                case NodeKind.t:
                    sym.n = terminals.Count;
                    terminals.Add(sym);
                    break;
                case NodeKind.pr:
                    pragmas.Add(sym);
                    break;
                case NodeKind.nt:
                    sym.n = nonterminals.Count;
                    nonterminals.Add(sym);
                    break;
            }
            return sym;
        }

        public Symbol FindSym(string name)
        {
            foreach (Symbol s in terminals)
                if (s.name == name) return s;
            foreach (Symbol s in nonterminals)
                if (s.name == name) return s;
            return null;
        }

        public SymTab FindSymtab(string name)
        {
            foreach (SymTab st in symtabs)
                if (st.name == name) return st;
            return null;
        }

        int Num(Node p)
        {
            if (p == null) return 0; else return p.n;
        }

        void PrintSym(Symbol sym)
        {
            trace.Write("{0,3} {1,-14} {2,-4}", sym.n, Name(sym.name), sym.typ);
            if (sym.attrPos == null) trace.Write(" false "); else trace.Write(" true  ");
            if (sym.typ == NodeKind.nt)
            {
                trace.Write("{0,5}", Num(sym.graph));
                if (sym.deletable) trace.Write(" true  "); else trace.Write(" false ");
            }
            else
                trace.Write("            ");
            trace.WriteLine("{0,5} {1}", sym.pos, tKind[sym.tokenKind]);
        }

        public void PrintSymbolTable()
        {
            trace.WriteLine("Symbol Table:");
            trace.WriteLine("------------"); trace.WriteLine();
            trace.WriteLine(" nr name          typ  hasAt graph  del    line tokenKind");
            foreach (Symbol sym in terminals) PrintSym(sym);
            foreach (Symbol sym in pragmas) PrintSym(sym);
            foreach (Symbol sym in nonterminals) PrintSym(sym);
            trace.WriteLine();
            trace.WriteLine("Literal Tokens:");
            trace.WriteLine("--------------");
            foreach (var e in literals)
            {
                trace.WriteLine("_" + e.Value.name + " = " + e.Key + ".");
            }
            trace.WriteLine();
        }

        public void PrintSet(BitArray s, int indent)
        {
            int col, len;
            col = indent;
            foreach (Symbol sym in terminals)
            {
                if (s[sym.n])
                {
                    len = sym.name.Length;
                    if (col + len >= 80)
                    {
                        trace.WriteLine();
                        for (col = 1; col < indent; col++) trace.Write(" ");
                    }
                    trace.Write("{0} ", sym.name);
                    col += len + 1;
                }
            }
            if (col == indent) trace.Write("-- empty set --");
            trace.WriteLine();
        }

        //---------------------------------------------------------------------
        //  Syntax graph management
        //---------------------------------------------------------------------

        public List<Node> nodes = new List<Node>();
        Node dummyNode;

        public Node NewNode(NodeKind typ, Symbol sym, int line)
        {
            Node node = new Node(typ, sym, line);
            node.n = nodes.Count;
            nodes.Add(node);
            return node;
        }

        public Node NewNode(NodeKind typ, Node sub)
        {
            Node node = NewNode(typ, null, 0);
            node.sub = sub;
            return node;
        }

        public Node NewNode(NodeKind typ, int val, int line)
        {
            Node node = NewNode(typ, null, line);
            node.val = val;
            return node;
        }

        public void MakeFirstAlt(Graph g)
        {
            g.l = NewNode(NodeKind.alt, g.l); g.l.line = g.l.sub.line;
            g.r.up = true;
            g.l.next = g.r;
            g.r = g.l;
        }

        // The result will be in g1
        public void MakeAlternative(Graph g1, Graph g2)
        {
            g2.l = NewNode(NodeKind.alt, g2.l); g2.l.line = g2.l.sub.line;
            g2.l.up = true;
            g2.r.up = true;
            Node p = g1.l; while (p.down != null) p = p.down;
            p.down = g2.l;
            p = g1.r; while (p.next != null) p = p.next;
            // append alternative to g1 end list
            p.next = g2.l;
            // append g2 end list to g1 end list
            g2.l.next = g2.r;
        }

        // The result will be in g1
        public void MakeSequence(Graph g1, Graph g2)
        {
            Node p = g1.r.next; g1.r.next = g2.l; // link head node
            while (p != null)
            {  // link substructure
                Node q = p.next; p.next = g2.l;
                p = q;
            }
            g1.r = g2.r;
        }

        public void MakeIteration(Graph g)
        {
            g.l = NewNode(NodeKind.iter, g.l);
            g.r.up = true;
            Node p = g.r;
            g.r = g.l;
            while (p != null)
            {
                Node q = p.next; p.next = g.l;
                p = q;
            }
        }

        public void MakeOption(Graph g)
        {
            g.l = NewNode(NodeKind.opt, g.l);
            g.r.up = true;
            g.l.next = g.r;
            g.r = g.l;
        }

        public void Finish(Graph g)
        {
            Node p = g.r;
            while (p != null)
            {
                Node q = p.next; p.next = null;
                p = q;
            }
        }

        public void DeleteNodes()
        {
            nodes.Clear();
            dummyNode = NewNode(NodeKind.eps, null, 0);
        }

        public Graph StrToGraph(string str)
        {
            string s = Unstring(str);
            if (s.Length == 0) parser.SemErr(82, "empty token not allowed");
            Graph g = new Graph();
            g.r = dummyNode;
            for (int i = 0; i < s.Length; i++)
            {
                Node p = NewNode(NodeKind.chr, (int)s[i], 0);
                g.r.next = p; g.r = p;
            }
            g.l = dummyNode.next; dummyNode.next = null;
            return g;
        }

        public void SetContextTrans(Node p)
        { // set transition code in the graph rooted at p
            while (p != null)
            {
                if (p.typ == NodeKind.chr || p.typ == NodeKind.clas)
                {
                    p.code = NodeTransition.contextTrans;
                }
                else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                {
                    SetContextTrans(p.sub);
                }
                else if (p.typ == NodeKind.alt)
                {
                    SetContextTrans(p.sub); SetContextTrans(p.down);
                }
                if (p.up) break;
                p = p.next;
            }
        }

        //------------ graph deletability check -----------------

        public static bool DelGraph(Node p)
        {
            return p == null || DelNode(p) && DelGraph(p.next);
        }

        public static bool DelSubGraph(Node p)
        {
            return p == null || DelNode(p) && (p.up || DelSubGraph(p.next));
        }

        public static bool DelNode(Node p)
        {
            if (p.typ == NodeKind.nt)
                return p.sym.deletable;
            else if (p.typ == NodeKind.alt)
                return DelSubGraph(p.sub)
                    || p.down != null && DelSubGraph(p.down);
            else
                return p.typ == NodeKind.iter
                    || p.typ == NodeKind.opt
                    || p.typ == NodeKind.sem
                    || p.typ == NodeKind.eps
                    || p.typ == NodeKind.rslv
                    || p.typ == NodeKind.sync;
        }

        //----------------- graph printing ----------------------

        string Ptr(Node p, bool up)
        {
            string ptr = (p == null) ? "0" : p.n.ToString();
            return (up) ? ("-" + ptr) : ptr;
        }

        string Pos(Range pos) => string.Format("{0,5}", pos?.start?.ToString() ?? string.Empty);

        public string Name(string name)
        {
            return (name + "           ").Substring(0, 12);
            // found no simpler way to get the first 12 characters of the name
            // padded with blanks on the right
        }


        public void PrintNodes()
        {
            trace.WriteLine("Graph nodes:");
            trace.WriteLine("----------------------------------------------------");
            trace.WriteLine("   n type name          next  down   sub   pos  line");
            trace.WriteLine("                               val  code");
            trace.WriteLine("----------------------------------------------------");
            foreach (Node p in nodes)
            {
                trace.Write("{0,4} {1,-4} ", p.n, p.typ);
                if (p.sym != null)
                    trace.Write("{0,12} ", Name(p.sym.name));
                else if (p.typ == NodeKind.clas)
                {
                    CharClass c = (CharClass)classes[p.val];
                    trace.Write("{0,12} ", Name(c.name));
                }
                else trace.Write("             ");
                trace.Write("{0,5} ", Ptr(p.next, p.up));
                switch (p.typ)
                {
                    case NodeKind.t:
                    case NodeKind.nt:
                    case NodeKind.wt:
                        trace.Write("             {0,5}", Pos(p.pos)); break;
                    case NodeKind.chr:
                        trace.Write("{0,5} {1,5}       ", p.val, p.code); break;
                    case NodeKind.clas:
                        trace.Write("      {0,5}       ", p.code); break;
                    case NodeKind.alt:
                    case NodeKind.iter:
                    case NodeKind.opt:
                        trace.Write("{0,5} {1,5}       ", Ptr(p.down, false), Ptr(p.sub, false)); break;
                    case NodeKind.sem:
                        trace.Write("             {0,5}", Pos(p.pos)); break;
                    case NodeKind.eps:
                    case NodeKind.any:
                    case NodeKind.sync:
                        trace.Write("                  "); break;
                }
                trace.WriteLine("{0,5}", p.line);
            }
            trace.WriteLine();
        }


        //---------------------------------------------------------------------
        //  Character class management
        //---------------------------------------------------------------------

        public List<CharClass> classes = new List<CharClass>();
        public int dummyName = 'A';

        public CharClass NewCharClass(string name, CharSet s)
        {
            if (name == "#") name = "#" + (char)dummyName++;
            CharClass c = new CharClass(name, s);
            c.n = classes.Count;
            classes.Add(c);
            // System.Console.WriteLine("CharClass {0} = {1}", name, s);  // TODO - Trace Flag
            return c;
        }

        public CharClass FindCharClass(string name)
        {
            foreach (CharClass c in classes)
                if (c.name == name) return c;
            return null;
        }

        public CharClass FindCharClass(CharSet s)
        {
            foreach (CharClass c in classes)
                if (s.Equals(c.set)) return c;
            return null;
        }

        public CharSet CharClassSet(int i)
        {
            return ((CharClass)classes[i]).set;
        }

        //----------- character class printing

        string Ch(int ch)
        {
            if (ch < ' ' || ch >= 127 || ch == '\'' || ch == '\\') return ch.ToString();
            else return String.Format("'{0}'", (char)ch);
        }

        void WriteCharSet(CharSet s)
        {
            for (CharSet.Range r = s.head; r != null; r = r.next)
                if (r.from < r.to) { trace.Write(Ch(r.from) + ".." + Ch(r.to) + " "); }
                else { trace.Write(Ch(r.from) + " "); }
        }

        public void WriteCharClasses()
        {
            foreach (CharClass c in classes)
            {
                trace.Write("{0,-10}: ", c.name);
                WriteCharSet(c.set);
                trace.WriteLine();
            }
            trace.WriteLine();
        }


        //---------------------------------------------------------------------
        //  Symbol set computations
        //---------------------------------------------------------------------

        /* Computes the first set for the graph rooted at p */
        BitArray First0(Node p, BitArray mark)
        {
            BitArray fs = new BitArray(terminals.Count);
            while (p != null && !mark[p.n])
            {
                mark[p.n] = true;
                switch (p.typ)
                {
                    case NodeKind.nt:
                        if (p.sym.firstReady) fs.Or(p.sym.first);
                        else fs.Or(First0(p.sym.graph, mark));
                        break;
                    case NodeKind.t:
                    case NodeKind.wt:
                        fs[p.sym.n] = true;
                        break;
                    case NodeKind.any:
                        fs.Or(p.set);
                        break;
                    case NodeKind.alt:
                        fs.Or(First0(p.sub, mark));
                        fs.Or(First0(p.down, mark));
                        break;
                    case NodeKind.iter:
                    case NodeKind.opt:
                        fs.Or(First0(p.sub, mark));
                        break;
                }
                if (!DelNode(p))
                    break;
                p = p.next;
            }
            return fs;
        }

        public BitArray First(Node p)
        {
            BitArray fs = First0(p, new BitArray(nodes.Count));
            if (ddt[3])
            {
                trace.WriteLine();
                if (p != null) trace.WriteLine("First: node = {0}", p.n);
                else trace.WriteLine("First: node = null");
                PrintSet(fs, 0);
            }
            return fs;
        }

        void CompFirstSets()
        {
            foreach (Symbol sym in nonterminals)
            {
                sym.first = new BitArray(terminals.Count);
                sym.firstReady = false;
            }
            foreach (Symbol sym in nonterminals)
            {
                sym.first = First(sym.graph);
                sym.firstReady = true;
            }
        }

        void CompFollow(Node p)
        {
            while (p != null && !visited[p.n])
            {
                visited[p.n] = true;
                if (p.typ == NodeKind.nt)
                {
                    BitArray s = First(p.next);
                    p.sym.follow.Or(s);
                    if (DelGraph(p.next))
                        p.sym.nts[curSy.n] = true;
                }
                else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                {
                    CompFollow(p.sub);
                }
                else if (p.typ == NodeKind.alt)
                {
                    CompFollow(p.sub); CompFollow(p.down);
                }
                p = p.next;
            }
        }

        void Complete(Symbol sym)
        {
            if (!visited[sym.n])
            {
                visited[sym.n] = true;
                foreach (Symbol s in nonterminals)
                {
                    if (sym.nts[s.n])
                    {
                        Complete(s);
                        sym.follow.Or(s.follow);
                        if (sym == curSy) sym.nts[s.n] = false;
                    }
                }
            }
        }

        void CompFollowSets()
        {
            foreach (Symbol sym in nonterminals)
            {
                sym.follow = new BitArray(terminals.Count);
                sym.nts = new BitArray(nonterminals.Count);
            }
            gramSy.follow[eofSy.n] = true;
            visited = new BitArray(nodes.Count);
            foreach (Symbol sym in nonterminals)
            { // get direct successors of nonterminals
                curSy = sym;
                CompFollow(sym.graph);
            }
            foreach (Symbol sym in nonterminals)
            { // add indirect successors to followers
                visited = new BitArray(nonterminals.Count);
                curSy = sym;
                Complete(sym);
            }
        }

        Node LeadingAny(Node p)
        {
            if (p == null) return null;
            Node a = null;
            if (p.typ == NodeKind.any) a = p;
            else if (p.typ == NodeKind.alt)
            {
                a = LeadingAny(p.sub);
                if (a == null) a = LeadingAny(p.down);
            }
            else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter) a = LeadingAny(p.sub);
            if (a == null && DelNode(p) && !p.up) a = LeadingAny(p.next);
            return a;
        }

        void FindAS(Node p)
        { // find ANY sets
            Node a;
            while (p != null)
            {
                if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                {
                    FindAS(p.sub);
                    a = LeadingAny(p.sub);
                    if (a != null) a.set.Subtract(First(p.next));
                }
                else if (p.typ == NodeKind.alt)
                {
                    BitArray s1 = new BitArray(terminals.Count);
                    Node q = p;
                    while (q != null)
                    {
                        FindAS(q.sub);
                        a = LeadingAny(q.sub);
                        if (a != null)
                            a.set.Subtract(First(q.down).Or(s1));
                        else
                            s1.Or(First(q.sub));
                        q = q.down;
                    }
                }

                // Remove alternative terminals before ANY, in the following
                // examples a and b must be removed from the ANY set:
                // [a] ANY, or {a|b} ANY, or [a][b] ANY, or (a|) ANY, or
                // A = [a]. A ANY
                if (DelNode(p))
                {
                    a = LeadingAny(p.next);
                    if (a != null)
                    {
                        Node q = (p.typ == NodeKind.nt) ? p.sym.graph : p.sub;
                        a.set.Subtract(First(q));
                    }
                }

                if (p.up) break;
                p = p.next;
            }
        }

        void CompAnySets()
        {
            foreach (Symbol sym in nonterminals) FindAS(sym.graph);
        }

        public BitArray Expected(Node p, Symbol curSy)
        {
            BitArray s = First(p);
            if (DelGraph(p)) s.Or(curSy.follow);
            return s;
        }

        // does not look behind resolvers; only called during LL(1) test and in CheckRes
        public BitArray Expected0(Node p, Symbol curSy)
        {
            if (p.typ == NodeKind.rslv) return new BitArray(terminals.Count);
            else return Expected(p, curSy);
        }

        void CompSync(Node p)
        {
            while (p != null && !visited[p.n])
            {
                visited[p.n] = true;
                if (p.typ == NodeKind.sync)
                {
                    BitArray s = Expected(p.next, curSy);
                    s[eofSy.n] = true;
                    allSyncSets.Or(s);
                    p.set = s;
                }
                else if (p.typ == NodeKind.alt)
                {
                    CompSync(p.sub); CompSync(p.down);
                }
                else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                    CompSync(p.sub);
                p = p.next;
            }
        }

        void CompSyncSets()
        {
            allSyncSets = new BitArray(terminals.Count);
            allSyncSets[eofSy.n] = true;
            visited = new BitArray(nodes.Count);
            foreach (Symbol sym in nonterminals)
            {
                curSy = sym;
                CompSync(curSy.graph);
            }
        }

        public void SetupAnys()
        {
            foreach (Node p in nodes)
                if (p.typ == NodeKind.any)
                {
                    p.set = new BitArray(terminals.Count, true);
                    p.set[eofSy.n] = false;
                }
        }

        public void CompDeletableSymbols()
        {
            bool changed;
            do
            {
                changed = false;
                foreach (Symbol sym in nonterminals)
                    if (!sym.deletable && sym.graph != null && DelGraph(sym.graph))
                    {
                        sym.deletable = true; changed = true;
                    }
            } while (changed);
            foreach (Symbol sym in nonterminals)
                if (sym.deletable) 
                    errors.Warning(sym.pos, "NT " + sym.name + " deletable", 11);
        }

        public void RenumberPragmas()
        {
            int n = terminals.Count;
            foreach (Symbol sym in pragmas) sym.n = n++;
        }

        public void CompSymbolSets()
        {
            CompDeletableSymbols();
            CompFirstSets();
            CompAnySets();
            CompFollowSets();
            CompSyncSets();
            if (ddt[1])
            {
                trace.WriteLine();
                trace.WriteLine("First & follow symbols:");
                trace.WriteLine("----------------------"); trace.WriteLine();
                foreach (Symbol sym in nonterminals)
                {
                    trace.WriteLine(sym.name);
                    trace.Write("first:   "); PrintSet(sym.first, 10);
                    trace.Write("follow:  "); PrintSet(sym.follow, 10);
                    trace.WriteLine();
                }
            }
            if (ddt[4])
            {
                trace.WriteLine();
                trace.WriteLine("ANY and SYNC sets:");
                trace.WriteLine("-----------------");
                foreach (Node p in nodes)
                    if (p.typ == NodeKind.any || p.typ == NodeKind.sync)
                    {
                        trace.Write("{0,4} {1,-4}: ", p.n, p.typ);
                        PrintSet(p.set, 11);
                    }
            }
        }

        //---------------------------------------------------------------------
        //  String handling
        //---------------------------------------------------------------------

        char Hex2Char(string s)
        {
            int val = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if ('0' <= ch && ch <= '9') val = 16 * val + (ch - '0');
                else if ('a' <= ch && ch <= 'f') val = 16 * val + (10 + ch - 'a');
                else if ('A' <= ch && ch <= 'F') val = 16 * val + (10 + ch - 'A');
                else parser.SemErr(83, "bad escape sequence in string or character");
            }
            if (val > char.MaxValue) /* pdt */
                parser.SemErr(84, "bad escape sequence in string or character");
            return (char)val;
        }

        string Char2Hex(char ch)
        {
            StringWriter w = new StringWriter();
            w.Write("\\u{0:x4}", (int)ch);
            return w.ToString();
        }

        public string Unstring(string s)
        {
            if (s == null || s.Length < 2) return s;
            return Unescape(s.Substring(1, s.Length - 2));
        }

        public string Unescape(string s)
        {
            /* replaces escape sequences in s by their Unicode values. */
            StringBuilder buf = new StringBuilder();
            int i = 0;
            while (i < s.Length)
            {
                if (s[i] == '\\')
                {
                    switch (s[i + 1])
                    {
                        case '\\': buf.Append('\\'); i += 2; break;
                        case '\'': buf.Append('\''); i += 2; break;
                        case '\"': buf.Append('\"'); i += 2; break;
                        case 'r': buf.Append('\r'); i += 2; break;
                        case 'n': buf.Append('\n'); i += 2; break;
                        case 't': buf.Append('\t'); i += 2; break;
                        case '0': buf.Append('\0'); i += 2; break;
                        case 'a': buf.Append('\a'); i += 2; break;
                        case 'b': buf.Append('\b'); i += 2; break;
                        case 'f': buf.Append('\f'); i += 2; break;
                        case 'v': buf.Append('\v'); i += 2; break;
                        case 'u':
                        case 'x':
                            if (i + 6 <= s.Length)
                            {
                                buf.Append(Hex2Char(s.Substring(i + 2, 4))); i += 6; break;
                            }
                            else
                            {
                                parser.SemErr(85, "bad escape sequence in string or character"); i = s.Length; break;
                            }
                        default: parser.SemErr(86, "bad escape sequence in string or character"); i += 2; break;
                    }
                }
                else
                {
                    buf.Append(s[i]);
                    i++;
                }
            }
            return buf.ToString();
        }

        public string Quoted(string s)
        {
            if (s == null) return "null";
            return string.Concat("\"", Escape(s), "\"");
        }

        public string Escape(string s)
        {
            StringBuilder buf = new StringBuilder();
            foreach (char ch in s)
            {
                switch (ch)
                {
                    case '\\': buf.Append("\\\\"); break;
                    case '\'': buf.Append("\\'"); break;
                    case '\"': buf.Append("\\\""); break;
                    case '\t': buf.Append("\\t"); break;
                    case '\r': buf.Append("\\r"); break;
                    case '\n': buf.Append("\\n"); break;
                    default:
                        if (ch < ' ' || ch > '\u007f') buf.Append(Char2Hex(ch));
                        else buf.Append(ch);
                        break;
                }
            }
            return buf.ToString();
        }

        //---------------------------------------------------------------------
        //  Grammar checks
        //---------------------------------------------------------------------

        public bool GrammarOk()
        {
            bool ok = NtsComplete()
                && AllNtReached()
                && NoCircularProductions()
                && AllNtToTerm();
            if (ok) { CheckResolvers(); CheckLL1(); }
            return ok;
        }

        //--------------- check for circular productions ----------------------

        class CNode
        {   // node of list for finding circular productions
            public Symbol left, right;

            public CNode(Symbol l, Symbol r)
            {
                left = l; right = r;
            }
        }

        void GetSingles(Node p, List<Symbol> singles)
        {
            if (p == null) return;  // end of graph
            if (p.typ == NodeKind.nt)
            {
                if (p.up || DelGraph(p.next)) singles.Add(p.sym);
            }
            else if (p.typ == NodeKind.alt || p.typ == NodeKind.iter || p.typ == NodeKind.opt)
            {
                if (p.up || DelGraph(p.next))
                {
                    GetSingles(p.sub, singles);
                    if (p.typ == NodeKind.alt) GetSingles(p.down, singles);
                }
            }
            if (!p.up && DelNode(p)) GetSingles(p.next, singles);
        }

        public bool NoCircularProductions()
        {
            bool ok, changed, onLeftSide, onRightSide;
            var list = new List<CNode>();
            foreach (Symbol sym in nonterminals)
            {
                var singles = new List<Symbol>();
                GetSingles(sym.graph, singles); // get nonterminals s such that sym-->s
                foreach (Symbol s in singles) 
                    list.Add(new CNode(sym, s));
            }
            do
            {
                changed = false;
                for (int i = 0; i < list.Count; i++)
                {
                    CNode n = list[i];
                    onLeftSide = false; onRightSide = false;
                    foreach (CNode m in list)
                    {
                        if (n.left == m.right) onRightSide = true;
                        if (n.right == m.left) onLeftSide = true;
                    }
                    if (!onLeftSide || !onRightSide)
                    {
                        list.Remove(n); 
                        i--; 
                        changed = true;
                    }
                }
            } while (changed);
            ok = true;
            foreach (CNode n in list)
            {
                ok = false;
                errors.SemErr(n.left.pos, n.left.name + " --> " + n.right.name + n.right.pos.ToString(), 87);
            }
            return ok;
        }

        //--------------- check for LL(1) errors ----------------------

        void LL1Error(int cond, Symbol sym)
        {
            string s = "LL1 warning in " + curSy.name + ": ";
            if (sym != null) s += sym.name + sym.pos.ToString() + " is ";
            switch (cond)
            {
                case 1: s += "start of several alternatives"; break;
                case 2: s += "start & successor of deletable structure"; break;
                case 3: s += "an ANY node that matches no symbol"; break;
                case 4: s += "contents of [...] or {...} must not be deletable"; break;
            }
            errors.Warning(curSy.pos, s, 20 + cond);
        }

        void CheckOverlap(BitArray s1, BitArray s2, int cond)
        {
            foreach (Symbol sym in terminals)
            {
                if (s1[sym.n] && s2[sym.n]) LL1Error(cond, sym);
            }
        }

        void CheckAlts(Node p)
        {
            BitArray s1, s2;
            while (p != null)
            {
                if (p.typ == NodeKind.alt)
                {
                    Node q = p;
                    s1 = new BitArray(terminals.Count);
                    while (q != null)
                    { // for all alternatives
                        s2 = Expected0(q.sub, curSy);
                        CheckOverlap(s1, s2, 1);
                        s1.Or(s2);
                        CheckAlts(q.sub);
                        q = q.down;
                    }
                }
                else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                {
                    if (DelSubGraph(p.sub)) LL1Error(4, null); // e.g. [[...]]
                    else
                    {
                        s1 = Expected0(p.sub, curSy);
                        s2 = Expected(p.next, curSy);
                        CheckOverlap(s1, s2, 2);
                    }
                    CheckAlts(p.sub);
                }
                else if (p.typ == NodeKind.any)
                {
                    if (!p.set.Any()) LL1Error(3, null);
                    // e.g. {ANY} ANY or [ANY] ANY or ( ANY | ANY )
                }
                if (p.up) break;
                p = p.next;
            }
        }

        public void CheckLL1()
        {
            foreach (Symbol sym in nonterminals)
            {
                curSy = sym;
                CheckAlts(curSy.graph);
            }
        }

        //------------- check if resolvers are legal  --------------------

        void ResErr(Node p, string msg)
        {            
            errors.Warning(p.pos.start, msg, 31);
        }

        void CheckRes(Node p, bool rslvAllowed)
        {
            while (p != null)
            {
                switch (p.typ)
                {
                    case NodeKind.alt:
                        BitArray expected = new BitArray(terminals.Count);
                        for (Node q = p; q != null; q = q.down)
                            expected.Or(Expected0(q.sub, curSy));
                        BitArray soFar = new BitArray(terminals.Count);
                        for (Node q = p; q != null; q = q.down)
                        {
                            if (q.sub.typ == NodeKind.rslv)
                            {
                                BitArray fs = Expected(q.sub.next, curSy);
                                if (fs.Intersects(soFar))
                                    ResErr(q.sub, "Warning: Resolver will never be evaluated. Place it at previous conflicting alternative.");
                                if (!fs.Intersects(expected))
                                    ResErr(q.sub, "Warning: Misplaced resolver: no LL(1) conflict.");
                            }
                            else soFar.Or(Expected(q.sub, curSy));
                            CheckRes(q.sub, true);
                        }
                        break;
                    case NodeKind.iter:
                    case NodeKind.opt:
                        if (p.sub.typ == NodeKind.rslv)
                        {
                            BitArray fs = First(p.sub.next);
                            BitArray fsNext = Expected(p.next, curSy);
                            if (!fs.Intersects(fsNext))
                                ResErr(p.sub, "Warning: Misplaced resolver: no LL(1) conflict.");
                        }
                        CheckRes(p.sub, true);
                        break;
                    case NodeKind.rslv:
                        if (!rslvAllowed)
                            ResErr(p, "Warning: Misplaced resolver: no alternative.");
                        break;
                }
                if (p.up) break;
                p = p.next;
                rslvAllowed = false;
            }
        }

        public void CheckResolvers()
        {
            foreach (Symbol sym in nonterminals)
            {
                curSy = sym;
                CheckRes(curSy.graph, false);
            }
        }

        //------------- check if every nts has a production --------------------

        public bool NtsComplete()
        {
            bool complete = true;
            foreach (Symbol sym in nonterminals)
            {
                if (sym.graph == null)
                {
                    complete = false;
                    errors.SemErr(sym.pos, "No production for " + sym.name, 88);
                }
            }
            return complete;
        }

        //-------------- check if every nts can be reached  -----------------

        void MarkReachedNts(Node p)
        {
            while (p != null)
            {
                if (p.typ == NodeKind.nt && !visited[p.sym.n])
                { // new nt reached
                    visited[p.sym.n] = true;
                    MarkReachedNts(p.sym.graph);
                }
                else if (p.typ == NodeKind.alt || p.typ == NodeKind.iter || p.typ == NodeKind.opt)
                {
                    MarkReachedNts(p.sub);
                    if (p.typ == NodeKind.alt) MarkReachedNts(p.down);
                }
                if (p.up) break;
                p = p.next;
            }
        }

        public bool AllNtReached()
        {
            bool ok = true;
            visited = new BitArray(nonterminals.Count);
            visited[gramSy.n] = true;
            MarkReachedNts(gramSy.graph);
            foreach (Symbol sym in nonterminals)
            {
                if (!visited[sym.n])
                {
                    ok = false;
                    errors.Warning(sym.pos, sym.name + " cannot be reached", 41);
                }
            }
            return ok;
        }

        //--------- check if every nts can be derived to terminals  ------------

        bool IsTerm(Node p, BitArray mark)
        { // true if graph can be derived to terminals
            while (p != null)
            {
                if (p.typ == NodeKind.nt && !mark[p.sym.n]) 
                    return false;
                if (p.typ == NodeKind.alt 
                    && !IsTerm(p.sub, mark)
                    && (p.down == null || !IsTerm(p.down, mark))
                    ) return false;
                if (p.up) break;
                p = p.next;
            }
            return true;
        }

        public bool AllNtToTerm()
        {
            bool changed, ok = true;
            BitArray mark = new BitArray(nonterminals.Count);
            // a nonterminal is marked if it can be derived to terminal symbols
            do
            {
                changed = false;
                foreach (Symbol sym in nonterminals)
                    if (!mark[sym.n] && IsTerm(sym.graph, mark))
                    {
                        mark[sym.n] = true; changed = true;
                    }
            } while (changed);
            foreach (Symbol sym in nonterminals)
                if (!mark[sym.n])
                {
                    ok = false;
                    errors.SemErr(sym.pos, sym.name + " cannot be derived to terminals", 89);
                }
            return ok;
        }

        //---------------------------------------------------------------------
        //  Cross reference list
        //---------------------------------------------------------------------

        private class SymbolComp : IComparer<Symbol>
        {
            public int Compare(Symbol x, Symbol y)
            {
                return x.name.CompareTo(y.name);
            }
        }

        public void XRef()
        {
            var xref = new SortedList<Symbol, List<int>>(new SymbolComp());
            // collect lines where symbols have been defined
            foreach (Symbol sym in nonterminals)
            {
                if (!xref.TryGetValue(sym, out var list))
                {
                    list = new List<int>();
                    xref[sym] = list;
                }
                list.Add(-sym.pos.line);
            }
            // collect lines where symbols have been referenced
            foreach (Node n in nodes)
            {
                if (n.typ == NodeKind.t || n.typ == NodeKind.wt || n.typ == NodeKind.nt)
                {
                    if (!xref.TryGetValue(n.sym, out var list))
                    {
                        list = new List<int>();
                        xref[n.sym] = list;
                    }
                    list.Add(n.line);
                }
            }
            // print cross reference list
            trace.WriteLine();
            trace.WriteLine("Cross reference list:");
            trace.WriteLine("---------------------");
            trace.WriteLine();
            foreach (Symbol sym in xref.Keys)
            {
                trace.Write("  {0,-12}", Name(sym.name));
                var list = xref[sym];
                int col = 14;
                foreach (int line in list)
                {
                    if (col + 5 > 80)
                    {
                        trace.WriteLine();
                        for (col = 1; col <= 14; col++) trace.Write(" ");
                    }
                    trace.Write("{0,5}", line); col += 5;
                }
                trace.WriteLine();
            }
            trace.WriteLine();
            trace.WriteLine();
        }

        public void SetDDT(string s)
        {
            s = s.ToUpper();
            foreach (char ch in s)
            {
                if ('0' <= ch && ch <= '9') ddt[ch - '0'] = true;
                else switch (ch)
                    {
                        case 'A': ddt[0] = true; break; // trace automaton
                        case 'F': ddt[1] = true; break; // list first/follow sets
                        case 'G': ddt[2] = true; break; // print syntax graph
                        case 'I': ddt[3] = true; break; // trace computation of first sets
                        case 'J': ddt[4] = true; break; // print ANY and SYNC sets
                        case 'P': ddt[8] = true; break; // print statistics
                        case 'S': ddt[6] = true; break; // list symbol table
                        case 'X': ddt[7] = true; break; // list cross reference table
                        default: break;
                    }
            }
        }

        public void SetOption(string s)
        {
            string[] option = s.Split(new char[] { '=' }, 2);
            string name = option[0], value = option[1];
            if ("$namespace".Equals(name))
            {
                if (nsName == null) nsName = value;
            }
            else if ("$checkEOF".Equals(name))
            {
                checkEOF = "true".Equals(value);
            }
        }


    } // end Tab

} // end namespace
