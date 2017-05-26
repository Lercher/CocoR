using System;
using System.IO;
using System.Text;
using System.Collections;
using CocoRCore;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{

    //-----------------------------------------------------------------------------
    //  DFA
    //-----------------------------------------------------------------------------

    public class DFA
    {
        private int maxStates;
        private int lastStateNr;   // highest state number
        private State firstState;
        private State lastState;   // last allocated state
        private int lastSimState;  // last non melted state
        private FileStream fram;   // scanner frame input
        private StreamWriter gen;  // generated scanner file
        private Symbol curSy;      // current token to be recognized (in FindTrans)
        private bool dirtyDFA;     // DFA may become nondeterministic in MatchLiteral

        public bool ignoreCase;   // true if input should be treated case-insensitively
        public bool hasCtxMoves;  // DFA has context transitions

        // other Coco objects
        private Parser parser;
        private Tab tab;
        private ErrorsBase errors;
        private TextWriter trace;

        //---------- Output primitives
        private string Ch(int ch)
        {
            if (ch < ' ' || ch >= 127 || ch == '\'' || ch == '\\') return Convert.ToString(ch);
            else return String.Format("'{0}'", (char)ch);
        }

        private string ChCond(char ch) => String.Format("ch == {0}", Ch(ch));
        private string ChCondNot(char ch) => String.Format("ch != {0}", Ch(ch));

        private void PutRange(CharSet s)
        {
            for (CharSet.Range r = s.head; r != null; r = r.next)
            {
                if (r.from == r.to) { gen.Write("ch == " + Ch(r.from)); }
                else if (r.from == 0) { gen.Write("ch <= " + Ch(r.to)); }
                else { gen.Write("ch >= " + Ch(r.from) + " && ch <= " + Ch(r.to)); }
                if (r.next != null) gen.Write(" || ");
            }
        }

        //---------- State handling

        State NewState()
        {
            State s = new State(); s.nr = ++lastStateNr;
            if (firstState == null) firstState = s; else lastState.next = s;
            lastState = s;
            return s;
        }

        void NewTransition(State from, State to, NodeKind typ, int sym, NodeTransition tc)
        {
            Target t = new Target() { state = to };
            Action a = new Action(typ, sym, tc) { target = t };
            from.AddAction(a);
            if (typ == NodeKind.clas) curSy.tokenKind = Symbol.classToken;
        }

        void CombineShifts()
        {
            State state;
            Action a, b, c;
            CharSet seta, setb;
            for (state = firstState; state != null; state = state.next)
            {
                for (a = state.firstAction; a != null; a = a.next)
                {
                    b = a.next;
                    while (b != null)
                        if (a.target.state == b.target.state && a.tc == b.tc)
                        {
                            seta = a.Symbols(tab); setb = b.Symbols(tab);
                            seta.Or(setb);
                            a.ShiftWith(seta, tab);
                            c = b; b = b.next; state.DetachAction(c);
                        }
                        else b = b.next;
                }
            }
        }

        void FindUsedStates(State state, BitArray used)
        {
            if (used[state.nr]) return;
            used[state.nr] = true;
            for (Action a = state.firstAction; a != null; a = a.next)
                FindUsedStates(a.target.state, used);
        }

        void DeleteRedundantStates()
        {
            State[] newState = new State[lastStateNr + 1];
            BitArray used = new BitArray(lastStateNr + 1);
            FindUsedStates(firstState, used);
            // combine equal final states
            for (State s1 = firstState.next; s1 != null; s1 = s1.next) // firstState cannot be final
                if (used[s1.nr] && s1.endOf != null && s1.firstAction == null && !s1.ctx)
                    for (State s2 = s1.next; s2 != null; s2 = s2.next)
                        if (used[s2.nr] && s1.endOf == s2.endOf && s2.firstAction == null & !s2.ctx)
                        {
                            used[s2.nr] = false; newState[s2.nr] = s1;
                        }
            for (State state = firstState; state != null; state = state.next)
                if (used[state.nr])
                    for (Action a = state.firstAction; a != null; a = a.next)
                        if (!used[a.target.state.nr])
                            a.target.state = newState[a.target.state.nr];
            // delete unused states
            lastState = firstState; lastStateNr = 0; // firstState has number 0
            for (State state = firstState.next; state != null; state = state.next)
                if (used[state.nr]) { state.nr = ++lastStateNr; lastState = state; }
                else lastState.next = state.next;
        }

        State TheState(Node p)
        {
            State state;
            if (p == null) { state = NewState(); state.endOf = curSy; return state; }
            else return p.state;
        }

        void Step(State from, Node p, BitArray stepped)
        {
            if (p == null) return;
            stepped[p.n] = true;
            switch (p.typ)
            {
                case NodeKind.clas:
                case NodeKind.chr:
                    {
                        NewTransition(from, TheState(p.next), p.typ, p.val, p.code);
                        break;
                    }
                case NodeKind.alt:
                    {
                        Step(from, p.sub, stepped); Step(from, p.down, stepped);
                        break;
                    }
                case NodeKind.iter:
                    {
                        if (Tab.DelSubGraph(p.sub))
                        {
                            parser.SemErr(61, "contents of {...} must not be deletable");
                            return;
                        }
                        if (p.next != null && !stepped[p.next.n]) Step(from, p.next, stepped);
                        Step(from, p.sub, stepped);
                        if (p.state != from)
                        {
                            Step(p.state, p, new BitArray(tab.nodes.Count));
                        }
                        break;
                    }
                case NodeKind.opt:
                    {
                        if (p.next != null && !stepped[p.next.n]) Step(from, p.next, stepped);
                        Step(from, p.sub, stepped);
                        break;
                    }
            }
        }

        // Assigns a state n.state to every node n. There will be a transition from
        // n.state to n.next.state triggered by n.val. All nodes in an alternative
        // chain are represented by the same state.
        // Numbering scheme:
        //  - any node after a chr, clas, opt, or alt, must get a new number
        //  - if a nested structure starts with an iteration the iter node must get a new number
        //  - if an iteration follows an iteration, it must get a new number
        void NumberNodes(Node p, State state, bool renumIter)
        {
            if (p == null) return;
            if (p.state != null) return; // already visited;
            if (state == null || (p.typ == NodeKind.iter && renumIter)) state = NewState();
            p.state = state;
            if (Tab.DelGraph(p)) state.endOf = curSy;
            switch (p.typ)
            {
                case NodeKind.clas:
                case NodeKind.chr:
                    {
                        NumberNodes(p.next, null, false);
                        break;
                    }
                case NodeKind.opt:
                    {
                        NumberNodes(p.next, null, false);
                        NumberNodes(p.sub, state, true);
                        break;
                    }
                case NodeKind.iter:
                    {
                        NumberNodes(p.next, state, true);
                        NumberNodes(p.sub, state, true);
                        break;
                    }
                case NodeKind.alt:
                    {
                        NumberNodes(p.next, null, false);
                        NumberNodes(p.sub, state, true);
                        NumberNodes(p.down, state, renumIter);
                        break;
                    }
            }
        }

        void FindTrans(Node p, bool start, BitArray marked)
        {
            if (p == null || marked[p.n]) return;
            marked[p.n] = true;
            if (start) Step(p.state, p, new BitArray(tab.nodes.Count)); // start of group of equally numbered nodes
            switch (p.typ)
            {
                case NodeKind.clas:
                case NodeKind.chr:
                    {
                        FindTrans(p.next, true, marked);
                        break;
                    }
                case NodeKind.opt:
                    {
                        FindTrans(p.next, true, marked); FindTrans(p.sub, false, marked);
                        break;
                    }
                case NodeKind.iter:
                    {
                        FindTrans(p.next, false, marked); FindTrans(p.sub, false, marked);
                        break;
                    }
                case NodeKind.alt:
                    {
                        FindTrans(p.sub, false, marked); FindTrans(p.down, false, marked);
                        break;
                    }
            }
        }

        public void ConvertToStates(Node p, Symbol sym)
        {
            curSy = sym;
            if (Tab.DelGraph(p))
            {
                parser.SemErr(62, "token might be empty");
                return;
            }
            NumberNodes(p, firstState, true);
            FindTrans(p, true, new BitArray(tab.nodes.Count));
            if (p.typ == NodeKind.iter)
            {
                Step(firstState, p, new BitArray(tab.nodes.Count));
            }
        }

        // match string against current automaton; store it either as a fixedToken or as a litToken
        public void MatchLiteral(string s, Symbol sym)
        {
            s = tab.Unescape(s.Substring(1, s.Length - 2));
            int i, len = s.Length;
            State state = firstState;
            Action a = null;
            for (i = 0; i < len; i++)
            { // try to match s against existing DFA
                a = FindAction(state, s[i]);
                if (a == null) break;
                state = a.target.state;
            }
            // if s was not totally consumed or leads to a non-final state => make new DFA from it
            if (i != len || state.endOf == null)
            {
                state = firstState; i = 0; a = null;
                dirtyDFA = true;
            }
            for (; i < len; i++)
            { // make new DFA for s[i..len-1], ML: i is either 0 or len
                State to = NewState();
                NewTransition(state, to, NodeKind.chr, s[i], NodeTransition.normalTrans);
                state = to;
            }
            Symbol matchedSym = state.endOf;
            if (state.endOf == null)
            {
                state.endOf = sym;
            }
            else if (matchedSym.tokenKind == Symbol.fixedToken || (a != null && a.tc == NodeTransition.contextTrans))
            {
                // s matched a token with a fixed definition or a token with an appendix that will be cut off
                parser.SemErr(63, "tokens " + sym.name + " and " + matchedSym.name + " cannot be distinguished");
            }
            else
            { // matchedSym == classToken || classLitToken
                matchedSym.tokenKind = Symbol.classLitToken;
                sym.tokenKind = Symbol.litToken;
            }
        }

        void SplitActions(State state, Action a, Action b)
        {
            Action c; CharSet seta, setb, setc;
            seta = a.Symbols(tab); setb = b.Symbols(tab);
            if (seta.Equals(setb))
            {
                a.AddTargets(b);
                state.DetachAction(b);
            }
            else if (seta.Includes(setb))
            {
                setc = seta.Clone(); setc.Subtract(setb);
                b.AddTargets(a);
                a.ShiftWith(setc, tab);
            }
            else if (setb.Includes(seta))
            {
                setc = setb.Clone(); setc.Subtract(seta);
                a.AddTargets(b);
                b.ShiftWith(setc, tab);
            }
            else
            {
                setc = seta.Clone(); setc.And(setb);
                seta.Subtract(setc);
                setb.Subtract(setc);
                a.ShiftWith(seta, tab);
                b.ShiftWith(setb, tab);
                c = new Action(0, 0, NodeTransition.normalTrans);  // typ and sym are set in ShiftWith
                c.AddTargets(a);
                c.AddTargets(b);
                c.ShiftWith(setc, tab);
                state.AddAction(c);
            }
        }

        bool Overlap(Action a, Action b)
        {
            CharSet seta, setb;
            if (a.typ == NodeKind.chr)
                if (b.typ == NodeKind.chr) return a.sym == b.sym;
                else { setb = tab.CharClassSet(b.sym); return setb[a.sym]; }
            else
            {
                seta = tab.CharClassSet(a.sym);
                if (b.typ == NodeKind.chr) return seta[b.sym];
                else { setb = tab.CharClassSet(b.sym); return seta.Intersects(setb); }
            }
        }

        void MakeUnique(State state)
        {
            bool changed;
            do
            {
                changed = false;
                for (Action a = state.firstAction; a != null; a = a.next)
                    for (Action b = a.next; b != null; b = b.next)
                        if (Overlap(a, b)) { SplitActions(state, a, b); changed = true; }
            } while (changed);
        }

        void MeltStates(State state)
        {
            bool ctx;
            BitArray targets;
            Symbol endOf;
            for (Action action = state.firstAction; action != null; action = action.next)
            {
                if (action.target.next != null)
                {
                    GetTargetStates(action, out targets, out endOf, out ctx);
                    Melted melt = StateWithSet(targets);
                    if (melt == null)
                    {
                        State s = NewState(); s.endOf = endOf; s.ctx = ctx;
                        for (Target targ = action.target; targ != null; targ = targ.next)
                            s.MeltWith(targ.state);
                        MakeUnique(s);
                        melt = NewMelted(targets, s);
                    }
                    action.target.next = null;
                    action.target.state = melt.state;
                }
            }
        }

        void FindCtxStates()
        {
            for (State state = firstState; state != null; state = state.next)
                for (Action a = state.firstAction; a != null; a = a.next)
                    if (a.tc == NodeTransition.contextTrans) a.target.state.ctx = true;
        }

        public void MakeDeterministic()
        {
            State state;
            lastSimState = lastState.nr;
            maxStates = 2 * lastSimState; // heuristic for set size in Melted.set
            FindCtxStates();
            for (state = firstState; state != null; state = state.next)
                MakeUnique(state);
            for (state = firstState; state != null; state = state.next)
                MeltStates(state);
            DeleteRedundantStates();
            CombineShifts();
        }

        public void PrintStates()
        {
            trace.WriteLine();
            trace.WriteLine("---------- states ----------");
            for (State state = firstState; state != null; state = state.next)
            {
                bool first = true;
                if (state.endOf == null) trace.Write("               ");
                else trace.Write("E({0,12})", tab.Name(state.endOf.name));
                trace.Write("{0,3}:", state.nr);
                if (state.firstAction == null) trace.WriteLine();
                for (Action action = state.firstAction; action != null; action = action.next)
                {
                    if (first) { trace.Write(" "); first = false; } else trace.Write("                    ");
                    if (action.typ == NodeKind.clas) trace.Write(((CharClass)tab.classes[action.sym]).name);
                    else trace.Write("{0, 3}", Ch(action.sym));
                    for (Target targ = action.target; targ != null; targ = targ.next)
                        trace.Write(" {0, 3}", targ.state.nr);
                    if (action.tc == NodeTransition.contextTrans) trace.WriteLine(" context"); else trace.WriteLine();
                }
            }
            trace.WriteLine();
            trace.WriteLine("---------- character classes ----------");
            tab.WriteCharClasses();
        }

        //---------------------------- actions --------------------------------

        public Action FindAction(State state, char ch)
        {
            for (Action a = state.firstAction; a != null; a = a.next)
                if (a.typ == NodeKind.chr && ch == a.sym) return a;
                else if (a.typ == NodeKind.clas)
                {
                    CharSet s = tab.CharClassSet(a.sym);
                    if (s[ch]) return a;
                }
            return null;
        }

        public void GetTargetStates(Action a, out BitArray targets, out Symbol endOf, out bool ctx)
        {
            // compute the set of target states
            targets = new BitArray(maxStates); endOf = null;
            ctx = false;
            for (Target t = a.target; t != null; t = t.next)
            {
                int stateNr = t.state.nr;
                if (stateNr <= lastSimState) targets[stateNr] = true;
                else targets.Or(MeltedSet(stateNr));
                if (t.state.endOf != null)
                    if (endOf == null || endOf == t.state.endOf)
                        endOf = t.state.endOf;
                    else
                        parser.errors.SemErr(endOf.pos, "Tokens " + endOf.name + " and " + t.state.endOf.name + " cannot be distinguished", 67);
                if (t.state.ctx)
                {
                    ctx = true;
                    // The following check seems to be unnecessary. It reported an error
                    // if a symbol + context was the prefix of another symbol, e.g.
                    //   s1 = "a" "b" "c".
                    //   s2 = "a" CONTEXT("b").
                    // But this is ok.
                    // if (t.state.endOf != null) {
                    //   Console.WriteLine("Ambiguous context clause");
                    //	 errors.count++;
                    // }
                }
            }
        }

        //------------------------- melted states ------------------------------

        Melted firstMelted; // head of melted state list

        Melted NewMelted(BitArray set, State state)
        {
            Melted m = new Melted(set, state);
            m.next = firstMelted; firstMelted = m;
            return m;
        }

        BitArray MeltedSet(int nr)
        {
            Melted m = firstMelted;
            while (m != null)
            {
                if (m.state.nr == nr) return m.set; else m = m.next;
            }
            throw new FatalError("compiler error in Melted.Set");
        }

        Melted StateWithSet(BitArray s)
        {
            for (Melted m = firstMelted; m != null; m = m.next)
                if (Sets.Equals(s, m.set)) return m;
            return null;
        }

        //------------------------ comments --------------------------------

        public Comment firstComment;    // list of comments

        string CommentStr(Node p)
        {
            StringBuilder s = new StringBuilder();
            while (p != null)
            {
                if (p.typ == NodeKind.chr)
                {
                    s.Append((char)p.val);
                }
                else if (p.typ == NodeKind.clas)
                {
                    CharSet set = tab.CharClassSet(p.val);
                    if (set.Elements() != 1) parser.SemErr(64, "character set contains more than 1 character");
                    s.Append((char)set.First());
                }
                else parser.SemErr(65, "comment delimiters may not be structured");
                p = p.next;
            }
            if (s.Length == 0 || s.Length > 2)
            {
                parser.SemErr(66, "comment delimiters must be 1 or 2 characters long");
                s = new StringBuilder("?");
            }
            return s.ToString();
        }

        public void NewComment(Node from, Node to, bool nested)
        {
            Comment c = new Comment(CommentStr(from), CommentStr(to), nested);
            c.next = firstComment; firstComment = c;
        }


        //------------------------ scanner generation ----------------------

        void GenComBody(Comment com)
        {
            gen.WriteLine("\t\t\tfor(;;) {");
            gen.Write("\t\t\t\tif ({0}) ", ChCond(com.stop[0])); gen.WriteLine("{");
            if (com.stop.Length == 1)
            {
                gen.WriteLine("\t\t\t\t\tlevel--;");
                gen.WriteLine("\t\t\t\t\tif (level == 0) { NextCh(); return true; }");
                gen.WriteLine("\t\t\t\t\tNextCh();");
            }
            else
            {
                gen.WriteLine("\t\t\t\t\tNextCh();");
                gen.WriteLine("\t\t\t\t\tif ({0}) {{", ChCond(com.stop[1]));
                gen.WriteLine("\t\t\t\t\t\tlevel--;");
                gen.WriteLine("\t\t\t\t\t\tif (level == 0) { NextCh(); return true; }");
                gen.WriteLine("\t\t\t\t\t\tNextCh();");
                gen.WriteLine("\t\t\t\t\t}");
            }
            if (com.nested)
            {
                gen.Write("\t\t\t\t}"); gen.Write(" else if ({0}) ", ChCond(com.start[0])); gen.WriteLine("{");
                if (com.start.Length == 1)
                    gen.WriteLine("\t\t\t\t\tlevel++; NextCh();");
                else
                {
                    gen.WriteLine("\t\t\t\t\tNextCh();");
                    gen.Write("\t\t\t\t\tif ({0}) ", ChCond(com.start[1])); gen.WriteLine("{");
                    gen.WriteLine("\t\t\t\t\t\tlevel++; NextCh();");
                    gen.WriteLine("\t\t\t\t\t}");
                }
            }
            gen.WriteLine("\t\t\t\t} else if (ch == EOF) return false;");
            gen.WriteLine("\t\t\t\telse NextCh();");
            gen.WriteLine("\t\t\t}");
        }

        void GenComment(Comment com, int i)
        {
            gen.WriteLine();
            gen.WriteLine("\t\tbool Cmt{0}(Position bookmark)", i); 
            gen.WriteLine("\t\t{");
            gen.WriteLine("\t\t\tif ({0}) return false;", ChCondNot(com.start[0]));
            gen.WriteLine("\t\t\tvar level = 1;");
            if (com.start.Length == 1)
            {
                gen.WriteLine("\t\t\tNextCh();");
                GenComBody(com);
            }
            else
            {
                gen.WriteLine("\t\t\tNextCh();");
                gen.Write("\t\t\tif ({0}) ", ChCond(com.start[1])); gen.WriteLine("{");
                gen.WriteLine("\t\t\t\tNextCh();");
                GenComBody(com);
                gen.WriteLine("\t\t\t} else");
                gen.WriteLine("\t\t\t\tbuffer.ResetPositionTo(bookmark);");
                gen.WriteLine("\t\t\treturn false;");
            }
            gen.WriteLine("\t\t}");
        }

        string SymName(Symbol sym)
        {
            if (Char.IsLetter(sym.name[0]))
            { // real name value is stored in Tab.literals
                foreach (var e in tab.literals)
                    if (e.Value == sym) return e.Key;
            }
            return sym.name;
        }

        void GenLiterals()
        {
            foreach (IList ts in new IList[] { tab.terminals, tab.pragmas })
            {
                foreach (Symbol sym in ts)
                {
                    if (sym.tokenKind == Symbol.litToken)
                    {
                        string name = SymName(sym);
                        // sym.name stores literals with quotes, e.g. "\"Literal\""
                        gen.WriteLine("\t\t\t\tcase {0}: t.kind = {1}; break;", parser.scanner.casingString(name), sym.n);
                    }
                }
            }
            gen.WriteLine("\t\t\t\tdefault: break;");
        }

        void WriteState(State state)
        {
            Symbol endOf = state.endOf;
            gen.WriteLine("\t\t\t\tcase {0}:", state.nr);
            if (endOf != null && state.firstAction != null)
            {
                gen.WriteLine("\t\t\t\t\trecEnd = buffer.PositionM1; recKind = {0};", endOf.n);
            }
            bool ctxEnd = state.ctx;
            for (Action action = state.firstAction; action != null; action = action.next)
            {
                if (action == state.firstAction) gen.Write("\t\t\t\t\tif (");
                else gen.Write("\t\t\t\t\telse if (");
                if (action.typ == NodeKind.chr) gen.Write(ChCond((char)action.sym));
                else PutRange(tab.CharClassSet(action.sym));
                gen.Write(") {");
                if (action.tc == NodeTransition.contextTrans)
                {
                    gen.Write("apx++; "); ctxEnd = false;
                }
                else if (state.ctx)
                    gen.Write("apx = 0; ");
                gen.Write("AddCh(); goto case {0};", action.target.state.nr);
                gen.WriteLine("}");
            }
            if (state.firstAction == null)
                gen.Write("\t\t\t\t\t{");
            else
                gen.Write("\t\t\t\t\telse {");
            if (ctxEnd)
            { // final context state: cut appendix
                gen.WriteLine();
                gen.WriteLine("\t\t\t\t\t\ttval.Length -= apx;");
                gen.WriteLine("\t\t\t\t\t\tSetScannerBehindT();");
                gen.Write("\t\t\t\t\t\t");
            }
            if (endOf == null)
            {
                gen.WriteLine("goto case 0;}");
            }
            else
            {
                gen.Write("t.kind = {0}; ", endOf.n);
                if (endOf.tokenKind == Symbol.classLitToken)
                {
                    gen.WriteLine("t.setValue(tval.ToString(), casingString); CheckLiteral(); return t.Freeze(buffer.Position);}");
                }
                else
                {
                    gen.WriteLine("break;}");
                }
            }
        }

        void WriteStartTab()
        {
            for (Action action = firstState.firstAction; action != null; action = action.next)
            {
                int targetState = action.target.state.nr;
                if (action.typ == NodeKind.chr)
                {
                    gen.WriteLine($"\t\t\tstart[{action.sym}] = {targetState}; ");
                }
                else
                {
                    CharSet s = tab.CharClassSet(action.sym);
                    for (CharSet.Range r = s.head; r != null; r = r.next)
                    {
                        gen.WriteLine($"\t\t\tfor (var i = {r.from}; i <= {r.to}; ++i) start[i] = {targetState};");
                    }
                }
            }
            gen.WriteLine("\t\t\tstart[EOF] = -1;");
        }

        public void WriteScanner()
        {
            Generator g = new Generator(tab);
            fram = g.OpenFrame("Scanner.frame");
            gen = g.OpenGen("Scanner.cs");
            if (dirtyDFA) MakeDeterministic();

            g.GenCopyright();
            g.SkipFramePart("-->begin");

            g.CopyFramePart("-->namespace");
            if (tab.nsName != null && tab.nsName.Length > 0)
            {
                gen.Write("namespace ");
                gen.Write(tab.nsName);
                gen.Write(" {");
            }

            g.CopyFramePart("-->declarations");
            gen.WriteLine("\tprivate const int _maxT = {0};", tab.terminals.Count - 1);
            gen.WriteLine("\tconst int noSym = {0};", tab.noSym.n);

            g.CopyFramePart("-->staticinitialization");
            WriteStartTab();

            g.CopyFramePart("-->initialization");
            if (ignoreCase) 
            {
                // defaults to identity if !ignoreCase
                gen.WriteLine("\t\tcasing = char.ToLowerInvariant;");
                gen.WriteLine("\t\tcasingString = ScannerBase.ToLower;");
            }

            g.CopyFramePart("-->comments");
            int comIdx = 0;
            for (var com = firstComment; com != null; com = com.next)
            {
                comIdx++;
                GenComment(com, comIdx);
            }

            g.CopyFramePart("-->literals"); 
            GenLiterals();

            g.CopyFramePart("-->scan1");
            gen.Write("\t\t\t\t");
            if (tab.ignored.Elements() > 0) 
                PutRange(tab.ignored); 
            else 
                gen.Write("false"); 
            
            g.CopyFramePart("-->scan2");
            if (firstComment != null)
            {
                gen.WriteLine("\t\t\tvar bm = buffer.PositionM1; // comment(s)");
                gen.Write("\t\t\tif (");
                comIdx = 0;
                for (var com = firstComment; com != null; com = com.next)
                {
                    comIdx++;
                    gen.Write("Cmt{0}(bm)", comIdx);
                    if (com.next != null) 
                        gen.Write(" || ");                    
                }
                gen.WriteLine(")");
                gen.Write("\t\t\t\treturn NextToken();");
            }

            if (hasCtxMoves) 
            { 
                gen.WriteLine(); 
                gen.Write("\t\tvar apx = 0;"); 
            } /* pdt */
            
            g.CopyFramePart("-->scan3");
            for (State state = firstState.next; state != null; state = state.next)
                WriteState(state);
            
            // -->end   copy frame up to it's end
            g.CopyFramePart(null);
            if (tab.nsName != null && tab.nsName.Length > 0) gen.Write("}");
            gen.Dispose();
        }

        public DFA(CocoRCore.CSharp.Parser parser)
        {
            this.parser = parser;
            tab = parser.tab;
            errors = parser.errors;
            trace = parser.trace;
            firstState = null; lastState = null; lastStateNr = -1;
            firstState = NewState();
            firstMelted = null; firstComment = null;
            ignoreCase = false;
            dirtyDFA = false;
            hasCtxMoves = false;
        }

    } // end DFA

} // end namespace
