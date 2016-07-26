# C# version of the extended Coco/R compiler compiler

See http://www.ssw.uni-linz.ac.at/Coco/#CS for the
so called 2011 version of Coco/R.

## Status

* Compiling
* with known bugs

## How to Build

* build.bat - build coco.exe 
* coc.bat - translate coco.atg with coco.exe to parser.cs and scanner.cs
* test/cocbuild.bat - generate, build and run a test parser against sample.txt


## Known Bugs

* There are probably more keywords in the Coco grammar
  as stated in the user manual, because there could
  be conflicts of the production methods in the generated
  parser with utility methods such as isKind() or 
  StartOf(). This is by design of the 2011 version, but
  it can easily improved. 
  -> Needs to be investigated.
  -> probably "won't fix"

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
  -> there are now set0 without and set with
  inheritance taken into account. So StartOf() and
  error synchronization honor token inheritance.
  -> Fixed.