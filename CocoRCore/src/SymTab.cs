using System.Collections.Generic;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // SymTab
    //=====================================================================

    public class SymTab
    {
        public string name;
        public bool strict = false;
        public List<string> predefined = new List<string>();

        public SymTab(string name) { this.name = name; }

        public void Add(string name)
        {
            if (!predefined.Contains(name)) predefined.Add(name);
        }
    }

} // end namespace
