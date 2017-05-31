// This file has to be compiled and linked to a Coco/R generated parser.
// as a variant, you can reference the CocoRCore.dll/exe,
// that includes this classes in a precompiled form.

//#define POSITIONS

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable IDE1006 // Naming Styles

namespace CocoRCore
{

    //-----------------------------------------------------------------------------------
    // ParserBase
    //-----------------------------------------------------------------------------------
    public abstract class ParserBase : IDisposable
    {
        public virtual void Prime(ref Token t) { /* hook */ }
        public abstract string NameOfTokenKind(int tokenKind);
        public abstract int maxT { get; }
        protected abstract void Get();
        protected abstract bool StartOf(int s, int kind);  // is the lookahead token la a start of the production s?
        protected abstract int BaseKindOf(int kind);
        public abstract void Parse();
        public abstract string Syntaxerror(int n);

        public string DuplicateSymbol = "{0} '{1}' declared twice in '{2}'";
        public string MissingSymbol = "{0} '{1}' not declared in '{2}'";
        protected const int minErrDist = 2;
        public ScannerBase scanner { get; private set; }
        public readonly Errors errors;
        public readonly List<Alternative> AlternativeTokens = new List<Alternative>();

        public Token t;    // last recognized token
        public Token la;   // lookahead token
        public Token lb;   // lookback token
        protected int errDist = minErrDist;


        protected ParserBase() => errors = new Errors(this);

        public ParserBase Initialize(ScannerBase scanner)
        {
            this.scanner = scanner;
            return this;
        }

        // disposes only buffers and readers, normally no vital structures
        public virtual void Dispose() => scanner?.Dispose();

        protected void SynErr(int n)
            => DiagnosticMessage(errors.SynErr, la, $"{Syntaxerror(n)}, found {la.ToString(this)}.", n, true);

        public void SemErr(int n, string msg) => SemErr(n, msg, t);
        public void SemErr(int n, string msg, Token t) => DiagnosticMessage(errors.SemErr, t, msg, n, true);

        public void Warning(int n, string msg) => Warning(n, msg, t);
        public void Warning(int n, string msg, Token t) => DiagnosticMessage(errors.Warning, t, msg, n, false);

        public void Information(int n, string msg) => Information(n, msg, t);
        public void Information(int n, string msg, Token t) => DiagnosticMessage(errors.Info, t, msg, n, false);

        public void DiagnosticMessage(Action<Position, string, int> diag, Token t, string msg, int n, bool resetErrDistance)
        {
            if (t == null)
                diag(Position.Zero, msg, n);
            else
            {
                if (errDist >= minErrDist)
                    diag(t.position, msg, n);
                if (resetErrDistance)
                    errDist = 0;
            }
        }

        protected bool StartOf(int s) => StartOf(s, la.kind);

        protected bool isKind(Token t, int n)
        {
            var k = t.kind;
            while (k >= 0)
            {
                if (k == n) return true;
                k = BaseKindOf(k);
            }
            return false;
        }

        protected bool WeakSeparator(int n, int syFol, int repFol)
        {
            var kind = la.kind;
            if (isKind(la, n))
            {
                Get();
                return true;
            }
            else if (StartOf(repFol))
                return false;
            else
            {
                SynErr(n + 1); // error number starts at 1, expect at 0
                while (!(StartOf(syFol, kind) || StartOf(repFol, kind) || StartOf(0, kind)))
                {
                    Get();
                    kind = la.kind;
                }
                return StartOf(syFol);
            }
        }

        protected void Expect(int n)
        {
            if (isKind(la, n))
                Get();
            else
                SynErr(n + 1); // error number starts at 1, expect at 0
        }


        protected void ExpectWeak(int n, int follow)
        {
            if (isKind(la, n))
                Get();
            else
            {
                SynErr(n + 1); // error number starts at 1, expect at 0
                while (!StartOf(follow))
                    Get();
            }
        }


        protected Alt alternatives = null;

        protected void _newAlt() => alternatives = new Alt(maxT + 1);

        protected void addAlt(int kind) => alternatives.alt[kind] = true;

        // a terminal tokenclass of kind kind is restricted to this symbol table 
        // take the root scope, if it is the only scope,
        // make a copy of the scope stack otherwise, but preserve the list references
        protected void addAlt(int kind, Symboltable st) => alternatives.altst[kind] = st.CloneScopes();

        protected void addAlt(int[] range)
        {
            foreach (var kind in range)
                addAlt(kind);
        }

        protected void addAlt(bool[,] pred, int line)
        {
            for (var kind = 0; kind < maxT; kind++)
                if (pred[line, kind])
                    addAlt(kind);
        }


    }


    //-----------------------------------------------------------------------------------
    // Diagnostic
    //-----------------------------------------------------------------------------------
    public class Diagnostic
    {
        public readonly string prefix;
        public readonly int id;
        public readonly int line1;
        public readonly int col1;
        public readonly string level;
        public readonly string message;

        public Diagnostic(int id, string level, int line1, int col1, string message, string prefix)
        {
            this.line1 = line1;
            this.col1 = col1;
            this.id = id;
            this.level = level;
            this.message = message;
            this.prefix = prefix;
        }

        public string Format(string fmt, string uri) => string.Format(fmt, line1, col1, message, level, id, uri, prefix);
    }


    //-----------------------------------------------------------------------------------
    // Errors
    //-----------------------------------------------------------------------------------
    public class Errors : List<Diagnostic>, IFormattable
    {
        private readonly ParserBase parser;
        public System.IO.TextWriter Writer = null;

        public int InfoOffset = 4000; // Infos start at 4000, 1 based
        public int WarningOffset = 3000; // Warnings start at 3000, 1 based
        public int SemErrOffset = 2000; // Semantic Errors start at 2000, 1 based
        public int SynErrOffset = 1000; // Syntax Errors start at 1000, 1 based
        public string ErrorLevel = "error";
        public string WarningLevel = "warning";
        public string InfoLevel = "info";

        public int CountError { get; private set; }
        public int CountWarning { get; private set; }
        public int CountInfo { get; private set; }

        public bool EnableErrors = true;
        public bool EnableWarnings = true;
        public bool EnableInfos = false;

        public readonly ISet<int> Suppressed = new HashSet<int>();
        public IDictionary<int, int> DiagnosticsCounts => _diagnosticsCounts;
        private ConcurrentDictionary<int, int> _diagnosticsCounts = new ConcurrentDictionary<int, int>();

        public string DiagnosticFormat = "{5}({0},{1}): {3} {6}{4}: {2}"; // 0=line, 1=column, 2=text, 3=level, 4=id, 5=uri, 6=CC
        public string DiagnosticFormat0 = "{5}: {3} {6}{4}: {2}"; // 0=line, 1=column, 2=text, 3=level, 4=id, 5=uri, 6=CC
        public string DiagnosticIdPrefix = "CC";

        public Errors(ParserBase parser) => this.parser = parser;

        public void UseShortDiagnosticFormat()
        {
            DiagnosticFormat = "-- line {0}, col {1}: {2}";
            DiagnosticFormat0 = "-- {2}";
        }

        public virtual void Add(int id, string level, int line, int col, string message)
        {
            if (Suppressed.Contains(id)) return;

            if (Object.ReferenceEquals(level, ErrorLevel))
            {
                if (!EnableErrors) return;
                CountError++;
            }

            if (Object.ReferenceEquals(level, WarningLevel))
            {
                if (!EnableWarnings) return;
                CountWarning++;
            }

            if (Object.ReferenceEquals(level, InfoLevel))
            {
                if (!EnableInfos) return;
                CountInfo++;
            }

            _diagnosticsCounts.AddOrUpdate(id, 1, (_, j) => j + 1);
            var error = new Diagnostic(id, level, line, col, message, DiagnosticIdPrefix);
            Add(error);
            Writer?.WriteLine(error.Format(line == 0 ? DiagnosticFormat0 : DiagnosticFormat, parser?.scanner?.uri));
        }

        public void SynErr(int line, int col, string s, int id) => Add(SynErrOffset + id, ErrorLevel, line, col, s);
        public void SynErr(Position pos, string s, int id) => SynErr(pos.line, pos.col, s, id);

        public void SemErr(int line, int col, string s, int id) => Add(SemErrOffset + id, ErrorLevel, line, col, s);
        public void SemErr(Position pos, string s, int id) => SemErr(pos.line, pos.col, s, id);
        public void SemErr(string s, int id) => SemErr(0, 0, s, id);

        public void Warning(int line, int col, string s, int id) => Add(WarningOffset + id, WarningLevel, line, col, s);
        public void Warning(Position pos, string s, int id) => Warning(pos.line, pos.col, s, id);
        public void Warning(string s, int id) => Warning(0, 0, s, id);

        public void Info(int line, int col, string s, int id) => Add(InfoOffset + id, InfoLevel, line, col, s);
        public void Info(Position pos, string s, int id) => Info(pos.line, pos.col, s, id);


        public override string ToString() => ToString("i", null);

        public virtual string ToString(string iwef, IFormatProvider formatProvider)
        {
            iwef = iwef ?? "i";
            var newline = (iwef.ToUpperInvariant() == iwef);
            var separator = newline ? "\r\n" : "  ";
            var sb = new StringBuilder();
            Action<int, string> add = (n, s) => sb.Insert(0, string.Format("{0:n0} {1}{2}{3}", n, s, (n != 1) ? "s" : string.Empty, separator));
            switch (iwef.ToLowerInvariant())
            {
                case "f":
                    var qy =
                        from dc in DiagnosticsCounts
                        where dc.Value > 0
                        orderby dc.Key
                        select dc;
                    foreach (var dc in qy)
                        sb.AppendFormat("[{3}{0}:{1:n0}]{2}", dc.Key, dc.Value, separator, DiagnosticIdPrefix);
                    goto case "i";
                case "i":
                    if (EnableInfos) add(CountInfo, "Info");
                    goto case "w";
                case "w":
                    if (EnableWarnings) add(CountWarning, "Warning");
                    goto case "e";
                case "e":
                    if (EnableErrors) add(CountError, "Error");
                    break;
                default:
                    goto case "w";
            }
            return sb.ToString().Trim();
        }

    } // Errors



    //-----------------------------------------------------------------------------------
    // Alt
    //-----------------------------------------------------------------------------------
    // mutatable alternatives
    public class Alt
    {
        public BitArray alt = null;
        public Symboltable[] altst = null;
        public Symboltable stdeclares = null;
        public Symboltable streferences = null;
        public Token declaration = null;

        public Alt(int size)
        {
            alt = new BitArray(size);
            altst = new Symboltable[size];
        }
    }

    //-----------------------------------------------------------------------------------
    // Alternative, non mutatable and frozen symbols
    //-----------------------------------------------------------------------------------
    public class Alternative
    {
        public readonly Token t; // the scanned token
        public readonly string declares = null; // the name of the symboltable that t declares an item in
        public readonly string references = null; // the name of the symboltable that t references an item in
        public Token declaration = null; // the token where the declaration of t is
        public readonly BitArray alt; // alternative T indexed by kind, only Ts that are not represented by a symbol table are true
        public readonly FrozenSymboltable[] symbols; // symbol tables with alternative entries, usually 0..1 item

        public Alternative(Token t, Alt alternatives)
        {
            this.t = t;
            declares = alternatives.stdeclares?.name;
            references = alternatives.streferences?.name;
            declaration = alternatives.declaration;
            alt = new BitArray(alternatives.alt);

            var symbolsByKind = new List<FrozenSymboltable>();
            for (var kind = 0; kind < alt.Length; kind++)
                if (alt[kind] && alternatives.altst[kind] != null)
                    symbolsByKind.Add(alternatives.altst[kind].Freeze(kind));
            symbols = symbolsByKind.ToArray();

            foreach (var st in symbols)
                alt[st.Kind] = false;
        }
    }


    //-----------------------------------------------------------------------------------
    // TokenEventHandler
    //-----------------------------------------------------------------------------------
    public delegate void TokenEventHandler(Token t);


    //-----------------------------------------------------------------------------------
    // FrozenSymboltable
    //-----------------------------------------------------------------------------------
    public class FrozenSymboltable
    {
        public readonly int Kind;
        public readonly string Name;
        public readonly string[] Items;

        public FrozenSymboltable(int kind, string name, IEnumerable<string> items)
        {
            Kind = kind;
            Name = name;
            Items = items.ToArray();
        }
    }

    //-----------------------------------------------------------------------------------
    // Symboltable
    //-----------------------------------------------------------------------------------
    public class Symboltable
    {
        public readonly string name;
        public readonly bool ignoreCase;
        public readonly bool strict;
        public readonly ParserBase parser;

        public event TokenEventHandler TokenUsed;

        private Stack<List<Token>> scopes;
        private Stack<List<Token>> undeclaredTokens = new Stack<List<Token>>();
        private Symboltable clone = null;


        public Symboltable(string name, bool ignoreCase, bool strict, ParserBase parser)
        {
            this.name = name;
            this.ignoreCase = ignoreCase;
            this.strict = strict;
            this.parser = parser;
            scopes = new Stack<List<Token>>();
            pushNewScope();
        }

        private Symboltable(Symboltable st)
        {
            name = st.name;
            ignoreCase = st.ignoreCase;
            strict = st.strict;
            parser = st.parser;

            // now copy the scopes and its lists
            scopes = new Stack<List<Token>>();
            var reverse = new Stack<List<Token>>(st.scopes);
            foreach (var list in reverse)
                if (strict)
                    scopes.Push(new List<Token>(list)); // strict: copy the list values
                else
                    scopes.Push(list); // non strict: copy the list reference
        }

        // We can keep the clone until we push/pop of the stack, or add a new item. 
        public Symboltable CloneScopes()
        {
            if (clone != null) return clone;
            clone = new Symboltable(this); // i.e. copy scopes
            return clone;
        }

        public FrozenSymboltable Freeze(int kind) => new FrozenSymboltable(kind, name, from t in items select t.valScanned);

        private List<Alternative> fixuplist => parser.AlternativeTokens;

        private StringComparer comparer => ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        private Token Find(IEnumerable<Token> list, Token tok) => list.FirstOrDefault(t => t.val == tok.val);

        public Token Find(Token t) => scopes.Select(list => Find(list, t)).FirstOrDefault(tok => tok != null);


        // ----------------------------------- for Parser use start -------------------- 

        public bool Use(Token t, Alt a)
        {
            TokenUsed?.Invoke(t);
            a.streferences = this;
            if (strict)
            {
                a.declaration = Find(t);
                if (a.declaration != null) return true; // it's ok, if we know the symbol
                return false; // in strict mode we report an illegal symbol
            }
            else
            {
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

        public bool Add(Token t)
        {
            if (Find(currentScope, t) != null)
                return false;
            if (strict) clone = null; // if non strict, we have to keep the clone
            currentScope.Add(t);
            RemoveFromAndFixupList(undeclaredTokens.Peek(), t);
            return true;
        }

        public void Add(string s)
        {
            var t = new Token.Builder()
            {
                kind = -1,
                position = Position.MinusOne
            };
            t.setValue(s, parser.scanner.casingString);
            currentScope.Add(t.Freeze());
        }

        // ----------------------------------- for Parser use end --------------------	

        public bool Contains(Token t) => (Find(t) != null);

        void RemoveFromAndFixupList(List<Token> undeclared, Token declaration)
        {
            var cmp = comparer;
            var found = new List<Token>();
            foreach (var t in undeclared)
                if (0 == cmp.Compare(t.val, declaration.val))
                    found.Add(t);
            foreach (var t in found)
            {
                undeclared.Remove(t);
                foreach (var a in fixuplist)
                    if (a.t == t)
                        a.declaration = declaration;
            }
        }

        void pushNewScope()
        {
            clone = null;
            scopes.Push(new List<Token>());
            undeclaredTokens.Push(new List<Token>());
        }

        void popScope()
        {
            clone = null;
            scopes.Pop();
            PromoteUndeclaredToParent();
        }

        public void CheckDeclared()
        {
            var list = undeclaredTokens.Peek();
            foreach (var t in list)
            {
                var msg = string.Format(parser.MissingSymbol, parser.NameOfTokenKind(t.kind), t.val, name);
                parser.errors.SemErr(t.position, msg, 93);
            }
        }

        void PromoteUndeclaredToParent()
        {
            var list = undeclaredTokens.Pop();
            // now that the lexical scope is about to terminate, we know that there cannot be more declarations in this scope
            // so we can take the existing declarations of the parent scope to resolve these unresolved tokens in 'list'.
            foreach (var decl in currentScope)
                RemoveFromAndFixupList(list, decl);
            // now list contains all tokens that were not delared in the popped scope
            // and not yet declared in the now current scope
            undeclaredTokens.Peek().AddRange(list);
        }

        public IDisposable createScope()
        {
            pushNewScope();
            return new Popper(this);
        }

        public IDisposable createUsageCheck(bool oneOrMore, Token scopeToken)
            => new UseCounter(this, oneOrMore, scopeToken);

        public List<Token> currentScope => scopes.Peek();

        public IEnumerable<Token> items
        {
            get
            {
                if (scopes.Count == 1) return currentScope;

                var all = new Symboltable(name, ignoreCase, true, parser);
                foreach (var list in scopes)
                    foreach (var t in list)
                        all.Add(t);
                return all.currentScope;
            }
        }

        public int CountScopes => scopes.Count;

        private class Popper : IDisposable
        {
            private readonly Symboltable st;

            public Popper(Symboltable st) => this.st = st;

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                st.popScope();
            }
        }

        private class UseCounter : IDisposable
        {
            private readonly Symboltable st;
            public readonly bool oneOrMore; // t - 1..N, f - 0..1
            public readonly List<Token> uses;
            public readonly Token scopeToken;

            public UseCounter(Symboltable st, bool oneOrMore, Token scopeToken)
            {
                this.st = st;
                this.oneOrMore = oneOrMore;
                this.scopeToken = scopeToken;
                uses = new List<Token>();
                st.TokenUsed += uses.Add;
            }

            private bool isValid(List<Token> list)
            {
                var cnt = list.Count;
                if (oneOrMore) return (cnt >= 1);
                return (cnt <= 1);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                st.TokenUsed -= uses.Add;
                var counter = new Dictionary<string, List<Token>>(st.comparer);
                foreach (var t in st.items)
                    counter[t.val] = new List<Token>();
                foreach (var t in uses)
                    if (counter.ContainsKey(t.val)) // we ignore undeclared Tokens:
                        counter[t.val].Add(t);
                // now check for validity
                foreach (var s in counter.Keys)
                {
                    var list = counter[s];
                    if (!isValid(list))
                        if (oneOrMore)
                        {
                            var msg = string.Format("token '{0}' has to be used in this scope.", s);
                            st.parser.errors.SemErr(scopeToken.position, msg, 94);
                        }
                        else
                        {
                            var msg = string.Format("token '{0}' is used {1:n0} time(s) instead of at most once in this scope, see following errors for locations.", s, list.Count);
                            st.parser.errors.SemErr(scopeToken.position, msg, 95);
                            var n = 0;
                            foreach (var t in list)
                            {
                                n++;
                                var msgN = string.Format("... here #{0}/{1}: {2}", n, list.Count, s);
                                st.parser.errors.SemErr(t.position, msgN, 96);
                            }
                        }
                }
            }
        }

    }


    //-----------------------------------------------------------------------------------
    // AST
    //-----------------------------------------------------------------------------------
    public abstract class AST
    {
        public abstract string val { get; }
        public abstract AST this[int i] { get; }
        public abstract AST this[string s] { get; }
        public abstract int count { get; }
        public static readonly AST empty = new ASTLiteral(string.Empty);
        protected abstract void serialize(StringBuilder sb, int indent, Token at);
        public virtual bool merge(E e) => false;
        public Token startToken = null;
        public Token endToken = null;

        #region Formatting
        public static void newline(int indent, StringBuilder sb)
        {
            sb.AppendLine();
            for (var i = 0; i < indent; i++)
                sb.Append("  ");
        }

        public static void escapeJSON(string s, StringBuilder sb)
        {
            // see Mark Amery's comment in
            // http://stackoverflow.com/questions/19176024/how-to-escape-special-characters-in-building-a-json-string
            foreach (var ch in s)
                switch (ch)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\n': sb.Append("\\n"); break;
                    default:
                        if (ch < ' ' || ch > '\u007f') sb.AppendFormat("\\u{0:x4}", ch);
                        else sb.Append(ch);
                        break;
                }
        }

        public override string ToString()
        {
            var at0 = new Token.Builder()
            {
                position = Position.MinusOne
            };
            return ToString(at0.Freeze());
        }

        public string ToString(Token at)
        {
            var sb = new StringBuilder();
            serialize(sb, 0, at);
            return sb.ToString();
        }

        protected bool inOrder(Token at) => inOrder(startToken, at, endToken);

        protected static bool inOrder(Token t1, Token at, Token t2)
        {
            if (t1 == null || at == null || t2 == null)
                return false;
            return t1.pos <= at.pos && at.pos <= t2.pos;
        }

        protected void mergeStart(Token t)
        {
            if (t != null && (startToken == null || t.pos < startToken.pos))
                startToken = t;
        }

        protected void mergeEnd(Token t)
        {
            if (t != null && (endToken == null || endToken.pos < t.pos))
                endToken = t;
        }

        protected void mergeStartEnd(AST a)
        {
            mergeStart(a.startToken);
            mergeEnd(a.endToken);
        }

        // optional listing of character pos ranges for objects
        protected void addPos(StringBuilder sb)
        {
#if POSITIONS
			if (startToken == null || endToken == null) return;
			if (startToken.pos == endToken.pos)
				sb.AppendFormat("/*{0}*/", startToken.charPos);
			else
				sb.AppendFormat("/*{0}-{1}*/", startToken.charPos, endToken.charPos);
#endif
        }

        #endregion


        private abstract class ASTThrows : AST
        {
            public override AST this[int i] => throw new FatalError("not a list");
            public override AST this[string s] => throw new FatalError("not an object");
            public override string val => count.ToString();
        }


        private class ASTLiteral : ASTThrows
        {
            public ASTLiteral(string s) => _val = s;
            private readonly string _val;
            public override string val => _val;
            public override int count => -1;

            protected override void serialize(StringBuilder sb, int indent, Token at)
            {
                sb.Append('\"');
                AST.escapeJSON(val, sb);
                sb.Append('\"');
                addPos(sb);
            }
        }


        private class ASTList : ASTThrows
        {
            public readonly List<AST> list;

            public ASTList() => list = new List<AST>();

            public ASTList(AST a, int i) : this()
            {
                list.Add(a);
                startToken = a.startToken;
                endToken = a.endToken;
            }

            public ASTList(AST a)
            {
                if (a is ASTList li)
                {
                    list = li.list;
                    startToken = li.startToken;
                    endToken = li.endToken;
                    return;
                }
                list = new List<AST> { a };
                startToken = a.startToken;
                endToken = a.endToken;
            }

            public override AST this[int i]
            {
                get
                {
                    if (i < 0 || count <= i)
                        return AST.empty;
                    return list[i];
                }
            }
            public override int count => list.Count;
            public AST merge(AST a)
            {
                if (a is ASTList li)
                {
                    list.AddRange(li.list);
                    foreach (var ast in li.list)
                        mergeStartEnd(ast);
                }
                else
                {
                    list.Add(a);
                    mergeStartEnd(a);
                }
                return a;
            }

            protected override void serialize(StringBuilder sb, int indent, Token at) // ASTList
            {
                var longlist = (count > 3);
                sb.Append('[');
                addPos(sb);
                if (longlist) AST.newline(indent + 1, sb);
                var n = 0;
                foreach (var ast in list)
                {
                    ast.serialize(sb, indent + 1, at);
                    n++;
                    if (n < count)
                    {
                        sb.Append(", ");
                        if (longlist) AST.newline(indent + 1, sb);
                    }
                }
                if (longlist) AST.newline(indent, sb);
                sb.Append(']');
            }

        }


        private class ASTObject : ASTThrows
        {
            private readonly Dictionary<string, AST> ht = new Dictionary<string, AST>();

            public override AST this[string s]
            {
                get
                {
                    if (!ht.ContainsKey(s))
                        return AST.empty;
                    return ht[s];
                }
            }

            public override int count => ht.Keys.Count;

            public void add(E e)
            {
                ht[e.name] = e.ast;
                mergeStartEnd(e.ast);
            }

            public override bool merge(E e)
            {
                if (e.name == null) return false; // cannot merge an unnamed thing
                if (!ht.ContainsKey(e.name))
                {
                    add(e);
                    return true;
                }
                // we have e.nam, call it a thing:
                var thing = ht[e.name];
                if (thing is ASTList)
                {
                    ((ASTList)thing).merge(e.ast);
                    mergeStartEnd(thing);
                    return true;
                }
                // thing is not a list, so we cannot merge it with e
                return false;
            }

            protected override void serialize(StringBuilder sb, int indent, Token at) // ASTObject 
            {
                var longlist = (count > 3);
                sb.Append('{');
                addPos(sb);
                if (longlist) AST.newline(indent + 1, sb);
                var n = 0;
                if (inOrder(at))
                    sb.Append("\"$active\": true, ");
                foreach (var name in ht.Keys)
                {
                    var ast = ht[name];
                    sb.Append('\"');
                    AST.escapeJSON(name, sb);
                    sb.Append("\": ");
                    ast.serialize(sb, indent + 1, at);
                    n++;
                    if (n < count)
                    {
                        sb.Append(", ");
                        if (longlist) AST.newline(indent + 1, sb);
                    }
                }
                if (longlist) AST.newline(indent, sb);
                sb.Append('}');
            }
        }


        public class E
        {
            public string name = null;
            public AST ast = null;

            public override string ToString()
            {
                var a = ast == null ? "null" : ast.ToString();
                var n = name ?? ".";
                return string.Format("{0} = {1};", n, a);
            }

            public E add(E e)
            {
                if (name == e.name)
                {
                    //if (name == null) Console.WriteLine(" [merge two unnamed to a single list]"); else Console.WriteLine(" [merge two named {0} to a single list]", name);
                    var list = new ASTList(ast);
                    list.merge(e.ast);
                    var ret = new E()
                    {
                        ast = list,
                        name = name
                    };
                    return ret;
                }
                else if (name != null && e.name != null)
                {
                    //Console.WriteLine(" [merge named {0}+{1} to an unnamed object]", name, e.name);
                    var obj = new ASTObject();
                    obj.add(this);
                    obj.add(e);
                    var ret = new E()
                    {
                        ast = obj
                    };
                    return ret;
                }
                else if (ast.merge(e))
                    //Console.WriteLine(" [merged {1} into object {0}]", name, e.name);
                    return this;
                //Console.WriteLine(" [no merge available for {0}+{1}]", name, e.name);
                return null;
            }

            public void join(string joinwith)
            {
                if (ast == null || !(ast is ASTList)) return;
                var sb = new StringBuilder();
                for (var i = 0; i < ast.count; i++)
                {
                    if (i > 0) sb.Append(joinwith);
                    sb.Append(ast[i].val);
                }
                ast = new ASTLiteral(sb.ToString());
            }

            public void wrapinlist(bool merge)
            {
                if (ast == null)
                {
                    ast = new ASTList();
                    return;
                }
                if (merge && (ast is ASTList)) return;
                ast = new ASTList(ast, 1);
            }

            public void wrapinobject()
            {
                var o = new ASTObject();
                o.add(this);
                ast = o;
            }
        }


        public class Builder
        {
            public readonly ParserBase parser;
            private readonly Stack<E> stack = new Stack<E>();

            public Builder(ParserBase parser) => this.parser = parser;

            public E currentE => stack.Peek();

            public AST current => stack.Count > 0 ? currentE.ast : new ASTObject();

            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var e in stack)
                    sb.AppendFormat("{0}\n", e);
                return sb.ToString();
            }

            private void push(E e) => stack.Push(e);

            // that's what we call for #/##, built from an AstOp
            public void hatch(Token s, Token t, string literal, string name, bool islist)
            {
                //System.Console.WriteLine(">> hatch token {0,-20} as {2,-10}, islist {3}, literal:{1} at {4},{5}.", t.val, literal, name, islist, t.line, t.col);
                var e = new E()
                {
                    ast = new ASTLiteral(literal ?? t.val)
                };
                e.ast.mergeStart(s);
                e.ast.mergeEnd(t);
                if (islist)
                    e.ast = new ASTList(e.ast);
                e.name = name;
                push(e);
            }

            // that's what we call for ^/^^, built from an AstOp
            public void sendup(Token s, Token t, string literal, string name, bool islist)
            {
                if (stack.Count == 0) return;
                var e = currentE;
                if (e == null)
                {
                    e = new E();
                    if (islist)
                        e.ast = new ASTList();
                    else
                        e.ast = new ASTObject();
                    push(e);
                }
                e.ast.mergeStart(s);
                e.ast.mergeEnd(t);
                //if (islist) System.Console.WriteLine(">> send up as [{0}]: {1}", name, e); else System.Console.WriteLine(">> send up as {0}: {1}", name, e);
                if (name != e.name)
                    if (islist)
                    {
                        var merge = (e.name == null);
                        e.wrapinlist(merge);
                    }
                    else if (e.name != null)
                        e.wrapinobject();
                e.name = name;
                //System.Console.WriteLine("-------------> top {0}", e);
            }

            /*
            private void mergeConflict(Token t, E e, E with, string typ, int n) {
                parser.errors.Warning(t.line, t.col, string.Format("AST merge {2} size {3}: {0} WITH {1}", e, with, typ, n));
            } 
            */

            // remove the topmost null on the stack, keeping anythng else 
            public void popNull()
            {
                var list = new Stack<E>();
                while (true)
                {
                    if (stack.Count == 0) break;
                    var e = stack.Pop();
                    if (e == null) break;
                    list.Push(e);
                }
                foreach (var e in list)
                    stack.Push(e);
            }

            private void join(string joinwith, Token s, Token t, Token la)
            {
                var e = currentE;
                if (e == null)
                {
                    e = new E();
                    var source = parser.scanner.buffer.GetBufferedString(s.position.Range(la));
                    source = source.Trim();
                    e.ast = new ASTLiteral(source)
                    {
                        startToken = s,
                        endToken = t
                    };
                    push(e);
                }
                else if (e.ast is ASTList)
                    e.join(joinwith);
                else
                    parser.SemErr(91, "cannot join here: AST stack is neither empty nor stack top is an ASTList.");
            }

            private void mergeAt(Token t)
            {
                while (mergeToNull(t))
                    /**/
                    ;
                popNull();
            }

            private bool mergeToNull(Token t)
            {
                var somethingMerged = false;
                var list = new Stack<E>();
                var cnt = 0;
                while (true)
                {
                    if (stack.Count == 0) return false;
                    if (currentE == null) break; // don't pop the null
                    list.Push(stack.Pop());
                    cnt++;
                }
                if (cnt == 0) return false; // nothing was pushed
                if (cnt == 1)
                {
                    // we promote the one thing on the stack to the parent frame, i.e. swap:
                    popNull();
                    stack.Push(list.Pop());
                    stack.Push(null);
                    return false;
                }
                // merge as much as we can and push the results. Start with null
                E ret = null;
                var n = 0;
                foreach (var e in list)
                {
                    n++;
                    //System.Console.Write("{3}>> {1} of {2}   merge: {0}", e, n, cnt, stack.Count);
                    if (ret == null)
                        ret = e;
                    else
                    {
                        var merged = ret.add(e);
                        if (merged != null)
                        {
                            somethingMerged = true;
                            //mergeConflict(t, e, ret, "success", stack.Count);
                            ret = merged;
                        }
                        else
                        {
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


            public IDisposable createMarker(string name, string literal, bool islist, bool ishatch, bool primed)
                => new Marker(this, name, literal, islist, ishatch, primed);

            private class Marker : IDisposable
            {
                public readonly Builder builder;
                public readonly string literal;
                public readonly string name;
                public readonly bool islist;
                public readonly bool ishatch;
                public readonly bool primed;
                public readonly Token startToken;

                public Marker(Builder builder, string name, string literal, bool islist, bool ishatch, bool primed)
                {
                    this.builder = builder;
                    this.literal = literal;
                    this.name = name;
                    this.ishatch = ishatch;
                    this.islist = islist;
                    this.primed = primed;
                    var t = builder.parser.la;
                    startToken = t;
                    if (ishatch)
                    {
                        if (primed)
                            try
                            {
                                builder.parser.Prime(ref t);
                            }
                            catch (Exception ex)
                            {
                                builder.parser.SemErr(92, string.Format("unexpected error in Prime(t): {0}", ex.Message));
                            }
                        builder.hatch(t, t, literal, name, islist);
                    }
                    else
                        builder.stack.Push(null); // push a marker
                }

                public void Dispose()
                {
                    GC.SuppressFinalize(this);
                    var t = builder.parser.t;
                    if (!ishatch)
                    {
                        builder.sendup(startToken, t, literal, name, islist);
                        builder.mergeAt(t);
                    }
                }
            }


            public IDisposable createBarrier(string joinwith) => new Barrier(this, joinwith);

            private class Barrier : IDisposable
            {
                public readonly Builder builder;
                public readonly Token startToken;
                public readonly string joinwith;

                public Barrier(Builder builder, string joinwith)
                {
                    this.builder = builder;
                    this.joinwith = joinwith;
                    startToken = builder.parser.la;
                    builder.stack.Push(null); // push a marker
                }

                public void Dispose()
                {
                    GC.SuppressFinalize(this);
                    var t = builder.parser.t;
                    builder.mergeAt(t);
                    if (joinwith != null)
                        builder.join(joinwith, startToken, t, builder.parser.la);
                }
            }
        }
    } // end AST
}
