/*-------------------------------------------------------------------------
Tab.cs -- Symbol Table Management
Compiler Generator Coco/R,
Copyright (c) 1990, 2004 Hanspeter Moessenboeck, University of Linz
extended by M. Loeberbauer & A. Woess, Univ. of Linz
with improvements by Pat Terry, Rhodes University

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 2, or (at your option) any 
later version.

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License 
for more details.

You should have received a copy of the GNU General Public License along 
with this program; if not, write to the Free Software Foundation, Inc., 
59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.

As an exception, it is allowed to write an extension of Coco/R that is
used as a plugin in non-free software.

If not otherwise stated, any source code generated by Coco/R (other than 
Coco/R itself) does not fall under the GNU General Public License.
-------------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // Node
    //=====================================================================

    public class Node
    {
        // constants for node kinds
        public const int t = 1;  // terminal symbol
        public const int pr = 2;  // pragma
        public const int nt = 3;  // nonterminal symbol
        public const int clas = 4;  // character class
        public const int chr = 5;  // character
        public const int wt = 6;  // weak terminal symbol
        public const int any = 7;  // 
        public const int eps = 8;  // empty
        public const int sync = 9;  // synchronization symbol
        public const int sem = 10;  // semantic action: (. .)
        public const int alt = 11;  // alternative: |
        public const int iter = 12;  // iteration: { }
        public const int opt = 13;  // option: [ ]
        public const int rslv = 14;  // resolver expr

        public const int normalTrans = 0;       // transition codes
        public const int contextTrans = 1;

        public int n;           // node number
        public int typ;     // t, nt, wt, chr, clas, any, eps, sem, sync, alt, iter, opt, rslv
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

        public Node(int typ, Symbol sym, int line)
        {
            this.typ = typ; this.sym = sym; this.line = line;
        }

        public AstOp addAstOp()
        {
            AstOp ao = new AstOp();
            asts.Add(ao);
            return ao;
        }
    }

} // end namespace
