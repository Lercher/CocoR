using System;
using System.IO;
using System.Collections.Generic;
using CocoRCore;

namespace 
CocoRCore.Samples.ExternalTables
{



	//-----------------------------------------------------------------------------------
	// Scanner
	//-----------------------------------------------------------------------------------
	public class Scanner : ScannerBase
	{
        private const int _maxT = 63;
        private const int noSym = 63;

		protected override int maxT => _maxT;

		private static readonly Dictionary<int, int> start = new Dictionary<int, int>(); // maps first token character to start state
		static Scanner() 
		{
            for (var i = 48; i <= 57; ++i) start[i] = 2;
            for (var i = 95; i <= 95; ++i) start[i] = 3;
            for (var i = 97; i <= 122; ++i) start[i] = 3;
            for (var i = 223; i <= 223; ++i) start[i] = 3;
            for (var i = 228; i <= 228; ++i) start[i] = 3;
            for (var i = 246; i <= 246; ++i) start[i] = 3;
            for (var i = 252; i <= 252; ++i) start[i] = 3;
            for (var i = 39; i <= 39; ++i) start[i] = 13;
            for (var i = 42; i <= 42; ++i) start[i] = 15;
            start[45] = 1; 
            start[59] = 4; 
            start[40] = 5; 
            start[41] = 6; 
            start[47] = 7; 
            start[46] = 8; 
            start[44] = 9; 
            start[61] = 10; 
            start[124] = 11; 
            start[EOF] = -1;
		}
	
		public Scanner()
		{
            casing = char.ToLowerInvariant;
            casingString = ScannerBase.ToLower;
		}
				

        bool Cmt1(Position bm)
        {
            if (ch != '-') return false;
            var level = 1;
            NextCh();
            if (ch == '-') 
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
                case "set": t.kind = 3; break;
                case "serveroutput": t.kind = 4; break;
                case "on": t.kind = 5; break;
                case "size": t.kind = 6; break;
                case "insert": t.kind = 7; break;
                case "update": t.kind = 8; break;
                case "delete": t.kind = 9; break;
                case "into": t.kind = 10; break;
                case "values": t.kind = 11; break;
                case "prompt": t.kind = 12; break;
                case "null": t.kind = 13; break;
                case "lantusparam": t.kind = 14; break;
                case "tusparam": t.kind = 15; break;
                case "tusnom": t.kind = 16; break;
                case "tupcode": t.kind = 17; break;
                case "tupflagorfi": t.kind = 18; break;
                case "tuplibelle": t.kind = 19; break;
                case "declare": t.kind = 30; break;
                case "begin": t.kind = 31; break;
                case "commit": t.kind = 32; break;
                case "exception": t.kind = 33; break;
                case "when": t.kind = 34; break;
                case "others": t.kind = 35; break;
                case "then": t.kind = 36; break;
                case "rollback": t.kind = 37; break;
                case "end": t.kind = 38; break;
                case "where": t.kind = 39; break;
                case "and": t.kind = 40; break;
                case "from": t.kind = 41; break;
                case "ttrparam": t.kind = 42; break;
                case "ttrnom": t.kind = 43; break;
                case "ttpcode": t.kind = 44; break;
                case "lanttrparam": t.kind = 45; break;
                case "lancode": t.kind = 46; break;
                case "ttplibelle": t.kind = 47; break;
                case "ttphelptext": t.kind = 48; break;
                case "ttraitement": t.kind = 49; break;
                case "ttrflagpref": t.kind = 50; break;
                case "lanttraitement": t.kind = 51; break;
                case "ttrlibelle": t.kind = 52; break;
                case "ttrcontext": t.kind = 53; break;
                case "tuser": t.kind = 54; break;
                case "tuslongueur": t.kind = 55; break;
                case "lantuser": t.kind = 56; break;
                case "tuslibelle": t.kind = 57; break;
                case "tuphelptext": t.kind = 58; break;
                case "dbms_output": t.kind = 59; break;
                case "put_line": t.kind = 60; break;
                case "chr": t.kind = 61; break;
                case "sqlerrm": t.kind = 62; break;
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
                    if (ch == '_' || 'a' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 3;
                    } else {
                        t.kind = 2;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 4:
                    {
                        t.kind = 20; break;
                    }
                case 5:
                    {
                        t.kind = 21; break;
                    }
                case 6:
                    {
                        t.kind = 22; break;
                    }
                case 7:
                    {
                        t.kind = 23; break;
                    }
                case 8:
                    {
                        t.kind = 24; break;
                    }
                case 9:
                    {
                        t.kind = 25; break;
                    }
                case 10:
                    {
                        t.kind = 26; break;
                    }
                case 11:
                    if (ch == '|') {
                        AddCh(); goto case 12;
                    } else {
                        goto case 0;
                    }
                case 12:
                    {
                        t.kind = 27; break;
                    }
                case 13:
                    if (ch == 39) {
                        AddCh(); goto case 16;
                    } else if (ch <= 12 || 14 <= ch && ch <= '&' || '(' <= ch && ch <= 65535) {
                        AddCh(); goto case 13;
                    } else if (ch == 13) {
                        AddCh(); goto case 14;
                    } else {
                        goto case 0;
                    }
                case 14:
                    if (ch == 10) {
                        AddCh(); goto case 13;
                    } else {
                        goto case 0;
                    }
                case 15:
                    recEnd = buffer.Position; recKind = 29;
                    if (ch == '*') {
                        AddCh(); goto case 15;
                    } else {
                        t.kind = 29; break;
                    }
                case 16:
                    recEnd = buffer.Position; recKind = 28;
                    if (ch == 39) {
                        AddCh(); goto case 13;
                    } else {
                        t.kind = 28; break;
                    }
			}
			t.setValue(tval.ToString(), casingString);
			return t.Freeze(buffer.Position, buffer.PositionM1);
		}
		
	} // end Scanner

    }
