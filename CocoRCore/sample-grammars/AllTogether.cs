using System;
using System.IO;
using System.Linq;

namespace CocoRCore.Samples
{
    public class Sample
    {
        public static int Main(string[] arg)
        {
            Console.WriteLine("Coco/R Core Samples (May 29, 2017)");
            var all = new Func<CocoRCore.ParserBase>[] {
                () => Coco.Parser.Create(s => s.Initialize("Coco/Coco.atg")),
                () => Taste.Taste.Create("Taste/Test.tas"),
                () => Inheritance.Parser.Create("Inheritance/SampleInheritance.txt"),
                () => WFModel.Parser.Create("wfmodel/SampleWfModel.txt"),
                () => ExternalTables.Parser.Create("ExternalTables/ExternalTables.txt"),
            };
            foreach (var pgen in all)
            {
                var parser = pgen();
                try
                {
                    parser.errors.UseShortDiagnosticFormat();
                    // parser.errors.errorStream = Console.Out;
                    parser.Parse();
                    foreach (var e in parser.errors)
                        Console.Write(e.Format("", "")); // this is only to see if it compiles, it doesn't output anything
                    Console.WriteLine("{0}: {1:f}.", parser.scanner.uri, parser.errors);
                    if (parser.errors.CountError == 0)
                    {
                        foreach(var t in parser.tokens.Take(10))
                        {
                            Console.WriteLine($"  Token {t.t.Range()} = {t.t.valScanned}");
                        }
                    }
                }
                catch (FatalError ex)
                {
                    Console.WriteLine("-- {0}", ex.Message);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("-- {0}: {1}", parser.scanner.uri, ex);
                }
            }
            return 0;
        }
    }
}
