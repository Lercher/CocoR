..\coco inheritance.ATG -frames ..
if not errorlevel 1 (
    %windir%\Microsoft.Net\Framework\v2.0.50727\vbc.exe /optimize /nologo /out:Inheritance.exe /target:exe *.vb
    inheritance.exe sample.txt
)