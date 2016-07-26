# Coco/R Compiler Compiler with Token Inheritance

Based on the Coco/R Sources at
http://www.ssw.uni-linz.ac.at/Coco
that we call the "2011 version".

This code includes an enhancement called "token inheritance".
A typical usage scenario for the extension
would be to allow keywords as identifiers 
based on a parsing context that expects an identifier.


## Token Inheritance

We denote base types for tokens in the grammar file so that
a more specific token can be valid in a parsing
context that expects a more general token.

The base type of a token has to be declared explicitly
in the TOKENS section of the grammar file like that:

    TOKENS
      ident = letter { letter }.
      var : ident = "var". //  ": ident" is the new syntax 
      as = "as".

meaning that the keyword "var" is now valid in a 
production at a place, where an ident would be expected.
So, if you have a production like

    PRODUCTIONS
      Declaration = var ident as ident ";".

A text like

    var var as int; // valid

would be valid, whereas a text like

    var as as int; // invalid, because "as" is not an ident

would be invalid, just as the first text would 
with a parser generated with the 2011 version of Coco/R.


## Extended Syntax

see http://www.ssw.uni-linz.ac.at/Coco/Doc/UserManual.pdf 
with this modification of section "2.3.2 Tokens":

    TokenDecl = Symbol [':' Symbol] ['=' TokenExpr '.']. 

The Symbol behind the colon has to be a previously declared
token and is called the base token type. The generated parser
now accepts this declared token everywhere a token of its
base token type is expected. 

This compatibility is transitive.
However, it would be bad design to have complicated
inheritance trees in the grammar.


## Languages

* C# - OK, beta testing
* VB.Net - OK, beta testing
* Java - planned but not yet scheduled

Note: The generated code for .Net languages targets
plain vanilla .Net Framework 2.0 compilers and libraries. 

## Distributables

To generate parsers from attributed grammars, you only need
these files from the language directory you choose:

* coco.exe (built with build.bat if you don't have one yet)
* scanner.frame
* parser.frame
* copyright.frame (optional)

There are no dependencies to use the generated scanners and parsers.


## License

Coco/R, including this extension, is distributed under the terms
of the GNU General Public License (slightly extended).

This means that you have to open source any extension to
Coco/R itself but you are licensed to use generated
scanners and parsers in any software that you build, 
even in closed source projects.


## Known Bugs

see readme.md in the respective language folder
