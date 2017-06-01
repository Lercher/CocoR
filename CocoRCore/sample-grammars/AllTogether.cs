using System;
using System.IO;
using System.Linq;

namespace CocoRCore.Samples
{
    public class Sample
    {
        public static int Main(string[] arg)
        {
            Console.WriteLine("Coco/R Core Samples (June 01, 2017)");
            var all = new Func<CocoRCore.ParserBase>[] {
                () => Coco.Parser.Create(s => s.Initialize("Coco/Coco.atg")),
                () => Taste.Taste.Create("Taste/Test.tas"),
                () => Inheritance.Parser.Create("Inheritance/SampleInheritance.txt"),
                () => WFModel.Parser.Create("wfmodel/SampleWfModel.txt"),
                () => ExternalTables.Parser.Create("ExternalTables/ExternalTables.txt"),
                () => CodeLens.Parser.Create("CodeLens/CodeLens.txt")
            };
            foreach (var pgen in all)
            {
                var parser = pgen();
                try
                {
                    parser.errors.UseShortDiagnosticFormat();
                    // parser.errors.errorStream = Console.Out;
                    parser.Parse();
                    parser.Dispose();
                    foreach (var e in parser.errors)
                        Console.Write(e.Format("", "")); // this is only to see if it compiles, it doesn't output anything
                    Console.WriteLine("{0}: {1:f}.", parser.scanner.uri, parser.errors);
                    if (parser.errors.CountError == 0)
                    {
                        foreach(var a in parser.AlternativeTokens.Take(10))
                        {
                            Console.WriteLine($"  Token {a.t.Range()} = {a.t.valScanned}. Alternatives:");
                            System.Console.Write("      keywords:");
                            var n = 0;
                            for (var kind = 0; kind < a.alt.Length; kind++)
                            {
                                if (a.alt[kind])
                                {
                                    n++; if (n >= 6) 
                                    {
                                        Console.Write(" ...");
                                        break;
                                    }
                                    Console.Write($" {parser.NameOfTokenKind(kind)}");
                                }
                            }
                            System.Console.WriteLine();
                            foreach (var fst in a.symbols)
                            {
                                Console.WriteLine($"      {fst.Name}: {string.Join("|", fst.Items)}");
                            }
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
