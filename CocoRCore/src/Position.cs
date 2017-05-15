namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    public class Position
    {  // position of source code stretch (e.g. semantic action, resolver expressions)
        public readonly int beg;      // start relative to the beginning of the file
        public readonly int end;      // end of stretch
        public readonly int col;      // column number of start position
        public readonly int line;     // line number of start position

        public Position(int beg, int end, int col, int line)
        {
            this.beg = beg; this.end = end; this.col = col; this.line = line;
        }
    }

} // end namespace
