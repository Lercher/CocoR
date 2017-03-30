using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


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
		Console.WriteLine("Parse update_externaltable.sq syntax");
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

            // System.Console.WriteLine("----------------------- AST builder stack ----------------------------");
            // System.Console.WriteLine(parser.astbuilder);

            // System.Console.WriteLine("----------------------- AST ----------------------------");
            // System.Console.WriteLine(parser.ast);

            if (parser.errors.count > 0)
                return 1;
        } else {
            Console.WriteLine("usage: CasExternalTables.exe file");
            return 99;
        }
        return 0;
    }
}