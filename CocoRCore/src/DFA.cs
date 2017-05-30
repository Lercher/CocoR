using System;
using System.IO;
using System.Linq;
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
        private TextWriter gen;  // generated scanner file
        private Symbol curSy;      // current token to be recognized (in FindTrans)
        private bool dirtyDFA;     // DFA may become nondeterministic in MatchLiteral

        public bool ignoreCase;   // true if input should be treated case-insensitively
        public bool hasCtxMoves;  // DFA has context transitions

        // other Coco objects
        public readonly Parser Parser;                    // other Coco objects
        private TextWriter Trace => Parser.trace;
        private Tab Tab => Parser.tab;

        //---------- Output primitives
        public static string Ch(int ch)
        {
            if (ch < ' ' || ch >= 127 || ch == '\'' || ch == '\\')
                return ch.ToString();
            return $"'{(char)ch}'";
        }

        private static string ChCond(char ch) => $"ch == {Ch(ch)}";
        private static string ChCondNot(char ch) => $"ch != {Ch(ch)}";

        private void PutRange(CharSet s)
        {            
            for (var r = s.head; r != null; r = r.next)
            {
                if (r.from == r.to)
                    gen.Write($"ch == {Ch(r.from)}");
                else if (r.from == 0)
                    gen.Write($"ch <= {Ch(r.to)}"); 
                else
                    gen.Write($"{Ch(r.from)} <= ch && ch <= {Ch(r.to)}"); 
                if (r.next != null)
                    gen.Write(" || ");
            }
        }

        //---------- State handling

        State NewState()
        {
            var s = new State( nr: ++lastStateNr );
            if (firstState == null)
                firstState = s;
            else
                lastState.next = s;
            lastState = s;
            return s;
        }

        void NewTransition(State from, State to, NodeKind typ, int sym, NodeTransition tc)
        {
            var t = new Target() { state = to };
            var a = new Action(typ, sym, tc) { target = t };
            from.AddAction(a);
            if (typ == NodeKind.clas) curSy.tokenKind = TerminalTokenKind.classToken;
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
                            seta = a.Symbols(Tab); setb = b.Symbols(Tab);
                            seta.Or(setb);
                            a.ShiftWith(seta, Tab);
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
            for (var a = state.firstAction; a != null; a = a.next)
                FindUsedStates(a.target.state, used);
        }

        void DeleteRedundantStates()
        {
            var newState = new State[lastStateNr + 1];
            var used = new BitArray(lastStateNr + 1);
            FindUsedStates(firstState, used);
            // combine equal final states
            for (var s1 = firstState.next; s1 != null; s1 = s1.next) // firstState cannot be final
                if (used[s1.nr] && s1.endOf != null && s1.firstAction == null && !s1.ctx)
                    for (var s2 = s1.next; s2 != null; s2 = s2.next)
                        if (used[s2.nr] && s1.endOf == s2.endOf && s2.firstAction == null & !s2.ctx)
                        {
                            used[s2.nr] = false;
                            newState[s2.nr] = s1;
                        }

            for (var state = firstState; state != null; state = state.next)
                if (used[state.nr])
                    for (var a = state.firstAction; a != null; a = a.next)
                        if (!used[a.target.state.nr])
                            a.target.state = newState[a.target.state.nr];

            // delete unused states
            lastState = firstState;
            lastStateNr = 0; // firstState has number 0
            for (var state = firstState.next; state != null; state = state.next) // firstState cannot be final
                if (used[state.nr])
                {
                    state.nr = ++lastStateNr;
                    lastState = state;
                }
                else
                    lastState.next = state.next;
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
                            Parser.SemErr(61, "contents of {...} must not be deletable");
                            return;
                        }
                        if (p.next != null && !stepped[p.next.n]) Step(from, p.next, stepped);
                        Step(from, p.sub, stepped);
                        if (p.state != from)
                        {
                            Step(p.state, p, new BitArray(Tab.nodes.Count));
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
            if (start) Step(p.state, p, new BitArray(Tab.nodes.Count)); // start of group of equally numbered nodes
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
                Parser.SemErr(62, "token might be empty");
                return;
            }
            NumberNodes(p, firstState, true);
            FindTrans(p, true, new BitArray(Tab.nodes.Count));
            if (p.typ == NodeKind.iter)
            {
                Step(firstState, p, new BitArray(Tab.nodes.Count));
            }
        }

        // match string against current automaton; store it either as a fixedToken or as a litToken
        public void MatchLiteral(string s, Symbol sym)
        {
            s = Tab.Unescape(s.Substring(1, s.Length - 2));
            int i, len = s.Length;
            var state = firstState;
            Action a = null;
            for (i = 0; i < len; i++)
            { // try to match s against existing DFA
                a = FindAction(state, s[i]);
                if (a == null)
                    break;
                state = a.target.state;
            }
            // if s was not totally consumed or leads to a non-final state => make new DFA from it
            if (i != len || state.endOf == null)
            {
                state = firstState;
                i = 0;
                a = null;
                dirtyDFA = true;
            }
            for (; i < len; i++)
            { // make new DFA for s[i..len-1], ML: i is either 0 or len
                var to = NewState();
                NewTransition(state, to, NodeKind.chr, s[i], NodeTransition.normalTrans);
                state = to;
            }
            var matchedSym = state.endOf;
            if (state.endOf == null)
            {
                state.endOf = sym;
            }
            else if (matchedSym.tokenKind == TerminalTokenKind.fixedToken || (a != null && a.tc == NodeTransition.contextTrans))
            {
                // s matched a token with a fixed definition or a token with an appendix that will be cut off
                Parser.SemErr(63, $"tokens {sym.name} and {matchedSym.name} cannot be distinguished");
            }
            else
            { // matchedSym == classToken || classLitToken
                matchedSym.tokenKind = TerminalTokenKind.classLitToken;
                sym.tokenKind = TerminalTokenKind.litToken;
            }
        }

        void SplitActions(State state, Action a, Action b)
        {
            Action c; CharSet seta, setb, setc;
            seta = a.Symbols(Tab); setb = b.Symbols(Tab);
            if (seta.Equals(setb))
            {
                a.AddTargets(b);
                state.DetachAction(b);
            }
            else if (seta.Includes(setb))
            {
                setc = seta.Clone(); setc.Subtract(setb);
                b.AddTargets(a);
                a.ShiftWith(setc, Tab);
            }
            else if (setb.Includes(seta))
            {
                setc = setb.Clone(); setc.Subtract(seta);
                a.AddTargets(b);
                b.ShiftWith(setc, Tab);
            }
            else
            {
                setc = seta.Clone(); setc.And(setb);
                seta.Subtract(setc);
                setb.Subtract(setc);
                a.ShiftWith(seta, Tab);
                b.ShiftWith(setb, Tab);
                c = new Action(0, 0, NodeTransition.normalTrans);  // typ and sym are set in ShiftWith
                c.AddTargets(a);
                c.AddTargets(b);
                c.ShiftWith(setc, Tab);
                state.AddAction(c);
            }
        }

        bool Overlap(Action a, Action b)
        {
            CharSet seta, setb;
            if (a.typ == NodeKind.chr)
                if (b.typ == NodeKind.chr) return a.sym == b.sym;
                else { setb = Tab.CharClassSet(b.sym); return setb[a.sym]; }
            else
            {
                seta = Tab.CharClassSet(a.sym);
                if (b.typ == NodeKind.chr) return seta[b.sym];
                else { setb = Tab.CharClassSet(b.sym); return seta.Intersects(setb); }
            }
        }

        void MakeUnique(State state)
        {
            bool changed;
            do
            {
                changed = false;
                for (var a = state.firstAction; a != null; a = a.next)
                    for (var b = a.next; b != null; b = b.next)
                        if (Overlap(a, b))
                        {
                            SplitActions(state, a, b);
                            changed = true;
                        }
            } while (changed);
        }

        void MeltStates(State state)
        {
            for (var action = state.firstAction; action != null; action = action.next)
            {
                if (action.target.next != null)
                {
                    GetTargetStates(action, out var targets, out var endOf, out var ctx);
                    var melt = StateWithSet(targets);
                    if (melt == null)
                    {
                        var s = NewState(); s.endOf = endOf; s.ctx = ctx;
                        for (var targ = action.target; targ != null; targ = targ.next)
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
            for (var state = firstState; state != null; state = state.next)
                for (var a = state.firstAction; a != null; a = a.next)
                    if (a.tc == NodeTransition.contextTrans)
                        a.target.state.ctx = true;
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
            Trace.WriteLine();
            Trace.WriteLine("---------- states ----------");
            for (var state = firstState; state != null; state = state.next)
            {
                var first = true;
                if (state.endOf == null)
                    Trace.Write("               ");
                else
                    Trace.Write("E({0,12})", Tab.Name12(state.endOf.name));
                Trace.Write("{0,3}:", state.nr);
                if (state.firstAction == null)
                    Trace.WriteLine();
                for (var action = state.firstAction; action != null; action = action.next)
                {
                    if (first)
                    {
                        Trace.Write(" ");
                        first = false;
                    }
                    else
                        Trace.Write("                    ");
                    if (action.typ == NodeKind.clas)
                        Trace.Write(Tab.classes[action.sym].name);
                    else
                        Trace.Write("{0, 3}", Ch(action.sym));
                    for (var targ = action.target; targ != null; targ = targ.next)
                        Trace.Write(" {0, 3}", targ.state.nr);
                    if (action.tc == NodeTransition.contextTrans)
                        Trace.Write(" context");
                    Trace.WriteLine();
                }
            }
            Trace.WriteLine();
            Trace.WriteLine("---------- character classes ----------");
            Tab.WriteCharClasses();
        }

        //---------------------------- actions --------------------------------

        public Action FindAction(State state, char ch)
        {
            for (var a = state.firstAction; a != null; a = a.next)
                if (a.typ == NodeKind.chr && ch == a.sym)
                    return a;
                else if (a.typ == NodeKind.clas)
                {
                    var s = Tab.CharClassSet(a.sym);
                    if (s[ch])
                        return a;
                }
            return null;
        }

        public void GetTargetStates(Action a, out BitArray targets, out Symbol endOf, out bool ctx)
        {
            // compute the set of target states
            targets = new BitArray(maxStates);
            endOf = null;
            ctx = false;
            for (var t = a.target; t != null; t = t.next)
            {
                var stateNr = t.state.nr;
                if (stateNr <= lastSimState)
                    targets[stateNr] = true;
                else
                    targets.Or(MeltedSet(stateNr));
                if (t.state.endOf != null)
                    if (endOf == null || endOf == t.state.endOf)
                        endOf = t.state.endOf;
                    else
                        Parser.errors.SemErr(endOf.pos, $"Tokens {endOf.name} and {t.state.endOf.name} cannot be distinguished", 67);
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
            var m = new Melted(set, state, next: firstMelted);
            firstMelted = m;
            return m;
        }

        BitArray MeltedSet(int nr)
        {
            
            for (var m = firstMelted; m != null; m = m.next)
                if (m.state.nr == nr)
                    return m.set;
            throw new FatalError("compiler error in Melted.Set");
        }

        Melted StateWithSet(BitArray s)
        {
            for (var m = firstMelted; m != null; m = m.next)
                if (Sets.Equals(s, m.set))
                    return m;
            return null;
        }

        //------------------------ comments --------------------------------

        public Comment firstComment;    // list of comments

        string CommentStr(Node p)
        {
            var s = new StringBuilder();
            while (p != null)
            {
                if (p.typ == NodeKind.chr)
                {
                    s.Append((char)p.val);
                }
                else if (p.typ == NodeKind.clas)
                {
                    var set = Tab.CharClassSet(p.val);
                    if (set.Elements() != 1)
                        Parser.SemErr(64, "character set contains more than 1 character");
                    s.Append((char)set.First());
                }
                else Parser.SemErr(65, "comment delimiters may not be structured");
                p = p.next;
            }
            if (s.Length == 0 || s.Length > 2)
            {
                Parser.SemErr(66, "comment delimiters must be 1 or 2 characters long");
                s = new StringBuilder("?");
            }
            return s.ToString();
        }

        public void NewComment(Node from, Node to, bool nested)
        {
            var c = new Comment(CommentStr(from), CommentStr(to), nested) { next = firstComment };
            firstComment = c;
        }


        //------------------------ scanner generation ----------------------

        void GenComBody(Comment com)
        {
            gen.WriteLine("\t\t\t\tfor(;;) {");
            gen.Write("\t\t\t\t\tif ({0}) ", ChCond(com.stop[0])); gen.WriteLine("{");
            if (com.stop.Length == 1)
            {
                gen.WriteLine("\t\t\t\t\t\tlevel--;");
                gen.WriteLine("\t\t\t\t\t\tif (level == 0) { NextCh(); return true; }");
                gen.WriteLine("\t\t\t\t\t\tNextCh();");
            }
            else
            {
                gen.WriteLine("\t\t\t\t\t\tNextCh();");
                gen.WriteLine("\t\t\t\t\t\tif ({0}) {{", ChCond(com.stop[1]));
                gen.WriteLine("\t\t\t\t\t\t\tlevel--;");
                gen.WriteLine("\t\t\t\t\t\t\tif (level == 0) { NextCh(); return true; }");
                gen.WriteLine("\t\t\t\t\t\t\tNextCh();");
                gen.WriteLine("\t\t\t\t\t\t}");
            }
            if (com.nested)
            {
                gen.Write("\t\t\t\t\t}"); gen.Write(" else if ({0}) ", ChCond(com.start[0])); gen.WriteLine("{");
                if (com.start.Length == 1)
                    gen.WriteLine("\t\t\t\t\t\tlevel++; NextCh();");
                else
                {
                    gen.WriteLine("\t\t\t\t\t\tNextCh();");
                    gen.Write("\t\t\t\t\t\tif ({0}) ", ChCond(com.start[1])); gen.WriteLine("{");
                    gen.WriteLine("\t\t\t\t\t\t\tlevel++; NextCh();");
                    gen.WriteLine("\t\t\t\t\t\t}");
                }
            }
            gen.WriteLine("\t\t\t\t\t} else if (ch == EOF) return false;");
            gen.WriteLine("\t\t\t\t\telse NextCh();");
            gen.WriteLine("\t\t\t\t}");
        }

        void GenComment(Comment com, int i)
        {
            gen.WriteLine();
            gen.WriteLine("\t\tbool Cmt{0}(Position bm)", i);
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
                gen.WriteLine("\t\t\t\tbuffer.ResetPositionTo(bm);");
                gen.WriteLine("\t\t\treturn false;");
            }
            gen.WriteLine("\t\t}");
        }

        string SymName(Symbol sym)
        {
            if (char.IsLetter(sym.name[0]))
            { // real name value is stored in Tab.literals
                foreach (var e in Tab.literals)
                    if (e.Value == sym) return e.Key;
            }
            return sym.name;
        }

        void GenLiterals()
        {
            foreach (var sym in Tab.terminals.Concat(Tab.pragmas))
            {
                if (sym.tokenKind == TerminalTokenKind.litToken)
                {
                    var name = SymName(sym);
                    // sym.name stores literals with quotes, e.g. "\"Literal\""
                    gen.WriteLine("\t\t\t\tcase {0}: t.kind = {1}; break;", Parser.scanner.casingString(name), sym.n);
                }
            }
            gen.WriteLine("\t\t\t\tdefault: break;");
        }

        void WriteState(State state)
        {
            var endOf = state.endOf;
            gen.WriteLine("\t\t\t\tcase {0}:", state.nr);
            if (endOf != null && state.firstAction != null)
            {
                gen.WriteLine("\t\t\t\t\trecEnd = buffer.Position; recKind = {0};", endOf.n);
            }
            var ctxEnd = state.ctx;
            for (var action = state.firstAction; action != null; action = action.next)
            {
                if (action == state.firstAction)
                    gen.Write("\t\t\t\t\tif (");
                else
                    gen.Write("\t\t\t\t\telse if (");
                if (action.typ == NodeKind.chr)
                    gen.Write(ChCond((char)action.sym));
                else
                    PutRange(Tab.CharClassSet(action.sym));
                gen.Write(") {");
                if (action.tc == NodeTransition.contextTrans)
                {
                    gen.Write("apx++; ");
                    ctxEnd = false;
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
                gen.WriteLine("\t\t\t\t\t\tSetScannerBackBehindT();");
                gen.Write("\t\t\t\t\t\t");
            }
            if (endOf == null)
            {
                gen.WriteLine("goto case 0;}");
            }
            else
            {
                gen.Write("t.kind = {0}; ", endOf.n);
                if (endOf.tokenKind == TerminalTokenKind.classLitToken)
                {
                    gen.WriteLine("t.setValue(tval.ToString(), casingString); CheckLiteral(); return t.Freeze(buffer.Position, buffer.PositionM1);}");
                }
                else
                {
                    gen.WriteLine("break;}");
                }
            }
        }

        void WriteStartTab()
        {
            for (var action = firstState.firstAction; action != null; action = action.next)
            {
                var targetState = action.target.state.nr;
                if (action.typ == NodeKind.chr)
                {
                    gen.WriteLine($"\t\t\tstart[{action.sym}] = {targetState}; ");
                }
                else
                {
                    var s = Tab.CharClassSet(action.sym);
                    for (var r = s.head; r != null; r = r.next)
                    {
                        gen.WriteLine($"\t\t\tfor (var i = {r.from}; i <= {r.to}; ++i) start[i] = {targetState};");
                    }
                }
            }
            gen.WriteLine("\t\t\tstart[EOF] = -1;");
        }

        public void WriteScanner()
        {
            using (var g = new Generator(Tab))
            {
                g.OpenFrame("Scanner.frame");
                gen = g.OpenGen("Scanner.cs");
                if (dirtyDFA)
                    MakeDeterministic();

                g.SkipFramePart("-->begin");

                g.CopyFramePart("-->namespace");
                if (Tab.nsName != null && Tab.nsName.Length > 0)
                {
                    gen.Write("namespace ");
                    gen.Write(Tab.nsName);
                    gen.Write(" {");
                }

                g.CopyFramePart("-->declarations");
                gen.WriteLine("\tprivate const int _maxT = {0};", Tab.terminals.Count - 1);
                gen.WriteLine("\tconst int noSym = {0};", Tab.noSym.n);

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
                var comIdx = 0;
                for (var com = firstComment; com != null; com = com.next)
                {
                    comIdx++;
                    GenComment(com, comIdx);
                }

                g.CopyFramePart("-->literals");
                GenLiterals();

                g.CopyFramePart("-->scan1");
                if (Tab.ignored.Elements() > 0) {
                    gen.Write("\t\t\t\t|| ");
                    PutRange(Tab.ignored);
                    gen.WriteLine();
                }
                    

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
                    gen.WriteLine("\t\t\t\treturn NextToken();");
                }

                if (hasCtxMoves)
                {
                    gen.WriteLine();
                    gen.WriteLine("\t\tvar apx = 0;");
                } /* pdt */

                g.CopyFramePart("-->scan3");
                for (var state = firstState.next; state != null; state = state.next) // firstState cannot be final
                    WriteState(state);

                // -->end   copy frame up to it's end
                g.CopyFramePart(null);
                if (Tab.nsName != null && Tab.nsName.Length > 0)
                    gen.Write("}");
            }
        }

        public DFA(CocoRCore.CSharp.Parser parser)
        {
            Parser = parser;
            firstState = null;
            lastState = null;
            lastStateNr = -1;
            firstState = NewState();
            firstMelted = null;
            firstComment = null;
            ignoreCase = false;
            dirtyDFA = false;
            hasCtxMoves = false;
        }

    } // end DFA

} // end namespace
