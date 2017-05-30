using System.Collections;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //-----------------------------------------------------------------------------
    //  Melted
    //-----------------------------------------------------------------------------

    public class Melted
    {                   // info about melted states
        public readonly BitArray set;                // set of old states
        public readonly State state;                 // new state
        public readonly Melted next;

        public Melted(BitArray set, State state, Melted next)
        {
            this.set = set;
            this.state = state;
            this.next = next;
        }
    }

} // end namespace
