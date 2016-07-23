using System;
using System.IO;

public class Inheritance {

	public static int Main (string[] arg) {
		Console.WriteLine("Inheritance parser");
        if (arg.Length == 1)
        {
            Console.WriteLine("scanning {0} ...", arg[0]);
            Scanner scanner = new Scanner(arg[0]);
			Parser parser = new Parser(scanner);
            parser.Parse();
            Console.WriteLine("{0} error(s) detected", parser.errors.count);
            if (parser.errors.count > 0)
                return 1;
        } else {
            Console.WriteLine("usage: Inheritance.exe file");
            return 99;
        }
        return 0;
    }
}