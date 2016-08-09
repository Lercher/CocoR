copy /y ..\test\inheritance.exe
copy /y ..\..\Utilities\Binaries\Fleck.dll
%windir%\Microsoft.Net\Framework\v4.0.30319\csc.exe /debug /out:FleckEditor.exe /t:exe *.cs /r:Fleck.dll /r:inheritance.exe
if not errorlevel 1 (
    FleckEditor.exe
)