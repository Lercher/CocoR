using System;
using System.IO;
using System.Collections.Generic;
using CocoRCore;

namespace 
CocoRCore.Samples.Inheritance
{



	//-----------------------------------------------------------------------------------
	// Scanner
	//-----------------------------------------------------------------------------------
	public class Scanner : ScannerBase
	{
        private const int _maxT = 17;
        private const int noSym = 17;

		protected override int maxT => _maxT;

		private static readonly Dictionary<int, int> start = new Dictionary<int, int>(); // maps first token character to start state
		static Scanner() 
		{
            for (var i = 48; i <= 57; ++i) start[i] = 2;
            for (var i = 95; i <= 95; ++i) start[i] = 3;
            for (var i = 97; i <= 117; ++i) start[i] = 3;
            for (var i = 119; i <= 122; ++i) start[i] = 3;
            for (var i = 228; i <= 228; ++i) start[i] = 3;
            for (var i = 246; i <= 246; ++i) start[i] = 3;
            for (var i = 252; i <= 252; ++i) start[i] = 3;
            start[45] = 1; 
            start[118] = 11; 
            start[58] = 10; 
            start[123] = 14; 
            start[125] = 15; 
            start[59] = 16; 
            start[EOF] = -1;
		}
	
		public Scanner()
		{
            casing = char.ToLowerInvariant;
            casingString = ScannerBase.ToLower;
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
                case "keywordcamelcase": t.kind = 3; break;
                case "var": t.kind = 4; break;
                case "type": t.kind = 14; break;
                case "as": t.kind = 15; break;
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
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 2;
                    } else {
                        goto case 0;
                    }
                case 2:
                    recEnd = buffer.Position; recKind = 1;
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 2;
                    } else {
                        t.kind = 1; break;
                    }
                case 3:
                    recEnd = buffer.Position; recKind = 2;
                    if (ch == '_' || 'a' <= ch && ch <= 'z' || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 3;
                    } else {
                        t.kind = 2;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 4:
                    {
                        t.kind = 5; break;
                    }
                case 5:
                    {
                        t.kind = 6; break;
                    }
                case 6:
                    {
                        t.kind = 7; break;
                    }
                case 7:
                    {
                        t.kind = 8; break;
                    }
                case 8:
                    {
                        t.kind = 9; break;
                    }
                case 9:
                    {
                        t.kind = 10; break;
                    }
                case 10:
                    {
                        t.kind = 11; break;
                    }
                case 11:
                    recEnd = buffer.Position; recKind = 2;
                    if (ch == '_' || 'b' <= ch && ch <= 'z' || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 3;
                    } else if (ch == 'a') {
                        AddCh(); goto case 12;
                    } else {
                        t.kind = 2;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 12:
                    recEnd = buffer.Position; recKind = 2;
                    if (ch == '_' || 'a' <= ch && ch <= 'q' || 's' <= ch && ch <= 'z' || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 3;
                    } else if (ch == 'r') {
                        AddCh(); goto case 13;
                    } else {
                        t.kind = 2;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 13:
                    recEnd = buffer.Position; recKind = 2;
                    if (ch == '_' || 'a' <= ch && ch <= 'z' || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 3;
                    } else if (ch == '1') {
                        AddCh(); goto case 4;
                    } else if (ch == '2') {
                        AddCh(); goto case 5;
                    } else if (ch == '3') {
                        AddCh(); goto case 6;
                    } else if (ch == '4') {
                        AddCh(); goto case 7;
                    } else if (ch == '5') {
                        AddCh(); goto case 8;
                    } else if (ch == '6') {
                        AddCh(); goto case 9;
                    } else {
                        t.kind = 2;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 14:
                    {
                        t.kind = 12; break;
                    }
                case 15:
                    {
                        t.kind = 13; break;
                    }
                case 16:
                    {
                        t.kind = 16; break;
                    }
			}
			t.setValue(tval.ToString(), casingString);
			return t.Freeze(buffer.Position, buffer.PositionM1);
		}
		
	} // end Scanner

    }
