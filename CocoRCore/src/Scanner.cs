using System;
using System.IO;
using System.Collections.Generic;
using CocoRCore;

namespace 
CocoRCore.CSharp
{



	//-----------------------------------------------------------------------------------
	// Scanner
	//-----------------------------------------------------------------------------------
	public class Scanner : ScannerBase
	{
        private const int _maxT = 52;
        private const int noSym = 52;

		protected override int maxT => _maxT;

		private static readonly Dictionary<int, int> start = new Dictionary<int, int>(); // maps first token character to start state
		static Scanner() 
		{
            for (var i = 65; i <= 90; ++i) start[i] = 1;
            for (var i = 95; i <= 95; ++i) start[i] = 1;
            for (var i = 97; i <= 122; ++i) start[i] = 1;
            for (var i = 48; i <= 57; ++i) start[i] = 2;
            start[34] = 11; 
            start[39] = 12; 
            start[36] = 13; 
            start[46] = 35; 
            start[61] = 16; 
            start[40] = 36; 
            start[44] = 17; 
            start[41] = 18; 
            start[43] = 19; 
            start[45] = 20; 
            start[58] = 22; 
            start[60] = 37; 
            start[62] = 23; 
            start[124] = 26; 
            start[91] = 27; 
            start[93] = 28; 
            start[123] = 29; 
            start[125] = 30; 
            start[94] = 31; 
            start[35] = 32; 
            start[EOF] = -1;
		}
	
		public Scanner()
		{
		}
				

        bool Cmt1(Position bm)
        {
            if (ch != '/') return false;
            var level = 1;
            NextCh();
            if (ch == '/') 
            {
                NextCh();
                for(;;)
                {
                    if (ch == 10)
                    {
                        level--;
                        if (level == 0) { NextCh(); return true; }
                        NextCh();
                    }
                    else if (ch == EOF)
                        return false;
                    else
                        NextCh();
                }
            }
            else
                buffer.ResetPositionTo(bm);
            return false;
        }


        bool Cmt2(Position bm)
        {
            if (ch != '/') return false;
            var level = 1;
            NextCh();
            if (ch == '*') 
            {
                NextCh();
                for(;;)
                {
                    if (ch == '*')
                    {
                        NextCh();
                        if (ch == '/')
                        {
                            level--;
                            if (level == 0) { NextCh(); return true; }
                            NextCh();
                        }
                    }
                    else if (ch == '/')
                    {
                        NextCh();
                        if (ch == '*')
                        {
                            level++;
                            NextCh();
                        }
                    }
                    else if (ch == EOF)
                        return false;
                    else
                        NextCh();
                }
            }
            else
                buffer.ResetPositionTo(bm);
            return false;
        }



		protected override void CheckLiteral() 
		{
			// t.val is already lowercase if the scanner is ignorecase
			switch (t.val) {
                case "COMPILER": t.kind = 7; break;
                case "IGNORECASE": t.kind = 8; break;
                case "CHARACTERS": t.kind = 9; break;
                case "TOKENS": t.kind = 10; break;
                case "PRAGMAS": t.kind = 11; break;
                case "COMMENTS": t.kind = 12; break;
                case "FROM": t.kind = 13; break;
                case "TO": t.kind = 14; break;
                case "NESTED": t.kind = 15; break;
                case "IGNORE": t.kind = 16; break;
                case "SYMBOLTABLES": t.kind = 17; break;
                case "PRODUCTIONS": t.kind = 18; break;
                case "END": t.kind = 19; break;
                case "DELETEABLE": t.kind = 21; break;
                case "STRICT": t.kind = 23; break;
                case "SCOPES": t.kind = 24; break;
                case "USEONCE": t.kind = 28; break;
                case "USEALL": t.kind = 29; break;
                case "ANY": t.kind = 33; break;
                case "WEAK": t.kind = 40; break;
                case "SYNC": t.kind = 45; break;
                case "IF": t.kind = 48; break;
                case "CONTEXT": t.kind = 49; break;
                default: break;
			}
		}

		protected override Token NextToken() 
		{
			while (ch == ' '
                || 9 <= ch && ch <= 10 || ch == 13
			) 
				NextCh();
            var bm = buffer.PositionM1; // comment(s)
            if (Cmt1(bm) || Cmt2(bm))
                return NextToken();
			var recKind = noSym;
			var recEnd = buffer.Position;
			t = new Token.Builder(buffer);
			start.TryGetValue(ch, out var state); // state = 0 if not found; state = -1 if EOF;
			tval.Clear(); AddCh();
			
			switch (state) 
			{
				case -1: 
					t.kind = _EOF;
					break;
					// NextCh already done
				case 0: 
					if (recKind != noSym) 
					{
						tval.Length = recEnd.pos - t.position.pos;
						SetScannerBackBehindT();
					}
					t.kind = recKind; break;
					// NextCh already done
                case 1:
                    recEnd = buffer.Position; recKind = 1;
                    if ('0' <= ch && ch <= '9' || 'A' <= ch && ch <= 'Z' || ch == '_' || 'a' <= ch && ch <= 'z') {
                        AddCh(); goto case 1;
                    } else {
                        t.kind = 1;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 2:
                    recEnd = buffer.Position; recKind = 2;
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 2;
                    } else {
                        t.kind = 2; break;
                    }
                case 3:
                    {
                        t.kind = 3; break;
                    }
                case 4:
                    {
                        t.kind = 4; break;
                    }
                case 5:
                    if (ch == 39) {
                        AddCh(); goto case 8;
                    } else {
                        goto case 0;
                    }
                case 6:
                    if (' ' <= ch && ch <= '~') {
                        AddCh(); goto case 7;
                    } else {
                        goto case 0;
                    }
                case 7:
                    if ('0' <= ch && ch <= '9' || 'a' <= ch && ch <= 'f') {
                        AddCh(); goto case 7;
                    } else if (ch == 39) {
                        AddCh(); goto case 8;
                    } else {
                        goto case 0;
                    }
                case 8:
                    {
                        t.kind = 5; break;
                    }
                case 9:
                    recEnd = buffer.Position; recKind = 53;
                    if ('0' <= ch && ch <= '9' || 'A' <= ch && ch <= 'Z' || ch == '_' || 'a' <= ch && ch <= 'z') {
                        AddCh(); goto case 9;
                    } else {
                        t.kind = 53; break;
                    }
                case 10:
                    recEnd = buffer.Position; recKind = 54;
                    if ('-' <= ch && ch <= '.' || '0' <= ch && ch <= ':' || 'A' <= ch && ch <= 'Z' || ch == '_' || 'a' <= ch && ch <= 'z') {
                        AddCh(); goto case 10;
                    } else {
                        t.kind = 54; break;
                    }
                case 11:
                    if (ch <= 9 || 11 <= ch && ch <= 12 || 14 <= ch && ch <= '!' || '#' <= ch && ch <= '[' || ']' <= ch && ch <= 65535) {
                        AddCh(); goto case 11;
                    } else if (ch == 10 || ch == 13) {
                        AddCh(); goto case 4;
                    } else if (ch == '"') {
                        AddCh(); goto case 3;
                    } else if (ch == 92) {
                        AddCh(); goto case 14;
                    } else {
                        goto case 0;
                    }
                case 12:
                    recEnd = buffer.Position; recKind = 6;
                    if (ch <= 9 || 11 <= ch && ch <= 12 || 14 <= ch && ch <= '&' || '(' <= ch && ch <= '[' || ']' <= ch && ch <= 65535) {
                        AddCh(); goto case 5;
                    } else if (ch == 92) {
                        AddCh(); goto case 6;
                    } else {
                        t.kind = 6; break;
                    }
                case 13:
                    recEnd = buffer.Position; recKind = 53;
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 9;
                    } else if ('A' <= ch && ch <= 'Z' || ch == '_' || 'a' <= ch && ch <= 'z') {
                        AddCh(); goto case 15;
                    } else {
                        t.kind = 53; break;
                    }
                case 14:
                    if (' ' <= ch && ch <= '~') {
                        AddCh(); goto case 11;
                    } else {
                        goto case 0;
                    }
                case 15:
                    recEnd = buffer.Position; recKind = 53;
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 9;
                    } else if ('A' <= ch && ch <= 'Z' || ch == '_' || 'a' <= ch && ch <= 'z') {
                        AddCh(); goto case 15;
                    } else if (ch == '=') {
                        AddCh(); goto case 10;
                    } else {
                        t.kind = 53; break;
                    }
                case 16:
                    {
                        t.kind = 22; break;
                    }
                case 17:
                    {
                        t.kind = 26; break;
                    }
                case 18:
                    {
                        t.kind = 27; break;
                    }
                case 19:
                    {
                        t.kind = 30; break;
                    }
                case 20:
                    {
                        t.kind = 31; break;
                    }
                case 21:
                    {
                        t.kind = 32; break;
                    }
                case 22:
                    {
                        t.kind = 34; break;
                    }
                case 23:
                    {
                        t.kind = 36; break;
                    }
                case 24:
                    {
                        t.kind = 37; break;
                    }
                case 25:
                    {
                        t.kind = 38; break;
                    }
                case 26:
                    {
                        t.kind = 39; break;
                    }
                case 27:
                    {
                        t.kind = 41; break;
                    }
                case 28:
                    {
                        t.kind = 42; break;
                    }
                case 29:
                    {
                        t.kind = 43; break;
                    }
                case 30:
                    {
                        t.kind = 44; break;
                    }
                case 31:
                    {
                        t.kind = 46; break;
                    }
                case 32:
                    {
                        t.kind = 47; break;
                    }
                case 33:
                    {
                        t.kind = 50; break;
                    }
                case 34:
                    {
                        t.kind = 51; break;
                    }
                case 35:
                    recEnd = buffer.Position; recKind = 20;
                    if (ch == '.') {
                        AddCh(); goto case 21;
                    } else if (ch == '>') {
                        AddCh(); goto case 25;
                    } else if (ch == ')') {
                        AddCh(); goto case 34;
                    } else {
                        t.kind = 20; break;
                    }
                case 36:
                    recEnd = buffer.Position; recKind = 25;
                    if (ch == '.') {
                        AddCh(); goto case 33;
                    } else {
                        t.kind = 25; break;
                    }
                case 37:
                    recEnd = buffer.Position; recKind = 35;
                    if (ch == '.') {
                        AddCh(); goto case 24;
                    } else {
                        t.kind = 35; break;
                    }
			}
			t.setValue(tval.ToString(), casingString);
			return t.Freeze(buffer.Position, buffer.PositionM1);
		}
		
	} // end Scanner

    }
