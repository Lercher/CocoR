pushd ..\test
call cocbuild.bat -buildonly
popd
copy /y ..\test\wfmodel.exe
%windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /debug /out:WFModelEditor.exe /t:exe *.cs /r:wfmodel.exe
if not errorlevel 1 (
    WFModelEditor.exe
)