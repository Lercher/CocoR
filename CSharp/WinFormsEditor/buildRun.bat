pushd ..\test
call cocbuild.bat -buildonly
popd
copy /y ..\test\inheritance.exe
%windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /out:Editor.exe /t:exe *.cs /r:inheritance.exe
if not errorlevel 1 (
    Editor.exe
)