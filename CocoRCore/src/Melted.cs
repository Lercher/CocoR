using System.Collections;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //-----------------------------------------------------------------------------
    //  Melted
    //-----------------------------------------------------------------------------

    public class Melted
    {                   // info about melted states
        public BitArray set;                // set of old states
        public State state;                 // new state
        public Melted next;

        public Melted(BitArray set, State state)
        {
            this.set = set; this.state = state;
        }
    }

} // end namespace
