namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //-----------------------------------------------------------------------------
    //  Action
    //-----------------------------------------------------------------------------

    public class Action
    {           // action of finite automaton
        public int typ;                 // type of action symbol: clas, chr
        public int sym;                 // action symbol
        public int tc;                  // transition code: normalTrans, contextTrans
        public Target target;       // states reached from this action
        public Action next;

        public Action(int typ, int sym, int tc)
        {
            this.typ = typ; this.sym = sym; this.tc = tc;
        }

        public void AddTarget(Target t)
        { // add t to the action.targets
            Target last = null;
            Target p = target;
            while (p != null && t.state.nr >= p.state.nr)
            {
                if (t.state == p.state) return;
                last = p; p = p.next;
            }
            t.next = p;
            if (p == target) target = t; else last.next = t;
        }

        public void AddTargets(Action a)
        { // add copy of a.targets to action.targets
            for (Target p = a.target; p != null; p = p.next)
            {
                Target t = new Target(p.state);
                AddTarget(t);
            }
            if (a.tc == Node.contextTrans) tc = Node.contextTrans;
        }

        public CharSet Symbols(Tab tab)
        {
            CharSet s;
            if (typ == Node.clas)
                s = tab.CharClassSet(sym).Clone();
            else
            {
                s = new CharSet(); s.Set(sym);
            }
            return s;
        }

        public void ShiftWith(CharSet s, Tab tab)
        {
            if (s.Elements() == 1)
            {
                typ = Node.chr; sym = s.First();
            }
            else
            {
                CharClass c = tab.FindCharClass(s);
                if (c == null) c = tab.NewCharClass("#", s); // class with dummy name
                typ = Node.clas; sym = c.n;
            }
        }

    }

} // end namespace
