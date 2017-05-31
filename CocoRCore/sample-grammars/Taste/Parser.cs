using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using CocoRCore;

namespace CocoRCore.Samples.Taste
{

    public class Parser : ParserBase 
    {
        public const int _ident = 1; // TOKEN ident
        public const int _number = 2; // TOKEN number
        public const int _gte = 3; // TOKEN gte
        private const int __maxT = 30;
        private const bool _T = true;
        private const bool _x = false;
        
        public Symboltable symbols(string name)
        {
            return null;
        }


const int // types
	  undef = 0, integer = 1, boolean = 2;

	const int // object kinds
	  var = 0, proc = 1;

	public SymbolTable   tab;
	public CodeGenerator gen;
  
/*--------------------------------------------------------------------------*/

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
            }
        }


        void AddOp‿NT(out Op op)
        {
            {
                op = Op.ADD;
                addAlt(4); // ALT
                addAlt(5); // ALT
                if (isKind(la, 4))
                {
                    Get();
                }
                else if (isKind(la, 5))
                {
                    Get();
                    op = Op.SUB;
                } // end if
                else
                    SynErr(32);
            }
        }


        void Expr‿NT(out int type)
        {
            {
                int type1; Op op;
                SimExpr‿NT(out type);
                addAlt(new int[] {15, 16, 17}); // OPT
                if (isKind(la, 15) || isKind(la, 16) || isKind(la, 17))
                {
                    RelOp‿NT(out op);
                    SimExpr‿NT(out type1);
                    if (type != type1) SemErr(1, "incompatible types");
                    gen.Emit(op); type = boolean;
                }
            }
        }


        void SimExpr‿NT(out int type)
        {
            {
                int type1; Op op;
                Term‿NT(out type);
                addAlt(new int[] {4, 5}); // ITER start
                while (isKind(la, 4) || isKind(la, 5))
                {
                    AddOp‿NT(out op);
                    Term‿NT(out type1);
                    if (type != integer || type1 != integer)
                    SemErr(4, "integer type expected");
                    gen.Emit(op);
                    addAlt(new int[] {4, 5}); // ITER end
                }
            }
        }


        void RelOp‿NT(out Op op)
        {
            {
                op = Op.EQU;
                addAlt(15); // ALT
                addAlt(16); // ALT
                addAlt(17); // ALT
                if (isKind(la, 15))
                {
                    Get();
                }
                else if (isKind(la, 16))
                {
                    Get();
                    op = Op.LSS;
                }
                else if (isKind(la, 17))
                {
                    Get();
                    op = Op.GTR;
                } // end if
                else
                    SynErr(33);
            }
        }


        void Factor‿NT(out int type)
        {
            {
                int n; Obj obj; string name;
                type = undef;
                addAlt(1); // ALT
                addAlt(2); // ALT
                addAlt(5); // ALT
                addAlt(6); // ALT
                addAlt(7); // ALT
                if (isKind(la, 1))
                {
                    Ident‿NT(out name);
                    obj = tab.Find(name); type = obj.type;
                    if (obj.kind == var) {
                    if (obj.level == 0) gen.Emit(Op.LOADG, obj.adr);
                    else gen.Emit(Op.LOAD, obj.adr);
                    } else SemErr(2, "variable expected");
                }
                else if (isKind(la, 2))
                {
                    Get();
                    n = Convert.ToInt32(t.val);
                    gen.Emit(Op.CONST, n); type = integer;
                }
                else if (isKind(la, 5))
                {
                    Get();
                    Factor‿NT(out type);
                    if (type != integer) {
                    SemErr(3, "integer type expected"); type = integer;
                    }
                    gen.Emit(Op.NEG);
                }
                else if (isKind(la, 6))
                {
                    Get();
                    gen.Emit(Op.CONST, 1); type = boolean;
                }
                else if (isKind(la, 7))
                {
                    Get();
                    gen.Emit(Op.CONST, 0); type = boolean;
                } // end if
                else
                    SynErr(34);
            }
        }


        void Ident‿NT(out string name)
        {
            {
                addAlt(1); // T ident
                Expect(1); // ident
                name = t.val;
            }
        }


        void MulOp‿NT(out Op op)
        {
            {
                op = Op.MUL;
                addAlt(8); // ALT
                addAlt(9); // ALT
                if (isKind(la, 8))
                {
                    Get();
                }
                else if (isKind(la, 9))
                {
                    Get();
                    op = Op.DIV;
                } // end if
                else
                    SynErr(35);
            }
        }


        void ProcDecl‿NT()
        {
            {
                string name; Obj obj; int adr;
                addAlt(10); // T "void"
                Expect(10); // "void"
                Ident‿NT(out name);
                obj = tab.NewObj(name, proc, undef); obj.adr = gen.pc;
                if (name == "Main") gen.progStart = gen.pc;
                tab.OpenScope();
                addAlt(11); // T "("
                Expect(11); // "("
                addAlt(12); // T ")"
                Expect(12); // ")"
                addAlt(13); // T "{"
                Expect(13); // "{"
                gen.Emit(Op.ENTER, 0); adr = gen.pc - 2;
                addAlt(set0, 1); // ITER start
                while (StartOf(1))
                {
                    addAlt(new int[] {27, 28}); // ALT
                    addAlt(set0, 2); // ALT
                    if (isKind(la, 27) || isKind(la, 28))
                    {
                        VarDecl‿NT();
                    }
                    else
                    {
                        Stat‿NT();
                    }
                    addAlt(set0, 1); // ITER end
                }
                addAlt(14); // T "}"
                Expect(14); // "}"
                gen.Emit(Op.LEAVE); gen.Emit(Op.RET);
                gen.Patch(adr, tab.topScope.nextAdr);
                tab.CloseScope();
            }
        }


        void VarDecl‿NT()
        {
            {
                string name; int type;
                Type‿NT(out type);
                Ident‿NT(out name);
                tab.NewObj(name, var, type);
                addAlt(29); // ITER start
                while (isKind(la, 29))
                {
                    Get();
                    Ident‿NT(out name);
                    tab.NewObj(name, var, type);
                    addAlt(29); // ITER end
                }
                addAlt(19); // T ";"
                Expect(19); // ";"
            }
        }


        void Stat‿NT()
        {
            {
                int type; string name; Obj obj;
                int adr, adr2, loopstart;
                addAlt(1); // ALT
                addAlt(20); // ALT
                addAlt(21); // ALT
                addAlt(23); // ALT
                addAlt(24); // ALT
                addAlt(25); // ALT
                addAlt(13); // ALT
                switch (la.kind)
                {
                    case 1: // ident
                        { // scoping
                            Ident‿NT(out name);
                            obj = tab.Find(name);
                            addAlt(18); // ALT
                            addAlt(11); // ALT
                            if (isKind(la, 18))
                            {
                                Get();
                                if (obj.kind != var) SemErr(5, "cannot assign to procedure");
                                Expr‿NT(out type);
                                addAlt(19); // T ";"
                                Expect(19); // ";"
                                if (type != obj.type) SemErr(6, "incompatible types");
                                if (obj.level == 0) gen.Emit(Op.STOG, obj.adr);
                                else gen.Emit(Op.STO, obj.adr);
                            }
                            else if (isKind(la, 11))
                            {
                                Get();
                                addAlt(12); // T ")"
                                Expect(12); // ")"
                                addAlt(19); // T ";"
                                Expect(19); // ";"
                                if (obj.kind != proc) SemErr(7, "object is not a procedure");
                                gen.Emit(Op.CALL, obj.adr);
                            } // end if
                            else
                                SynErr(36);
                        }
                        break;
                    case 20: // "rel"
                        { // scoping
                            Get();
                            RelOp‿NT(out var op);
                            addAlt(19); // T ";"
                            Expect(19); // ";"
                        }
                        break;
                    case 21: // "if"
                        { // scoping
                            Get();
                            addAlt(11); // T "("
                            Expect(11); // "("
                            Expr‿NT(out type);
                            addAlt(12); // T ")"
                            Expect(12); // ")"
                            if (type != boolean) SemErr(8, "boolean type expected");
                            gen.Emit(Op.FJMP, 0); adr = gen.pc - 2;
                            Stat‿NT();
                            addAlt(22); // OPT
                            if (isKind(la, 22))
                            {
                                Get();
                                gen.Emit(Op.JMP, 0); adr2 = gen.pc - 2;
                                gen.Patch(adr, gen.pc);
                                adr = adr2;
                                Stat‿NT();
                            }
                            gen.Patch(adr, gen.pc);
                        }
                        break;
                    case 23: // "while"
                        { // scoping
                            Get();
                            loopstart = gen.pc;
                            addAlt(11); // T "("
                            Expect(11); // "("
                            Expr‿NT(out type);
                            addAlt(12); // T ")"
                            Expect(12); // ")"
                            if (type != boolean) SemErr(9, "boolean type expected");
                            gen.Emit(Op.FJMP, 0); adr = gen.pc - 2;
                            Stat‿NT();
                            gen.Emit(Op.JMP, loopstart); gen.Patch(adr, gen.pc);
                        }
                        break;
                    case 24: // "read"
                        { // scoping
                            Get();
                            Ident‿NT(out name);
                            addAlt(19); // T ";"
                            Expect(19); // ";"
                            obj = tab.Find(name);
                            if (obj.type != integer) SemErr(10, "integer type expected");
                            gen.Emit(Op.READ);
                            if (obj.level == 0) gen.Emit(Op.STOG, obj.adr);
                            else gen.Emit(Op.STO, obj.adr);
                        }
                        break;
                    case 25: // "write"
                        { // scoping
                            Get();
                            Expr‿NT(out type);
                            addAlt(19); // T ";"
                            Expect(19); // ";"
                            if (type != integer) SemErr(11, "integer type expected");
                            gen.Emit(Op.WRITE);
                        }
                        break;
                    case 13: // "{"
                        { // scoping
                            Get();
                            addAlt(set0, 1); // ITER start
                            while (StartOf(1))
                            {
                                addAlt(set0, 2); // ALT
                                addAlt(new int[] {27, 28}); // ALT
                                if (StartOf(2))
                                {
                                    Stat‿NT();
                                }
                                else
                                {
                                    VarDecl‿NT();
                                }
                                addAlt(set0, 1); // ITER end
                            }
                            addAlt(14); // T "}"
                            Expect(14); // "}"
                        }
                        break;
                    default:
                        SynErr(37);
                        break;
                } // end switch
            }
        }


        void Term‿NT(out int type)
        {
            {
                int type1; Op op;
                Factor‿NT(out type);
                addAlt(new int[] {8, 9}); // ITER start
                while (isKind(la, 8) || isKind(la, 9))
                {
                    MulOp‿NT(out op);
                    Factor‿NT(out type1);
                    if (type != integer || type1 != integer)
                    SemErr(13, "integer type expected");
                    gen.Emit(op);
                    addAlt(new int[] {8, 9}); // ITER end
                }
            }
        }


        void Taste‿NT()
        {
            {
                string name;
                addAlt(26); // T "program"
                Expect(26); // "program"
                Ident‿NT(out name);
                tab.OpenScope();
                addAlt(13); // T "{"
                Expect(13); // "{"
                addAlt(new int[] {10, 27, 28}); // ITER start
                while (isKind(la, 10) || isKind(la, 27) || isKind(la, 28))
                {
                    addAlt(new int[] {27, 28}); // ALT
                    addAlt(10); // ALT
                    if (isKind(la, 27) || isKind(la, 28))
                    {
                        VarDecl‿NT();
                    }
                    else
                    {
                        ProcDecl‿NT();
                    }
                    addAlt(new int[] {10, 27, 28}); // ITER end
                }
                addAlt(14); // T "}"
                Expect(14); // "}"
                tab.CloseScope();
                if (gen.progStart == -1) SemErr(12, "main function never defined");
            }
        }


        void Type‿NT(out int type)
        {
            {
                type = undef;
                addAlt(27); // ALT
                addAlt(28); // ALT
                if (isKind(la, 27))
                {
                    Get();
                    type = integer;
                }
                else if (isKind(la, 28))
                {
                    Get();
                    type = boolean;
                } // end if
                else
                    SynErr(38);
            }
        }



        public override void Parse() 
        {
            if (scanner == null) throw new FatalError("This parser is not Initialize()-ed.");
            lb = Token.Zero;
            la = Token.Zero;
            Get();
            Taste‿NT();
            Expect(0);
        }
    
        // a token's base type
        public static readonly int[] tBase = {
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1
        };
		protected override int BaseKindOf(int kind) => tBase[kind];

        // a token's name
        public static readonly string[] varTName = {
            "[EOF]",
            "[ident]",
            "[number]",
            ">=",
            "+",
            "-",
            "true",
            "false",
            "*",
            "/",
            "void",
            "(",
            ")",
            "{",
            "}",
            "==",
            "<",
            ">",
            "=",
            ";",
            "rel",
            "if",
            "else",
            "while",
            "read",
            "write",
            "program",
            "int",
            "bool",
            ",",
            "[???]"
        };
        public override string NameOfTokenKind(int tokenKind) => varTName[tokenKind];


        // as set0 but with token inheritance taken into account
        static readonly bool[,] set = {
            {_T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x},
            {_x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _T,_T,_x,_T,  _T,_T,_x,_T,  _T,_x,_x,_x},
            {_x,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_x,_x,  _x,_x,_x,_x,  _T,_T,_x,_T,  _T,_T,_x,_x,  _x,_x,_x,_x}
        };

        protected override bool StartOf(int s, int kind) => set[s, kind];



        public override string Syntaxerror(int n) 
        {
            switch (n) 
            {
                case 1: return "[EOF] expected";
                case 2: return "[ident] expected";
                case 3: return "[number] expected";
                case 4: return ">= expected";
                case 5: return "+ expected";
                case 6: return "- expected";
                case 7: return "true expected";
                case 8: return "false expected";
                case 9: return "* expected";
                case 10: return "/ expected";
                case 11: return "void expected";
                case 12: return "( expected";
                case 13: return ") expected";
                case 14: return "{ expected";
                case 15: return "} expected";
                case 16: return "== expected";
                case 17: return "< expected";
                case 18: return "> expected";
                case 19: return "= expected";
                case 20: return "; expected";
                case 21: return "rel expected";
                case 22: return "if expected";
                case 23: return "else expected";
                case 24: return "while expected";
                case 25: return "read expected";
                case 26: return "write expected";
                case 27: return "program expected";
                case 28: return "int expected";
                case 29: return "bool expected";
                case 30: return ", expected";
                case 31: return "[???] expected";
                case 32: return "invalid AddOp, expected + -";
                case 33: return "invalid RelOp, expected == < >";
                case 34: return "invalid Factor, expected [ident] [number] - true false";
                case 35: return "invalid MulOp, expected * /";
                case 36: return "invalid Stat, expected = (";
                case 37: return "invalid Stat, expected [ident] rel if while read write {";
                case 38: return "invalid Type, expected int bool";
                default: return $"error {n}";
            }
        }

    } // end Parser

// end namespace implicit
}
