
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
	public const int maxT = 26;

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
		addAlt(new int[] {3, 4, 5, 6, 7, 8, 9});
		while (StartOf(1)) {
			Declaration();
		}
		addAlt(12);
		while (isKind(la, 12)) {
			Get();
			NumberIdent();
			Console.WriteLine("NumberIdent {0}", t.val); 
			addAlt(13);
			Expect(13); // ";"
		}
		addAlt(new int[] {1, 3, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25});
		while (StartOf(2)) {
			IdentOrNumber();
		}
	}

	void Declaration() {
		Var();
		Ident();
		addAlt(new int[] {14, 15});
		while (isKind(la, 14) || isKind(la, 15)) {
			Separator();
			Ident();
		}
		while (!(isKind(la, 0) || isKind(la, 13))) {SynErr(27); Get();}
		addAlt(13);
		Expect(13); // ";"
	}

	void NumberIdent() {
		addAlt(16);
		addAlt(17);
		addAlt(18);
		addAlt(19);
		addAlt(20);
		addAlt(21);
		addAlt(22);
		addAlt(23);
		addAlt(24);
		addAlt(25);
		addAlt(1);
		switch (la.kind) {
		case 16: // "0"
		{
			Get();
			break;
		}
		case 17: // "1"
		{
			Get();
			break;
		}
		case 18: // "2"
		{
			Get();
			break;
		}
		case 19: // "3"
		{
			Get();
			break;
		}
		case 20: // "4"
		{
			Get();
			break;
		}
		case 21: // "5"
		{
			Get();
			break;
		}
		case 22: // "6"
		{
			Get();
			break;
		}
		case 23: // "7"
		{
			Get();
			break;
		}
		case 24: // "8"
		{
			Get();
			break;
		}
		case 25: // "9"
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
			Get();
			break;
		}
		default: SynErr(28); break;
		}
	}

	void IdentOrNumber() {
		addAlt(1);
		addAlt(new int[] {3, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25});
				if (isKind(la, 1)) {
			Get();
		} else if (StartOf(3)) {
			NumberVar();
		} else SynErr(29);
	}

	void Var() {
		addAlt(3);
		addAlt(4);
		addAlt(5);
		addAlt(6);
		addAlt(7);
		addAlt(8);
		addAlt(9);
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
		default: SynErr(30); break;
		}
	}

	void Ident() {
		addAlt(1);
		Expect(1); // ident
		addAlt(new int[] {10, 11});
		if (isKind(la, 10) || isKind(la, 11)) {
			addAlt(10);
			addAlt(11);
						if (isKind(la, 10)) {
				Get();
			} else {
				Get();
			}
			addAlt(1);
			Expect(1); // ident
		}
	}

	void Separator() {
		addAlt(14);
		addAlt(15);
				if (isKind(la, 14)) {
			addAlt(14);
			ExpectWeak(14, 4); // "," followed by var1
		} else if (isKind(la, 15)) {
			addAlt(15);
			ExpectWeak(15, 4); // "|" followed by var1
		} else SynErr(31);
	}

	void NumberVar() {
		addAlt(16);
		addAlt(17);
		addAlt(18);
		addAlt(19);
		addAlt(20);
		addAlt(21);
		addAlt(22);
		addAlt(23);
		addAlt(24);
		addAlt(25);
		addAlt(3);
		switch (la.kind) {
		case 16: // "0"
		{
			Get();
			break;
		}
		case 17: // "1"
		{
			Get();
			break;
		}
		case 18: // "2"
		{
			Get();
			break;
		}
		case 19: // "3"
		{
			Get();
			break;
		}
		case 20: // "4"
		{
			Get();
			break;
		}
		case 21: // "5"
		{
			Get();
			break;
		}
		case 22: // "6"
		{
			Get();
			break;
		}
		case 23: // "7"
		{
			Get();
			break;
		}
		case 24: // "8"
		{
			Get();
			break;
		}
		case 25: // "9"
		{
			Get();
			break;
		}
		case 3: // var
		{
			Get();
			break;
		}
		default: SynErr(32); break;
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";
		alt = new BitArray(maxT);		
		Get();
		Inheritance();
		Expect(0);

	}
	
	// a token's base type
	static readonly int[] tBase = {
		-1,-1,-1, 1,  1, 1, 1, 1,  1, 1, 1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","ident","keyword","var", "var1","var2","var3","var4", "var5","var6","as","colon", "\"NumberIdent\"","\";\"","\",\"","\"|\"", "\"0\"","\"1\"","\"2\"","\"3\"",
		"\"4\"","\"5\"","\"6\"","\"7\"", "\"8\"","\"9\"","???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x},
		{_T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x},
		{_T,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x}

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
			case 14: s = "\",\" expected"; break;
			case 15: s = "\"|\" expected"; break;
			case 16: s = "\"0\" expected"; break;
			case 17: s = "\"1\" expected"; break;
			case 18: s = "\"2\" expected"; break;
			case 19: s = "\"3\" expected"; break;
			case 20: s = "\"4\" expected"; break;
			case 21: s = "\"5\" expected"; break;
			case 22: s = "\"6\" expected"; break;
			case 23: s = "\"7\" expected"; break;
			case 24: s = "\"8\" expected"; break;
			case 25: s = "\"9\" expected"; break;
			case 26: s = "??? expected"; break;
			case 27: s = "this symbol not expected in Declaration"; break;
			case 28: s = "invalid NumberIdent"; break;
			case 29: s = "invalid IdentOrNumber"; break;
			case 30: s = "invalid Var"; break;
			case 31: s = "invalid Separator"; break;
			case 32: s = "invalid NumberVar"; break;

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
