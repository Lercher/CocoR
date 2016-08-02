using System;
using System.IO;

public class Inheritance {

    static void printST(Symboltable st) {
        if (st == null) return;
        Console.WriteLine("--- symbol-table{2} ------------------------------------------------------------------- {0}({1})", st.name, st.CountScopes, st.ignoreCase ? " IGNORECASE" : "");
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
            printST(parser.lang);
            printST(parser.domains);

            if (parser.errors.count > 0)
                return 1;
        } else {
            Console.WriteLine("usage: CASDomains.exe file");
            return 99;
        }
        return 0;
    }
}