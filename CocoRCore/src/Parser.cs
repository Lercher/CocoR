using System.IO;

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using CocoRCore;

namespace CocoRCore.CSharp
{

    public class Parser : ParserBase 
    {
        public const int _ident = 1; // TOKEN ident
        public const int _number = 2; // TOKEN number
        public const int _string = 3; // TOKEN string
        public const int _badString = 4; // TOKEN badString
        public const int _char = 5; // TOKEN char
        public const int _prime = 6; // TOKEN prime
        private const int __maxT = 52;
        public const int _ddtSym = 53;
        public const int _optionSym = 54;
        private const bool _T = true;
        private const bool _x = false;
        
        public Symboltable symbols(string name)
        {
            return null;
        }


const int id = 0;
    const int str = 1;
    
    public TextWriter trace;    // other Coco objects referenced in this ATG
    public Tab tab;
    public DFA dfa;
    public ParserGen pgen;

    bool   genScanner;
    string tokenString;         // used in declarations of literal tokens
    string noString = "-none-"; // used in declarations of literal tokens

    public override void Dispose() 
    {
        trace?.Dispose();
        base.Dispose();
    }

/*-------------------------------------------------------------------------*/


        public Parser()
        {
        }

        public static Parser Create(string fileName) 
            => Create(s => s.Initialize(fileName));

        public static Parser Create() 
            => Create(s => { });

        public static Parser Create(Action<Scanner> init)
        {
            var p = new Parser();
            var scanner = new Scanner();
            p.Initialize(scanner);
            init(scanner);
            return p;
        }


        public override int maxT => __maxT;

        protected override void Get() 
        {
            lb = t;
            t = la;
            for (;;) 
            {
                la = scanner.Scan();
                if (la.kind <= maxT) 
                { 
                    ++errDist; 
                    break; // it's not a pragma
                }
                // pragma code
                if (la.kind == 53) // pragmas don't inherit kinds
                {
                    tab.SetDDT(la.val);
                }
                if (la.kind == 54) // pragmas don't inherit kinds
                {
                    tab.SetOption(la.val);
                }
            }
        }


        void Coco‿NT()
        {
            {
                string gramName; CharSet s;
                if (StartOf(1))
                {
                    Get();
                    var usingPos = t.position;
                    while (StartOf(1))
                    {
                        Get();
                    }
                    pgen.usingPos = usingPos.Range(la);
                }
                Expect(7 /*COMPILER*/);
                genScanner = true;
                tab.ignored = new CharSet();
                Expect(1 /*[ident]*/);
                gramName = t.val;
                var semDeclPos = la.position;
                while (StartOf(2))
                {
                    Get();
                }
                tab.semDeclPos = semDeclPos.Range(la);
                if (isKind(la, 8 /*IGNORECASE*/))
                {
                    Get();
                    dfa.ignoreCase = true;
                }
                if (isKind(la, 9 /*CHARACTERS*/))
                {
                    Get();
                    while (isKind(la, 1 /*[ident]*/))
                    {
                        SetDecl‿NT();
                    }
                }
                if (isKind(la, 10 /*TOKENS*/))
                {
                    Get();
                    while (isKind(la, 1 /*[ident]*/) || isKind(la, 3 /*[string]*/) || isKind(la, 5 /*[char]*/))
                    {
                        TokenDecl‿NT(NodeKind.t);
                    }
                }
                if (isKind(la, 11 /*PRAGMAS*/))
                {
                    Get();
                    while (isKind(la, 1 /*[ident]*/) || isKind(la, 3 /*[string]*/) || isKind(la, 5 /*[char]*/))
                    {
                        TokenDecl‿NT(NodeKind.pr);
                    }
                }
                while (isKind(la, 12 /*COMMENTS*/))
                {
                    Get();
                    bool nested = false;
                    Expect(13 /*FROM*/);
                    TokenExpr‿NT(out var g1);
                    Expect(14 /*TO*/);
                    TokenExpr‿NT(out var g2);
                    if (isKind(la, 15 /*NESTED*/))
                    {
                        Get();
                        nested = true;
                    }
                    dfa.NewComment(g1.l, g2.l, nested);
                }
                while (isKind(la, 16 /*IGNORE*/))
                {
                    Get();
                    Set‿NT(out s);
                    tab.ignored.Or(s);
                }
                if (isKind(la, 17 /*SYMBOLTABLES*/))
                {
                    Get();
                    while (isKind(la, 1 /*[ident]*/))
                    {
                        SymboltableDecl‿NT();
                    }
                }
                while (!(isKind(la, 0 /*[EOF]*/) || isKind(la, 18 /*PRODUCTIONS*/)))
                {
                    SynErr(54);
                    Get();
                }
                Expect(18 /*PRODUCTIONS*/);
                if (genScanner) dfa.MakeDeterministic();
                tab.DeleteNodes();
                while (isKind(la, 1 /*[ident]*/) || isKind(la, 21 /*DELETEABLE*/))
                {
                    Production‿NT();
                }
                Expect(19 /*END*/);
                Expect(1 /*[ident]*/);
                if (gramName != t.val)
                SemErr(4, "name does not match grammar name");
                tab.gramSy = tab.FindSym(gramName);
                if (tab.gramSy == null)
                SemErr(5, "missing production for grammar name");
                else {
                var sym = tab.gramSy;
                if (sym.attrPos != null)
                SemErr(6, "grammar symbol must not have attributes");
                }
                tab.noSym = tab.NewSym(NodeKind.t, "???", Position.Zero); // noSym gets highest number
                tab.SetupAnys();
                tab.RenumberPragmas();
                if (tab.ddt[2]) tab.PrintNodes();
                if (errors.Count == 0) {
                Information(1, "checking if grammar is OK.", null);
                tab.CompSymbolSets();
                if (tab.ddt[7]) tab.XRef();
                if (tab.GrammarOk()) {
                pgen.WriteParser();
                if (genScanner) {
                dfa.WriteScanner();
                if (tab.ddt[0]) dfa.PrintStates();
                }
                Information(2, "parser and scanner generated.", null);
                if (tab.ddt[8]) pgen.WriteStatistics();
                }
                }
                if (tab.ddt[6]) tab.PrintSymbolTable();
                Expect(20 /*.*/);
            }
        }


        void SetDecl‿NT()
        {
            {
                CharSet s;
                Expect(1 /*[ident]*/);
                string name = t.val;
                CharClass c = tab.FindCharClass(name);
                if (c != null) SemErr(9, "name declared twice");
                Expect(22 /*=*/);
                Set‿NT(out s);
                if (s.Elements() == 0) SemErr(10, "character set must not be empty");
                tab.NewCharClass(name, s);
                Expect(20 /*.*/);
            }
        }


        void TokenDecl‿NT(NodeKind typ)
        {
            {
                Sym‿NT(out var name, out var kind);
                var sym = tab.FindSym(name);
                if (sym != null) SemErr(13, "name declared twice");
                else {
                sym = tab.NewSym(typ, name, t.position);
                sym.tokenKind = TerminalTokenKind.fixedToken;
                }
                tokenString = null;
                if (isKind(la, 34 /*:*/))
                {
                    Get();
                    Sym‿NT(out var inheritsName, out var inheritsKind);
                    var inheritsSym = tab.FindSym(inheritsName);
                    if (inheritsSym == null) SemErr(14, string.Format("token '{0}' can't inherit from '{1}', name not declared", sym.name, inheritsName));
                    else if (inheritsSym == sym) SemErr(15, string.Format("token '{0}' must not inherit from self", sym.name));
                    else if (inheritsSym.typ != typ) SemErr(16, string.Format("token '{0}' can't inherit from '{1}'", sym.name, inheritsSym.name));
                    else sym.inherits = inheritsSym;
                }
                while (!(StartOf(3)))
                {
                    SynErr(55);
                    Get();
                }
                if (isKind(la, 22 /*=*/))
                {
                    Get();
                    TokenExpr‿NT(out var g);
                    Expect(20 /*.*/);
                    if (kind == str) SemErr(17, "a literal must not be declared with a structure");
                    tab.Finish(g);
                    if (tokenString == null || tokenString.Equals(noString))
                    dfa.ConvertToStates(g.l, sym);
                    else { // TokenExpr is a single string
                    if (tab.literals.ContainsKey(tokenString))
                    SemErr(18, "token string declared twice");
                    tab.literals[tokenString] = sym;
                    dfa.MatchLiteral(tokenString, sym);
                    sym.definedAs = tokenString;
                    }
                }
                else if (StartOf(4))
                {
                    if (kind == id) genScanner = false;
                    else dfa.MatchLiteral(sym.name, sym);
                } // end if
                else
                    SynErr(56);
                if (isKind(la, 50 /*(.*/))
                {
                    SemText‿NT(out sym.semPos);
                    if (typ != NodeKind.pr) SemErr(19, "semantic action not allowed in a pragma context");
                }
            }
        }


        void TokenExpr‿NT(out Graph g)
        {
            {
                Graph g2;
                TokenTerm‿NT(out g);
                bool first = true;
                while (WeakSeparator(39 /*|*/, 5, 6) )
                {
                    TokenTerm‿NT(out g2);
                    if (first) { tab.MakeFirstAlt(g); first = false; }
                    tab.MakeAlternative(g, g2);
                }
            }
        }


        void Set‿NT(out CharSet s)
        {
            {
                CharSet s2;
                SimSet‿NT(out s);
                while (isKind(la, 30 /*+*/) || isKind(la, 31 /*-*/))
                {
                    if (isKind(la, 30 /*+*/))
                    {
                        Get();
                        SimSet‿NT(out s2);
                        s.Or(s2);
                    }
                    else
                    {
                        Get();
                        SimSet‿NT(out s2);
                        s.Subtract(s2);
                    }
                }
            }
        }


        void SymboltableDecl‿NT()
        {
            {
                SymTab st;
                Expect(1 /*[ident]*/);
                string name = t.val.ToLowerInvariant();
                if (tab.FindSymtab(name) != null)
                SemErr(7, "symbol table name declared twice");
                st = new SymTab(name);
                tab.symtabs.Add(st);
                if (isKind(la, 23 /*STRICT*/))
                {
                    Get();
                    st.strict = true;
                }
                while (isKind(la, 3 /*[string]*/))
                {
                    Get();
                    string predef = tab.Unstring(t.val);
                    if (dfa.ignoreCase) predef = predef.ToLowerInvariant();
                    st.Add(predef);
                }
                Expect(20 /*.*/);
            }
        }


        void Production‿NT()
        {
            {
                var deletableOK = false;
                if (isKind(la, 21 /*DELETEABLE*/))
                {
                    Get();
                    deletableOK = true;
                }
                Expect(1 /*[ident]*/);
                var sym = tab.FindSym(t.val);
                bool undef = sym == null;
                if (undef) sym = tab.NewSym(NodeKind.nt, t.val, t.position);
                else {
                if (sym.typ == NodeKind.nt) {
                if (sym.graph != null) SemErr(1, "name declared twice");
                } else SemErr(2, "this symbol kind not allowed on left side of production");
                sym.pos = t.position;
                }
                bool noAttrs = sym.attrPos == null;
                sym.attrPos = null;
                sym.deletableOK = deletableOK;
                if (isKind(la, 35 /*<*/) || isKind(la, 37 /*<.*/))
                {
                    AttrDecl‿NT(sym);
                }
                if (!undef)
                if (noAttrs != (sym.attrPos == null))
                SemErr(3, "attribute mismatch between declaration and use of this symbol");
                if (isKind(la, 30 /*+*/))
                {
                    ASTJoin‿NT(sym);
                }
                if (isKind(la, 24 /*SCOPES*/))
                {
                    ScopesDecl‿NT(sym);
                }
                if (isKind(la, 28 /*USEONCE*/))
                {
                    UseOnceDecl‿NT(sym);
                }
                if (isKind(la, 29 /*USEALL*/))
                {
                    UseAllDecl‿NT(sym);
                }
                if (isKind(la, 50 /*(.*/))
                {
                    SemText‿NT(out sym.semPos);
                }
                ExpectWeak(22 /*=*/, 7 /*COMPILER*/); // 22 followed by 7
                Expression‿NT(out var g);
                sym.graph = g.l;
                tab.Finish(g);
                ExpectWeak(20 /*.*/, 8 /*IGNORECASE*/); // 20 followed by 8
            }
        }


        void AttrDecl‿NT(Symbol sym)
        {
            {
                if (isKind(la, 35 /*<*/))
                {
                    Get();
                    var attrPos = la.position;
                    while (StartOf(9))
                    {
                        if (StartOf(10))
                        {
                            Get();
                        }
                        else
                        {
                            Get();
                            SemErr(20, "bad string in attributes");
                        }
                    }
                    Expect(36 /*>*/);
                    sym.attrPos = attrPos.RangeIfNotEmpty(t);
                }
                else if (isKind(la, 37 /*<.*/))
                {
                    Get();
                    var attrPos = la.position;
                    while (StartOf(11))
                    {
                        if (StartOf(12))
                        {
                            Get();
                        }
                        else
                        {
                            Get();
                            SemErr(21, "bad string in attributes");
                        }
                    }
                    Expect(38 /*.>*/);
                    sym.attrPos = attrPos.RangeIfNotEmpty(t);
                } // end if
                else
                    SynErr(57);
            }
        }


        void ASTJoin‿NT(Symbol sym)
        {
            {
                Expect(30 /*+*/);
                sym.astjoinwith = ""; pgen.needsAST = true;
                if (isKind(la, 3 /*[string]*/))
                {
                    Get();
                    sym.astjoinwith = tab.Unstring(t.val);
                }
            }
        }


        void ScopesDecl‿NT(Symbol sym)
        {
            {
                sym.scopes = new List<SymTab>();
                Expect(24 /*SCOPES*/);
                Expect(25 /*(*/);
                Symboltable‿NT(sym.scopes);
                while (isKind(la, 26 /*,*/))
                {
                    Get();
                    Symboltable‿NT(sym.scopes);
                }
                Expect(27 /*)*/);
            }
        }


        void UseOnceDecl‿NT(Symbol sym)
        {
            {
                sym.useonces = new List<SymTab>();
                Expect(28 /*USEONCE*/);
                Expect(25 /*(*/);
                Symboltable‿NT(sym.useonces);
                while (isKind(la, 26 /*,*/))
                {
                    Get();
                    Symboltable‿NT(sym.useonces);
                }
                Expect(27 /*)*/);
            }
        }


        void UseAllDecl‿NT(Symbol sym)
        {
            {
                sym.usealls = new List<SymTab>();
                Expect(29 /*USEALL*/);
                Expect(25 /*(*/);
                Symboltable‿NT(sym.usealls);
                while (isKind(la, 26 /*,*/))
                {
                    Get();
                    Symboltable‿NT(sym.usealls);
                }
                Expect(27 /*)*/);
            }
        }


        void SemText‿NT(out Range pos)
        {
            {
                Expect(50 /*(.*/);
                var p = la.position;
                while (StartOf(13))
                {
                    if (StartOf(14))
                    {
                        Get();
                    }
                    else if (isKind(la, 4 /*[badString]*/))
                    {
                        Get();
                        SemErr(36, "bad string in semantic action");
                    }
                    else
                    {
                        Get();
                        SemErr(37, "missing end of previous semantic action");
                    }
                }
                Expect(51 /*.)*/);
                pos = p.Range(t);
            }
        }


        void Expression‿NT(out Graph g)
        {
            {
                Graph g2;
                Term‿NT(out g);
                bool first = true;
                while (WeakSeparator(39 /*|*/, 15, 16) )
                {
                    Term‿NT(out g2);
                    if (first) { tab.MakeFirstAlt(g); first = false; }
                    tab.MakeAlternative(g, g2);
                }
            }
        }


        void Symboltable‿NT(List<SymTab> sts )
        {
            {
                Expect(1 /*[ident]*/);
                string stname = t.val.ToLowerInvariant();
                SymTab st = tab.FindSymtab(stname);
                if (st == null) SemErr(8, "undeclared symbol table " + t.val);
                else sts.Add(st);
            }
        }


        void SimSet‿NT(out CharSet s)
        {
            {
                int n1, n2;
                s = new CharSet();
                if (isKind(la, 1 /*[ident]*/))
                {
                    Get();
                    CharClass c = tab.FindCharClass(t.val);
                    if (c == null) SemErr(11, "undefined name"); else s.Or(c.set);
                }
                else if (isKind(la, 3 /*[string]*/))
                {
                    Get();
                    string name = tab.Unstring(t.val);
                    foreach (var ch in name)
                    if (dfa.ignoreCase) s.Set(char.ToLowerInvariant(ch));
                    else s.Set(ch);
                }
                else if (isKind(la, 5 /*[char]*/))
                {
                    Char‿NT(out n1);
                    s.Set(n1);
                    if (isKind(la, 32 /*..*/))
                    {
                        Get();
                        Char‿NT(out n2);
                        for (var i = n1; i <= n2; i++) s.Set(i);
                    }
                }
                else if (isKind(la, 33 /*ANY*/))
                {
                    Get();
                    s = new CharSet(); s.Fill();
                } // end if
                else
                    SynErr(58);
            }
        }


        void Char‿NT(out int n)
        {
            {
                Expect(5 /*[char]*/);
                string name = tab.Unstring(t.val); n = 0;
                if (name.Length == 1) n = name[0];
                else SemErr(12, "unacceptable character value");
                if (dfa.ignoreCase && (char)n >= 'A' && (char)n <= 'Z') n += 32;
            }
        }


        void Sym‿NT(out string name, out int kind)
        {
            {
                name = "???"; kind = id;
                if (isKind(la, 1 /*[ident]*/))
                {
                    Get();
                    kind = id; name = t.val;
                }
                else if (isKind(la, 3 /*[string]*/) || isKind(la, 5 /*[char]*/))
                {
                    if (isKind(la, 3 /*[string]*/))
                    {
                        Get();
                        name = t.val;
                    }
                    else
                    {
                        Get();
                        name = "\"" + t.val.Substring(1, t.val.Length-2) + "\"";
                    }
                    kind = str;
                    if (dfa.ignoreCase) name = name.ToLowerInvariant();
                    if (name.IndexOf(' ') >= 0)
                    SemErr(33, "literal tokens must not contain blanks");
                } // end if
                else
                    SynErr(59);
            }
        }


        void Term‿NT(out Graph g)
        {
            {
                Graph g2; Node rslv = null; g = null;
                if (StartOf(17))
                {
                    if (isKind(la, 48 /*IF*/))
                    {
                        rslv = tab.NewNode(NodeKind.rslv, null, la.line);
                        Resolver‿NT(out rslv.pos);
                        g = new Graph(rslv);
                    }
                    Factor‿NT(out g2);
                    if (rslv != null) tab.MakeSequence(g, g2);
                    else g = g2;
                    while (StartOf(18))
                    {
                        Factor‿NT(out g2);
                        tab.MakeSequence(g, g2);
                    }
                }
                else if (StartOf(19))
                {
                    g = new Graph(tab.NewNode(NodeKind.eps, null, 0));
                } // end if
                else
                    SynErr(60);
                if (g == null) // invalid start of Term
                g = new Graph(tab.NewNode(NodeKind.eps, null, 0));
            }
        }


        void Resolver‿NT(out Range pos)
        {
            {
                Expect(48 /*IF*/);
                Expect(25 /*(*/);
                var p = la.position;
                Condition‿NT();
                pos = p.Range(t);
            }
        }


        void Factor‿NT(out Graph g)
        {
            {
                string name; int kind; Range pos; bool weak = false;
                g = null;
                switch (la.kind)
                {
                    case 1: /*[ident]*/
                    case 3: /*[string]*/
                    case 5: /*[char]*/
                    case 40: /*WEAK*/
                        { // scoping
                            if (isKind(la, 40 /*WEAK*/))
                            {
                                Get();
                                weak = true;
                            }
                            Sym‿NT(out name, out kind);
                            Symbol sym = tab.FindSym(name);
                            if (sym == null && kind == str)
                            tab.literals.TryGetValue(name, out sym);
                            bool undef = sym == null;
                            if (undef) {
                            if (kind == id)
                            sym = tab.NewSym(NodeKind.nt, name, Position.Zero);  // forward nt
                            else if (genScanner) {
                            sym = tab.NewSym(NodeKind.t, name, t.position);
                            dfa.MatchLiteral(sym.name, sym);
                            } else {  // undefined string in production
                            SemErr(22, "undefined string in production");
                            sym = tab.eofSy;  // dummy
                            }
                            }
                            var typ = sym.typ;
                            if (typ != NodeKind.t && typ != NodeKind.nt)
                            SemErr(23, "this symbol kind is not allowed in a production");
                            if (weak)
                            if (typ == NodeKind.t) typ = NodeKind.wt;
                            else SemErr(24, "only terminals may be weak");
                            Node p = tab.NewNode(typ, sym, t.line);
                            g = new Graph(p);
                            if (StartOf(20))
                            {
                                if (isKind(la, 35 /*<*/) || isKind(la, 37 /*<.*/))
                                {
                                    Attribs‿NT(p);
                                    if (kind != id) SemErr(25, "a literal must not have attributes");
                                }
                                else if (isKind(la, 36 /*>*/))
                                {
                                    Get();
                                    Expect(1 /*[ident]*/);
                                    if (typ != NodeKind.t && typ != NodeKind.wt) SemErr(26, "only terminals or weak terminals can declare a name in a symbol table");
                                    p.declares = t.val.ToLowerInvariant();
                                    if (null == tab.FindSymtab(p.declares)) SemErr(27, string.Format("undeclared symbol table '{0}'", p.declares));
                                }
                                else
                                {
                                    Get();
                                    Expect(1 /*[ident]*/);
                                    if (typ != NodeKind.t && typ != NodeKind.wt) SemErr(28, "only terminals or weak terminals can lookup a name in a symbol table");
                                    p.declared = t.val.ToLowerInvariant();
                                    if (null == tab.FindSymtab(p.declared)) SemErr(29, string.Format("undeclared symbol table '{0}'", p.declared));
                                }
                            }
                            if (undef)
                            sym.attrPos = p.pos;  // dummy
                            else if ((p.pos == null) != (sym.attrPos == null))
                            SemErr(30, "attribute mismatch between declaration and use of this symbol");
                            if (isKind(la, 46 /*^*/) || isKind(la, 47 /*#*/))
                            {
                                AST‿NT(p);
                            }
                        }
                        break;
                    case 25: /*(*/
                        { // scoping
                            Get();
                            Expression‿NT(out g);
                            Expect(27 /*)*/);
                        }
                        break;
                    case 41: /*[*/
                        { // scoping
                            Get();
                            Expression‿NT(out g);
                            Expect(42 /*]*/);
                            tab.MakeOption(g);
                        }
                        break;
                    case 43: /*{*/
                        { // scoping
                            Get();
                            Expression‿NT(out g);
                            Expect(44 /*}*/);
                            tab.MakeIteration(g);
                        }
                        break;
                    case 50: /*(.*/
                        { // scoping
                            SemText‿NT(out pos);
                            Node p = tab.NewNode(NodeKind.sem, null, 0);
                            p.pos = pos;
                            g = new Graph(p);
                        }
                        break;
                    case 33: /*ANY*/
                        { // scoping
                            Get();
                            Node p = tab.NewNode(NodeKind.any, null, 0);  // p.set is set in tab.SetupAnys
                            g = new Graph(p);
                        }
                        break;
                    case 45: /*SYNC*/
                        { // scoping
                            Get();
                            Node p = tab.NewNode(NodeKind.sync, null, 0);
                            g = new Graph(p);
                        }
                        break;
                    default:
                        SynErr(61);
                        break;
                } // end switch
                if (g == null) // invalid start of Factor
                g = new Graph(tab.NewNode(NodeKind.eps, null, 0));
            }
        }


        void Attribs‿NT(Node p)
        {
            {
                if (isKind(la, 35 /*<*/))
                {
                    Get();
                    var pos = la.position;
                    while (StartOf(9))
                    {
                        if (StartOf(10))
                        {
                            Get();
                        }
                        else
                        {
                            Get();
                            SemErr(34, "bad string in attributes");
                        }
                    }
                    Expect(36 /*>*/);
                    p.pos = pos.RangeIfNotEmpty(t);
                }
                else if (isKind(la, 37 /*<.*/))
                {
                    Get();
                    var pos = la.position;
                    while (StartOf(11))
                    {
                        if (StartOf(12))
                        {
                            Get();
                        }
                        else
                        {
                            Get();
                            SemErr(35, "bad string in attributes");
                        }
                    }
                    Expect(38 /*.>*/);
                    p.pos = pos.RangeIfNotEmpty(t);
                } // end if
                else
                    SynErr(62);
            }
        }


        void AST‿NT(Node p)
        {
            {
                p.asts = new List<AstOp>(); pgen.needsAST = true;
                if (isKind(la, 46 /*^*/))
                {
                    ASTSendUp‿NT(p);
                }
                else if (isKind(la, 47 /*#*/))
                {
                    ASTHatch‿NT(p);
                    while (WeakSeparator(26 /*,*/, 21, 22) )
                    {
                        ASTHatch‿NT(p);
                    }
                } // end if
                else
                    SynErr(63);
            }
        }


        void ASTSendUp‿NT(Node p)
        {
            {
                AstOp ast = p.addAstOp();
                Expect(46 /*^*/);
                ast.ishatch = false;
                string n = p.sym.name;
                if (n.StartsWith("\"")) n = n.Substring(1, n.Length - 2);
                ast.name = n.ToLowerInvariant();
                if (isKind(la, 46 /*^*/))
                {
                    Get();
                    ast.isList = true;
                }
                if (isKind(la, 34 /*:*/))
                {
                    Get();
                    ASTVal‿NT(out ast.name);
                }
            }
        }


        void ASTHatch‿NT(Node p)
        {
            {
                AstOp ast = p.addAstOp();
                Expect(47 /*#*/);
                ast.ishatch = true;
                if (isKind(la, 47 /*#*/))
                {
                    Get();
                    ast.isList = true;
                }
                if (isKind(la, 6 /*\'*/))
                {
                    ASTPrime‿NT(p, ast);
                }
                if (isKind(la, 34 /*:*/))
                {
                    Get();
                    ASTVal‿NT(out ast.name);
                }
                if (isKind(la, 22 /*=*/))
                {
                    Get();
                    ASTConst‿NT(ast);
                }
            }
        }


        void ASTVal‿NT(out string val)
        {
            {
                val = "?";
                if (isKind(la, 1 /*[ident]*/))
                {
                    Get();
                    val = t.val;
                }
                else if (isKind(la, 3 /*[string]*/))
                {
                    Get();
                    val = tab.Unstring(t.val);
                } // end if
                else
                    SynErr(64);
            }
        }


        void ASTPrime‿NT(Node p, AstOp ast)
        {
            {
                Expect(6 /*\'*/);
                ast.primed = true;
                if (p.typ != NodeKind.t && p.typ != NodeKind.wt)
                SemErr(31, "can only prime terminals");
                if (pgen.IgnoreSemanticActions)
                Warning(1, "token priming is ignored when ignoring semantic actions (-is switch).");
                // no way do define the Prime:void->Token function.
            }
        }


        void ASTConst‿NT(AstOp ast)
        {
            {
                ASTVal‿NT(out ast.literal);
            }
        }


        void Condition‿NT()
        {
            {
                while (StartOf(23))
                {
                    if (isKind(la, 25 /*(*/))
                    {
                        Get();
                        Condition‿NT();
                    }
                    else
                    {
                        Get();
                    }
                }
                Expect(27 /*)*/);
            }
        }


        void TokenTerm‿NT(out Graph g)
        {
            {
                Graph g2;
                TokenFactor‿NT(out g);
                while (StartOf(5))
                {
                    TokenFactor‿NT(out g2);
                    tab.MakeSequence(g, g2);
                }
                if (isKind(la, 49 /*CONTEXT*/))
                {
                    Get();
                    Expect(25 /*(*/);
                    TokenExpr‿NT(out g2);
                    tab.SetContextTrans(g2.l); dfa.hasCtxMoves = true;
                    tab.MakeSequence(g, g2);
                    Expect(27 /*)*/);
                }
            }
        }


        void TokenFactor‿NT(out Graph g)
        {
            {
                string name; int kind;
                g = null;
                if (isKind(la, 1 /*[ident]*/) || isKind(la, 3 /*[string]*/) || isKind(la, 5 /*[char]*/))
                {
                    Sym‿NT(out name, out kind);
                    if (kind == id) {
                    CharClass c = tab.FindCharClass(name);
                    if (c == null) {
                    SemErr(32, "undefined name");
                    c = tab.NewCharClass(name, new CharSet());
                    }
                    Node p = tab.NewNode(NodeKind.clas, null, 0); p.val = c.n;
                    g = new Graph(p);
                    tokenString = noString;
                    } else { // str
                    g = tab.StrToGraph(name);
                    if (tokenString == null) tokenString = name;
                    else tokenString = noString;
                    }
                }
                else if (isKind(la, 25 /*(*/))
                {
                    Get();
                    TokenExpr‿NT(out g);
                    Expect(27 /*)*/);
                }
                else if (isKind(la, 41 /*[*/))
                {
                    Get();
                    TokenExpr‿NT(out g);
                    Expect(42 /*]*/);
                    tab.MakeOption(g); tokenString = noString;
                }
                else if (isKind(la, 43 /*{*/))
                {
                    Get();
                    TokenExpr‿NT(out g);
                    Expect(44 /*}*/);
                    tab.MakeIteration(g); tokenString = noString;
                } // end if
                else
                    SynErr(65);
                if (g == null) // invalid start of TokenFactor
                g = new Graph(tab.NewNode(NodeKind.eps, null, 0));
            }
        }



        public override void Parse() 
        {
            if (scanner == null) throw new FatalError("This parser is not Initialize()-ed.");
            lb = Token.Zero;
            la = Token.Zero;
            Get();
            Coco‿NT();
            Expect(0);
        }
    
        // a token's base type
        public static readonly int[] tBase = {
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1
        };
		protected override int BaseKindOf(int kind) => tBase[kind];

        // a token's name
        public static readonly string[] varTName = {
            "[EOF]",
            "[ident]",
            "[number]",
            "[string]",
            "[badString]",
            "[char]",
            "\\\'",
            "COMPILER",
            "IGNORECASE",
            "CHARACTERS",
            "TOKENS",
            "PRAGMAS",
            "COMMENTS",
            "FROM",
            "TO",
            "NESTED",
            "IGNORE",
            "SYMBOLTABLES",
            "PRODUCTIONS",
            "END",
            ".",
            "DELETEABLE",
            "=",
            "STRICT",
            "SCOPES",
            "(",
            ",",
            ")",
            "USEONCE",
            "USEALL",
            "+",
            "-",
            "..",
            "ANY",
            ":",
            "<",
            ">",
            "<.",
            ".>",
            "|",
            "WEAK",
            "[",
            "]",
            "{",
            "}",
            "SYNC",
            "^",
            "#",
            "IF",
            "CONTEXT",
            "(.",
            ".)",
            "[???]"
        };
        public override string NameOfTokenKind(int tokenKind) => varTName[tokenKind];

		// states that a particular production (1st index) can start with a particular token (2nd index). Needed by addAlt().
		static readonly bool[,] set0 = {
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_x,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _x,_x,_x,_x,  _x,_T,_T,_T,  _x,_x,_x,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_T,_T,  _T,_T,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_x,  _T,_x,_T,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_T,_x,_T,  _x,_T,_x,_x,  _T,_x,_T,_x,  _x,_x},
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_T,  _x,_T,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_x,  _T,_x},
            {_x,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_x,  _T,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_T,_x,_T,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_T,_T,_T,  _T,_T,_x,_x,  _T,_x,_T,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _T,_T,_x,_T,  _x,_T,_x,_x,  _T,_x,_T,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _T,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_T,_x,_T,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_T,_T,_T,  _T,_T,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_x,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x}
		};

        // as set0 but with token inheritance taken into account
        static readonly bool[,] set = {
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_x,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _x,_x,_x,_x,  _x,_T,_T,_T,  _x,_x,_x,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_T,_T,  _T,_T,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_x,  _T,_x,_T,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_T,_x,_T,  _x,_T,_x,_x,  _T,_x,_T,_x,  _x,_x},
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_T,  _x,_T,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_x,  _T,_x},
            {_x,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_x,  _T,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_T,_x,_T,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_T,_T,_T,  _T,_T,_x,_x,  _T,_x,_T,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _T,_T,_x,_T,  _x,_T,_x,_x,  _T,_x,_T,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _T,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_T,_x,_T,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_T,_T,_T,  _T,_T,_x,_x,  _x,_x,_T,_x,  _x,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_x,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x}
        };

        protected override bool StartOf(int s, int kind) => set[s, kind];



        public override string Syntaxerror(int n) 
        {
            switch (n) 
            {
                case 1: return "[EOF] expected";
                case 2: return "[ident] expected";
                case 3: return "[number] expected";
                case 4: return "[string] expected";
                case 5: return "[badString] expected";
                case 6: return "[char] expected";
                case 7: return "\\\' expected";
                case 8: return "COMPILER expected";
                case 9: return "IGNORECASE expected";
                case 10: return "CHARACTERS expected";
                case 11: return "TOKENS expected";
                case 12: return "PRAGMAS expected";
                case 13: return "COMMENTS expected";
                case 14: return "FROM expected";
                case 15: return "TO expected";
                case 16: return "NESTED expected";
                case 17: return "IGNORE expected";
                case 18: return "SYMBOLTABLES expected";
                case 19: return "PRODUCTIONS expected";
                case 20: return "END expected";
                case 21: return ". expected";
                case 22: return "DELETEABLE expected";
                case 23: return "= expected";
                case 24: return "STRICT expected";
                case 25: return "SCOPES expected";
                case 26: return "( expected";
                case 27: return ", expected";
                case 28: return ") expected";
                case 29: return "USEONCE expected";
                case 30: return "USEALL expected";
                case 31: return "+ expected";
                case 32: return "- expected";
                case 33: return ".. expected";
                case 34: return "ANY expected";
                case 35: return ": expected";
                case 36: return "< expected";
                case 37: return "> expected";
                case 38: return "<. expected";
                case 39: return ".> expected";
                case 40: return "| expected";
                case 41: return "WEAK expected";
                case 42: return "[ expected";
                case 43: return "] expected";
                case 44: return "{ expected";
                case 45: return "} expected";
                case 46: return "SYNC expected";
                case 47: return "^ expected";
                case 48: return "# expected";
                case 49: return "IF expected";
                case 50: return "CONTEXT expected";
                case 51: return "(. expected";
                case 52: return ".) expected";
                case 53: return "[???] expected";
                case 54: return "symbol not expected in Coco (SYNC error)";
                case 55: return "symbol not expected in TokenDecl (SYNC error)";
                case 56: return "invalid TokenDecl, expected = [ident] [string] [char] PRAGMAS COMMENTS IGNORE SYMBOLTABLES PRODUCTIONS (.";
                case 57: return "invalid AttrDecl, expected < <.";
                case 58: return "invalid SimSet, expected [ident] [string] [char] ANY";
                case 59: return "invalid Sym, expected [ident] [string] [char]";
                case 60: return "invalid Term, expected [ident] [string] [char] ( ANY WEAK [ { SYNC IF (. . ) | ] }";
                case 61: return "invalid Factor, expected [ident] [string] [char] WEAK ( [ { (. ANY SYNC";
                case 62: return "invalid Attribs, expected < <.";
                case 63: return "invalid AST, expected ^ #";
                case 64: return "invalid ASTVal, expected [ident] [string]";
                case 65: return "invalid TokenFactor, expected [ident] [string] [char] ( [ {";
                default: return $"error {n}";
            }
        }

    } // end Parser

// end namespace implicit
}
