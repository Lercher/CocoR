
using System;
using System.Collections;
using System.Collections.Generic;



public class Parser {
	public const int _EOF = 0; // TOKEN EOF
	public const int _databasecode = 1; // TOKEN databasecode
	public const int _languagecode = 2; // TOKEN languagecode
	public const int _twodigitnumber = 3; // TOKEN twodigitnumber
	public const int _string = 4; // TOKEN string
	public const int _orfi = 5; // TOKEN orfi
	public const int _language = 6; // TOKEN language
	public const int _domain = 7; // TOKEN domain
	public const int _values = 8; // TOKEN values
	public const int _length = 9; // TOKEN length
	public const int _end = 10; // TOKEN end
	public const int maxT = 16;

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

	
	void CassiopaeDB‿NT() {
		Languages‿NT();
		addAlt(7); // ITER start
		while (isKind(la, 7)) {
			Domain‿NT();
			addAlt(7); // ITER end
		}
	}

	void Languages‿NT() {
		addAlt(6); // T
		Expect(6); // language
		addAlt(11); // T
		Expect(11); // "("
		addAlt(2); // T
		Expect(2); // languagecode
		addAlt(12); // T
		Expect(12); // ","
		addAlt(2); // T
		Expect(2); // languagecode
		addAlt(12); // T
		Expect(12); // ","
		addAlt(2); // T
		Expect(2); // languagecode
		addAlt(13); // T
		Expect(13); // ")"
	}

	void Domain‿NT() {
		addAlt(7); // T
		Expect(7); // domain
		addAlt(5); // OPT
		if (isKind(la, 5)) {
			Get();
		}
		Databasecode‿NT();
		addAlt(9); // T
		Expect(9); // length
		addAlt(3); // T
		Expect(3); // twodigitnumber
		Translations‿NT();
		Domainvalues‿NT();
		addAlt(10); // T
		Expect(10); // end
		addAlt(7); // T
		Expect(7); // domain
	}

	void Databasecode‿NT() {
		addAlt(1); // ALT
		addAlt(4); // ALT
		if (isKind(la, 1)) {
			Get();
		} else if (isKind(la, 4)) {
			Get();
		} else SynErr(17);
	}

	void Translations‿NT() {
		addAlt(2); // T
		Expect(2); // languagecode
		addAlt(14); // T
		Expect(14); // ":"
		addAlt(4); // T
		Expect(4); // string
		addAlt(2); // T
		Expect(2); // languagecode
		addAlt(14); // T
		Expect(14); // ":"
		addAlt(4); // T
		Expect(4); // string
		addAlt(2); // T
		Expect(2); // languagecode
		addAlt(14); // T
		Expect(14); // ":"
		addAlt(4); // T
		Expect(4); // string
	}

	void Domainvalues‿NT() {
		addAlt(8); // T
		Expect(8); // values
		Domainvalue‿NT();
		addAlt(1); // ITER start
		while (isKind(la, 1)) {
			Domainvalue‿NT();
			addAlt(1); // ITER end
		}
	}

	void Domainvalue‿NT() {
		addAlt(1); // T
		Expect(1); // databasecode
		addAlt(15); // T
		Expect(15); // "="
		Domaintranslations‿NT();
	}

	void Domaintranslations‿NT() {
		addAlt(2); // T
		Expect(2); // languagecode
		addAlt(14); // T
		Expect(14); // ":"
		addAlt(4); // T
		Expect(4); // string
		addAlt(12); // T
		Expect(12); // ","
		addAlt(4); // T
		Expect(4); // string
		addAlt(2); // T
		Expect(2); // languagecode
		addAlt(14); // T
		Expect(14); // ":"
		addAlt(4); // T
		Expect(4); // string
		addAlt(12); // T
		Expect(12); // ","
		addAlt(4); // T
		Expect(4); // string
		addAlt(2); // T
		Expect(2); // languagecode
		addAlt(14); // T
		Expect(14); // ":"
		addAlt(4); // T
		Expect(4); // string
		addAlt(12); // T
		Expect(12); // ","
		addAlt(4); // T
		Expect(4); // string
	}



	public void Parse() {
		la = new Token();
		la.val = "";
		Get();
		CassiopaeDB‿NT();
		Expect(0);

	}
	
	// a token's base type
	public static readonly int[] tBase = {
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","databasecode","languagecode","twodigitnumber", "\"\"\"","\"orfi\"","\"languages\"","\"domain\"", "\"values\"","\"length\"","\"end\"","\"(\"", "\",\"","\")\"","\":\"","\"=\"", "???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x}

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
			case 1: s = "databasecode expected"; break;
			case 2: s = "languagecode expected"; break;
			case 3: s = "twodigitnumber expected"; break;
			case 4: s = "string expected"; break;
			case 5: s = "orfi expected"; break;
			case 6: s = "language expected"; break;
			case 7: s = "domain expected"; break;
			case 8: s = "values expected"; break;
			case 9: s = "length expected"; break;
			case 10: s = "end expected"; break;
			case 11: s = "\"(\" expected"; break;
			case 12: s = "\",\" expected"; break;
			case 13: s = "\")\" expected"; break;
			case 14: s = "\":\" expected"; break;
			case 15: s = "\"=\" expected"; break;
			case 16: s = "??? expected"; break;
			case 17: s = "invalid Databasecode"; break;

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
