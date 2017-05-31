using System;
using System.IO;
using System.Collections.Generic;
using CocoRCore;

namespace 
CocoRCore.Samples.WFModel
{



	//-----------------------------------------------------------------------------------
	// Scanner
	//-----------------------------------------------------------------------------------
	public class Scanner : ScannerBase
	{
        private const int _maxT = 72;
        private const int noSym = 72;

		protected override int maxT => _maxT;

		private static readonly Dictionary<int, int> start = new Dictionary<int, int>(); // maps first token character to start state
		static Scanner() 
		{
            for (var i = 97; i <= 114; ++i) start[i] = 21;
            for (var i = 116; i <= 122; ++i) start[i] = 21;
            for (var i = 223; i <= 223; ++i) start[i] = 21;
            for (var i = 228; i <= 228; ++i) start[i] = 21;
            for (var i = 246; i <= 246; ++i) start[i] = 21;
            for (var i = 252; i <= 252; ++i) start[i] = 21;
            for (var i = 48; i <= 57; ++i) start[i] = 22;
            start[45] = 23; 
            start[34] = 5; 
            start[123] = 7; 
            start[40] = 9; 
            start[46] = 11; 
            start[124] = 12; 
            start[58] = 13; 
            start[91] = 18; 
            start[61] = 27; 
            start[35] = 28; 
            start[115] = 31; 
            start[EOF] = -1;
		}
	
		public Scanner()
		{
            casing = char.ToLowerInvariant;
            casingString = ScannerBase.ToLower;
		}
				

        bool Cmt1(Position bm)
        {
            if (ch != 39) return false;
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



		protected override void CheckLiteral() 
		{
			// t.val is already lowercase if the scanner is ignorecase
			switch (t.val) {
                case "end": t.kind = 8; break;
                case "version": t.kind = 13; break;
                case "search": t.kind = 14; break;
                case "select": t.kind = 15; break;
                case "details": t.kind = 16; break;
                case "edit": t.kind = 17; break;
                case "clear": t.kind = 18; break;
                case "keys": t.kind = 19; break;
                case "displayname": t.kind = 20; break;
                case "namespace": t.kind = 22; break;
                case "readerwriterprefix": t.kind = 23; break;
                case "rootclass": t.kind = 24; break;
                case "data": t.kind = 25; break;
                case "class": t.kind = 26; break;
                case "via": t.kind = 27; break;
                case "inherits": t.kind = 28; break;
                case "property": t.kind = 29; break;
                case "infoproperty": t.kind = 30; break;
                case "approperty": t.kind = 31; break;
                case "list": t.kind = 32; break;
                case "selectlist": t.kind = 33; break;
                case "flagslist": t.kind = 34; break;
                case "longproperty": t.kind = 35; break;
                case "infolongproperty": t.kind = 36; break;
                case "true": t.kind = 38; break;
                case "false": t.kind = 39; break;
                case "as": t.kind = 41; break;
                case "double": t.kind = 42; break;
                case "date": t.kind = 43; break;
                case "datetime": t.kind = 44; break;
                case "integer": t.kind = 45; break;
                case "percent": t.kind = 46; break;
                case "percentwithdefault": t.kind = 47; break;
                case "doublewithdefault": t.kind = 48; break;
                case "integerwithdefault": t.kind = 49; break;
                case "n2": t.kind = 50; break;
                case "n0": t.kind = 51; break;
                case "string": t.kind = 52; break;
                case "boolean": t.kind = 53; break;
                case "guid": t.kind = 54; break;
                case "xml": t.kind = 56; break;
                case "mimics": t.kind = 57; break;
                case "query": t.kind = 58; break;
                case "txt": t.kind = 59; break;
                case "xl": t.kind = 60; break;
                case "ref": t.kind = 61; break;
                case "subsystem": t.kind = 62; break;
                case "ssname": t.kind = 63; break;
                case "ssconfig": t.kind = 64; break;
                case "sstyp": t.kind = 65; break;
                case "sscommands": t.kind = 66; break;
                case "sskey": t.kind = 67; break;
                case "ssclear": t.kind = 68; break;
                case "flags": t.kind = 69; break;
                case "enum": t.kind = 70; break;
                case "default": t.kind = 71; break;
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
            if (Cmt1(bm))
                return NextToken();
            var apx = 0;
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
                    {
                        tval.Length -= apx;
                        SetScannerBackBehindT();
                        t.kind = 2; break;
                    }
                case 2:
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 3;
                    } else {
                        goto case 0;
                    }
                case 3:
                    recEnd = buffer.Position; recKind = 3;
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 3;
                    } else if (ch == '#' || ch == 'r') {
                        AddCh(); goto case 4;
                    } else {
                        t.kind = 3; break;
                    }
                case 4:
                    {
                        t.kind = 3; break;
                    }
                case 5:
                    if (ch <= 9 || 11 <= ch && ch <= 12 || 14 <= ch && ch <= '!' || '#' <= ch && ch <= 65535) {
                        AddCh(); goto case 5;
                    } else if (ch == '"') {
                        AddCh(); goto case 6;
                    } else {
                        goto case 0;
                    }
                case 6:
                    {
                        t.kind = 5; break;
                    }
                case 7:
                    if (ch <= '|' || '~' <= ch && ch <= 65535) {
                        AddCh(); goto case 7;
                    } else if (ch == '}') {
                        AddCh(); goto case 8;
                    } else {
                        goto case 0;
                    }
                case 8:
                    {
                        t.kind = 6; break;
                    }
                case 9:
                    if (ch <= '(' || '*' <= ch && ch <= 65535) {
                        AddCh(); goto case 9;
                    } else if (ch == ')') {
                        AddCh(); goto case 10;
                    } else {
                        goto case 0;
                    }
                case 10:
                    {
                        t.kind = 7; break;
                    }
                case 11:
                    {
                        t.kind = 9; break;
                    }
                case 12:
                    {
                        t.kind = 10; break;
                    }
                case 13:
                    {
                        t.kind = 11; break;
                    }
                case 14:
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 15;
                    } else {
                        goto case 0;
                    }
                case 15:
                    if (ch == '.') {
                        AddCh(); goto case 16;
                    } else {
                        goto case 0;
                    }
                case 16:
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 17;
                    } else {
                        goto case 0;
                    }
                case 17:
                    {
                        t.kind = 12; break;
                    }
                case 18:
                    if ('a' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 19;
                    } else {
                        goto case 0;
                    }
                case 19:
                    if ('0' <= ch && ch <= '9' || ch == '_' || 'a' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 19;
                    } else if (ch == ']') {
                        AddCh(); goto case 20;
                    } else {
                        goto case 0;
                    }
                case 20:
                    {
                        t.kind = 21; break;
                    }
                case 21:
                    recEnd = buffer.Position; recKind = 1;
                    if ('0' <= ch && ch <= '9' || ch == '_' || 'a' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 21;
                    } else if (ch == '.') {
                        apx++; 
                        AddCh(); goto case 1;
                    } else {
                        t.kind = 1;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 22:
                    recEnd = buffer.Position; recKind = 4;
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 24;
                    } else if (ch == '.') {
                        AddCh(); goto case 25;
                    } else {
                        t.kind = 4; break;
                    }
                case 23:
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 24;
                    } else {
                        goto case 0;
                    }
                case 24:
                    recEnd = buffer.Position; recKind = 4;
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 24;
                    } else if (ch == '.') {
                        AddCh(); goto case 2;
                    } else {
                        t.kind = 4; break;
                    }
                case 25:
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 26;
                    } else {
                        goto case 0;
                    }
                case 26:
                    recEnd = buffer.Position; recKind = 3;
                    if ('0' <= ch && ch <= '9') {
                        AddCh(); goto case 3;
                    } else if (ch == '#' || ch == 'r') {
                        AddCh(); goto case 4;
                    } else if (ch == '.') {
                        AddCh(); goto case 14;
                    } else {
                        t.kind = 3; break;
                    }
                case 27:
                    {
                        t.kind = 37; break;
                    }
                case 28:
                    {
                        t.kind = 40; break;
                    }
                case 29:
                    if (ch == ')') {
                        AddCh(); goto case 30;
                    } else {
                        goto case 0;
                    }
                case 30:
                    {
                        t.kind = 55; break;
                    }
                case 31:
                    recEnd = buffer.Position; recKind = 1;
                    if ('0' <= ch && ch <= '9' || ch == '_' || 'a' <= ch && ch <= 's' || 'u' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 21;
                    } else if (ch == '.') {
                        apx++; 
                        AddCh(); goto case 1;
                    } else if (ch == 't') {
                        AddCh(); goto case 32;
                    } else {
                        t.kind = 1;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 32:
                    recEnd = buffer.Position; recKind = 1;
                    if ('0' <= ch && ch <= '9' || ch == '_' || 'a' <= ch && ch <= 'q' || 's' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 21;
                    } else if (ch == '.') {
                        apx++; 
                        AddCh(); goto case 1;
                    } else if (ch == 'r') {
                        AddCh(); goto case 33;
                    } else {
                        t.kind = 1;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 33:
                    recEnd = buffer.Position; recKind = 1;
                    if ('0' <= ch && ch <= '9' || ch == '_' || 'a' <= ch && ch <= 'h' || 'j' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 21;
                    } else if (ch == '.') {
                        apx++; 
                        AddCh(); goto case 1;
                    } else if (ch == 'i') {
                        AddCh(); goto case 34;
                    } else {
                        t.kind = 1;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 34:
                    recEnd = buffer.Position; recKind = 1;
                    if ('0' <= ch && ch <= '9' || ch == '_' || 'a' <= ch && ch <= 'm' || 'o' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 21;
                    } else if (ch == '.') {
                        apx++; 
                        AddCh(); goto case 1;
                    } else if (ch == 'n') {
                        AddCh(); goto case 35;
                    } else {
                        t.kind = 1;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 35:
                    recEnd = buffer.Position; recKind = 1;
                    if ('0' <= ch && ch <= '9' || ch == '_' || 'a' <= ch && ch <= 'f' || 'h' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 21;
                    } else if (ch == '.') {
                        apx++; 
                        AddCh(); goto case 1;
                    } else if (ch == 'g') {
                        AddCh(); goto case 36;
                    } else {
                        t.kind = 1;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
                case 36:
                    recEnd = buffer.Position; recKind = 1;
                    if ('0' <= ch && ch <= '9' || ch == '_' || 'a' <= ch && ch <= 'z' || ch == 223 || ch == 228 || ch == 246 || ch == 252) {
                        AddCh(); goto case 21;
                    } else if (ch == '.') {
                        apx++; 
                        AddCh(); goto case 1;
                    } else if (ch == '(') {
                        AddCh(); goto case 29;
                    } else {
                        t.kind = 1;
                        t.setValue(tval.ToString(), casingString);
                        CheckLiteral();
                        return t.Freeze(buffer.Position, buffer.PositionM1);
                    }
			}
			t.setValue(tval.ToString(), casingString);
			return t.Freeze(buffer.Position, buffer.PositionM1);
		}
		
	} // end Scanner

    }
