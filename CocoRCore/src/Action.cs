using System.Linq;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //-----------------------------------------------------------------------------
    //  Action
    //-----------------------------------------------------------------------------

    public class Action
    {           
        // action of finite automaton

        public NodeKind typ;                 // type of action symbol: clas, chr
        public int sym;                 // action symbol
        public NodeTransition tc;                  // transition code: normalTrans, contextTrans
        public Target target;       // states reached from this action
        public Action next;

        public Action(NodeKind typ, int sym, NodeTransition tc)
        {
            this.typ = typ; 
            this.sym = sym; 
            this.tc = tc;
        }

        public void AddTarget(Target t)
        { // add t to the action.targets
            Target last = null;
            var p = target;
            while (p != null && t.state.nr >= p.state.nr)
            {
                if (t.state == p.state) return;
                last = p; p = p.next;
            }
            t.next = p;
            if (p == target)
                target = t;
            else
                last.next = t;
        }

        public void AddTargets(Action a)
        { // add copy of a.targets to action.targets
            for (var p = a.target; p != null; p = p.next)
            {
                var t = new Target() { state = p.state };
                AddTarget(t);
            }
            if (a.tc == NodeTransition.contextTrans) tc = NodeTransition.contextTrans;
        }

        public CharSet Symbols(Tab tab)
        {
            CharSet s;
            if (typ == NodeKind.clas)
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
                typ = NodeKind.chr; sym = s.First();
            }
            else
            {
                var c = tab.FindCharClass(s);
                if (c == null) c = tab.NewCharClass("#", s); // class with dummy name
                typ = NodeKind.clas; sym = c.n;
            }
        }

    }

} // end namespace
