..\coco CasDomains.atg -frames .. -ac -is -utf8
if not errorlevel 1 (
    CasDomains.exe sample.txt
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /out:CasDomainsEditor.exe /t:exe Editor.cs Parser.cs Scanner.cs
    if not errorlevel 1 (
        rem copy /y *.exe \\ntsdtsc.singhammer.de\Leasing\bin\CAS\
        rem copy /y *.txt \\ntsdtsc.singhammer.de\Leasing\bin\CAS\
        rem copy /y *.atg \\ntsdtsc.singhammer.de\Leasing\bin\CAS\
        CasDomainsEditor.exe            
    )
)
