namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //-----------------------------------------------------------------------------
    //  Target
    //-----------------------------------------------------------------------------

    public class Target
    {               // set of states that are reached by an action
        public State state;             // target state
        public Target next;

        public Target(State s)
        {
            state = s;
        }
    }

} // end namespace
