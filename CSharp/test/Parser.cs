
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;



public class Parser {
	public const int _EOF = 0; // TOKEN EOF
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
	public const int _as = 11; // TOKEN as INHERITS ident
	public const int _t = 12; // TOKEN t INHERITS ident
	public const int _v = 13; // TOKEN v INHERITS ident
	public const int _colon = 14; // TOKEN colon
	public const int maxT = 36;

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

	public readonly Symboltable variables;
	public readonly Symboltable types;
	public Symboltable symbols(string name) {
		if (name == "variables") return variables;
		if (name == "types") return types;
		return null;
	}

	public Token Prime() { return t; }



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
		astbuilder = new AST.Builder(this);
		variables = new Symboltable("variables", false, false, tokens);
		types = new Symboltable("types", false, true, tokens);
		types.Add("string");
		types.Add("int");
		types.Add("double");

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

	
	void Inheritance‿NT() {
		using(types.createUsageCheck(false, errors, la)) // 0..1
		using(types.createUsageCheck(true, errors, la)) // 1..N
		{
		TDBs‿NT();
		addAlt(15); // ITER start
		while (isKind(la, 15)) {
			Get();
			addAlt(16); // T
			Expect(16); // "("
			NumberIdent‿NT();
			addAlt(17); // T
			Expect(17); // ")"
			addAlt(18); // T
			Expect(18); // ";"
			addAlt(15); // ITER end
		}
		addAlt(22); // ITER start
		while (isKind(la, 22)) {
			using(astbuilder.createMarker(null, "call", true, false, false))
				Call‿NT();
			addAlt(22); // ITER end
		}
		addAlt(19); // ITER start
		while (isKind(la, 19)) {
			Get();
			addAlt(12); // ALT
			addAlt(13); // ALT
			if (isKind(la, 12)) {
				Get();
				if (!types.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, types.name));
				addAlt(2); // T
				addAlt(2, types); // T ident uses symbol table 'types'
				Expect(2); // ident
			} else if (isKind(la, 13)) {
				Get();
				if (!variables.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, variables.name));
				addAlt(2); // T
				addAlt(2, variables); // T ident uses symbol table 'variables'
				Expect(2); // ident
			} else SynErr(37);
			addAlt(18); // T
			Expect(18); // ";"
			addAlt(19); // ITER end
		}
		addAlt(set0, 1); // ITER start
		while (StartOf(1)) {
			IdentOrNumber‿NT();
			addAlt(set0, 1); // ITER end
		}
	}}

	void TDBs‿NT() {
		{
		addAlt(set0, 2); // ITER start
		while (StartOf(2)) {
			addAlt(24); // ALT
			addAlt(set0, 3); // ALT
			addAlt(20); // ALT
			addAlt(22); // ALT
			if (isKind(la, 24)) {
				Type‿NT();
			} else if (StartOf(3)) {
				Declaration‿NT();
			} else if (isKind(la, 20)) {
				Block‿NT();
			} else {
				using(astbuilder.createMarker(null, "tbdcall", true, false, false))
					Call‿NT();
			}
			addAlt(set0, 2); // ITER end
		}
	}}

	void NumberIdent‿NT() {
		{
		addAlt(26); // ALT
		addAlt(27); // ALT
		addAlt(28); // ALT
		addAlt(29); // ALT
		addAlt(30); // ALT
		addAlt(31); // ALT
		addAlt(32); // ALT
		addAlt(33); // ALT
		addAlt(34); // ALT
		addAlt(35); // ALT
		addAlt(2); // ALT
		addAlt(2, variables); // ALT ident uses symbol table 'variables'
		switch (la.kind) {
		case 26: // "0"
		{
			Get();
			break;
		}
		case 27: // "1"
		{
			Get();
			break;
		}
		case 28: // "2"
		{
			Get();
			break;
		}
		case 29: // "3"
		{
			Get();
			break;
		}
		case 30: // "4"
		{
			Get();
			break;
		}
		case 31: // "5"
		{
			Get();
			break;
		}
		case 32: // "6"
		{
			Get();
			break;
		}
		case 33: // "7"
		{
			Get();
			break;
		}
		case 34: // "8"
		{
			Get();
			break;
		}
		case 35: // "9"
		{
			Get();
			break;
		}
		case 2: // ident
		case 4: // var
		case 5: // var1
		case 6: // var2
		case 7: // var3
		case 8: // var4
		case 9: // var5
		case 10: // var6
		case 11: // as
		case 12: // t
		case 13: // v
		{
			if (!variables.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, variables.name));
			Get();
			break;
		}
		default: SynErr(38); break;
		}
	}}

	void Call‿NT() {
		{
		addAlt(22); // T
		Expect(22); // "call"
		addAlt(16); // T
		Expect(16); // "("
		using(astbuilder.createMarker(null, null, true, true, false))
			Param‿NT();
		addAlt(23); // ITER start
		while (isKind(la, 23)) {
			Get();
			using(astbuilder.createMarker(null, null, true, true, false))
				Param‿NT();
			addAlt(23); // ITER end
		}
		addAlt(17); // T
		Expect(17); // ")"
		addAlt(18); // T
		Expect(18); // ";"
	}}

	void IdentOrNumber‿NT() {
		{
		addAlt(2); // ALT
		addAlt(set0, 4); // ALT
		if (isKind(la, 2)) {
			Get();
		} else if (StartOf(4)) {
			NumberVar‿NT();
		} else SynErr(39);
	}}

	void Type‿NT() {
		{
		addAlt(24); // T
		Expect(24); // "type"
		if (!types.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "ident", la.val, types.name));
		alternatives.tdeclares = types;
		addAlt(2); // T
		Expect(2); // ident
		addAlt(18); // T
		Expect(18); // ";"
	}}

	void Declaration‿NT() {
		{
		Var‿NT();
		Ident‿NT();
		addAlt(new int[] {23, 25}); // ITER start
		while (isKind(la, 23) || isKind(la, 25)) {
			Separator‿NT();
			Ident‿NT();
			addAlt(new int[] {23, 25}); // ITER end
		}
		while (!(isKind(la, 0) || isKind(la, 18))) {SynErr(40); Get();}
		addAlt(18); // T
		Expect(18); // ";"
	}}

	void Block‿NT() {
		using(variables.createScope()) 
		using(types.createScope()) 
		{
		addAlt(20); // T
		Expect(20); // "{"
		TDBs‿NT();
		addAlt(21); // T
		Expect(21); // "}"
	}}

	void Param‿NT() {
		{
		addAlt(2); // ALT
		addAlt(2, variables); // ALT ident uses symbol table 'variables'
		addAlt(1); // ALT
		if (isKind(la, 2)) {
			if (!variables.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, variables.name));
			Get();
		} else if (isKind(la, 1)) {
			Get();
		} else SynErr(41);
	}}

	void Var‿NT() {
		{
		addAlt(4); // ALT
		addAlt(5); // ALT
		addAlt(6); // ALT
		addAlt(7); // ALT
		addAlt(8); // ALT
		addAlt(9); // ALT
		addAlt(10); // ALT
		switch (la.kind) {
		case 4: // var
		{
			Get();
			break;
		}
		case 5: // var1
		{
			Get();
			break;
		}
		case 6: // var2
		{
			Get();
			break;
		}
		case 7: // var3
		{
			Get();
			break;
		}
		case 8: // var4
		{
			Get();
			break;
		}
		case 9: // var5
		{
			Get();
			break;
		}
		case 10: // var6
		{
			Get();
			break;
		}
		default: SynErr(42); break;
		}
	}}

	void Ident‿NT() {
		{
		if (!variables.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "ident", la.val, variables.name));
		alternatives.tdeclares = variables;
		addAlt(2); // T
		Expect(2); // ident
		addAlt(new int[] {11, 14}); // OPT
		if (isKind(la, 11) || isKind(la, 14)) {
			addAlt(11); // ALT
			addAlt(14); // ALT
			if (isKind(la, 11)) {
				Get();
			} else {
				Get();
			}
			if (!types.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, types.name));
			addAlt(2); // T
			addAlt(2, types); // T ident uses symbol table 'types'
			Expect(2); // ident
		}
	}}

	void Separator‿NT() {
		{
		addAlt(23); // ALT
		addAlt(25); // ALT
		if (isKind(la, 23)) {
			addAlt(23); // WT
			ExpectWeak(23, 5); // "," followed by var1
		} else if (isKind(la, 25)) {
			addAlt(25); // WT
			ExpectWeak(25, 5); // "|" followed by var1
		} else SynErr(43);
	}}

	void NumberVar‿NT() {
		{
		addAlt(26); // ALT
		addAlt(27); // ALT
		addAlt(28); // ALT
		addAlt(29); // ALT
		addAlt(30); // ALT
		addAlt(31); // ALT
		addAlt(32); // ALT
		addAlt(33); // ALT
		addAlt(34); // ALT
		addAlt(35); // ALT
		addAlt(4); // ALT
		switch (la.kind) {
		case 26: // "0"
		{
			Get();
			break;
		}
		case 27: // "1"
		{
			Get();
			break;
		}
		case 28: // "2"
		{
			Get();
			break;
		}
		case 29: // "3"
		{
			Get();
			break;
		}
		case 30: // "4"
		{
			Get();
			break;
		}
		case 31: // "5"
		{
			Get();
			break;
		}
		case 32: // "6"
		{
			Get();
			break;
		}
		case 33: // "7"
		{
			Get();
			break;
		}
		case 34: // "8"
		{
			Get();
			break;
		}
		case 35: // "9"
		{
			Get();
			break;
		}
		case 4: // var
		{
			Get();
			break;
		}
		default: SynErr(44); break;
		}
	}}



	public void Parse() {
		la = new Token();
		la.val = "";
		Get();
		using(astbuilder.createMarker(null, null, false, false, false))
		Inheritance‿NT();
		Expect(0);
		variables.CheckDeclared(errors);
		types.CheckDeclared(errors);

		ast = astbuilder.current;
	}
	
	// a token's base type
	public static readonly int[] tBase = {
		-1,-1,-1,-1,  2, 2, 2, 2,  2, 2, 2, 2,  2, 2,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","number","ident","\"keyword\"", "\"var\"","\"var1\"","\"var2\"","\"var3\"", "\"var4\"","\"var5\"","\"var6\"","\"as\"", "\"t\"","\"v\"","\":\"","\"NumberIdent\"", "\"(\"","\")\"","\";\"","\"check\"",
		"\"{\"","\"}\"","\"call\"","\",\"", "\"type\"","\"|\"","\"0\"","\"1\"", "\"2\"","\"3\"","\"4\"","\"5\"", "\"6\"","\"7\"","\"8\"","\"9\"", "???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x},
		{_x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x},
		{_T,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x},
		{_x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x},
		{_T,_x,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x}

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
			case 1: s = "number expected"; break;
			case 2: s = "ident expected"; break;
			case 3: s = "keyword expected"; break;
			case 4: s = "var expected"; break;
			case 5: s = "var1 expected"; break;
			case 6: s = "var2 expected"; break;
			case 7: s = "var3 expected"; break;
			case 8: s = "var4 expected"; break;
			case 9: s = "var5 expected"; break;
			case 10: s = "var6 expected"; break;
			case 11: s = "as expected"; break;
			case 12: s = "t expected"; break;
			case 13: s = "v expected"; break;
			case 14: s = "colon expected"; break;
			case 15: s = "\"NumberIdent\" expected"; break;
			case 16: s = "\"(\" expected"; break;
			case 17: s = "\")\" expected"; break;
			case 18: s = "\";\" expected"; break;
			case 19: s = "\"check\" expected"; break;
			case 20: s = "\"{\" expected"; break;
			case 21: s = "\"}\" expected"; break;
			case 22: s = "\"call\" expected"; break;
			case 23: s = "\",\" expected"; break;
			case 24: s = "\"type\" expected"; break;
			case 25: s = "\"|\" expected"; break;
			case 26: s = "\"0\" expected"; break;
			case 27: s = "\"1\" expected"; break;
			case 28: s = "\"2\" expected"; break;
			case 29: s = "\"3\" expected"; break;
			case 30: s = "\"4\" expected"; break;
			case 31: s = "\"5\" expected"; break;
			case 32: s = "\"6\" expected"; break;
			case 33: s = "\"7\" expected"; break;
			case 34: s = "\"8\" expected"; break;
			case 35: s = "\"9\" expected"; break;
			case 36: s = "??? expected"; break;
			case 37: s = "invalid Inheritance"; break;
			case 38: s = "invalid NumberIdent"; break;
			case 39: s = "invalid IdentOrNumber"; break;
			case 40: s = "this symbol not expected in Declaration"; break;
			case 41: s = "invalid Param"; break;
			case 42: s = "invalid Var"; break;
			case 43: s = "invalid Separator"; break;
			case 44: s = "invalid NumberVar"; break;

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

        public ASTList(AST a) {
            if (a is ASTList)
                list = ((ASTList)a).list;
            else {
                list = new List<AST>();
                list.Add(a);
            }
        }

        public ASTList(AST a, int i) {
            list = new List<AST>();
            list.Add(a);
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
            if (longlist) AST.newline(indent, sb);
            int n = 0;
            foreach(AST ast in list) {
                ast.serialize(sb, indent + 1);
                n++;
                if (n < count) {
                    sb.Append(", ");
                    if (longlist) AST.newline(indent, sb);
                }
            }
            if (longlist) AST.newline(indent - 1, sb);
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
                ASTList list = new ASTList(ast);
                list.merge(e.ast);
                E ret = new E();
                ret.ast = list;
                ret.name = name;
                return ret;
            } else if (name != null && e.name != null) {
                ASTObject obj = new ASTObject();
                obj.add(this);
                obj.add(e);
                E ret = new E();
                ret.ast = obj;
                return ret;
            } else if (ast.merge(e))
                return this;
            return null;
        }

        public void wrapinlist() {
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
        public AST current { get { return currentE.ast; } }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach(E e in stack)
                sb.AppendFormat("{0}\n", e);
            return sb.ToString();
        }

        private void push(E e) {
            stack.Push(e);
            System.Console.WriteLine("-> push {0}, size {1}", e, stack.Count);
        }

        // that's what we call for #/##, built from an AstOp
        public void hatch(Token t, string literal, string name, bool islist) {
            System.Console.WriteLine(">> hatch token {0,-20} as {2,-10}, islist {3}, literal:{1}.", t.val, literal, name, islist);
            E e = new E();
            e.ast = new ASTLiteral(literal != null ? literal : t.val);
            if (islist)
                e.ast = new ASTList(e.ast);
            e.name = name;
            push(e);
        }

        // that's what we call for ^, built from an AstOp
        public void sendup(Token t, string literal, string name, bool islist) {
            E e = currentE;
            if (islist)
                System.Console.WriteLine(">> send up as [{0}]: {1}", name, e);
            else
                System.Console.WriteLine(">> send up as {0}: {1}", name, e);
            if (name != e.name) {
                if (islist)
                    e.wrapinlist(); 
                else 
                    parser.errors.Warning(t.line, t.col, string.Format("overwriting AST objectname '{0}' with '{1}'", e.name, name));
            }
            e.name = name;
            System.Console.WriteLine("-------------> top {0}", e);
        }

        private void mergeToNull() {
            Stack<E> list = new Stack<E>();
            int cnt = 0;
            while(true) {
                E e = stack.Pop();
                if (e == null) break;
                list.Push(e);
                cnt++;
            }
            if (cnt == 0) return; // nothing was pushed
            if (cnt == 1) {
                // we promote the one thing on the stack to the parent frame:
                push(list.Pop());
                return;
            }
            // merge as much as we can and push the results. Start with null
            E ret = null;
            int n = 0;
            foreach(E e in list) {
                n++;
                System.Console.Write(">> {1} of {2}   merge: {0}", e, n, cnt);
                if (ret == null) 
                    ret = e;
                else {
                    E merged = ret.add(e);
                    if (merged != null)
                        ret = merged;
                    else {
                        push(ret);
                        ret = e;
                    }
                }
                System.Console.WriteLine(" -> ret={0}", ret);
            }
            push(ret);
        }

        public IDisposable createMarker(string literal, string name, bool islist, bool ishatch, bool primed) {
            return new Marker(this, literal, name, islist, ishatch, primed);
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
                builder.stack.Push(null); // push a marker
            }

            public void Dispose() {
                Token t = primed ? builder.parser.Prime() : builder.parser.t;
                if (ishatch) builder.hatch(t, literal, name, islist);
                builder.mergeToNull();
                if (!ishatch) builder.sendup(t, literal, name, islist);
            }
        }
    }
}
