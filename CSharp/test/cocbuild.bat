..\coco inheritance.ATG -frames .. -ac -utf8
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /debug /out:inheritance.exe /t:exe *.cs
    if not errorlevel 1 if /%1/==// (
        inheritance.exe sample.txt
    )
)