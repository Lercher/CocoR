# Enhanced Coco/R Compiler Compiler

Based on the Coco/R Sources at
http://www.ssw.uni-linz.ac.at/Coco
that we call the "2011 version".

This code includes these enhancements:

* Token Inheritance -
A typical usage scenario for the extension
would be to allow keywords as identifiers 
based on a parsing context that expects an identifier.

* Autocomplete Information -
If the switch -ac is set, the generator produces a
parser that records to any parsed terminal symbol
all alternative terminal symbols that would be valid
instead of the actually parsed token.

* Symbol Tables -
Having autocomplete information is quite useless
for token classes, unless you can specify from where
you will take all valid identifiers. In this version
of Coco/R you can declare global symbol tables for
a parser and mark tokenclasses in productions to 
create a new symbol, error out if it was already
declared and mark a tokenclass to use such a symbol,
i.e. error out if it was not declared before. At such 
a symbol use point, when looking at alternatives, you
can ask the parser of which symbols it knows at this
point to provide autocomplete for token classes.

* Lexical Scopes for Symbol Tables -
The existance and no-redefinition checks can be scoped
lexically by marking a production as a scope for a symbol
table. By creating a new scope for a production, you can
create and redefine any symbol, as well as use any symbol
in this scope and all of it's parent scopes. This scope
as well as all newly created symbols is destroyed when 
you leave the generated production method. So if you 
need to preserve the scoped symbols you might want to
add a semantic action that stores the symbol 
table's current scope `currentScope` in your AST.   



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


### Extended syntax for token inheritance

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



## Autocomplete Information

After parsing, the parser has a list of all relevant
tokens `public List<Alternative> tokens` in text ordering,
i.e. no comments, no whitespace and no pragmas.

For each such element the property `t` holds the actually
parsed token. 

The property `alt` holds a BitArray of all
Terminal symbols that would be valid before the current 
token `t` was parsed. With the help of the `Parser.tName`
array and the index in the `alt` array, a client can resolve
the name of the token from the TOKENS section of the atg grammar
file or the literal of an inline declared keyword.

Furthermore, if an alternative token class is associated with 
a symbol table in the current production like in `ident:variables` 
it's symbol table `variables` is accessible by 
the `st` array at its kind position. 


Note: Currently there is a flaw in the implementiation, because
the calculation of possible alternatives stops as soon as the 
actual token gets validly parsed. So this list can be truncated.
-> this has to be investigated.


### Sample Code

This Sample Code lists all tokens and their respective
variants by name. If a variant has an associated symbol table,
a colon and the symbol table's name is appended to the 
token's symbol name. 

    foreach (Alternative a in parser.tokens)
    {
        Token t = a.t;
        Console.Write("({0,3},{1,3}) {2,3} {3,-20} {4, -20}", 
          t.line, t.col, t.kind, Parser.tName[t.kind], t.val);
        Console.Write(" alt: ");
        for (int k = 0; k <= Parser.maxT; k++)
        {
            if (a.alt[k])
                Console.Write("{1} ", k, Parser.tName[k]);
                if (a.st[k] != null)
                    Console.Write(":{0}", a.st[k].name); 
                    // symbol table associated with this k-th terminal
                    // in the current parsing context
                Console.Write(' ');
        }
        Console.WriteLine();
    }

See CSharp/Test/main.cs for a more elaborate
example.


### Autocomplete Information plus Editor (hypothetical)

While comparing the current position inside a hypothetical editor
with the token sequence of the paresd full text, the editor could
provide coloring based on the actual token `t`'s information 
as well as autocompletion based on the `alt` (for keywords)
and `st` array for (available symbols).

Planned: Build a language-server for Visual Studio Code. See
https://code.visualstudio.com/docs/extensions/example-language-server



## Symbol Tables

There is a new section `SYMBOLTABLES` just before `PRODUCTIONS`
where symbol tables for the generated parser can be declared and
initialized.

    STDecl = ident { string } .

Example

    SYMBOLTABLES
      variables.  // an empty symbol table named variables
      types "string" "int" "double". // a symbol table named types with three prefilled entries

In productions you can append to each terminal or weak terminal `t`
an angle bracket plus a declared symboltable symbol to declare a new
name in this symbol table or a colon plus a symboltable to use
a declared name in this table. Semantic errors are generated if the name
is declared twice or does not exist respectivly.

Example

    Decl = "var" ident>variables ':' ident:types.

this production creates a new name in `variables` for the first ident
and checks that the second ident is present in `types`.


### Scoped Symbols

To introduce a new lexical scope to a production like `Block`
for a list of symbol tables,
you have to list them in the production definition:

    Block SCOPES(variables, types) = 
      '{' 
      { Decl } 
      { Statement } 
      '}' .

So the extended Coco/R syntax for productions is

    Production = ident [FormalAttributes] [ScopesDecl] [LocalDecl] '=' Expression '.'.
    ScopesDecl = "SCOPES" '(' ident { ',' ident } ')'.

See http://www.ssw.uni-linz.ac.at/Coco/Doc/UserManual.pdf 
section "2.4 Parser Specification".

Note: Every symbol table has at least one scope, so you don't have
to declare any `SCOPES(...)` block at all. This root scope is the only scope,
that is available after the call to `Parse()`. So, if you need
to preseve the content of a lexically scoped symbol table, store it's
`currentScope`, which is a `List<string>` (C#) / `List(Of String)` (VB.Net)
inside a semantic action. If you need all symbols in all currently active 
scopes, take a look at `items` which is an `IEnumerable<string>`.


### Accessing symbol tables form outside

The declared symbol tables are accessible via a declared public readonly field
of type `Symboltable` and via the generic method `symbol(string name)` that
returns the symbol table with the specified name or null if not found.



## Extended command line arguments

* -ac - Turn on generation of autocomplete / intellisense information


## Languages

* C# - token inheritance *beta*, autocomplete information *alpha*, symbol tables *alpha*
* VB.Net - token inheritance *beta*
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


## Git Branches

* master - main development

* autocomplete - experimental branch to record possible
  alternatives for an editor with autocomplete/intellisense.
  Plus integrated symbol tables for use with autocomplete.  
