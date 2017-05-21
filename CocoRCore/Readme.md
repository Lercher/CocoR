# Coco/R for .Net Core (C# Version)

This version of Coco/R modernizes types and some build concepts for
the .Net core platform and compilers such as C#7.
It includes several enhancements from other Coco/R versions 
within this repository, such as symbol tables or terminal inheritance.

If you are stuck with the .Net Framework 2 CLR and it's compilers, 
please use the other versions, e.g. those contained in the `CSharp` folder.


## Planned changes to the standard version

The main intention is to enable Coco/R to play well in a .Net core ecosystem and to allow
multiple different parsers to live efficently in the same compile unit, 
scanning strings *and* streams. And maybe even have a way to dynamicly produce the 
code for a parser and compile it on the fly within the Roslyn compiler architecture.

In particular:

* Getting rid of non-generic collections such as `ArrayList` and `Hashtable`
* Getting rid of enum kind `int`s
* One class per file for Coco/R
* Putting some common `Scanner.frame` and `Parser.frame` classes in a seperate source file each, bute these will contain many classes per file
* Refactoring the scanner to accept `IEnumerable<char>` in addition to `Stream`
* Factoring out `Scanner` code to a common base class
* Factoring out `Parser` code to a common base class
* Getting rid of huge *.frame files, without loosing the flexibility they provide. Maybe become linked resources.
* Getting the compiler compiler itself as a linkable component instead of an executable alone 
* using `Func<X,Y>` etc. instead of frame file code.


## Build on the command line

Change to this directory and then:
* `cd src`
* `dotnet restore` to restore packages.
* `dotnet build` to build Coco/R itself and the sample grammars
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

Running the sample parsers in the second step should print something like that
````plaintext
Coco/R Core Samples (May 19, 2017)
...\CocoR\CocoRCore\sample-grammars\Coco\Coco.atg: 0 error(s), 0 warning(s).
...\CocoR\CocoRCore\sample-grammars\Taste\Test.tas: 0 error(s), 0 warning(s).
...\CocoR\CocoRCore\sample-grammars\Inheritance\SampleInheritance.txt: 0 error(s), 0 warning(s).
...\CocoR\CocoRCore\sample-grammars\WFModel\SampleWFModel.txt: 0 error(s), 0 warning(s).
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

Note: OmniSharp detects a lot of problems, if a project reference is used. 
If you don't like that, open `sample-grammars.csproj` and change

````xml
  <ItemGroup>
    <ProjectReference Include="..\src\CocoRCore.csproj" />
    <!-- <Compile Include="../src/*.frame.cs"/> -->
  </ItemGroup>
````

to

````xml
  <ItemGroup>
    <!-- <ProjectReference Include="..\src\CocoRCore.csproj" /> -->
    <Compile Include="../src/*.frame.cs"/>
  </ItemGroup>
````

This is probably due to this bug: 
[omnisharp-roslyn #762](https://github.com/OmniSharp/omnisharp-roslyn/issues/762)


## Status: incubating

* compiles to be run as a process
* generates mixed V2 and core code
* uses shorter frames instead of the long original ones
* multiple test cases but no unit tests
* Coco/R core can successfully compile itself now
* four sample grammars compile, can be linked together
* four sample grammars parse their samples successfully and without diagnostics
* problem with a csproj reference, can't find `CocoRCore.ParserBase` then
* including `src/*.frame.cs` works
* API unstable
* switch -ac (autocomplete) unstable: removes initialization but not the calls
