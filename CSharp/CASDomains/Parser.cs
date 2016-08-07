
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;



public class Parserbase {
	public virtual void Prime(Token t) { /* hook */ }
}

public class Parser : Parserbase {
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
	public readonly List<Alternative> tokens = new List<Alternative>();
	public AST ast;
	public readonly AST.Builder astbuilder; 
	
	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

	public readonly Symboltable lang;
	public readonly Symboltable langstring;
	public readonly Symboltable domains;
	public readonly Symboltable values;
	public Symboltable symbols(string name) {
		if (name == "lang") return lang;
		if (name == "langstring") return langstring;
		if (name == "domains") return domains;
		if (name == "values") return values;
		return null;
	}

public override void Prime(Token t) { 
		//if (t.kind == _string) 
		t.val = t.val.Substring(1, t.val.Length - 2);
	}




	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
		astbuilder = new AST.Builder(this);
		lang = new Symboltable("lang", false, true, tokens);
		langstring = new Symboltable("langstring", false, true, tokens);
		domains = new Symboltable("domains", false, true, tokens);
		values = new Symboltable("values", false, true, tokens);

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

	
	void LanguageName‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(1); // ALT
		addAlt(3); // ALT
		if (isKind(la, 1)) {
			if (!lang.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "dbcode", la.val, lang.name));
			alternatives.tdeclares = lang;
			using(astbuilder.createMarker(null, null, true, true, false))  Get();
		} else if (isKind(la, 3)) {
			if (!langstring.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "string", la.val, langstring.name));
			alternatives.tdeclares = langstring;
			using(astbuilder.createMarker(null, null, true, true, true))  Get();
		} else SynErr(12);
	}}

	void DomainName‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(1); // ALT
		addAlt(3); // ALT
		if (isKind(la, 1)) {
			if (!domains.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "dbcode", la.val, domains.name));
			alternatives.tdeclares = domains;
			using(astbuilder.createMarker(null, null, false, true, false))  Get();
		} else if (isKind(la, 3)) {
			if (!domains.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "string", la.val, domains.name));
			alternatives.tdeclares = domains;
			using(astbuilder.createMarker(null, null, false, true, true))  Get();
		} else SynErr(13);
	}}

	void ValueName‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(1); // ALT
		addAlt(3); // ALT
		if (isKind(la, 1)) {
			if (!values.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "dbcode", la.val, values.name));
			alternatives.tdeclares = values;
			using(astbuilder.createMarker(null, null, false, true, false))  Get();
		} else if (isKind(la, 3)) {
			if (!values.Add(la)) SemErr(la, string.Format(DuplicateSymbol, "string", la.val, values.name));
			alternatives.tdeclares = values;
			using(astbuilder.createMarker(null, null, false, true, true))  Get();
		} else SynErr(14);
	}}

	void UseLanguageName‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(1); // ALT
		addAlt(1, lang); // ALT dbcode uses symbol table 'lang'
		addAlt(3); // ALT
		addAlt(3, langstring); // ALT string uses symbol table 'langstring'
		if (isKind(la, 1)) {
			if (!lang.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "dbcode", la.val, lang.name));
			using(astbuilder.createMarker(null, null, false, true, false))  Get();
		} else if (isKind(la, 3)) {
			if (!langstring.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, langstring.name));
			using(astbuilder.createMarker(null, null, false, true, true))  Get();
		} else SynErr(15);
	}}

	void CASDomains‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(6); // T
		Expect(6); // "casdomains"
		using(astbuilder.createMarker(null, "languages", false, false, false))  Languages‿NT();
		addAlt(4); // ITER start
		while (isKind(la, 4)) {
			using(astbuilder.createMarker(null, "domains", true, false, false))  Domain‿NT();
			addAlt(4); // ITER end
		}
	}}

	void Languages‿NT() {
		using(astbuilder.createBarrier())
		{
		addAlt(7); // T
		Expect(7); // "languages"
		LanguageName‿NT();
		addAlt(new int[] {1, 3}); // ITER start
		while (isKind(la, 1) || isKind(la, 3)) {
			LanguageName‿NT();
			addAlt(new int[] {1, 3}); // ITER end
		}
	}}

	void Domain‿NT() {
		using(astbuilder.createBarrier())
		using(values.createScope()) 
		{
		while (!(isKind(la, 0) || isKind(la, 4))) {SynErr(16); Get();}
		addAlt(4); // T
		Expect(4); // domain
		using(astbuilder.createMarker(null, "domain", false, false, false))  DomainName‿NT();
		addAlt(8); // OPT
		if (isKind(la, 8)) {
			using(astbuilder.createMarker("t", "orfi", false, true, false))  Get();
		}
		addAlt(9); // OPT
		if (isKind(la, 9)) {
			Get();
			addAlt(2); // T
			using(astbuilder.createMarker(null, "length", false, true, false))  Expect(2); // twodigitnumber
		}
		using(astbuilder.createMarker(null, "translations", true, false, false))  Translations‿NT();
		using(astbuilder.createMarker(null, "values", true, false, false))  Domainvalue‿NT();
		addAlt(10); // ITER start
		while (isKind(la, 10)) {
			using(astbuilder.createMarker(null, "values", true, false, false))  Domainvalue‿NT();
			addAlt(10); // ITER end
		}
		addAlt(5); // T
		Expect(5); // end
		addAlt(4); // T
		Expect(4); // domain
	}}

	void Translations‿NT() {
		using(astbuilder.createBarrier())
		using(lang.createUsageCheck(false, errors, la)) // 0..1
		using(langstring.createUsageCheck(false, errors, la)) // 0..1
		using(lang.createUsageCheck(true, errors, la)) // 1..N
		using(langstring.createUsageCheck(true, errors, la)) // 1..N
		{
		addAlt(new int[] {1, 3}); // ITER start
		while (isKind(la, 1) || isKind(la, 3)) {
			using(astbuilder.createMarker(null, "lang", false, false, false))  UseLanguageName‿NT();
			addAlt(3); // T
			using(astbuilder.createMarker(null, "tl", false, true, true))  Expect(3); // string
			addAlt(new int[] {1, 3}); // ITER end
		}
	}}

	void Domainvalue‿NT() {
		using(astbuilder.createBarrier())
		{
		while (!(isKind(la, 0) || isKind(la, 10))) {SynErr(17); Get();}
		addAlt(10); // T
		Expect(10); // "value"
		using(astbuilder.createMarker(null, "value", false, false, false))  ValueName‿NT();
		using(astbuilder.createMarker(null, "translations", true, false, false))  TranslationsWithHelptext‿NT();
	}}

	void TranslationsWithHelptext‿NT() {
		using(astbuilder.createBarrier())
		using(lang.createUsageCheck(false, errors, la)) // 0..1
		using(langstring.createUsageCheck(false, errors, la)) // 0..1
		using(lang.createUsageCheck(true, errors, la)) // 1..N
		using(langstring.createUsageCheck(true, errors, la)) // 1..N
		{
		addAlt(new int[] {1, 3}); // ITER start
		while (isKind(la, 1) || isKind(la, 3)) {
			using(astbuilder.createMarker(null, "lang", false, false, false))  UseLanguageName‿NT();
			addAlt(3); // T
			using(astbuilder.createMarker(null, "tl", false, true, true))  Expect(3); // string
			addAlt(3); // T
			using(astbuilder.createMarker(null, "help", false, true, true))  Expect(3); // string
			addAlt(new int[] {1, 3}); // ITER end
		}
	}}



	public void Parse() {
		la = new Token();
		la.val = "";
		Get();
		CASDomains‿NT();
		Expect(0);
		lang.CheckDeclared(errors);
		langstring.CheckDeclared(errors);
		domains.CheckDeclared(errors);
		values.CheckDeclared(errors);

		ast = astbuilder.current;
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
