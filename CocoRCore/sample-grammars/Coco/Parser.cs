using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using CocoRCore;

namespace CocoRCore.Samples.Coco
{

    public class Parser : ParserBase 
    {
        public const int _ident = 1; // TOKEN ident
        public const int _number = 2; // TOKEN number
        public const int _string = 3; // TOKEN string
        public const int _badString = 4; // TOKEN badString
        public const int _char = 5; // TOKEN char
        public const int _prime = 6; // TOKEN prime
        private const int __maxT = 51;
        public const int _ddtSym = 52;
        public const int _optionSym = 53;
        private const bool _T = true;
        private const bool _x = false;
        
        public Symboltable symbols(string name)
        {
            return null;
        }



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
            if (alternatives != null) 
            {
                tokens.Add(new Alternative(t, alternatives));
            }
            _newAlt();
            for (;;) 
            {
                la = scanner.Scan();
                if (la.kind <= maxT) 
                { 
                    ++errDist; 
                    break; // it's not a pragma
                }
                // pragma code
                if (la.kind == 52) // pragmas don't inherit kinds
                {
                }
                if (la.kind == 53) // pragmas don't inherit kinds
                {
                }
            }
        }


        void Coco‿NT()
        {
            {
                addAlt(set0, 1); // OPT
                if (StartOf(1))
                {
                    Get();
                    addAlt(set0, 1); // ITER start
                    while (StartOf(1))
                    {
                        Get();
                        addAlt(set0, 1); // ITER end
                    }
                }
                addAlt(7); // T "COMPILER"
                Expect(7); // "COMPILER"
                addAlt(1); // T ident
                Expect(1); // ident
                addAlt(set0, 2); // ITER start
                while (StartOf(2))
                {
                    Get();
                    addAlt(set0, 2); // ITER end
                }
                addAlt(8); // OPT
                if (isKind(la, 8))
                {
                    Get();
                }
                addAlt(9); // OPT
                if (isKind(la, 9))
                {
                    Get();
                    addAlt(1); // ITER start
                    while (isKind(la, 1))
                    {
                        SetDecl‿NT();
                        addAlt(1); // ITER end
                    }
                }
                addAlt(10); // OPT
                if (isKind(la, 10))
                {
                    Get();
                    addAlt(new int[] {1, 3, 5}); // ITER start
                    while (isKind(la, 1) || isKind(la, 3) || isKind(la, 5))
                    {
                        TokenDecl‿NT();
                        addAlt(new int[] {1, 3, 5}); // ITER end
                    }
                }
                addAlt(11); // OPT
                if (isKind(la, 11))
                {
                    Get();
                    addAlt(new int[] {1, 3, 5}); // ITER start
                    while (isKind(la, 1) || isKind(la, 3) || isKind(la, 5))
                    {
                        TokenDecl‿NT();
                        addAlt(new int[] {1, 3, 5}); // ITER end
                    }
                }
                addAlt(12); // ITER start
                while (isKind(la, 12))
                {
                    Get();
                    addAlt(13); // T "FROM"
                    Expect(13); // "FROM"
                    TokenExpr‿NT();
                    addAlt(14); // T "TO"
                    Expect(14); // "TO"
                    TokenExpr‿NT();
                    addAlt(15); // OPT
                    if (isKind(la, 15))
                    {
                        Get();
                    }
                    addAlt(12); // ITER end
                }
                addAlt(16); // ITER start
                while (isKind(la, 16))
                {
                    Get();
                    Set‿NT();
                    addAlt(16); // ITER end
                }
                addAlt(17); // OPT
                if (isKind(la, 17))
                {
                    Get();
                    addAlt(1); // ITER start
                    while (isKind(la, 1))
                    {
                        SymboltableDecl‿NT();
                        addAlt(1); // ITER end
                    }
                }
                while (!(isKind(la, 0) || isKind(la, 18)))
                {
                    SynErr(53);
                    Get();
                }
                addAlt(18); // T "PRODUCTIONS"
                Expect(18); // "PRODUCTIONS"
                addAlt(1); // ITER start
                while (isKind(la, 1))
                {
                    Get();
                    addAlt(new int[] {34, 36}); // OPT
                    if (isKind(la, 34) || isKind(la, 36))
                    {
                        AttrDecl‿NT();
                    }
                    addAlt(29); // OPT
                    if (isKind(la, 29))
                    {
                        ASTJoin‿NT();
                    }
                    addAlt(23); // OPT
                    if (isKind(la, 23))
                    {
                        ScopesDecl‿NT();
                    }
                    addAlt(27); // OPT
                    if (isKind(la, 27))
                    {
                        UseOnceDecl‿NT();
                    }
                    addAlt(28); // OPT
                    if (isKind(la, 28))
                    {
                        UseAllDecl‿NT();
                    }
                    addAlt(49); // OPT
                    if (isKind(la, 49))
                    {
                        SemText‿NT();
                    }
                    addAlt(19); // WT "="
                    ExpectWeak(19, 3); // "=" followed by string
                    Expression‿NT();
                    addAlt(20); // WT "."
                    ExpectWeak(20, 4); // "." followed by badString
                    addAlt(1); // ITER end
                }
                addAlt(21); // T "END"
                Expect(21); // "END"
                addAlt(1); // T ident
                Expect(1); // ident
                addAlt(20); // T "."
                Expect(20); // "."
            }
        }


        void SetDecl‿NT()
        {
            {
                addAlt(1); // T ident
                Expect(1); // ident
                addAlt(19); // T "="
                Expect(19); // "="
                Set‿NT();
                addAlt(20); // T "."
                Expect(20); // "."
            }
        }


        void TokenDecl‿NT()
        {
            {
                Sym‿NT();
                addAlt(33); // OPT
                if (isKind(la, 33))
                {
                    Get();
                    Sym‿NT();
                }
                while (!(StartOf(5)))
                {
                    SynErr(54);
                    Get();
                }
                addAlt(19); // ALT
                addAlt(set0, 6); // ALT
                if (isKind(la, 19))
                {
                    Get();
                    TokenExpr‿NT();
                    addAlt(20); // T "."
                    Expect(20); // "."
                }
                else if (StartOf(6))
                {
                } // end if
                else
                    SynErr(55);
                addAlt(49); // OPT
                if (isKind(la, 49))
                {
                    SemText‿NT();
                }
            }
        }


        void TokenExpr‿NT()
        {
            {
                TokenTerm‿NT();
                addAlt(38); // ITER start
                while (WeakSeparator(38, 7, 8) )
                {
                    TokenTerm‿NT();
                    addAlt(38); // ITER end
                }
            }
        }


        void Set‿NT()
        {
            {
                SimSet‿NT();
                addAlt(new int[] {29, 30}); // ITER start
                while (isKind(la, 29) || isKind(la, 30))
                {
                    addAlt(29); // ALT
                    addAlt(30); // ALT
                    if (isKind(la, 29))
                    {
                        Get();
                        SimSet‿NT();
                    }
                    else
                    {
                        Get();
                        SimSet‿NT();
                    }
                    addAlt(new int[] {29, 30}); // ITER end
                }
            }
        }


        void SymboltableDecl‿NT()
        {
            {
                addAlt(1); // T ident
                Expect(1); // ident
                addAlt(22); // OPT
                if (isKind(la, 22))
                {
                    Get();
                }
                addAlt(3); // ITER start
                while (isKind(la, 3))
                {
                    Get();
                    addAlt(3); // ITER end
                }
                addAlt(20); // T "."
                Expect(20); // "."
            }
        }


        void AttrDecl‿NT()
        {
            {
                addAlt(34); // ALT
                addAlt(36); // ALT
                if (isKind(la, 34))
                {
                    Get();
                    addAlt(set0, 9); // ITER start
                    while (StartOf(9))
                    {
                        addAlt(set0, 10); // ALT
                        addAlt(4); // ALT
                        if (StartOf(10))
                        {
                            Get();
                        }
                        else
                        {
                            Get();
                        }
                        addAlt(set0, 9); // ITER end
                    }
                    addAlt(35); // T ">"
                    Expect(35); // ">"
                }
                else if (isKind(la, 36))
                {
                    Get();
                    addAlt(set0, 11); // ITER start
                    while (StartOf(11))
                    {
                        addAlt(set0, 12); // ALT
                        addAlt(4); // ALT
                        if (StartOf(12))
                        {
                            Get();
                        }
                        else
                        {
                            Get();
                        }
                        addAlt(set0, 11); // ITER end
                    }
                    addAlt(37); // T ".>"
                    Expect(37); // ".>"
                } // end if
                else
                    SynErr(56);
            }
        }


        void ASTJoin‿NT()
        {
            {
                addAlt(29); // T "+"
                Expect(29); // "+"
                addAlt(3); // OPT
                if (isKind(la, 3))
                {
                    Get();
                }
            }
        }


        void ScopesDecl‿NT()
        {
            {
                addAlt(23); // T "SCOPES"
                Expect(23); // "SCOPES"
                addAlt(24); // T "("
                Expect(24); // "("
                Symboltable‿NT();
                addAlt(25); // ITER start
                while (isKind(la, 25))
                {
                    Get();
                    Symboltable‿NT();
                    addAlt(25); // ITER end
                }
                addAlt(26); // T ")"
                Expect(26); // ")"
            }
        }


        void UseOnceDecl‿NT()
        {
            {
                addAlt(27); // T "USEONCE"
                Expect(27); // "USEONCE"
                addAlt(24); // T "("
                Expect(24); // "("
                Symboltable‿NT();
                addAlt(25); // ITER start
                while (isKind(la, 25))
                {
                    Get();
                    Symboltable‿NT();
                    addAlt(25); // ITER end
                }
                addAlt(26); // T ")"
                Expect(26); // ")"
            }
        }


        void UseAllDecl‿NT()
        {
            {
                addAlt(28); // T "USEALL"
                Expect(28); // "USEALL"
                addAlt(24); // T "("
                Expect(24); // "("
                Symboltable‿NT();
                addAlt(25); // ITER start
                while (isKind(la, 25))
                {
                    Get();
                    Symboltable‿NT();
                    addAlt(25); // ITER end
                }
                addAlt(26); // T ")"
                Expect(26); // ")"
            }
        }


        void SemText‿NT()
        {
            {
                addAlt(49); // T "(."
                Expect(49); // "(."
                addAlt(set0, 13); // ITER start
                while (StartOf(13))
                {
                    addAlt(set0, 14); // ALT
                    addAlt(4); // ALT
                    addAlt(49); // ALT
                    if (StartOf(14))
                    {
                        Get();
                    }
                    else if (isKind(la, 4))
                    {
                        Get();
                    }
                    else
                    {
                        Get();
                    }
                    addAlt(set0, 13); // ITER end
                }
                addAlt(50); // T ".)"
                Expect(50); // ".)"
            }
        }


        void Expression‿NT()
        {
            {
                Term‿NT();
                addAlt(38); // ITER start
                while (WeakSeparator(38, 15, 16) )
                {
                    Term‿NT();
                    addAlt(38); // ITER end
                }
            }
        }


        void Symboltable‿NT()
        {
            {
                addAlt(1); // T ident
                Expect(1); // ident
            }
        }


        void SimSet‿NT()
        {
            {
                addAlt(1); // ALT
                addAlt(3); // ALT
                addAlt(5); // ALT
                addAlt(32); // ALT
                if (isKind(la, 1))
                {
                    Get();
                }
                else if (isKind(la, 3))
                {
                    Get();
                }
                else if (isKind(la, 5))
                {
                    Char‿NT();
                    addAlt(31); // OPT
                    if (isKind(la, 31))
                    {
                        Get();
                        Char‿NT();
                    }
                }
                else if (isKind(la, 32))
                {
                    Get();
                } // end if
                else
                    SynErr(57);
            }
        }


        void Char‿NT()
        {
            {
                addAlt(5); // T char
                Expect(5); // char
            }
        }


        void Sym‿NT()
        {
            {
                addAlt(1); // ALT
                addAlt(new int[] {3, 5}); // ALT
                if (isKind(la, 1))
                {
                    Get();
                }
                else if (isKind(la, 3) || isKind(la, 5))
                {
                    addAlt(3); // ALT
                    addAlt(5); // ALT
                    if (isKind(la, 3))
                    {
                        Get();
                    }
                    else
                    {
                        Get();
                    }
                } // end if
                else
                    SynErr(58);
            }
        }


        void Term‿NT()
        {
            {
                addAlt(set0, 17); // ALT
                addAlt(set0, 18); // ALT
                if (StartOf(17))
                {
                    addAlt(47); // OPT
                    if (isKind(la, 47))
                    {
                        Resolver‿NT();
                    }
                    Factor‿NT();
                    addAlt(set0, 19); // ITER start
                    while (StartOf(19))
                    {
                        Factor‿NT();
                        addAlt(set0, 19); // ITER end
                    }
                }
                else if (StartOf(18))
                {
                } // end if
                else
                    SynErr(59);
            }
        }


        void Resolver‿NT()
        {
            {
                addAlt(47); // T "IF"
                Expect(47); // "IF"
                addAlt(24); // T "("
                Expect(24); // "("
                Condition‿NT();
            }
        }


        void Factor‿NT()
        {
            {
                addAlt(set0, 20); // ALT
                addAlt(24); // ALT
                addAlt(40); // ALT
                addAlt(42); // ALT
                addAlt(49); // ALT
                addAlt(32); // ALT
                addAlt(44); // ALT
                switch (la.kind)
                {
                    case 1: // ident
                    case 3: // string
                    case 5: // char
                    case 39: // "WEAK"
                        { // scoping
                            addAlt(39); // OPT
                            if (isKind(la, 39))
                            {
                                Get();
                            }
                            Sym‿NT();
                            addAlt(set0, 21); // OPT
                            if (StartOf(21))
                            {
                                addAlt(new int[] {34, 36}); // ALT
                                addAlt(35); // ALT
                                addAlt(33); // ALT
                                if (isKind(la, 34) || isKind(la, 36))
                                {
                                    Attribs‿NT();
                                }
                                else if (isKind(la, 35))
                                {
                                    Get();
                                    addAlt(1); // T ident
                                    Expect(1); // ident
                                }
                                else
                                {
                                    Get();
                                    addAlt(1); // T ident
                                    Expect(1); // ident
                                }
                            }
                            addAlt(new int[] {45, 46}); // OPT
                            if (isKind(la, 45) || isKind(la, 46))
                            {
                                AST‿NT();
                            }
                        }
                        break;
                    case 24: // "("
                        { // scoping
                            Get();
                            Expression‿NT();
                            addAlt(26); // T ")"
                            Expect(26); // ")"
                        }
                        break;
                    case 40: // "["
                        { // scoping
                            Get();
                            Expression‿NT();
                            addAlt(41); // T "]"
                            Expect(41); // "]"
                        }
                        break;
                    case 42: // "{"
                        { // scoping
                            Get();
                            Expression‿NT();
                            addAlt(43); // T "}"
                            Expect(43); // "}"
                        }
                        break;
                    case 49: // "(."
                        { // scoping
                            SemText‿NT();
                        }
                        break;
                    case 32: // "ANY"
                        { // scoping
                            Get();
                        }
                        break;
                    case 44: // "SYNC"
                        { // scoping
                            Get();
                        }
                        break;
                    default:
                        SynErr(60);
                        break;
                } // end switch
            }
        }


        void Attribs‿NT()
        {
            {
                addAlt(34); // ALT
                addAlt(36); // ALT
                if (isKind(la, 34))
                {
                    Get();
                    addAlt(set0, 9); // ITER start
                    while (StartOf(9))
                    {
                        addAlt(set0, 10); // ALT
                        addAlt(4); // ALT
                        if (StartOf(10))
                        {
                            Get();
                        }
                        else
                        {
                            Get();
                        }
                        addAlt(set0, 9); // ITER end
                    }
                    addAlt(35); // T ">"
                    Expect(35); // ">"
                }
                else if (isKind(la, 36))
                {
                    Get();
                    addAlt(set0, 11); // ITER start
                    while (StartOf(11))
                    {
                        addAlt(set0, 12); // ALT
                        addAlt(4); // ALT
                        if (StartOf(12))
                        {
                            Get();
                        }
                        else
                        {
                            Get();
                        }
                        addAlt(set0, 11); // ITER end
                    }
                    addAlt(37); // T ".>"
                    Expect(37); // ".>"
                } // end if
                else
                    SynErr(61);
            }
        }


        void AST‿NT()
        {
            {
                addAlt(45); // ALT
                addAlt(46); // ALT
                if (isKind(la, 45))
                {
                    ASTSendUp‿NT();
                }
                else if (isKind(la, 46))
                {
                    ASTHatch‿NT();
                    addAlt(25); // ITER start
                    while (WeakSeparator(25, 22, 23) )
                    {
                        ASTHatch‿NT();
                        addAlt(25); // ITER end
                    }
                } // end if
                else
                    SynErr(62);
            }
        }


        void ASTSendUp‿NT()
        {
            {
                addAlt(45); // T "^"
                Expect(45); // "^"
                addAlt(45); // OPT
                if (isKind(la, 45))
                {
                    Get();
                }
                addAlt(33); // OPT
                if (isKind(la, 33))
                {
                    Get();
                    ASTVal‿NT();
                }
            }
        }


        void ASTHatch‿NT()
        {
            {
                addAlt(46); // T "#"
                Expect(46); // "#"
                addAlt(46); // OPT
                if (isKind(la, 46))
                {
                    Get();
                }
                addAlt(6); // OPT
                if (isKind(la, 6))
                {
                    ASTPrime‿NT();
                }
                addAlt(33); // OPT
                if (isKind(la, 33))
                {
                    Get();
                    ASTVal‿NT();
                }
                addAlt(19); // OPT
                if (isKind(la, 19))
                {
                    Get();
                    ASTConst‿NT();
                }
            }
        }


        void ASTVal‿NT()
        {
            {
                addAlt(1); // ALT
                addAlt(3); // ALT
                if (isKind(la, 1))
                {
                    Get();
                }
                else if (isKind(la, 3))
                {
                    Get();
                } // end if
                else
                    SynErr(63);
            }
        }


        void ASTPrime‿NT()
        {
            {
                addAlt(6); // T prime
                Expect(6); // prime
            }
        }


        void ASTConst‿NT()
        {
            {
                ASTVal‿NT();
            }
        }


        void Condition‿NT()
        {
            {
                addAlt(set0, 24); // ITER start
                while (StartOf(24))
                {
                    addAlt(24); // ALT
                    addAlt(set0, 25); // ALT
                    if (isKind(la, 24))
                    {
                        Get();
                        Condition‿NT();
                    }
                    else
                    {
                        Get();
                    }
                    addAlt(set0, 24); // ITER end
                }
                addAlt(26); // T ")"
                Expect(26); // ")"
            }
        }


        void TokenTerm‿NT()
        {
            {
                TokenFactor‿NT();
                addAlt(set0, 7); // ITER start
                while (StartOf(7))
                {
                    TokenFactor‿NT();
                    addAlt(set0, 7); // ITER end
                }
                addAlt(48); // OPT
                if (isKind(la, 48))
                {
                    Get();
                    addAlt(24); // T "("
                    Expect(24); // "("
                    TokenExpr‿NT();
                    addAlt(26); // T ")"
                    Expect(26); // ")"
                }
            }
        }


        void TokenFactor‿NT()
        {
            {
                addAlt(new int[] {1, 3, 5}); // ALT
                addAlt(24); // ALT
                addAlt(40); // ALT
                addAlt(42); // ALT
                if (isKind(la, 1) || isKind(la, 3) || isKind(la, 5))
                {
                    Sym‿NT();
                }
                else if (isKind(la, 24))
                {
                    Get();
                    TokenExpr‿NT();
                    addAlt(26); // T ")"
                    Expect(26); // ")"
                }
                else if (isKind(la, 40))
                {
                    Get();
                    TokenExpr‿NT();
                    addAlt(41); // T "]"
                    Expect(41); // "]"
                }
                else if (isKind(la, 42))
                {
                    Get();
                    TokenExpr‿NT();
                    addAlt(43); // T "}"
                    Expect(43); // "}"
                } // end if
                else
                    SynErr(64);
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
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1
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
            "=",
            ".",
            "END",
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


        // as set0 but with token inheritance taken into account
        static readonly bool[,] set = {
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_x,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _x,_x,_x,_x,  _x,_T,_T,_T,  _x,_x,_x,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_T,  _T,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_T,_T,  _T,_x,_T,_x,  _T,_x,_x,_T,  _x,_T,_x,_x,  _x},
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x},
            {_T,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_T,  _T,_x,_x,_x,  _T,_T,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_T,_T,  _T,_T,_T,_x,  _T,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_x,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_x,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_T,  _x},
            {_x,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_T,  _x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _T,_x,_T,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_T,  _x,_T,_x,_x,  _x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_T,  _T,_x,_T,_x,  _T,_x,_x,_T,  _x,_T,_x,_x,  _x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_T,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_T,  _T,_x,_T,_x,  _T,_x,_x,_x,  _x,_T,_x,_x,  _x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_T,_T,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x},
            {_x,_T,_x,_T,  _x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _T,_x,_T,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_x,  _x,_T,_x,_x,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_x,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x}
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
                case 20: return "= expected";
                case 21: return ". expected";
                case 22: return "END expected";
                case 23: return "STRICT expected";
                case 24: return "SCOPES expected";
                case 25: return "( expected";
                case 26: return ", expected";
                case 27: return ") expected";
                case 28: return "USEONCE expected";
                case 29: return "USEALL expected";
                case 30: return "+ expected";
                case 31: return "- expected";
                case 32: return ".. expected";
                case 33: return "ANY expected";
                case 34: return ": expected";
                case 35: return "< expected";
                case 36: return "> expected";
                case 37: return "<. expected";
                case 38: return ".> expected";
                case 39: return "| expected";
                case 40: return "WEAK expected";
                case 41: return "[ expected";
                case 42: return "] expected";
                case 43: return "{ expected";
                case 44: return "} expected";
                case 45: return "SYNC expected";
                case 46: return "^ expected";
                case 47: return "# expected";
                case 48: return "IF expected";
                case 49: return "CONTEXT expected";
                case 50: return "(. expected";
                case 51: return ".) expected";
                case 52: return "[???] expected";
                case 53: return "symbol not expected in Coco (SYNC error)";
                case 54: return "symbol not expected in TokenDecl (SYNC error)";
                case 55: return "invalid TokenDecl, expected = [ident] [string] [char] PRAGMAS COMMENTS IGNORE SYMBOLTABLES PRODUCTIONS (.";
                case 56: return "invalid AttrDecl, expected < <.";
                case 57: return "invalid SimSet, expected [ident] [string] [char] ANY";
                case 58: return "invalid Sym, expected [ident] [string] [char]";
                case 59: return "invalid Term, expected [ident] [string] [char] ( ANY WEAK [ { SYNC IF (. . ) | ] }";
                case 60: return "invalid Factor, expected [ident] [string] [char] WEAK ( [ { (. ANY SYNC";
                case 61: return "invalid Attribs, expected < <.";
                case 62: return "invalid AST, expected ^ #";
                case 63: return "invalid ASTVal, expected [ident] [string]";
                case 64: return "invalid TokenFactor, expected [ident] [string] [char] ( [ {";
                default: return $"error {n}";
            }
        }

    } // end Parser

// end namespace implicit
}
