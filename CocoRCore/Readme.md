# Coco/R for .Net Core (C# Version)

This version of Coco/R modernizes types and some build concepts for
the .Net core platform and compilers such as C#7.
It includes several enhancements from other Coco/R versions 
within this repository. If you are stuck with the .Net Framework 2 CLR 
and it's compilers, please use e.g. the version contained in the `CSharp`
folder.

## Planned changes to the standard version

The main intention is, to enable Coco/R to play well in a .Net core ecosystem and to allow
multiple different parsers to live efficently in the same compile unit, 
scanning strings *and* streams. And maybe even have a way to dynamicly produce the 
code for a parser and compile it on the fly within the Roslyn compiler architecture.

* Getting rid of non-generic collections such as ArrayList
* Putting common *.frame classes in a seperate source file
* Refactoring the scanner to accept `IEnumerable<char>` in addition to `Stream`
* Factoring out Scanner code to a common base class
* Factoring out Parser code to a common base class
* Getting rid of huge *.frame files without loosing the flexibility, they provide
* Getting the compiler compiler itself as a linkable component instead of an executable alone 


## Build on the command line

Change to this directory and then:
* `dotnet restore` to restore packages. However, there are currently no packages to restore.
* `dotnet build` to build
* `dotnet run` to run Coco/R

Running should print something like that
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
  -utf8      [force UTF-8 processing, even without BOM]
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


## Build in vscode

We recommend to use Visual Studio Code to build this version. First open this direcory with vscode or open a command prompt, change to this directory and then:
* `code .` or `code <this-directory>` to open vscode.
* have a look at the C# sources, if you coose to do so.
* `Ctrl-Shift-B` to build. 
* If asked to restore packages, answer "Yes". However, there are currently no packages to restore.
* `F5` to run.

