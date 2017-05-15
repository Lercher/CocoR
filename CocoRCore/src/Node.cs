using System.Collections;
using System.Collections.Generic;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // Node
    //=====================================================================
    public enum NodeKind // constants for node kinds
    {
        t = 1,  // terminal symbol
        pr = 2,  // pragma
        nt = 3,  // nonterminal symbol
        clas = 4,  // character class
        chr = 5,  // character
        wt = 6,  // weak terminal symbol
        any = 7,  // any 
        eps = 8,  // empty
        sync = 9,  // synchronization symbol
        sem = 10,  // semantic action: (. .)
        alt = 11,  // alternative: |
        iter = 12,  // iteration: { }
        opt = 13,  // option: [ ]
        rslv = 14  // resolver expr
    }

    public class Node
    {
        public const int normalTrans = 0;       // transition codes
        public const int contextTrans = 1;

        public int n;           // node number
        public NodeKind typ;     // t, nt, wt, chr, clas, any, eps, sem, sync, alt, iter, opt, rslv
        public Node next;       // to successor node
        public Node down;       // alt: to next alternative
        public Node sub;        // alt, iter, opt: to first node of substructure
        public bool up;         // true: "next" leads to successor in enclosing structure
        public Symbol sym;      // nt, t, wt: symbol represented by this node
        public int val;     // chr:  ordinal character value
                            // clas: index of character class
        public int code;        // chr, clas: transition code
        public BitArray set;        // any, sync: the set represented by this node
        public Position pos;        // nt, t, wt: pos of actual attributes
                                    // sem:       pos of semantic action in source text
                                    // rslv:       pos of resolver in source text
        public int line;        // source text line number of item in this node
        public State state; // DFA state corresponding to this node
                            // (only used in DFA.ConvertToStates)
        public string declares;    // t, wt: the symbol declares a new entry to the symboltable with this name
        public string declared;    // t, wt: the symbol has to be declared in the symboltable with this name
        public List<AstOp> asts;         // nt, t, wt: AST Operations, # ## ^ ^^ +

        public Node(NodeKind typ, Symbol sym, int line)
        {
            this.typ = typ;
            this.sym = sym;
            this.line = line;
        }

        public AstOp addAstOp()
        {
            AstOp ao = new AstOp();
            asts.Add(ao);
            return ao;
        }
    }

} // end namespace
