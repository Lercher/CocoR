copy /y ..\..\Utilities\Binaries\Fleck.dll
..\coco wfmodel.ATG -frames .. -ac -utf8
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\csc.exe /debug /out:wfmodel.exe /t:exe Scanner.cs Parser.cs main.cs    
    if not errorlevel 1 (
        %windir%\Microsoft.Net\Framework\v4.0.30319\csc.exe /debug /out:WFmodelEditor.exe /t:exe Scanner.cs Parser.cs Editor.cs Websocket.cs /r:Fleck.dll
        if not errorlevel 1 (
            WFmodelEditor.exe
        )
    )
)
