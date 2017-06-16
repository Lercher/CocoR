using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{

    public enum ErrorCodes
    {
        anyErr = 1,
        syncErr = 2
    }

    public class ParserGen
    {
        const string PROD_SUFFIX = "‿NT"; // U+203F UNDERTIE, see http://www.fileformat.info/info/unicode/category/Pc/list.htm 
        const int maxTerm = 3;      // sets of size < maxTerm are enumerated
        const char CR = '\r';
        const char LF = '\n';
        const int EOF = -1;


        public Range usingPos; // "using" definitions from the attributed grammar
        public bool GenerateAutocompleteInformation = false;  // generate addAlt() calls to fill the "alt" set with alternatives to the next to Get() token.
        public bool IgnoreSemanticActions = false;
        public bool needsAST = false;

        private Generator Gen; // generator for parser source file

        private List<string> Errors = new List<string>(); // generated parser error messages


        private Symbol CurrentNtSym;     // symbol whose production is currently generated
        private List<BitArray> symSet = new List<BitArray>();

        public readonly Parser parser;                    // other Coco objects
        private TextWriter Trace => parser.trace;
        private Tab Tab => parser.tab;



        bool Overlaps(BitArray s1, BitArray s2)
        {
            var len = s1.Length;
            for (var i = 0; i < len; ++i)
            {
                if (s1[i] && s2[i])
                {
                    return true;
                }
            }
            return false;
        }

        // use a switch if 
        //   more than 5 alternatives 
        //   and none starts with a resolver
        //   and no LL1 warning
        bool UseSwitch(Node p)
        {
            if (p.typ != NodeKind.alt) return false;
            var nAlts = 0;
            var s1 = new BitArray(Tab.terminals.Count);
            for (var pp = p; pp != null; pp = pp.down)
            {
                var s2 = Tab.Expected0(pp.sub, CurrentNtSym);
                // must not optimize with switch statement, if there are ll1 warnings
                if (Overlaps(s1, s2))
                    return false;
                s1.Or(s2);
                ++nAlts;
                // must not optimize with switch-statement, if alt uses a resolver expression
                if (pp.sub.typ == NodeKind.rslv)
                    return false;
            }
            return nAlts > 5;
        }

        void CopySourcePart(Range range, bool indent)
        {
            if (IgnoreSemanticActions || range == null)
                return;
            var s = parser.scanner.buffer.GetBufferedString(range);
            if (!indent)
            {
                // s only contains LFs (0x0A). For Windows we need "\n", i.e. CRLF (0x0D, 0x0A)
                // so we split s as LFs, trim trailing spaces and CRs
                var lines = s.Split((char)ScannerBase.EOL);
                for (var i = 0; i < lines.Length; i++)
                {
                    if (i > 0)
                        Gen.Write(GW.EndLine, string.Empty);
                    var line = lines[i].TrimEnd();
                    Gen.Write(GW.Append, line);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(s))
                    return;
                var lines = s.Split((char)ScannerBase.EOL);
                var n = range.start.line;
                foreach (var l in lines)
                {
                    if (!string.IsNullOrWhiteSpace(l))
                    {
                        if (Tab.emitLines)
                        {
                            Gen.Write(GW.Line, "#line {0} \"{1}\"", n, Tab.srcName);
                        }
                        Gen.Write(GW.Line, l.Trim());
                    }
                    n++;
                }
            }
        }


        void GenErrorMsg(string escaped) => Errors.Add($"case {Errors.Count + 1}: return \"{escaped}\";");

        void GenTerminalErrorMsg(Symbol tSym)
        {
            var sn = Tab.Escape(tSym.VariantName);
            GenErrorMsg($"{sn} expected");
        }

        void GenErrorMsg(ErrorCodes errTyp, Symbol sym)
        {
            var sn = Tab.Escape(sym.name);
            switch (errTyp)
            {
                case ErrorCodes.anyErr:
                    GenErrorMsg($"invalid {sn} (ANY error)");
                    break;
                case ErrorCodes.syncErr:
                    GenErrorMsg($"symbol not expected in {sn} (SYNC error)");
                    break;
            }
        }

        void GenAltErrorMsg(Node p)
        {
            var ht = new HashSet<string>();
            for (var p2 = p; p2 != null; p2 = p2.down)
            {
                var s1 = Tab.Expected(p2.sub, CurrentNtSym);
                // we probably don't need BitArray s = DerivationsOf(s0); here
                foreach (var sym in Tab.terminals)
                    if (s1[sym.n])
                        ht.Add(sym.VariantName);
            }
            var sb = new StringBuilder();
            var sn = Tab.Escape(CurrentNtSym.name);
            sb.AppendFormat("invalid {0}, expected", sn);
            foreach (var s in ht)
                sb.AppendFormat(" {0}", Tab.Escape(s));
            GenErrorMsg(sb.ToString());
            // gen and use the std msg:
            // GenErrorMsg(ErrorCodes.altErr, CurrentNtSym);
        }

        int NewCondSet(BitArray s)
        {
            for (var i = 1; i < symSet.Count; i++) // skip symSet[0] (reserved for union of SYNC sets)
                if (Sets.Equals(s, (BitArray)symSet[i])) return i;
            symSet.Add(Clone(s));
            return symSet.Count - 1;
        }

        private static BitArray Clone(BitArray arr)
        {
            if (arr == null) return null;
            return new BitArray(arr);
        }

        // for autocomplete/intellisense
        // same as GenCond(), but we only notfiy the 'alt' list of alternatives of new members		
        void GenAutocomplete(BitArray s, Node p, string comment)
        {
            if (!GenerateAutocompleteInformation) return; // we don't want autocomplete information in the parser
            if (p.typ == NodeKind.rslv) return; // if we have a resolver, we don't know what to do (yet), so we do nothing
            var c = s.ElementCount();
            if (c == 0) return;
            if (c > maxTerm)
            {
                Gen.Write(GW.Line, "addAlt(set0, {0}); // {1}", NewCondSet(s), comment);
            }
            else
            {
                Gen.Write(GW.StartLine, "addAlt(");
                if (c > 1) Gen.Write(GW.Append, "new int[] {");
                var n = 0;
                foreach (var sym in Tab.terminals)
                {
                    if (s[sym.n])
                    {
                        n++;
                        if (n > 1) Gen.Write(GW.Append, ", ");
                        Gen.Write(GW.Append, sym.n.ToString());
                        // note: we don't need to take sym.inherits or isKind() into account here
                        // because we only want to see alternatives as specified in the parser productions.
                        // So a keyword:indent = "keyword". token spec will produce only an "ident" variant
                        // and not a "keyword" as well as an "ident".
                    }
                }
                if (c > 1) Gen.Write(GW.Append, "}");
                Gen.Write(GW.EndLine, "); // {0}", comment);
            }
        }

        void GenAutocomplete(int kind, string comment)
        {
            if (!GenerateAutocompleteInformation) return; // we don't want autocomplete information in the parser
            Gen.Write(GW.Line, "addAlt({0}); // {1}", kind, comment);
        }


        void GenCond(BitArray s, Node p)
        {
            if (p.typ == NodeKind.rslv)
                CopySourcePart(p.pos, indent: false);
            else
            {
                var n = s.ElementCount();
                if (n == 0)
                    Gen.Write(GW.Append, "false"); // happens if an ANY set matches no symbol
                else if (n <= maxTerm)
                {
                    foreach (var sym in Tab.terminals)
                    {
                        if (s[sym.n])
                        {
                            Gen.Write(GW.Append, "isKind(la, {0} {1})", sym.n, sym.CSharpCommentName);
                            --n;
                            if (n > 0) Gen.Write(GW.Append, " || ");
                        }
                    }
                }
                else
                {
                    Gen.Write(GW.Append, "StartOf({0})", NewCondSet(s));
                }
            }
        }

        void PutCaseLabels(BitArray s0)
        {
            var s = DerivationsOf(s0);
            foreach (var sym in Tab.terminals)
                if (s[sym.n])
                    Gen.Write(GW.Line, "case {0}: {1}", sym.n, sym.CSharpCommentName);
        }

        BitArray DerivationsOf(BitArray s0)
        {
            var s = Clone(s0);
            var done = false;
            while (!done)
            {
                done = true;
                foreach (var sym in Tab.terminals)
                {
                    if (s[sym.n])
                    {
                        foreach (var baseSym in Tab.terminals)
                        {
                            if (baseSym.inherits == sym && !s[baseSym.n])
                            {
                                s[baseSym.n] = true;
                                done = false;
                            }
                        }
                    }
                }
            }
            return s;
        }

        void GenSymboltableCheck(Node p)
        {
            if (!string.IsNullOrEmpty(p.declares))
            {
                Gen.Write(GW.Line, "if (!{0}.Add(la)) SemErr(71, string.Format(DuplicateSymbol, {1}, la.val, {0}.name), la);", p.declares, Tab.Quoted(p.sym.name));
                Gen.Write(GW.Line, "alternatives.stdeclares = {0};", p.declares);
            }
            else if (!string.IsNullOrEmpty(p.declared))
            {
                Gen.Write(GW.Line, "if (!{0}.Use(la, alternatives)) SemErr(72, string.Format(MissingSymbol, {1}, la.val, {0}.name), la);", p.declared, Tab.Quoted(p.sym.name));
            }
        }

        void GenAutocompleteSymboltable(Node p, string comment)
        {
            if (!GenerateAutocompleteInformation) return;
            if (!string.IsNullOrEmpty(p.declared))
            {
                Gen.Write(GW.Line, "addAlt({0}, {1}); // {3} {2} uses symbol table '{1}'", p.sym.n, p.declared, p.sym.name, comment);
            }
        }

        void GenAstBuilder(Node p)
        {
            if (needsAST && p.asts != null)
                foreach (var astOp in p.asts)
                    Gen.Write(GW.Line, "using(astbuilder.createMarker({0}, {1}, {2}, {3}, {4}))", Tab.Quoted(astOp.name), Tab.Quoted(astOp.literal), ToTF(astOp.isList), ToTF(astOp.ishatch), ToTF(astOp.primed));
        }

        void GenCode(Node pn, BitArray isChecked)
        {
            BitArray s1, s2;
            for (var p = pn; p != null; p = p.next)
            {
                switch (p.typ)
                {
                    case NodeKind.nt:
                        // generate a production method call ...
                        GenAstBuilder(p);
                        Gen.Write(GW.StartLine, "{0}{1}(", p.sym.name, PROD_SUFFIX);
                        CopySourcePart(p.pos, indent: false); // ... with actual arguments
                        Gen.Write(GW.EndLine, ");");
                        break;
                    case NodeKind.t:
                        GenSymboltableCheck(p);
                        // assert: if isChecked[p.sym.n] is true, then isChecked contains only p.sym.n
                        if (isChecked[p.sym.n])
                        {
                            GenAstBuilder(p);
                            Gen.Write(GW.Line, "Get();");
                        }
                        else
                        {
                            GenAutocomplete(p.sym.n, "T " + p.sym.name);
                            GenAutocompleteSymboltable(p, "T " + p.sym.name);
                            GenAstBuilder(p);
                            Gen.Write(GW.Line, "Expect({0} {1});", p.sym.n, p.sym.CSharpCommentName);
                        }
                        break;
                    case NodeKind.wt:
                        GenSymboltableCheck(p);
                        s1 = Tab.Expected(p.next, CurrentNtSym);
                        s1.Or(Tab.allSyncSets);
                        var ncs1 = NewCondSet(s1);
                        var ncs1sym = Tab.terminals[ncs1];
                        GenAutocomplete(p.sym.n, "WT " + p.sym.name);
                        GenAutocompleteSymboltable(p, "WT " + p.sym.name);
                        GenAstBuilder(p);
                        Gen.Write(GW.Line, "ExpectWeak({0} {2}, {1} {3}); // {0} followed by {1}", p.sym.n, ncs1, p.sym.CSharpCommentName, ncs1sym.CSharpCommentName);
                        break;
                    case NodeKind.any:
                        var acc = p.set.ElementCount();
                        if (Tab.terminals.Count == (acc + 1) || (acc > 0 && Sets.Equals(p.set, isChecked)))
                        {
                            // either this ANY accepts any terminal (the + 1 = end of file), or exactly what's allowed here
                            Gen.Write(GW.Line, "Get();");
                        }
                        else
                        {
                            GenErrorMsg(ErrorCodes.anyErr, CurrentNtSym);
                            if (acc > 0)
                            {
                                GenAutocomplete(p.set, p, "ANY");
                                Gen.Write(GW.StartLine, "if (");
                                GenCond(p.set, p);
                                Gen.Write(GW.EndLine, ")");
                                Gen.Write(GW.LineIndent1, "Get();");
                                Gen.Write(GW.Line, "else");
                                Gen.Write(GW.LineIndent1, "SynErr({0});", Errors.Count);
                            }
                            else
                                Gen.Write(GW.Line, "SynErr({0}); // ANY node that matches no symbol", Errors.Count);
                        }
                        break;
                    case NodeKind.eps:
                        break; // nothing
                    case NodeKind.rslv:
                        break; // nothing
                    case NodeKind.sem:
                        CopySourcePart(p.pos, indent: true); // semantic action
                        break;
                    case NodeKind.sync:
                        GenErrorMsg(ErrorCodes.syncErr, CurrentNtSym);
                        s1 = Clone(p.set);
                        Gen.Write(GW.StartLine, "while (!(");
                        GenCond(s1, p);
                        Gen.Write(GW.EndLine, "))");
                        Gen.Write(GW.Line, "{");
                        Gen.Write(GW.LineIndent1, "SynErr({0});", Errors.Count);
                        Gen.Write(GW.LineIndent1, "Get();", Errors.Count);
                        Gen.Write(GW.Line, "}");
                        break;
                    case NodeKind.alt:
                        s1 = Tab.First(p);
                        var equal = Sets.Equals(s1, isChecked);

                        // intellisense / autocomplete
                        for (var pd = p; pd != null; pd = pd.down)
                        {
                            s1 = Tab.Expected(pd.sub, CurrentNtSym);
                            GenAutocomplete(s1, pd.sub, "ALT");
                            GenAutocompleteSymboltable(pd.sub, "ALT");
                        }
                        // end intellisense

                        var useSwitch = UseSwitch(p);
                        if (useSwitch)
                        {
                            Gen.Write(GW.Line, "switch (la.kind)");
                            Gen.Write(GW.Line, "{");
                            Gen.Indentation++;
                        }
                        for (var pp = p; pp != null; pp = pp.down)
                        {
                            s1 = Tab.Expected(pp.sub, CurrentNtSym);
                            if (useSwitch)
                            {
                                PutCaseLabels(s1);   // case x:, case y:
                                Gen.Indentation++;
                                Gen.Write(GW.Line, "{ // scoping"); // for a semantic's scoping
                                Gen.Indentation++;
                            }
                            else if (pp == p)
                            {
                                Gen.Write(GW.StartLine, "if (");
                                GenCond(s1, pp.sub);
                                Gen.Write(GW.EndLine, ")");
                                Gen.Write(GW.Line, "{");
                                Gen.Indentation++;
                            }
                            else if (pp.down == null && equal)
                            {
                                Gen.Indentation--;
                                Gen.Write(GW.Line, "}");
                                Gen.Write(GW.Line, "else");
                                Gen.Write(GW.Line, "{");
                                Gen.Indentation++;
                            }
                            else
                            {
                                Gen.Indentation--;
                                Gen.Write(GW.Line, "}");
                                Gen.Write(GW.StartLine, "else if (");
                                GenCond(s1, pp.sub);
                                Gen.Write(GW.EndLine, ")");
                                Gen.Write(GW.Line, "{");
                                Gen.Indentation++;
                            }
                            GenCode(pp.sub, s1);
                            if (useSwitch)
                            {
                                Gen.Indentation--;
                                Gen.Write(GW.Line, "}");
                                Gen.Write(GW.Line, "break;");
                                Gen.Indentation--;
                            }
                        }
                        if (equal)
                        {
                            Gen.Indentation--;
                            Gen.Write(GW.Line, "}");
                        }
                        else
                        {
                            GenAltErrorMsg(p);
                            if (useSwitch)
                            {
                                Gen.Write(GW.Line, "default:");
                                Gen.Write(GW.LineIndent1, "SynErr({0});", Errors.Count);
                                Gen.Write(GW.LineIndent1, "break;", Errors.Count);
                                Gen.Indentation--; // is effectively two tabs back
                                Gen.Write(GW.Line, "} // end switch");
                            }
                            else
                            {
                                Gen.Indentation--;
                                Gen.Write(GW.Line, "} // end if");
                                Gen.Write(GW.Line, "else");
                                Gen.Write(GW.LineIndent1, "SynErr({0});", Errors.Count);
                            }
                        }
                        break;
                    case NodeKind.iter:
                        var p2 = p.sub;
                        var pac = p2;
                        var sac = Tab.First(pac);
                        GenAutocomplete(sac, pac, "ITER start");
                        GenAutocompleteSymboltable(pac, "ITER start");
                        Gen.Write(GW.StartLine, "while (");
                        if (p2.typ == NodeKind.wt)
                        {
                            s1 = Tab.Expected(p2.next, CurrentNtSym);
                            s2 = Tab.Expected(p.next, CurrentNtSym);
                            Gen.Write(GW.Append, "WeakSeparator({0} {3}, {1}, {2}) ", p2.sym.n, NewCondSet(s1), NewCondSet(s2), p2.sym.CSharpCommentName);
                            s1 = new BitArray(Tab.terminals.Count);  // for inner structure
                            if (p2.up || p2.next == null)
                                p2 = null;
                            else
                                p2 = p2.next;
                        }
                        else
                        {
                            s1 = Tab.First(p2);
                            GenCond(s1, p2);
                        }
                        Gen.Write(GW.EndLine, ")");
                        Gen.Write(GW.Line, "{");
                        Gen.Indentation++;
                        GenCode(p2, s1);
                        GenAutocomplete(sac, pac, "ITER end");
                        GenAutocompleteSymboltable(pac, "ITER end");
                        Gen.Indentation--;
                        Gen.Write(GW.Line, "}");
                        break;
                    case NodeKind.opt:
                        s1 = Tab.First(p.sub);
                        GenAutocomplete(s1, p.sub, "OPT");
                        Gen.Write(GW.StartLine, "if (");
                        GenCond(s1, p.sub);
                        Gen.Write(GW.EndLine, ")");
                        Gen.Write(GW.Line, "{");
                        Gen.Indentation++;
                        GenCode(p.sub, s1);
                        Gen.Indentation--;
                        Gen.Write(GW.Line, "}");
                        break;
                }
                if (p.typ != NodeKind.eps && p.typ != NodeKind.sem && p.typ != NodeKind.sync)
                    isChecked.SetAll(false);  // = new BitArray(tab.terminals.Count);
                if (p.up)
                    break;
            }
        }

        void GenTokens()
        {
            foreach (var sym in Tab.terminals)
            {
                if (char.IsLetter(sym.name[0]) && sym.name != "EOF")
                    Gen.Write(GW.Line, "public const int _{0} = {1}; // TOKEN {0}{2}", sym.name, sym.n, sym.inherits != null ? " INHERITS " + sym.inherits.name : "");
            }
        }

        void ForAllTerminals(bool group, Action<Symbol> write)
        {
            var n = 0;
            foreach (var sym in Tab.terminals)
            {
                if (group)
                {
                    if (n % 20 == 0)
                        Gen.Write(GW.StartLine, string.Empty);
                    else if (n % 4 == 0)
                        Gen.Write(GW.Append, "  ");
                    n++;
                    write(sym);
                    if (n == Tab.terminals.Count)
                        Gen.Write(GW.EndLine, string.Empty);
                    else
                    {
                        Gen.Write(GW.Append, ",");
                        if (n % 20 == 0)
                            Gen.Write(GW.EndLine, string.Empty);
                    }
                }
                else
                {
                    Gen.Write(GW.StartLine, string.Empty);
                    n++;
                    write(sym);
                    if (n < Tab.terminals.Count)
                        Gen.Write(GW.Append, ",");
                    Gen.Write(GW.EndLine, string.Empty);
                }
            }
        }

        void GenTokenBase() => ForAllTerminals(true, sym => Gen.Write(GW.Append, "{0,2}", sym.inherits?.n ?? -1));

        void GenTokenNames() => ForAllTerminals(false, sym => Gen.Write(GW.Append, Tab.Quoted(sym.VariantName)));


        void GenPragmas()
        {
            foreach (var sym in Tab.pragmas)
                Gen.Write(GW.Line, "public const int _{0} = {1};", sym.name, sym.n);
        }

        void GenCodePragmas()
        {
            foreach (var sym in Tab.pragmas)
            {
                Gen.Write(GW.Line, "if (la.kind == {0}) // pragmas don't inherit kinds", sym.n);
                Gen.Write(GW.Line, "{");
                Gen.Indentation++;
                CopySourcePart(sym.semPos, indent: true);
                Gen.Indentation--;
                Gen.Write(GW.Line, "}");
            }
        }

        void GenUsingSymtabSomething(List<SymTab> list, string method, string param, string comment)
        {
            if (list == null) return;
            foreach (var st in list)
                Gen.Write(GW.Line, "using({0}.{1}({2})) {3}", st.name, method, param, comment); // intentionally no ; !
        }

        void GenProductions()
        {
            foreach (var sym in Tab.nonterminals)
            {
                CurrentNtSym = sym;
                Gen.Write(GW.StartLine, "void {0}{1}(", sym.name, PROD_SUFFIX);
                CopySourcePart(sym.attrPos, indent: false);
                Gen.Write(GW.EndLine, ")");
                Gen.Write(GW.Line, "{");
                Gen.Indentation++;
                if (needsAST)
                    Gen.Write(GW.Line, "using(astbuilder.createBarrier({0}))", Tab.Quoted(sym.astjoinwith)); // intentionally no ; !
                GenUsingSymtabSomething(sym.scopes, "createScope", "", "");  // needs to be first
                GenUsingSymtabSomething(sym.useonces, "createUsageCheck", "false, la", "// 0..1"); // needs to be after createScope 
                GenUsingSymtabSomething(sym.usealls, "createUsageCheck", "true, la", "// 1..N");  // needs to be after createScope
                Gen.Write(GW.Line, "{");
                Gen.Indentation++;
                CopySourcePart(sym.semPos, indent: true);
                GenCode(sym.graph, new BitArray(Tab.terminals.Count));
                Gen.Indentation--;
                Gen.Write(GW.Line, "}");
                Gen.Indentation--;
                Gen.Write(GW.Line, "}");
                Gen.Write(GW.Break, string.Empty);
            }
        }

        void InitSets0()
        {
            for (var i = 0; i < symSet.Count; i++)
            {
                var s = symSet[i];
                var islast = (i == symSet.Count - 1);
                WriteSetsLine(s, islast);
            }
        }

        void InitSets()
        {
            for (var i = 0; i < symSet.Count; i++)
            {
                var s = DerivationsOf(symSet[i]);
                var islast = (i == symSet.Count - 1);
                WriteSetsLine(s, islast);
            }
        }

        private void WriteSetsLine(BitArray s, bool islast)
        {
            Gen.Write(GW.StartLine, "{");
            var j = 0;
            foreach (var sym in Tab.terminals)
            {
                if (s[sym.n])
                    Gen.Write(GW.Append, "_T,");
                else
                    Gen.Write(GW.Append, "_x,");
                ++j;
                if (j % 4 == 0)
                    Gen.Write(GW.Append, "  ");

            }
            // now write an elephant at the last position to not fiddle with the commas:
            if (!islast)
                Gen.Write(GW.EndLine, "_x},");
            else
                Gen.Write(GW.EndLine, "_x}");
        }

        private static string ToTF(bool b) => b ? "true" : "false";

        void GenSymbolTables(bool declare)
        {
            foreach (var st in Tab.symtabs)
            {
                if (declare)
                    Gen.Write(GW.Line, "public readonly Symboltable {0};", st.name);
                else
                    Gen.Write(GW.Line, "{0} = new Symboltable(\"{0}\", {1}, {2}, this);", st.name, ToTF(parser.dfa.ignoreCase), ToTF(st.strict));
            }
            if (declare)
            {
                Gen.Write(GW.Line, "public Symboltable symbols(string name)");
                Gen.Write(GW.Line, "{");
                Gen.Indentation++;
                foreach (var st in Tab.symtabs)
                    Gen.Write(GW.Line, "if (name == {1}) return {0};", st.name, Tab.Quoted(st.name));
                Gen.Write(GW.Line, "return null;");
                Gen.Indentation--;
                Gen.Write(GW.Line, "}");
                Gen.Write(GW.Break, string.Empty);
            }
        }

        void GenSymbolTablesPredfinedValues()
        {
            foreach (var st in Tab.symtabs)
            {
                foreach (var s in st.predefined)
                    Gen.Write(GW.Line, "{0}.Add({1});", st.name, Tab.Quoted(s));
            }
        }

        void GenSymbolTablesChecks()
        {
            foreach (var st in Tab.symtabs)
                Gen.Write(GW.Line, "{0}.CheckDeclared();", st.name);
        }

        public void WriteParser()
        {
            using (Gen = new Generator(Tab))
            {
                var frame = Gen.OpenFrame("Parser.frame");
                parser.errors.Info(0, 0, $"using {frame}", 21);
                var pars = Gen.OpenGen("Parser.cs");
                parser.errors.Info(0, 0, $"generating parser {pars.FullName}", 22);

                symSet.Add(Tab.allSyncSets);
                Tab.terminals.ForEach(GenTerminalErrorMsg);

                if (usingPos != null)
                {
                    CopySourcePart(usingPos, indent: false);
                }

                Gen.CopyFramePart("-->namespace");
                /* AW open namespace, if it exists */
                if (!string.IsNullOrWhiteSpace(Tab.nsName))
                {
                    Gen.Write(GW.Line, "namespace {0}", Tab.nsName);
                    Gen.Write(GW.Line, "{");
                    Gen.Indentation++;
                }

                Gen.CopyFramePart("-->constants");
                Gen.Indentation++; // now in class Parser
                GenTokens(); /* ML 2002/09/07 write the token kinds */
                Gen.Write(GW.Line, "private const int __maxT = {0};", Tab.terminals.Count - 1);
                GenPragmas(); /* ML 2005/09/23 write the pragma kinds */

                Gen.CopyFramePart("-->declarations");
                GenSymbolTables(declare: true);
                CopySourcePart(Tab.semDeclPos, indent: false);

                Gen.CopyFramePart("-->constructor");
                Gen.Indentation++;
                GenSymbolTables(declare: false);
                if (needsAST)
                    Gen.Write(GW.Line, "astbuilder = new AST.Builder(this);");
                Gen.Indentation--;

                Gen.CopyFramePart("(((beginalternatives");
                Gen.Indentation += 2; // in Get()/for()
                Gen.CopyFramePart(")))endalternatives", GenerateAutocompleteInformation);

                Gen.CopyFramePart("-->pragmas");
                GenCodePragmas();

                Gen.CopyFramePart("-->productions");
                Gen.Indentation -= 2; // back in class Parser
                GenProductions();

                Gen.CopyFramePart("-->parseRoot");
                Gen.Indentation++; // in void Parse()
                GenSymbolTablesPredfinedValues();
                Gen.Write(GW.Line, "{0}{1}();", Tab.gramSy.name, PROD_SUFFIX); // main production
                if (Tab.checkEOF)
                    Gen.Write(GW.Line, "Expect(0);");
                GenSymbolTablesChecks();
                Gen.Indentation--;

                Gen.CopyFramePart("-->tbase");
                Gen.Indentation++;
                GenTokenBase(); // write all tokens base types
                Gen.Indentation--;

                Gen.CopyFramePart("-->tname");
                Gen.Indentation++;
                GenTokenNames(); // write all token names
                Gen.Indentation--;

                Gen.CopyFramePart("-->initialization0");
                Gen.Indentation++;
                InitSets0();
                Gen.Indentation--;

                Gen.CopyFramePart("-->initialization");
                Gen.Indentation++;
                InitSets();
                Gen.Indentation--;

                Gen.CopyFramePart("(((beginastcode"); // class AST, only needed, if declarative AST is used.
                Gen.CopyFramePart(")))endastcode", needsAST);

                Gen.CopyFramePart("-->errors");
                Gen.Indentation += 2; // void Syntaxerror() / switch() 
                foreach (var e in Errors)
                    Gen.Write(GW.Line, e);
                Gen.Indentation -= 2;

                Gen.CopyFramePart(null);
                Gen.Indentation--; // now out of class Parser

                /* AW 2002-12-20 close namespace, if it exists */
                if (!string.IsNullOrWhiteSpace(Tab.nsName))
                {
                    Gen.Indentation--;
                    Gen.Write(GW.Line, "}");
                }
            }
        }

        public void WriteStatistics()
        {
            Trace.WriteLine();
            Trace.WriteLine("{0} terminals", Tab.terminals.Count);
            Trace.WriteLine("{0} symbols", Tab.terminals.Count + Tab.pragmas.Count + Tab.nonterminals.Count);
            Trace.WriteLine("{0} nodes", Tab.nodes.Count);
            Trace.WriteLine("{0} sets", symSet.Count);
        }

        public ParserGen(CocoRCore.CSharp.Parser parser)
        {
            this.parser = parser;
            usingPos = null;
        }

    } // end ParserGen

} // end namespace
