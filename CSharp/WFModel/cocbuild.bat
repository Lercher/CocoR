..\coco wfmodel.ATG -frames .. -ac -utf8
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /debug /out:wfmodel.exe /t:exe *.cs
)
