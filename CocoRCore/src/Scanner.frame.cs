// This file has to be compiled and linked to a Coco/R generated scanner.
// as a variant, you can reference the CocoRCore.dll,
// that includes this classes in a compiled form.

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace CocoRCore
{
    //-----------------------------------------------------------------------------------
    // Scanner
    //-----------------------------------------------------------------------------------
    public abstract class ScannerBase
    {
        public string uri;
        protected const char EOL = '\n';
        protected const int eofSym = 0; /* pdt */

        public Buffer buffer; // scanner buffer

        protected Token t;          // current token
        protected int ch;           // current input character (probably lowercased)
        protected char valCh;       // current input character (original version)
        protected int pos;          // byte position of current character
        protected int charPos;      // position by unicode characters starting with 0
        protected int col;          // column number of current character
        protected int line;         // line number of current character
        protected int oldEols;      // EOLs that appeared in a comment;
        protected Token tokens;     // list of tokens already peeked (first token is a dummy)
        protected Token peekToken;         // current peek token
        protected StringBuilder tval = new StringBuilder(capacity: 64); // text of current token

        protected Func<char, char> casing = c => c;

        protected abstract int maxT { get; }

        protected void Initialize(string fileName, bool isBOMFreeUTF8)
        {
            try
            {
                var f = new System.IO.FileInfo(fileName);
                uri = f.FullName;
                Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                buffer = new Buffer(stream, false);
                Init(isBOMFreeUTF8);
            }
            catch (IOException ex)
            {
                throw new FatalError(ex.Message);
            }
        }

        protected void Initialize(Stream s, bool isBOMFreeUTF8)
        {
            uri = "about:blank";            
            buffer = new Buffer(s, true);
            Init(isBOMFreeUTF8);
        }

        private void Init(bool isBOMFreeUTF8)
        {
            // Console.Write("First bytes: ");
            pos = -1; line = 1; col = 0; charPos = -1;
            oldEols = 0;
            if (isBOMFreeUTF8)
            {
                // we know that it is a UTF-8 stream and that it has or has no BOM
                buffer = new UTF8Buffer(buffer);
            }
            NextCh();
            if (ch == 0xEF)
            { // check optional byte order mark for UTF-8
                NextCh(); int ch1 = ch;
                NextCh(); int ch2 = ch;
                if (ch1 != 0xBB || ch2 != 0xBF)
                {
                    throw new FatalError(String.Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
                }
                buffer = new UTF8Buffer(buffer); col = 0; charPos = -1;
                NextCh();
            }
            else if (ch == 0xFEFF)
            { // optional byte order mark for UTF-8 using UTF8Buffer
                col = 0; charPos = -1; // reset the locgical position to zero and ...
                NextCh(); // ... ignore the BOM, updating col and charPos
            }
            peekToken = tokens = new Token();  // first token is a dummy
        }

        protected void NextCh()
        {
            if (oldEols > 0) { ch = EOL; oldEols--; }
            else
            {
                pos = buffer.Pos;
                // buffer reads unicode chars, if UTF8 has been detected
                ch = buffer.Read(); col++; charPos++;
                // replace isolated '\r' by '\n' in order to make
                // eol handling uniform across Windows, Unix and Mac
                if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
                if (ch == EOL) { line++; col = 0; }
            }
            //if (pos <= 10) Console.Write("{0:X} ", ch);
            if (ch != Buffer.EOF) 
            {
                valCh = (char) ch;
                ch = casing(valCh);
            }
        }

        protected void AddCh()
        {
            if (ch != Buffer.EOF)
            {
                tval.Append(valCh);
                NextCh();
            }
        }

        protected abstract void CheckLiteral();

        protected abstract Token NextToken();

        protected void SetScannerBehindT()
        {
            buffer.Pos = t.pos;
            NextCh();
            line = t.line; col = t.col; charPos = t.charPos;
            for (int i = 0; i < tval.Length; i++)
                NextCh();
        }

        // get the next token (possibly a token already seen during peeking)
        public Token Scan()
        {
            if (tokens.next == null)
            {
                return NextToken();
            }
            else
            {
                peekToken = tokens = tokens.next;
                return tokens;
            }
        }

        // peek for the next token, ignore pragmas
        public Token Peek()
        {
            do
            {
                if (peekToken.next == null)
                {
                    peekToken.next = NextToken();
                }
                peekToken = peekToken.next;
            } while (peekToken.kind > maxT); // skip pragmas

            return peekToken;
        }

        // make sure that peeking starts at the current scan position
        public void ResetPeek() { peekToken = tokens; }

    } // end Scanner


    //-----------------------------------------------------------------------------------
    // Token
    //-----------------------------------------------------------------------------------
    public class Token
    {
        public static readonly Token Zero = new Token() { val = string.Empty, valScanned = string.Empty };
        public int kind;    // token kind
        public int pos;     // token position in bytes in the source text (starting at 0)
        public int charPos;  // token position in characters in the source text (starting at 0)
        public int col;     // token column (starting at 1)
        public int line;    // token line (starting at 1)
        public string val;  // token value, lowercase if case insensitive parser
        public string valScanned; // token value as scanned (always case sensitive)
        public Token next;  // ML 2005-03-11 Tokens are kept in linked list

        public Token Copy() => (Token)(this.MemberwiseClone());
        public Position Position() => new Position(this);
    }

    //-----------------------------------------------------------------------------------
    // Position
    //-----------------------------------------------------------------------------------
    public class Position
    {  // position of source code stretch (e.g. semantic action, resolver expressions)
        public static readonly Position Zero = new Position(Token.Zero);
        private readonly Token t;
        public Position(Token t) => this.t = t.Copy();
        public Range Range(Token t) => new Range(this, t.Position());
        public Range RangeIfNotEmpty(Token t) => t.pos > this.t.pos ? Range(t) : null;
        public int line => t.line;
        public int col => t.col;
        internal int pos => t.pos;

        public override string ToString() => string.Format("({0},{1})", line, col);
    }

    //-----------------------------------------------------------------------------------
    // Range
    //-----------------------------------------------------------------------------------
    public class Range
    {
        public readonly Position start;
        public readonly Position end;

        public Range(Position start, Position end)
        {
            this.start = start;
            this.end = end;
        }

        public override string ToString() => string.Format("{0}..{1}", start, end);
    }

    //-----------------------------------------------------------------------------------
    // Buffer
    //-----------------------------------------------------------------------------------
    public class Buffer
    {
        // This Buffer supports the following cases:
        // 1) seekable stream (file)
        //    a) whole stream in buffer
        //    b) part of stream in buffer
        // 2) non seekable stream (network, console)

        public const int EOF = char.MaxValue + 1;
        const int MIN_BUFFER_LENGTH = 1024; // 1KB
        const int MAX_BUFFER_LENGTH = MIN_BUFFER_LENGTH * 64; // 64KB
        byte[] buf;         // input buffer
        int bufStart;       // position of first byte in buffer relative to input stream
        int bufLen;         // length of buffer
        int fileLen;        // length of input stream (may change if the stream is no file)
        int bufPos;         // current position in buffer
        Stream stream;      // input stream (seekable)
        bool isUserStream;  // was the stream opened by the user?

        public Buffer(Stream s, bool isUserStream)
        {
            stream = s; this.isUserStream = isUserStream;

            if (stream.CanSeek)
            {
                fileLen = (int)stream.Length;
                bufLen = Math.Min(fileLen, MAX_BUFFER_LENGTH);
                bufStart = Int32.MaxValue; // nothing in the buffer so far
            }
            else
            {
                fileLen = bufLen = bufStart = 0;
            }

            buf = new byte[(bufLen > 0) ? bufLen : MIN_BUFFER_LENGTH];
            if (fileLen > 0) Pos = 0; // setup buffer to position 0 (start)
            else bufPos = 0; // index 0 is already after the file, thus Pos = 0 is invalid
            if (bufLen == fileLen && stream.CanSeek) Close();
        }

        protected Buffer(Buffer b)
        { // called in UTF8Buffer constructor
            buf = b.buf;
            bufStart = b.bufStart;
            bufLen = b.bufLen;
            fileLen = b.fileLen;
            bufPos = b.bufPos;
            stream = b.stream;
            // keep destructor from closing the stream
            b.stream = null;
            isUserStream = b.isUserStream;
        }

        ~Buffer() { Close(); }

        protected void Close()
        {
            if (!isUserStream && stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }

        public virtual int Read()
        {
            if (bufPos < bufLen)
            {
                return buf[bufPos++];
            }
            else if (Pos < fileLen)
            {
                Pos = Pos; // shift buffer start to Pos
                return buf[bufPos++];
            }
            else if (stream != null && !stream.CanSeek && ReadNextStreamChunk() > 0)
            {
                return buf[bufPos++];
            }
            else
            {
                return EOF;
            }
        }

        public int Peek()
        {
            int curPos = Pos;
            int ch = Read();
            Pos = curPos;
            return ch;
        }

        // beg .. begin, zero-based, inclusive, in byte
        // end .. end, zero-based, exclusive, in byte
        public string GetString(int beg, int end)
        {
            int len = 0;
            char[] buf = new char[end - beg];
            int oldPos = Pos;
            Pos = beg;
            while (Pos < end) buf[len++] = (char)Read();
            Pos = oldPos;
            return new String(buf, 0, len);
        }

        public int Pos
        {
            get { return bufPos + bufStart; }
            set
            {
                if (value >= fileLen && stream != null && !stream.CanSeek)
                {
                    // Wanted position is after buffer and the stream
                    // is not seek-able e.g. network or console,
                    // thus we have to read the stream manually till
                    // the wanted position is in sight.
                    while (value >= fileLen && ReadNextStreamChunk() > 0) ;
                }

                if (value < 0 || value > fileLen)
                {
                    throw new FatalError("buffer out of bounds access, position: " + value);
                }

                if (value >= bufStart && value < bufStart + bufLen)
                { // already in buffer
                    bufPos = value - bufStart;
                }
                else if (stream != null)
                { // must be swapped in
                    stream.Seek(value, SeekOrigin.Begin);
                    bufLen = stream.Read(buf, 0, buf.Length);
                    bufStart = value; bufPos = 0;
                }
                else
                {
                    // set the position to the end of the file, Pos will return fileLen.
                    bufPos = fileLen - bufStart;
                }
            }
        }

        // Read the next chunk of bytes from the stream, increases the buffer
        // if needed and updates the fields fileLen and bufLen.
        // Returns the number of bytes read.
        private int ReadNextStreamChunk()
        {
            int free = buf.Length - bufLen;
            if (free == 0)
            {
                // in the case of a growing input stream
                // we can neither seek in the stream, nor can we
                // foresee the maximum length, thus we must adapt
                // the buffer size on demand.
                byte[] newBuf = new byte[bufLen * 2];
                Array.Copy(buf, newBuf, bufLen);
                buf = newBuf;
                free = bufLen;
            }
            int read = stream.Read(buf, bufLen, free);
            if (read > 0)
            {
                fileLen = bufLen = (bufLen + read);
                return read;
            }
            // end of stream reached
            return 0;
        }
    }

    //-----------------------------------------------------------------------------------
    // UTF8Buffer
    //-----------------------------------------------------------------------------------
    public class UTF8Buffer : Buffer
    {
        public UTF8Buffer(Buffer b) : base(b) { }

        public override int Read()
        {
            int ch;
            do
            {
                ch = base.Read();
                // until we find a utf8 start (0xxxxxxx or 11xxxxxx)
            } while ((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
            if (ch < 128 || ch == EOF)
            {
                // nothing to do, first 127 chars are the same in ascii and utf8
                // 0xxxxxxx or end of file character
            }
            else if ((ch & 0xF0) == 0xF0)
            {
                // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
                int c1 = ch & 0x07; ch = base.Read();
                int c2 = ch & 0x3F; ch = base.Read();
                int c3 = ch & 0x3F; ch = base.Read();
                int c4 = ch & 0x3F;
                ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
            }
            else if ((ch & 0xE0) == 0xE0)
            {
                // 1110xxxx 10xxxxxx 10xxxxxx
                int c1 = ch & 0x0F; ch = base.Read();
                int c2 = ch & 0x3F; ch = base.Read();
                int c3 = ch & 0x3F;
                ch = (((c1 << 6) | c2) << 6) | c3;
            }
            else if ((ch & 0xC0) == 0xC0)
            {
                // 110xxxxx 10xxxxxx
                int c1 = ch & 0x1F; ch = base.Read();
                int c2 = ch & 0x3F;
                ch = (c1 << 6) | c2;
            }
            return ch;
        }
    }

    // --------------------------------
    // FatalError used in Scanner and Parser
    // --------------------------------
    public class FatalError : Exception
    {
        public FatalError(string m) : base(m) { }
    }

}