..\coco CasDomains.atg -frames .. -ac -utf8
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /debug /out:CasDomainsEditor.exe /t:exe Editor.cs Parser.cs Scanner.cs
    if not errorlevel 1 (
        copy /y *.exe \\ntsdtsc.singhammer.de\Leasing\bin\CAS\
        copy /y *.txt \\ntsdtsc.singhammer.de\Leasing\bin\CAS\
        copy /y *.atg \\ntsdtsc.singhammer.de\Leasing\bin\CAS\
        CasDomainsEditor.exe            
    )
)
