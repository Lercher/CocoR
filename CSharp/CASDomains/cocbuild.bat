..\coco CasDomains.atg -frames .. -ac -is -utf8
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /out:CasDomains.exe /t:exe Main.cs Parser.cs Scanner.cs
    if not errorlevel 1 (
        CasDomains.exe sample.txt
    )
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /out:CasDomainsEditor.exe /t:exe Editor.cs Parser.cs Scanner.cs
    if not errorlevel 1 (
        CasDomainsEditor.exe
    )
)