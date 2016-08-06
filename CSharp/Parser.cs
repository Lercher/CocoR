using System.IO;



using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace at.jku.ssw.Coco {



public class Parser {
	public const int _EOF = 0; // TOKEN EOF
	public const int _ident = 1; // TOKEN ident
	public const int _number = 2; // TOKEN number
	public const int _string = 3; // TOKEN string
	public const int _badString = 4; // TOKEN badString
	public const int _char = 5; // TOKEN char
	public const int _prime = 6; // TOKEN prime
	public const int maxT = 51;
	public const int _ddtSym = 52;
	public const int _optionSym = 53;

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

	public Symboltable symbols(string name) {
		return null;
	}

const int id = 0;
	const int str = 1;
	
	public TextWriter trace;    // other Coco objects referenced in this ATG
	public Tab tab;
	public DFA dfa;
	public ParserGen pgen;

	bool   genScanner;
	string tokenString;         // used in declarations of literal tokens
	string noString = "-none-"; // used in declarations of literal tokens

  public void Prime(Token t) { }

/*-------------------------------------------------------------------------*/



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
		astbuilder = new AST.Builder(this);

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

			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }
				if (la.kind == 52) {
				tab.SetDDT(la.val); 
				}
				if (la.kind == 53) {
				tab.SetOption(la.val); 
				}

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

	
	void Coco‿NT() {
		{
		Symbol sym; Graph g, g1, g2; string gramName; CharSet s; int beg, line; 
		if (StartOf(1)) {
			Get();
			beg = t.pos; line = t.line; 
			while (StartOf(1)) {
				Get();
							}
			pgen.usingPos = new Position(beg, la.pos, 0, line); 
		}
		Expect(7); // "COMPILER"
		genScanner = true; 
		tab.ignored = new CharSet(); 
		Expect(1); // ident
		gramName = t.val;
		beg = la.pos; line = la.line;
		
		while (StartOf(2)) {
			Get();
					}
		tab.semDeclPos = new Position(beg, la.pos, 0, line); 
		if (isKind(la, 8)) {
			Get();
			dfa.ignoreCase = true; 
		}
		if (isKind(la, 9)) {
			Get();
			while (isKind(la, 1)) {
				SetDecl‿NT();
							}
		}
		if (isKind(la, 10)) {
			Get();
			while (isKind(la, 1) || isKind(la, 3) || isKind(la, 5)) {
				TokenDecl‿NT(Node.t);
							}
		}
		if (isKind(la, 11)) {
			Get();
			while (isKind(la, 1) || isKind(la, 3) || isKind(la, 5)) {
				TokenDecl‿NT(Node.pr);
							}
		}
		while (isKind(la, 12)) {
			Get();
			bool nested = false; 
			Expect(13); // "FROM"
			TokenExpr‿NT(out g1);
			Expect(14); // "TO"
			TokenExpr‿NT(out g2);
			if (isKind(la, 15)) {
				Get();
				nested = true; 
			}
			dfa.NewComment(g1.l, g2.l, nested); 
					}
		while (isKind(la, 16)) {
			Get();
			Set‿NT(out s);
			tab.ignored.Or(s); 
					}
		if (isKind(la, 17)) {
			Get();
			while (isKind(la, 1)) {
				SymboltableDecl‿NT();
							}
		}
		while (!(isKind(la, 0) || isKind(la, 18))) {SynErr(52); Get();}
		Expect(18); // "PRODUCTIONS"
		if (genScanner) dfa.MakeDeterministic();
		tab.DeleteNodes();
		
		while (isKind(la, 1)) {
			Get();
			sym = tab.FindSym(t.val);
			bool undef = sym == null;
			if (undef) sym = tab.NewSym(Node.nt, t.val, t.line);
			else {
			 if (sym.typ == Node.nt) {
			   if (sym.graph != null) SemErr("name declared twice");
			 } else SemErr("this symbol kind not allowed on left side of production");
			 sym.line = t.line;
			}
			bool noAttrs = sym.attrPos == null;
			sym.attrPos = null;
			
			if (isKind(la, 34) || isKind(la, 36)) {
				AttrDecl‿NT(sym);
			}
			if (!undef)
			 if (noAttrs != (sym.attrPos == null))
			   SemErr("attribute mismatch between declaration and use of this symbol");
			
			if (isKind(la, 23)) {
				ScopesDecl‿NT(sym);
			}
			if (isKind(la, 27)) {
				UseOnceDecl‿NT(sym);
			}
			if (isKind(la, 28)) {
				UseAllDecl‿NT(sym);
			}
			if (isKind(la, 49)) {
				SemText‿NT(out sym.semPos);
			}
			ExpectWeak(19, 3); // "=" followed by string
			Expression‿NT(out g);
			sym.graph = g.l;
			tab.Finish(g);
			
			ExpectWeak(20, 4); // "." followed by badString
					}
		Expect(21); // "END"
		Expect(1); // ident
		if (gramName != t.val)
		 SemErr("name does not match grammar name");
		tab.gramSy = tab.FindSym(gramName);
		if (tab.gramSy == null)
		 SemErr("missing production for grammar name");
		else {
		 sym = tab.gramSy;
		 if (sym.attrPos != null)
		   SemErr("grammar symbol must not have attributes");
		}
		tab.noSym = tab.NewSym(Node.t, "???", 0); // noSym gets highest number
		tab.SetupAnys();
		tab.RenumberPragmas();
		if (tab.ddt[2]) tab.PrintNodes();
		if (errors.count == 0) {
		 Console.WriteLine("checking");
		 tab.CompSymbolSets();
		 if (tab.ddt[7]) tab.XRef();
		 if (tab.GrammarOk()) {
		   Console.Write("parser");
		   pgen.WriteParser();
		   if (genScanner) {
		     Console.Write(" + scanner");
		     dfa.WriteScanner();
		     if (tab.ddt[0]) dfa.PrintStates();
		   }
		   Console.WriteLine(" generated");
		   if (tab.ddt[8]) pgen.WriteStatistics();
		 }
		}
		if (tab.ddt[6]) tab.PrintSymbolTable();
		
		Expect(20); // "."
	}}

	void SetDecl‿NT() {
		{
		CharSet s; 
		Expect(1); // ident
		string name = t.val;
		CharClass c = tab.FindCharClass(name);
		if (c != null) SemErr("name declared twice");
		
		Expect(19); // "="
		Set‿NT(out s);
		if (s.Elements() == 0) SemErr("character set must not be empty");
		tab.NewCharClass(name, s);
		
		Expect(20); // "."
	}}

	void TokenDecl‿NT(int typ) {
		{
		string name; int kind; Symbol sym; Graph g; 
		string inheritsName; int inheritsKind; Symbol inheritsSym; 
		
		Sym‿NT(out name, out kind);
		sym = tab.FindSym(name);
		if (sym != null) SemErr("name declared twice");
		else {
		 sym = tab.NewSym(typ, name, t.line);
		 sym.tokenKind = Symbol.fixedToken;
		}
		tokenString = null;
		
		if (isKind(la, 33)) {
			Get();
			Sym‿NT(out inheritsName, out inheritsKind);
			inheritsSym = tab.FindSym(inheritsName);
			if (inheritsSym == null) SemErr(string.Format("token '{0}' can't inherit from '{1}', name not declared", sym.name, inheritsName));
			else if (inheritsSym == sym) SemErr(string.Format("token '{0}' must not inherit from self", sym.name));
			else if (inheritsSym.typ != typ) SemErr(string.Format("token '{0}' can't inherit from '{1}'", sym.name, inheritsSym.name));
			else sym.inherits = inheritsSym;
			
		}
		while (!(StartOf(5))) {SynErr(53); Get();}
		if (isKind(la, 19)) {
			Get();
			TokenExpr‿NT(out g);
			Expect(20); // "."
			if (kind == str) SemErr("a literal must not be declared with a structure");
			tab.Finish(g);
			if (tokenString == null || tokenString.Equals(noString))
			 dfa.ConvertToStates(g.l, sym);
			else { // TokenExpr is a single string
			 if (tab.literals[tokenString] != null)
			   SemErr("token string declared twice");
			 tab.literals[tokenString] = sym;
			 dfa.MatchLiteral(tokenString, sym);
			 sym.definedAs = tokenString;
			}
			
		} else if (StartOf(6)) {
			if (kind == id) genScanner = false;
			else dfa.MatchLiteral(sym.name, sym);
			
		} else SynErr(54);
		if (isKind(la, 49)) {
			SemText‿NT(out sym.semPos);
			if (typ != Node.pr) SemErr("semantic action not allowed in a pragma context"); 
		}
	}}

	void TokenExpr‿NT(out Graph g) {
		{
		Graph g2; 
		TokenTerm‿NT(out g);
		bool first = true; 
		while (WeakSeparator(38,7,8) ) {
			TokenTerm‿NT(out g2);
			if (first) { tab.MakeFirstAlt(g); first = false; }
			tab.MakeAlternative(g, g2);
			
					}
	}}

	void Set‿NT(out CharSet s) {
		{
		CharSet s2; 
		SimSet‿NT(out s);
		while (isKind(la, 29) || isKind(la, 30)) {
			if (isKind(la, 29)) {
				Get();
				SimSet‿NT(out s2);
				s.Or(s2); 
			} else {
				Get();
				SimSet‿NT(out s2);
				s.Subtract(s2); 
			}
					}
	}}

	void SymboltableDecl‿NT() {
		{
		SymTab st; 
		Expect(1); // ident
		string name = t.val.ToLower();                                    
		if (tab.FindSymtab(name) != null) 
		 SemErr("symbol table name declared twice");
		st = new SymTab(name);
		tab.symtabs.Add(st);
		
		if (isKind(la, 22)) {
			Get();
			st.strict = true; 
		}
		while (isKind(la, 3)) {
			Get();
			string predef = t.val;
			predef = tab.Unescape(predef.Substring(1, predef.Length-2));
			if (dfa.ignoreCase) predef = predef.ToLower();
			st.Add(predef);
			
					}
		Expect(20); // "."
	}}

	void AttrDecl‿NT(Symbol sym) {
		{
		if (isKind(la, 34)) {
			Get();
			int beg = la.pos; int col = la.col; int line = la.line; 
			while (StartOf(9)) {
				if (StartOf(10)) {
					Get();
				} else {
					Get();
					SemErr("bad string in attributes"); 
				}
							}
			Expect(35); // ">"
			if (t.pos > beg)
			 sym.attrPos = new Position(beg, t.pos, col, line); 
		} else if (isKind(la, 36)) {
			Get();
			int beg = la.pos; int col = la.col; int line = la.line; 
			while (StartOf(11)) {
				if (StartOf(12)) {
					Get();
				} else {
					Get();
					SemErr("bad string in attributes"); 
				}
							}
			Expect(37); // ".>"
			if (t.pos > beg)
			 sym.attrPos = new Position(beg, t.pos, col, line); 
		} else SynErr(55);
	}}

	void ScopesDecl‿NT(Symbol sym) {
		{
		sym.scopes = new List<SymTab>(); 
		Expect(23); // "SCOPES"
		Expect(24); // "("
		Symboltable‿NT(sym.scopes);
		while (isKind(la, 25)) {
			Get();
			Symboltable‿NT(sym.scopes);
					}
		Expect(26); // ")"
	}}

	void UseOnceDecl‿NT(Symbol sym) {
		{
		sym.useonces = new List<SymTab>(); 
		Expect(27); // "USEONCE"
		Expect(24); // "("
		Symboltable‿NT(sym.useonces);
		while (isKind(la, 25)) {
			Get();
			Symboltable‿NT(sym.useonces);
					}
		Expect(26); // ")"
	}}

	void UseAllDecl‿NT(Symbol sym) {
		{
		sym.usealls = new List<SymTab>(); 
		Expect(28); // "USEALL"
		Expect(24); // "("
		Symboltable‿NT(sym.usealls);
		while (isKind(la, 25)) {
			Get();
			Symboltable‿NT(sym.usealls);
					}
		Expect(26); // ")"
	}}

	void SemText‿NT(out Position pos) {
		{
		Expect(49); // "(."
		int beg = la.pos; int col = la.col; int line = la.line; 
		while (StartOf(13)) {
			if (StartOf(14)) {
				Get();
			} else if (isKind(la, 4)) {
				Get();
				SemErr("bad string in semantic action"); 
			} else {
				Get();
				SemErr("missing end of previous semantic action"); 
			}
					}
		Expect(50); // ".)"
		pos = new Position(beg, t.pos, col, line); 
	}}

	void Expression‿NT(out Graph g) {
		{
		Graph g2; 
		Term‿NT(out g);
		bool first = true; 
		while (WeakSeparator(38,15,16) ) {
			Term‿NT(out g2);
			if (first) { tab.MakeFirstAlt(g); first = false; }
			tab.MakeAlternative(g, g2);
			
					}
	}}

	void Symboltable‿NT(List<SymTab> sts ) {
		{
		Expect(1); // ident
		string stname = t.val.ToLower();
		SymTab st = tab.FindSymtab(stname); 
		if (st == null) SemErr("undeclared symbol table " + t.val);
		else sts.Add(st);
		
	}}

	void SimSet‿NT(out CharSet s) {
		{
		int n1, n2; 
		s = new CharSet(); 
		if (isKind(la, 1)) {
			Get();
			CharClass c = tab.FindCharClass(t.val);
			if (c == null) SemErr("undefined name"); else s.Or(c.set);
			
		} else if (isKind(la, 3)) {
			Get();
			string name = t.val;
			name = tab.Unescape(name.Substring(1, name.Length-2));
			foreach (char ch in name)
			 if (dfa.ignoreCase) s.Set(char.ToLower(ch));
			 else s.Set(ch); 
		} else if (isKind(la, 5)) {
			Char‿NT(out n1);
			s.Set(n1); 
			if (isKind(la, 31)) {
				Get();
				Char‿NT(out n2);
				for (int i = n1; i <= n2; i++) s.Set(i); 
			}
		} else if (isKind(la, 32)) {
			Get();
			s = new CharSet(); s.Fill(); 
		} else SynErr(56);
	}}

	void Char‿NT(out int n) {
		{
		Expect(5); // char
		string name = t.val; n = 0;
		name = tab.Unescape(name.Substring(1, name.Length-2));
		if (name.Length == 1) n = name[0];
		else SemErr("unacceptable character value");
		if (dfa.ignoreCase && (char)n >= 'A' && (char)n <= 'Z') n += 32;
		
	}}

	void Sym‿NT(out string name, out int kind) {
		{
		name = "???"; kind = id; 
		if (isKind(la, 1)) {
			Get();
			kind = id; name = t.val; 
		} else if (isKind(la, 3) || isKind(la, 5)) {
			if (isKind(la, 3)) {
				Get();
				name = t.val; 
			} else {
				Get();
				name = "\"" + t.val.Substring(1, t.val.Length-2) + "\""; 
			}
			kind = str;
			if (dfa.ignoreCase) name = name.ToLower();
			if (name.IndexOf(' ') >= 0)
			 SemErr("literal tokens must not contain blanks"); 
		} else SynErr(57);
	}}

	void Term‿NT(out Graph g) {
		{
		Graph g2; Node rslv = null; g = null; 
		if (StartOf(17)) {
			if (isKind(la, 47)) {
				rslv = tab.NewNode(Node.rslv, null, la.line); 
				Resolver‿NT(out rslv.pos);
				g = new Graph(rslv); 
			}
			Factor‿NT(out g2);
			if (rslv != null) tab.MakeSequence(g, g2);
			else g = g2;
			
			while (StartOf(18)) {
				Factor‿NT(out g2);
				tab.MakeSequence(g, g2); 
							}
		} else if (StartOf(19)) {
			g = new Graph(tab.NewNode(Node.eps, null, 0)); 
		} else SynErr(58);
		if (g == null) // invalid start of Term
		 g = new Graph(tab.NewNode(Node.eps, null, 0));
		
	}}

	void Resolver‿NT(out Position pos) {
		{
		Expect(47); // "IF"
		Expect(24); // "("
		int beg = la.pos; int col = la.col; int line = la.line; 
		Condition‿NT();
		pos = new Position(beg, t.pos, col, line); 
	}}

	void Factor‿NT(out Graph g) {
		{
		string name; int kind; Position pos; bool weak = false; 
		g = null;
		
		switch (la.kind) {
		case 1: // ident
		case 3: // string
		case 5: // char
		case 39: // "WEAK"
		{
			if (isKind(la, 39)) {
				Get();
				weak = true; 
			}
			Sym‿NT(out name, out kind);
			Symbol sym = tab.FindSym(name);
			if (sym == null && kind == str)
			 sym = tab.literals[name] as Symbol;
			bool undef = sym == null;
			if (undef) {
			 if (kind == id)
			   sym = tab.NewSym(Node.nt, name, 0);  // forward nt
			 else if (genScanner) { 
			   sym = tab.NewSym(Node.t, name, t.line);
			   dfa.MatchLiteral(sym.name, sym);
			 } else {  // undefined string in production
			   SemErr("undefined string in production");
			   sym = tab.eofSy;  // dummy
			 }
			}
			int typ = sym.typ;
			if (typ != Node.t && typ != Node.nt)
			 SemErr("this symbol kind is not allowed in a production");
			if (weak)
			 if (typ == Node.t) typ = Node.wt;
			 else SemErr("only terminals may be weak");
			Node p = tab.NewNode(typ, sym, t.line);
			g = new Graph(p);
			
			if (StartOf(20)) {
				if (isKind(la, 34) || isKind(la, 36)) {
					Attribs‿NT(p);
					if (kind != id) SemErr("a literal must not have attributes"); 
				} else if (isKind(la, 35)) {
					Get();
					Expect(1); // ident
					if (typ != Node.t && typ != Node.wt) SemErr("only terminals or weak terminals can declare a name in a symbol table"); 
					p.declares = t.val.ToLower();
					if (null == tab.FindSymtab(p.declares)) SemErr(string.Format("undeclared symbol table '{0}'", p.declares));
					
				} else {
					Get();
					Expect(1); // ident
					if (typ != Node.t && typ != Node.wt) SemErr("only terminals or weak terminals can lookup a name in a symbol table"); 
					p.declared = t.val.ToLower(); 
					if (null == tab.FindSymtab(p.declared)) SemErr(string.Format("undeclared symbol table '{0}'", p.declared));
					
				}
			}
			if (undef)
			 sym.attrPos = p.pos;  // dummy
			else if ((p.pos == null) != (sym.attrPos == null))
			 SemErr("attribute mismatch between declaration and use of this symbol");
			
			if (isKind(la, 45) || isKind(la, 46)) {
				AST‿NT(p);
			}
			break;
		}
		case 24: // "("
		{
			Get();
			Expression‿NT(out g);
			Expect(26); // ")"
			break;
		}
		case 40: // "["
		{
			Get();
			Expression‿NT(out g);
			Expect(41); // "]"
			tab.MakeOption(g); 
			break;
		}
		case 42: // "{"
		{
			Get();
			Expression‿NT(out g);
			Expect(43); // "}"
			tab.MakeIteration(g); 
			break;
		}
		case 49: // "(."
		{
			SemText‿NT(out pos);
			Node p = tab.NewNode(Node.sem, null, 0);
			p.pos = pos;
			g = new Graph(p);
			
			break;
		}
		case 32: // "ANY"
		{
			Get();
			Node p = tab.NewNode(Node.any, null, 0);  // p.set is set in tab.SetupAnys
			g = new Graph(p);
			
			break;
		}
		case 44: // "SYNC"
		{
			Get();
			Node p = tab.NewNode(Node.sync, null, 0);
			g = new Graph(p);
			
			break;
		}
		default: SynErr(59); break;
		}
		if (g == null) // invalid start of Factor
		 g = new Graph(tab.NewNode(Node.eps, null, 0));
		
	}}

	void Attribs‿NT(Node p) {
		{
		if (isKind(la, 34)) {
			Get();
			int beg = la.pos; int col = la.col; int line = la.line; 
			while (StartOf(9)) {
				if (StartOf(10)) {
					Get();
				} else {
					Get();
					SemErr("bad string in attributes"); 
				}
							}
			Expect(35); // ">"
			if (t.pos > beg) p.pos = new Position(beg, t.pos, col, line); 
		} else if (isKind(la, 36)) {
			Get();
			int beg = la.pos; int col = la.col; int line = la.line; 
			while (StartOf(11)) {
				if (StartOf(12)) {
					Get();
				} else {
					Get();
					SemErr("bad string in attributes"); 
				}
							}
			Expect(37); // ".>"
			if (t.pos > beg) p.pos = new Position(beg, t.pos, col, line); 
		} else SynErr(60);
	}}

	void AST‿NT(Node p) {
		{
		p.ast = new AstOp(); 
		if (isKind(la, 45)) {
			ASTSendUp‿NT(p);
		} else if (isKind(la, 46)) {
			ASTHatch‿NT(p);
		} else SynErr(61);
	}}

	void ASTSendUp‿NT(Node p) {
		{
		Expect(45); // "^"
		p.ast.ishatch = false;
		string n = p.sym.name;
		if (n.StartsWith("\"")) n = n.Substring(1, n.Length - 2);
		p.ast.name = n.ToLower(); 
		
		if (isKind(la, 45)) {
			Get();
			p.ast.isList = true; 
		}
		if (isKind(la, 33)) {
			Get();
			ASTVal‿NT(out p.ast.name);
		}
	}}

	void ASTHatch‿NT(Node p) {
		{
		Expect(46); // "#"
		p.ast.ishatch = true; 
		if (isKind(la, 46)) {
			Get();
			p.ast.isList = true; 
		}
		if (isKind(la, 6)) {
			ASTPrime‿NT(p);
		}
		if (isKind(la, 33)) {
			Get();
			ASTVal‿NT(out p.ast.name);
		}
		if (isKind(la, 19)) {
			Get();
			ASTConst‿NT(p);
		}
	}}

	void ASTVal‿NT(out string val) {
		{
		val = "?"; 
		if (isKind(la, 1)) {
			Get();
			val = t.val; 
		} else if (isKind(la, 3)) {
			Get();
			val = t.val.Substring(1, t.val.Length - 2); 
		} else SynErr(62);
	}}

	void ASTPrime‿NT(Node p) {
		{
		Expect(6); // prime
		p.ast.primed = true;
		if (p.typ != Node.t && p.typ != Node.wt)
		 SemErr("can only prime terminals");
		if (pgen.IgnoreSemanticActions)
		 errors.Warning(t.line, t.col, "token priming is ignored when ignoring semantic actions (-is switch).");
		 // no way do define the Prime:void->Token function.                                        
		
	}}

	void ASTConst‿NT(Node p) {
		{
		ASTVal‿NT(out p.ast.literal);
	}}

	void Condition‿NT() {
		{
		while (StartOf(21)) {
			if (isKind(la, 24)) {
				Get();
				Condition‿NT();
			} else {
				Get();
			}
					}
		Expect(26); // ")"
	}}

	void TokenTerm‿NT(out Graph g) {
		{
		Graph g2; 
		TokenFactor‿NT(out g);
		while (StartOf(7)) {
			TokenFactor‿NT(out g2);
			tab.MakeSequence(g, g2); 
					}
		if (isKind(la, 48)) {
			Get();
			Expect(24); // "("
			TokenExpr‿NT(out g2);
			tab.SetContextTrans(g2.l); dfa.hasCtxMoves = true;
			tab.MakeSequence(g, g2); 
			Expect(26); // ")"
		}
	}}

	void TokenFactor‿NT(out Graph g) {
		{
		string name; int kind; 
		g = null; 
		if (isKind(la, 1) || isKind(la, 3) || isKind(la, 5)) {
			Sym‿NT(out name, out kind);
			if (kind == id) {
			 CharClass c = tab.FindCharClass(name);
			 if (c == null) {
			   SemErr("undefined name");
			   c = tab.NewCharClass(name, new CharSet());
			 }
			 Node p = tab.NewNode(Node.clas, null, 0); p.val = c.n;
			 g = new Graph(p);
			 tokenString = noString;
			} else { // str
			 g = tab.StrToGraph(name);
			 if (tokenString == null) tokenString = name;
			 else tokenString = noString;
			}
			
		} else if (isKind(la, 24)) {
			Get();
			TokenExpr‿NT(out g);
			Expect(26); // ")"
		} else if (isKind(la, 40)) {
			Get();
			TokenExpr‿NT(out g);
			Expect(41); // "]"
			tab.MakeOption(g); tokenString = noString; 
		} else if (isKind(la, 42)) {
			Get();
			TokenExpr‿NT(out g);
			Expect(43); // "}"
			tab.MakeIteration(g); tokenString = noString; 
		} else SynErr(63);
		if (g == null) // invalid start of TokenFactor
		 g = new Graph(tab.NewNode(Node.eps, null, 0)); 
	}}



	public void Parse() {
		la = new Token();
		la.val = "";
		Get();
		using(astbuilder.createMarker(null, null, false, false, false))
		Coco‿NT();
		Expect(0);

		ast = astbuilder.current;
	}
	
	// a token's base type
	public static readonly int[] tBase = {
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","ident","number","string", "badString","char","\"\\\'\"","\"COMPILER\"", "\"IGNORECASE\"","\"CHARACTERS\"","\"TOKENS\"","\"PRAGMAS\"", "\"COMMENTS\"","\"FROM\"","\"TO\"","\"NESTED\"", "\"IGNORE\"","\"SYMBOLTABLES\"","\"PRODUCTIONS\"","\"=\"",
		"\".\"","\"END\"","\"STRICT\"","\"SCOPES\"", "\"(\"","\",\"","\")\"","\"USEONCE\"", "\"USEALL\"","\"+\"","\"-\"","\"..\"", "\"ANY\"","\":\"","\"<\"","\">\"", "\"<.\"","\".>\"","\"|\"","\"WEAK\"",
		"\"[\"","\"]\"","\"{\"","\"}\"", "\"SYNC\"","\"^\"","\"#\"","\"IF\"", "\"CONTEXT\"","\"(.\"","\".)\"","???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_T,_T,_T, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_T,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_T, _T,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_T, _T,_x,_T,_x, _T,_x,_x,_T, _x,_T,_x,_x, _x},
		{_T,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x},
		{_T,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_T, _T,_T,_T,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_T, _x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_T, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _T,_x,_T,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_x,_x,_T, _x,_T,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_x,_T,_x, _T,_x,_x,_T, _x,_T,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_x,_T,_x, _T,_x,_x,_x, _x,_T,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_T,_T,_T, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_T,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_T, _T,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_T, _T,_x,_T,_x, _T,_x,_x,_T, _x,_T,_x,_x, _x},
		{_T,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x},
		{_T,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_T, _T,_T,_T,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_T, _x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_T, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _T,_x,_T,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_x,_x,_T, _x,_T,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_x,_T,_x, _T,_x,_x,_T, _x,_T,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_x,_T,_x, _T,_x,_x,_x, _x,_T,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x}

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
			case 3: s = "string expected"; break;
			case 4: s = "badString expected"; break;
			case 5: s = "char expected"; break;
			case 6: s = "prime expected"; break;
			case 7: s = "\"COMPILER\" expected"; break;
			case 8: s = "\"IGNORECASE\" expected"; break;
			case 9: s = "\"CHARACTERS\" expected"; break;
			case 10: s = "\"TOKENS\" expected"; break;
			case 11: s = "\"PRAGMAS\" expected"; break;
			case 12: s = "\"COMMENTS\" expected"; break;
			case 13: s = "\"FROM\" expected"; break;
			case 14: s = "\"TO\" expected"; break;
			case 15: s = "\"NESTED\" expected"; break;
			case 16: s = "\"IGNORE\" expected"; break;
			case 17: s = "\"SYMBOLTABLES\" expected"; break;
			case 18: s = "\"PRODUCTIONS\" expected"; break;
			case 19: s = "\"=\" expected"; break;
			case 20: s = "\".\" expected"; break;
			case 21: s = "\"END\" expected"; break;
			case 22: s = "\"STRICT\" expected"; break;
			case 23: s = "\"SCOPES\" expected"; break;
			case 24: s = "\"(\" expected"; break;
			case 25: s = "\",\" expected"; break;
			case 26: s = "\")\" expected"; break;
			case 27: s = "\"USEONCE\" expected"; break;
			case 28: s = "\"USEALL\" expected"; break;
			case 29: s = "\"+\" expected"; break;
			case 30: s = "\"-\" expected"; break;
			case 31: s = "\"..\" expected"; break;
			case 32: s = "\"ANY\" expected"; break;
			case 33: s = "\":\" expected"; break;
			case 34: s = "\"<\" expected"; break;
			case 35: s = "\">\" expected"; break;
			case 36: s = "\"<.\" expected"; break;
			case 37: s = "\".>\" expected"; break;
			case 38: s = "\"|\" expected"; break;
			case 39: s = "\"WEAK\" expected"; break;
			case 40: s = "\"[\" expected"; break;
			case 41: s = "\"]\" expected"; break;
			case 42: s = "\"{\" expected"; break;
			case 43: s = "\"}\" expected"; break;
			case 44: s = "\"SYNC\" expected"; break;
			case 45: s = "\"^\" expected"; break;
			case 46: s = "\"#\" expected"; break;
			case 47: s = "\"IF\" expected"; break;
			case 48: s = "\"CONTEXT\" expected"; break;
			case 49: s = "\"(.\" expected"; break;
			case 50: s = "\".)\" expected"; break;
			case 51: s = "??? expected"; break;
			case 52: s = "this symbol not expected in Coco"; break;
			case 53: s = "this symbol not expected in TokenDecl"; break;
			case 54: s = "invalid TokenDecl"; break;
			case 55: s = "invalid AttrDecl"; break;
			case 56: s = "invalid SimSet"; break;
			case 57: s = "invalid Sym"; break;
			case 58: s = "invalid Term"; break;
			case 59: s = "invalid Factor"; break;
			case 60: s = "invalid Attribs"; break;
			case 61: s = "invalid AST"; break;
			case 62: s = "invalid ASTVal"; break;
			case 63: s = "invalid TokenFactor"; break;

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
            System.Console.WriteLine("-> push {0}, size {1}", e, stack.Count);
        }

        // that's what we call for #/##, built from an AstOp
        public void hatch(Token t, string literal, string name, bool islist) {
            System.Console.WriteLine(">> hatch token {0,-20} as {2,-10}, islist {3}, literal:{1} at {4},{5}.", t.val, literal, name, islist, t.line, t.col);
            E e = new E();
            e.ast = new ASTLiteral(literal != null ? literal : t.val);
            if (islist)
                e.ast = new ASTList(e.ast);
            e.name = name;
            push(e);
        }

        // that's what we call for ^, built from an AstOp
        public void sendup(Token t, string literal, string name, bool islist) {
			if (stack.Count == 0) return;
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
				GC.SuppressFinalize(this);
                Token t = builder.parser.t;
				if (primed) {t = t.Copy(); builder.parser.Prime(t); }
                if (ishatch) builder.hatch(t, literal, name, islist);
                builder.mergeToNull();
                if (!ishatch) builder.sendup(t, literal, name, islist);
            }
        }
    }
}
}