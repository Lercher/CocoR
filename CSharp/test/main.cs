using System;
using System.IO;

public class Inheritance {

    static void printST(Symboltable st) {
        Console.WriteLine("--- symbol-table ------------------------------------------------------------------- {0}{1}", st.name, st.ignoreCase ? " IGONRECASE" : "");
        int n = 0;
        foreach (string s in st.items) {
            n++;
            Console.Write("{0,-20}  ", s);
            if (n%4 == 0) Console.WriteLine(); 
        }
        if (n%4 != 0) Console.WriteLine();
        Console.WriteLine();
    }

	public static int Main (string[] arg) {
		Console.WriteLine("Inheritance parser");
        if (arg.Length == 1)
        {
            Console.WriteLine("scanning {0} ...", arg[0]);
            Scanner scanner = new Scanner(arg[0]);
			Parser parser = new Parser(scanner);
            parser.Parse();
            Console.WriteLine("{0} error(s) detected", parser.errors.count);
            int line = 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // list all symbol table values
            printST(parser.types);
            printST(parser.variables);


            // list all alternatives
            foreach (Alternative a in parser.tokens)
            {
                Token t = a.t;
                if (line == 0) {                    
                    sb.Append(new string('-', t.col + 50));
                    sb.Append("  ");
                    line = t.line;
                }
                if (line != t.line) {
                    line = t.line;
                    Console.WriteLine(sb.ToString());
                    sb.Length = 0;
                    sb.Append(new string('-', t.col + 50));
                    sb.Append("  ");
                }
                sb.Append(t.val); sb.Append(' ');
                Console.Write("({0,3},{1,3}) {2,3} {3,-20} {4, -20}", t.line, t.col, t.kind, Parser.tName[t.kind], t.val);
                Console.Write("                    alt: ");
                for (int k = 0; k <= Parser.maxT; k++)
                {
                    if (a.alt[k]) {
                        Console.Write("{1}", k, Parser.tName[k]);
                        if (a.st[k] != null)
                            Console.Write(":{0}", a.st[k].name);
                        Console.Write(' ');
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine(sb.ToString());
            if (parser.errors.count > 0)
                return 1;
        } else {
            Console.WriteLine("usage: Inheritance.exe file");
            return 99;
        }
        return 0;
    }
}