
using System;
using System.Collections;
using System.Collections.Generic;



public class Alternative {
	public readonly Token t;
	public BitArray alt;

	public Alternative(Token t, BitArray alt) {
		this.t = t;
		this.alt = alt;
	}
}

public class Symboltable {
	private List<string> list = new List<string>();
	public readonly string name;
	public readonly bool ignoreCase;

	public Symboltable(string name, bool ignoreCase) {
		this.name = name;
		this.ignoreCase = ignoreCase;
	}

	public bool Add(string s) {
		if (ignoreCase) s = s.ToLower();
		if (list.Contains(s))
			return false;
		list.Add(s);
		return true;
	}

	public bool Contains(string s) {
		if (ignoreCase) s = s.ToLower();
		return list.Contains(s);
	}

	public IEnumerable<string> Items() {
		return list;
	}
}

public class Parser {
	public const int _EOF = 0; // TOKEN EOF
	public const int _ident = 1; // TOKEN ident
	public const int _keyword = 2; // TOKEN keyword
	public const int _var = 3; // TOKEN var INHERITS ident
	public const int _var1 = 4; // TOKEN var1 INHERITS ident
	public const int _var2 = 5; // TOKEN var2 INHERITS ident
	public const int _var3 = 6; // TOKEN var3 INHERITS ident
	public const int _var4 = 7; // TOKEN var4 INHERITS ident
	public const int _var5 = 8; // TOKEN var5 INHERITS ident
	public const int _var6 = 9; // TOKEN var6 INHERITS ident
	public const int _as = 10; // TOKEN as INHERITS ident
	public const int _colon = 11; // TOKEN colon
	public const int maxT = 27;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;
	public List<Alternative> tokens = new List<Alternative>();
	public BitArray alt;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

	public readonly Symboltable variables = new Symboltable("variables", false);
	public readonly Symboltable types = new Symboltable("types", false);
	public Symboltable symbols(string name) {
		if (name == "variables") return variables;
		if (name == "types") return types;
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
			if (t.kind != _EOF) {
				tokens.Add(new Alternative(t, alt));
				alt = new BitArray(maxT);
			}
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}

	void addAlt(int kind) {
		alt[kind] = true;
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

	
	void Inheritance() {
		addAlt(14); // ITER start
		while (isKind(la, 14)) {
			Type();
			addAlt(14); // ITER end
		}
		addAlt(set0, 1); // ITER start
		while (StartOf(1)) {
			Declaration();
			addAlt(set0, 1); // ITER end
		}
		addAlt(12); // ITER start
		while (isKind(la, 12)) {
			Get();
			NumberIdent();
			Console.WriteLine("NumberIdent {0}", t.val); 
			addAlt(13); // T
			Expect(13); // ";"
			addAlt(12); // ITER end
		}
		addAlt(set0, 2); // ITER start
		while (StartOf(2)) {
			IdentOrNumber();
			addAlt(set0, 2); // ITER end
		}
	}

	void Type() {
		addAlt(14); // T
		Expect(14); // "type"
		if (!types.Add(la.val)) SemErr(string.Format("ident '{0}' declared twice in 'types'", la.val));
		addAlt(1); // T
		Expect(1); // ident
		addAlt(13); // T
		Expect(13); // ";"
	}

	void Declaration() {
		Var();
		Ident();
		addAlt(new int[] {15, 16}); // ITER start
		while (isKind(la, 15) || isKind(la, 16)) {
			Separator();
			Ident();
			addAlt(new int[] {15, 16}); // ITER end
		}
		while (!(isKind(la, 0) || isKind(la, 13))) {SynErr(28); Get();}
		addAlt(13); // T
		Expect(13); // ";"
	}

	void NumberIdent() {
		addAlt(17); // ALT
		addAlt(18); // ALT
		addAlt(19); // ALT
		addAlt(20); // ALT
		addAlt(21); // ALT
		addAlt(22); // ALT
		addAlt(23); // ALT
		addAlt(24); // ALT
		addAlt(25); // ALT
		addAlt(26); // ALT
		addAlt(1); // ALT
		switch (la.kind) {
		case 17: // "0"
		{
			Get();
			break;
		}
		case 18: // "1"
		{
			Get();
			break;
		}
		case 19: // "2"
		{
			Get();
			break;
		}
		case 20: // "3"
		{
			Get();
			break;
		}
		case 21: // "4"
		{
			Get();
			break;
		}
		case 22: // "5"
		{
			Get();
			break;
		}
		case 23: // "6"
		{
			Get();
			break;
		}
		case 24: // "7"
		{
			Get();
			break;
		}
		case 25: // "8"
		{
			Get();
			break;
		}
		case 26: // "9"
		{
			Get();
			break;
		}
		case 1: // ident
		case 3: // var
		case 4: // var1
		case 5: // var2
		case 6: // var3
		case 7: // var4
		case 8: // var5
		case 9: // var6
		case 10: // as
		{
			if (!variables.Contains(la.val)) SemErr(string.Format("ident '{0}' not declared in 'variables'", la.val));
			Get();
			break;
		}
		default: SynErr(29); break;
		}
	}

	void IdentOrNumber() {
		addAlt(1); // ALT
		addAlt(set0, 3); // ALT
		if (isKind(la, 1)) {
			Get();
		} else if (StartOf(3)) {
			NumberVar();
		} else SynErr(30);
	}

	void Var() {
		addAlt(3); // ALT
		addAlt(4); // ALT
		addAlt(5); // ALT
		addAlt(6); // ALT
		addAlt(7); // ALT
		addAlt(8); // ALT
		addAlt(9); // ALT
		switch (la.kind) {
		case 3: // var
		{
			Get();
			break;
		}
		case 4: // var1
		{
			Get();
			break;
		}
		case 5: // var2
		{
			Get();
			break;
		}
		case 6: // var3
		{
			Get();
			break;
		}
		case 7: // var4
		{
			Get();
			break;
		}
		case 8: // var5
		{
			Get();
			break;
		}
		case 9: // var6
		{
			Get();
			break;
		}
		default: SynErr(31); break;
		}
	}

	void Ident() {
		if (!variables.Add(la.val)) SemErr(string.Format("ident '{0}' declared twice in 'variables'", la.val));
		addAlt(1); // T
		Expect(1); // ident
		addAlt(new int[] {10, 11}); // OPT
		if (isKind(la, 10) || isKind(la, 11)) {
			addAlt(10); // ALT
			addAlt(11); // ALT
			if (isKind(la, 10)) {
				Get();
			} else {
				Get();
			}
			if (!types.Contains(la.val)) SemErr(string.Format("ident '{0}' not declared in 'types'", la.val));
			addAlt(1); // T
			Expect(1); // ident
		}
	}

	void Separator() {
		addAlt(15); // ALT
		addAlt(16); // ALT
		if (isKind(la, 15)) {
			addAlt(15); // weak T
			ExpectWeak(15, 4); // "," followed by var1
		} else if (isKind(la, 16)) {
			addAlt(16); // weak T
			ExpectWeak(16, 4); // "|" followed by var1
		} else SynErr(32);
	}

	void NumberVar() {
		addAlt(17); // ALT
		addAlt(18); // ALT
		addAlt(19); // ALT
		addAlt(20); // ALT
		addAlt(21); // ALT
		addAlt(22); // ALT
		addAlt(23); // ALT
		addAlt(24); // ALT
		addAlt(25); // ALT
		addAlt(26); // ALT
		addAlt(3); // ALT
		switch (la.kind) {
		case 17: // "0"
		{
			Get();
			break;
		}
		case 18: // "1"
		{
			Get();
			break;
		}
		case 19: // "2"
		{
			Get();
			break;
		}
		case 20: // "3"
		{
			Get();
			break;
		}
		case 21: // "4"
		{
			Get();
			break;
		}
		case 22: // "5"
		{
			Get();
			break;
		}
		case 23: // "6"
		{
			Get();
			break;
		}
		case 24: // "7"
		{
			Get();
			break;
		}
		case 25: // "8"
		{
			Get();
			break;
		}
		case 26: // "9"
		{
			Get();
			break;
		}
		case 3: // var
		{
			Get();
			break;
		}
		default: SynErr(33); break;
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";
		alt = new BitArray(maxT);		
		Get();
		types.Add("string");
		types.Add("int");
		types.Add("double");
		Inheritance();
		Expect(0);

	}
	
	// a token's base type
	static readonly int[] tBase = {
		-1,-1,-1, 1,  1, 1, 1, 1,  1, 1, 1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1,-1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","ident","keyword","var", "var1","var2","var3","var4", "var5","var6","as","colon", "\"NumberIdent\"","\";\"","\"type\"","\",\"", "\"|\"","\"0\"","\"1\"","\"2\"",
		"\"3\"","\"4\"","\"5\"","\"6\"", "\"7\"","\"8\"","\"9\"","???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x},
		{_T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x},
		{_T,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x}

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
			case 2: s = "keyword expected"; break;
			case 3: s = "var expected"; break;
			case 4: s = "var1 expected"; break;
			case 5: s = "var2 expected"; break;
			case 6: s = "var3 expected"; break;
			case 7: s = "var4 expected"; break;
			case 8: s = "var5 expected"; break;
			case 9: s = "var6 expected"; break;
			case 10: s = "as expected"; break;
			case 11: s = "colon expected"; break;
			case 12: s = "\"NumberIdent\" expected"; break;
			case 13: s = "\";\" expected"; break;
			case 14: s = "\"type\" expected"; break;
			case 15: s = "\",\" expected"; break;
			case 16: s = "\"|\" expected"; break;
			case 17: s = "\"0\" expected"; break;
			case 18: s = "\"1\" expected"; break;
			case 19: s = "\"2\" expected"; break;
			case 20: s = "\"3\" expected"; break;
			case 21: s = "\"4\" expected"; break;
			case 22: s = "\"5\" expected"; break;
			case 23: s = "\"6\" expected"; break;
			case 24: s = "\"7\" expected"; break;
			case 25: s = "\"8\" expected"; break;
			case 26: s = "\"9\" expected"; break;
			case 27: s = "??? expected"; break;
			case 28: s = "this symbol not expected in Declaration"; break;
			case 29: s = "invalid NumberIdent"; break;
			case 30: s = "invalid IdentOrNumber"; break;
			case 31: s = "invalid Var"; break;
			case 32: s = "invalid Separator"; break;
			case 33: s = "invalid NumberVar"; break;

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
