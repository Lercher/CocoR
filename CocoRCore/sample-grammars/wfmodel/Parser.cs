using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using CocoRCore;

namespace CocoRCore.Samples.WFModel
{

    public class Parser : ParserBase 
    {
        public const int _ident = 1; // TOKEN ident
        public const int _dottedident = 2; // TOKEN dottedident
        public const int _number = 3; // TOKEN number
        public const int _int = 4; // TOKEN int
        public const int _string = 5; // TOKEN string
        public const int _braced = 6; // TOKEN braced
        public const int _bracketed = 7; // TOKEN bracketed
        public const int _end = 8; // TOKEN end
        public const int _dot = 9; // TOKEN dot
        public const int _bar = 10; // TOKEN bar
        public const int _colon = 11; // TOKEN colon
        public const int _versionnumber = 12; // TOKEN versionnumber
        public const int _version = 13; // TOKEN version INHERITS ident
        public const int _search = 14; // TOKEN search INHERITS ident
        public const int _select = 15; // TOKEN select INHERITS ident
        public const int _details = 16; // TOKEN details INHERITS ident
        public const int _edit = 17; // TOKEN edit INHERITS ident
        public const int _clear = 18; // TOKEN clear INHERITS ident
        public const int _keys = 19; // TOKEN keys INHERITS ident
        public const int _displayname = 20; // TOKEN displayname INHERITS ident
        public const int _vbident = 21; // TOKEN vbident INHERITS ident
        private const int __maxT = 72;
        private const bool _T = true;
        private const bool _x = false;
        
        public readonly Symboltable types;
        public readonly Symboltable enumtypes;
        public Symboltable symbols(string name)
        {
            if (name == "types") return types;
            if (name == "enumtypes") return enumtypes;
            return null;
        }


public override void Prime(ref Token t) { 
		if (t.kind == _string || t.kind == _braced || t.kind == _bracketed)
		{
			var tb = t.Copy(); 
			tb.setValue(t.valScanned.Substring(1, t.val.Length - 2), scanner.casingString);
			t = tb.Freeze();
		}
	}


        public Parser()
        {
        types = new Symboltable("types", true, false, this);
        enumtypes = new Symboltable("enumtypes", true, false, this);
        astbuilder = new AST.Builder(this);
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


        void WFModel‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                using(astbuilder.createMarker("version", null, false, false, false))
                Version‿NT();
                using(astbuilder.createMarker("namespace", null, false, false, false))
                Namespace‿NT();
                addAlt(23); // OPT
                if (isKind(la, 23 /*readerwriterprefix*/))
                {
                    using(astbuilder.createMarker("readerwriterprefix", null, false, false, false))
                    ReaderWriterPrefix‿NT();
                }
                using(astbuilder.createMarker("rootclass", null, false, false, false))
                RootClass‿NT();
                addAlt(set0, 1); // ITER start
                while (StartOf(1))
                {
                    addAlt(26); // ALT
                    addAlt(62); // ALT
                    addAlt(70); // ALT
                    addAlt(69); // ALT
                    if (isKind(la, 26 /*class*/))
                    {
                        using(astbuilder.createMarker("class", null, true, false, false))
                        Class‿NT();
                    }
                    else if (isKind(la, 62 /*subsystem*/))
                    {
                        using(astbuilder.createMarker("subsystem", null, true, false, false))
                        SubSystem‿NT();
                    }
                    else if (isKind(la, 70 /*enum*/))
                    {
                        using(astbuilder.createMarker("enum", null, true, false, false))
                        Enum‿NT();
                    }
                    else
                    {
                        using(astbuilder.createMarker("flags", null, true, false, false))
                        Flags‿NT();
                    }
                    addAlt(set0, 1); // ITER end
                }
                EndNamespace‿NT();
            }
        }


        void Version‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(13); // T version
                Expect(13 /*version*/);
                addAlt(12); // T versionnumber
                using(astbuilder.createMarker(null, null, false, true, false))
                Expect(12 /*[versionnumber]*/);
            }
        }


        void Namespace‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                while (!(isKind(la, 0 /*[EOF]*/) || isKind(la, 22 /*namespace*/)))
                {
                    SynErr(74);
                    Get();
                }
                addAlt(22); // T "namespace"
                Expect(22 /*namespace*/);
                DottedIdent‿NT();
            }
        }


        void ReaderWriterPrefix‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                while (!(isKind(la, 0 /*[EOF]*/) || isKind(la, 23 /*readerwriterprefix*/)))
                {
                    SynErr(75);
                    Get();
                }
                addAlt(23); // T "readerwriterprefix"
                Expect(23 /*readerwriterprefix*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker(null, null, false, true, false))
                Expect(1 /*[ident]*/);
            }
        }


        void RootClass‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                while (!(isKind(la, 0 /*[EOF]*/) || isKind(la, 24 /*rootclass*/)))
                {
                    SynErr(76);
                    Get();
                }
                addAlt(24); // T "rootclass"
                using(astbuilder.createMarker("typ", null, false, true, false))
                Expect(24 /*rootclass*/);
                addAlt(25); // T "data"
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(25 /*data*/);
                using(astbuilder.createMarker("properties", null, true, false, false))
                Properties‿NT();
                addAlt(8); // T end
                Expect(8 /*end*/);
                addAlt(26); // T "class"
                Expect(26 /*class*/);
            }
        }


        void Class‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                while (!(isKind(la, 0 /*[EOF]*/) || isKind(la, 26 /*class*/)))
                {
                    SynErr(77);
                    Get();
                }
                addAlt(26); // T "class"
                using(astbuilder.createMarker("typ", null, false, true, false))
                Expect(26 /*class*/);
                if (!types.Add(la)) SemErr(71, string.Format(DuplicateSymbol, "ident", la.val, types.name), la);
                alternatives.stdeclares = types;
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
                addAlt(6); // OPT
                if (isKind(la, 6 /*[braced]*/))
                {
                    using(astbuilder.createMarker("title", null, false, false, false))
                    Title‿NT();
                }
                addAlt(28); // OPT
                if (isKind(la, 28 /*inherits*/))
                {
                    using(astbuilder.createMarker("inherits", null, false, false, false))
                    Inherits‿NT();
                }
                addAlt(27); // OPT
                if (isKind(la, 27 /*via*/))
                {
                    using(astbuilder.createMarker("via", null, false, false, false))
                    Via‿NT();
                }
                using(astbuilder.createMarker("properties", null, true, false, false))
                Properties‿NT();
                addAlt(8); // T end
                Expect(8 /*end*/);
                addAlt(26); // T "class"
                Expect(26 /*class*/);
            }
        }


        void SubSystem‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                while (!(isKind(la, 0 /*[EOF]*/) || isKind(la, 62 /*subsystem*/)))
                {
                    SynErr(78);
                    Get();
                }
                addAlt(62); // T "subsystem"
                Expect(62 /*subsystem*/);
                if (!types.Add(la)) SemErr(71, string.Format(DuplicateSymbol, "ident", la.val, types.name), la);
                alternatives.stdeclares = types;
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
                addAlt(63); // T "ssname"
                Expect(63 /*ssname*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("ssname", null, false, true, false))
                Expect(1 /*[ident]*/);
                addAlt(64); // T "ssconfig"
                Expect(64 /*ssconfig*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("ssconfig", null, false, true, false))
                Expect(1 /*[ident]*/);
                addAlt(65); // T "sstyp"
                Expect(65 /*sstyp*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("sstyp", null, false, true, false))
                Expect(1 /*[ident]*/);
                addAlt(66); // T "sscommands"
                Expect(66 /*sscommands*/);
                using(astbuilder.createMarker("sscommands", null, true, false, false))
                SSCommands‿NT();
                addAlt(67); // OPT
                if (isKind(la, 67 /*sskey*/))
                {
                    Get();
                    addAlt(5); // T string
                    using(astbuilder.createMarker("sskey", null, false, true, true))
                    Expect(5 /*[string]*/);
                }
                addAlt(68); // OPT
                if (isKind(la, 68 /*ssclear*/))
                {
                    Get();
                    addAlt(5); // T string
                    using(astbuilder.createMarker("ssclear", null, false, true, true))
                    Expect(5 /*[string]*/);
                }
                addAlt(30); // ITER start
                while (isKind(la, 30 /*infoproperty*/))
                {
                    using(astbuilder.createMarker("properties", null, true, false, false))
                    InfoProperty‿NT();
                    addAlt(30); // ITER end
                }
                addAlt(8); // T end
                Expect(8 /*end*/);
                addAlt(62); // T "subsystem"
                Expect(62 /*subsystem*/);
            }
        }


        void Enum‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                while (!(isKind(la, 0 /*[EOF]*/) || isKind(la, 70 /*enum*/)))
                {
                    SynErr(79);
                    Get();
                }
                addAlt(70); // T "enum"
                Expect(70 /*enum*/);
                if (!enumtypes.Add(la)) SemErr(71, string.Format(DuplicateSymbol, "ident", la.val, enumtypes.name), la);
                alternatives.stdeclares = enumtypes;
                addAlt(1); // T ident
                using(astbuilder.createMarker(null, null, false, true, false))
                Expect(1 /*[ident]*/);
                EnumValues‿NT();
                addAlt(8); // T end
                Expect(8 /*end*/);
                addAlt(70); // T "enum"
                Expect(70 /*enum*/);
            }
        }


        void Flags‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                while (!(isKind(la, 0 /*[EOF]*/) || isKind(la, 69 /*flags*/)))
                {
                    SynErr(80);
                    Get();
                }
                addAlt(69); // T "flags"
                Expect(69 /*flags*/);
                if (!types.Add(la)) SemErr(71, string.Format(DuplicateSymbol, "ident", la.val, types.name), la);
                alternatives.stdeclares = types;
                addAlt(1); // T ident
                using(astbuilder.createMarker(null, null, false, true, false))
                Expect(1 /*[ident]*/);
                addAlt(1); // ITER start
                while (isKind(la, 1 /*[ident]*/))
                {
                    EnumValue‿NT();
                    addAlt(1); // ITER end
                }
                addAlt(8); // T end
                Expect(8 /*end*/);
                addAlt(69); // T "flags"
                Expect(69 /*flags*/);
            }
        }


        void EndNamespace‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(8); // T end
                Expect(8 /*end*/);
                addAlt(22); // T "namespace"
                Expect(22 /*namespace*/);
            }
        }


        void DottedIdent‿NT()
        {
            using(astbuilder.createBarrier("."))
            {
                addAlt(2); // OPT
                if (isKind(la, 2 /*[dottedident]*/))
                {
                    using(astbuilder.createMarker(null, null, false, true, false))
                    Get();
                    addAlt(9); // T dot
                    Expect(9 /*.*/);
                    addAlt(2); // ITER start
                    while (isKind(la, 2 /*[dottedident]*/))
                    {
                        using(astbuilder.createMarker(null, null, false, true, false))
                        Get();
                        addAlt(9); // T dot
                        Expect(9 /*.*/);
                        addAlt(2); // ITER end
                    }
                }
                addAlt(1); // T ident
                using(astbuilder.createMarker(null, null, false, true, false))
                Expect(1 /*[ident]*/);
            }
        }


        void DottedIdentBare‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(2); // OPT
                if (isKind(la, 2 /*[dottedident]*/))
                {
                    Get();
                    addAlt(9); // T dot
                    Expect(9 /*.*/);
                    addAlt(2); // ITER start
                    while (isKind(la, 2 /*[dottedident]*/))
                    {
                        Get();
                        addAlt(9); // T dot
                        Expect(9 /*.*/);
                        addAlt(2); // ITER end
                    }
                }
                addAlt(1); // T ident
                Expect(1 /*[ident]*/);
            }
        }


        void Properties‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(set0, 2); // ITER start
                while (StartOf(2))
                {
                    Prop‿NT();
                    addAlt(set0, 2); // ITER end
                }
            }
        }


        void Title‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(6); // T braced
                using(astbuilder.createMarker(null, null, false, true, false))
                Expect(6 /*[braced]*/);
            }
        }


        void Inherits‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(28); // T "inherits"
                Expect(28 /*inherits*/);
                DottedIdent‿NT();
            }
        }


        void Via‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(27); // T "via"
                Expect(27 /*via*/);
                DottedIdent‿NT();
            }
        }


        void Prop‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                while (!(StartOf(3)))
                {
                    SynErr(81);
                    Get();
                }
                addAlt(29); // ALT
                addAlt(30); // ALT
                addAlt(31); // ALT
                addAlt(32); // ALT
                addAlt(33); // ALT
                addAlt(34); // ALT
                addAlt(35); // ALT
                addAlt(36); // ALT
                switch (la.kind)
                {
                    case 29: /*property*/
                        { // scoping
                            Property‿NT();
                        }
                        break;
                    case 30: /*infoproperty*/
                        { // scoping
                            InfoProperty‿NT();
                        }
                        break;
                    case 31: /*approperty*/
                        { // scoping
                            APProperty‿NT();
                        }
                        break;
                    case 32: /*list*/
                        { // scoping
                            List‿NT();
                        }
                        break;
                    case 33: /*selectlist*/
                        { // scoping
                            SelectList‿NT();
                        }
                        break;
                    case 34: /*flagslist*/
                        { // scoping
                            FlagsList‿NT();
                        }
                        break;
                    case 35: /*longproperty*/
                        { // scoping
                            LongProperty‿NT();
                        }
                        break;
                    case 36: /*infolongproperty*/
                        { // scoping
                            InfoLongProperty‿NT();
                        }
                        break;
                    default:
                        SynErr(82);
                        break;
                } // end switch
            }
        }


        void Property‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(29); // T "property"
                using(astbuilder.createMarker("writeable", "t", false, true, false))
                Expect(29 /*property*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
                using(astbuilder.createMarker("type", null, false, false, false))
                Type‿NT();
            }
        }


        void InfoProperty‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(30); // T "infoproperty"
                using(astbuilder.createMarker("writeable", "f", false, true, false))
                Expect(30 /*infoproperty*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
                using(astbuilder.createMarker("type", null, false, false, false))
                Type‿NT();
            }
        }


        void APProperty‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(31); // T "approperty"
                using(astbuilder.createMarker("writeable", "t", false, true, false))
                using(astbuilder.createMarker("autopostback", "t", false, true, false))
                Expect(31 /*approperty*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
                using(astbuilder.createMarker("type", null, false, false, false))
                Type‿NT();
            }
        }


        void List‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(32); // T "list"
                using(astbuilder.createMarker("list", "t", false, true, false))
                Expect(32 /*list*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
                addAlt(41); // OPT
                if (isKind(la, 41 /*as*/))
                {
                    using(astbuilder.createMarker("type", null, false, false, false))
                    As‿NT();
                }
            }
        }


        void SelectList‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(33); // T "selectlist"
                using(astbuilder.createMarker("list", "t", false, true, false))
                using(astbuilder.createMarker("select", "t", false, true, false))
                Expect(33 /*selectlist*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
                using(astbuilder.createMarker("type", null, false, false, false))
                As‿NT();
            }
        }


        void FlagsList‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(34); // T "flagslist"
                using(astbuilder.createMarker("list", "t", false, true, false))
                using(astbuilder.createMarker("flags", "t", false, true, false))
                Expect(34 /*flagslist*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
                using(astbuilder.createMarker("type", null, false, false, false))
                Mimics‿NT();
            }
        }


        void LongProperty‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(35); // T "longproperty"
                using(astbuilder.createMarker("writeable", "t", false, true, false))
                using(astbuilder.createMarker("long", "t", false, true, false))
                Expect(35 /*longproperty*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
            }
        }


        void InfoLongProperty‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(36); // T "infolongproperty"
                using(astbuilder.createMarker("writeable", "f", false, true, false))
                using(astbuilder.createMarker("long", "t", false, true, false))
                Expect(36 /*infolongproperty*/);
                addAlt(1); // T ident
                using(astbuilder.createMarker("name", null, false, true, false))
                Expect(1 /*[ident]*/);
            }
        }


        void Type‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(set0, 4); // ALT
                addAlt(41); // ALT
                addAlt(57); // ALT
                if (StartOf(4))
                {
                    using(astbuilder.createMarker("basic", "String", false, true, false))
                    EmptyType‿NT();
                }
                else if (isKind(la, 41 /*as*/))
                {
                    As‿NT();
                }
                else if (isKind(la, 57 /*mimics*/))
                {
                    Mimics‿NT();
                } // end if
                else
                    SynErr(83);
                addAlt(37); // OPT
                if (isKind(la, 37 /*=*/))
                {
                    Get();
                    using(astbuilder.createMarker("initvalue", null, false, false, false))
                    InitValue‿NT();
                }
                addAlt(6); // OPT
                if (isKind(la, 6 /*[braced]*/))
                {
                    using(astbuilder.createMarker("samplevalue", null, false, false, false))
                    SampleValue‿NT();
                }
            }
        }


        void As‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(41); // T "as"
                Expect(41 /*as*/);
                addAlt(set0, 5); // ALT
                addAlt(1); // ALT
                addAlt(1, types); // ALT ident uses symbol table 'types'
                addAlt(new int[] {1, 2}); // ALT
                if (StartOf(5))
                {
                    BaseType‿NT();
                }
                else if (isKind(la, 1 /*[ident]*/))
                {
                    if (!types.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "ident", la.val, types.name), la);
                    using(astbuilder.createMarker("basic", null, false, true, false))
                    Get();
                }
                else if (isKind(la, 1 /*[ident]*/) || isKind(la, 2 /*[dottedident]*/))
                {
                    using(astbuilder.createMarker("basic", null, false, false, false))
                    DottedIdent‿NT();
                } // end if
                else
                    SynErr(84);
            }
        }


        void Mimics‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(57); // T "mimics"
                using(astbuilder.createMarker("basic", "String", false, true, false))
                Expect(57 /*mimics*/);
                addAlt(set0, 6); // ALT
                addAlt(1); // ALT
                addAlt(1, enumtypes); // ALT ident uses symbol table 'enumtypes'
                if (StartOf(6))
                {
                    using(astbuilder.createMarker("mimicsspec", null, false, false, false))
                    MimicsSpec‿NT();
                }
                else if (isKind(la, 1 /*[ident]*/))
                {
                    if (!enumtypes.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, "ident", la.val, enumtypes.name), la);
                    using(astbuilder.createMarker("mimicsspec", null, false, true, false))
                    using(astbuilder.createMarker("enum", "t", false, true, false))
                    Get();
                } // end if
                else
                    SynErr(85);
            }
        }


        void EmptyType‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
            }
        }


        void InitValue‿NT()
        {
            using(astbuilder.createBarrier(""))
            {
                addAlt(3); // ALT
                addAlt(4); // ALT
                addAlt(5); // ALT
                addAlt(38); // ALT
                addAlt(39); // ALT
                addAlt(40); // ALT
                addAlt(new int[] {1, 2}); // ALT
                switch (la.kind)
                {
                    case 3: /*[number]*/
                        { // scoping
                            Get();
                        }
                        break;
                    case 4: /*[int]*/
                        { // scoping
                            Get();
                        }
                        break;
                    case 5: /*[string]*/
                        { // scoping
                            Get();
                        }
                        break;
                    case 38: /*true*/
                        { // scoping
                            Get();
                        }
                        break;
                    case 39: /*false*/
                        { // scoping
                            Get();
                        }
                        break;
                    case 40: /*#*/
                        { // scoping
                            Get();
                            addAlt(set0, 7); // ITER start
                            while (StartOf(7))
                            {
                                Get();
                                addAlt(set0, 7); // ITER end
                            }
                            addAlt(40); // T "#"
                            Expect(40 /*#*/);
                        }
                        break;
                    case 1: /*[ident]*/
                    case 2: /*[dottedident]*/
                    case 13: /*version*/
                    case 14: /*search*/
                    case 15: /*select*/
                    case 16: /*details*/
                    case 17: /*edit*/
                    case 18: /*clear*/
                    case 19: /*keys*/
                    case 20: /*displayname*/
                    case 21: /*[vbident]*/
                        { // scoping
                            FunctionCall‿NT();
                        }
                        break;
                    default:
                        SynErr(86);
                        break;
                } // end switch
            }
        }


        void SampleValue‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(6); // T braced
                using(astbuilder.createMarker(null, null, false, true, true))
                Expect(6 /*[braced]*/);
            }
        }


        void FunctionCall‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                DottedIdentBare‿NT();
                addAlt(7); // T bracketed
                Expect(7 /*[bracketed]*/);
            }
        }


        void BaseType‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(42); // ALT
                addAlt(43); // ALT
                addAlt(44); // ALT
                addAlt(45); // ALT
                addAlt(46); // ALT
                addAlt(47); // ALT
                addAlt(48); // ALT
                addAlt(49); // ALT
                addAlt(50); // ALT
                addAlt(51); // ALT
                addAlt(52); // ALT
                addAlt(53); // ALT
                addAlt(54); // ALT
                addAlt(55); // ALT
                addAlt(56); // ALT
                switch (la.kind)
                {
                    case 42: /*double*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            using(astbuilder.createMarker("format", "c", false, true, false))
                            Get();
                        }
                        break;
                    case 43: /*date*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            using(astbuilder.createMarker("format", "d", false, true, false))
                            Get();
                        }
                        break;
                    case 44: /*datetime*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            using(astbuilder.createMarker("format", "{0:d} {0:t}", false, true, false))
                            Get();
                        }
                        break;
                    case 45: /*integer*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            using(astbuilder.createMarker("format", "n0", false, true, false))
                            Get();
                        }
                        break;
                    case 46: /*percent*/
                        { // scoping
                            using(astbuilder.createMarker("basic", "double", false, true, false))
                            using(astbuilder.createMarker("format", "p", false, true, false))
                            Get();
                        }
                        break;
                    case 47: /*percentwithdefault*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            using(astbuilder.createMarker("format", "p", false, true, false))
                            Get();
                        }
                        break;
                    case 48: /*doublewithdefault*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            using(astbuilder.createMarker("format", "c", false, true, false))
                            Get();
                        }
                        break;
                    case 49: /*integerwithdefault*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            using(astbuilder.createMarker("format", "n0", false, true, false))
                            Get();
                        }
                        break;
                    case 50: /*n2*/
                        { // scoping
                            using(astbuilder.createMarker("basic", "double", false, true, false))
                            using(astbuilder.createMarker("format", "n2", false, true, false))
                            Get();
                        }
                        break;
                    case 51: /*n0*/
                        { // scoping
                            using(astbuilder.createMarker("basic", "integer", false, true, false))
                            using(astbuilder.createMarker("format", "n0", false, true, false))
                            Get();
                        }
                        break;
                    case 52: /*string*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            Get();
                        }
                        break;
                    case 53: /*boolean*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            Get();
                        }
                        break;
                    case 54: /*guid*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            Get();
                        }
                        break;
                    case 55: /*string()*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            Get();
                        }
                        break;
                    case 56: /*xml*/
                        { // scoping
                            using(astbuilder.createMarker("basic", null, false, true, false))
                            Get();
                        }
                        break;
                    default:
                        SynErr(87);
                        break;
                } // end switch
            }
        }


        void MimicsSpec‿NT()
        {
            using(astbuilder.createBarrier(""))
            {
                addAlt(58); // ALT
                addAlt(59); // ALT
                addAlt(60); // ALT
                addAlt(61); // ALT
                if (isKind(la, 58 /*query*/))
                {
                    Query‿NT();
                }
                else if (isKind(la, 59 /*txt*/))
                {
                    Txt‿NT();
                }
                else if (isKind(la, 60 /*xl*/))
                {
                    XL‿NT();
                }
                else if (isKind(la, 61 /*ref*/))
                {
                    Ref‿NT();
                } // end if
                else
                    SynErr(88);
            }
        }


        void Query‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(58); // T "query"
                Expect(58 /*query*/);
                addAlt(11); // T colon
                Expect(11 /*:*/);
                addAlt(2); // T dottedident
                Expect(2 /*[dottedident]*/);
                addAlt(9); // T dot
                Expect(9 /*.*/);
                addAlt(1); // T ident
                Expect(1 /*[ident]*/);
                addAlt(11); // T colon
                Expect(11 /*:*/);
                StringOrIdent‿NT();
            }
        }


        void Txt‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(59); // T "txt"
                Expect(59 /*txt*/);
                addAlt(11); // T colon
                Expect(11 /*:*/);
                addAlt(2); // T dottedident
                Expect(2 /*[dottedident]*/);
                addAlt(9); // T dot
                Expect(9 /*.*/);
                addAlt(1); // T ident
                Expect(1 /*[ident]*/);
                addAlt(11); // T colon
                Expect(11 /*:*/);
                StringOrIdent‿NT();
            }
        }


        void XL‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(60); // T "xl"
                Expect(60 /*xl*/);
                addAlt(11); // T colon
                Expect(11 /*:*/);
                addAlt(2); // T dottedident
                Expect(2 /*[dottedident]*/);
                addAlt(9); // T dot
                Expect(9 /*.*/);
                addAlt(1); // T ident
                Expect(1 /*[ident]*/);
                addAlt(11); // T colon
                Expect(11 /*:*/);
                StringOrIdent‿NT();
            }
        }


        void Ref‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(61); // T "ref"
                Expect(61 /*ref*/);
                addAlt(11); // T colon
                Expect(11 /*:*/);
                addAlt(19); // ALT
                addAlt(20); // ALT
                if (isKind(la, 19 /*keys*/))
                {
                    Get();
                }
                else if (isKind(la, 20 /*displayname*/))
                {
                    Get();
                } // end if
                else
                    SynErr(89);
                addAlt(11); // T colon
                Expect(11 /*:*/);
                StringOrIdent‿NT();
            }
        }


        void StringOrIdent‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(5); // ALT
                addAlt(new int[] {1, 2}); // ALT
                if (isKind(la, 5 /*[string]*/))
                {
                    Get();
                }
                else if (isKind(la, 1 /*[ident]*/) || isKind(la, 2 /*[dottedident]*/))
                {
                    DottedIdentBare‿NT();
                } // end if
                else
                    SynErr(90);
            }
        }


        void SSCommands‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                using(astbuilder.createMarker(null, null, true, true, false))
                SSCommand‿NT();
                addAlt(10); // ITER start
                while (isKind(la, 10 /*|*/))
                {
                    Get();
                    using(astbuilder.createMarker(null, null, true, true, false))
                    SSCommand‿NT();
                    addAlt(10); // ITER end
                }
            }
        }


        void SSCommand‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(14); // ALT
                addAlt(15); // ALT
                addAlt(16); // ALT
                addAlt(17); // ALT
                addAlt(18); // ALT
                if (isKind(la, 14 /*search*/))
                {
                    Get();
                }
                else if (isKind(la, 15 /*select*/))
                {
                    Get();
                }
                else if (isKind(la, 16 /*details*/))
                {
                    Get();
                }
                else if (isKind(la, 17 /*edit*/))
                {
                    Get();
                }
                else if (isKind(la, 18 /*clear*/))
                {
                    Get();
                } // end if
                else
                    SynErr(91);
            }
        }


        void EnumValue‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(1); // T ident
                Expect(1 /*[ident]*/);
                addAlt(37); // OPT
                if (isKind(la, 37 /*=*/))
                {
                    EnumIntValue‿NT();
                }
            }
        }


        void EnumValues‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(1); // ITER start
                while (isKind(la, 1 /*[ident]*/))
                {
                    EnumValue‿NT();
                    addAlt(1); // ITER end
                }
                addAlt(71); // T "default"
                Expect(71 /*default*/);
                EnumValue‿NT();
                addAlt(1); // ITER start
                while (isKind(la, 1 /*[ident]*/))
                {
                    EnumValue‿NT();
                    addAlt(1); // ITER end
                }
            }
        }


        void EnumIntValue‿NT()
        {
            using(astbuilder.createBarrier(null))
            {
                addAlt(37); // T "="
                Expect(37 /*=*/);
                addAlt(4); // T int
                Expect(4 /*[int]*/);
            }
        }



        public override void Parse() 
        {
            if (scanner == null) throw new FatalError("This parser is not Initialize()-ed.");
            lb = Token.Zero;
            la = Token.Zero;
            Get();
            WFModel‿NT();
            Expect(0);
            types.CheckDeclared();
            enumtypes.CheckDeclared();
        }
    
        // a token's base type
        public static readonly int[] tBase = {
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1, 1, 1, 1,   1, 1, 1, 1,
             1, 1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,
            -1,-1,-1,-1,  -1,-1,-1,-1,  -1,-1,-1,-1,  -1
        };
		protected override int BaseKindOf(int kind) => tBase[kind];

        // a token's name
        public static readonly string[] varTName = {
            "[EOF]",
            "[ident]",
            "[dottedident]",
            "[number]",
            "[int]",
            "[string]",
            "[braced]",
            "[bracketed]",
            "end",
            ".",
            "|",
            ":",
            "[versionnumber]",
            "version",
            "search",
            "select",
            "details",
            "edit",
            "clear",
            "keys",
            "displayname",
            "[vbident]",
            "namespace",
            "readerwriterprefix",
            "rootclass",
            "data",
            "class",
            "via",
            "inherits",
            "property",
            "infoproperty",
            "approperty",
            "list",
            "selectlist",
            "flagslist",
            "longproperty",
            "infolongproperty",
            "=",
            "true",
            "false",
            "#",
            "as",
            "double",
            "date",
            "datetime",
            "integer",
            "percent",
            "percentwithdefault",
            "doublewithdefault",
            "integerwithdefault",
            "n2",
            "n0",
            "string",
            "boolean",
            "guid",
            "string()",
            "xml",
            "mimics",
            "query",
            "txt",
            "xl",
            "ref",
            "subsystem",
            "ssname",
            "ssconfig",
            "sstyp",
            "sscommands",
            "sskey",
            "ssclear",
            "flags",
            "enum",
            "default",
            "[???]"
        };
        public override string NameOfTokenKind(int tokenKind) => varTName[tokenKind];

		// states that a particular production (1st index) can start with a particular token (2nd index). Needed by addAlt().
		static readonly bool[,] set0 = {
            {_T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _T,_x,_T,_x,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_T,_T,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_T,_T,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _T,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x}
		};

        // as set0 but with token inheritance taken into account
        static readonly bool[,] set = {
            {_T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _T,_x,_T,_x,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_T,_T,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_x,  _x,_x,_x,_x,  _x,_T,_T,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_T,_x,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x,_T,_T,  _T,_T,_x,_x,  _x,_x,_x,_x,  _x,_x,_x,_x,  _x,_x},
            {_x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _x,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_T,_T,_T,  _T,_x}
        };

        protected override bool StartOf(int s, int kind) => set[s, kind];

        public readonly AST.Builder astbuilder; // can also be private  
        public AST ast { get { return astbuilder.current; }}


        public override string Syntaxerror(int n) 
        {
            switch (n) 
            {
                case 1: return "[EOF] expected";
                case 2: return "[ident] expected";
                case 3: return "[dottedident] expected";
                case 4: return "[number] expected";
                case 5: return "[int] expected";
                case 6: return "[string] expected";
                case 7: return "[braced] expected";
                case 8: return "[bracketed] expected";
                case 9: return "end expected";
                case 10: return ". expected";
                case 11: return "| expected";
                case 12: return ": expected";
                case 13: return "[versionnumber] expected";
                case 14: return "version expected";
                case 15: return "search expected";
                case 16: return "select expected";
                case 17: return "details expected";
                case 18: return "edit expected";
                case 19: return "clear expected";
                case 20: return "keys expected";
                case 21: return "displayname expected";
                case 22: return "[vbident] expected";
                case 23: return "namespace expected";
                case 24: return "readerwriterprefix expected";
                case 25: return "rootclass expected";
                case 26: return "data expected";
                case 27: return "class expected";
                case 28: return "via expected";
                case 29: return "inherits expected";
                case 30: return "property expected";
                case 31: return "infoproperty expected";
                case 32: return "approperty expected";
                case 33: return "list expected";
                case 34: return "selectlist expected";
                case 35: return "flagslist expected";
                case 36: return "longproperty expected";
                case 37: return "infolongproperty expected";
                case 38: return "= expected";
                case 39: return "true expected";
                case 40: return "false expected";
                case 41: return "# expected";
                case 42: return "as expected";
                case 43: return "double expected";
                case 44: return "date expected";
                case 45: return "datetime expected";
                case 46: return "integer expected";
                case 47: return "percent expected";
                case 48: return "percentwithdefault expected";
                case 49: return "doublewithdefault expected";
                case 50: return "integerwithdefault expected";
                case 51: return "n2 expected";
                case 52: return "n0 expected";
                case 53: return "string expected";
                case 54: return "boolean expected";
                case 55: return "guid expected";
                case 56: return "string() expected";
                case 57: return "xml expected";
                case 58: return "mimics expected";
                case 59: return "query expected";
                case 60: return "txt expected";
                case 61: return "xl expected";
                case 62: return "ref expected";
                case 63: return "subsystem expected";
                case 64: return "ssname expected";
                case 65: return "ssconfig expected";
                case 66: return "sstyp expected";
                case 67: return "sscommands expected";
                case 68: return "sskey expected";
                case 69: return "ssclear expected";
                case 70: return "flags expected";
                case 71: return "enum expected";
                case 72: return "default expected";
                case 73: return "[???] expected";
                case 74: return "symbol not expected in Namespace (SYNC error)";
                case 75: return "symbol not expected in ReaderWriterPrefix (SYNC error)";
                case 76: return "symbol not expected in RootClass (SYNC error)";
                case 77: return "symbol not expected in Class (SYNC error)";
                case 78: return "symbol not expected in SubSystem (SYNC error)";
                case 79: return "symbol not expected in Enum (SYNC error)";
                case 80: return "symbol not expected in Flags (SYNC error)";
                case 81: return "symbol not expected in Prop (SYNC error)";
                case 82: return "invalid Prop, expected property infoproperty approperty list selectlist flagslist longproperty infolongproperty";
                case 83: return "invalid Type, expected [braced] end property infoproperty approperty list selectlist flagslist longproperty infolongproperty = as mimics";
                case 84: return "invalid As, expected double date datetime integer percent percentwithdefault doublewithdefault integerwithdefault n2 n0 string boolean guid string() xml [ident] [dottedident]";
                case 85: return "invalid Mimics, expected query txt xl ref [ident]";
                case 86: return "invalid InitValue, expected [number] [int] [string] true false # [ident] [dottedident]";
                case 87: return "invalid BaseType, expected double date datetime integer percent percentwithdefault doublewithdefault integerwithdefault n2 n0 string boolean guid string() xml";
                case 88: return "invalid MimicsSpec, expected query txt xl ref";
                case 89: return "invalid Ref, expected keys displayname";
                case 90: return "invalid StringOrIdent, expected [string] [ident] [dottedident]";
                case 91: return "invalid SSCommand, expected search select details edit clear";
                default: return $"error {n}";
            }
        }

    } // end Parser

// end namespace implicit
}
