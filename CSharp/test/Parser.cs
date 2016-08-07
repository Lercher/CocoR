
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;



public class Parserbase {
	public virtual void Prime(Token t) { /* hook */ }
}

public class Parser : Parserbase {
	public const int _EOF = 0; // TOKEN EOF
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
	public const int maxT = 72;

	const bool _T = true;
	const bool _x = false;
	public const string DuplicateSymbol = "{0} '{1}' declared twice in '{2}'";
	public const string MissingSymbol ="{0} '{1}' not declared in '{2}'";
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;
	public readonly List<Alternative> tokens = new List<Alternative>();
	public AST ast;
	public readonly AST.Builder astbuilder; 
	
	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

	public readonly Symboltable types;
	public readonly Symboltable enumtypes;
	public Symboltable symbols(string name) {
		if (name == "types") return types;
		if (name == "enumtypes") return enumtypes;
		return null;
	}

public override void Prime(Token t) { 
		if (t.kind == _string || t.kind == _braced || t.kind == _bracketed) 
		t.val = t.val.Substring(1, t.val.Length - 2);
	}



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
		astbuilder = new AST.Builder(this);
		types = new Symboltable("types", true, false, tokens);
		enumtypes = new Symboltable("enumtypes", true, false, tokens);

	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		SemErr(t, msg);
	}
	
	public void SemErr (Token t, string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}

	void Get () {
		for (;;) {
			t = la;

			if (alternatives != null) {
				tokens.Add(new Alternative(t, alternatives));
			}
			_newAlt();

			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}


	Alt alternatives = null;

	void _newAlt() {
		alternatives = new Alt(maxT + 1);
	}

	void addAlt(int kind) {
		alternatives.alt[kind] = true;
	}

	// a terminal tokenclass of kind kind is restricted to this symbol table 
	void addAlt(int kind, Symboltable st) {
		// take the root scope, if it is the only scope,
		// make a copy of the scope stack otherwise, but preserve the list references
		alternatives.altst[kind] = st.CloneScopes();
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
		using(astbuilder.createBarrier())
		{
		Version‿NT();
		Namespace‿NT();
		addAlt(23); // OPT
		if (isKind(la, 23)) {
			ReaderWriterPrefix‿NT();
		}
		using(astbuilder.createMarker(null, "rootclass", false, false, false))  RootClass‿NT();
		addAlt(set0, 1); // ITER start
		while (StartOf(1)) {
			addAlt(26); // ALT
			addAlt(62); // ALT
			addAlt(70); // ALT
			addAlt(69); // ALT
			if (isKind(la, 26)) {
				using(astbuilder.createMarker(null, "class", true, false, false))  Class‿NT();
			} else if (isKind(la, 62)) {
				using(astbuilder.createMarker(null, "subsystem", true, false, false))  SubSystem‿NT();
			} else if (isKind(la, 70)) {
				using(astbuilder.createMarker(null, "enum", true, false, false))  Enum‿NT();
			} else {
				using(astbuilder.createMarker(null, "flags", true, false, false))  Flags‿NT();
			}
			addAlt(set0, 1); // ITER end
		}
		EndNamespace‿NT();
	}}

	void Version‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(13); // T
		Expect(13); // version
		addAlt(12); // T
		Expect(12); // versionnumber
	}}

	void Namespace‿NT() {
		using(astbuilder.createBarrier())
		{
		while (!(isKind(la, 0) || isKind(la, 22))) {SynErr(73); Get();}
		addAlt(22); // T
		Expect(22); // "namespace"
		DottedIdent‿NT();
	}}

	void ReaderWriterPrefix‿NT() {
		using(astbuilder.createBarrier())
		{
		while (!(isKind(la, 0) || isKind(la, 23))) {SynErr(74); Get();}
		addAlt(23); // T
		Expect(23); // "readerwriterprefix"
		addAlt(1); // T
		Expect(1); // ident
	}}

	void RootClass‿NT() {
		using(astbuilder.createBarrier())
		{
		while (!(isKind(la, 0) || isKind(la, 24))) {SynErr(75); Get();}
		addAlt(24); // T
		using(astbuilder.createMarker(null, "typ", false, true, false))  Expect(24); // "rootclass"
		addAlt(25); // T
		using(astbuilder.createMarker(null, "name", false, true, false))  Expect(25); // "data"
		using(astbuilder.createMarker(null, "properties", true, false, false))  Properties‿NT();
		addAlt(8); // T
		Expect(8); // end
		addAlt(26); // T
		Expect(26); // "class"
	}}

	void Class‿NT() {
		using(astbuilder.createBarrier())
		{
		while (!(isKind(la, 0) || isKind(la, 26))) {SynErr(76); Get();}
		addAlt(26); // T
		using(astbuilder.createMarker(null, "typ", false, true, false))  Expect(26); // "class"
		if (!types.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "ident", la.val, types.name));
		alternatives.tdeclares = types;
		addAlt(1); // T
		using(astbuilder.createMarker(null, "name", false, true, false))  Expect(1); // ident
		addAlt(6); // OPT
		if (isKind(la, 6)) {
			Title‿NT();
		}
		addAlt(28); // OPT
		if (isKind(la, 28)) {
			Inherits‿NT();
		}
		addAlt(27); // OPT
		if (isKind(la, 27)) {
			Via‿NT();
		}
		using(astbuilder.createMarker(null, "properties", true, false, false))  Properties‿NT();
		addAlt(8); // T
		Expect(8); // end
		addAlt(26); // T
		Expect(26); // "class"
	}}

	void SubSystem‿NT() {
		using(astbuilder.createBarrier())
		{
		while (!(isKind(la, 0) || isKind(la, 62))) {SynErr(77); Get();}
		addAlt(62); // T
		Expect(62); // "subsystem"
		if (!types.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "ident", la.val, types.name));
		alternatives.tdeclares = types;
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
		addAlt(63); // T
		Expect(63); // "ssname"
		addAlt(1); // T
		Expect(1); // ident
		addAlt(64); // T
		Expect(64); // "ssconfig"
		addAlt(1); // T
		Expect(1); // ident
		addAlt(65); // T
		Expect(65); // "sstyp"
		addAlt(1); // T
		Expect(1); // ident
		addAlt(66); // T
		Expect(66); // "sscommands"
		SSCommands‿NT();
		addAlt(67); // OPT
		if (isKind(la, 67)) {
			Get();
			addAlt(5); // T
			Expect(5); // string
		}
		addAlt(68); // OPT
		if (isKind(la, 68)) {
			Get();
			addAlt(5); // T
			Expect(5); // string
		}
		addAlt(30); // ITER start
		while (isKind(la, 30)) {
			InfoProperty‿NT();
			addAlt(30); // ITER end
		}
		addAlt(8); // T
		Expect(8); // end
		addAlt(62); // T
		Expect(62); // "subsystem"
	}}

	void Enum‿NT() {
		using(astbuilder.createBarrier())
		{
		while (!(isKind(la, 0) || isKind(la, 70))) {SynErr(78); Get();}
		addAlt(70); // T
		Expect(70); // "enum"
		if (!enumtypes.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "ident", la.val, enumtypes.name));
		alternatives.tdeclares = enumtypes;
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
		EnumValues‿NT();
		addAlt(8); // T
		Expect(8); // end
		addAlt(70); // T
		Expect(70); // "enum"
	}}

	void Flags‿NT() {
		using(astbuilder.createBarrier())
		{
		while (!(isKind(la, 0) || isKind(la, 69))) {SynErr(79); Get();}
		addAlt(69); // T
		Expect(69); // "flags"
		if (!types.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "ident", la.val, types.name));
		alternatives.tdeclares = types;
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
		addAlt(1); // ITER start
		while (isKind(la, 1)) {
			EnumValue‿NT();
			addAlt(1); // ITER end
		}
		addAlt(8); // T
		Expect(8); // end
		addAlt(69); // T
		Expect(69); // "flags"
	}}

	void EndNamespace‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(8); // T
		Expect(8); // end
		addAlt(22); // T
		Expect(22); // "namespace"
	}}

	void DottedIdent‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(2); // OPT
		if (isKind(la, 2)) {
			Get();
			addAlt(9); // T
			Expect(9); // dot
			addAlt(2); // ITER start
			while (isKind(la, 2)) {
				Get();
				addAlt(9); // T
				Expect(9); // dot
				addAlt(2); // ITER end
			}
		}
		addAlt(1); // T
		Expect(1); // ident
	}}

	void Properties‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(set0, 2); // ITER start
		while (StartOf(2)) {
			Prop‿NT();
			addAlt(set0, 2); // ITER end
		}
	}}

	void Title‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(6); // T
		Expect(6); // braced
	}}

	void Inherits‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(28); // T
		Expect(28); // "inherits"
		DottedIdent‿NT();
	}}

	void Via‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(27); // T
		Expect(27); // "via"
		DottedIdent‿NT();
	}}

	void Prop‿NT() {
		using(astbuilder.createBarrier())
		{
		while (!(StartOf(3))) {SynErr(80); Get();}
		addAlt(29); // ALT
		addAlt(30); // ALT
		addAlt(31); // ALT
		addAlt(32); // ALT
		addAlt(33); // ALT
		addAlt(34); // ALT
		addAlt(35); // ALT
		addAlt(36); // ALT
		switch (la.kind) {
		case 29: // "property"
		{
			Property‿NT();
			break;
		}
		case 30: // "infoproperty"
		{
			InfoProperty‿NT();
			break;
		}
		case 31: // "approperty"
		{
			APProperty‿NT();
			break;
		}
		case 32: // "list"
		{
			List‿NT();
			break;
		}
		case 33: // "selectlist"
		{
			SelectList‿NT();
			break;
		}
		case 34: // "flagslist"
		{
			FlagsList‿NT();
			break;
		}
		case 35: // "longproperty"
		{
			LongProperty‿NT();
			break;
		}
		case 36: // "infolongproperty"
		{
			InfoLongProperty‿NT();
			break;
		}
		default: SynErr(81); break;
		}
	}}

	void Property‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(29); // T
		Expect(29); // "property"
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
		Type‿NT();
	}}

	void InfoProperty‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(30); // T
		Expect(30); // "infoproperty"
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
		Type‿NT();
	}}

	void APProperty‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(31); // T
		Expect(31); // "approperty"
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
		Type‿NT();
	}}

	void List‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(32); // T
		Expect(32); // "list"
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
		addAlt(41); // OPT
		if (isKind(la, 41)) {
			As‿NT();
		}
	}}

	void SelectList‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(33); // T
		Expect(33); // "selectlist"
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
		As‿NT();
	}}

	void FlagsList‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(34); // T
		Expect(34); // "flagslist"
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
		Mimics‿NT();
	}}

	void LongProperty‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(35); // T
		Expect(35); // "longproperty"
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
	}}

	void InfoLongProperty‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(36); // T
		Expect(36); // "infolongproperty"
		addAlt(1); // T
		using(astbuilder.createMarker(null, null, false, true, false))  Expect(1); // ident
	}}

	void Type‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(set0, 4); // ALT
		addAlt(41); // ALT
		addAlt(57); // ALT
		if (StartOf(4)) {
			EmptyType‿NT();
		} else if (isKind(la, 41)) {
			As‿NT();
		} else if (isKind(la, 57)) {
			Mimics‿NT();
		} else SynErr(82);
		addAlt(37); // OPT
		if (isKind(la, 37)) {
			Get();
			InitValue‿NT();
		}
		addAlt(6); // OPT
		if (isKind(la, 6)) {
			SampleValue‿NT();
		}
	}}

	void As‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(41); // T
		Expect(41); // "as"
		addAlt(set0, 5); // ALT
		addAlt(1); // ALT
		addAlt(1, types); // ALT ident uses symbol table 'types'
		addAlt(new int[] {1, 2}); // ALT
		if (StartOf(5)) {
			BaseType‿NT();
		} else if (isKind(la, 1)) {
			if (!types.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, types.name));
			Get();
		} else if (isKind(la, 1) || isKind(la, 2)) {
			DottedIdent‿NT();
		} else SynErr(83);
	}}

	void Mimics‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(57); // T
		Expect(57); // "mimics"
		MimicsSpec‿NT();
	}}

	void EmptyType‿NT() {
		using(astbuilder.createBarrier())
		{
	}}

	void InitValue‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(3); // ALT
		addAlt(4); // ALT
		addAlt(5); // ALT
		addAlt(38); // ALT
		addAlt(39); // ALT
		addAlt(40); // ALT
		addAlt(new int[] {1, 2}); // ALT
		switch (la.kind) {
		case 3: // number
		{
			Get();
			break;
		}
		case 4: // int
		{
			Get();
			break;
		}
		case 5: // string
		{
			Get();
			break;
		}
		case 38: // "true"
		{
			Get();
			break;
		}
		case 39: // "false"
		{
			Get();
			break;
		}
		case 40: // "#"
		{
			Get();
			addAlt(set0, 6); // ITER start
			while (StartOf(6)) {
				Get();
				addAlt(set0, 6); // ITER end
			}
			addAlt(40); // T
			Expect(40); // "#"
			break;
		}
		case 1: // ident
		case 2: // dottedident
		case 13: // version
		case 14: // search
		case 15: // select
		case 16: // details
		case 17: // edit
		case 18: // clear
		case 19: // keys
		case 20: // displayname
		case 21: // vbident
		{
			FunctionCall‿NT();
			break;
		}
		default: SynErr(84); break;
		}
	}}

	void SampleValue‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(6); // T
		Expect(6); // braced
	}}

	void FunctionCall‿NT() {
		using(astbuilder.createBarrier())
		{
		DottedIdent‿NT();
		addAlt(7); // T
		Expect(7); // bracketed
	}}

	void BaseType‿NT() {
		using(astbuilder.createBarrier())
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
		switch (la.kind) {
		case 42: // "double"
		{
			Get();
			break;
		}
		case 43: // "date"
		{
			Get();
			break;
		}
		case 44: // "datetime"
		{
			Get();
			break;
		}
		case 45: // "integer"
		{
			Get();
			break;
		}
		case 46: // "percent"
		{
			Get();
			break;
		}
		case 47: // "percentwithdefault"
		{
			Get();
			break;
		}
		case 48: // "doublewithdefault"
		{
			Get();
			break;
		}
		case 49: // "integerwithdefault"
		{
			Get();
			break;
		}
		case 50: // "n2"
		{
			Get();
			break;
		}
		case 51: // "n0"
		{
			Get();
			break;
		}
		case 52: // "string"
		{
			Get();
			break;
		}
		case 53: // "boolean"
		{
			Get();
			break;
		}
		case 54: // "guid"
		{
			Get();
			break;
		}
		case 55: // "string()"
		{
			Get();
			break;
		}
		case 56: // "xml"
		{
			Get();
			break;
		}
		default: SynErr(85); break;
		}
	}}

	void MimicsSpec‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(58); // ALT
		addAlt(59); // ALT
		addAlt(60); // ALT
		addAlt(61); // ALT
		addAlt(1); // ALT
		addAlt(1, enumtypes); // ALT ident uses symbol table 'enumtypes'
		if (isKind(la, 58)) {
			Query‿NT();
		} else if (isKind(la, 59)) {
			Txt‿NT();
		} else if (isKind(la, 60)) {
			XL‿NT();
		} else if (isKind(la, 61)) {
			Ref‿NT();
		} else if (isKind(la, 1)) {
			if (!enumtypes.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, enumtypes.name));
			Get();
		} else SynErr(86);
	}}

	void Query‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(58); // T
		Expect(58); // "query"
		addAlt(11); // T
		Expect(11); // colon
		addAlt(2); // T
		Expect(2); // dottedident
		addAlt(9); // T
		Expect(9); // dot
		addAlt(1); // T
		Expect(1); // ident
		addAlt(11); // T
		Expect(11); // colon
		StringOrIdent‿NT();
	}}

	void Txt‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(59); // T
		Expect(59); // "txt"
		addAlt(11); // T
		Expect(11); // colon
		addAlt(2); // T
		Expect(2); // dottedident
		addAlt(9); // T
		Expect(9); // dot
		addAlt(1); // T
		Expect(1); // ident
		addAlt(11); // T
		Expect(11); // colon
		StringOrIdent‿NT();
	}}

	void XL‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(60); // T
		Expect(60); // "xl"
		addAlt(11); // T
		Expect(11); // colon
		addAlt(2); // T
		Expect(2); // dottedident
		addAlt(9); // T
		Expect(9); // dot
		addAlt(1); // T
		Expect(1); // ident
		addAlt(11); // T
		Expect(11); // colon
		StringOrIdent‿NT();
	}}

	void Ref‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(61); // T
		Expect(61); // "ref"
		addAlt(11); // T
		Expect(11); // colon
		addAlt(19); // ALT
		addAlt(20); // ALT
		if (isKind(la, 19)) {
			Get();
		} else if (isKind(la, 20)) {
			Get();
		} else SynErr(87);
		addAlt(11); // T
		Expect(11); // colon
		StringOrIdent‿NT();
	}}

	void StringOrIdent‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(5); // ALT
		addAlt(new int[] {1, 2}); // ALT
		if (isKind(la, 5)) {
			Get();
		} else if (isKind(la, 1) || isKind(la, 2)) {
			DottedIdent‿NT();
		} else SynErr(88);
	}}

	void SSCommands‿NT() {
		using(astbuilder.createBarrier())
		{
		SSCommand‿NT();
		addAlt(10); // ITER start
		while (isKind(la, 10)) {
			Get();
			SSCommand‿NT();
			addAlt(10); // ITER end
		}
	}}

	void SSCommand‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(14); // ALT
		addAlt(15); // ALT
		addAlt(16); // ALT
		addAlt(17); // ALT
		addAlt(18); // ALT
		if (isKind(la, 14)) {
			Get();
		} else if (isKind(la, 15)) {
			Get();
		} else if (isKind(la, 16)) {
			Get();
		} else if (isKind(la, 17)) {
			Get();
		} else if (isKind(la, 18)) {
			Get();
		} else SynErr(89);
	}}

	void EnumValue‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(1); // T
		Expect(1); // ident
		addAlt(37); // OPT
		if (isKind(la, 37)) {
			EnumIntValue‿NT();
		}
	}}

	void EnumValues‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(1); // ITER start
		while (isKind(la, 1)) {
			EnumValue‿NT();
			addAlt(1); // ITER end
		}
		addAlt(71); // T
		Expect(71); // "default"
		EnumValue‿NT();
		addAlt(1); // ITER start
		while (isKind(la, 1)) {
			EnumValue‿NT();
			addAlt(1); // ITER end
		}
	}}

	void EnumIntValue‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(37); // T
		Expect(37); // "="
		addAlt(4); // T
		Expect(4); // int
	}}



	public void Parse() {
		la = new Token();
		la.val = "";
		Get();
		WFModel‿NT();
		Expect(0);
		types.CheckDeclared(errors);
		enumtypes.CheckDeclared(errors);

		ast = astbuilder.current;
	}
	
	// a token's base type
	public static readonly int[] tBase = {
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1, 1, 1, 1,  1, 1, 1, 1,
		 1, 1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","ident","dottedident","number", "int","string","braced","bracketed", "\"end\"","\".\"","\"|\"","\":\"", "versionnumber","\"version\"","\"search\"","\"select\"", "\"details\"","\"edit\"","\"clear\"","\"keys\"",
		"\"displayname\"","vbident","\"namespace\"","\"readerwriterprefix\"", "\"rootclass\"","\"data\"","\"class\"","\"via\"", "\"inherits\"","\"property\"","\"infoproperty\"","\"approperty\"", "\"list\"","\"selectlist\"","\"flagslist\"","\"longproperty\"", "\"infolongproperty\"","\"=\"","\"true\"","\"false\"",
		"\"#\"","\"as\"","\"double\"","\"date\"", "\"datetime\"","\"integer\"","\"percent\"","\"percentwithdefault\"", "\"doublewithdefault\"","\"integerwithdefault\"","\"n2\"","\"n0\"", "\"string\"","\"boolean\"","\"guid\"","\"string()\"", "\"xml\"","\"mimics\"","\"query\"","\"txt\"",
		"\"xl\"","\"ref\"","\"subsystem\"","\"ssname\"", "\"ssconfig\"","\"sstyp\"","\"sscommands\"","\"sskey\"", "\"ssclear\"","\"flags\"","\"enum\"","\"default\"", "???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_T,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_T,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x}

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
			case 2: s = "dottedident expected"; break;
			case 3: s = "number expected"; break;
			case 4: s = "int expected"; break;
			case 5: s = "string expected"; break;
			case 6: s = "braced expected"; break;
			case 7: s = "bracketed expected"; break;
			case 8: s = "end expected"; break;
			case 9: s = "dot expected"; break;
			case 10: s = "bar expected"; break;
			case 11: s = "colon expected"; break;
			case 12: s = "versionnumber expected"; break;
			case 13: s = "version expected"; break;
			case 14: s = "search expected"; break;
			case 15: s = "select expected"; break;
			case 16: s = "details expected"; break;
			case 17: s = "edit expected"; break;
			case 18: s = "clear expected"; break;
			case 19: s = "keys expected"; break;
			case 20: s = "displayname expected"; break;
			case 21: s = "vbident expected"; break;
			case 22: s = "\"namespace\" expected"; break;
			case 23: s = "\"readerwriterprefix\" expected"; break;
			case 24: s = "\"rootclass\" expected"; break;
			case 25: s = "\"data\" expected"; break;
			case 26: s = "\"class\" expected"; break;
			case 27: s = "\"via\" expected"; break;
			case 28: s = "\"inherits\" expected"; break;
			case 29: s = "\"property\" expected"; break;
			case 30: s = "\"infoproperty\" expected"; break;
			case 31: s = "\"approperty\" expected"; break;
			case 32: s = "\"list\" expected"; break;
			case 33: s = "\"selectlist\" expected"; break;
			case 34: s = "\"flagslist\" expected"; break;
			case 35: s = "\"longproperty\" expected"; break;
			case 36: s = "\"infolongproperty\" expected"; break;
			case 37: s = "\"=\" expected"; break;
			case 38: s = "\"true\" expected"; break;
			case 39: s = "\"false\" expected"; break;
			case 40: s = "\"#\" expected"; break;
			case 41: s = "\"as\" expected"; break;
			case 42: s = "\"double\" expected"; break;
			case 43: s = "\"date\" expected"; break;
			case 44: s = "\"datetime\" expected"; break;
			case 45: s = "\"integer\" expected"; break;
			case 46: s = "\"percent\" expected"; break;
			case 47: s = "\"percentwithdefault\" expected"; break;
			case 48: s = "\"doublewithdefault\" expected"; break;
			case 49: s = "\"integerwithdefault\" expected"; break;
			case 50: s = "\"n2\" expected"; break;
			case 51: s = "\"n0\" expected"; break;
			case 52: s = "\"string\" expected"; break;
			case 53: s = "\"boolean\" expected"; break;
			case 54: s = "\"guid\" expected"; break;
			case 55: s = "\"string()\" expected"; break;
			case 56: s = "\"xml\" expected"; break;
			case 57: s = "\"mimics\" expected"; break;
			case 58: s = "\"query\" expected"; break;
			case 59: s = "\"txt\" expected"; break;
			case 60: s = "\"xl\" expected"; break;
			case 61: s = "\"ref\" expected"; break;
			case 62: s = "\"subsystem\" expected"; break;
			case 63: s = "\"ssname\" expected"; break;
			case 64: s = "\"ssconfig\" expected"; break;
			case 65: s = "\"sstyp\" expected"; break;
			case 66: s = "\"sscommands\" expected"; break;
			case 67: s = "\"sskey\" expected"; break;
			case 68: s = "\"ssclear\" expected"; break;
			case 69: s = "\"flags\" expected"; break;
			case 70: s = "\"enum\" expected"; break;
			case 71: s = "\"default\" expected"; break;
			case 72: s = "??? expected"; break;
			case 73: s = "this symbol not expected in Namespace"; break;
			case 74: s = "this symbol not expected in ReaderWriterPrefix"; break;
			case 75: s = "this symbol not expected in RootClass"; break;
			case 76: s = "this symbol not expected in Class"; break;
			case 77: s = "this symbol not expected in SubSystem"; break;
			case 78: s = "this symbol not expected in Enum"; break;
			case 79: s = "this symbol not expected in Flags"; break;
			case 80: s = "this symbol not expected in Prop"; break;
			case 81: s = "invalid Prop"; break;
			case 82: s = "invalid Type"; break;
			case 83: s = "invalid As"; break;
			case 84: s = "invalid InitValue"; break;
			case 85: s = "invalid BaseType"; break;
			case 86: s = "invalid MimicsSpec"; break;
			case 87: s = "invalid Ref"; break;
			case 88: s = "invalid StringOrIdent"; break;
			case 89: s = "invalid SSCommand"; break;

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

// mutatable alternatives
public class Alt {
	public BitArray alt = null;
	public Symboltable[] altst = null;
	public Symboltable tdeclares = null;
	public Symboltable tdeclared = null;
	public Token declaration = null;

	public Alt(int size) {
		alt = new BitArray(size);
		altst = new Symboltable[size];
	}
}

// non mutatable
public class Alternative {
	public readonly Token t;
	public readonly string declares = null;
	public readonly string declared = null;
	public readonly BitArray alt;
	public readonly Symboltable[] st;
	public Token declaration = null;

	public Alternative(Token t, Alt alternatives) {
		this.t = t;
		if (alternatives.tdeclares != null)
			this.declares = alternatives.tdeclares.name;
		if (alternatives.tdeclared != null)
			this.declared = alternatives.tdeclared.name;
		this.alt = alternatives.alt;
		this.st = alternatives.altst;
		this.declaration = alternatives.declaration;		
	}
}

public delegate void TokenEventHandler(Token t);
public class Symboltable {
	private Stack<List<Token>> scopes;
	private Stack<List<Token>> undeclaredTokens = new Stack<List<Token>>();
	public readonly string name;
	public readonly bool ignoreCase;
	public readonly bool strict;
	public readonly List<Alternative> fixuplist;
	private Symboltable clone = null;
	public event TokenEventHandler TokenUsed;

	public Symboltable(string name, bool ignoreCase, bool strict, List<Alternative> alternatives) {
		this.name = name;
		this.ignoreCase = ignoreCase;
		this.strict = strict;
		this.scopes = new Stack<List<Token>>();
		this.fixuplist = alternatives;
		pushNewScope();
	}

	private Symboltable(Symboltable st) {
		this.name = st.name;
		this.ignoreCase = st.ignoreCase;
		this.strict = st.strict;
		this.fixuplist = st.fixuplist;

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
	
	public bool Use(Token t, Alt a) {
		if (TokenUsed != null) TokenUsed(t);
		a.tdeclared = this;
		if (strict) {
			a.declaration = Find(t);
			if (a.declaration != null) return true; // it's ok, if we know the symbol
			return false; // in strict mode we report an illegal symbol
		} else {
			// in non-strict mode we can only use declarations
			// known in the top scope, so that we dont't find a declaration 
			// in a parent scope and redefine a symbol in this topmost scope 
			a.declaration = Find(currentScope, t);
			if (a.declaration != null) return true; // it's ok, if we know the symbol in this scope
		}
		// in non strict mode we need to store the token for future checking
		undeclaredTokens.Peek().Add(t); 
		return true; // we can't report an invalid symbol yet, so report "all ok".
	}

	public bool Add(Token t) {
		if (Find(currentScope, t) != null)
			return false;
		if (strict) clone = null; // if non strict, we have to keep the clone
		currentScope.Add(t);
		RemoveFromAndFixupList(undeclaredTokens.Peek(), t);
		return true;
	}

	public void Add(string s) {
		Token t = new Token();
		t.kind = -1;
		t.pos = -1;
		t.charPos = -1;
		t.val = s;
		t.line = -1;
		currentScope.Add(t);
	}

	// ----------------------------------- for Parser use end --------------------	

	public bool Contains(Token t) {
		return (Find(t) != null);
	}

	void RemoveFromAndFixupList(List<Token> undeclared, Token declaration) {
		StringComparer cmp = comparer;
		List<Token> found = new List<Token>();
		foreach(Token t in undeclared)
			if (0 == cmp.Compare(t.val, declaration.val))
				found.Add(t);
		foreach(Token t in found) {
			undeclared.Remove(t);
			foreach(Alternative a in fixuplist)
				if (a.t == t)
					a.declaration = declaration;
		}
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
		// now that the lexical scope is about to terminate, we know that there cannot be more declarations in this scope
		// so we can take the existing declarations of the parent scope to resolve these unresolved tokens in 'list'.
		foreach(Token decl in currentScope)
			RemoveFromAndFixupList(list, decl);
		// now list contains all tokens that were not delared in the popped scope
		// and not yet declared in the now current scope
		undeclaredTokens.Peek().AddRange(list);
	} 

	public IDisposable createScope() {
		pushNewScope();
		return new Popper(this);
	} 

	public IDisposable createUsageCheck(bool oneOrMore, Errors errors, Token scopeToken) {
		return new UseCounter(this, oneOrMore, errors, scopeToken);
	}

	public List<Token> currentScope {
		get { return scopes.Peek(); } 
	}

	public IEnumerable<Token> items {
		get {
		    if (scopes.Count == 1) return currentScope;

			Symboltable all = new Symboltable(name, ignoreCase, true, fixuplist);
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
			GC.SuppressFinalize(this);
			st.popScope();
		}
	}

	private class UseCounter : IDisposable {
		private readonly Symboltable st;
		public readonly bool oneOrMore; // t - 1..N, f - 0..1
		public readonly List<Token> uses;
		public readonly Errors errors;
		public readonly Token scopeToken;

		public UseCounter(Symboltable st, bool oneOrMore, Errors errors, Token scopeToken) {
			this.st = st;
			this.oneOrMore = oneOrMore;
			this.errors = errors;
			this.scopeToken = scopeToken;
			this.uses = new List<Token>();
			st.TokenUsed += uses.Add;
		}

		private bool isValid(List<Token> list) {
			int cnt = list.Count;
			if (oneOrMore) return (cnt >= 1);
			return (cnt <= 1);
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			st.TokenUsed -= uses.Add;
			Dictionary<string, List<Token>> counter = new Dictionary<string, List<Token>>(st.comparer);
			foreach(Token t in st.items)
				counter[t.val] = new List<Token>();
			foreach(Token t in uses)
				if (counter.ContainsKey(t.val)) // we ignore undeclared Tokens:
					counter[t.val].Add(t);
			// now check for validity
			foreach(string s in counter.Keys) {
				List<Token> list = counter[s];
				if (!isValid(list)) {
					if (oneOrMore) {
						string msg = string.Format("token '{0}' has to be used in this scope.", s); 
						errors.SemErr(scopeToken.line, scopeToken.col, msg);
					} else {
						string msg = string.Format("token '{0}' is used {1:n0} time(s) instead of at most once in this scope, see following errors for locations.", s, list.Count); 
						errors.SemErr(scopeToken.line, scopeToken.col, msg);
						foreach(Token t in list)
							errors.SemErr(t.line, t.col, "... here");
					}
				}
			} 
		}
	}
}

public abstract class AST {
    public abstract string val { get; }
    public abstract AST this[int i] { get; }
    public abstract AST this[string s] { get; }
    public abstract int count { get; }
    public static readonly AST empty = new ASTLiteral(string.Empty);
    protected abstract void serialize(StringBuilder sb, int indent);
    public virtual bool merge(E e) { return false; }
    
#region Formatting
	public static void newline(int indent, StringBuilder sb) {
        sb.AppendLine();
        for(int i = 0; i < indent; i++)
            sb.Append("  ");
    }

	public static void escape(string s, StringBuilder sb) {
		foreach (char ch in s) {
			switch(ch) {
				case '\\': sb.Append("\\\\"); break;
				case '\'': sb.Append("\\'"); break;
				case '\"': sb.Append("\\\""); break;
				case '\t': sb.Append("\\t"); break;
				case '\r': sb.Append("\\r"); break;
				case '\n': sb.Append("\\n"); break;
				default:
					if (ch < ' ' || ch > '\u007f') sb.AppendFormat("{0:x4}",ch);
					else sb.Append(ch);
					break;
			}
		}
	}

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        serialize(sb, 0);
        return sb.ToString();
    }

#endregion

    private abstract class ASTThrows : AST {
        public override string val { get { throw new ApplicationException("not a literal"); } }
        public override AST this[int i] { get { throw new ApplicationException("not a list"); } }
        public override AST this[string s] { get { throw new ApplicationException("not an object"); } }
    }

    private class ASTLiteral : ASTThrows {
        public ASTLiteral(string s) { _val = s; }
        private readonly string _val;
        public override string val { get { return _val; } }
        public override int count { get { return -1; } }

        protected override void serialize(StringBuilder sb, int indent)
        {
            sb.Append('\"');
            AST.escape(val, sb);
            sb.Append('\"');
        }
    }

    private class ASTList : ASTThrows {
        public readonly List<AST> list;

        public ASTList() {
			list = new List<AST>();
		}

        public ASTList(AST a, int i) : this() {
            list.Add(a);
        }

        public ASTList(AST a) {
            if (a is ASTList)
                list = ((ASTList)a).list;
            else {
                list = new List<AST>();
                list.Add(a);
            }
        }

        public override AST this[int i] { 
            get { 
                if (i < 0 || count <= i)
                    return AST.empty;
                return list[i];
            } 
        }
        public override int count { get { return list.Count; } }
        
        public AST merge(AST a) {
            if (a is ASTList) {
                ASTList li = (ASTList) a;
                list.AddRange(li.list);
            } else
                list.Add(a);
            return a;
        }

        protected override void serialize(StringBuilder sb, int indent)
        {
            bool longlist = (count > 3);
            sb.Append('[');
            if (longlist) AST.newline(indent + 1, sb);
            int n = 0;
            foreach(AST ast in list) {
                ast.serialize(sb, indent + 1);
                n++;
                if (n < count) {
                    sb.Append(", ");
                    if (longlist) AST.newline(indent + 1, sb);
                }
            }
            if (longlist) AST.newline(indent, sb);
            sb.Append(']');
        }

    }

    private class ASTObject : ASTThrows {
        private readonly Dictionary<string,AST> ht = new Dictionary<string,AST>();         
        public override AST this[string s] { 
            get { 
                if (!ht.ContainsKey(s))
                    return AST.empty;
                return ht[s];
            } 
        }
        public override int count { get { return ht.Keys.Count; } }
        
        public void add(E e) {
            ht[e.name] = e.ast; 
        }

        public override bool merge(E e) {
            if (e.name == null) return false; // cannot merge an unnamed thing
            if (!ht.ContainsKey(e.name)) {
                add(e);
                return true;
            }
            // we have e.nam, call it a thing:
            AST thing = ht[e.name];
            if (thing is ASTList) {
                ((ASTList) thing).merge(e.ast);
                return true;
            }
            // thing is not a list, so we cannot merge it with e
            return false;
        }

        protected override void serialize(StringBuilder sb, int indent) {
            bool longlist = (count > 3);
            sb.Append('{');
            if (longlist) AST.newline(indent + 1, sb);
            int n = 0;
            foreach(string name in ht.Keys) {
                AST ast = ht[name];
                sb.Append('\"');
                AST.escape(name, sb);
                sb.Append("\": ");
                ast.serialize(sb, indent + 1);
                n++;
                if (n < count) {
                    sb.Append(", ");
                    if (longlist) AST.newline(indent + 1, sb);
                }
            }
            if (longlist) AST.newline(indent, sb);
            sb.Append('}');
        }
    }

    public class E {
        public string name = null;
        public AST ast = null;

        public override string ToString() {
            string a = ast == null ? "null" : ast.ToString();
            string n = name == null ? "." : name;
            return string.Format("{0} = {1};", n, a);
        }

        public E add(E e) {
            if (name == e.name) {
				//if (name == null) Console.WriteLine(" [merge two unnamed to a single list]"); else Console.WriteLine(" [merge two named {0} to a single list]", name);
                ASTList list = new ASTList(ast);
                list.merge(e.ast);
                E ret = new E();
                ret.ast = list;
                ret.name = name;
                return ret;
            } else if (name != null && e.name != null) {
				//Console.WriteLine(" [merge named {0}+{1} to an unnamed object]", name, e.name);
                ASTObject obj = new ASTObject();
                obj.add(this);
                obj.add(e);
                E ret = new E();
                ret.ast = obj;
                return ret;
            } else if (ast.merge(e)) {
				//Console.WriteLine(" [merged {1} into object {0}]", name, e.name);
                return this;
			}
			//Console.WriteLine(" [no merge available for {0}+{1}]", name, e.name);
            return null;
        }

        public void wrapinlist(bool merge) {			
			if (ast == null) { 
				ast = new ASTList();
				return;
			}
			if (merge && (ast is ASTList)) return;
            ast = new ASTList(ast, 1);
        }
    }

    public class Builder {
        public readonly Parser parser;
        private readonly Stack<E> stack = new Stack<E>();

        public Builder(Parser parser) {
            this.parser = parser;
        }
        
        public E currentE { get { return stack.Peek(); } }
        
		public AST current { 
			get { 
				return stack.Count > 0 ? currentE.ast : new ASTObject(); 
			} 
		}

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach(E e in stack)
                sb.AppendFormat("{0}\n", e);
            return sb.ToString();
        }

        private void push(E e) {
            stack.Push(e);
            //System.Console.WriteLine("-> push {0}, size {1}", e, stack.Count);
        }

        // that's what we call for #/##, built from an AstOp
        public void hatch(Token t, string literal, string name, bool islist) {
            //System.Console.WriteLine(">> hatch token {0,-20} as {2,-10}, islist {3}, literal:{1} at {4},{5}.", t.val, literal, name, islist, t.line, t.col);
            E e = new E();
            e.ast = new ASTLiteral(literal != null ? literal : t.val);
            if (islist)
                e.ast = new ASTList(e.ast);
            e.name = name;
            push(e);
        }

        // that's what we call for ^/^^, built from an AstOp
        public void sendup(Token t, string literal, string name, bool islist) {
			if (stack.Count == 0) return;
            E e = currentE;
			if (e == null) {
				e = new E();
				if (islist)
					e.ast = new ASTList();
				else
					e.ast = new ASTObject();
				push(e);
			}
            //if (islist) System.Console.WriteLine(">> send up as [{0}]: {1}", name, e); else System.Console.WriteLine(">> send up as {0}: {1}", name, e);
            if (name != e.name) {
                if (islist) {
					bool merge = (e.name == null);
                    e.wrapinlist(merge);
				} else if (e.name != null)
                    parser.errors.Warning(t.line, t.col, string.Format("overwriting AST objectname '{0}' with '{1}'", e.name, name));
            }
            e.name = name;
            //System.Console.WriteLine("-------------> top {0}", e);
        }

		/*
		private void mergeConflict(Token t, E e, E with, string typ, int n) {
			parser.errors.Warning(t.line, t.col, string.Format("AST merge {2} size {3}: {0} WITH {1}", e, with, typ, n));
		} 
		*/

		// remove the topmost null on the stack, keeping anythng else 
		public void popNull() {
			Stack<E> list = new Stack<E>();
			while(true) {
				if (stack.Count == 0) break;
				E e = stack.Pop();
				if (e == null) break;
				list.Push(e);
			}
			foreach(E e in list)
				stack.Push(e);
		}

		private void mergeAt(Token t) {
			while(mergeToNull(t))
				/**/;
			popNull();
		}

        private bool mergeToNull(Token t) {
			bool somethingMerged = false;
			Stack<E> list = new Stack<E>();
			int cnt = 0;
			while(true) {
				if (stack.Count == 0) return false;
				if (currentE == null) break; // don't pop the null
				list.Push(stack.Pop());
				cnt++;
			}
			if (cnt == 0) return false; // nothing was pushed
			if (cnt == 1) {
				// we promote the one thing on the stack to the parent frame, i.e. swap:
				popNull();
				stack.Push(list.Pop());
				stack.Push(null);
				return false;
			}
			// merge as much as we can and push the results. Start with null
			E ret = null;
			int n = 0;
			foreach(E e in list) {
				n++;
				//System.Console.Write("{3}>> {1} of {2}   merge: {0}", e, n, cnt, stack.Count);
				if (ret == null) 
					ret = e;
				else {
					E merged = ret.add(e);
					if (merged != null) {
						somethingMerged = true;
						//mergeConflict(t, e, ret, "success", stack.Count);
						ret = merged;
					} else {
						//mergeConflict(t, e, ret, "conflict", stack.Count);
						push(ret);
						ret = e; 
					}                    
				}
				//System.Console.WriteLine(" -> ret={0}", ret);
			}
			push(ret);
			return somethingMerged;
        }

        public IDisposable createMarker(string literal, string name, bool islist, bool ishatch, bool primed) {
            return new Marker(this, literal, name, islist, ishatch, primed);
        }

        public IDisposable createBarrier() {
            return new Barrier(this);
        }

        private class Marker : IDisposable {
            public readonly Builder builder;
            public readonly string literal;
            public readonly string name;
            public readonly bool islist;
            public readonly bool ishatch;
            public readonly bool primed;

            public Marker(Builder builder, string literal, string name, bool islist, bool ishatch, bool primed) {
                this.builder = builder;                
                this.literal = literal;
                this.name = name;
                this.ishatch = ishatch;
                this.islist = islist;
                this.primed = primed;
				if (!ishatch)
                	builder.stack.Push(null); // push a marker
            }

            public void Dispose() {
				GC.SuppressFinalize(this);
                Token t = builder.parser.t;				
                if (ishatch) {
					if (primed) {
						try { 
							t = t.Copy(); builder.parser.Prime(t);
						} catch(Exception ex) {
							builder.parser.SemErr(string.Format("unexpected error in Prime(t): {0}", ex.Message));
						} 
					}
					builder.hatch(t, literal, name, islist);
				} else {
                	builder.sendup(t, literal, name, islist);
					builder.mergeAt(t);
				}
            }
        }

        private class Barrier : IDisposable {
            public readonly Builder builder;

            public Barrier(Builder builder) {
                this.builder = builder;             
				builder.stack.Push(null); // push a marker
            }

            public void Dispose() {
				GC.SuppressFinalize(this);
                Token t = builder.parser.t;
				builder.mergeAt(t);				
            }
        }
    }
}
