namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // CharClass
    //=====================================================================

    public class CharClass
    {
        public int n;           // class number
        public string name;     // class name
        public CharSet set; // set representing the class

        public CharClass(string name, CharSet s)
        {
            this.name = name; this.set = s;
        }
    }

} // end namespace
