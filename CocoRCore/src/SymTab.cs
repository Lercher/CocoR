using System.Collections.Generic;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // SymTab - Symbol Table Declaration
    //=====================================================================

    public class SymTab
    {
        public readonly string name;
        public bool strict = false;
        public readonly List<string> predefined = new List<string>();

        public SymTab(string name) => this.name = name; 

        public void Add(string name)
        {
            if (!predefined.Contains(name)) 
                predefined.Add(name);
        }
    }

} // end namespace
