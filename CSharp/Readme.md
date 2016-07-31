# C# version of the extended Coco/R compiler compiler

See http://www.ssw.uni-linz.ac.at/Coco/#CS for the
so called 2011 version of Coco/R, and of course 
the readme.md in the root directory.


## Status

* Compiling
* no known bugs
* beta testing


## How to Build

* build.bat - build coco.exe 
* coc.bat - translate coco.atg with coco.exe to parser.cs and scanner.cs
* test/cocbuild.bat - generate, build and run a test parser against sample.txt


## Known Bugs

* none


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
  naming conflicts.
  -> Fixed.
