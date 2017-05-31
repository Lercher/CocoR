using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using CocoRCore;

namespace CocoRCore.Samples.Inheritance
{

    public class Parser : ParserBase 
    {
        public const int _number = 1; // TOKEN number
        public const int _ident = 2; // TOKEN ident
        public const int _keyword = 3; // TOKEN keyword
        public const int _var = 4; // TOKEN var INHERITS ident
        public const int _var1 = 5; // TOKEN var1 INHERITS ident
        public const int _var2 = 6; // TOKEN var2 INHERITS ident
        public const int _var3 = 7; // TOKEN var3 INHERITS ident
        public const int _var4 = 8; // TOKEN var4 INHERITS ident
        public const int _var5 = 9; // TOKEN var5 INHERITS ident
        public const int _var6 = 10; // TOKEN var6 INHERITS ident
        public const int _colon = 11; // TOKEN colon
        private const int __maxT = 17;
        private const bool _T = true;
        private const bool _x = false;
        
        public readonly Symboltable variables;
        public readonly Symboltable types;
        public Symboltable symbols(string name)
        {
            if (name == "variables") return variables;
            if (name == "types") return types;
            return null;
        }



        public Parser()
        {
        variables = new Symboltable("variables", true, false, this);
        types = new Symboltable("types", true, true, this);
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
            }
        }


        void Inheritance‿NT()
        {
            using(types.createUsageCheck(false, la)) // 0..1
            using(types.createUsageCheck(true, la)) // 1..N
            {
                Seq‿NT();
            }
        }


        void Seq‿NT()
        {
            {
                addAlt(set0, 1); // ITER start
                while (StartOf(1))
                {
                    addAlt(set0, 2); // ALT
                    addAlt(12); // ALT
                    addAlt(14); // ALT
                    if (StartOf(2))
                    {
                        Ident‿NT();
                    }
                    else if (isKind(la, 12))
                    {
                        Block‿NT();
                    }
                    else
                    {
                        Type‿NT();
                    }
                    addAlt(set0, 1); // ITER end
                }
            }
        }


        void Block‿NT()
        {
            using(variables.createScope()) 
            using(types.createScope()) 
            {
                addAlt(12); // T "{"
                Expect(12); // "{"
                Seq‿NT();
                addAlt(13); // T "}"
                Expect(13); // "}"
            }
        }


        void Ident‿NT()
        {
            {
                Var‿NT();
                if (!variables.Add(la)) SemErr(71, string.Format(DuplicateSymbol, "ident", la.val, variables.name), la);
                alternatives.tdeclares = variables;
                addAlt(2); // T ident
                Expect(2); // ident
                addAlt(new int[] {11, 15}); // OPT
                if (isKind(la, 11) || isKind(la, 15))
                {
                    addAlt(15); // ALT
                    addAlt(11); // ALT
                    if (isKind(la, 15))
                    {
                        Get();
                    }
                    else
                    {
                        Get();
                    }
                    if (!types.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "ident", la.val, types.name), la);
                    addAlt(2); // T ident
                    addAlt(2, types); // T ident ident uses symbol table 'types'
                    Expect(2); // ident
                }
                while (!(isKind(la, 0) || isKind(la, 16)))
                {
                    SynErr(19);
                    Get();
                }
                addAlt(16); // T ";"
                Expect(16); // ";"
            }
        }


        void Type‿NT()
        {
            {
                addAlt(14); // T "type"
                Expect(14); // "type"
                if (!types.Add(la)) SemErr(71, string.Format(DuplicateSymbol, "ident", la.val, types.name), la);
                alternatives.tdeclares = types;
                addAlt(2); // T ident
                Expect(2); // ident
            }
        }


        void Var‿NT()
        {
            {
                addAlt(4); // ALT
                addAlt(5); // ALT
                addAlt(6); // ALT
                addAlt(7); // ALT
                addAlt(8); // ALT
                addAlt(9); // ALT
                addAlt(10); // ALT
                switch (la.kind)
                {
                    case 4: // var
                        { // scoping
                            Get();
                        }
                        break;
                    case 5: // var1
                        { // scoping
                            Get();
                        }
                        break;
                    case 6: // var2
                        { // scoping
                            Get();
                        }
                        break;
                    case 7: // var3
                        { // scoping
                            Get();
                        }
                        break;
                    case 8: // var4
                        { // scoping
                            Get();
                        }
                        break;
                    case 9: // var5
                        { // scoping
                            Get();
                        }
                        break;
                    case 10: // var6
                        { // scoping
                            Get();
                        }
                        break;
                    default:
                        SynErr(20);
                        break;
                } // end switch
            }
        }



        public override void Parse() 
        {
            if (scanner == null) throw new FatalError("This parser is not Initialize()-ed.");
            lb = Token.Zero;
            la = Token.Zero;
            Get();
            types.Add("string");
            types.Add("int");
            types.Add("double");
            Inheritance‿NT();
            Expect(0);
            variables.CheckDeclared();
            types.CheckDeclared();
        }
    
        // a token's base type
        public static readonly int[] tBase = {
            -1,-1,-1,-1,   2, 2, 2, 2,   2, 2, 2,-1,  -1,-1,-1,-1,  -1,-1
        };
		protected override int BaseKindOf(int kind) => tBase[kind];

        // a token's name
        public static readonly string[] varTName = {
            "[EOF]",
            "[number]",
            "[ident]",
            "keywordcamelcase",
            "var",
            "var1",
            "var2",
            "var3",
            "var4",
            "var5",
            "var6",
            ":",
            "{",
            "}",
            "type",
            "as",
            ";",
            "[???]"
        };
        public override string NameOfTokenKind(int tokenKind) => varTName[tokenKind];


        // as set0 but with token inheritance taken into account
        static readonly bool[,] set = {
            {_T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x},
            {_x,_x,_x,_x,  _T,_T,_T,_T,  _T,_T,_T,_x,  _T,_x,_T,_x,  _x,_x,_x},
            {_x,_x,_x,_x,  _T,_T,_T,_T,  _T,_T,_T,_x,  _x,_x,_x,_x,  _x,_x,_x}
        };

        protected override bool StartOf(int s, int kind) => set[s, kind];



        public override string Syntaxerror(int n) 
        {
            switch (n) 
            {
                case 1: return "[EOF] expected";
                case 2: return "[number] expected";
                case 3: return "[ident] expected";
                case 4: return "keywordcamelcase expected";
                case 5: return "var expected";
                case 6: return "var1 expected";
                case 7: return "var2 expected";
                case 8: return "var3 expected";
                case 9: return "var4 expected";
                case 10: return "var5 expected";
                case 11: return "var6 expected";
                case 12: return ": expected";
                case 13: return "{ expected";
                case 14: return "} expected";
                case 15: return "type expected";
                case 16: return "as expected";
                case 17: return "; expected";
                case 18: return "[???] expected";
                case 19: return "symbol not expected in Ident (SYNC error)";
                case 20: return "invalid Var, expected var var1 var2 var3 var4 var5 var6";
                default: return $"error {n}";
            }
        }

    } // end Parser

// end namespace implicit
}
