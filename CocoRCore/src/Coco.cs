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
            Console.Write("Coco/R Core (May 31, 2017)");
            string srcName = null, nsName = null, frameDir = null, ddtString = null,
            traceFileName = null, outDir = null;
            var emitLines = false;
            var generateAutocompleteInformation = false;
            var ignoreSemanticActions = false;
            var createOld = false;
            var enableWarnings = true;
            var enableInfos = true;
            var useShort = false;

            var retVal = 1;
            for (var i = 0; i < arg.Length; i++)
            {
                if (false) {}
                else if  (arg[i] == "-namespace" && i < arg.Length - 1) nsName = arg[++i].Trim();
                else if  (arg[i] == "-frames"    && i < arg.Length - 1) frameDir = arg[++i].Trim();
                else if  (arg[i] == "-trace"     && i < arg.Length - 1) ddtString = arg[++i].Trim();
                else if  (arg[i] == "-o"         && i < arg.Length - 1) outDir = arg[++i].Trim();
                else if  (arg[i] == "-lines") emitLines = true;
                else if  (arg[i] == "-ac") generateAutocompleteInformation = true;
                else if  (arg[i] == "-is") ignoreSemanticActions = true;
                else if  (arg[i] == "-old") createOld = true;
                else if  (arg[i] == "-nowarn") enableWarnings = false;
                else if  (arg[i] == "-noinfo") enableInfos = false;
                else if  (arg[i] == "-short") useShort = true;
                else if (!arg[i].StartsWith("-")) srcName = arg[i];
                else Console.Write($" [arg{i}: {arg[i]} ignored]");
            }
            if (!enableWarnings) Console.Write(" [no WARN]");
            if (!enableInfos) Console.Write(" [no INFO]");
            if (emitLines) Console.Write(" [emit #line directives]");
            if (generateAutocompleteInformation) Console.Write(" [with autocomplete]");
            if (ignoreSemanticActions) Console.Write(" [ignore semantic actions]");
            if (createOld) Console.Write(" [*.old files]");
            if (useShort) Console.Write(" [short message format]");

            if (arg.Length > 0 && srcName != null)
            {
                try
                {
                    var src = new FileInfo(srcName);
                    var srcDir = src.DirectoryName;
                    outDir = outDir ?? srcDir;
                    traceFileName = Path.Combine(outDir, "trace.txt");

                    var parser = Parser.Create();
                    parser.trace = File.CreateText(traceFileName);
                    parser.tab = new Tab(parser);
                    parser.dfa = new DFA(parser);
                    parser.pgen = new ParserGen(parser);
                    parser.pgen.GenerateAutocompleteInformation = generateAutocompleteInformation;
                    parser.pgen.IgnoreSemanticActions = ignoreSemanticActions;
                    parser.errors.Writer = Console.Out;
                    parser.errors.DiagnosticIdPrefix = "ATG";
                    parser.errors.EnableWarnings = enableWarnings;
                    parser.errors.EnableInfos = enableInfos;
                    if (useShort) parser.errors.UseShortDiagnosticFormat();
                    parser.tab.srcName = srcName;
                    parser.tab.srcDir = srcDir;
                    parser.tab.nsName = nsName;
                    parser.tab.frameDir = frameDir;
                    parser.tab.outDir = outDir;
                    parser.tab.emitLines = emitLines;
                    parser.tab.createOld = createOld;
                    parser.tab.SetDDT(ddtString);

                    Console.Write("  ");
                    Console.WriteLine(src.FullName);
                    parser.scanner.Initialize(src);
                    parser.Parse();
                    parser.Dispose();
                    Console.WriteLine(parser.errors);

                    var trc = new FileInfo(traceFileName);
                    if (trc.Length == 0) 
                        trc.Delete();
                    else 
                        Console.WriteLine("trace output is in " + trc.FullName);
                    
                    if (parser.errors.CountError == 0) 
                        retVal = 0;
                }
                catch (FatalError ex)
                {
                    Console.WriteLine("***FATAL** " + ex.Message);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("***IO*** " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"*** {ex.Message} ***\n{ex}");
                }   
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Usage: CocoRCore grammar.atg {{Option}}{0}"
                                  + "Options with a parameter:{0}"
                                  + "  -namespace <namespaceName>{0}"
                                  + "  -frames <frameFilesDirectory>{0}"
                                  + "  -trace <traceString>          [trace.txt file at *.ATG location]{0}"
                                  + "  -o <outputDirectory>          [defaults to *.ATG location]{0}"
                                  + "Other options:{0}"
                                  + "  -lines  [emit #line directives]{0}"
                                  + "  -ac     [generate autocomplete/intellisense information]{0}"
                                  + "  -is     [ignore semantic actions]{0}"
                                  + "  -old    [create *.old files]{0}"
                                  + "  -nowarn [disable all warnings]{0}"
                                  + "  -noinfo [disable all informational messages]{0}"
                                  + "  -short  [use short diagnostic format]{0}"
                                  + "Valid characters in the <traceString>:{0}"
                                  + "  A  trace automaton{0}"
                                  + "  F  list first/follow sets{0}"
                                  + "  G  print syntax graph{0}"
                                  + "  I  trace computation of first sets{0}"
                                  + "  J  list ANY and SYNC sets{0}"
                                  + "  P  print statistics{0}"
                                  + "  S  list symbol table{0}"
                                  + "  X  list cross reference table{0}"
                                  + "Scanner.frame and Parser.frame files needed in ATG directory{0}"
                                  + "or in a directory specified in the -frames <frameFilesDirectory> option.",
                                  Environment.NewLine);
            }
            return retVal;
        }

    } // end Coco

} // end namespace
