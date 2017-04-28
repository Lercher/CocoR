..\coco CasExternalTables.atg -frames .. -ac -utf8
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /debug /out:CasExternalTables.exe /t:exe *.cs
    if not errorlevel 1 if /%1/==// (
        CasExternalTables.exe "\\ntsdtsc\leasing\TEMP\Lercher\UPDATE_EXTERNALTABLES.SQL"
    )
)