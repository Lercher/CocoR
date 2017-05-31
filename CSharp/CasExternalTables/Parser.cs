
//#define POSITIONS

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;



public class Parserbase {
	public virtual void Prime(Token t) { /* hook */ }
}

public class Parser : Parserbase {
	public const int _EOF = 0; // TOKEN EOF
	public const int _number = 1; // TOKEN number
	public const int _ident = 2; // TOKEN ident
	public const int _set = 3; // TOKEN set INHERITS ident
	public const int _serveroutput = 4; // TOKEN serveroutput INHERITS ident
	public const int _on = 5; // TOKEN on INHERITS ident
	public const int _size = 6; // TOKEN size INHERITS ident
	public const int _insert = 7; // TOKEN insert INHERITS ident
	public const int _update = 8; // TOKEN update INHERITS ident
	public const int _delete = 9; // TOKEN delete INHERITS ident
	public const int _into = 10; // TOKEN into INHERITS ident
	public const int _values = 11; // TOKEN values INHERITS ident
	public const int _prompt = 12; // TOKEN prompt INHERITS ident
	public const int _null = 13; // TOKEN null INHERITS ident
	public const int _lantusparam = 14; // TOKEN lantusparam INHERITS ident
	public const int _tusparam = 15; // TOKEN tusparam INHERITS ident
	public const int _tusnom = 16; // TOKEN tusnom INHERITS ident
	public const int _tupcode = 17; // TOKEN tupcode INHERITS ident
	public const int _tupflagorfi = 18; // TOKEN tupflagorfi INHERITS ident
	public const int _tuplibelle = 19; // TOKEN tuplibelle INHERITS ident
	public const int _sc = 20; // TOKEN sc
	public const int _openparen = 21; // TOKEN openparen
	public const int _closeparen = 22; // TOKEN closeparen
	public const int _slash = 23; // TOKEN slash
	public const int _dot = 24; // TOKEN dot
	public const int _comma = 25; // TOKEN comma
	public const int _equals = 26; // TOKEN equals
	public const int _doublebar = 27; // TOKEN doublebar
	public const int _string = 28; // TOKEN string
	public const int _stars = 29; // TOKEN stars
	public const int maxT = 63;

	const bool _T = true;
	const bool _x = false;
	public const string DuplicateSymbol = "{0} '{1}' declared twice in '{2}'";
	public const string MissingSymbol ="{0} '{1}' not declared in '{2}'";
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;
	public readonly List<Alternative> tokens = new List<Alternative>();
	
	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

	public readonly Symboltable languages;
	public readonly Symboltable deletabletables;
	public readonly Symboltable updatetables;
	public readonly Symboltable columns;
	public readonly Symboltable chrarguments;
	public Symboltable symbols(string name) {
		if (name == "languages") return languages;
		if (name == "deletabletables") return deletabletables;
		if (name == "updatetables") return updatetables;
		if (name == "columns") return columns;
		if (name == "chrarguments") return chrarguments;
		return null;
	}

public HashSet<int> suppressed = new HashSet<int>();
public Dictionary<int, int> diagnostics = new Dictionary<int, int>();

void SemErr(int n, string s, string kind) {
    if (suppressed.Contains(n)) return;
    int count = 0;
    diagnostics.TryGetValue(n, out count);
    diagnostics[n] = count + 1;
    var msg = string.Format("{2}-#{0} {1}", n, s, kind);
    SemErr(msg);
}

// C# methods of Parser -- end



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();		
		languages = new Symboltable("languages", true, true, tokens);
		languages.Add("\'en\'");
		languages.Add("\'fr\'");
		deletabletables = new Symboltable("deletabletables", true, true, tokens);
		deletabletables.Add("lantusparam");
		deletabletables.Add("tusparam");
		deletabletables.Add("lktuptactpg");
		updatetables = new Symboltable("updatetables", true, true, tokens);
		updatetables.Add("tusparam");
		updatetables.Add("lantusparam");
		columns = new Symboltable("columns", true, true, tokens);
		columns.Add("tusnom");
		columns.Add("tupcode");
		columns.Add("tupflagorfi");
		chrarguments = new Symboltable("chrarguments", true, true, tokens);
		chrarguments.Add("38");

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

	
	void CasExternalTables‿NT() {
		{
		addAlt(12); // OPT
		if (isKind(la, 12)) {
			StarPrompt‿NT();
			addAlt(23); // T
			Expect(23); // slash
		}
		SetServeroutput‿NT();
		addAlt(23); // T
		Expect(23); // slash
		addAlt(set0, 1); // ITER start
		while (StartOf(1)) {
			Block‿NT();
			addAlt(set0, 1); // ITER end
		}
		addAlt(0); // T
		Expect(0); // EOF
	}}

	void StarPrompt‿NT() {
		{
		addAlt(12); // T
		Expect(12); // prompt
		addAlt(29); // T
		Expect(29); // stars
		addAlt(set0, 2); // ITER start
		while (StartOf(2)) {
			Get();
			addAlt(set0, 2); // ITER end
		}
		addAlt(29); // T
		Expect(29); // stars
	}}

	void SetServeroutput‿NT() {
		{
		addAlt(3); // T
		Expect(3); // set
		addAlt(4); // T
		Expect(4); // serveroutput
		addAlt(5); // T
		Expect(5); // on
		addAlt(6); // T
		Expect(6); // size
		addAlt(1); // T
		Expect(1); // number
		addAlt(20); // T
		Expect(20); // sc
	}}

	void Block‿NT() {
		{
		addAlt(new int[] {3, 31}); // ALT
		addAlt(12); // ALT
		addAlt(30); // ALT
		if (isKind(la, 3) || isKind(la, 31)) {
			ExceptionHandledBlock‿NT();
		} else if (isKind(la, 12)) {
			StarPrompt‿NT();
		} else if (isKind(la, 30)) {
			DeclareBlock‿NT();
		} else SynErr(64);
		addAlt(23); // ITER start
		while (isKind(la, 23)) {
			Get();
			addAlt(23); // ITER end
		}
	}}

	void ExceptionHandledBlock‿NT() {
		{
		addAlt(3); // OPT
		if (isKind(la, 3)) {
			Get();
			addAlt(4); // T
			Expect(4); // serveroutput
			addAlt(5); // T
			Expect(5); // on
			addAlt(20); // T
			Expect(20); // sc
		}
		addAlt(31); // T
		Expect(31); // "begin"
		InsertDeleteUpdate‿NT();
		addAlt(new int[] {7, 8, 9}); // ITER start
		while (isKind(la, 7) || isKind(la, 8) || isKind(la, 9)) {
			InsertDeleteUpdate‿NT();
			addAlt(new int[] {7, 8, 9}); // ITER end
		}
		addAlt(32); // ALT
		addAlt(new int[] {33, 59}); // ALT
		if (isKind(la, 32)) {
			Get();
			addAlt(20); // T
			Expect(20); // sc
		} else if (isKind(la, 33) || isKind(la, 59)) {
			SemErr(2, "COMMIT in ExceptionHandledBlock missing", "CRIT"); 
		} else SynErr(65);
		addAlt(59); // ALT
		addAlt(33); // ALT
		if (isKind(la, 59)) {
			PutLine‿NT();
		} else if (isKind(la, 33)) {
			SemErr(3, "COMMIT without dbms_output.PUT_LINE", "WARN"); 
		} else SynErr(66);
		addAlt(33); // T
		Expect(33); // "exception"
		addAlt(34); // T
		Expect(34); // "when"
		addAlt(35); // T
		Expect(35); // "others"
		addAlt(36); // T
		Expect(36); // "then"
		addAlt(59); // OPT
		if (isKind(la, 59)) {
			PutLine‿NT();
		}
		addAlt(37); // ALT
		addAlt(13); // ALT
		if (isKind(la, 37)) {
			Get();
		} else if (isKind(la, 13)) {
			Get();
			SemErr(20, "Null instead of ROLLBACK in exception block", "WARN"); 
		} else SynErr(67);
		addAlt(20); // T
		Expect(20); // sc
		addAlt(59); // OPT
		if (isKind(la, 59)) {
			PutLine‿NT();
		}
		addAlt(38); // T
		Expect(38); // "end"
		addAlt(20); // T
		Expect(20); // sc
		addAlt(23); // T
		Expect(23); // slash
	}}

	void DeclareBlock‿NT() {
		{
		addAlt(30); // T
		Expect(30); // "declare"
		SemErr(1, "Mysterious DECLARE block detected", "TBD"); 
		addAlt(set0, 3); // ITER start
		while (StartOf(3)) {
			Get();
			addAlt(set0, 3); // ITER end
		}
		addAlt(23); // T
		Expect(23); // slash
	}}

	void InsertDeleteUpdate‿NT() {
		{
		addAlt(7); // ALT
		addAlt(9); // ALT
		addAlt(8); // ALT
		if (isKind(la, 7)) {
			Insert‿NT();
		} else if (isKind(la, 9)) {
			Delete‿NT();
		} else if (isKind(la, 8)) {
			Update‿NT();
		} else SynErr(68);
	}}

	void PutLine‿NT() {
		{
		addAlt(59); // T
		Expect(59); // "dbms_output"
		addAlt(24); // T
		Expect(24); // dot
		addAlt(60); // T
		Expect(60); // "put_line"
		addAlt(21); // T
		Expect(21); // openparen
		UncheckedString‿NT();
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
	}}

	void Insert‿NT() {
		{
		addAlt(7); // T
		Expect(7); // insert
		addAlt(10); // T
		Expect(10); // into
		addAlt(54); // ALT
		addAlt(56); // ALT
		addAlt(15); // ALT
		addAlt(14); // ALT
		addAlt(42); // ALT
		addAlt(45); // ALT
		addAlt(49); // ALT
		addAlt(51); // ALT
		switch (la.kind) {
		case 54: // "tuser"
		{
			TUSER‿NT();
			break;
		}
		case 56: // "lantuser"
		{
			LANTUSER‿NT();
			break;
		}
		case 15: // tusparam
		{
			TUSPARAM‿NT();
			break;
		}
		case 14: // lantusparam
		{
			LANTUSPARAM‿NT();
			break;
		}
		case 42: // "ttrparam"
		{
			TTRPARAM‿NT();
			break;
		}
		case 45: // "lanttrparam"
		{
			LANTTRPARAM‿NT();
			break;
		}
		case 49: // "ttraitement"
		{
			TTRAITEMENT‿NT();
			break;
		}
		case 51: // "lanttraitement"
		{
			LANTTRAITEMENT‿NT();
			break;
		}
		default: SynErr(69); break;
		}
	}}

	void Delete‿NT() {
		{
		addAlt(9); // T
		Expect(9); // delete
		addAlt(41); // T
		Expect(41); // "from"
		if (!deletabletables.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, deletabletables.name));
		addAlt(2); // T
		addAlt(2, deletabletables); // T ident uses symbol table 'deletabletables'
		Expect(2); // ident
		addAlt(39); // T
		Expect(39); // "where"
		if (!columns.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, columns.name));
		addAlt(2); // T
		addAlt(2, columns); // T ident uses symbol table 'columns'
		Expect(2); // ident
		addAlt(26); // T
		Expect(26); // equals
		String‿NT();
		addAlt(40); // ITER start
		while (isKind(la, 40)) {
			Get();
			Get();
			addAlt(set0, 4); // ITER start
			while (StartOf(4)) {
				Get();
				addAlt(set0, 4); // ITER end
			}
			addAlt(40); // ITER end
		}
		addAlt(20); // T
		Expect(20); // sc
	}}

	void Update‿NT() {
		{
		addAlt(8); // T
		Expect(8); // update
		if (!updatetables.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, updatetables.name));
		addAlt(2); // T
		addAlt(2, updatetables); // T ident uses symbol table 'updatetables'
		Expect(2); // ident
		addAlt(3); // T
		Expect(3); // set
		if (!columns.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, columns.name));
		addAlt(2); // T
		addAlt(2, columns); // T ident uses symbol table 'columns'
		Expect(2); // ident
		addAlt(26); // T
		Expect(26); // equals
		addAlt(1); // ALT
		addAlt(28); // ALT
		addAlt(2); // ALT
		addAlt(2, columns); // ALT ident uses symbol table 'columns'
		if (isKind(la, 1)) {
			Get();
		} else if (isKind(la, 28)) {
			Get();
		} else if (isKind(la, 2)) {
			if (!columns.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, columns.name));
			Get();
		} else SynErr(70);
		addAlt(39); // T
		Expect(39); // "where"
		if (!columns.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "ident", la.val, columns.name));
		addAlt(2); // T
		addAlt(2, columns); // T ident uses symbol table 'columns'
		Expect(2); // ident
		addAlt(26); // T
		Expect(26); // equals
		String‿NT();
		addAlt(40); // ITER start
		while (isKind(la, 40)) {
			Get();
			Get();
			addAlt(set0, 4); // ITER start
			while (StartOf(4)) {
				Get();
				addAlt(set0, 4); // ITER end
			}
			addAlt(40); // ITER end
		}
		addAlt(20); // T
		Expect(20); // sc
	}}

	void String‿NT() {
		{
		StringFactor‿NT();
		if (t.kind == _string && t.val.StartsWith("' ")) {
		   if (t.val.ToUpper() == t.val && t.val.Replace(" ", "").Length + 1 == t.val.Length)
		       SemErr(24, "Key-kind string literal starts with a space: " + t.val, "CRIT");
		   else
		       SemErr(17, "First string literal starts with a space: " + t.val, "WARN");
		}
		
		addAlt(27); // ITER start
		while (isKind(la, 27)) {
			Get();
			StringFactor‿NT();
			addAlt(27); // ITER end
		}
		if (t.kind == _string && t.val.EndsWith(" '")) {
		   if (t.val.ToUpper() == t.val && t.val.Replace(" ", "").Length + 1 == t.val.Length)
		       SemErr(25, "Key-kind string literal ends with a space: " + t.val, "CRIT");
		   else
		       SemErr(18, "Last string literal ends with a space: " + t.val, "WARN");
		}
		
	}}

	void TUSER‿NT() {
		{
		addAlt(54); // T
		Expect(54); // "tuser"
		addAlt(21); // ALT
		addAlt(11); // ALT
		if (isKind(la, 21)) {
			Get();
			addAlt(16); // T
			Expect(16); // tusnom
			addAlt(25); // T
			Expect(25); // comma
			addAlt(55); // T
			Expect(55); // "tuslongueur"
			addAlt(22); // T
			Expect(22); // closeparen
		} else if (isKind(la, 11)) {
			SemErr(10, "Insert into TUSER without column list", "WARN"); 
		} else SynErr(71);
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		addAlt(1); // ALT
		addAlt(13); // ALT
		addAlt(28); // ALT
		if (isKind(la, 1)) {
			Get();
		} else if (isKind(la, 13)) {
			Get();
			SemErr(11, t.val + " as TUSLONGUEUR (a length) is invalid", "TBD"); 
		} else if (isKind(la, 28)) {
			Get();
			SemErr(12, t.val + " (string) as TUSLONGUEUR (a length) is invalid", "TBD"); 
		} else SynErr(72);
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
	}}

	void LANTUSER‿NT() {
		{
		addAlt(56); // T
		Expect(56); // "lantuser"
		addAlt(21); // ALT
		addAlt(11); // ALT
		if (isKind(la, 21)) {
			Get();
			addAlt(16); // T
			Expect(16); // tusnom
			addAlt(25); // T
			Expect(25); // comma
			addAlt(46); // ALT
			addAlt(57); // ALT
			if (isKind(la, 46)) {
				LANTUSER_LANCODE‿NT();
			} else if (isKind(la, 57)) {
				LANTUSER_TUSLIBELLE‿NT();
			} else SynErr(73);
		} else if (isKind(la, 11)) {
			Get();
			addAlt(21); // T
			Expect(21); // openparen
			String‿NT();
			SemErr(13, "INSERT INTO LANTUSER without column list: " + t.val, "WARN"); 
			addAlt(25); // T
			Expect(25); // comma
			String‿NT();
			addAlt(25); // T
			Expect(25); // comma
			String‿NT();
			addAlt(22); // T
			Expect(22); // closeparen
			addAlt(20); // T
			Expect(20); // sc
		} else SynErr(74);
	}}

	void TUSPARAM‿NT() {
		{
		bool isOrfi = false; 
		addAlt(15); // T
		Expect(15); // tusparam
		addAlt(21); // ALT
		addAlt(11); // ALT
		if (isKind(la, 21)) {
			Get();
			addAlt(16); // T
			Expect(16); // tusnom
			addAlt(25); // T
			Expect(25); // comma
			addAlt(17); // T
			Expect(17); // tupcode
			addAlt(25); // OPT
			if (isKind(la, 25)) {
				Get();
				addAlt(18); // T
				Expect(18); // tupflagorfi
			}
			addAlt(22); // T
			Expect(22); // closeparen
		} else if (isKind(la, 11)) {
			SemErr(14, "INSERT INTO TUSPARAM without column list", "WARN"); 
		} else SynErr(75);
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // OPT
		if (isKind(la, 25)) {
			Get();
			addAlt(1); // ALT
			addAlt(13); // ALT
			addAlt(28); // ALT
			if (isKind(la, 1)) {
				Get();
				isOrfi = true; 
			} else if (isKind(la, 13)) {
				Get();
			} else if (isKind(la, 28)) {
				Get();
				SemErr(15, "TUPFLAGORFI (a number) is assigned the string " + t.val, "CRIT"); 
			} else SynErr(76);
		}
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
		if (isOrfi)
		   SemErr(21, "Suspicious INSERT to table TUSPARAM in externaltables with ORFI set", "WARN");
		else
		   SemErr(22, "Forbidden INSERT to table TUSPARAM in externaltables", "CRIT");
		
	}}

	void LANTUSPARAM‿NT() {
		{
		addAlt(14); // T
		Expect(14); // lantusparam
		addAlt(21); // ALT
		addAlt(11); // ALT
		if (isKind(la, 21)) {
			Get();
			addAlt(16); // T
			Expect(16); // tusnom
			addAlt(25); // T
			Expect(25); // comma
			addAlt(17); // T
			Expect(17); // tupcode
			addAlt(25); // T
			Expect(25); // comma
			addAlt(46); // ALT
			addAlt(19); // ALT
			if (isKind(la, 46)) {
				LANTUSPARAM_LANCODE‿NT();
			} else if (isKind(la, 19)) {
				LANTUSPARAM_TUPLIBELLE‿NT();
			} else SynErr(77);
		} else if (isKind(la, 11)) {
			Get();
			addAlt(21); // T
			Expect(21); // openparen
			String‿NT();
			SemErr(16, "INSERT INTO LANTUSPARAM without column list: " + t.val, "WARN"); 
			addAlt(25); // T
			Expect(25); // comma
			String‿NT();
			addAlt(25); // T
			Expect(25); // comma
			if (!languages.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, languages.name));
			addAlt(28); // T
			addAlt(28, languages); // T string uses symbol table 'languages'
			Expect(28); // string
			addAlt(25); // T
			Expect(25); // comma
			String‿NT();
			addAlt(25); // T
			Expect(25); // comma
			addAlt(new int[] {28, 61, 62}); // ALT
			addAlt(13); // ALT
			if (isKind(la, 28) || isKind(la, 61) || isKind(la, 62)) {
				String‿NT();
			} else if (isKind(la, 13)) {
				Get();
			} else SynErr(78);
			addAlt(22); // T
			Expect(22); // closeparen
			addAlt(20); // T
			Expect(20); // sc
		} else SynErr(79);
	}}

	void TTRPARAM‿NT() {
		{
		addAlt(42); // T
		Expect(42); // "ttrparam"
		SemErr(4, "Forbidden insert into TTRPARAM in externaltables", "CRIT"); 
		addAlt(21); // T
		Expect(21); // openparen
		addAlt(43); // T
		Expect(43); // "ttrnom"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(44); // T
		Expect(44); // "ttpcode"
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
	}}

	void LANTTRPARAM‿NT() {
		{
		addAlt(45); // T
		Expect(45); // "lanttrparam"
		SemErr(5, "Forbidden insert into LANTTRPARAM in externaltables", "CRIT"); 
		addAlt(21); // T
		Expect(21); // openparen
		addAlt(43); // T
		Expect(43); // "ttrnom"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(44); // T
		Expect(44); // "ttpcode"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(46); // ALT
		addAlt(47); // ALT
		if (isKind(la, 46)) {
			LANTTRPARAM_LANCODE‿NT();
		} else if (isKind(la, 47)) {
			LANTTRPARAM_TTPLIBELLE‿NT();
		} else SynErr(80);
		addAlt(25); // ALT
		addAlt(22); // ALT
		if (isKind(la, 25)) {
			Get();
			addAlt(new int[] {28, 61, 62}); // ALT
			addAlt(13); // ALT
			if (isKind(la, 28) || isKind(la, 61) || isKind(la, 62)) {
				String‿NT();
			} else if (isKind(la, 13)) {
				Get();
				SemErr(6, "LANTTRPARAM without Helptext (null)", "WARN"); 
			} else SynErr(81);
		} else if (isKind(la, 22)) {
			SemErr(7, "LANTTRPARAM without Helptext (column missing)", "WARN"); 
		} else SynErr(82);
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
	}}

	void TTRAITEMENT‿NT() {
		{
		addAlt(49); // T
		Expect(49); // "ttraitement"
		SemErr(8, "Forbidden insert into TTRAITEMENT in externaltables", "CRIT"); 
		addAlt(21); // T
		Expect(21); // openparen
		addAlt(43); // T
		Expect(43); // "ttrnom"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(50); // T
		Expect(50); // "ttrflagpref"
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		addAlt(1); // T
		Expect(1); // number
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
	}}

	void LANTTRAITEMENT‿NT() {
		{
		addAlt(51); // T
		Expect(51); // "lanttraitement"
		SemErr(9, "Forbidden insert into LANTTRAITEMENT in externaltables", "CRIT"); 
		addAlt(21); // T
		Expect(21); // openparen
		addAlt(43); // T
		Expect(43); // "ttrnom"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(46); // ALT
		addAlt(52); // ALT
		if (isKind(la, 46)) {
			LANTTRAITEMENT_LANCODE‿NT();
		} else if (isKind(la, 52)) {
			LANTTRAITEMENT_TTRLIBELLE‿NT();
		} else SynErr(83);
		addAlt(20); // T
		Expect(20); // sc
	}}

	void LANTTRPARAM_LANCODE‿NT() {
		{
		addAlt(46); // T
		Expect(46); // "lancode"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(47); // T
		Expect(47); // "ttplibelle"
		addAlt(25); // OPT
		if (isKind(la, 25)) {
			Get();
			addAlt(48); // T
			Expect(48); // "ttphelptext"
		}
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		if (!languages.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, languages.name));
		addAlt(28); // T
		addAlt(28, languages); // T string uses symbol table 'languages'
		Expect(28); // string
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
	}}

	void LANTTRPARAM_TTPLIBELLE‿NT() {
		{
		addAlt(47); // T
		Expect(47); // "ttplibelle"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(46); // T
		Expect(46); // "lancode"
		addAlt(25); // OPT
		if (isKind(la, 25)) {
			Get();
			addAlt(48); // T
			Expect(48); // "ttphelptext"
		}
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		if (!languages.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, languages.name));
		addAlt(28); // T
		addAlt(28, languages); // T string uses symbol table 'languages'
		Expect(28); // string
	}}

	void LANTTRAITEMENT_LANCODE‿NT() {
		{
		addAlt(46); // T
		Expect(46); // "lancode"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(52); // T
		Expect(52); // "ttrlibelle"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(53); // T
		Expect(53); // "ttrcontext"
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		if (!languages.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, languages.name));
		addAlt(28); // T
		addAlt(28, languages); // T string uses symbol table 'languages'
		Expect(28); // string
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(22); // T
		Expect(22); // closeparen
	}}

	void LANTTRAITEMENT_TTRLIBELLE‿NT() {
		{
		addAlt(52); // T
		Expect(52); // "ttrlibelle"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(53); // T
		Expect(53); // "ttrcontext"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(46); // T
		Expect(46); // "lancode"
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		if (!languages.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, languages.name));
		addAlt(28); // T
		addAlt(28, languages); // T string uses symbol table 'languages'
		Expect(28); // string
		addAlt(22); // T
		Expect(22); // closeparen
	}}

	void LANTUSER_LANCODE‿NT() {
		{
		addAlt(46); // T
		Expect(46); // "lancode"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(57); // T
		Expect(57); // "tuslibelle"
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		if (!languages.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, languages.name));
		addAlt(28); // T
		addAlt(28, languages); // T string uses symbol table 'languages'
		Expect(28); // string
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
	}}

	void LANTUSER_TUSLIBELLE‿NT() {
		{
		addAlt(57); // T
		Expect(57); // "tuslibelle"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(46); // T
		Expect(46); // "lancode"
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		if (!languages.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, languages.name));
		addAlt(28); // T
		addAlt(28, languages); // T string uses symbol table 'languages'
		Expect(28); // string
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
	}}

	void LANTUSPARAM_LANCODE‿NT() {
		{
		addAlt(46); // T
		Expect(46); // "lancode"
		addAlt(25); // T
		Expect(25); // comma
		addAlt(19); // T
		Expect(19); // tuplibelle
		addAlt(25); // OPT
		if (isKind(la, 25)) {
			Get();
			addAlt(58); // T
			Expect(58); // "tuphelptext"
		}
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		if (!languages.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, languages.name));
		addAlt(28); // T
		addAlt(28, languages); // T string uses symbol table 'languages'
		Expect(28); // string
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // OPT
		if (isKind(la, 25)) {
			Get();
			addAlt(new int[] {28, 61, 62}); // ALT
			addAlt(13); // ALT
			if (isKind(la, 28) || isKind(la, 61) || isKind(la, 62)) {
				String‿NT();
			} else if (isKind(la, 13)) {
				Get();
			} else SynErr(84);
		}
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
	}}

	void LANTUSPARAM_TUPLIBELLE‿NT() {
		{
		addAlt(19); // T
		Expect(19); // tuplibelle
		addAlt(25); // T
		Expect(25); // comma
		addAlt(46); // T
		Expect(46); // "lancode"
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(11); // T
		Expect(11); // values
		addAlt(21); // T
		Expect(21); // openparen
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		String‿NT();
		addAlt(25); // T
		Expect(25); // comma
		if (!languages.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "string", la.val, languages.name));
		addAlt(28); // T
		addAlt(28, languages); // T string uses symbol table 'languages'
		Expect(28); // string
		addAlt(22); // T
		Expect(22); // closeparen
		addAlt(20); // T
		Expect(20); // sc
	}}

	void UncheckedString‿NT() {
		{
		StringFactor‿NT();
		addAlt(27); // ITER start
		while (isKind(la, 27)) {
			Get();
			StringFactor‿NT();
			addAlt(27); // ITER end
		}
	}}

	void StringFactor‿NT() {
		{
		addAlt(28); // ALT
		addAlt(61); // ALT
		addAlt(62); // ALT
		if (isKind(la, 28)) {
			Get();
			if (t.val.Contains("\n"))
			   SemErr(19, "Illegal line break in string literal", "CRIT");
			if (t.val.Contains("Ã"))
			   SemErr(23, "Suspicious UTF-8/ANSI char in string literal: " + t.val, "WARN");
			
		} else if (isKind(la, 61)) {
			Get();
			addAlt(21); // T
			Expect(21); // openparen
			if (!chrarguments.Use(la, alternatives)) SemErr(la, string.Format(MissingSymbol, "number", la.val, chrarguments.name));
			addAlt(1); // T
			addAlt(1, chrarguments); // T number uses symbol table 'chrarguments'
			Expect(1); // number
			addAlt(22); // T
			Expect(22); // closeparen
		} else if (isKind(la, 62)) {
			Get();
		} else SynErr(85);
	}}



	public void Parse() {
		la = new Token();
		la.val = "";
		Get();
		CasExternalTables‿NT();
		Expect(0);
		languages.CheckDeclared(errors);
		deletabletables.CheckDeclared(errors);
		updatetables.CheckDeclared(errors);
		columns.CheckDeclared(errors);
		chrarguments.CheckDeclared(errors);
		
	}
	
	// a token's base type
	public static readonly int[] tBase = {
		-1,-1,-1, 2,  2, 2, 2, 2,  2, 2, 2, 2,  2, 2, 2, 2,  2, 2, 2, 2,
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1, -1,-1,-1,-1,
		-1,-1,-1,-1
	};

	// a token's name
	public static readonly string[] tName = {
		"EOF","number","ident","\"set\"", "\"serveroutput\"","\"on\"","\"size\"","\"insert\"", "\"update\"","\"delete\"","\"into\"","\"values\"", "\"prompt\"","\"null\"","\"lantusparam\"","\"tusparam\"", "\"tusnom\"","\"tupcode\"","\"tupflagorfi\"","\"tuplibelle\"",
		"\";\"","\"(\"","\")\"","\"/\"", "\".\"","\",\"","\"=\"","\"||\"", "string","stars","\"declare\"","\"begin\"", "\"commit\"","\"exception\"","\"when\"","\"others\"", "\"then\"","\"rollback\"","\"end\"","\"where\"",
		"\"and\"","\"from\"","\"ttrparam\"","\"ttrnom\"", "\"ttpcode\"","\"lanttrparam\"","\"lancode\"","\"ttplibelle\"", "\"ttphelptext\"","\"ttraitement\"","\"ttrflagpref\"","\"lanttraitement\"", "\"ttrlibelle\"","\"ttrcontext\"","\"tuser\"","\"tuslongueur\"", "\"lantuser\"","\"tuslibelle\"","\"tuphelptext\"","\"dbms_output\"",
		"\"put_line\"","\"chr\"","\"sqlerrm\"","???"
	};

	// states that a particular production (1st index) can start with a particular token (2nd index)
	static readonly bool[,] set0 = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x}

	};

	// as set0 but with token inheritance taken into account
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x}

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
			case 3: s = "set expected"; break;
			case 4: s = "serveroutput expected"; break;
			case 5: s = "on expected"; break;
			case 6: s = "size expected"; break;
			case 7: s = "insert expected"; break;
			case 8: s = "update expected"; break;
			case 9: s = "delete expected"; break;
			case 10: s = "into expected"; break;
			case 11: s = "values expected"; break;
			case 12: s = "prompt expected"; break;
			case 13: s = "null expected"; break;
			case 14: s = "lantusparam expected"; break;
			case 15: s = "tusparam expected"; break;
			case 16: s = "tusnom expected"; break;
			case 17: s = "tupcode expected"; break;
			case 18: s = "tupflagorfi expected"; break;
			case 19: s = "tuplibelle expected"; break;
			case 20: s = "sc expected"; break;
			case 21: s = "openparen expected"; break;
			case 22: s = "closeparen expected"; break;
			case 23: s = "slash expected"; break;
			case 24: s = "dot expected"; break;
			case 25: s = "comma expected"; break;
			case 26: s = "equals expected"; break;
			case 27: s = "doublebar expected"; break;
			case 28: s = "string expected"; break;
			case 29: s = "stars expected"; break;
			case 30: s = "\"declare\" expected"; break;
			case 31: s = "\"begin\" expected"; break;
			case 32: s = "\"commit\" expected"; break;
			case 33: s = "\"exception\" expected"; break;
			case 34: s = "\"when\" expected"; break;
			case 35: s = "\"others\" expected"; break;
			case 36: s = "\"then\" expected"; break;
			case 37: s = "\"rollback\" expected"; break;
			case 38: s = "\"end\" expected"; break;
			case 39: s = "\"where\" expected"; break;
			case 40: s = "\"and\" expected"; break;
			case 41: s = "\"from\" expected"; break;
			case 42: s = "\"ttrparam\" expected"; break;
			case 43: s = "\"ttrnom\" expected"; break;
			case 44: s = "\"ttpcode\" expected"; break;
			case 45: s = "\"lanttrparam\" expected"; break;
			case 46: s = "\"lancode\" expected"; break;
			case 47: s = "\"ttplibelle\" expected"; break;
			case 48: s = "\"ttphelptext\" expected"; break;
			case 49: s = "\"ttraitement\" expected"; break;
			case 50: s = "\"ttrflagpref\" expected"; break;
			case 51: s = "\"lanttraitement\" expected"; break;
			case 52: s = "\"ttrlibelle\" expected"; break;
			case 53: s = "\"ttrcontext\" expected"; break;
			case 54: s = "\"tuser\" expected"; break;
			case 55: s = "\"tuslongueur\" expected"; break;
			case 56: s = "\"lantuser\" expected"; break;
			case 57: s = "\"tuslibelle\" expected"; break;
			case 58: s = "\"tuphelptext\" expected"; break;
			case 59: s = "\"dbms_output\" expected"; break;
			case 60: s = "\"put_line\" expected"; break;
			case 61: s = "\"chr\" expected"; break;
			case 62: s = "\"sqlerrm\" expected"; break;
			case 63: s = "??? expected"; break;
			case 64: s = "invalid Block"; break;
			case 65: s = "invalid ExceptionHandledBlock"; break;
			case 66: s = "invalid ExceptionHandledBlock"; break;
			case 67: s = "invalid ExceptionHandledBlock"; break;
			case 68: s = "invalid InsertDeleteUpdate"; break;
			case 69: s = "invalid Insert"; break;
			case 70: s = "invalid Update"; break;
			case 71: s = "invalid TUSER"; break;
			case 72: s = "invalid TUSER"; break;
			case 73: s = "invalid LANTUSER"; break;
			case 74: s = "invalid LANTUSER"; break;
			case 75: s = "invalid TUSPARAM"; break;
			case 76: s = "invalid TUSPARAM"; break;
			case 77: s = "invalid LANTUSPARAM"; break;
			case 78: s = "invalid LANTUSPARAM"; break;
			case 79: s = "invalid LANTUSPARAM"; break;
			case 80: s = "invalid LANTTRPARAM"; break;
			case 81: s = "invalid LANTTRPARAM"; break;
			case 82: s = "invalid LANTTRPARAM"; break;
			case 83: s = "invalid LANTTRAITEMENT"; break;
			case 84: s = "invalid LANTUSPARAM_LANCODE"; break;
			case 85: s = "invalid StringFactor"; break;

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
