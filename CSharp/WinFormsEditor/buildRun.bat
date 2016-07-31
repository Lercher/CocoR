%windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /out:WinFormsEditor.exe /t:exe *.cs
if not errorlevel 1 (
    WinFormsEditor.exe
)