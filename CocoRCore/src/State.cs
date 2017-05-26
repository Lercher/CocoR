namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //-----------------------------------------------------------------------------
    //  State
    //-----------------------------------------------------------------------------

    public class State
    {               // state of finite automaton
        public int nr;                      // state number
        public Action firstAction;// to first action of this state
        public Symbol endOf;            // recognized token if state is final
        public bool ctx;                    // true if state is reached via contextTrans
        public State next;

        public void AddAction(Action act)
        {
            Action lasta = null, a = firstAction;
            while (a != null && act.typ >= a.typ) { lasta = a; a = a.next; }
            // collecting classes at the beginning gives better performance
            act.next = a;
            if (a == firstAction) firstAction = act; else lasta.next = act;
        }

        public void DetachAction(Action act)
        {
            Action lasta = null, a = firstAction;
            while (a != null && a != act) { lasta = a; a = a.next; }
            if (a != null)
                if (a == firstAction) firstAction = a.next; else lasta.next = a.next;
        }

        public void MeltWith(State s)
        { // copy actions of s to state
            for (Action action = s.firstAction; action != null; action = action.next)
            {
                Action a = new Action(action.typ, action.sym, action.tc);
                a.AddTargets(action);
                AddAction(a);
            }
        }

    }

} // end namespace
