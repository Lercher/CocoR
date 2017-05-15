namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // AstOp AST Operation
    //=====================================================================

    public class AstOp
    {
        public bool isList = false;
        public bool primed = false;
        public bool ishatch = true; // t - # or ## (hatch), f - ^ or ^^ (sendup) 
        public string name = null;
        public string literal = null;
    }

} // end namespace
