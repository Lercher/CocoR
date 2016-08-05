using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


// AST belongs to parser.frame

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
        for(int i = 0; i < indent; i++);
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
        public readonly Errors errors;
        private readonly Stack<E> stack = new Stack<E>();

        public Builder(Errors errors) {
            this.errors = errors;
        }
        
        public E currentE { get { return stack.Peek(); } }
        public AST current { get { return currentE.ast; } }

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
            System.Console.WriteLine(">> hatch token {0,-20} as {2,-10}, islist {3}, literal:{1}.", t.val, literal, name, islist);
            E e = new E();
            e.ast = new ASTLiteral(literal != null ? literal : t.val);
            if (islist)
                e.ast = new ASTList(e.ast);
            e.name = name;
            push(e);
        }

        // that's what we call for ^, built from an AstOp
        public void sendup(Token t, string literal, string name, bool islist) {
            E e = currentE;
            if (islist)
                System.Console.WriteLine(">> send up as [{0}]: {1}", name, e);
            else
                System.Console.WriteLine(">> send up as {0}: {1}", name, e);
            if (name != e.name) {
                if (islist)
                    e.wrapinlist(); 
                else 
                    errors.Warning(t.line, t.col, string.Format("overwriting AST objectname '{0}' with '{1}'", e.name, name));
            }
            e.name = name;
            mergeCompatibles(false);
            System.Console.WriteLine("-------------> top {0}", e);
        }

        public void mergeCompatibles(bool final) {
            E ret = null;
            while(stack.Count > 0) {      
                E e = currentE;
                if (e == null) {
                    if (!final) break;                        
                    stack.Pop();
                } else if (ret == null) { 
                    ret = e;
                    stack.Pop();
                } else {
                    System.Console.Write(">> try merge {0} with {1}", ret, e);
                    E merged = e.add(ret);
                    if (merged != null) {
                        ret = merged;
                        stack.Pop();
                    }                        
                    else 
                        break;
                }
                System.Console.WriteLine(" -> ret={0}", ret);
            }
            push(ret);
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

        public IDisposable createMarker() {
            return new Marker(this);
        }

        private class Marker : IDisposable {
            public readonly Builder builder;

            public Marker(Builder builder) {
                this.builder = builder;
                builder.stack.Push(null);
            }

            public void Dispose() {
                builder.mergeToNull();
            }
        }

    }
}



public class Inheritance {

    static void printST(Symboltable st) {
        Console.WriteLine("--- symbol-table{2} ------------------------------------------------------------------- {0}({1})", st.name, st.CountScopes, st.ignoreCase ? " IGNORECASE" : "");
        int n = 0;
        foreach (Token t in st.currentScope) {
            n++;
            string s = string.Format("{0}({1},{2})", t.val, t.line, t.col);
            Console.Write("{0,-20}  ", s);
            if (n%4 == 0) Console.WriteLine(); 
        }
        if (n%4 != 0) Console.WriteLine();
        Console.WriteLine();
    }

	public static int Main (string[] arg) {
		Console.WriteLine("Inheritance parser");
        if (arg.Length >= 1)
        {
            Console.WriteLine("scanning {0} ...", arg[0]);
            Scanner scanner = new Scanner(arg[0], true); // is UTF8 source
			Parser parser = new Parser(scanner);
            parser.Parse();
            Console.WriteLine("{0} error(s) detected", parser.errors.count);

            // list all symbol table values
            printST(parser.types);
            printST(parser.variables);

            System.Console.WriteLine("----------------------- AST builder stack ----------------------------");
            System.Console.WriteLine(parser.astbuilder);

            System.Console.WriteLine("----------------------- AST ----------------------------");
            System.Console.WriteLine(parser.ast);

            if (arg.Length > 1) {
                // list all alternatives
                int line = 0;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (Alternative a in parser.tokens)
                {
                    Token t = a.t;
                    if (line == 0) {                    
                        sb.Append(new string('-', 50));
                        sb.Append(new string(' ', t.col));
                        sb.Append("  ");
                        line = t.line;
                    }
                    if (line != t.line) {
                        line = t.line;
                        Console.WriteLine(sb.ToString());
                        sb.Length = 0;
                        sb.Append(new string('-', 50));
                        sb.Append(new string(' ', t.col));
                        sb.Append("  ");
                    }
                    sb.Append(t.val); sb.Append(' ');
                    string decl = a.declaration == null ? "" : string.Format(" declared({0},{1})", a.declaration.line, a.declaration.col);
                    Console.Write("({0,3},{1,3}) {2,3} {3,-30} {4, -20}", t.line, t.col, t.kind, Parser.tName[t.kind] + decl, t.val);
                    Console.Write("      alt: ");
                    for (int k = 0; k <= Parser.maxT; k++)
                    {
                        if (a.alt[k]) {
                            Console.Write("{1}", k, Parser.tName[k]);
                            if (a.st[k] != null) {
                                Console.Write(":{0}({1})|", a.st[k].name, a.st[k].CountScopes);
                                foreach (Token tok in a.st[k].currentScope)
                                    Console.Write("{0}({1},{2})|", tok.val, tok.line, tok.col);    
                            }
                            Console.Write(' ');
                        }
                    }
                    Console.WriteLine();
                }
                Console.WriteLine(sb.ToString());
            }
            if (parser.errors.count > 0)
                return 1;
        } else {
            Console.WriteLine("usage: Inheritance.exe file [-ac]");
            return 99;
        }
        return 0;
    }
}