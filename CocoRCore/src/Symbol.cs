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
        public int typ;         // t, nt, pr, unknown, rslv /* ML 29_11_2002 slv added */ /* AW slv --> rslv */
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

        public Symbol(int typ, string name, int line)
        {
            this.typ = typ; this.name = name; this.line = line;
        }
    }

} // end namespace
