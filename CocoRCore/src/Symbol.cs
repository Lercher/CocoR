using System.Collections;
using System.Collections.Generic;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // Symbol
    //=====================================================================
    public enum TerminalTokenKind
    {
        fixedToken = 0, // e.g. 'a' ('b' | 'c') (structure of literals)
        classToken = 1,    // e.g. digit {digit}   (at least one char class)
        litToken = 2, // e.g. "while"
        classLitToken = 3 // e.g. letter {letter} but without literals that have the same structure
    }

    public class Symbol
    {
        public int n;           // symbol number
        public readonly NodeKind typ;         // t, nt, pr, unknown, rslv /* ML 29_11_2002 slv added */ /* AW slv --> rslv */
        public readonly string name;        // symbol name
        public string definedAs;     // t:  the definition of this terminal or its name (from parser.tokenString)
        public Node graph;       // nt: to first node of syntax graph
        public TerminalTokenKind tokenKind;   // t:  token kind (fixedToken, classToken, ...)
        public bool deletable;   // nt: true if nonterminal is deletable
        public bool deletableOK; // nt: true if we noticed and accepted that this nonterminal is deletable
        public bool firstReady;  // nt: true if terminal start symbols have already been computed
        public BitArray first;       // nt: terminal start symbols
        public BitArray follow;      // nt: terminal followers
        public BitArray nts;         // nt: nonterminals whose followers have to be added to this sym
        public Position pos;        // source text line number of item in this node
        public Range attrPos;     // nt: position of attributes in source text (or null)
        public Range semPos;      // pr: pos of semantic action in source text (or null)
                                  // nt: pos of local declarations in source text (or null)
        public Symbol inherits;    // optional, token from which this token derives
        public List<SymTab> scopes;  // nt: optional, list of SymTabs that this NT starts a new scope of
        public List<SymTab> usealls;  // nt: optional, list of SymTabs that all symbols must be used within
        public List<SymTab> useonces; // nt: optional, list of SymTabs that all symbols must be used at most once within
        public string astjoinwith;  // nt: optional, join a stack top list of ASTLiterals to a single ASTLiteral (+".")

        public Symbol(NodeKind typ, string name, Position pos)
        {
            this.typ = typ;
            this.name = name;
            this.pos = pos;  // mutates sometimes
            definedAs = name; // mutates sometimes
        }

        public string CSharpCommentName
        {
            get
            {
                var s = VariantName;
                if (s.Contains("*")) s = s.Replace('*', '_');
                return $"/*{s}*/";
            }
        }

        public string VariantName
        {
            get
            {
                switch (typ)
                {
                    case NodeKind.t:
                    case NodeKind.wt:
                        if (definedAs.StartsWith("\""))
                            return definedAs.Substring(1, definedAs.Length - 2);
                        return $"[{definedAs}]";
                    default:
                        return definedAs;
                }
            }
        }

        public override string ToString() 
            => $"{VariantName}{pos}";
    }
}
