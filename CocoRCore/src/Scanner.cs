
using System;
using System.IO;
using System.Collections.Generic;
using CocoRCore;

namespace CocoRCore.CSharp {



	//-----------------------------------------------------------------------------------
	// Scanner
	//-----------------------------------------------------------------------------------
	public class Scanner : ScannerBase
	{
		private const int _maxT = 51;
	const int noSym = 51;


		private static readonly Dictionary<int, int> start = new Dictionary<int, int>(); // maps first token character to start state
		static Scanner() 
		{
			for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 95; i <= 95; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 48; i <= 57; ++i) start[i] = 2;
		start[34] = 11; 
		start[39] = 12; 
		start[36] = 13; 
		start[61] = 16; 
		start[46] = 35; 
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
		start[Buffer.EOF] = -1;

		}
	
		public static Scanner Create(string fileName)
		{
			return Create(fileName, false);
		}

		public static Scanner Create(string fileName, bool isBOMFreeUTF8)
		{
			var s = new Scanner();
			s.Initialize(fileName, isBOMFreeUTF8);
			return s;
		}

		public static Scanner Create(Stream st)
		{
			return Create(st, false);
		}

		public static Scanner Create(Stream st, bool isBOMFreeUTF8)
		{
			var s = new Scanner();
			s.Initialize(st, isBOMFreeUTF8);
			return s;
		}
		
		protected override int maxT => _maxT;
		
	
	bool Comment0() {
		int level = 1, pos0 = pos, line0 = line, col0 = col, charPos0 = charPos;
		NextCh();
		if (ch == '/') {
			NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0; charPos = charPos0;
		}
		return false;
	}

	bool Comment1() {
		int level = 1, pos0 = pos, line0 = line, col0 = col, charPos0 = charPos;
		NextCh();
		if (ch == '*') {
			NextCh();
			for(;;) {
				if (ch == '*') {
					NextCh();
					if (ch == '/') {
						level--;
						if (level == 0) { oldEols = line - line0; NextCh(); return true; }
						NextCh();
					}
				} else if (ch == '/') {
					NextCh();
					if (ch == '*') {
						level++; NextCh();
					}
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0; charPos = charPos0;
		}
		return false;
	}


		protected override void CheckLiteral() 
		{
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
			case "END": t.kind = 21; break;
			case "STRICT": t.kind = 22; break;
			case "SCOPES": t.kind = 23; break;
			case "USEONCE": t.kind = 27; break;
			case "USEALL": t.kind = 28; break;
			case "ANY": t.kind = 32; break;
			case "WEAK": t.kind = 39; break;
			case "SYNC": t.kind = 44; break;
			case "IF": t.kind = 47; break;
			case "CONTEXT": t.kind = 48; break;
			default: break;
		}
		}

		protected override Token NextToken() 
		{
			while (ch == ' ' ||
				ch >= 9 && ch <= 10 || ch == 13
			) NextCh();
			if (ch == '/' && Comment0() ||ch == '/' && Comment1()) return NextToken();
			int recKind = noSym;
			int recEnd = pos;
			t = new Token();
			t.pos = pos; t.col = col; t.line = line; t.charPos = charPos;
			int state;
			state = start.ContainsKey(ch) ? start[ch] : 0;
			tval.Clear(); AddCh();
			
			switch (state) 
			{
				case -1: { t.kind = eofSym; break; } // NextCh already done
				case 0: 
					{
						if (recKind != noSym) 
						{
							tval.Length = recEnd - t.pos;
							SetScannerBehindT();
						}
						t.kind = recKind; break;
					} // NextCh already done
				case 1:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = tval.ToString(); CheckLiteral(); return t;}
			case 2:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 2;}
				else {t.kind = 2; break;}
			case 3:
				{t.kind = 3; break;}
			case 4:
				{t.kind = 4; break;}
			case 5:
				if (ch == 39) {AddCh(); goto case 8;}
				else {goto case 0;}
			case 6:
				if (ch >= ' ' && ch <= '~') {AddCh(); goto case 7;}
				else {goto case 0;}
			case 7:
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 7;}
				else if (ch == 39) {AddCh(); goto case 8;}
				else {goto case 0;}
			case 8:
				{t.kind = 5; break;}
			case 9:
				recEnd = pos; recKind = 52;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 9;}
				else {t.kind = 52; break;}
			case 10:
				recEnd = pos; recKind = 53;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= ':' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 10;}
				else {t.kind = 53; break;}
			case 11:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 11;}
				else if (ch == 10 || ch == 13) {AddCh(); goto case 4;}
				else if (ch == '"') {AddCh(); goto case 3;}
				else if (ch == 92) {AddCh(); goto case 14;}
				else {goto case 0;}
			case 12:
				recEnd = pos; recKind = 6;
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 5;}
				else if (ch == 92) {AddCh(); goto case 6;}
				else {t.kind = 6; break;}
			case 13:
				recEnd = pos; recKind = 52;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
				else if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 15;}
				else {t.kind = 52; break;}
			case 14:
				if (ch >= ' ' && ch <= '~') {AddCh(); goto case 11;}
				else {goto case 0;}
			case 15:
				recEnd = pos; recKind = 52;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
				else if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 15;}
				else if (ch == '=') {AddCh(); goto case 10;}
				else {t.kind = 52; break;}
			case 16:
				{t.kind = 19; break;}
			case 17:
				{t.kind = 25; break;}
			case 18:
				{t.kind = 26; break;}
			case 19:
				{t.kind = 29; break;}
			case 20:
				{t.kind = 30; break;}
			case 21:
				{t.kind = 31; break;}
			case 22:
				{t.kind = 33; break;}
			case 23:
				{t.kind = 35; break;}
			case 24:
				{t.kind = 36; break;}
			case 25:
				{t.kind = 37; break;}
			case 26:
				{t.kind = 38; break;}
			case 27:
				{t.kind = 40; break;}
			case 28:
				{t.kind = 41; break;}
			case 29:
				{t.kind = 42; break;}
			case 30:
				{t.kind = 43; break;}
			case 31:
				{t.kind = 45; break;}
			case 32:
				{t.kind = 46; break;}
			case 33:
				{t.kind = 49; break;}
			case 34:
				{t.kind = 50; break;}
			case 35:
				recEnd = pos; recKind = 20;
				if (ch == '.') {AddCh(); goto case 21;}
				else if (ch == '>') {AddCh(); goto case 25;}
				else if (ch == ')') {AddCh(); goto case 34;}
				else {t.kind = 20; break;}
			case 36:
				recEnd = pos; recKind = 24;
				if (ch == '.') {AddCh(); goto case 33;}
				else {t.kind = 24; break;}
			case 37:
				recEnd = pos; recKind = 34;
				if (ch == '.') {AddCh(); goto case 24;}
				else {t.kind = 34; break;}

			}
			t.valScanned = tval.ToString();
			t.val = t.valScanned;

			return t;
		}
		
	} // end Scanner

}