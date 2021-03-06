# C# version of the extended Coco/R compiler compiler

See http://www.ssw.uni-linz.ac.at/Coco/#CS for the
so called 2011 version of Coco/R, and of course 
the readme.md in the root directory.


## Status

* Compiling
* beta testing
* sample editor with autocompletion available


## Sub-Folders

* test - test grammar Inheritance
* WinFormsEditor - a simple TextBoxStyle editor to evaluate autocompletion features


## How to Build

* build.bat - build coco.exe 
* coc.bat - translate coco.atg with coco.exe to parser.cs and scanner.cs
* test/cocbuild.bat - generate, build and run a test parser against sample.txt


## Known Bugs

In the winforms editor:

* all lowercased keywords for autocomplete in a `IGNORECASE` grammar

General:

* None


## Resolved Bugs

* The switch optimization, used with 5+ alternatives, 
  has to be implemented contravariantly in base types. 
  It is currently covariant, which is wrong.
  -> Testable example NumberIdent in test\inheritance.atg
  -> Fixed. 

* 'set' array related methods caclulate based on
  non inheritance aware tables at parse time.
  This could probably be moved to compiler
  generation time.
  -> there are now `set0` without and `set` with
  inheritance taken into account. So StartOf() and
  error synchronization honor token inheritance.
  -> Fixed.

* There are probably more keywords in the Coco grammar
  as stated in the user manual, because there could
  be conflicts of the production methods in the generated
  parser with utility methods such as isKind() or 
  StartOf(). This is by design of the 2011 version, but
  it can be improved.
  -> Resolved by adding a unicode UNDERTIE, which is a valid C#
  identifier part, plus "NT" (i.e. `"‿NT"`)
  to the production method name. As an UNDERTIE is no valid
  letter for a Coco/R production, there can be no more
  naming conflicts. See http://www.fileformat.info/info/unicode/category/Pc/list.htm
  -> Fixed.

* german umlauts terminate parsing although declared as character classes
  -> *.atg files were UTF-8 but had no BOM, using forced UTF-8 Scanner -> Fixed.

* positioning in the source editor panel is out of sync (b/c Tabs, and Umlauts)
  -> Fixed. Seems to work now, probably since the *.atg files are scanned as UTF-8.

* tokens with a `"` inside their text don't display well in the right navigator.
  -> should be fixed now.

* token expressions were listed as literals in the `tName` array. -> Fixed. 

* missing error messages locator in the lower pane / doubleclick -> Implemented

* ALT defines (or lists) a symbol twice, see "languages DE" 
  -> works as designed, was a bug in the grammar. One DE was listed for
  dbcode>lang, a second one for string>lang -> Fixed


* errormessage "dbcode 'EN' not declared in 'lang'" points to token 
  before the error -> Fixed

* errormessage "dbcode 'SOMETHING' declared twice in 'domains'" points 
  to token before the error -> Fixed

* non strict symbol tables yield the wrong declaration for a scoped
  token that is used before the symbol is declared locally.
  -> Fixed

* the hierarchy building for the new AST features is broken. I have
  to rethink the process. Probably a marker barrier on the stack
  at the production level may be helpful.
  -> Fixed for my examples

* sprinkled Console.WriteLine()s in the generated parser
  -> Fixed
  
* Priming function is mandatory but it shouldn't. -> Is now a hook in the
  parsers base class that can but don't have to be overridden. -> Fixed
