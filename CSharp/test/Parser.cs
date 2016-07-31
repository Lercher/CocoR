
using System;
using System.Collections;
using System.Collections.Generic;



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
	public const int maxT = 35;

	const bool _T = true;
	const bool _x = false;
	const string _DuplicateSymbol = "{0} '{1}' declared twice in '{2}'";
	const string _MissingSymbol ="{0} '{1}' not declared in '{2}'";
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;
	public List<Alternative> tokens = new List<Alternative>();
	
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

	
	void Inheritance() {
		TDBs();
		addAlt(12); // ITER start
		while (isKind(la, 12)) {
			Get();
			addAlt(13); // T
			Expect(13); // "("
			NumberIdent();
			addAlt(14); // T
			Expect(14); // ")"
			addAlt(15); // T
			Expect(15); // ";"
			addAlt(12); // ITER end
		}
		addAlt(21); // ITER start
		while (isKind(la, 21)) {
			Call();
			addAlt(21); // ITER end
		}
		addAlt(16); // ITER start
		while (isKind(la, 16)) {
			Get();
			addAlt(17); // ALT
			addAlt(18); // ALT
			if (isKind(la, 17)) {
				Get();
				if (!types.Contains(la.val)) SemErr(string.Format(_MissingSymbol, "ident", la.val, types.name));
				addAlt(1); // T
				addAlt(1, types); // T ident uses symbol table 'types'
				Expect(1); // ident
			} else if (isKind(la, 18)) {
				Get();
				if (!variables.Contains(la.val)) SemErr(string.Format(_MissingSymbol, "ident", la.val, variables.name));
				addAlt(1); // T
				addAlt(1, variables); // T ident uses symbol table 'variables'
				Expect(1); // ident
			} else SynErr(36);
			addAlt(15); // T
			Expect(15); // ";"
			addAlt(16); // ITER end
		}
		addAlt(set0, 1); // ITER start
		while (StartOf(1)) {
			IdentOrNumber();
			addAlt(set0, 1); // ITER end
		}
	}

	void TDBs() {
		addAlt(set0, 2); // ITER start
		while (StartOf(2)) {
			addAlt(23); // ALT
			addAlt(set0, 3); // ALT
			addAlt(19); // ALT
			addAlt(21); // ALT
			if (isKind(la, 23)) {
				Type();
			} else if (StartOf(3)) {
				Declaration();
			} else if (isKind(la, 19)) {
				Block();
			} else {
				Call();
			}
			addAlt(set0, 2); // ITER end
		}
	}

	void NumberIdent() {
		addAlt(25); // ALT
		addAlt(26); // ALT
		addAlt(27); // ALT
		addAlt(28); // ALT
		addAlt(29); // ALT
		addAlt(30); // ALT
		addAlt(31); // ALT
		addAlt(32); // ALT
		addAlt(33); // ALT
		addAlt(34); // ALT
		addAlt(1); // ALT
		addAlt(1, variables); // ALT ident uses symbol table 'variables'
		switch (la.kind) {
		case 25: // "0"
		{
			Get();
			break;
		}
		case 26: // "1"
		{
			Get();
			break;
		}
		case 27: // "2"
		{
			Get();
			break;
		}
		case 28: // "3"
		{
			Get();
			break;
		}
		case 29: // "4"
		{
			Get();
			break;
		}
		case 30: // "5"
		{
			Get();
			break;
		}
		case 31: // "6"
		{
			Get();
			break;
		}
		case 32: // "7"
		{
			Get();
			break;
		}
		case 33: // "8"
		{
			Get();
			break;
		}
		case 34: // "9"
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
			if (!variables.Contains(la.val)) SemErr(string.Format(_MissingSymbol, "ident", la.val, variables.name));
			Get();
			break;
		}
		default: SynErr(37); break;
		}
	}

	void Call() {
		addAlt(21); // T
		Expect(21); // "call"
		addAlt(13); // T
		Expect(13); // "("
		if (!variables.Contains(la.val)) SemErr(string.Format(_MissingSymbol, "ident", la.val, variables.name));
		addAlt(1); // T
		addAlt(1, variables); // T ident uses symbol table 'variables'
		Expect(1); // ident
		addAlt(22); // ITER start
		while (isKind(la, 22)) {
			Get();
			if (!variables.Contains(la.val)) SemErr(string.Format(_MissingSymbol, "ident", la.val, variables.name));
			addAlt(1); // T
			addAlt(1, variables); // T ident uses symbol table 'variables'
			Expect(1); // ident
			addAlt(22); // ITER end
		}
		addAlt(14); // T
		Expect(14); // ")"
		addAlt(15); // T
		Expect(15); // ";"
	}

	void IdentOrNumber() {
		addAlt(1); // ALT
		addAlt(set0, 4); // ALT
		if (isKind(la, 1)) {
			Get();
		} else if (StartOf(4)) {
			NumberVar();
		} else SynErr(38);
	}

	void Type() {
		addAlt(23); // T
		Expect(23); // "type"
		if (!types.Add(la.val)) SemErr(string.Format(_DuplicateSymbol, "ident", la.val, types.name));
		addAlt(1); // T
		Expect(1); // ident
		addAlt(15); // T
		Expect(15); // ";"
	}

	void Declaration() {
		Var();
		Ident();
		addAlt(new int[] {22, 24}); // ITER start
		while (isKind(la, 22) || isKind(la, 24)) {
			Separator();
			Ident();
			addAlt(new int[] {22, 24}); // ITER end
		}
		while (!(isKind(la, 0) || isKind(la, 15))) {SynErr(39); Get();}
		addAlt(15); // T
		Expect(15); // ";"
	}

	void Block() {
		using(variables.createScope()) using(types.createScope()) {
		addAlt(19); // T
		Expect(19); // "{"
		TDBs();
		addAlt(20); // T
		Expect(20); // "}"
	}}

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
		default: SynErr(40); break;
		}
	}

	void Ident() {
		if (!variables.Add(la.val)) SemErr(string.Format(_DuplicateSymbol, "ident", la.val, variables.name));
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
			if (!types.Contains(la.val)) SemErr(string.Format(_MissingSymbol, "ident", la.val, types.name));
			addAlt(1); // T
			addAlt(1, types); // T ident uses symbol table 'types'
			Expect(1); // ident
		}
	}

	void Separator() {
		addAlt(22); // ALT
		addAlt(24); // ALT
		if (isKind(la, 22)) {
			addAlt(22); // WT
			ExpectWeak(22, 5); // "," followed by var2
		} else if (isKind(la, 24)) {
			addAlt(24); // WT
			ExpectWeak(24, 5); // "|" followed by var2
		} else SynErr(41);
	}

	void NumberVar() {
		addAlt(25); // ALT
		addAlt(26); // ALT
		addAlt(27); // ALT
		addAlt(28); // ALT
		addAlt(29); // ALT
		addAlt(30); // ALT
		addAlt(31); // ALT
		addAlt(32); // ALT
		addAlt(33); // ALT
		addAlt(34); // ALT
		addAlt(3); // ALT
		switch (la.kind) {
		case 25: // "0"
		{
			Get();
			break;
		}
		case 26: // "1"
		{
			Get();
			break;
		}
		case 27: // "2"
		{
			Get();
			break;
		}
		case 28: // "3"
		{
			Get();
			break;
		}
		case 29: // "4"
		{
			Get();
			break;
		}
		case 30: // "5"
		{
			Get();
			break;
		}
		case 31: // "6"
		{
			Get();
			break;
		}
		case 32: // "7"
		{
			Get();
			break;
		}
		case 33: // "8"
		{
			Get();
			break;
		}
		case 34: // "9"
		{
			Get();
			break;
		}
		case 3: // var
		{
			Get();
			break;
		}
		default: SynErr(42); break;
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";
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
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","ident","\"keyword\"","\"var\"", "\"var1\"","\"var2\"","\"var3\"","\"var4\"", "\"var5\"","\"var6\"","\"as\"","\":\"", "\"NumberIdent\"","\"(\"","\")\"","\";\"", "\"check\"","\"t\"","\"v\"","\"{\"",
		"\"}\"","\"call\"","\",\"","\"type\"", "\"|\"","\"0\"","\"1\"","\"2\"", "\"3\"","\"4\"","\"5\"","\"6\"", "\"7\"","\"8\"","\"9\"","???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x},
		{_T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x},
		{_T,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x}

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
			case 13: s = "\"(\" expected"; break;
			case 14: s = "\")\" expected"; break;
			case 15: s = "\";\" expected"; break;
			case 16: s = "\"check\" expected"; break;
			case 17: s = "\"t\" expected"; break;
			case 18: s = "\"v\" expected"; break;
			case 19: s = "\"{\" expected"; break;
			case 20: s = "\"}\" expected"; break;
			case 21: s = "\"call\" expected"; break;
			case 22: s = "\",\" expected"; break;
			case 23: s = "\"type\" expected"; break;
			case 24: s = "\"|\" expected"; break;
			case 25: s = "\"0\" expected"; break;
			case 26: s = "\"1\" expected"; break;
			case 27: s = "\"2\" expected"; break;
			case 28: s = "\"3\" expected"; break;
			case 29: s = "\"4\" expected"; break;
			case 30: s = "\"5\" expected"; break;
			case 31: s = "\"6\" expected"; break;
			case 32: s = "\"7\" expected"; break;
			case 33: s = "\"8\" expected"; break;
			case 34: s = "\"9\" expected"; break;
			case 35: s = "??? expected"; break;
			case 36: s = "invalid Inheritance"; break;
			case 37: s = "invalid NumberIdent"; break;
			case 38: s = "invalid IdentOrNumber"; break;
			case 39: s = "this symbol not expected in Declaration"; break;
			case 40: s = "invalid Var"; break;
			case 41: s = "invalid Separator"; break;
			case 42: s = "invalid NumberVar"; break;

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
}

public class Symboltable {
	private Stack<List<string>> scopes;
	public readonly string name;
	public readonly bool ignoreCase;
	private Symboltable clone = null;

	public Symboltable(string name, bool ignoreCase) {
		this.name = name;
		this.ignoreCase = ignoreCase;
		this.scopes = new Stack<List<string>>();
		pushNewScope();
	}

	private Symboltable(Symboltable st) {
		this.name = st.name;
		this.ignoreCase = st.ignoreCase;

		// now copy the scopes and its lists
		this.scopes = new Stack<List<string>>();				 		
		Stack<List<string>> reverse = new Stack<List<string>>(st.scopes);
		foreach(List<string> list in reverse) {
			this.scopes.Push(new List<string>(list));
		}
	}

	// We can keep the clone until we push/pop of the stack, or add a new item. 
	public Symboltable CloneScopes() {
		if (clone != null) return clone;
		clone = new Symboltable(this); // i.e. copy scopes
		return clone;
	}

	public bool Add(string s) {
		if (ignoreCase) s = s.ToLower();
		if (currentScope.Contains(s))
			return false;
		clone = null;
		currentScope.Add(s);
		return true;
	}

	void pushNewScope() {
		clone = null;
		scopes.Push(new List<string>());
	}

	void popScope() {
		clone = null;
		scopes.Pop();
	}

	public IDisposable createScope() {
		pushNewScope();
		return new Popper(this);
	} 

	public List<string> currentScope {
		get { return scopes.Peek(); } 
	}

	public bool Contains(string s) {
		if (ignoreCase) s = s.ToLower();
		foreach(List<string> list in scopes)
			if (list.Contains(s)) return true;
		return false;
	}

	public IEnumerable<string> items {
		get {
		    if (scopes.Count == 1) return currentScope;

			Symboltable all = new Symboltable(name, ignoreCase);
			foreach(List<string> list in scopes)
				foreach(string s in list)
					all.Add(s);
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
