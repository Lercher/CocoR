using System.Collections;
using System.Collections.Generic;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // Symbol
    //=====================================================================

    public class Symbol
    {

        // token kinds
        public const int fixedToken = 0; // e.g. 'a' ('b' | 'c') (structure of literals)
        public const int classToken = 1;    // e.g. digit {digit}   (at least one char class)
        public const int litToken = 2; // e.g. "while"
        public const int classLitToken = 3; // e.g. letter {letter} but without literals that have the same structure

        public int n;           // symbol number
        public NodeKind typ;         // t, nt, pr, unknown, rslv /* ML 29_11_2002 slv added */ /* AW slv --> rslv */
        public string name;        // symbol name
        public string definedAs;     // t:  the definition of this terminal or its name
        public Node graph;       // nt: to first node of syntax graph
        public int tokenKind;   // t:  token kind (fixedToken, classToken, ...)
        public bool deletable;   // nt: true if nonterminal is deletable
        public bool firstReady;  // nt: true if terminal start symbols have already been computed
        public BitArray first;       // nt: terminal start symbols
        public BitArray follow;      // nt: terminal followers
        public BitArray nts;         // nt: nonterminals whose followers have to be added to this sym
        public int line;        // source text line number of item in this node
        public Position attrPos;     // nt: position of attributes in source text (or null)
        public Position semPos;      // pr: pos of semantic action in source text (or null)
                                     // nt: pos of local declarations in source text (or null)
        public Symbol inherits;    // optional, token from which this token derives
        public List<SymTab> scopes;  // nt: optional, list of SymTabs that this NT starts a new scope of
        public List<SymTab> usealls;  // nt: optional, list of SymTabs that all symbols must be used within
        public List<SymTab> useonces; // nt: optional, list of SymTabs that all symbols must be used at most once within
        public string astjoinwith;  // nt: optional, join a stack top list of ASTLiterals to a single ASTLiteral (+".")

        public Symbol(NodeKind typ, string name, int line)
        {
            this.typ = typ; 
            this.name = name; 
            this.line = line;
        }
    }

} // end namespace
