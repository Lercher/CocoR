..\coco CasDomains.atg -frames .. -ac -is -utf8
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /out:CasDomains.exe /t:exe *.cs
    if not errorlevel 1 (
        CasDomains.exe sample.txt
    )
)
