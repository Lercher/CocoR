using System;
using System.IO;

namespace CocoRCore.Samples
{
    public class Sample
    {
        public static int Main(string[] arg)
        {
            Console.WriteLine("Coco/R Core Samples (May 19, 2017)");
            var all = new Func<CocoRCore.ParserBase>[] {
                () => new Coco.Parser(Coco.Scanner.Create(@"Coco\Coco.atg", true)),
                () => new Taste.Parser(Taste.Scanner.Create(@"Taste\Test.tas", true)),
                () => new Inheritance.Parser(Inheritance.Scanner.Create(@"Inheritance\SampleInheritance.txt", true)),
                () => new WFModel.Parser(WFModel.Scanner.Create(@"WFModel\SampleWFModel.txt", true))
            };
            foreach (var pgen in all)
            {
                try
                {
                    var parser = pgen();
                    parser.Parse();
                    foreach(var e in parser.errors)
                        Console.Write(e.Format("", "")); // this is only to see if it compiles, it doesn't output anything
                    Console.WriteLine("{0}: {1} error(s), {2} warning(s).", parser.scanner.uri, parser.errors.CountError, parser.errors.CountWarning);
                }
                catch (FatalError ex) 
                {
                    Console.WriteLine("-- {0}", ex.Message);
                }
                catch (System.Exception ex)
                {                    
                    Console.WriteLine("-- {0}", ex);
                }
            }
            return 0;
        }
    }
}
