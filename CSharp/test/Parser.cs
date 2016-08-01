
using System;
using System.Collections;
using System.Collections.Generic;



public class Parser {
	public const int _EOF = 0; // TOKEN EOF
	public const int _ident = 1; // TOKEN ident
	public const int _number = 2; // TOKEN number
	public const int _int = 3; // TOKEN int
	public const int _string = 4; // TOKEN string
	public const int _braced = 5; // TOKEN braced
	public const int _bracketed = 6; // TOKEN bracketed
	public const int _end = 7; // TOKEN end
	public const int _dot = 8; // TOKEN dot
	public const int _bar = 9; // TOKEN bar
	public const int _colon = 10; // TOKEN colon
	public const int _versionnumber = 11; // TOKEN versionnumber
	public const int _version = 12; // TOKEN version INHERITS ident
	public const int _search = 13; // TOKEN search INHERITS ident
	public const int _select = 14; // TOKEN select INHERITS ident
	public const int _details = 15; // TOKEN details INHERITS ident
	public const int _edit = 16; // TOKEN edit INHERITS ident
	public const int _clear = 17; // TOKEN clear INHERITS ident
	public const int _keys = 18; // TOKEN keys INHERITS ident
	public const int _displayname = 19; // TOKEN displayname INHERITS ident
	public const int _vbident = 20; // TOKEN vbident INHERITS ident
	public const int maxT = 68;

	const bool _T = true;
	const bool _x = false;
	public const string DuplicateSymbol = "{0} '{1}' declared twice in '{2}'";
	public const string MissingSymbol ="{0} '{1}' not declared in '{2}'";
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;
	public List<Alternative> tokens = new List<Alternative>();
	
	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

	public Symboltable symbols(string name) {
		return null;
	}



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	

	void Get () {
		for (;;) {
			t = la;

			if (alt != null) {
				tokens.Add(new Alternative(t, alt, altst));
			}
			_newAlt();

			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}


	BitArray alt = null;
	Symboltable[] altst = null;

	void _newAlt() {
		alt = new BitArray(maxT+1);
		altst = new Symboltable[maxT+1];
	}

	void addAlt(int kind) {
		alt[kind] = true;
	}

	// a terminal tokenclass of kind kind is restricted to this symbol table 
	void addAlt(int kind, Symboltable st) {
		// take the root scope, if it is the only scope,
		// make a copy of the scope stack otherwise, but preserve the list references
		altst[kind] = st.CloneScopes();
	}

	void addAlt(int[] range) {
		foreach(int kind in range)
			addAlt(kind);
	}

	void addAlt(bool[,] pred, int line) {
		for(int kind = 0; kind < maxT; kind++)
			if (pred[line, kind])
				addAlt(kind);
	}


	bool isKind(Token t, int n) {
		int k = t.kind;
		while(k >= 0) {
			if (k == n) return true;
			k = tBase[k];
		}
		return false;
	}
	
	void Expect (int n) {
		if (isKind(la, n)) Get(); else { SynErr(n); }
	}
	
	// is the lookahead token la a start of the production s?
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (isKind(la, n)) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (isKind(la, n)) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void WFModel‿NT() {
		Version‿NT();
		Namespace‿NT();
		addAlt(22); // OPT
		if (isKind(la, 22)) {
			ReaderWriterPrefix‿NT();
		}
		RootClass‿NT();
		addAlt(set0, 1); // ITER start
		while (StartOf(1)) {
			addAlt(25); // ALT
			addAlt(58); // ALT
			addAlt(66); // ALT
			addAlt(65); // ALT
			if (isKind(la, 25)) {
				Class‿NT();
			} else if (isKind(la, 58)) {
				SubSystem‿NT();
			} else if (isKind(la, 66)) {
				Enum‿NT();
			} else {
				Flags‿NT();
			}
			addAlt(set0, 1); // ITER end
		}
		EndNamespace‿NT();
	}

	void Version‿NT() {
		addAlt(12); // T
		Expect(12); // version
		addAlt(11); // T
		Expect(11); // versionnumber
	}

	void Namespace‿NT() {
		while (!(isKind(la, 0) || isKind(la, 21))) {SynErr(69); Get();}
		addAlt(21); // T
		Expect(21); // "namespace"
		DottedIdent‿NT();
	}

	void ReaderWriterPrefix‿NT() {
		while (!(isKind(la, 0) || isKind(la, 22))) {SynErr(70); Get();}
		addAlt(22); // T
		Expect(22); // "readerwriterprefix"
		addAlt(1); // T
		Expect(1); // ident
	}

	void RootClass‿NT() {
		while (!(isKind(la, 0) || isKind(la, 23))) {SynErr(71); Get();}
		addAlt(23); // T
		Expect(23); // "rootclass"
		addAlt(24); // T
		Expect(24); // "data"
		Properties‿NT();
		addAlt(7); // T
		Expect(7); // end
		addAlt(25); // T
		Expect(25); // "class"
	}

	void Class‿NT() {
		while (!(isKind(la, 0) || isKind(la, 25))) {SynErr(72); Get();}
		addAlt(25); // T
		Expect(25); // "class"
		ClassType‿NT();
		addAlt(5); // OPT
		if (isKind(la, 5)) {
			Title‿NT();
		}
		addAlt(27); // OPT
		if (isKind(la, 27)) {
			Inherits‿NT();
		}
		addAlt(26); // OPT
		if (isKind(la, 26)) {
			Via‿NT();
		}
		Properties‿NT();
		addAlt(7); // T
		Expect(7); // end
		addAlt(25); // T
		Expect(25); // "class"
	}

	void SubSystem‿NT() {
		while (!(isKind(la, 0) || isKind(la, 58))) {SynErr(73); Get();}
		addAlt(58); // T
		Expect(58); // "subsystem"
		ClassType‿NT();
		addAlt(59); // T
		Expect(59); // "ssname"
		addAlt(1); // T
		Expect(1); // ident
		addAlt(60); // T
		Expect(60); // "ssconfig"
		addAlt(1); // T
		Expect(1); // ident
		addAlt(61); // T
		Expect(61); // "sstyp"
		addAlt(1); // T
		Expect(1); // ident
		addAlt(62); // T
		Expect(62); // "sscommands"
		SSCommands‿NT();
		addAlt(63); // OPT
		if (isKind(la, 63)) {
			Get();
			addAlt(4); // T
			Expect(4); // string
		}
		addAlt(64); // OPT
		if (isKind(la, 64)) {
			Get();
			addAlt(4); // T
			Expect(4); // string
		}
		addAlt(29); // ITER start
		while (isKind(la, 29)) {
			InfoProperty‿NT();
			addAlt(29); // ITER end
		}
		addAlt(7); // T
		Expect(7); // end
		addAlt(58); // T
		Expect(58); // "subsystem"
	}

	void Enum‿NT() {
		while (!(isKind(la, 0) || isKind(la, 66))) {SynErr(74); Get();}
		addAlt(66); // T
		Expect(66); // "enum"
		EnumType‿NT();
		EnumValues‿NT();
		addAlt(7); // T
		Expect(7); // end
		addAlt(66); // T
		Expect(66); // "enum"
	}

	void Flags‿NT() {
		while (!(isKind(la, 0) || isKind(la, 65))) {SynErr(75); Get();}
		addAlt(65); // T
		Expect(65); // "flags"
		FlagsType‿NT();
		addAlt(1); // ITER start
		while (isKind(la, 1)) {
			EnumValue‿NT();
			addAlt(1); // ITER end
		}
		addAlt(7); // T
		Expect(7); // end
		addAlt(65); // T
		Expect(65); // "flags"
	}

	void EndNamespace‿NT() {
		addAlt(7); // T
		Expect(7); // end
		addAlt(21); // T
		Expect(21); // "namespace"
	}

	void DottedIdent‿NT() {
		addAlt(1); // T
		Expect(1); // ident
		addAlt(8); // ITER start
		while (isKind(la, 8)) {
			Get();
			addAlt(1); // T
			Expect(1); // ident
			addAlt(8); // ITER end
		}
	}

	void Properties‿NT() {
		addAlt(set0, 2); // ITER start
		while (StartOf(2)) {
			Prop‿NT();
			addAlt(set0, 2); // ITER end
		}
	}

	void ClassType‿NT() {
		addAlt(1); // T
		Expect(1); // ident
	}

	void Title‿NT() {
		addAlt(5); // T
		Expect(5); // braced
	}

	void Inherits‿NT() {
		addAlt(27); // T
		Expect(27); // "inherits"
		DottedIdent‿NT();
	}

	void Via‿NT() {
		addAlt(26); // T
		Expect(26); // "via"
		DottedIdent‿NT();
	}

	void Prop‿NT() {
		while (!(StartOf(3))) {SynErr(76); Get();}
		addAlt(28); // ALT
		addAlt(29); // ALT
		addAlt(30); // ALT
		addAlt(31); // ALT
		addAlt(32); // ALT
		addAlt(33); // ALT
		addAlt(34); // ALT
		addAlt(35); // ALT
		switch (la.kind) {
		case 28: // "property"
		{
			Property‿NT();
			break;
		}
		case 29: // "infoproperty"
		{
			InfoProperty‿NT();
			break;
		}
		case 30: // "approperty"
		{
			APProperty‿NT();
			break;
		}
		case 31: // "list"
		{
			List‿NT();
			break;
		}
		case 32: // "selectlist"
		{
			SelectList‿NT();
			break;
		}
		case 33: // "flagslist"
		{
			FlagsList‿NT();
			break;
		}
		case 34: // "longproperty"
		{
			LongProperty‿NT();
			break;
		}
		case 35: // "infolongproperty"
		{
			InfoLongProperty‿NT();
			break;
		}
		default: SynErr(77); break;
		}
	}

	void Property‿NT() {
		addAlt(28); // T
		Expect(28); // "property"
		addAlt(1); // T
		Expect(1); // ident
		Type‿NT();
	}

	void InfoProperty‿NT() {
		addAlt(29); // T
		Expect(29); // "infoproperty"
		addAlt(1); // T
		Expect(1); // ident
		Type‿NT();
	}

	void APProperty‿NT() {
		addAlt(30); // T
		Expect(30); // "approperty"
		addAlt(1); // T
		Expect(1); // ident
		Type‿NT();
	}

	void List‿NT() {
		addAlt(31); // T
		Expect(31); // "list"
		addAlt(1); // T
		Expect(1); // ident
		addAlt(40); // OPT
		if (isKind(la, 40)) {
			As‿NT();
		}
	}

	void SelectList‿NT() {
		addAlt(32); // T
		Expect(32); // "selectlist"
		addAlt(1); // T
		Expect(1); // ident
		As‿NT();
	}

	void FlagsList‿NT() {
		addAlt(33); // T
		Expect(33); // "flagslist"
		addAlt(1); // T
		Expect(1); // ident
		Mimics‿NT();
	}

	void LongProperty‿NT() {
		addAlt(34); // T
		Expect(34); // "longproperty"
		addAlt(1); // T
		Expect(1); // ident
	}

	void InfoLongProperty‿NT() {
		addAlt(35); // T
		Expect(35); // "infolongproperty"
		addAlt(1); // T
		Expect(1); // ident
	}

	void Type‿NT() {
		addAlt(set0, 4); // ALT
		addAlt(40); // ALT
		addAlt(53); // ALT
		if (StartOf(4)) {
			EmptyType‿NT();
		} else if (isKind(la, 40)) {
			As‿NT();
		} else if (isKind(la, 53)) {
			Mimics‿NT();
		} else SynErr(78);
		addAlt(36); // OPT
		if (isKind(la, 36)) {
			Get();
			InitValue‿NT();
		}
		addAlt(5); // OPT
		if (isKind(la, 5)) {
			SampleValue‿NT();
		}
	}

	void As‿NT() {
		addAlt(40); // T
		Expect(40); // "as"
		addAlt(set0, 5); // ALT
		addAlt(1); // ALT
		addAlt(1); // ALT
		addAlt(1); // ALT
		addAlt(1); // ALT
		if (StartOf(5)) {
			BaseType‿NT();
		} else if (isKind(la, 1)) {
			DottedIdent‿NT();
		} else if (isKind(la, 1)) {
			ClassType‿NT();
		} else if (isKind(la, 1)) {
			EnumType‿NT();
		} else if (isKind(la, 1)) {
			FlagsType‿NT();
		} else SynErr(79);
	}

	void Mimics‿NT() {
		addAlt(53); // T
		Expect(53); // "mimics"
		MimicsSpec‿NT();
	}

	void EmptyType‿NT() {
	}

	void InitValue‿NT() {
		addAlt(2); // ALT
		addAlt(3); // ALT
		addAlt(4); // ALT
		addAlt(37); // ALT
		addAlt(38); // ALT
		addAlt(39); // ALT
		addAlt(1); // ALT
		switch (la.kind) {
		case 2: // number
		{
			Get();
			break;
		}
		case 3: // int
		{
			Get();
			break;
		}
		case 4: // string
		{
			Get();
			break;
		}
		case 37: // "true"
		{
			Get();
			break;
		}
		case 38: // "false"
		{
			Get();
			break;
		}
		case 39: // "#"
		{
			Get();
			addAlt(set0, 6); // ITER start
			while (StartOf(6)) {
				Get();
				addAlt(set0, 6); // ITER end
			}
			addAlt(39); // T
			Expect(39); // "#"
			break;
		}
		case 1: // ident
		case 12: // version
		case 13: // search
		case 14: // select
		case 15: // details
		case 16: // edit
		case 17: // clear
		case 18: // keys
		case 19: // displayname
		case 20: // vbident
		{
			FunctionCall‿NT();
			break;
		}
		default: SynErr(80); break;
		}
	}

	void SampleValue‿NT() {
		addAlt(5); // T
		Expect(5); // braced
	}

	void FunctionCall‿NT() {
		DottedIdent‿NT();
		addAlt(6); // T
		Expect(6); // bracketed
	}

	void BaseType‿NT() {
		addAlt(41); // ALT
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
		switch (la.kind) {
		case 41: // "double"
		{
			Get();
			break;
		}
		case 42: // "date"
		{
			Get();
			break;
		}
		case 43: // "datetime"
		{
			Get();
			break;
		}
		case 44: // "integer"
		{
			Get();
			break;
		}
		case 45: // "percent"
		{
			Get();
			break;
		}
		case 46: // "percentwithdefault"
		{
			Get();
			break;
		}
		case 47: // "doublewithdefault"
		{
			Get();
			break;
		}
		case 48: // "integerwithdefault"
		{
			Get();
			break;
		}
		case 49: // "n2"
		{
			Get();
			break;
		}
		case 50: // "n0"
		{
			Get();
			break;
		}
		case 51: // "string"
		{
			Get();
			break;
		}
		case 52: // "string()"
		{
			Get();
			break;
		}
		default: SynErr(81); break;
		}
	}

	void EnumType‿NT() {
		addAlt(1); // T
		Expect(1); // ident
	}

	void FlagsType‿NT() {
		addAlt(1); // T
		Expect(1); // ident
	}

	void MimicsSpec‿NT() {
		addAlt(1); // ALT
		addAlt(54); // ALT
		addAlt(55); // ALT
		addAlt(56); // ALT
		addAlt(57); // ALT
		if (isKind(la, 1)) {
			EnumType‿NT();
		} else if (isKind(la, 54)) {
			Query‿NT();
		} else if (isKind(la, 55)) {
			Txt‿NT();
		} else if (isKind(la, 56)) {
			XL‿NT();
		} else if (isKind(la, 57)) {
			Ref‿NT();
		} else SynErr(82);
	}

	void Query‿NT() {
		addAlt(54); // T
		Expect(54); // "query"
		addAlt(10); // T
		Expect(10); // colon
		addAlt(1); // T
		Expect(1); // ident
		addAlt(8); // T
		Expect(8); // dot
		addAlt(1); // T
		Expect(1); // ident
		addAlt(10); // T
		Expect(10); // colon
		StringOrIdent‿NT();
	}

	void Txt‿NT() {
		addAlt(55); // T
		Expect(55); // "txt"
		addAlt(10); // T
		Expect(10); // colon
		addAlt(1); // T
		Expect(1); // ident
		addAlt(8); // T
		Expect(8); // dot
		addAlt(1); // T
		Expect(1); // ident
		addAlt(10); // T
		Expect(10); // colon
		StringOrIdent‿NT();
	}

	void XL‿NT() {
		addAlt(56); // T
		Expect(56); // "xl"
		addAlt(10); // T
		Expect(10); // colon
		addAlt(1); // T
		Expect(1); // ident
		addAlt(8); // T
		Expect(8); // dot
		addAlt(1); // T
		Expect(1); // ident
		addAlt(10); // T
		Expect(10); // colon
		StringOrIdent‿NT();
	}

	void Ref‿NT() {
		addAlt(57); // T
		Expect(57); // "ref"
		addAlt(10); // T
		Expect(10); // colon
		addAlt(18); // ALT
		addAlt(19); // ALT
		if (isKind(la, 18)) {
			Get();
		} else if (isKind(la, 19)) {
			Get();
		} else SynErr(83);
		addAlt(10); // T
		Expect(10); // colon
		StringOrIdent‿NT();
	}

	void StringOrIdent‿NT() {
		addAlt(4); // ALT
		addAlt(1); // ALT
		if (isKind(la, 4)) {
			Get();
		} else if (isKind(la, 1)) {
			DottedIdent‿NT();
		} else SynErr(84);
	}

	void SSCommands‿NT() {
		SSCommand‿NT();
		addAlt(9); // ITER start
		while (isKind(la, 9)) {
			Get();
			SSCommand‿NT();
			addAlt(9); // ITER end
		}
	}

	void SSCommand‿NT() {
		addAlt(13); // ALT
		addAlt(14); // ALT
		addAlt(15); // ALT
		addAlt(16); // ALT
		addAlt(17); // ALT
		if (isKind(la, 13)) {
			Get();
		} else if (isKind(la, 14)) {
			Get();
		} else if (isKind(la, 15)) {
			Get();
		} else if (isKind(la, 16)) {
			Get();
		} else if (isKind(la, 17)) {
			Get();
		} else SynErr(85);
	}

	void EnumValue‿NT() {
		addAlt(1); // T
		Expect(1); // ident
		addAlt(36); // OPT
		if (isKind(la, 36)) {
			EnumIntValue‿NT();
		}
	}

	void EnumValues‿NT() {
		addAlt(1); // ITER start
		while (isKind(la, 1)) {
			EnumValue‿NT();
			addAlt(1); // ITER end
		}
		addAlt(67); // T
		Expect(67); // "default"
		EnumValue‿NT();
		addAlt(1); // ITER start
		while (isKind(la, 1)) {
			EnumValue‿NT();
			addAlt(1); // ITER end
		}
	}

	void EnumIntValue‿NT() {
		addAlt(36); // T
		Expect(36); // "="
		addAlt(3); // T
		Expect(3); // int
	}



	public void Parse() {
		la = new Token();
		la.val = "";
		Get();
		WFModel‿NT();
		Expect(0);

	}
	
	// a token's base type
	public static readonly int[] tBase = {
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1,  1, 1, 1, 1,  1, 1, 1, 1,
		 1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1,-1, -1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","ident","\"-\"","\"-\"", "\"\"\"","\"{\"","\"(\"","\"end\"", "\".\"","\"|\"","\":\"","versionnumber", "\"version\"","\"search\"","\"select\"","\"details\"", "\"edit\"","\"clear\"","\"keys\"","\"displayname\"",
		"\"[\"","\"namespace\"","\"readerwriterprefix\"","\"rootclass\"", "\"data\"","\"class\"","\"via\"","\"inherits\"", "\"property\"","\"infoproperty\"","\"approperty\"","\"list\"", "\"selectlist\"","\"flagslist\"","\"longproperty\"","\"infolongproperty\"", "\"=\"","\"true\"","\"false\"","\"#\"",
		"\"as\"","\"double\"","\"date\"","\"datetime\"", "\"integer\"","\"percent\"","\"percentwithdefault\"","\"doublewithdefault\"", "\"integerwithdefault\"","\"n2\"","\"n0\"","\"string\"", "\"string()\"","\"mimics\"","\"query\"","\"txt\"", "\"xl\"","\"ref\"","\"subsystem\"","\"ssname\"",
		"\"ssconfig\"","\"sstyp\"","\"sscommands\"","\"sskey\"", "\"ssclear\"","\"flags\"","\"enum\"","\"default\"", "???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _x,_T,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _x,_T,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "int expected"; break;
			case 4: s = "string expected"; break;
			case 5: s = "braced expected"; break;
			case 6: s = "bracketed expected"; break;
			case 7: s = "end expected"; break;
			case 8: s = "dot expected"; break;
			case 9: s = "bar expected"; break;
			case 10: s = "colon expected"; break;
			case 11: s = "versionnumber expected"; break;
			case 12: s = "version expected"; break;
			case 13: s = "search expected"; break;
			case 14: s = "select expected"; break;
			case 15: s = "details expected"; break;
			case 16: s = "edit expected"; break;
			case 17: s = "clear expected"; break;
			case 18: s = "keys expected"; break;
			case 19: s = "displayname expected"; break;
			case 20: s = "vbident expected"; break;
			case 21: s = "\"namespace\" expected"; break;
			case 22: s = "\"readerwriterprefix\" expected"; break;
			case 23: s = "\"rootclass\" expected"; break;
			case 24: s = "\"data\" expected"; break;
			case 25: s = "\"class\" expected"; break;
			case 26: s = "\"via\" expected"; break;
			case 27: s = "\"inherits\" expected"; break;
			case 28: s = "\"property\" expected"; break;
			case 29: s = "\"infoproperty\" expected"; break;
			case 30: s = "\"approperty\" expected"; break;
			case 31: s = "\"list\" expected"; break;
			case 32: s = "\"selectlist\" expected"; break;
			case 33: s = "\"flagslist\" expected"; break;
			case 34: s = "\"longproperty\" expected"; break;
			case 35: s = "\"infolongproperty\" expected"; break;
			case 36: s = "\"=\" expected"; break;
			case 37: s = "\"true\" expected"; break;
			case 38: s = "\"false\" expected"; break;
			case 39: s = "\"#\" expected"; break;
			case 40: s = "\"as\" expected"; break;
			case 41: s = "\"double\" expected"; break;
			case 42: s = "\"date\" expected"; break;
			case 43: s = "\"datetime\" expected"; break;
			case 44: s = "\"integer\" expected"; break;
			case 45: s = "\"percent\" expected"; break;
			case 46: s = "\"percentwithdefault\" expected"; break;
			case 47: s = "\"doublewithdefault\" expected"; break;
			case 48: s = "\"integerwithdefault\" expected"; break;
			case 49: s = "\"n2\" expected"; break;
			case 50: s = "\"n0\" expected"; break;
			case 51: s = "\"string\" expected"; break;
			case 52: s = "\"string()\" expected"; break;
			case 53: s = "\"mimics\" expected"; break;
			case 54: s = "\"query\" expected"; break;
			case 55: s = "\"txt\" expected"; break;
			case 56: s = "\"xl\" expected"; break;
			case 57: s = "\"ref\" expected"; break;
			case 58: s = "\"subsystem\" expected"; break;
			case 59: s = "\"ssname\" expected"; break;
			case 60: s = "\"ssconfig\" expected"; break;
			case 61: s = "\"sstyp\" expected"; break;
			case 62: s = "\"sscommands\" expected"; break;
			case 63: s = "\"sskey\" expected"; break;
			case 64: s = "\"ssclear\" expected"; break;
			case 65: s = "\"flags\" expected"; break;
			case 66: s = "\"enum\" expected"; break;
			case 67: s = "\"default\" expected"; break;
			case 68: s = "??? expected"; break;
			case 69: s = "this symbol not expected in Namespace"; break;
			case 70: s = "this symbol not expected in ReaderWriterPrefix"; break;
			case 71: s = "this symbol not expected in RootClass"; break;
			case 72: s = "this symbol not expected in Class"; break;
			case 73: s = "this symbol not expected in SubSystem"; break;
			case 74: s = "this symbol not expected in Enum"; break;
			case 75: s = "this symbol not expected in Flags"; break;
			case 76: s = "this symbol not expected in Prop"; break;
			case 77: s = "invalid Prop"; break;
			case 78: s = "invalid Type"; break;
			case 79: s = "invalid As"; break;
			case 80: s = "invalid InitValue"; break;
			case 81: s = "invalid BaseType"; break;
			case 82: s = "invalid MimicsSpec"; break;
			case 83: s = "invalid Ref"; break;
			case 84: s = "invalid StringOrIdent"; break;
			case 85: s = "invalid SSCommand"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}

public class Alternative {
	public readonly Token t;
	public readonly BitArray alt;
	public readonly Symboltable[] st;

	public Alternative(Token t, BitArray alt, Symboltable[] st) {
		this.t = t;
		this.alt = alt;
		this.st = st;		
	}

	public Token declaredAt {
		get {
			// foreach(Symboltable tab in st) if (tab != null) Console.Write("{0}-", tab.name);			
			int k = t.kind;
			while(k >= 0) {		
				// Console.WriteLine("{0} test kind {1}", t.val, k);
				Symboltable table = st[k];
				if (table != null) {
					// Console.WriteLine("  has ST {0}", table.name);
					Token tt = table.Find(t);
					if (tt != null)
						return tt;
					// Console.WriteLine("  but no defining token");
				}
				k = Parser.tBase[k];
			}
			return null;
		}
	}
}

public class Symboltable {
	private Stack<List<Token>> scopes;
	private Stack<List<Token>> undeclaredTokens = new Stack<List<Token>>();
	public readonly string name;
	public readonly bool ignoreCase;
	public readonly bool strict;
	private Symboltable clone = null;

	public Symboltable(string name, bool ignoreCase, bool strict) {
		this.name = name;
		this.ignoreCase = ignoreCase;
		this.strict = strict;
		this.scopes = new Stack<List<Token>>();
		pushNewScope();
	}

	private Symboltable(Symboltable st) {
		this.name = st.name;
		this.ignoreCase = st.ignoreCase;
		this.strict = st.strict;

		// now copy the scopes and its lists
		this.scopes = new Stack<List<Token>>();				 		
		Stack<List<Token>> reverse = new Stack<List<Token>>(st.scopes);
		foreach(List<Token> list in reverse) {
			if (strict)
				this.scopes.Push(new List<Token>(list)); // strict: copy the list values
			else
				this.scopes.Push(list); // non strict: copy the list reference
		}
	}

	// We can keep the clone until we push/pop of the stack, or add a new item. 
	public Symboltable CloneScopes() {
		if (clone != null) return clone;
		clone = new Symboltable(this); // i.e. copy scopes
		return clone;
	}

	private StringComparer comparer {
		get {
			return ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
		}
	}

	private Token Find(IEnumerable<Token> list, Token tok) {
		StringComparer cmp = comparer;   
		foreach(Token t in list)
			if (0 == cmp.Compare(t.val, tok.val))
				return t;
		return null;
	} 

	public Token Find(Token t) {
		foreach(List<Token> list in scopes) {
			Token tok = Find(list, t);
			if (tok != null) return tok;
		}
		return null;
	}

	// ----------------------------------- for Parser use start -------------------- 
	
	public bool Use(Token t) {
		if (Contains(t)) return true; // it's ok, if we know the symbol
		if (strict) return false; // in strict mode we report an illegal symbol
		undeclaredTokens.Peek().Add(t); // in non strict mode we store the token for future checking
		return true; // we can't report an invalid symbol yet, so report "all ok".
	}

	public bool Add(Token t) {
		if (Find(currentScope, t) != null)
			return false;
		if (strict) clone = null; // if non strict, we have to keep the clone
		currentScope.Add(t);
		RemoveUndeclared(t);
		return true;
	}

	public void Add(string s) {
		Token t = new Token();
		t.kind = -1;
		t.pos = -1;
		t.charPos = -1;
		t.val = s;
		t.line = -1;
		Add(t);
	}

	// ----------------------------------- for Parser use end --------------------	

	public bool Contains(Token t) {
		return (Find(t) != null);
	}

	void RemoveUndeclared(Token tok) {
		StringComparer cmp = comparer;
		List<Token> found = new List<Token>();
		List<Token> undeclared = undeclaredTokens.Peek(); 
		foreach(Token t in undeclared)
			if (0 == cmp.Compare(t.val, tok.val))
				found.Add(t);
		foreach(Token t in found)
			undeclared.Remove(t);
	}

	void pushNewScope() {
		clone = null;
		scopes.Push(new List<Token>());
		undeclaredTokens.Push(new List<Token>());
	}

	void popScope() {
		clone = null;
		scopes.Pop();
		PromoteUndeclaredToParent();
	}

	public void CheckDeclared(Errors errors) {
		List<Token> list = undeclaredTokens.Peek();
		foreach(Token t in list) {
			string msg = string.Format(Parser.MissingSymbol, Parser.tName[t.kind], t.val, this.name);
			errors.SemErr(t.line, t.col, msg);
		}
	}

	void PromoteUndeclaredToParent() {
		List<Token> list = undeclaredTokens.Pop();
		undeclaredTokens.Peek().AddRange(list);
	} 

	public IDisposable createScope() {
		pushNewScope();
		return new Popper(this);
	} 

	public List<Token> currentScope {
		get { return scopes.Peek(); } 
	}

	public IEnumerable<Token> items {
		get {
		    if (scopes.Count == 1) return currentScope;

			Symboltable all = new Symboltable(name, ignoreCase, strict);
			foreach(List<Token> list in scopes)
				foreach(Token t in list)
					all.Add(t);
			return all.currentScope; 
		}
	}

	public int CountScopes {
		get { return scopes.Count; }
	}

	private class Popper : IDisposable {
		private readonly Symboltable st;

		public Popper(Symboltable st) {
			this.st = st;
		}

		public void Dispose() {
			st.popScope();
		}
	}
}
