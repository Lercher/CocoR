using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{

    //=====================================================================
    // Tab
    //=====================================================================

    public enum LL1Condition
    {
        StartOfSeveralAlternatives_W21 = 21,
        StartAndSuccessorOfDeleteableStructure_W22 = 22,
        AnyNodeThatMatchesNoSymbol_W23 = 23,
        ContentsOfOptOrAltMustNotBeDeleteable_W24 = 24
    }

    public class Tab
    {
        public Range semDeclPos;       // position of global semantic declarations
        public CharSet ignored;           // characters ignored by the scanner
        public readonly bool[] ddt = new bool[10]; // debug and test switches
        public Symbol gramSy;             // root nonterminal; filled by ATG
        public Symbol eofSy;              // end of file symbol
        public Symbol noSym;              // used in case of an error
        public BitArray allSyncSets;      // union of all synchronisation sets
        public readonly IDictionary<string, Symbol> literals;        // symbols that are used as literals
        public readonly List<SymTab> symtabs = new List<SymTab>();

        public string srcName;            // name of the atg file (including path)
        public string srcDir;             // directory path of the atg file
        public string nsName;             // namespace for generated files
        public string frameDir;           // directory containing the frame files
        public string outDir;             // directory for generated files
        public bool checkEOF = true;      // should coco generate a check for EOF at the end of Parser.Parse():
        public bool emitLines;            // emit #line pragmas for semantic actions in the generated parser
        public bool createOld;              // omit scanner.cs.old and parser.cs.old

        public readonly Parser parser;                    // other Coco objects
        private TextWriter Trace => parser.trace;
        private Errors Errors => parser.errors;


        public Tab(CocoRCore.CSharp.Parser parser)
        {
            this.parser = parser;
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

        public Symbol NewSym(NodeKind typ, string name, Position pos)
        {
            if (name.Length == 2 && name[0] == '"')
            {
                parser.SemErr(81, "empty token not allowed");
                name = "???";
            }
            var sym = new Symbol(typ, name, pos);
            switch (typ)
            {
                case NodeKind.t:
                    sym.n = terminals.Count;
                    terminals.Add(sym);
                    break;
                case NodeKind.pr:
                    sym.n = pragmas.Count;
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
            var qy = from s in terminals.Concat(nonterminals) where s.name == name select s;
            return qy.FirstOrDefault();
        }

        public SymTab FindSymtab(string name)
        {
            var qy = from st in symtabs where st.name == name select st;
            return qy.FirstOrDefault();
        }

        int Num(Node p) => p?.n ?? 0;

        void PrintSym(Symbol sym)
        {
            Trace.Write("{0,3} {1,-14} {2,-4}", sym.n, Name12(sym.name), sym.typ);
            if (sym.attrPos == null) Trace.Write(" false "); else Trace.Write(" true  ");
            if (sym.typ == NodeKind.nt)
            {
                Trace.Write("{0,5}", Num(sym.graph));
                if (sym.deletable) Trace.Write(" true  "); else Trace.Write(" false ");
            }
            else
                Trace.Write("            ");
            Trace.WriteLine("{0,5} {1}", sym.pos, sym.tokenKind);
        }

        public void PrintSymbolTable()
        {
            Trace.WriteLine("Symbol Table:");
            Trace.WriteLine("------------"); Trace.WriteLine();
            Trace.WriteLine(" nr name          typ  hasAt graph  del    line tokenKind");
            terminals.ForEach(PrintSym);
            pragmas.ForEach(PrintSym);
            nonterminals.ForEach(PrintSym);
            Trace.WriteLine();
            Trace.WriteLine("Literal Tokens:");
            Trace.WriteLine("--------------");
            foreach (var e in literals)
                Trace.WriteLine("_" + e.Value.name + " = " + e.Key + ".");
            Trace.WriteLine();
        }

        public void PrintSet(BitArray s, int indent)
        {
            int col, len;
            col = indent;
            foreach (var sym in terminals)
                if (s[sym.n])
                {
                    len = sym.name.Length;
                    if (col + len >= 80)
                    {
                        Trace.WriteLine();
                        for (col = 1; col < indent; col++) Trace.Write(" ");
                    }
                    Trace.Write("{0} ", sym.name);
                    col += len + 1;
                }
            if (col == indent) Trace.Write("-- empty set --");
            Trace.WriteLine();
        }

        //---------------------------------------------------------------------
        //  Syntax graph management
        //---------------------------------------------------------------------

        public List<Node> nodes = new List<Node>();
        Node dummyNode;

        public Node NewNode(NodeKind typ, Symbol sym, int line)
        {
            var node = new Node(typ, sym, line) { n = nodes.Count };
            nodes.Add(node);
            return node;
        }

        public Node NewNode(NodeKind typ, Node sub)
        {
            var node = NewNode(typ, null, 0);
            node.sub = sub;
            return node;
        }

        public Node NewNode(NodeKind typ, int val, int line)
        {
            var node = NewNode(typ, null, line);
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
            var p = g1.l;
            while (p.down != null)
                p = p.down;
            p.down = g2.l;
            p = g1.r;
            while (p.next != null)
                p = p.next;
            // append alternative to g1 end list
            p.next = g2.l;
            // append g2 end list to g1 end list
            g2.l.next = g2.r;
        }

        // The result will be in g1
        public void MakeSequence(Graph g1, Graph g2)
        {
            var p = g1.r.next;
            g1.r.next = g2.l; // link head node
            while (p != null)
            {  // link substructure
                var q = p.next;
                p.next = g2.l;
                p = q;
            }
            g1.r = g2.r;
        }

        public void MakeIteration(Graph g)
        {
            g.l = NewNode(NodeKind.iter, g.l);
            g.r.up = true;
            var p = g.r;
            g.r = g.l;
            while (p != null)
            {
                var q = p.next;
                p.next = g.l;
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
            var p = g.r;
            while (p != null)
            {
                var q = p.next;
                p.next = null;
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
            var s = Unstring(str);
            if (s.Length == 0)
                parser.SemErr(82, "empty token not allowed");
            var g = new Graph() { r = dummyNode };
            for (var i = 0; i < s.Length; i++)
            {
                var p = NewNode(NodeKind.chr, (int)s[i], 0);
                g.r.next = p;
                g.r = p;
            }
            g.l = dummyNode.next; dummyNode.next = null;
            return g;
        }

        public void SetContextTrans(Node p)
        {
            // set transition code in the graph rooted at p
            while (p != null)
            {
                if (p.typ == NodeKind.chr || p.typ == NodeKind.clas)
                    p.code = NodeTransition.contextTrans;
                else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                    SetContextTrans(p.sub);
                else if (p.typ == NodeKind.alt)
                {
                    SetContextTrans(p.sub); SetContextTrans(p.down);
                }
                if (p.up) break;
                p = p.next;
            }
        }

        //------------ graph deletability check -----------------

        public static bool DelGraph(Node p) => p == null || DelNode(p) && DelGraph(p.next);

        public static bool DelSubGraph(Node p) => p == null || DelNode(p) && (p.up || DelSubGraph(p.next));

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

        private static string Ptr(Node p, bool up)
        {
            var ptr = (p == null) ? "0" : p.n.ToString();
            return (up) ? ("-" + ptr) : ptr;
        }

        private static string Pos(Range pos) => string.Format("{0,5}", pos?.start.ToString() ?? string.Empty);

        public static string Name12(string name) => name.Length > 12 ? name.Substring(0, 12) : name;


        public void PrintNodes()
        {
            Trace.WriteLine("Graph nodes:");
            Trace.WriteLine("----------------------------------------------------");
            Trace.WriteLine("   n type name          next  down   sub   pos  line");
            Trace.WriteLine("                               val  code");
            Trace.WriteLine("----------------------------------------------------");
            foreach (var p in nodes)
            {
                Trace.Write("{0,4} {1,-4} ", p.n, p.typ);
                if (p.sym != null)
                    Trace.Write("{0,12} ", Name12(p.sym.name));
                else if (p.typ == NodeKind.clas)
                {
                    var c = classes[p.val];
                    Trace.Write("{0,12} ", Name12(c.name));
                }
                else Trace.Write("             ");
                Trace.Write("{0,5} ", Ptr(p.next, p.up));
                switch (p.typ)
                {
                    case NodeKind.t:
                    case NodeKind.nt:
                    case NodeKind.wt:
                        Trace.Write("             {0,5}", Pos(p.pos)); break;
                    case NodeKind.chr:
                        Trace.Write("{0,5} {1,5}       ", p.val, p.code); break;
                    case NodeKind.clas:
                        Trace.Write("      {0,5}       ", p.code); break;
                    case NodeKind.alt:
                    case NodeKind.iter:
                    case NodeKind.opt:
                        Trace.Write("{0,5} {1,5}       ", Ptr(p.down, false), Ptr(p.sub, false)); break;
                    case NodeKind.sem:
                        Trace.Write("             {0,5}", Pos(p.pos)); break;
                    case NodeKind.eps:
                    case NodeKind.any:
                    case NodeKind.sync:
                        Trace.Write("                  "); break;
                }
                Trace.WriteLine("{0,5}", p.line);
            }
            Trace.WriteLine();
        }


        //---------------------------------------------------------------------
        //  Character class management
        //---------------------------------------------------------------------

        public List<CharClass> classes = new List<CharClass>();
        public int dummyName = 'A';

        public CharClass NewCharClass(string name, CharSet s)
        {
            if (name == "#")
                name = "#" + (char)dummyName++;
            var c = new CharClass(name, s) { n = classes.Count };
            classes.Add(c);
            // System.Console.WriteLine("CharClass {0} = {1}", name, s);  // TODO - Trace Flag
            return c;
        }

        public CharClass FindCharClass(string name) => classes.Where(cc => cc.name == name).FirstOrDefault();

        public CharClass FindCharClass(CharSet s) => classes.Where(cc => s.Equals(cc.set)).FirstOrDefault();

        public CharSet CharClassSet(int i) => classes[i].set;

        //----------- character class printing


        void WriteCharSet(CharSet s)
        {
            for (var r = s.head; r != null; r = r.next)
                if (r.from < r.to)
                    Trace.Write(DFA.Ch(r.from) + ".." + DFA.Ch(r.to) + " ");
                else
                    Trace.Write(DFA.Ch(r.from) + " ");
        }

        public void WriteCharClasses()
        {
            foreach (var cc in classes)
            {
                Trace.Write("{0,-10}: ", cc.name);
                WriteCharSet(cc.set);
                Trace.WriteLine();
            }
            Trace.WriteLine();
        }


        //---------------------------------------------------------------------
        //  Symbol set computations
        //---------------------------------------------------------------------

        /* Computes the first set for the graph rooted at p */
        BitArray First0(Node p, BitArray mark)
        {
            var fs = new BitArray(terminals.Count);
            while (p != null && !mark[p.n])
            {
                mark[p.n] = true;
                switch (p.typ)
                {
                    case NodeKind.nt:
                        if (p.sym.firstReady)
                            fs.Or(p.sym.first);
                        else
                            fs.Or(First0(p.sym.graph, mark));
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
            var fs = First0(p, new BitArray(nodes.Count));
            if (ddt[3])
            {
                Trace.WriteLine();
                if (p != null)
                    Trace.WriteLine("First: node = {0}", p.n);
                else
                    Trace.WriteLine("First: node = null");
                PrintSet(fs, 0);
            }
            return fs;
        }

        void CompFirstSets()
        {
            foreach (var sym in nonterminals)
            {
                sym.first = new BitArray(terminals.Count);
                sym.firstReady = false;
            }
            foreach (var sym in nonterminals)
            {
                sym.first = First(sym.graph);
                sym.firstReady = true;
            }
        }

        void CompFollow(Node pp, Symbol curSy, BitArray visited)
        {
            for (var p = pp;  p != null && !visited[p.n]; p = p.next)
            {
                visited[p.n] = true;
                if (p.typ == NodeKind.nt)
                {
                    var s = First(p.next);
                    p.sym.follow.Or(s);
                    if (DelGraph(p.next))
                        p.sym.nts[curSy.n] = true;
                }
                else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                    CompFollow(p.sub, curSy, visited);
                else if (p.typ == NodeKind.alt)
                {
                    CompFollow(p.sub, curSy, visited);
                    CompFollow(p.down, curSy, visited);
                }
            }
        }

        private void Complete(Symbol sym, Symbol curSy, BitArray visited)
        {
            if (!visited[sym.n])
            {
                visited[sym.n] = true;
                foreach (var s in nonterminals)
                    if (sym.nts[s.n])
                    {
                        Complete(s, curSy, visited);
                        sym.follow.Or(s.follow);
                        if (sym == curSy)
                            sym.nts[s.n] = false;
                    }
            }
        }

        void CompFollowSets()
        {
            foreach (var sym in nonterminals)
            {
                sym.follow = new BitArray(terminals.Count);
                sym.nts = new BitArray(nonterminals.Count);
            }
            gramSy.follow[eofSy.n] = true;
            {
                var visited = new BitArray(nodes.Count);
                foreach (var sym in nonterminals)
                { 
                    // get direct successors of nonterminals
                    CompFollow(sym.graph, sym, visited);
                }
            }
            foreach (var sym in nonterminals)
            { 
                // add indirect successors to followers
                var visited = new BitArray(nonterminals.Count);
                Complete(sym, sym, visited);
            }
        }

        Node LeadingAny(Node p)
        {
            if (p == null) return null;
            Node a = null;
            if (p.typ == NodeKind.any)
                a = p;
            else if (p.typ == NodeKind.alt)
            {
                a = LeadingAny(p.sub);
                if (a == null)
                    a = LeadingAny(p.down);
            }
            else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                a = LeadingAny(p.sub);
            if (a == null && DelNode(p) && !p.up)
                a = LeadingAny(p.next);
            return a;
        }

        void FindAS(Node p)
        { 
            // find ANY sets
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
                    var s1 = new BitArray(terminals.Count);
                    var q = p;
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
                        var q = (p.typ == NodeKind.nt) ? p.sym.graph : p.sub;
                        a.set.Subtract(First(q));
                    }
                }

                if (p.up) break;
                p = p.next;
            }
        }

        void CompAnySets()
        {
            foreach (var sym in nonterminals)
                FindAS(sym.graph);
        }

        public BitArray Expected(Node p, Symbol curSy)
        {
            var s = First(p);
            if (DelGraph(p))
                s.Or(curSy.follow);
            return s;
        }

        // does not look behind resolvers; only called during LL(1) test and in CheckRes
        public BitArray Expected0(Node p, Symbol curSy)
        {
            if (p.typ == NodeKind.rslv)
                return new BitArray(terminals.Count);
            else
                return Expected(p, curSy);
        }

        void CompSync(Node p, Symbol curSy, BitArray visited)
        {
            while (p != null && !visited[p.n])
            {
                visited[p.n] = true;
                if (p.typ == NodeKind.sync)
                {
                    var s = Expected(p.next, curSy);
                    s[eofSy.n] = true;
                    allSyncSets.Or(s);
                    p.set = s;
                }
                else if (p.typ == NodeKind.alt)
                {
                    CompSync(p.sub, curSy, visited);
                    CompSync(p.down, curSy, visited);
                }
                else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                    CompSync(p.sub, curSy, visited);
                p = p.next;
            }
        }

        void CompSyncSets()
        {
            allSyncSets = new BitArray(terminals.Count)
            {
                [eofSy.n] = true
            };
            var visited = new BitArray(nodes.Count);
            foreach (var sym in nonterminals)
            {
                CompSync(sym.graph, sym, visited);
            }
        }

        public void SetupAnys()
        {
            foreach (var p in nodes)
                if (p.typ == NodeKind.any)
                {
                    p.set = new BitArray(terminals.Count, true)
                    {
                        [eofSy.n] = false
                    };
                }
        }

        public void CompDeletableSymbols()
        {
            bool changed;
            do
            {
                changed = false;
                foreach (var sym in nonterminals)
                    if (!sym.deletable && sym.graph != null && DelGraph(sym.graph))
                    {
                        sym.deletable = true;
                        changed = true;
                    }
            } while (changed);
            foreach (var sym in nonterminals)
                if (sym.deletable && !sym.deletableOK)
                    Errors.Warning(sym.pos, $"NT {sym.name} deletable", 11);
        }

        public void RenumberPragmas()
        {
            var n = terminals.Count;
            foreach (var sym in pragmas)
                sym.n = n++;
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
                Trace.WriteLine();
                Trace.WriteLine("First & follow symbols:");
                Trace.WriteLine("----------------------"); Trace.WriteLine();
                foreach (var sym in nonterminals)
                {
                    Trace.WriteLine(sym.name);
                    Trace.Write("first:   "); PrintSet(sym.first, 10);
                    Trace.Write("follow:  "); PrintSet(sym.follow, 10);
                    Trace.WriteLine();
                }
            }
            if (ddt[4])
            {
                Trace.WriteLine();
                Trace.WriteLine("ANY and SYNC sets:");
                Trace.WriteLine("-----------------");
                foreach (var p in nodes)
                    if (p.typ == NodeKind.any || p.typ == NodeKind.sync)
                    {
                        Trace.Write("{0,4} {1,-4}: ", p.n, p.typ);
                        PrintSet(p.set, 11);
                    }
            }
        }

        //---------------------------------------------------------------------
        //  String handling
        //---------------------------------------------------------------------

        char Hex2Char(string s)
        {
            var val = 0;
            for (var i = 0; i < s.Length; i++)
            {
                var ch = s[i];
                if ('0' <= ch && ch <= '9')
                    val = 16 * val + (ch - '0');
                else if ('a' <= ch && ch <= 'f')
                    val = 16 * val + (10 + ch - 'a');
                else if ('A' <= ch && ch <= 'F')
                    val = 16 * val + (10 + ch - 'A');
                else
                    parser.SemErr(83, "non hex escape sequence in string or character");
            }
            if (val > char.MaxValue)
            {   /* pdt */
                parser.SemErr(84, "too big hex escape sequence in string or character");
                return '\0';
            }
            return (char)val;
        }

        string Char2Hex(char ch) => string.Format(@"\u{0:x4}", (int)ch);

        public string Unstring(string s)
        {
            if (s == null || s.Length < 2)
                return s;
            return Unescape(s.Substring(1, s.Length - 2));
        }

        public string Unescape(string s)
        {
            /* replaces escape sequences in s by their Unicode values. */
            var buf = new StringBuilder();
            var i = 0;
            while (i < s.Length)
            {
                if (s[i] == '\\')
                    if (i + 1 < s.Length)
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
                                    buf.Append(Hex2Char(s.Substring(i + 2, 4)));
                                    i += 6;
                                    break;
                                }
                                else
                                {
                                    parser.SemErr(85, "bad escape sequence in string or character");
                                    i = s.Length;
                                    break;
                                }
                            default:
                                parser.SemErr(86, "bad escape sequence in string or character");
                                i += 2;
                                break;
                        }
                    else
                    {
                        parser.SemErr(87, "bad escape sequence in string or character");
                        i = s.Length;
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
            var buf = new StringBuilder();
            foreach (var ch in s)
                switch (ch)
                {
                    case '\\': buf.Append("\\\\"); break;
                    case '\'': buf.Append("\\'"); break;
                    case '\"': buf.Append("\\\""); break;
                    case '\t': buf.Append("\\t"); break;
                    case '\r': buf.Append("\\r"); break;
                    case '\n': buf.Append("\\n"); break;
                    default:
                        if (ch < ' ' || '\u007f' < ch)
                            buf.Append(Char2Hex(ch));
                        else
                            buf.Append(ch);
                        break;
                }
            return buf.ToString();
        }

        //---------------------------------------------------------------------
        //  Grammar checks
        //---------------------------------------------------------------------

        public bool GrammarOk()
        {
            var ok = NtsComplete()
                && AllNtReached()
                && NoCircularProductions()
                && AllNtToTerm();
            if (ok)
            {
                CheckResolvers();
                CheckLL1();
            }
            return ok;
        }

        //--------------- check for circular productions ----------------------

        class CNode
        {   // node of list for finding circular productions
            public Symbol left, right;

            public CNode(Symbol l, Symbol r)
            {
                left = l;
                right = r;
            }
        }

        void GetSingles(Node p, List<Symbol> singles)
        {
            if (p == null) return;  // end of graph
            if (p.typ == NodeKind.nt)
            {
                if (p.up || DelGraph(p.next))
                    singles.Add(p.sym);
            }
            else if (p.typ == NodeKind.alt || p.typ == NodeKind.iter || p.typ == NodeKind.opt)
                if (p.up || DelGraph(p.next))
                {
                    GetSingles(p.sub, singles);
                    if (p.typ == NodeKind.alt)
                        GetSingles(p.down, singles);
                }
            if (!p.up && DelNode(p))
                GetSingles(p.next, singles);
        }

        public bool NoCircularProductions()
        {
            bool ok, changed, onLeftSide, onRightSide;
            var list = new List<CNode>();
            foreach (var sym in nonterminals)
            {
                var singles = new List<Symbol>();
                GetSingles(sym.graph, singles); // get nonterminals s such that sym-->s
                foreach (var s in singles)
                    list.Add(new CNode(sym, s));
            }
            do
            {
                changed = false;
                for (var i = 0; i < list.Count; i++)
                {
                    var n = list[i];
                    onLeftSide = false;
                    onRightSide = false;
                    foreach (var m in list)
                    {
                        if (n.left == m.right)
                            onRightSide = true;
                        if (n.right == m.left)
                            onLeftSide = true;
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
            foreach (var n in list)
            {
                ok = false;
                Errors.SemErr(n.left.pos, n.left.name + " --> " + n.right.name + n.right.pos.ToString(), 87);
            }
            return ok;
        }

        //--------------- check for LL(1) errors ----------------------

        private void LL1Warning(LL1Condition cond, Symbol sym, Node p, Node q, Symbol curSy)
        {
            var s = $"LL1 warning in production {curSy.name}: ";
            if (sym != null)
                s += $"{sym} is ";
            switch (cond)
            {
                case LL1Condition.StartOfSeveralAlternatives_W21:
                    s += "start of several alternatives";
                    break;
                case LL1Condition.StartAndSuccessorOfDeleteableStructure_W22:
                    s += "start and successor of deletable structure";
                    break;
                case LL1Condition.AnyNodeThatMatchesNoSymbol_W23:
                    s += "there is an ANY node that matches no symbol";
                    break;
                case LL1Condition.ContentsOfOptOrAltMustNotBeDeleteable_W24:
                    s += "contents of [...] or {...} must not be deletable";
                    break;
            }
            if (p != null) s += $" [{p}]";
            if (q != null) s += $" [{q}]";
            Errors.Warning(curSy.pos, s, (int)cond); // warning 21, 22, 23, 24
        }


        private void CheckOverlap(BitArray s1, BitArray s2, Node p, Node q, Symbol curSy, LL1Condition cond)
        {
            foreach (var sym in terminals)
                if (s1[sym.n] && s2[sym.n])
                    LL1Warning(cond, sym, p, q, curSy);
        }

        void CheckAlts(Node p, Symbol curSy)
        {
            while (p != null)
            {
                if (p.typ == NodeKind.alt)
                {
                    
                    var s1 = new BitArray(terminals.Count); // start at: all false
                    for (var q = p; q != null; q = q.down)
                    { 
                        // for all alternatives
                        var s2 = Expected0(q.sub, curSy);
                        CheckOverlap(s1, s2, p, q, curSy, LL1Condition.StartOfSeveralAlternatives_W21);
                        s1.Or(s2); // mutates s1
                        CheckAlts(q.sub, curSy);
                    }
                }
                else if (p.typ == NodeKind.opt || p.typ == NodeKind.iter)
                {
                    if (DelSubGraph(p.sub))
                        LL1Warning(LL1Condition.ContentsOfOptOrAltMustNotBeDeleteable_W24, null, p, null, curSy); // e.g. [[...]]
                    else
                    {
                        var s1 = Expected0(p.sub, curSy);
                        var s2 = Expected(p.next, curSy);
                        CheckOverlap(s1, s2, null, null, curSy, LL1Condition.StartAndSuccessorOfDeleteableStructure_W22);
                    }
                    CheckAlts(p.sub, curSy);
                }
                else if (p.typ == NodeKind.any)
                    if (!p.set.Any())
                        LL1Warning(LL1Condition.AnyNodeThatMatchesNoSymbol_W23, null, null, null, curSy); // e.g. {ANY} ANY or [ANY] ANY or ( ANY | ANY )
                if (p.up)
                    break;
                p = p.next;
            }
        }

        public void CheckLL1()
        {
            foreach (var sym in nonterminals)
                CheckAlts(sym.graph, sym);
        }

        //------------- check if resolvers are legal  --------------------

        void ResErr(Node p, string msg) => Errors.Warning(p.pos.start, msg, id: 31);

        void CheckRes(Node p, bool rslvAllowed, Symbol curSy)
        {
            while (p != null)
            {
                switch (p.typ)
                {
                    case NodeKind.alt:
                        var expected = new BitArray(terminals.Count);
                        for (var q = p; q != null; q = q.down)
                            expected.Or(Expected0(q.sub, curSy));
                        var soFar = new BitArray(terminals.Count);
                        for (var q = p; q != null; q = q.down)
                        {
                            if (q.sub.typ == NodeKind.rslv)
                            {
                                var fs = Expected(q.sub.next, curSy);
                                if (fs.Intersects(soFar))
                                    ResErr(q.sub, "Warning: Resolver will never be evaluated. Place it at previous conflicting alternative.");
                                if (!fs.Intersects(expected))
                                    ResErr(q.sub, "Warning: Misplaced resolver: no LL(1) conflict.");
                            }
                            else soFar.Or(Expected(q.sub, curSy));
                            CheckRes(q.sub, true, curSy);
                        }
                        break;
                    case NodeKind.iter:
                    case NodeKind.opt:
                        if (p.sub.typ == NodeKind.rslv)
                        {
                            var fs = First(p.sub.next);
                            var fsNext = Expected(p.next, curSy);
                            if (!fs.Intersects(fsNext))
                                ResErr(p.sub, "Warning: Misplaced resolver: no LL(1) conflict.");
                        }
                        CheckRes(p.sub, true, curSy);
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
            foreach (var sym in nonterminals)
                CheckRes(sym.graph, false, sym);
        }

        //------------- check if every nts has a production --------------------

        public bool NtsComplete()
        {
            var complete = true;
            foreach (var sym in nonterminals)
                if (sym.graph == null)
                {
                    complete = false;
                    Errors.SemErr(sym.pos, "No production for " + sym.name, 88);
                }
            return complete;
        }

        //-------------- check if every nts can be reached  -----------------

        void MarkReachedNts(Node pp, BitArray visited)
        {
            for (var p = pp; p != null; p = p.next)
            {
                if (p.typ == NodeKind.nt && !visited[p.sym.n])
                { 
                    // new nt reached
                    visited[p.sym.n] = true;
                    MarkReachedNts(p.sym.graph, visited);
                }
                else if (p.typ == NodeKind.alt || p.typ == NodeKind.iter || p.typ == NodeKind.opt)
                {
                    MarkReachedNts(p.sub, visited);
                    if (p.typ == NodeKind.alt)
                        MarkReachedNts(p.down, visited);
                }
                if (p.up)
                    break;
            }
        }

        public bool AllNtReached()
        {
            var ok = true;
            var visited = new BitArray(nonterminals.Count)
            {
                [gramSy.n] = true
            };
            MarkReachedNts(gramSy.graph, visited);
            foreach (var sym in nonterminals)
                if (!visited[sym.n])
                {
                    ok = false;
                    Errors.Warning(sym.pos, $"{sym.name} cannot be reached", 41);
                }
            return ok;
        }

        //--------- check if every nts can be derived to terminals  ------------

        bool IsTerm(Node p, BitArray mark)
        { 
            // true if graph can be derived to terminals
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
            var mark = new BitArray(nonterminals.Count);
            // a nonterminal is marked if it can be derived to terminal symbols
            do
            {
                changed = false;
                foreach (var sym in nonterminals)
                    if (!mark[sym.n] && IsTerm(sym.graph, mark))
                    {
                        mark[sym.n] = true; changed = true;
                    }
            } while (changed);
            foreach (var sym in nonterminals)
                if (!mark[sym.n])
                {
                    ok = false;
                    Errors.SemErr(sym.pos, sym.name + " cannot be derived to terminals", 89);
                }
            return ok;
        }

        //---------------------------------------------------------------------
        //  Cross reference list
        //---------------------------------------------------------------------

        private class SymbolComp : IComparer<Symbol>
        {
            public int Compare(Symbol x, Symbol y) => x.name.CompareTo(y.name);
        }

        public void XRef()
        {
            var xref = new SortedList<Symbol, List<int>>(new SymbolComp());
            // collect lines where symbols have been defined
            foreach (var sym in nonterminals)
            {
                if (!xref.TryGetValue(sym, out var list))
                {
                    list = new List<int>();
                    xref[sym] = list;
                }
                list.Add(-sym.pos.line);
            }
            // collect lines where symbols have been referenced
            foreach (var n in nodes)
                if (n.typ == NodeKind.t || n.typ == NodeKind.wt || n.typ == NodeKind.nt)
                {
                    if (!xref.TryGetValue(n.sym, out var list))
                    {
                        list = new List<int>();
                        xref[n.sym] = list;
                    }
                    list.Add(n.line);
                }
            // print cross reference list
            Trace.WriteLine();
            Trace.WriteLine("Cross reference list:");
            Trace.WriteLine("---------------------");
            Trace.WriteLine();
            foreach (var sym in xref.Keys)
            {
                Trace.Write("  {0,-12}", Name12(sym.name));
                var list = xref[sym];
                var col = 14;
                foreach (var line in list)
                {
                    if (col + 5 > 80)
                    {
                        Trace.WriteLine();
                        for (col = 1; col <= 14; col++) Trace.Write(" ");
                    }
                    Trace.Write("{0,5}", line); col += 5;
                }
                Trace.WriteLine();
            }
            Trace.WriteLine();
            Trace.WriteLine();
        }

        public void SetDDT(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            s = s.ToUpper();
            foreach (var ch in s)
                if (char.IsDigit(ch))
                    ddt[ch - '0'] = true;
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

        public void SetOption(string s)
        {
            var option = s.Split('=');
            var name = option.First();
            var value = option.Last();
            switch (name)
            {
                case "$namespace":
                    nsName = value;
                    break;
                case "$checkEOF":
                    checkEOF = ("true" == value);
                    break;
            }
        }


    } // end Tab

} // end namespace
