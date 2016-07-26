Public Class Main
    Public Shared Function Main(ByVal arg() As String) As Integer
        Console.WriteLine("VB.Net Inheritance parser")
        If arg.Length = 1 Then
            Console.WriteLine("scanning {0} ...", arg(0))
            Dim scanner As New Scanner(arg(0))
			Dim parser As New Parser(scanner)
            parser.Parse()
            Console.WriteLine("{0} error(s) detected", parser.errors.count)
            If parser.errors.count > 0 Then Return 1
        Else
            Console.WriteLine("usage: Inheritance.exe file")
            Return 99
        End If
        Return 0
    End Function
End Class
