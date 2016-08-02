
using System;
using System.Collections;
using System.Collections.Generic;



public class Parser {
	public const int _EOF = 0; // TOKEN EOF
	public const int _dbcode = 1; // TOKEN dbcode
	public const int _twodigitnumber = 2; // TOKEN twodigitnumber
	public const int _string = 3; // TOKEN string
	public const int _domain = 4; // TOKEN domain
	public const int _end = 5; // TOKEN end
	public const int maxT = 11;

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

	public readonly Symboltable lang = new Symboltable("lang", false, true);
	public readonly Symboltable domains = new Symboltable("domains", false, true);
	public readonly Symboltable values = new Symboltable("values", false, true);
	public Symboltable symbols(string name) {
		if (name == "lang") return lang;
		if (name == "domains") return domains;
		if (name == "values") return values;
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

	
	void LanguageName‿NT() {
		addAlt(1); // ALT
		addAlt(3); // ALT
		if (isKind(la, 1)) {
			if (!lang.Add(la, tokens)) SemErr(string.Format(DuplicateSymbol, "dbcode", la.val, lang.name));
			alternatives.tdeclares = lang;
			Get();
		} else if (isKind(la, 3)) {
			if (!lang.Add(la, tokens)) SemErr(string.Format(DuplicateSymbol, "string", la.val, lang.name));
			alternatives.tdeclares = lang;
			Get();
		} else SynErr(12);
	}

	void DomainName‿NT() {
		addAlt(1); // ALT
		addAlt(3); // ALT
		if (isKind(la, 1)) {
			if (!domains.Add(la, tokens)) SemErr(string.Format(DuplicateSymbol, "dbcode", la.val, domains.name));
			alternatives.tdeclares = domains;
			Get();
		} else if (isKind(la, 3)) {
			if (!domains.Add(la, tokens)) SemErr(string.Format(DuplicateSymbol, "string", la.val, domains.name));
			alternatives.tdeclares = domains;
			Get();
		} else SynErr(13);
	}

	void ValueName‿NT() {
		addAlt(1); // ALT
		addAlt(3); // ALT
		if (isKind(la, 1)) {
			if (!values.Add(la, tokens)) SemErr(string.Format(DuplicateSymbol, "dbcode", la.val, values.name));
			alternatives.tdeclares = values;
			Get();
		} else if (isKind(la, 3)) {
			if (!values.Add(la, tokens)) SemErr(string.Format(DuplicateSymbol, "string", la.val, values.name));
			alternatives.tdeclares = values;
			Get();
		} else SynErr(14);
	}

	void UseLanguageName‿NT() {
		addAlt(1); // ALT
		addAlt(1, lang); // ALT dbcode uses symbol table 'lang'
		addAlt(3); // ALT
		addAlt(3, lang); // ALT string uses symbol table 'lang'
		if (isKind(la, 1)) {
			if (!lang.Use(la, alternatives)) SemErr(string.Format(MissingSymbol, "dbcode", la.val, lang.name));
			Get();
		} else if (isKind(la, 3)) {
			if (!lang.Use(la, alternatives)) SemErr(string.Format(MissingSymbol, "string", la.val, lang.name));
			Get();
		} else SynErr(15);
	}

	void CASDomains‿NT() {
		addAlt(6); // T
		Expect(6); // "casdomains"
		Languages‿NT();
		addAlt(4); // ITER start
		while (isKind(la, 4)) {
			Domain‿NT();
			addAlt(4); // ITER end
		}
	}

	void Languages‿NT() {
		addAlt(7); // T
		Expect(7); // "languages"
		LanguageName‿NT();
		addAlt(new int[] {1, 3}); // ITER start
		while (isKind(la, 1) || isKind(la, 3)) {
			LanguageName‿NT();
			addAlt(new int[] {1, 3}); // ITER end
		}
	}

	void Domain‿NT() {
		using(values.createScope()) {
		while (!(isKind(la, 0) || isKind(la, 4))) {SynErr(16); Get();}
		addAlt(4); // T
		Expect(4); // domain
		DomainName‿NT();
		addAlt(8); // OPT
		if (isKind(la, 8)) {
			Get();
		}
		addAlt(9); // OPT
		if (isKind(la, 9)) {
			Get();
			addAlt(2); // T
			Expect(2); // twodigitnumber
		}
		Translations‿NT();
		Domainvalue‿NT();
		addAlt(10); // ITER start
		while (isKind(la, 10)) {
			Domainvalue‿NT();
			addAlt(10); // ITER end
		}
		addAlt(5); // T
		Expect(5); // end
		addAlt(4); // T
		Expect(4); // domain
	}}

	void Translations‿NT() {
		addAlt(new int[] {1, 3}); // ITER start
		while (isKind(la, 1) || isKind(la, 3)) {
			UseLanguageName‿NT();
			addAlt(3); // T
			Expect(3); // string
			addAlt(new int[] {1, 3}); // ITER end
		}
	}

	void Domainvalue‿NT() {
		while (!(isKind(la, 0) || isKind(la, 10))) {SynErr(17); Get();}
		addAlt(10); // T
		Expect(10); // "value"
		ValueName‿NT();
		TranslationsWithHelptext‿NT();
	}

	void TranslationsWithHelptext‿NT() {
		addAlt(new int[] {1, 3}); // ITER start
		while (isKind(la, 1) || isKind(la, 3)) {
			UseLanguageName‿NT();
			addAlt(3); // T
			Expect(3); // string
			addAlt(3); // T
			Expect(3); // string
			addAlt(new int[] {1, 3}); // ITER end
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";
		Get();
		CASDomains‿NT();
		Expect(0);
		lang.CheckDeclared(errors);
		domains.CheckDeclared(errors);
		values.CheckDeclared(errors);

	}
	
	// a token's base type
	public static readonly int[] tBase = {
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","dbcode","twodigitnumber","string", "\"domain\"","\"end\"","\"casdomains\"","\"languages\"", "\"orfi\"","\"length\"","\"value\"","???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x}

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
			case 1: s = "dbcode expected"; break;
			case 2: s = "twodigitnumber expected"; break;
			case 3: s = "string expected"; break;
			case 4: s = "domain expected"; break;
			case 5: s = "end expected"; break;
			case 6: s = "\"casdomains\" expected"; break;
			case 7: s = "\"languages\" expected"; break;
			case 8: s = "\"orfi\" expected"; break;
			case 9: s = "\"length\" expected"; break;
			case 10: s = "\"value\" expected"; break;
			case 11: s = "??? expected"; break;
			case 12: s = "invalid LanguageName"; break;
			case 13: s = "invalid DomainName"; break;
			case 14: s = "invalid ValueName"; break;
			case 15: s = "invalid UseLanguageName"; break;
			case 16: s = "this symbol not expected in Domain"; break;
			case 17: s = "this symbol not expected in Domainvalue"; break;

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
	
	public bool Use(Token t, Alt a) {
		a.tdeclared = this;
		a.declaration = Find(t);
		if (a.declaration != null) return true; // it's ok, if we know the symbol
		if (strict) return false; // in strict mode we report an illegal symbol
		undeclaredTokens.Peek().Add(t); // in non strict mode we store the token for future checking
		return true; // we can't report an invalid symbol yet, so report "all ok".
	}

	public bool Add(Token t, IEnumerable<Alternative> fixuplist) {
		if (Find(currentScope, t) != null)
			return false;
		if (strict) clone = null; // if non strict, we have to keep the clone
		currentScope.Add(t);
		IEnumerable<Token> nowdeclareds = RemoveFromUndeclared(t);
		if (fixuplist != null)
			foreach(Token tok in nowdeclareds)
				foreach(Alternative a in fixuplist)
					if (a.t == tok)
						a.declaration = t;
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

	IEnumerable<Token> RemoveFromUndeclared(Token declaration) {
		StringComparer cmp = comparer;
		List<Token> found = new List<Token>();
		List<Token> undeclared = undeclaredTokens.Peek(); 
		foreach(Token t in undeclared)
			if (0 == cmp.Compare(t.val, declaration.val))
				found.Add(t);
		foreach(Token t in found)
			undeclared.Remove(t);
		return found;
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
					all.Add(t, null);
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
