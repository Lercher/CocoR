using System;
using System.IO;
using CocoRCore;

namespace CocoRCore.Samples
{
    public class Sample
    {
        public static int Main(string[] arg)
        {
            Console.WriteLine("Coco/R Core Samples (May 19, 2017)");
            var all = new Func<ParserBase>[] {
                () => new Coco.Parser(Coco.Scanner.Create(@"Coco\Coco.atg", true)),
                () => new Taste.Parser(Taste.Scanner.Create(@"Taste\SampleTaste.txt", true)),
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
                        Console.WriteLine(e.message);
                }
                catch (System.Exception ex)
                {                    
                    Console.WriteLine("-- {0}", ex.Message);
                }
            }
            return 0;
        }
    }
}
