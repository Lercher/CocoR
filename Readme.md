# C# version of the Coco/R compiler compiler

With enhancement of base types for tokens so that
a more specific token can be valid in a parsing
context that expects a more general token.

The base type of a token has to be declared explicitly
in the TOKENS section of the grammar file.

A typical usage scenario for the extension
would be to have keywords as identifiers 
based on parsing context such as in "var var = 5;"
with the production D = "var" ident '=' number. 

see http://www.ssw.uni-linz.ac.at/Coco/#CS for the
so called 2011 version of Coco/R.

## Status

* Compiling
* with known bugs

## Known Bugs

* There are probably more keywords in the Coco grammar
  as stated in the user manual, because there could
  be conflicts of the production methods in the generated
  parser with utility methods such as isKind() or 
  StartOf(). This is by design of the 2011 version, but
  it can easily improved. 
  -> Needs to be investigated.

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