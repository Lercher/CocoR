# Coco/R for .Net Core (C# Version)

This version of Coco/R modernizes types and some build concepts for
the .Net core platform and compilers such as C#7.
It includes several enhancements from other Coco/R versions 
within this repository, such as symbol tables or terminal inheritance.

If you are stuck with the .Net Framework 2 CLR and it's compilers, 
please use the other versions, e.g. those contained in the `CSharp` folder.
However, if you plan to use modern frameworks, such as the 
[Language Server Protocol binding](https://github.com/Lercher/csharp-language-server-protocol)
to provide ATG language services to a compatible IDE like vscode, this version
will be the one to choose.



## Planned changes to the standard version's code and API

The main intention is to enable Coco/R to play well in a .Net core ecosystem and to allow
multiple different parsers to live efficently in the same compile unit, 
scanning strings, stringbuilders and char streams. And maybe even have a way to dynamicly produce the code for a parser and compile it on the fly within the Roslyn compiler architecture.

In particular:

* Getting rid of non-generic collections such as `ArrayList` and `Hashtable`
* Getting rid of enum kind `int`s
* getting rid of problematic CRLF handling. Should be normalized to unix file convention to only have linefeeds \n out of the scanner
* One class per file for Coco/R
* Putting some common `Scanner.frame` and `Parser.frame` classes in a separate source file each, bute these will contain many classes per file
* Refactoring the scanner to accept `TextReader` instead of `Stream`
* Factoring out `Scanner` code to a common base class
* Factoring out `Parser` code to a common base class
* Getting rid of huge *.frame files, without loosing the flexibility they provide. Maybe become linked resources.
* Getting the compiler compiler itself as a linkable component instead of an executable alone 
* using `Func<X,Y>` etc. instead of frame file code.
* remove/replace scanner buffer spaghetti code
* better diagnostic messages (e.g. on alternatives)
* better LL1 warnings
* Scanner comment handling could be a method call with parameters, instead of generated code
* comments should be made available to the parser just like pragmas
* better Parser/Scanner creation and initialization, i.e. getting rid of myriad new() variants

## Howto install the dotnet SDK on ubuntu 17.04

[Doku on microsoft.com](https://www.microsoft.com/net/core#linuxubuntu)
(use the 16.10 kind)

## Build on the command line

Change to this directory and then:
* `cd src`
* `dotnet restore` to restore packages.
* `dotnet build /t:grammars` to build Coco/R itself and the sample grammars
* `dotnet run` to run Coco/R
* `cd ..`

Second step, the sample grammars:

* `cd sample-grammars`
* `dotnet restore` to restore packages, esp. to make the referenced dll from the first step available.
* `dotnet build` to build Coco/R itself and the sample grammars
* `dotnet run` to run all the sample parsers on their sample files.
* `cd ..`

Running Coco/R in the first step should print something like that
````plaintext
Coco/R Core (May 14, 2017)
Usage: Coco Grammar.ATG {Option}
Options:
  -namespace <namespaceName>
  -frames    <frameFilesDirectory>
  -trace     <traceString>
  -o         <outputDirectory>
  -lines     [emit lines]
  -ac        [generate autocomplete/intellisense information]
  -is        [ignore semantic actions]
  -oo        [omit *.old files]
Valid characters in the trace string:
  A  trace automaton
  F  list first/follow sets
  G  print syntax graph
  I  trace computation of first sets
  J  list ANY and SYNC sets
  P  print statistics
  S  list symbol table
  X  list cross reference table
Scanner.frame and Parser.frame files needed in ATG directory
or in a directory specified in the -frames option.
````

Running the sample parsers in the second step should print something like that
````plaintext
d:\Daten\Git\CocoR\CocoRCore\sample-grammars\Coco\Coco.atg: 0 error(s), 0 warning(s).
  Token (0,0)..(0,0) = 
  Token (32,1)..(32,6) = using
  Token (32,7)..(32,13) = System
  Token (32,13)..(32,14) = .
  Token (32,14)..(32,16) = IO
  Token (32,16)..(33,0) = ;
  Token (34,1)..(34,9) = COMPILER
  Token (34,10)..(35,0) = Coco
  Token (36,2)..(36,7) = const
  Token (36,8)..(36,11) = int
d:\Daten\Git\CocoR\CocoRCore\sample-grammars\Taste\Test.tas: 0 error(s), 0 warning(s).
  Token (6,1)..(6,8) = program
  Token (6,9)..(6,13) = Täst
  Token (6,14)..(7,0) = {
  Token (7,2)..(7,5) = int
  Token (7,6)..(7,7) = i
  Token (7,7)..(7,8) = ;
  Token (9,2)..(9,6) = void
  Token (9,7)..(9,10) = Föö
  Token (9,10)..(9,11) = (
  Token (9,11)..(9,12) = )
d:\Daten\Git\CocoR\CocoRCore\sample-grammars\Inheritance\SampleInheritance.txt: 0 error(s), 0 warning(s).
  Token (1,1)..(1,4) = var
  Token (1,5)..(1,6) = a
  Token (1,7)..(1,9) = as
  Token (1,10)..(1,16) = string
  Token (1,16)..(2,0) = ;
  Token (2,1)..(2,4) = VAR
  Token (2,5)..(2,6) = c
  Token (2,7)..(2,9) = as
  Token (2,10)..(2,16) = DOUBLE
  Token (2,16)..(3,0) = ;
d:\Daten\Git\CocoR\CocoRCore\sample-grammars\WFModel\SampleWFModel.txt: 0 error(s), 0 warning(s).
  Token (6,1)..(6,8) = Version
  Token (6,9)..(7,0) = 2.0.1.7
  Token (7,1)..(7,10) = Namespace
  Token (7,11)..(7,20) = Cassiopae
  Token (7,20)..(7,21) = .
  Token (7,21)..(8,0) = Angebot
  Token (8,1)..(8,19) = ReaderWriterPrefix
  Token (8,20)..(9,0) = CAS
  Token (10,1)..(10,10) = RootClass
  Token (10,11)..(11,0) = Data
````


## Build in vscode

We recommend to use Visual Studio Code to build this version. First open this direcory with vscode or open a command prompt, change to this directory and then:
* `code src` or `code <this-directory>\src` to open vscode.
* `code sample-grammars` or `code <this-directory>\sample-grammars` to open vscode.
* have a look at the C# sources, if you coose to do so.
* `Ctrl-Shift-B` to build Coco/R itself and the sample grammars.
* If asked to restore packages, answer "Yes".
* `F5` to run.
* If it won't run, try `F1` plus `task restore` first.


## Status: alpha / stabilizing API

* compiles to be run as a process
* generates mixed V2 and core code
* uses much shorter frames instead of the long original ones
* choose from importing two `*.frame.cs` files or referencing `CocoRCore.dll`
* multiple test cases but no unit tests
* Coco/R core can successfully compile itself
* four sample grammars compile, can be linked together
* four sample grammars parse their samples successfully and without diagnostics
* problem with a csproj reference, can't find `CocoRCore.ParserBase` if on, but ...
* ... including `src/*.frame.cs` works
* API unstable
* switch -ac (autocomplete) unstable: removes initialization but not the calls
* more new C#7 code, var, lambdas etc.
* no more Buffer but a TextReader facade
* due to a buffer constraint, ATGs with semantic actions cannot be longer than aprox. 128k characters currently
* targets core and net40 now
