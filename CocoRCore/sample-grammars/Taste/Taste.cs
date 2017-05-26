using System;

namespace CocoRCore.Samples.Taste
{
    public static class Taste
    {

        public static ParserBase Create(string arg)
        {
            Scanner scanner = new Scanner().Initialize(arg);
            Parser parser = new Parser(scanner);
            parser.tab = new SymbolTable(parser);
            parser.gen = new CodeGenerator();
            return parser;
        }
    }

} // end namespace
