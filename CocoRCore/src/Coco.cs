/*-------------------------------------------------------------------------
  Trace output options
  0 | A: prints the states of the scanner automaton
  1 | F: prints the First and Follow sets of all nonterminals
  2 | G: prints the syntax graph of the productions
  3 | I: traces the computation of the First sets
  4 | J: prints the sets associated with ANYs and synchronisation sets
  6 | S: prints the symbol table (terminals, nonterminals, pragmas)
  7 | X: prints a cross reference list of all syntax symbols
  8 | P: prints statistics about the Coco run
  
  Trace output can be switched on by the pragma
    $ { digit | letter }
  in the attributed grammar or as a command-line option
  -------------------------------------------------------------------------*/

using System;
using System.IO;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{

    public class Coco
    {

        public static int Main(string[] arg)
        {
            Console.Write("Coco/R Core (May 14, 2017)");
            string srcName = null, nsName = null, frameDir = null, ddtString = null,
            traceFileName = null, outDir = null;
            bool emitLines = false, generateAutocompleteInformation = false, ignoreSemanticActions = false,
            isUTF8 = false;
            int retVal = 1;
            for (int i = 0; i < arg.Length; i++)
            {
                if (arg[i] == "-namespace" && i < arg.Length - 1) nsName = arg[++i].Trim();
                else if (arg[i] == "-frames" && i < arg.Length - 1) frameDir = arg[++i].Trim();
                else if (arg[i] == "-trace" && i < arg.Length - 1) ddtString = arg[++i].Trim();
                else if (arg[i] == "-o" && i < arg.Length - 1) outDir = arg[++i].Trim();
                else if (arg[i] == "-lines") emitLines = true;
                else if (arg[i] == "-ac") generateAutocompleteInformation = true;
                else if (arg[i] == "-is") ignoreSemanticActions = true;
                else if (arg[i] == "-utf8") isUTF8 = true;
                else srcName = arg[i];
            }
            if (emitLines) Console.Write(" [emit lines]");
            if (generateAutocompleteInformation) Console.Write(" [generate autocomplete information]");
            if (ignoreSemanticActions) Console.Write(" [ignore semantic actions]");
            if (isUTF8) Console.Write(" [forced UTF8 processing]");
            Console.WriteLine();
            Console.WriteLine(srcName);
            if (arg.Length > 0 && srcName != null)
            {
                try
                {
                    string srcDir = Path.GetDirectoryName(srcName);

                    var scanner = Scanner.Create(srcName, isUTF8);
                    var parser = new Parser(scanner);

                    traceFileName = Path.Combine(srcDir, "trace.txt");
                    parser.trace = new StreamWriter(new FileStream(traceFileName, FileMode.Create));
                    parser.tab = new Tab(parser);
                    parser.dfa = new DFA(parser);
                    parser.pgen = new ParserGen(parser);
                    parser.pgen.GenerateAutocompleteInformation = generateAutocompleteInformation;
                    parser.pgen.IgnoreSemanticActions = ignoreSemanticActions;

                    parser.tab.srcName = srcName;
                    parser.tab.srcDir = srcDir;
                    parser.tab.nsName = nsName;
                    parser.tab.frameDir = frameDir;
                    parser.tab.outDir = (outDir != null) ? outDir : srcDir;
                    parser.tab.emitLines = emitLines;
                    if (ddtString != null) parser.tab.SetDDT(ddtString);

                    parser.Parse();

                    Console.WriteLine("grammar scanned by using {0}", scanner.buffer.GetType().Name);
                    parser.trace.Dispose();
                    FileInfo f = new FileInfo(traceFileName);
                    if (f.Length == 0) f.Delete();
                    else Console.WriteLine("trace output is in " + traceFileName);
                    Console.WriteLine("{0} error(s) detected", parser.errors.Count);
                    if (parser.errors.Count == 0) { retVal = 0; }
                }
                catch (IOException)
                {
                    Console.WriteLine("-- could not open " + traceFileName);
                }
                catch (FatalError e)
                {
                    Console.WriteLine("-- " + e.Message);
                }
            }
            else
            {
                Console.WriteLine("Usage: Coco Grammar.ATG {{Option}}{0}"
                                  + "Options:{0}"
                                  + "  -namespace <namespaceName>{0}"
                                  + "  -frames    <frameFilesDirectory>{0}"
                                  + "  -trace     <traceString>{0}"
                                  + "  -o         <outputDirectory>{0}"
                                  + "  -lines     [emit lines]{0}"
                                  + "  -ac        [generate autocomplete/intellisense information]{0}"
                                  + "  -is        [ignore semantic actions]{0}"
                                  + "  -utf8      [force UTF-8 processing, even without BOM]{0}"
                                  + "Valid characters in the trace string:{0}"
                                  + "  A  trace automaton{0}"
                                  + "  F  list first/follow sets{0}"
                                  + "  G  print syntax graph{0}"
                                  + "  I  trace computation of first sets{0}"
                                  + "  J  list ANY and SYNC sets{0}"
                                  + "  P  print statistics{0}"
                                  + "  S  list symbol table{0}"
                                  + "  X  list cross reference table{0}"
                                  + "Scanner.frame and Parser.frame files needed in ATG directory{0}"
                                  + "or in a directory specified in the -frames option.",
                                  Environment.NewLine);
            }
            return retVal;
        }

    } // end Coco

} // end namespace
