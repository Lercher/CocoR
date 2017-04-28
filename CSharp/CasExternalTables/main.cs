using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;


public class Inheritance {

    static void printST(Symboltable st) {
        Console.WriteLine("--- {0,-12} ------------------------------------------------------------------- ", st.name, st.CountScopes, st.ignoreCase ? " IGNORECASE" : "");
        int n = 0;
        foreach (Token t in st.currentScope) {
            n++;
            string s = string.Format("{0}", t.val, t.line, t.col);
            Console.Write("{0,-20}  ", s);
            if (n%4 == 0) Console.WriteLine(); 
        }
        if (n%4 != 0) Console.WriteLine();
        Console.WriteLine();
    }

    static void Line() {
        Console.WriteLine("----------------------------------------------------------------- ");
    }

	public static int Main (string[] arg) {
        Line();
		Console.WriteLine("--- Parsing 'update_externaltable.sql syntax'");
        Line();
        if (arg.Length >= 1)
        {
            Console.WriteLine("--- scanning {0} ...", arg[0]);
            Scanner scanner = new Scanner(arg[0], true); // is UTF8 source
			Parser parser = new Parser(scanner);
            for (var i = 1; i < arg.Length; i++) {
                int minus = 0;
                if (int.TryParse(arg[i], out minus) && minus < 0) {
                    parser.suppressed.Add(-minus);
                    Console.WriteLine("---   suppressing diagnostic #{0}", -minus);
                }
            }
            parser.Parse();
            var qy = from kv in parser.diagnostics orderby kv.Key select kv; 
            Line();
            foreach (var kv in qy)
                Console.WriteLine("--- #{0,2}: {1,5:n0}", kv.Key, kv.Value);
            Console.WriteLine("--- {0} error(s) detected", parser.errors.count);

            // list all known symbol table values (see section SYMBOLTABLES in the *.atg file)
            printST(parser.languages);
            printST(parser.deletabletables);
            printST(parser.updatetables);
            printST(parser.columns);
            printST(parser.chrarguments);

            if (parser.errors.count > 0)
                return 1;
        } else {
            Console.WriteLine("usage: CasExternalTables.exe file-path [-# ...]");
            Console.WriteLine("       -# where # is an int: switch off diagnostic message #");
            return 99;
        }
        return 0;
    }
}