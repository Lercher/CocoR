using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using CocoRCore;

namespace CocoRCore.Samples.ExternalTables
{

    public class Parser : ParserBase 
    {
        public const int _number = 1; // TOKEN number
        public const int _ident = 2; // TOKEN ident
        public const int _set = 3; // TOKEN set INHERITS ident
        public const int _serveroutput = 4; // TOKEN serveroutput INHERITS ident
        public const int _on = 5; // TOKEN on INHERITS ident
        public const int _size = 6; // TOKEN size INHERITS ident
        public const int _insert = 7; // TOKEN insert INHERITS ident
        public const int _update = 8; // TOKEN update INHERITS ident
        public const int _delete = 9; // TOKEN delete INHERITS ident
        public const int _into = 10; // TOKEN into INHERITS ident
        public const int _values = 11; // TOKEN values INHERITS ident
        public const int _prompt = 12; // TOKEN prompt INHERITS ident
        public const int _null = 13; // TOKEN null INHERITS ident
        public const int _lantusparam = 14; // TOKEN lantusparam INHERITS ident
        public const int _tusparam = 15; // TOKEN tusparam INHERITS ident
        public const int _tusnom = 16; // TOKEN tusnom INHERITS ident
        public const int _tupcode = 17; // TOKEN tupcode INHERITS ident
        public const int _tupflagorfi = 18; // TOKEN tupflagorfi INHERITS ident
        public const int _tuplibelle = 19; // TOKEN tuplibelle INHERITS ident
        public const int _sc = 20; // TOKEN sc
        public const int _openparen = 21; // TOKEN openparen
        public const int _closeparen = 22; // TOKEN closeparen
        public const int _slash = 23; // TOKEN slash
        public const int _dot = 24; // TOKEN dot
        public const int _comma = 25; // TOKEN comma
        public const int _equals = 26; // TOKEN equals
        public const int _doublebar = 27; // TOKEN doublebar
        public const int _string = 28; // TOKEN string
        public const int _stars = 29; // TOKEN stars
        private const int __maxT = 63;
        private const bool _T = true;
        private const bool _x = false;
        
        public readonly Symboltable languages;
        public readonly Symboltable deletabletables;
        public readonly Symboltable updatetables;
        public readonly Symboltable columns;
        public readonly Symboltable chrarguments;
        public Symboltable symbols(string name)
        {
            if (name == "languages") return languages;
            if (name == "deletabletables") return deletabletables;
            if (name == "updatetables") return updatetables;
            if (name == "columns") return columns;
            if (name == "chrarguments") return chrarguments;
            return null;
        }


void Diag(int n, string s, string kind) {
    var msg = $"[{kind}] {s}";
    if (kind == "CRIT")
        SemErr(n, msg);
    else
        Warning(n, msg);
}



        public Parser()
        {
            languages = new Symboltable("languages", true, true, this);
            deletabletables = new Symboltable("deletabletables", true, true, this);
            updatetables = new Symboltable("updatetables", true, true, this);
            columns = new Symboltable("columns", true, true, this);
            chrarguments = new Symboltable("chrarguments", true, true, this);
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
                AlternativeTokens.Add(new Alternative(t, alternatives));
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


        void ExternalTables‿NT()
        {
            {
                addAlt(12); // OPT
                if (isKind(la, 12 /*prompt*/))
                {
                    StarPrompt‿NT();
                    Slash‿NT();
                }
                SetServeroutput‿NT();
                Slash‿NT();
                addAlt(set0, 1); // ITER start
                while (StartOf(1))
                {
                    Block‿NT();
                    addAlt(set0, 1); // ITER end
                }
                addAlt(0); // T EOF
                Expect(0 /*[EOF]*/);
            }
        }


        void StarPrompt‿NT()
        {
            {
                addAlt(12); // T prompt
                Expect(12 /*prompt*/);
                addAlt(29); // T stars
                Expect(29 /*[stars]*/);
                addAlt(set0, 2); // ITER start
                while (StartOf(2))
                {
                    Get();
                    addAlt(set0, 2); // ITER end
                }
                addAlt(29); // T stars
                Expect(29 /*[stars]*/);
            }
        }


        void Slash‿NT()
        {
            {
                while (!(isKind(la, 0 /*[EOF]*/) || isKind(la, 23 /*/*/)))
                {
                    SynErr(65);
                    Get();
                }
                addAlt(23); // T slash
                Expect(23 /*/*/);
            }
        }


        void SetServeroutput‿NT()
        {
            {
                addAlt(3); // T set
                Expect(3 /*set*/);
                addAlt(4); // T serveroutput
                Expect(4 /*serveroutput*/);
                addAlt(5); // T on
                Expect(5 /*on*/);
                addAlt(6); // T size
                Expect(6 /*size*/);
                addAlt(1); // T number
                Expect(1 /*[number]*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void Block‿NT()
        {
            {
                addAlt(new int[] {3, 31}); // ALT
                addAlt(12); // ALT
                addAlt(30); // ALT
                if (isKind(la, 3 /*set*/) || isKind(la, 31 /*begin*/))
                {
                    ExceptionHandledBlock‿NT();
                }
                else if (isKind(la, 12 /*prompt*/))
                {
                    StarPrompt‿NT();
                }
                else if (isKind(la, 30 /*declare*/))
                {
                    DeclareBlock‿NT();
                } // end if
                else
                    SynErr(66);
                addAlt(23); // ITER start
                while (isKind(la, 23 /*/*/))
                {
                    Slash‿NT();
                    addAlt(23); // ITER end
                }
            }
        }


        void ExceptionHandledBlock‿NT()
        {
            {
                addAlt(3); // OPT
                if (isKind(la, 3 /*set*/))
                {
                    Get();
                    addAlt(4); // T serveroutput
                    Expect(4 /*serveroutput*/);
                    addAlt(5); // T on
                    Expect(5 /*on*/);
                    addAlt(20); // T sc
                    Expect(20 /*;*/);
                }
                addAlt(31); // T "begin"
                Expect(31 /*begin*/);
                InsertDeleteUpdate‿NT();
                addAlt(new int[] {7, 8, 9}); // ITER start
                while (isKind(la, 7 /*insert*/) || isKind(la, 8 /*update*/) || isKind(la, 9 /*delete*/))
                {
                    InsertDeleteUpdate‿NT();
                    addAlt(new int[] {7, 8, 9}); // ITER end
                }
                addAlt(32); // ALT
                addAlt(new int[] {33, 59}); // ALT
                if (isKind(la, 32 /*commit*/))
                {
                    Get();
                    addAlt(20); // T sc
                    Expect(20 /*;*/);
                }
                else if (isKind(la, 33 /*exception*/) || isKind(la, 59 /*dbms_output*/))
                {
                    Diag(2, "COMMIT in ExceptionHandledBlock missing", "CRIT");
                } // end if
                else
                    SynErr(67);
                addAlt(59); // ALT
                addAlt(33); // ALT
                if (isKind(la, 59 /*dbms_output*/))
                {
                    PutLine‿NT();
                }
                else if (isKind(la, 33 /*exception*/))
                {
                    Diag(3, "COMMIT without dbms_output.PUT_LINE", "WARN");
                } // end if
                else
                    SynErr(68);
                addAlt(33); // T "exception"
                Expect(33 /*exception*/);
                addAlt(34); // T "when"
                Expect(34 /*when*/);
                addAlt(35); // T "others"
                Expect(35 /*others*/);
                addAlt(36); // T "then"
                Expect(36 /*then*/);
                addAlt(59); // OPT
                if (isKind(la, 59 /*dbms_output*/))
                {
                    PutLine‿NT();
                }
                addAlt(37); // ALT
                addAlt(13); // ALT
                if (isKind(la, 37 /*rollback*/))
                {
                    Get();
                }
                else if (isKind(la, 13 /*null*/))
                {
                    Get();
                    Diag(20, "Null instead of ROLLBACK in exception block", "WARN");
                } // end if
                else
                    SynErr(69);
                addAlt(20); // T sc
                Expect(20 /*;*/);
                addAlt(59); // OPT
                if (isKind(la, 59 /*dbms_output*/))
                {
                    PutLine‿NT();
                }
                addAlt(38); // T "end"
                Expect(38 /*end*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
                Slash‿NT();
            }
        }


        void DeclareBlock‿NT()
        {
            {
                addAlt(30); // T "declare"
                Expect(30 /*declare*/);
                Diag(1, "Mysterious DECLARE block detected", "TBD");
                addAlt(set0, 3); // ITER start
                while (StartOf(3))
                {
                    Get();
                    addAlt(set0, 3); // ITER end
                }
                Slash‿NT();
            }
        }


        void InsertDeleteUpdate‿NT()
        {
            {
                addAlt(7); // ALT
                addAlt(9); // ALT
                addAlt(8); // ALT
                if (isKind(la, 7 /*insert*/))
                {
                    Insert‿NT();
                }
                else if (isKind(la, 9 /*delete*/))
                {
                    Delete‿NT();
                }
                else if (isKind(la, 8 /*update*/))
                {
                    Update‿NT();
                } // end if
                else
                    SynErr(70);
            }
        }


        void PutLine‿NT()
        {
            {
                addAlt(59); // T "dbms_output"
                Expect(59 /*dbms_output*/);
                addAlt(24); // T dot
                Expect(24 /*.*/);
                addAlt(60); // T "put_line"
                Expect(60 /*PUT_LINE*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                UncheckedString‿NT();
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void Insert‿NT()
        {
            {
                addAlt(7); // T insert
                Expect(7 /*insert*/);
                addAlt(10); // T into
                Expect(10 /*into*/);
                addAlt(54); // ALT
                addAlt(56); // ALT
                addAlt(15); // ALT
                addAlt(14); // ALT
                addAlt(42); // ALT
                addAlt(45); // ALT
                addAlt(49); // ALT
                addAlt(51); // ALT
                switch (la.kind)
                {
                    case 54: /*TUSER*/
                        { // scoping
                            TUSER‿NT();
                        }
                        break;
                    case 56: /*LANTUSER*/
                        { // scoping
                            LANTUSER‿NT();
                        }
                        break;
                    case 15: /*tusparam*/
                        { // scoping
                            TUSPARAM‿NT();
                        }
                        break;
                    case 14: /*lantusparam*/
                        { // scoping
                            LANTUSPARAM‿NT();
                        }
                        break;
                    case 42: /*TTRPARAM*/
                        { // scoping
                            TTRPARAM‿NT();
                        }
                        break;
                    case 45: /*LANTTRPARAM*/
                        { // scoping
                            LANTTRPARAM‿NT();
                        }
                        break;
                    case 49: /*TTRAITEMENT*/
                        { // scoping
                            TTRAITEMENT‿NT();
                        }
                        break;
                    case 51: /*LANTTRAITEMENT*/
                        { // scoping
                            LANTTRAITEMENT‿NT();
                        }
                        break;
                    default:
                        SynErr(71);
                        break;
                } // end switch
            }
        }


        void Delete‿NT()
        {
            {
                addAlt(9); // T delete
                Expect(9 /*delete*/);
                addAlt(41); // T "from"
                Expect(41 /*from*/);
                if (!deletabletables.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "ident", la.val, deletabletables.name), la);
                addAlt(2); // T ident
                addAlt(2, deletabletables); // T ident ident uses symbol table 'deletabletables'
                Expect(2 /*[ident]*/);
                addAlt(39); // T "where"
                Expect(39 /*where*/);
                if (!columns.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "ident", la.val, columns.name), la);
                addAlt(2); // T ident
                addAlt(2, columns); // T ident ident uses symbol table 'columns'
                Expect(2 /*[ident]*/);
                addAlt(26); // T equals
                Expect(26 /*=*/);
                String‿NT();
                addAlt(40); // ITER start
                while (isKind(la, 40 /*and*/))
                {
                    Get();
                    Get();
                    addAlt(set0, 4); // ITER start
                    while (StartOf(4))
                    {
                        Get();
                        addAlt(set0, 4); // ITER end
                    }
                    addAlt(40); // ITER end
                }
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void Update‿NT()
        {
            {
                addAlt(8); // T update
                Expect(8 /*update*/);
                if (!updatetables.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "ident", la.val, updatetables.name), la);
                addAlt(2); // T ident
                addAlt(2, updatetables); // T ident ident uses symbol table 'updatetables'
                Expect(2 /*[ident]*/);
                addAlt(3); // T set
                Expect(3 /*set*/);
                if (!columns.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "ident", la.val, columns.name), la);
                addAlt(2); // T ident
                addAlt(2, columns); // T ident ident uses symbol table 'columns'
                Expect(2 /*[ident]*/);
                addAlt(26); // T equals
                Expect(26 /*=*/);
                addAlt(1); // ALT
                addAlt(28); // ALT
                addAlt(2); // ALT
                addAlt(2, columns); // ALT ident uses symbol table 'columns'
                if (isKind(la, 1 /*[number]*/))
                {
                    Get();
                }
                else if (isKind(la, 28 /*[string]*/))
                {
                    Get();
                }
                else if (isKind(la, 2 /*[ident]*/))
                {
                    if (!columns.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "ident", la.val, columns.name), la);
                    Get();
                } // end if
                else
                    SynErr(72);
                addAlt(39); // T "where"
                Expect(39 /*where*/);
                if (!columns.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "ident", la.val, columns.name), la);
                addAlt(2); // T ident
                addAlt(2, columns); // T ident ident uses symbol table 'columns'
                Expect(2 /*[ident]*/);
                addAlt(26); // T equals
                Expect(26 /*=*/);
                String‿NT();
                addAlt(40); // ITER start
                while (isKind(la, 40 /*and*/))
                {
                    Get();
                    Get();
                    addAlt(set0, 4); // ITER start
                    while (StartOf(4))
                    {
                        Get();
                        addAlt(set0, 4); // ITER end
                    }
                    addAlt(40); // ITER end
                }
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void String‿NT()
        {
            {
                StringFactor‿NT();
                if (t.kind == _string && t.val.StartsWith("' ")) {
                if (t.val.ToUpper() == t.val && t.val.Replace(" ", "").Length + 1 == t.val.Length)
                Diag(24, "Key-kind string literal starts with a space: " + t.val, "CRIT");
                else
                Diag(17, "First string literal starts with a space: " + t.val, "WARN");
                }
                addAlt(27); // ITER start
                while (isKind(la, 27 /*||*/))
                {
                    Get();
                    StringFactor‿NT();
                    addAlt(27); // ITER end
                }
                if (t.kind == _string && t.val.EndsWith(" '")) {
                if (t.val.ToUpper() == t.val && t.val.Replace(" ", "").Length + 1 == t.val.Length)
                Diag(25, "Key-kind string literal ends with a space: " + t.val, "CRIT");
                else
                Diag(18, "Last string literal ends with a space: " + t.val, "WARN");
                }
            }
        }


        void TUSER‿NT()
        {
            {
                addAlt(54); // T "tuser"
                Expect(54 /*TUSER*/);
                addAlt(21); // ALT
                addAlt(11); // ALT
                if (isKind(la, 21 /*(*/))
                {
                    Get();
                    addAlt(16); // T tusnom
                    Expect(16 /*tusnom*/);
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    addAlt(55); // T "tuslongueur"
                    Expect(55 /*TUSLONGUEUR*/);
                    addAlt(22); // T closeparen
                    Expect(22 /*)*/);
                }
                else if (isKind(la, 11 /*values*/))
                {
                    Diag(10, "Insert into TUSER without column list", "WARN");
                } // end if
                else
                    SynErr(73);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(1); // ALT
                addAlt(13); // ALT
                addAlt(28); // ALT
                if (isKind(la, 1 /*[number]*/))
                {
                    Get();
                }
                else if (isKind(la, 13 /*null*/))
                {
                    Get();
                    Diag(11, t.val + " as TUSLONGUEUR (a length) is invalid", "TBD");
                }
                else if (isKind(la, 28 /*[string]*/))
                {
                    Get();
                    Diag(12, t.val + " (string) as TUSLONGUEUR (a length) is invalid", "TBD");
                } // end if
                else
                    SynErr(74);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void LANTUSER‿NT()
        {
            {
                addAlt(56); // T "lantuser"
                Expect(56 /*LANTUSER*/);
                addAlt(21); // ALT
                addAlt(11); // ALT
                if (isKind(la, 21 /*(*/))
                {
                    Get();
                    addAlt(16); // T tusnom
                    Expect(16 /*tusnom*/);
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    addAlt(46); // ALT
                    addAlt(57); // ALT
                    if (isKind(la, 46 /*LANCODE*/))
                    {
                        LANTUSER_LANCODE‿NT();
                    }
                    else if (isKind(la, 57 /*TUSLIBELLE*/))
                    {
                        LANTUSER_TUSLIBELLE‿NT();
                    } // end if
                    else
                        SynErr(75);
                }
                else if (isKind(la, 11 /*values*/))
                {
                    Get();
                    addAlt(21); // T openparen
                    Expect(21 /*(*/);
                    String‿NT();
                    Diag(13, "INSERT INTO LANTUSER without column list: " + t.val, "WARN");
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    String‿NT();
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    String‿NT();
                    addAlt(22); // T closeparen
                    Expect(22 /*)*/);
                    addAlt(20); // T sc
                    Expect(20 /*;*/);
                } // end if
                else
                    SynErr(76);
            }
        }


        void TUSPARAM‿NT()
        {
            {
                bool isOrfi = false;
                addAlt(15); // T tusparam
                Expect(15 /*tusparam*/);
                addAlt(21); // ALT
                addAlt(11); // ALT
                if (isKind(la, 21 /*(*/))
                {
                    Get();
                    addAlt(16); // T tusnom
                    Expect(16 /*tusnom*/);
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    addAlt(17); // T tupcode
                    Expect(17 /*tupcode*/);
                    addAlt(25); // OPT
                    if (isKind(la, 25 /*,*/))
                    {
                        Get();
                        addAlt(18); // T tupflagorfi
                        Expect(18 /*tupflagorfi*/);
                    }
                    addAlt(22); // T closeparen
                    Expect(22 /*)*/);
                }
                else if (isKind(la, 11 /*values*/))
                {
                    Diag(14, "INSERT INTO TUSPARAM without column list", "WARN");
                } // end if
                else
                    SynErr(77);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // OPT
                if (isKind(la, 25 /*,*/))
                {
                    Get();
                    addAlt(1); // ALT
                    addAlt(13); // ALT
                    addAlt(28); // ALT
                    if (isKind(la, 1 /*[number]*/))
                    {
                        Get();
                        isOrfi = true;
                    }
                    else if (isKind(la, 13 /*null*/))
                    {
                        Get();
                    }
                    else if (isKind(la, 28 /*[string]*/))
                    {
                        Get();
                        Diag(15, "TUPFLAGORFI (a number) is assigned the string " + t.val, "CRIT");
                    } // end if
                    else
                        SynErr(78);
                }
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
                if (isOrfi)
                Diag(21, "Suspicious INSERT to table TUSPARAM in externaltables with ORFI set", "WARN");
                else
                Diag(22, "Forbidden INSERT to table TUSPARAM in externaltables", "CRIT");
            }
        }


        void LANTUSPARAM‿NT()
        {
            {
                addAlt(14); // T lantusparam
                Expect(14 /*lantusparam*/);
                addAlt(21); // ALT
                addAlt(11); // ALT
                if (isKind(la, 21 /*(*/))
                {
                    Get();
                    addAlt(16); // T tusnom
                    Expect(16 /*tusnom*/);
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    addAlt(17); // T tupcode
                    Expect(17 /*tupcode*/);
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    addAlt(46); // ALT
                    addAlt(19); // ALT
                    if (isKind(la, 46 /*LANCODE*/))
                    {
                        LANTUSPARAM_LANCODE‿NT();
                    }
                    else if (isKind(la, 19 /*tuplibelle*/))
                    {
                        LANTUSPARAM_TUPLIBELLE‿NT();
                    } // end if
                    else
                        SynErr(79);
                }
                else if (isKind(la, 11 /*values*/))
                {
                    Get();
                    addAlt(21); // T openparen
                    Expect(21 /*(*/);
                    String‿NT();
                    Diag(16, "INSERT INTO LANTUSPARAM without column list: " + t.val, "WARN");
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    String‿NT();
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    if (!languages.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "string", la.val, languages.name), la);
                    addAlt(28); // T string
                    addAlt(28, languages); // T string string uses symbol table 'languages'
                    Expect(28 /*[string]*/);
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    String‿NT();
                    addAlt(25); // T comma
                    Expect(25 /*,*/);
                    addAlt(new int[] {28, 61, 62}); // ALT
                    addAlt(13); // ALT
                    if (isKind(la, 28 /*[string]*/) || isKind(la, 61 /*chr*/) || isKind(la, 62 /*SQLERRM*/))
                    {
                        String‿NT();
                    }
                    else if (isKind(la, 13 /*null*/))
                    {
                        Get();
                    } // end if
                    else
                        SynErr(80);
                    addAlt(22); // T closeparen
                    Expect(22 /*)*/);
                    addAlt(20); // T sc
                    Expect(20 /*;*/);
                } // end if
                else
                    SynErr(81);
            }
        }


        void TTRPARAM‿NT()
        {
            {
                addAlt(42); // T "ttrparam"
                Expect(42 /*TTRPARAM*/);
                Diag(4, "Forbidden insert into TTRPARAM in externaltables", "CRIT");
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                addAlt(43); // T "ttrnom"
                Expect(43 /*TTRNOM*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(44); // T "ttpcode"
                Expect(44 /*TTPCODE*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void LANTTRPARAM‿NT()
        {
            {
                addAlt(45); // T "lanttrparam"
                Expect(45 /*LANTTRPARAM*/);
                Diag(5, "Forbidden insert into LANTTRPARAM in externaltables", "CRIT");
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                addAlt(43); // T "ttrnom"
                Expect(43 /*TTRNOM*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(44); // T "ttpcode"
                Expect(44 /*TTPCODE*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(46); // ALT
                addAlt(47); // ALT
                if (isKind(la, 46 /*LANCODE*/))
                {
                    LANTTRPARAM_LANCODE‿NT();
                }
                else if (isKind(la, 47 /*TTPLIBELLE*/))
                {
                    LANTTRPARAM_TTPLIBELLE‿NT();
                } // end if
                else
                    SynErr(82);
                addAlt(25); // ALT
                addAlt(22); // ALT
                if (isKind(la, 25 /*,*/))
                {
                    Get();
                    addAlt(new int[] {28, 61, 62}); // ALT
                    addAlt(13); // ALT
                    if (isKind(la, 28 /*[string]*/) || isKind(la, 61 /*chr*/) || isKind(la, 62 /*SQLERRM*/))
                    {
                        String‿NT();
                    }
                    else if (isKind(la, 13 /*null*/))
                    {
                        Get();
                        Diag(6, "LANTTRPARAM without Helptext (null)", "WARN");
                    } // end if
                    else
                        SynErr(83);
                }
                else if (isKind(la, 22 /*)*/))
                {
                    Diag(7, "LANTTRPARAM without Helptext (column missing)", "WARN");
                } // end if
                else
                    SynErr(84);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void TTRAITEMENT‿NT()
        {
            {
                addAlt(49); // T "ttraitement"
                Expect(49 /*TTRAITEMENT*/);
                Diag(8, "Forbidden insert into TTRAITEMENT in externaltables", "CRIT");
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                addAlt(43); // T "ttrnom"
                Expect(43 /*TTRNOM*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(50); // T "ttrflagpref"
                Expect(50 /*TTRFLAGPREF*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(1); // T number
                Expect(1 /*[number]*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void LANTTRAITEMENT‿NT()
        {
            {
                addAlt(51); // T "lanttraitement"
                Expect(51 /*LANTTRAITEMENT*/);
                Diag(9, "Forbidden insert into LANTTRAITEMENT in externaltables", "CRIT");
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                addAlt(43); // T "ttrnom"
                Expect(43 /*TTRNOM*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(46); // ALT
                addAlt(52); // ALT
                if (isKind(la, 46 /*LANCODE*/))
                {
                    LANTTRAITEMENT_LANCODE‿NT();
                }
                else if (isKind(la, 52 /*TTRLIBELLE*/))
                {
                    LANTTRAITEMENT_TTRLIBELLE‿NT();
                } // end if
                else
                    SynErr(85);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void LANTTRPARAM_LANCODE‿NT()
        {
            {
                addAlt(46); // T "lancode"
                Expect(46 /*LANCODE*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(47); // T "ttplibelle"
                Expect(47 /*TTPLIBELLE*/);
                addAlt(25); // OPT
                if (isKind(la, 25 /*,*/))
                {
                    Get();
                    addAlt(48); // T "ttphelptext"
                    Expect(48 /*TTPHELPTEXT*/);
                }
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                if (!languages.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "string", la.val, languages.name), la);
                addAlt(28); // T string
                addAlt(28, languages); // T string string uses symbol table 'languages'
                Expect(28 /*[string]*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
            }
        }


        void LANTTRPARAM_TTPLIBELLE‿NT()
        {
            {
                addAlt(47); // T "ttplibelle"
                Expect(47 /*TTPLIBELLE*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(46); // T "lancode"
                Expect(46 /*LANCODE*/);
                addAlt(25); // OPT
                if (isKind(la, 25 /*,*/))
                {
                    Get();
                    addAlt(48); // T "ttphelptext"
                    Expect(48 /*TTPHELPTEXT*/);
                }
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                if (!languages.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "string", la.val, languages.name), la);
                addAlt(28); // T string
                addAlt(28, languages); // T string string uses symbol table 'languages'
                Expect(28 /*[string]*/);
            }
        }


        void LANTTRAITEMENT_LANCODE‿NT()
        {
            {
                addAlt(46); // T "lancode"
                Expect(46 /*LANCODE*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(52); // T "ttrlibelle"
                Expect(52 /*TTRLIBELLE*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(53); // T "ttrcontext"
                Expect(53 /*TTRCONTEXT*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                if (!languages.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "string", la.val, languages.name), la);
                addAlt(28); // T string
                addAlt(28, languages); // T string string uses symbol table 'languages'
                Expect(28 /*[string]*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
            }
        }


        void LANTTRAITEMENT_TTRLIBELLE‿NT()
        {
            {
                addAlt(52); // T "ttrlibelle"
                Expect(52 /*TTRLIBELLE*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(53); // T "ttrcontext"
                Expect(53 /*TTRCONTEXT*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(46); // T "lancode"
                Expect(46 /*LANCODE*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                if (!languages.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "string", la.val, languages.name), la);
                addAlt(28); // T string
                addAlt(28, languages); // T string string uses symbol table 'languages'
                Expect(28 /*[string]*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
            }
        }


        void LANTUSER_LANCODE‿NT()
        {
            {
                addAlt(46); // T "lancode"
                Expect(46 /*LANCODE*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(57); // T "tuslibelle"
                Expect(57 /*TUSLIBELLE*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                if (!languages.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "string", la.val, languages.name), la);
                addAlt(28); // T string
                addAlt(28, languages); // T string string uses symbol table 'languages'
                Expect(28 /*[string]*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void LANTUSER_TUSLIBELLE‿NT()
        {
            {
                addAlt(57); // T "tuslibelle"
                Expect(57 /*TUSLIBELLE*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(46); // T "lancode"
                Expect(46 /*LANCODE*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                if (!languages.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "string", la.val, languages.name), la);
                addAlt(28); // T string
                addAlt(28, languages); // T string string uses symbol table 'languages'
                Expect(28 /*[string]*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void LANTUSPARAM_LANCODE‿NT()
        {
            {
                addAlt(46); // T "lancode"
                Expect(46 /*LANCODE*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(19); // T tuplibelle
                Expect(19 /*tuplibelle*/);
                addAlt(25); // OPT
                if (isKind(la, 25 /*,*/))
                {
                    Get();
                    addAlt(58); // T "tuphelptext"
                    Expect(58 /*TUPHELPTEXT*/);
                }
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                if (!languages.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "string", la.val, languages.name), la);
                addAlt(28); // T string
                addAlt(28, languages); // T string string uses symbol table 'languages'
                Expect(28 /*[string]*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // OPT
                if (isKind(la, 25 /*,*/))
                {
                    Get();
                    addAlt(new int[] {28, 61, 62}); // ALT
                    addAlt(13); // ALT
                    if (isKind(la, 28 /*[string]*/) || isKind(la, 61 /*chr*/) || isKind(la, 62 /*SQLERRM*/))
                    {
                        String‿NT();
                    }
                    else if (isKind(la, 13 /*null*/))
                    {
                        Get();
                    } // end if
                    else
                        SynErr(86);
                }
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void LANTUSPARAM_TUPLIBELLE‿NT()
        {
            {
                addAlt(19); // T tuplibelle
                Expect(19 /*tuplibelle*/);
                addAlt(25); // T comma
                Expect(25 /*,*/);
                addAlt(46); // T "lancode"
                Expect(46 /*LANCODE*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(11); // T values
                Expect(11 /*values*/);
                addAlt(21); // T openparen
                Expect(21 /*(*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                String‿NT();
                addAlt(25); // T comma
                Expect(25 /*,*/);
                if (!languages.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "string", la.val, languages.name), la);
                addAlt(28); // T string
                addAlt(28, languages); // T string string uses symbol table 'languages'
                Expect(28 /*[string]*/);
                addAlt(22); // T closeparen
                Expect(22 /*)*/);
                addAlt(20); // T sc
                Expect(20 /*;*/);
            }
        }


        void UncheckedString‿NT()
        {
            {
                StringFactor‿NT();
                addAlt(27); // ITER start
                while (isKind(la, 27 /*||*/))
                {
                    Get();
                    StringFactor‿NT();
                    addAlt(27); // ITER end
                }
            }
        }


        void StringFactor‿NT()
        {
            {
                addAlt(28); // ALT
                addAlt(61); // ALT
                addAlt(62); // ALT
                if (isKind(la, 28 /*[string]*/))
                {
                    Get();
                    if (t.val.Contains("\n"))
                    Diag(19, "Illegal line break in string literal", "CRIT");
                    if (t.val.Contains("Ã"))
                    Diag(23, "Suspicious UTF-8/ANSI char in string literal: " + t.val, "WARN");
                }
                else if (isKind(la, 61 /*chr*/))
                {
                    Get();
                    addAlt(21); // T openparen
                    Expect(21 /*(*/);
                    if (!chrarguments.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "number", la.val, chrarguments.name), la);
                    addAlt(1); // T number
                    addAlt(1, chrarguments); // T number number uses symbol table 'chrarguments'
                    Expect(1 /*[number]*/);
                    addAlt(22); // T closeparen
                    Expect(22 /*)*/);
                }
                else if (isKind(la, 62 /*SQLERRM*/))
                {
                    Get();
                } // end if
                else
                    SynErr(87);
            }
        }



        public override void Parse() 
        {
            if (scanner == null) throw new FatalError("This parser is not Initialize()-ed.");
            lb = Token.Zero;
            la = Token.Zero;
            Get();
            languages.Add("\'en\'");
            languages.Add("\'fr\'");
            deletabletables.Add("lantusparam");
            deletabletables.Add("tusparam");
            deletabletables.Add("lktuptactpg");
            updatetables.Add("tusparam");
            updatetables.Add("lantusparam");
            columns.Add("tusnom");
            columns.Add("tupcode");
            columns.Add("tupflagorfi");
            chrarguments.Add("38");
            ExternalTables‿NT();
            Expect(0);
            languages.CheckDeclared();
            deletabletables.CheckDeclared();
            updatetables.CheckDeclared();
            columns.CheckDeclared();
            chrarguments.CheckDeclared();
        }
    
        // a token's base type
        public static readonly int[] tBase = {
            -1,-1,-1, 2,   2, 2, 2, 2,   2, 2, 2, 2,   2, 2, 2, 2,   2, 2, 2, 2,
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,
            -1,-1,-1,-1
        };
		protected override int BaseKindOf(int kind) => tBase[kind];

        // a token's name
        public static readonly string[] varTName = {
            "[EOF]",
            "[number]",
            "[ident]",
            "set",
            "serveroutput",
            "on",
            "size",
            "insert",
            "update",
            "delete",
            "into",
            "values",
            "prompt",
            "null",
            "lantusparam",
            "tusparam",
            "tusnom",
            "tupcode",
            "tupflagorfi",
            "tuplibelle",
            ";",
            "(",
            ")",
            "/",
            ".",
            ",",
            "=",
            "||",
            "[string]",
            "[stars]",
            "declare",
            "begin",
            "commit",
            "exception",
            "when",
            "others",
            "then",
            "rollback",
            "end",
            "where",
            "and",
            "from",
            "TTRPARAM",
            "TTRNOM",
            "TTPCODE",
            "LANTTRPARAM",
            "LANCODE",
            "TTPLIBELLE",
            "TTPHELPTEXT",
            "TTRAITEMENT",
            "TTRFLAGPREF",
            "LANTTRAITEMENT",
            "TTRLIBELLE",
            "TTRCONTEXT",
            "TUSER",
            "TUSLONGUEUR",
            "LANTUSER",
            "TUSLIBELLE",
            "TUPHELPTEXT",
            "dbms_output",
            "PUT_LINE",
            "chr",
            "SQLERRM",
            "[???]"
        };
        public override string NameOfTokenKind(int tokenKind) => varTName[tokenKind];

		// states that a particular production (1st index) can start with a particular token (2nd index). Needed by addAlt().
		static readonly bool[,] set0 = {
            {_T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_x,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x}
		};

        // as set0 but with token inheritance taken into account
        static readonly bool[,] set = {
            {_T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_x,_x,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_x,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x}
        };

        protected override bool StartOf(int s, int kind) => set[s, kind];



        public override string Syntaxerror(int n) 
        {
            switch (n) 
            {
                case 1: return "[EOF] expected";
                case 2: return "[number] expected";
                case 3: return "[ident] expected";
                case 4: return "set expected";
                case 5: return "serveroutput expected";
                case 6: return "on expected";
                case 7: return "size expected";
                case 8: return "insert expected";
                case 9: return "update expected";
                case 10: return "delete expected";
                case 11: return "into expected";
                case 12: return "values expected";
                case 13: return "prompt expected";
                case 14: return "null expected";
                case 15: return "lantusparam expected";
                case 16: return "tusparam expected";
                case 17: return "tusnom expected";
                case 18: return "tupcode expected";
                case 19: return "tupflagorfi expected";
                case 20: return "tuplibelle expected";
                case 21: return "; expected";
                case 22: return "( expected";
                case 23: return ") expected";
                case 24: return "/ expected";
                case 25: return ". expected";
                case 26: return ", expected";
                case 27: return "= expected";
                case 28: return "|| expected";
                case 29: return "[string] expected";
                case 30: return "[stars] expected";
                case 31: return "declare expected";
                case 32: return "begin expected";
                case 33: return "commit expected";
                case 34: return "exception expected";
                case 35: return "when expected";
                case 36: return "others expected";
                case 37: return "then expected";
                case 38: return "rollback expected";
                case 39: return "end expected";
                case 40: return "where expected";
                case 41: return "and expected";
                case 42: return "from expected";
                case 43: return "TTRPARAM expected";
                case 44: return "TTRNOM expected";
                case 45: return "TTPCODE expected";
                case 46: return "LANTTRPARAM expected";
                case 47: return "LANCODE expected";
                case 48: return "TTPLIBELLE expected";
                case 49: return "TTPHELPTEXT expected";
                case 50: return "TTRAITEMENT expected";
                case 51: return "TTRFLAGPREF expected";
                case 52: return "LANTTRAITEMENT expected";
                case 53: return "TTRLIBELLE expected";
                case 54: return "TTRCONTEXT expected";
                case 55: return "TUSER expected";
                case 56: return "TUSLONGUEUR expected";
                case 57: return "LANTUSER expected";
                case 58: return "TUSLIBELLE expected";
                case 59: return "TUPHELPTEXT expected";
                case 60: return "dbms_output expected";
                case 61: return "PUT_LINE expected";
                case 62: return "chr expected";
                case 63: return "SQLERRM expected";
                case 64: return "[???] expected";
                case 65: return "symbol not expected in Slash (SYNC error)";
                case 66: return "invalid Block, expected set begin prompt declare";
                case 67: return "invalid ExceptionHandledBlock, expected commit exception dbms_output";
                case 68: return "invalid ExceptionHandledBlock, expected dbms_output exception";
                case 69: return "invalid ExceptionHandledBlock, expected rollback null";
                case 70: return "invalid InsertDeleteUpdate, expected insert delete update";
                case 71: return "invalid Insert, expected TUSER LANTUSER tusparam lantusparam TTRPARAM LANTTRPARAM TTRAITEMENT LANTTRAITEMENT";
                case 72: return "invalid Update, expected [number] [string] [ident]";
                case 73: return "invalid TUSER, expected ( values";
                case 74: return "invalid TUSER, expected [number] null [string]";
                case 75: return "invalid LANTUSER, expected LANCODE TUSLIBELLE";
                case 76: return "invalid LANTUSER, expected ( values";
                case 77: return "invalid TUSPARAM, expected ( values";
                case 78: return "invalid TUSPARAM, expected [number] null [string]";
                case 79: return "invalid LANTUSPARAM, expected LANCODE tuplibelle";
                case 80: return "invalid LANTUSPARAM, expected [string] chr SQLERRM null";
                case 81: return "invalid LANTUSPARAM, expected ( values";
                case 82: return "invalid LANTTRPARAM, expected LANCODE TTPLIBELLE";
                case 83: return "invalid LANTTRPARAM, expected [string] chr SQLERRM null";
                case 84: return "invalid LANTTRPARAM, expected , )";
                case 85: return "invalid LANTTRAITEMENT, expected LANCODE TTRLIBELLE";
                case 86: return "invalid LANTUSPARAM_LANCODE, expected [string] chr SQLERRM null";
                case 87: return "invalid StringFactor, expected [string] chr SQLERRM";
                default: return $"error {n}";
            }
        }

    } // end Parser

// end namespace implicit
}
