..\coco inheritance.ATG -frames .. -ac -is -utf8
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /out:inheritance.exe /t:exe *.cs
)
..\coco wfmodel.ATG -frames .. -ac -is -utf8
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /out:wfmodel.exe /t:exe *.cs
)
