using System;
using System.IO;
using System.Collections.Generic;
using CocoRCore;

namespace 
CocoRCore.Samples.Taste
{



	//-----------------------------------------------------------------------------------
	// Scanner
	//-----------------------------------------------------------------------------------
	public class Scanner : ScannerBase
	{
        private const int _maxT = 30;
        private const int noSym = 30;

		protected override int maxT => _maxT;

		private static readonly Dictionary<int, int> start = new Dictionary<int, int>(); // maps first token character to start state
		static Scanner() 
		{
            for (var i = 65; i <= 90; ++i) start[i] = 1;
            for (var i = 97; i <= 122; ++i) start[i] = 1;
            for (var i = 196; i <= 196; ++i) start[i] = 1;
            for (var i = 214; i <= 214; ++i) start[i] = 1;
            for (var i = 220; i <= 220; ++i) start[i] = 1;
            for (var i = 223; i <= 223; ++i) start[i] = 1;
            for (var i = 228; i <= 228; ++i) start[i] = 1;
            for (var i = 246; i <= 246; ++i) start[i] = 1;
            for (var i = 252; i <= 252; ++i) start[i] = 1;
            for (var i = 48; i <= 57; ++i) start[i] = 2;
            start[62] = 16; 
            start[43] = 4; 
            start[45] = 5; 
            start[42] = 6; 
            start[47] = 7; 
            start[40] = 8; 
            start[41] = 9; 
            start[123] = 10; 
            start[125] = 11; 
            start[61] = 17; 
            start[60] = 13; 
            start[59] = 14; 
            start[44] = 15; 
            start[EOF] = -1;
		}
	
		public Scanner()
		{
		}
				

        bool Cmt1(Position bm)
        {
            if (ch != '#') return false;
            var level = 1;
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


        bool Cmt2(Position bm)
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


        bool Cmt3(Position bm)
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
                case "true": t.kind = 6; break;
                case "false": t.kind = 7; break;
                case "void": t.kind = 10; break;
                case "rel": t.kind = 20; break;
                case "if": t.kind = 21; break;
                case "else": t.kind = 22; break;
                case "while": t.kind = 23; break;
                case "read": t.kind = 24; break;
                case "write": t.kind = 25; break;
                case "program": t.kind = 26; break;
                case "int": t.kind = 27; break;
                case "bool": t.kind = 28; break;
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
            if (Cmt1(bm) || Cmt2(bm) || Cmt3(bm))
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
                    if ('0' <= ch && ch <= '9' || 'A' <= ch && ch <= 'Z' || 'a' <= ch && ch <= 'z' || ch == 196 || ch == 214 || ch == 220 || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
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
                    {
                        t.kind = 5; break;
                    }
                case 6:
                    {
                        t.kind = 8; break;
                    }
                case 7:
                    {
                        t.kind = 9; break;
                    }
                case 8:
                    {
                        t.kind = 11; break;
                    }
                case 9:
                    {
                        t.kind = 12; break;
                    }
                case 10:
                    {
                        t.kind = 13; break;
                    }
                case 11:
                    {
                        t.kind = 14; break;
                    }
                case 12:
                    {
                        t.kind = 15; break;
                    }
                case 13:
                    {
                        t.kind = 16; break;
                    }
                case 14:
                    {
                        t.kind = 19; break;
                    }
                case 15:
                    {
                        t.kind = 29; break;
                    }
                case 16:
                    recEnd = buffer.Position; recKind = 17;
                    if (ch == '=') {
                        AddCh(); goto case 3;
                    } else {
                        t.kind = 17; break;
                    }
                case 17:
                    recEnd = buffer.Position; recKind = 18;
                    if (ch == '=') {
                        AddCh(); goto case 12;
                    } else {
                        t.kind = 18; break;
                    }
			}
			t.setValue(tval.ToString(), casingString);
			return t.Freeze(buffer.Position, buffer.PositionM1);
		}
		
	} // end Scanner

    }
