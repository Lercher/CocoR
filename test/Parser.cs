
using System;



public class Parser {
	public const int _EOF = 0; // TOKEN EOF
	public const int _ident = 1; // TOKEN ident
	public const int _keyword = 2; // TOKEN keyword
	public const int _var = 3; // TOKEN var
	public const int _var1 = 4; // TOKEN var1
	public const int _var2 = 5; // TOKEN var2
	public const int _var3 = 6; // TOKEN var3
	public const int _var4 = 7; // TOKEN var4
	public const int _var5 = 8; // TOKEN var5
	public const int _var6 = 9; // TOKEN var6
	public const int _as = 10; // TOKEN as
	public const int _colon = 11; // TOKEN colon
	int[] tBase = {-1, -1, -1, 1, 1, 1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1};
	public const int maxT = 15;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

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
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
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
		while (StartOf(1)) {
			Declaration();
		}
	}

	void Declaration() {
		Var();
		Ident();
		while (isKind(la, 13) || isKind(la, 14)) {
			Separator();
			Ident();
		}
		while (!(isKind(la, 0) || isKind(la, 12))) {SynErr(16); Get();}
		Expect(12); // ";"
	}

	void Var() {
		if (isKind(la, 3)) {
			Get();
		} else if (isKind(la, 4)) {
			Get();
		} else if (isKind(la, 5)) {
			Get();
		} else if (isKind(la, 6)) {
			Get();
		} else if (isKind(la, 7)) {
			Get();
		} else if (isKind(la, 8)) {
			Get();
		} else if (isKind(la, 9)) {
			Get();
		} else SynErr(17);
	}

	void Ident() {
		Expect(1); // ident
		if (isKind(la, 10) || isKind(la, 11)) {
			if (isKind(la, 10)) {
				Get();
			} else {
				Get();
			}
			Expect(1); // ident
		}
	}

	void Separator() {
		if (isKind(la, 13)) {
			ExpectWeak(13, 2); // "," followed by keyword
		} else if (isKind(la, 14)) {
			ExpectWeak(14, 2); // "|" followed by keyword
		} else SynErr(18);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Inheritance();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x},
		{_x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x},
		{_T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x}

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
			case 12: s = "\";\" expected"; break;
			case 13: s = "\",\" expected"; break;
			case 14: s = "\"|\" expected"; break;
			case 15: s = "??? expected"; break;
			case 16: s = "this symbol not expected in Declaration"; break;
			case 17: s = "invalid Var"; break;
			case 18: s = "invalid Separator"; break;

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
