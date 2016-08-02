using System;
using System.IO;

public class Inheritance {

    static void printST(Symboltable st) {
        if (st == null) return;
        Console.WriteLine("--- symbol-table ------------------------------------------------------------------- {0}({1}){2}", st.name, st.CountScopes, st.ignoreCase ? " IGONRECASE" : "");
        int n = 0;
        foreach (Token t in st.currentScope) {
            n++;
            string s = string.Format("{0}({1},{2})", t.val, t.line, t.col);
            Console.Write("{0,-40}  ", s);
            if (n%3 == 0) Console.WriteLine(); 
        }
        if (n%3 != 0) Console.WriteLine();
        Console.WriteLine();
    }

	public static int Main (string[] arg) {
		Console.WriteLine("WFModel parser");
        if (arg.Length == 1)
        {
            Console.WriteLine("scanning {0} ...", arg[0]);
            Scanner scanner = new Scanner(arg[0], true); // is UTF8 source
			Parser parser = new Parser(scanner);
            parser.Parse();
            Console.WriteLine("{0} error(s) detected", parser.errors.count);

            // list all symbol table values
            printST(parser.symbols("types"));
            printST(parser.symbols("enumtypes"));
            printST(parser.symbols("variables"));

            if (false) {
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
            Console.WriteLine("usage: WFModel.exe file");
            return 99;
        }
        return 0;
    }
}