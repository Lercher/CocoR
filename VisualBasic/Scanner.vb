'-------------------------------------------------------------------------------
'Compiler Generator Coco/R,
'Copyright (c) 1990, 2004 Hanspeter Moessenboeck, University of Linz
'extended by M. Loeberbauer & A. Woess, Univ. of Linz
'with improvements by Pat Terry, Rhodes University
'
'This program is free software; you can redistribute it and/or modify it
'under the terms of the GNU General Public License as published by the
'Free Software Foundation; either version 2, or (at your option) any
'later version.
'
'This program is distributed in the hope that it will be useful, but
'WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
'or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
'for more details.
'
'You should have received a copy of the GNU General Public License along
'with this program; if not, write to the Free Software Foundation, Inc.,
'59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
'
'As an exception, it is allowed to write an extension of Coco/R that is
'used as a plugin in non-free software.
'
'If not otherwise stated, any source code generated by Coco/R (other than
'Coco/R itself) does not fall under the GNU General Public License.
'-------------------------------------------------------------------------------
Option Compare Binary
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace at.jku.ssw.Coco

	Public Class Token
		Public kind    As Integer ' token kind
		Public pos     As Integer ' token position in bytes      in the source text (starting at 0)
		Public charPos As Integer ' token position in characters in the source text (starting at 0)
		Public col     As Integer ' token column (starting at 1)
		Public line    As Integer ' token line   (starting at 1)
		Public val     As String  ' token value
		Public [next]  As Token   ' ML 2005-03-11 Tokens are kept in linked list
	End Class

	Public Class Buffer
		' This Buffer supports the following cases:
		' 1) seekable stream (file)
		'    a) whole stream in buffer
		'    b) part of stream in buffer
		' 2) non seekable stream (network, console)
		Public  Const EOF               As Integer = AscW(Char.MinValue) - 1
		Private Const MIN_BUFFER_LENGTH As Integer = 1024                   '  1KB
		Private Const MAX_BUFFER_LENGTH As Integer = MIN_BUFFER_LENGTH * 64 ' 64KB
		Private       buf               As Byte()                           ' input buffer
		Private       bufStart          As Integer                          ' position of first byte in buffer relative to input stream
		Private       bufLen            As Integer                          ' length of buffer
		Private       fileLen           As Integer                          ' length of input stream (may change if the stream is no file)
		Private       bufPos            As Integer                          ' current position in buffer
		Private       stream            As Stream                           ' input stream (seekable)
		Private       isUserStream      As Boolean                          ' was the stream opened by the user?
		Public Sub New(ByVal s As Stream, ByVal isUserStream As Boolean)
			stream = s
			Me.isUserStream = isUserStream
			If stream.CanSeek Then
				fileLen  = CInt(stream.Length)
				bufLen   = Math.Min(fileLen, MAX_BUFFER_LENGTH)
				bufStart = Int32.MaxValue ' nothing in the buffer so far
			Else
				bufStart = 0
				bufLen   = 0
				fileLen  = 0
			End If
			If bufLen > 0 Then
				buf = New Byte(bufLen - 1) {}
			Else
				buf = New Byte(MIN_BUFFER_LENGTH - 1) {}
			End If
			If fileLen > 0 Then
				Pos = 0    ' setup buffer to position 0 (start)
			Else
				bufPos = 0 ' index 0 is already after the file, thus Pos = 0 is invalid
			End If
			If bufLen = fileLen AndAlso stream.CanSeek Then
				Close()
			End If
		End Sub
		Protected Sub New(ByVal b As Buffer) ' called in UTF8Buffer constructor
			buf = b.buf
			bufStart = b.bufStart
			bufLen = b.bufLen
			fileLen = b.fileLen
			bufPos = b.bufPos
			stream = b.stream
			' keep destructor from closing the stream
			b.stream = Nothing
			isUserStream = b.isUserStream
		End Sub
		Protected Overrides Sub Finalize()
			Try
				Close()
			Finally
				MyBase.Finalize()
			End Try
		End Sub
		Protected Sub Close()
			If Not isUserStream AndAlso stream IsNot Nothing Then
				stream.Close()
				stream = Nothing
			End If
		End Sub
		Public Overridable Function Read() As Integer
			Dim intReturn As Integer
			If bufPos < bufLen Then
				intReturn = buf(bufPos)
				bufPos += 1
			ElseIf Pos < fileLen Then
				Pos = Pos ' shift buffer start to Pos
				intReturn = buf(bufPos)
				bufPos += 1
			ElseIf stream IsNot Nothing AndAlso Not stream.CanSeek AndAlso ReadNextStreamChunk() > 0 Then
				intReturn = buf(bufPos)
				bufPos += 1
			Else
				intReturn = EOF
			End If
			Return intReturn
		End Function
		Public Function Peek() As Integer
			Dim curPos As Integer = Pos
			Dim ch As Integer = Read()
			Pos = curPos
			Return ch
		End Function
		' beg .. begin, zero-based, inclusive, in byte
		' end .. end,   zero-based, exclusive, in byte
		Public Function GetString(ByVal beg As Integer, ByVal [end] As Integer) As String
			Dim len As Integer = 0
			Dim buf As Char() = New Char([end] - beg) {}
			Dim oldPos As Integer = Pos
			Pos = beg
			While Pos < [end]
				Dim ch As Integer = Read()
				buf(len) = ChrW(ch)
				len += 1
			End While
			Pos = oldPos
			Return New String(buf, 0, len)
		End Function
		Public Property Pos() As Integer
			Get
				Return bufPos + bufStart
			End Get
			Set
				If value >= fileLen AndAlso stream IsNot Nothing AndAlso Not stream.CanSeek Then
					' Wanted position is after buffer and the stream
					' is not seek-able e.g. network or console,
					' thus we have to read the stream manually till
					' the wanted position is in sight.
					While value >= fileLen AndAlso ReadNextStreamChunk() > 0
					End While
				End If
				If value < 0 OrElse value > fileLen Then
					Throw New FatalError([String].Format("buffer out of bounds access, position: {0}", value))
				End If
				If value >= bufStart AndAlso value < bufStart + bufLen Then ' already in buffer
					bufPos = value - bufStart
				ElseIf stream IsNot Nothing Then ' must be swapped in
					stream.Seek(value, SeekOrigin.Begin)
					bufLen = stream.Read(buf, 0, buf.Length)
					bufStart = value
					bufPos = 0
				Else
					' set the position to the end of the file, Pos will return fileLen.
					bufPos = fileLen - bufStart
				End If
			End Set
		End Property
		' Read the next chunk of bytes from the stream, increases the buffer
		' if needed and updates the fields fileLen and bufLen.
		' Returns the number of bytes read.
		Private Function ReadNextStreamChunk() As Integer
			Dim free As Integer = buf.Length - bufLen
			If free = 0 Then
				' in the case of a growing input stream
				' we can neither seek in the stream, nor can we
				' foresee the maximum length, thus we must adapt
				' the buffer size on demand.
				Dim newBuf As Byte() = New Byte(bufLen * 2 - 1) {}
				Array.Copy(buf, newBuf, bufLen)
				buf = newBuf
				free = bufLen
			End If
			Dim read As Integer = stream.Read(buf, bufLen, free)
			If read > 0 Then
				bufLen += read
				fileLen = bufLen
				Return read
			End If
			' end of stream reached
			Return 0
		End Function
	End Class

	Public Class UTF8Buffer
		Inherits Buffer
		Public Sub New(ByVal b As Buffer)
			MyBase.New(b)
		End Sub
		Public Overloads Overrides Function Read() As Integer
			Dim ch As Integer
			Do
				' until we find a utf8 start (0xxxxxxx or 11xxxxxx)
				ch = MyBase.Read()
			Loop While (ch >= 128) AndAlso ((ch And 192) <> 192) AndAlso (ch <> EOF)
			If ch < 128 OrElse ch = EOF Then
				' nothing to do, first 127 chars are the same in ascii and utf8
				' 0xxxxxxx or end of file character
			ElseIf (ch And 240) = 240 Then
				' 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
				Dim c1 As Integer = ch And 7
				ch = MyBase.Read()
				Dim c2 As Integer = ch And 63
				ch = MyBase.Read()
				Dim c3 As Integer = ch And 63
				ch = MyBase.Read()
				Dim c4 As Integer = ch And 63
				ch = (((((c1 << 6) Or c2) << 6) Or c3) << 6) Or c4
			ElseIf (ch And 224) = 224 Then
				' 1110xxxx 10xxxxxx 10xxxxxx
				Dim c1 As Integer = ch And 15
				ch = MyBase.Read()
				Dim c2 As Integer = ch And 63
				ch = MyBase.Read()
				Dim c3 As Integer = ch And 63
				ch = (((c1 << 6) Or c2) << 6) Or c3
			ElseIf (ch And 192) = 192 Then
				' 110xxxxx 10xxxxxx
				Dim c1 As Integer = ch And 31
				ch = MyBase.Read()
				Dim c2 As Integer = ch And 63
				ch = (c1 << 6) Or c2
			End If
			Return ch
		End Function
	End Class

	Public Class Scanner
		Private Const           EOL     As Char      = ChrW(10)
		Private Const           eofSym  As Integer   =  0                  ' pdt
		Private Const           maxT    As Integer   = 43
		Private Const           noSym   As Integer   = 43
		Public                  buffer  As Buffer                          ' scanner buffer
		Private                 t       As Token                           ' current token
		Private                 ch      As Integer                         ' current input character
		Private                 pos     As Integer                         ' byte position of current character
		Private                 charPos As Integer                         ' position by unicode characters starting with 0
		Private                 col     As Integer                         ' column number of current character
		Private                 line    As Integer                         ' line number of current character
		Private                 oldEols As Integer                         ' EOLs that appeared in a comment
		Private Shared ReadOnly start   As Dictionary(Of Integer, Integer) ' maps first token character to start state
		Private                 tokens  As Token                           ' list of tokens already peeked (first token is a dummy)
		Private                 pt      As Token                           ' current peek token
		Private                 tval()  As Char      = New Char(128) {}    ' text of current token
		Private                 tlen    As Integer                         ' length of current token
		Shared Sub New()
			start = New Dictionary(Of Integer, Integer)(128)
			For i As Integer =   65 To   90
				start(i) =    1
			Next
			For i As Integer =   95 To   95
				start(i) =    1
			Next
			For i As Integer =   97 To  122
				start(i) =    1
			Next
			For i As Integer =   48 To   57
				start(i) =    2
			Next
			start(        34) =   12
			start(        39) =    5
			start(        36) =   13
			start(        61) =   16
			start(        46) =   32
			start(        43) =   17
			start(        45) =   18
			start(        58) =   20
			start(        60) =   33
			start(        62) =   21
			start(       124) =   24
			start(        40) =   34
			start(        41) =   25
			start(        91) =   26
			start(        93) =   27
			start(       123) =   28
			start(       125) =   29
			start(Buffer.EOF) =   -1
		End Sub
		Public Sub New(ByVal fileName As String)
			Try
				Dim stream As Stream = New FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)
				buffer = New Buffer(stream, False)
				Init()
			Catch generatedExceptionName As IOException
				Throw New FatalError("Cannot open file " & fileName)
			End Try
		End Sub
		Public Sub New(ByVal s As Stream)
			buffer = New Buffer(s, True)
			Init()
		End Sub
		Private Sub Init()
			pos     = -1
			line    =  1
			col     =  0
			charPos = -1
			oldEols =  0
			NextCh()
			If ch = 239 Then
				' check optional byte order mark for UTF-8
				NextCh()
				Dim ch1 As Integer = ch
				NextCh()
				Dim ch2 As Integer = ch
				If ch1 <> 187 OrElse ch2 <> 191 Then
					Throw New FatalError([String].Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2))
				End If
				buffer  = New UTF8Buffer(buffer)
				col     =  0
				charPos = -1
				NextCh()
			End If
			tokens = New Token()
			pt = tokens ' first token is a dummy
		End Sub
		Private Sub NextCh()
			If oldEols > 0 Then
				ch = AscW(EOL)
				oldEols -= 1
			Else
				pos = buffer.Pos
				' buffer reads unicode chars, if UTF8 has been detected
				ch  = buffer.Read()
				col     += 1
				charPos += 1
				' replace isolated '\r' by '\n' in order to make
				' eol handling uniform across Windows, Unix and Mac
				If ch = 13 AndAlso buffer.Peek() <> 10 Then
					ch = AscW(EOL)
				End If
				If ch = AscW(EOL) Then
					line += 1
					col = 0
				End If
			End If
		End Sub
		Private Sub AddCh()
			If tlen >= tval.Length Then
				Dim newBuf() As Char = New Char(2 * tval.Length) {}
				Array.Copy(tval, 0, newBuf, 0, tval.Length)
				tval = newBuf
			End If
			If ch <> Buffer.EOF Then
				tval(tlen) = ChrW(ch)
				tlen += 1
				NextCh()
			End If
		End Sub
		Private Function Comment0() As Boolean
			Dim level    As Integer = 1
			Dim pos0     As Integer = pos
			Dim line0    As Integer = line
			Dim col0     As Integer = col
			Dim charPos0 As Integer = charPos
			NextCh()
			If ch = AscW("/"C) Then
				NextCh()
				While True
					If ch = 10 Then
						level -= 1
						If level = 0 Then
							oldEols = line - line0
							NextCh()
							Return True
						End If
						NextCh()
					ElseIf ch = Buffer.EOF Then
						Return False
					Else
						NextCh()
					End If
				End While
			Else
				buffer.Pos = pos0
				NextCh()
				line       = line0
				col        = col0
				charPos    = charPos0
			End If
			Return False
		End Function
		Private Function Comment1() As Boolean
			Dim level    As Integer = 1
			Dim pos0     As Integer = pos
			Dim line0    As Integer = line
			Dim col0     As Integer = col
			Dim charPos0 As Integer = charPos
			NextCh()
			If ch = AscW("*"C) Then
				NextCh()
				While True
					If ch = AscW("*"C) Then
						NextCh()
						If ch = AscW("/"C) Then
							level -= 1
							If level = 0 Then
								oldEols = line - line0
								NextCh()
								Return True
							End If
							NextCh()
						End If
					ElseIf ch = AscW("/"C) Then
						NextCh()
						If ch = AscW("*"C) Then
							level += 1
							NextCh()
						End If
					ElseIf ch = Buffer.EOF Then
						Return False
					Else
						NextCh()
					End If
				End While
			Else
				buffer.Pos = pos0
				NextCh()
				line       = line0
				col        = col0
				charPos    = charPos0
			End If
			Return False
		End Function
		Private Sub CheckLiteral()
			Select Case t.val
				Case "Imports"       : t.kind =  6
				Case "COMPILER"      : t.kind =  7
				Case "IGNORECASE"    : t.kind =  8
				Case "CHARACTERS"    : t.kind =  9
				Case "TOKENS"        : t.kind = 10
				Case "PRAGMAS"       : t.kind = 11
				Case "COMMENTS"      : t.kind = 12
				Case "FROM"          : t.kind = 13
				Case "TO"            : t.kind = 14
				Case "NESTED"        : t.kind = 15
				Case "IGNORE"        : t.kind = 16
				Case "PRODUCTIONS"   : t.kind = 17
				Case "END"           : t.kind = 20
				Case "ANY"           : t.kind = 24
				Case "WEAK"          : t.kind = 31
				Case "SYNC"          : t.kind = 38
				Case "IF"            : t.kind = 39
				Case "CONTEXT"       : t.kind = 40
				Case Else
			End Select
		End Sub
		Private Function NextToken() As Token
			While ch = AscW(" "C) OrElse _
				ch >= 9 AndAlso ch <= 10 OrElse ch = 13
				NextCh()
			End While
			If ch = AscW("/"C) AndAlso Comment0() OrElse ch = AscW("/"C) AndAlso Comment1() Then
				Return NextToken()
			End If
			Dim recKind As Integer = noSym
			Dim recEnd  As Integer = pos
			t = New Token()
			t.pos     = pos
			t.col     = col
			t.line    = line
			t.charPos = charPos
			Dim state As Integer
			If start.ContainsKey(ch) Then
				state = CType(start(ch), Integer)
			Else
				state = 0
			End If
			tlen = 0
			AddCh()
			Select Case state
				Case -1 ' NextCh already done
					t.kind = eofSym
				Case 0  ' NextCh already done
				Case_0:
					If recKind <> noSym Then
						tlen = recEnd - t.pos
						SetScannerBehindT()
					End If
					t.kind = recKind
				Case 1
				Case_1:
					recEnd = pos
					recKind = 1
					If ch >= AscW("0"C) AndAlso ch <= AscW("9"C) OrElse ch >= AscW("A"C) AndAlso ch <= AscW("Z"C) OrElse ch = AscW("_"C) OrElse ch >= AscW("a"C) AndAlso ch <= AscW("z"C) Then
						AddCh()
						GoTo Case_1
					Else
						t.kind = 1
						t.val = New String(tval, 0, tlen)
						CheckLiteral()
						Return t
					End If
				Case 2
				Case_2:
					recEnd = pos
					recKind = 2
					If ch >= AscW("0"C) AndAlso ch <= AscW("9"C) Then
						AddCh()
						GoTo Case_2
					Else
						t.kind = 2
					End If
				Case 3
				Case_3:
					t.kind = 3
				Case 4
				Case_4:
					t.kind = 4
				Case 5
				Case_5:
					If ch <= 9 OrElse ch >= 11 AndAlso ch <= 12 OrElse ch >= 14 AndAlso ch <= AscW("&"C) OrElse ch >= AscW("("C) AndAlso ch <= AscW("["C) OrElse ch >= AscW("]"C) AndAlso ch <= AscW(Char.MaxValue) Then
						AddCh()
						GoTo Case_6
					ElseIf ch = 92 Then
						AddCh()
						GoTo Case_7
					Else
						GoTo Case_0
					End If
				Case 6
				Case_6:
					If ch = 39 Then
						AddCh()
						GoTo Case_9
					Else
						GoTo Case_0
					End If
				Case 7
				Case_7:
					If ch >= AscW(" "C) AndAlso ch <= AscW("~"C) Then
						AddCh()
						GoTo Case_8
					Else
						GoTo Case_0
					End If
				Case 8
				Case_8:
					If ch >= AscW("0"C) AndAlso ch <= AscW("9"C) OrElse ch >= AscW("a"C) AndAlso ch <= AscW("f"C) Then
						AddCh()
						GoTo Case_8
					ElseIf ch = 39 Then
						AddCh()
						GoTo Case_9
					Else
						GoTo Case_0
					End If
				Case 9
				Case_9:
					t.kind = 5
				Case 10
				Case_10:
					recEnd = pos
					recKind = 44
					If ch >= AscW("0"C) AndAlso ch <= AscW("9"C) OrElse ch >= AscW("A"C) AndAlso ch <= AscW("Z"C) OrElse ch = AscW("_"C) OrElse ch >= AscW("a"C) AndAlso ch <= AscW("z"C) Then
						AddCh()
						GoTo Case_10
					Else
						t.kind = 44
					End If
				Case 11
				Case_11:
					recEnd = pos
					recKind = 45
					If ch >= AscW("-"C) AndAlso ch <= AscW("."C) OrElse ch >= AscW("0"C) AndAlso ch <= AscW(":"C) OrElse ch >= AscW("A"C) AndAlso ch <= AscW("Z"C) OrElse ch = AscW("_"C) OrElse ch >= AscW("a"C) AndAlso ch <= AscW("z"C) Then
						AddCh()
						GoTo Case_11
					Else
						t.kind = 45
					End If
				Case 12
				Case_12:
					If ch <= 9 OrElse ch >= 11 AndAlso ch <= 12 OrElse ch >= 14 AndAlso ch <= AscW("!"C) OrElse ch >= AscW("#"C) AndAlso ch <= AscW("["C) OrElse ch >= AscW("]"C) AndAlso ch <= AscW(Char.MaxValue) Then
						AddCh()
						GoTo Case_12
					ElseIf ch = 10 OrElse ch = 13 Then
						AddCh()
						GoTo Case_4
					ElseIf ch = AscW(""""C) Then
						AddCh()
						GoTo Case_3
					ElseIf ch = 92 Then
						AddCh()
						GoTo Case_14
					Else
						GoTo Case_0
					End If
				Case 13
				Case_13:
					recEnd = pos
					recKind = 44
					If ch >= AscW("0"C) AndAlso ch <= AscW("9"C) Then
						AddCh()
						GoTo Case_10
					ElseIf ch >= AscW("A"C) AndAlso ch <= AscW("Z"C) OrElse ch = AscW("_"C) OrElse ch >= AscW("a"C) AndAlso ch <= AscW("z"C) Then
						AddCh()
						GoTo Case_15
					Else
						t.kind = 44
					End If
				Case 14
				Case_14:
					If ch >= AscW(" "C) AndAlso ch <= AscW("~"C) Then
						AddCh()
						GoTo Case_12
					Else
						GoTo Case_0
					End If
				Case 15
				Case_15:
					recEnd = pos
					recKind = 44
					If ch >= AscW("0"C) AndAlso ch <= AscW("9"C) Then
						AddCh()
						GoTo Case_10
					ElseIf ch >= AscW("A"C) AndAlso ch <= AscW("Z"C) OrElse ch = AscW("_"C) OrElse ch >= AscW("a"C) AndAlso ch <= AscW("z"C) Then
						AddCh()
						GoTo Case_15
					ElseIf ch = AscW("="C) Then
						AddCh()
						GoTo Case_11
					Else
						t.kind = 44
					End If
				Case 16
				Case_16:
					t.kind = 18
				Case 17
				Case_17:
					t.kind = 21
				Case 18
				Case_18:
					t.kind = 22
				Case 19
				Case_19:
					t.kind = 23
				Case 20
				Case_20:
					t.kind = 25
				Case 21
				Case_21:
					t.kind = 27
				Case 22
				Case_22:
					t.kind = 28
				Case 23
				Case_23:
					t.kind = 29
				Case 24
				Case_24:
					t.kind = 30
				Case 25
				Case_25:
					t.kind = 33
				Case 26
				Case_26:
					t.kind = 34
				Case 27
				Case_27:
					t.kind = 35
				Case 28
				Case_28:
					t.kind = 36
				Case 29
				Case_29:
					t.kind = 37
				Case 30
				Case_30:
					t.kind = 41
				Case 31
				Case_31:
					t.kind = 42
				Case 32
				Case_32:
					recEnd = pos
					recKind = 19
					If ch = AscW("."C) Then
						AddCh()
						GoTo Case_19
					ElseIf ch = AscW(">"C) Then
						AddCh()
						GoTo Case_23
					ElseIf ch = AscW(")"C) Then
						AddCh()
						GoTo Case_31
					Else
						t.kind = 19
					End If
				Case 33
				Case_33:
					recEnd = pos
					recKind = 26
					If ch = AscW("."C) Then
						AddCh()
						GoTo Case_22
					Else
						t.kind = 26
					End If
				Case 34
				Case_34:
					recEnd = pos
					recKind = 32
					If ch = AscW("."C) Then
						AddCh()
						GoTo Case_30
					Else
						t.kind = 32
					End If
			End Select
			t.val = New String(tval, 0, tlen)
			Return t
		End Function
		Private Sub SetScannerBehindT()
			buffer.Pos = t.pos
			NextCh()
			line = t.line
			col = t.col
			For i As Integer = 0 To tlen - 1
				NextCh()
			Next
		End Sub
		' get the next token (possibly a token already seen during peeking)
		Public Function Scan() As Token
			If tokens.[next] Is Nothing Then
				Return NextToken()
			Else
				tokens = tokens.[next]
				pt = tokens
				Return tokens
			End If
		End Function
		' peek for the next token, ignore pragmas
		Public Function Peek() As Token
			Do
				If pt.[next] Is Nothing Then
					pt.[next] = NextToken()
				End If
				pt = pt.[next]
			Loop While pt.kind > maxT ' skip pragmas
			Return pt
		End Function
		' make sure that peeking starts at the current scan position
		Public Sub ResetPeek()
			pt = tokens
		End Sub
	End Class

End Namespace