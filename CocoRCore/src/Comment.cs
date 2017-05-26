namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //-----------------------------------------------------------------------------
    //  Comment
    //-----------------------------------------------------------------------------

    public class Comment
    {                   // info about comment syntax
        public string start;
        public string stop;
        public bool nested;
        public Comment next;

        public Comment(string start, string stop, bool nested)
        {
            this.start = start; this.stop = stop; this.nested = nested;
        }

    }

} // end namespace
