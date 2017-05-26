using System.IO;



//#define POSITIONS

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using CocoRCore;

namespace CocoRCore.CSharp {



	public class Parser : ParserBase 
	{
	public const int _ident = 1; // TOKEN ident
	public const int _number = 2; // TOKEN number
	public const int _string = 3; // TOKEN string
	public const int _badString = 4; // TOKEN badString
	public const int _char = 5; // TOKEN char
	public const int _prime = 6; // TOKEN prime
	private const int __maxT = 51;
	public const int _ddtSym = 52;
	public const int _optionSym = 53;

		private const bool _T = true;
		private const bool _x = false;
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

/*-------------------------------------------------------------------------*/



		public Parser(ScannerBase scanner) : base(scanner, new Errors())
		{

		}

		public override int maxT => __maxT;

		protected override void Get() 
		{
			for (;;) 
			{
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

        private bool isKind(Token t, int n)
        {
            var k = t.kind;
            while (k >= 0)
            {
                if (k == n) return true;
                k = tBase[k];
            }
            return false;
        }

        // is the lookahead token la a start of the production s?
        private bool StartOf(int s)
        {
            return set[s, la.kind];
        }

        private bool WeakSeparator(int n, int syFol, int repFol)
        {
            var kind = la.kind;
            if (isKind(la, n)) { Get(); return true; }
            else if (StartOf(repFol)) { return false; }
            else
            {
                SynErr(n);
                while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind]))
                {
                    Get();
                    kind = la.kind;
                }
                return StartOf(syFol);
            }
        }

        protected void Expect(int n)
        {
            if (isKind(la, n)) Get(); else { SynErr(n); }
        }


        protected void ExpectWeak(int n, int follow)
        {
            if (isKind(la, n)) Get();
            else
            {
                SynErr(n);
                while (!StartOf(follow)) Get();
            }
        }


	void Coco‿NT() {
		{
		Symbol sym; Graph g, g1, g2; string gramName; CharSet s; 
		if (StartOf(1)) {
			Get();
			var usingPos = t.position; 
			while (StartOf(1)) {
				Get();
							}
			pgen.usingPos = usingPos.Range(la); 
		}
		Expect(7); // "COMPILER"
		genScanner = true; 
		tab.ignored = new CharSet(); 
		Expect(1); // ident
		gramName = t.val;
		var semDeclPos = la.position;
		
		while (StartOf(2)) {
			Get();
					}
		tab.semDeclPos = semDeclPos.Range(la); 
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
				TokenDecl‿NT(NodeKind.t);
							}
		}
		if (isKind(la, 11)) {
			Get();
			while (isKind(la, 1) || isKind(la, 3) || isKind(la, 5)) {
				TokenDecl‿NT(NodeKind.pr);
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
			if (undef) sym = tab.NewSym(NodeKind.nt, t.val, t.position);
			else {
			 if (sym.typ == NodeKind.nt) {
			   if (sym.graph != null) SemErr(1, "name declared twice");
			 } else SemErr(2, "this symbol kind not allowed on left side of production");
			 sym.pos = t.position;
			}
			bool noAttrs = sym.attrPos == null;
			sym.attrPos = null;
			
			if (isKind(la, 34) || isKind(la, 36)) {
				AttrDecl‿NT(sym);
			}
			if (!undef)
			 if (noAttrs != (sym.attrPos == null))
			   SemErr(3, "attribute mismatch between declaration and use of this symbol");
			
			if (isKind(la, 29)) {
				ASTJoin‿NT(sym);
			}
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
		 SemErr(4, "name does not match grammar name");
		tab.gramSy = tab.FindSym(gramName);
		if (tab.gramSy == null)
		 SemErr(5, "missing production for grammar name");
		else {
		 sym = tab.gramSy;
		 if (sym.attrPos != null)
		   SemErr(6, "grammar symbol must not have attributes");
		}
		tab.noSym = tab.NewSym(NodeKind.t, "???", Position.Zero); // noSym gets highest number
		tab.SetupAnys();
		tab.RenumberPragmas();
		if (tab.ddt[2]) tab.PrintNodes();
		if (errors.Count == 0) {
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
		if (c != null) SemErr(9, "name declared twice");
		
		Expect(19); // "="
		Set‿NT(out s);
		if (s.Elements() == 0) SemErr(10, "character set must not be empty");
		tab.NewCharClass(name, s);
		
		Expect(20); // "."
	}}

	void TokenDecl‿NT(NodeKind typ) {
		{
		Sym‿NT(out var name, out var kind);
		var sym = tab.FindSym(name);
		if (sym != null) SemErr(13, "name declared twice");
		else {
		 sym = tab.NewSym(typ, name, t.position);
		 sym.tokenKind = Symbol.fixedToken;
		}
		tokenString = null;
		
		if (isKind(la, 33)) {
			Get();
			Sym‿NT(out var inheritsName, out var inheritsKind);
			var inheritsSym = tab.FindSym(inheritsName);
			if (inheritsSym == null) SemErr(14, string.Format("token '{0}' can't inherit from '{1}', name not declared", sym.name, inheritsName));
			else if (inheritsSym == sym) SemErr(15, string.Format("token '{0}' must not inherit from self", sym.name));
			else if (inheritsSym.typ != typ) SemErr(16, string.Format("token '{0}' can't inherit from '{1}'", sym.name, inheritsSym.name));
			else sym.inherits = inheritsSym;
			
		}
		while (!(StartOf(5))) {SynErr(53); Get();}
		if (isKind(la, 19)) {
			Get();
			TokenExpr‿NT(out var g);
			Expect(20); // "."
			if (kind == str) SemErr(17, "a literal must not be declared with a structure");
			tab.Finish(g);
			if (tokenString == null || tokenString.Equals(noString))
			 dfa.ConvertToStates(g.l, sym);
			else { // TokenExpr is a single string
			 if (tab.literals.ContainsKey(tokenString))
			   SemErr(18, "token string declared twice");
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
			if (typ != NodeKind.pr) SemErr(19, "semantic action not allowed in a pragma context"); 
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
		string name = t.val.ToLowerInvariant();                                    
		if (tab.FindSymtab(name) != null) 
		 SemErr(7, "symbol table name declared twice");
		st = new SymTab(name);
		tab.symtabs.Add(st);
		
		if (isKind(la, 22)) {
			Get();
			st.strict = true; 
		}
		while (isKind(la, 3)) {
			Get();
			string predef = tab.Unstring(t.val);
			if (dfa.ignoreCase) predef = predef.ToLowerInvariant();
			st.Add(predef);
			
					}
		Expect(20); // "."
	}}

	void AttrDecl‿NT(Symbol sym) {
		{
		if (isKind(la, 34)) {
			Get();
			var attrPos = la.position; 
			while (StartOf(9)) {
				if (StartOf(10)) {
					Get();
				} else {
					Get();
					SemErr(20, "bad string in attributes"); 
				}
							}
			Expect(35); // ">"
			sym.attrPos = attrPos.RangeIfNotEmpty(t); 
		} else if (isKind(la, 36)) {
			Get();
			var attrPos = la.position; 
			while (StartOf(11)) {
				if (StartOf(12)) {
					Get();
				} else {
					Get();
					SemErr(21, "bad string in attributes"); 
				}
							}
			Expect(37); // ".>"
			sym.attrPos = attrPos.RangeIfNotEmpty(t); 
		} else SynErr(55);
	}}

	void ASTJoin‿NT(Symbol sym) {
		{
		Expect(29); // "+"
		sym.astjoinwith = ""; pgen.needsAST = true; 
		if (isKind(la, 3)) {
			Get();
			sym.astjoinwith = tab.Unstring(t.val);
		}
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

	void SemText‿NT(out Range pos) {
		{
		Expect(49); // "(."
		var p = la.position; 
		while (StartOf(13)) {
			if (StartOf(14)) {
				Get();
			} else if (isKind(la, 4)) {
				Get();
				SemErr(36, "bad string in semantic action"); 
			} else {
				Get();
				SemErr(37, "missing end of previous semantic action"); 
			}
					}
		Expect(50); // ".)"
		pos = p.Range(t); 
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
		string stname = t.val.ToLowerInvariant();
		SymTab st = tab.FindSymtab(stname); 
		if (st == null) SemErr(8, "undeclared symbol table " + t.val);
		else sts.Add(st);
		
	}}

	void SimSet‿NT(out CharSet s) {
		{
		int n1, n2; 
		s = new CharSet(); 
		if (isKind(la, 1)) {
			Get();
			CharClass c = tab.FindCharClass(t.val);
			if (c == null) SemErr(11, "undefined name"); else s.Or(c.set);
			
		} else if (isKind(la, 3)) {
			Get();
			string name = tab.Unstring(t.val);
			foreach (char ch in name)
			 if (dfa.ignoreCase) s.Set(char.ToLowerInvariant(ch));
			 else s.Set(ch); 
		} else if (isKind(la, 5)) {
			Char‿NT(out n1);
			s.Set(n1); 
			if (isKind(la, 31)) {
				Get();
				Char‿NT(out n2);
				for (var i = n1; i <= n2; i++) s.Set(i); 
			}
		} else if (isKind(la, 32)) {
			Get();
			s = new CharSet(); s.Fill(); 
		} else SynErr(56);
	}}

	void Char‿NT(out int n) {
		{
		Expect(5); // char
		string name = tab.Unstring(t.val); n = 0;
		if (name.Length == 1) n = name[0];
		else SemErr(12, "unacceptable character value");
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
			if (dfa.ignoreCase) name = name.ToLowerInvariant();
			if (name.IndexOf(' ') >= 0)
			 SemErr(33, "literal tokens must not contain blanks"); 
		} else SynErr(57);
	}}

	void Term‿NT(out Graph g) {
		{
		Graph g2; Node rslv = null; g = null; 
		if (StartOf(17)) {
			if (isKind(la, 47)) {
				rslv = tab.NewNode(NodeKind.rslv, null, la.line); 
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
			g = new Graph(tab.NewNode(NodeKind.eps, null, 0)); 
		} else SynErr(58);
		if (g == null) // invalid start of Term
		 g = new Graph(tab.NewNode(NodeKind.eps, null, 0));
		
	}}

	void Resolver‿NT(out Range pos) {
		{
		Expect(47); // "IF"
		Expect(24); // "("
		var p = la.position; 
		Condition‿NT();
		pos = p.Range(t); 
	}}

	void Factor‿NT(out Graph g) {
		{
		string name; int kind; Range pos; bool weak = false; 
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
			 tab.literals.TryGetValue(name, out sym);
			bool undef = sym == null;
			if (undef) {
			 if (kind == id)
			   sym = tab.NewSym(NodeKind.nt, name, Position.Zero);  // forward nt
			 else if (genScanner) { 
			   sym = tab.NewSym(NodeKind.t, name, t.position);
			   dfa.MatchLiteral(sym.name, sym);
			 } else {  // undefined string in production
			   SemErr(22, "undefined string in production");
			   sym = tab.eofSy;  // dummy
			 }
			}
			var typ = sym.typ;
			if (typ != NodeKind.t && typ != NodeKind.nt)
			 SemErr(23, "this symbol kind is not allowed in a production");
			if (weak)
			 if (typ == NodeKind.t) typ = NodeKind.wt;
			 else SemErr(24, "only terminals may be weak");
			Node p = tab.NewNode(typ, sym, t.line);
			g = new Graph(p);
			
			if (StartOf(20)) {
				if (isKind(la, 34) || isKind(la, 36)) {
					Attribs‿NT(p);
					if (kind != id) SemErr(25, "a literal must not have attributes"); 
				} else if (isKind(la, 35)) {
					Get();
					Expect(1); // ident
					if (typ != NodeKind.t && typ != NodeKind.wt) SemErr(26, "only terminals or weak terminals can declare a name in a symbol table"); 
					p.declares = t.val.ToLowerInvariant();
					if (null == tab.FindSymtab(p.declares)) SemErr(27, string.Format("undeclared symbol table '{0}'", p.declares));
					
				} else {
					Get();
					Expect(1); // ident
					if (typ != NodeKind.t && typ != NodeKind.wt) SemErr(28, "only terminals or weak terminals can lookup a name in a symbol table"); 
					p.declared = t.val.ToLowerInvariant(); 
					if (null == tab.FindSymtab(p.declared)) SemErr(29, string.Format("undeclared symbol table '{0}'", p.declared));
					
				}
			}
			if (undef)
			 sym.attrPos = p.pos;  // dummy
			else if ((p.pos == null) != (sym.attrPos == null))
			 SemErr(30, "attribute mismatch between declaration and use of this symbol");
			
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
			Node p = tab.NewNode(NodeKind.sem, null, 0);
			p.pos = pos;
			g = new Graph(p);
			
			break;
		}
		case 32: // "ANY"
		{
			Get();
			Node p = tab.NewNode(NodeKind.any, null, 0);  // p.set is set in tab.SetupAnys
			g = new Graph(p);
			
			break;
		}
		case 44: // "SYNC"
		{
			Get();
			Node p = tab.NewNode(NodeKind.sync, null, 0);
			g = new Graph(p);
			
			break;
		}
		default: SynErr(59); break;
		}
		if (g == null) // invalid start of Factor
		 g = new Graph(tab.NewNode(NodeKind.eps, null, 0));
		
	}}

	void Attribs‿NT(Node p) {
		{
		if (isKind(la, 34)) {
			Get();
			var pos = la.position; 
			while (StartOf(9)) {
				if (StartOf(10)) {
					Get();
				} else {
					Get();
					SemErr(34, "bad string in attributes"); 
				}
							}
			Expect(35); // ">"
			p.pos = pos.RangeIfNotEmpty(t); 
		} else if (isKind(la, 36)) {
			Get();
			var pos = la.position; 
			while (StartOf(11)) {
				if (StartOf(12)) {
					Get();
				} else {
					Get();
					SemErr(35, "bad string in attributes"); 
				}
							}
			Expect(37); // ".>"
			p.pos = pos.RangeIfNotEmpty(t); 
		} else SynErr(60);
	}}

	void AST‿NT(Node p) {
		{
		p.asts = new List<AstOp>(); pgen.needsAST = true; 
		if (isKind(la, 45)) {
			ASTSendUp‿NT(p);
		} else if (isKind(la, 46)) {
			ASTHatch‿NT(p);
			while (WeakSeparator(25,21,22) ) {
				ASTHatch‿NT(p);
							}
		} else SynErr(61);
	}}

	void ASTSendUp‿NT(Node p) {
		{
		AstOp ast = p.addAstOp(); 
		Expect(45); // "^"
		ast.ishatch = false;
		string n = p.sym.name;
		if (n.StartsWith("\"")) n = n.Substring(1, n.Length - 2);
		ast.name = n.ToLowerInvariant(); 
		
		if (isKind(la, 45)) {
			Get();
			ast.isList = true; 
		}
		if (isKind(la, 33)) {
			Get();
			ASTVal‿NT(out ast.name);
		}
	}}

	void ASTHatch‿NT(Node p) {
		{
		AstOp ast = p.addAstOp(); 
		Expect(46); // "#"
		ast.ishatch = true; 
		if (isKind(la, 46)) {
			Get();
			ast.isList = true; 
		}
		if (isKind(la, 6)) {
			ASTPrime‿NT(p, ast);
		}
		if (isKind(la, 33)) {
			Get();
			ASTVal‿NT(out ast.name);
		}
		if (isKind(la, 19)) {
			Get();
			ASTConst‿NT(ast);
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
			val = tab.Unstring(t.val); 
		} else SynErr(62);
	}}

	void ASTPrime‿NT(Node p, AstOp ast) {
		{
		Expect(6); // prime
		ast.primed = true;
		if (p.typ != NodeKind.t && p.typ != NodeKind.wt)
		 SemErr(31, "can only prime terminals");
		if (pgen.IgnoreSemanticActions)
		 Warning(1, "token priming is ignored when ignoring semantic actions (-is switch).");
		 // no way do define the Prime:void->Token function.                                        
		
	}}

	void ASTConst‿NT(AstOp ast) {
		{
		ASTVal‿NT(out ast.literal);
	}}

	void Condition‿NT() {
		{
		while (StartOf(23)) {
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
			   SemErr(32, "undefined name");
			   c = tab.NewCharClass(name, new CharSet());
			 }
			 Node p = tab.NewNode(NodeKind.clas, null, 0); p.val = c.n;
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
		 g = new Graph(tab.NewNode(NodeKind.eps, null, 0)); 
	}}



		public override void Parse() 
		{
			la = Token.Zero;
			Get();
		Coco‿NT();
		Expect(0);
		
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
		public override string NameOf(int tokenKind) => tName[tokenKind];

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
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _T,_x,_T,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_T,_x,_x, _x},
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
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _T,_x,_T,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_T,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x}

		};




		private class Errors : ErrorsBase
		{
			public override void SynErr(int line, int col, int n) 
			{
				string s;
				switch (n) 
				{
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
				// public void Add(int id, int level, int line, int col, string message)
				Add(SynErrOffset + n, ErrorLevel, line, col, s);
			}
		} // Errors

	} // end Parser

// end namespace implicit
}