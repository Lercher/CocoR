// This file has to be compiled and linked to a Coco/R generated scanner.
// as a variant, you can reference the CocoRCore.dll,
// that includes this classes in a compiled form.

#pragma warning disable IDE1006 // Naming Styles

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CocoRCore
{
    //-----------------------------------------------------------------------------------
    // Scanner
    //-----------------------------------------------------------------------------------
    public abstract class ScannerBase : IDisposable
    {
        public const int EOF = -1;
        public const int EOL = '\n';
        public const int _EOF = 0; // TOKEN EOF is the same for every parser

        // --- buffer abstraction
        public IBufferedReader buffer { get; private set; } // scanner buffer
        protected int pos => buffer.Position.pos;          // char position of current character starting with 0
        protected int col => buffer.Position.col;          // column number of current character 1-based
        protected int line => buffer.Position.line;        // line number of current character 1-based
        public string uri => buffer.Uri;

        // --- token builder abstraction
        protected StringBuilder tval = new StringBuilder(); // text for current token
        protected Token.Builder t;          // current token builder
        protected int ch;           // current input character (probably lowercased)
        protected char valCh;       // current input character (original version)

        protected Token tokens = Token.Zero;     // list of tokens already peeked (first token is a dummy)
        protected Token peekToken = Token.Zero;         // current peek token

        protected Func<char, char> casing = c => c;
        public Func<string, string> casingString = c => c;

        protected static string ToLower(string s) => s.ToLowerInvariant();

        protected abstract int maxT { get; }

        private void PutCh(int c, char v)
        {
            ch = c;
            valCh = v;
        }

        private readonly Stack<IDisposable> disposables = new Stack<IDisposable>();
        public T Track<T>(T disposable) where T : IDisposable
        {
            disposables.Push(disposable);
            return disposable;
        }

        public void Dispose()
        {
            foreach (var d in disposables)
                d.Dispose();
            disposables.Clear();
        }

        public ScannerBase Initialize(string fileName) => Initialize(new FileInfo(fileName));

        public ScannerBase Initialize(FileInfo file)
        {
            try
            {
                var stream = Track(file.OpenRead());
                var tr = Track(new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096));
                return Initialize(tr, file.FullName);
            }
            catch (IOException ex)
            {
                throw new FatalError(ex.Message, ex);
            }
        }

        public ScannerBase Initialize(string source, string uri)
        {
            var sr = Track(new StringReader(source));
            return Initialize(sr, uri);
        }

        public ScannerBase Initialize(StringBuilder sb, string uri)
        {
            var sbr = Track(new StringBuilderReader(sb));
            return Initialize(sbr, uri);
        }

        public ScannerBase Initialize(TextReader rd, string uri)
        {
            buffer = Track(new Reader(rd, uri, casing, PutCh));
            return this;
        }

        protected void NextCh() => buffer.NextCh();

        protected void AddCh()
        {
            if (ch != EOF)
            {
                tval.Append(valCh);
                NextCh();
            }
        }

        protected abstract void CheckLiteral();

        protected abstract Token NextToken();

        protected void SetScannerBackBehindT()
        {
            buffer.ResetPositionTo(t.positionM1);
            for (var i = 0; i < tval.Length; i++)
                NextCh();
        }

        // get the next token (possibly a token already seen during peeking)
        public Token Scan()
        {
            if (buffer == null)
                throw new FatalError($"The Scanner {GetType().FullName} has to be Initialize()-ed before use");
            if (tokens.next == null)
                return NextToken();
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
                    peekToken.next = NextToken();
                peekToken = peekToken.next;
            } while (peekToken.kind > maxT); // skip pragmas

            return peekToken;
        }

        // make sure that peeking starts at the current scan position
        public void ResetPeek() => peekToken = tokens;
    } // end Scanner


    //-----------------------------------------------------------------------------------
    // Token
    //-----------------------------------------------------------------------------------
    public class Token
    {
        private Token(Builder b)
        {
            kind = b.kind;
            position = b.position;
            endPosition = b.endPosition;
            val = b.val;
            valScanned = b.valScanned;
        }


        private Token()
        {
            position = Position.Zero;
            endPosition = Position.Zero;
            val = string.Empty;
            valScanned = string.Empty;
        }

        public static readonly Token Zero = new Token();
        public readonly int kind;    // token kind
        public readonly Position position; // start position
        public readonly Position endPosition;
        public int pos => position.pos;
        public int line => position.line;
        public int col => position.col;

        public readonly string val;  // token value, lowercase if case insensitive parser
        public readonly string valScanned; // token value as scanned (always case sensitive)

        public Token next;  // ML 2005-03-11 Peeked Tokens are kept in linked list

        public Token.Builder Copy() => new Token.Builder(this); // to modify attributes
        public Range Range() => new Range(position, endPosition);

        public string ToString(ParserBase parser)
        {
            var nm = parser.NameOfTokenKind(kind);
            if (!nm.StartsWith("["))
                return valScanned;
            if (valScanned.Length > 30)
                return $"{nm} '{valScanned.Substring(0, 28)}...'";
            return $"{nm} '{valScanned}'";
        }

        public override string ToString() => $"{kind}:{valScanned}{position}";

        public class Builder
        {
            public Builder(IBufferedReader buffer)
            {
                positionM1 = buffer.PositionM1;
                position = buffer.Position;
            }

            public Builder()
            {
                positionM1 = Position.MinusOne;
                position = Position.Zero;
            }

            public Builder(Token t)
            {
                kind = t.kind;
                position = t.position;
                val = t.val;
                valScanned = t.valScanned;
            }

            public int kind;
            public Position positionM1;
            public Position position;
            public Position endPosition;
            public string val { get; private set; }
            public string valScanned { get; private set; }

            public void setValue(string scanned, Func<string, string> casing)
            {
                valScanned = scanned;
                val = casing(scanned);
            }

            public Token Freeze() => new Token(this);

            public Token Freeze(Position end, Position endM1)
            {
                endPosition = end.col <= 0 ? endM1.OneCharForward() : end;
                return Freeze();
            }
        }
    }

    //-----------------------------------------------------------------------------------
    // Position
    //-----------------------------------------------------------------------------------
    public struct Position
    {  // position of source code stretch (e.g. semantic action, resolver expressions)
        public static readonly Position Zero; // default struct constructor applies here
        public static readonly Position MinusOne = new Position(-1, -1, -1);
        public static readonly Position StartOfFile = new Position(0, 0, 1);

        public Position(int pos0, int col1, int line1)
        {
            pos = pos0;
            col = col1;
            line = line1;
        }

        public readonly int pos;     // token position in characters in the source text (starting at 0, counting 1 for \r\n)
        public readonly int col;     // token column (starting at 1)
        public readonly int line;    // token line (starting at 1)

        public Range Range(Token t) => new Range(this, t.position);
        public Range RangeIfNotEmpty(Token t) => t.pos > pos ? Range(t) : null;

        public override string ToString() => string.Format("({0},{1})", line, col);

        public Position OneCharForward() => new Position(pos + 1, col + 1, line);
        public Position OneLinebreakForward() => new Position(pos + 1, 0, line + 1);
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
    // IBufferedReader
    //-----------------------------------------------------------------------------------
    public interface IBufferedReader
    {
        /* What we want:
         * read the next char
         * handle EOF
         * handle \r\n, \r, \n to produce a single EOL
         * be a TextReader (on Stream, on string and on StringBuilder)
         * have a position (what about \n?) with pos, line and col
         * for comments: store a single bookmark position and reset reading to this position or remove the bookmark -> Queue<char>
         * fetch a not too old string defined by a Range -> SlidingBuffer
         */
        string Uri { get; }
        void NextCh();
        Position PositionM1 { get; } // Position before last Read()
        Position Position { get; } // Position after last Read()
        string GetBufferedString(Range r);
        void ResetPositionTo(Position bookmark);
    }

    //-----------------------------------------------------------------------------------
    // Reader
    //-----------------------------------------------------------------------------------
    public class Reader : IBufferedReader, IDisposable
    {
        private readonly TextReader _tr;
        private Position _nextPosition;
        private Position _pos;
        private readonly SlidingBuffer _slider = new SlidingBuffer(128_000); // 128k chars for GetBufferedString()
        private Func<int> _readnext;
        private Func<char, char> _casing;
        private Action<int, char> _putCh;
        public string Uri { get; private set; }

        public Reader(TextReader tr, string uri, Func<char, char> casing, Action<int, char> putCh)
        {
            _tr = tr;
            Uri = uri;
            _casing = casing;
            _putCh = putCh;
            _nextPosition = Position.StartOfFile;
            _pos = Position.MinusOne;
            _readnext = ReadNextRaw;
            NextCh();
        }

        public void Dispose()
        {
            _tr.Dispose();
            _slider.Dispose();
        }

        public Position PositionM1 => _pos;
        public Position Position => _nextPosition;

        public void NextCh()
        {
            var ch = Read();
            if (ch == ScannerBase.EOF)
                _putCh(ch, '\0');
            else
            {
                var valCh = (char)ch;
                ch = _casing(valCh);
                _putCh(ch, valCh);
            }
        }

        public void ResetPositionTo(Position bookmark)
        {
            _nextPosition = bookmark;
            _readnext = () =>
            {
                if (_pos.pos >= _slider.pos - 1)
                    _readnext = ReadNextRaw;
                return _slider.CharAt(_pos.pos);
            };
            NextCh();
        }

        public string GetBufferedString(Range r) => _slider.String(r.start.pos - 1, r.end.pos - 1);

        public int Read()
        {
            _pos = _nextPosition;
            var ch = _readnext();
            if (ch == ScannerBase.EOL)
                _nextPosition = _nextPosition.OneLinebreakForward();
            else
                _nextPosition = _nextPosition.OneCharForward();
            return ch;
        }

        private int ReadNextRaw()
        {
            var ch = ReadNextHandleEOL();
            if (ch != ScannerBase.EOF)
                _slider.Put((char)ch);
            return ch;
        }

        private int ReadNextHandleEOL()
        {
            var ch = _tr.Read();
            if (ch < 0)
                return ScannerBase.EOF;
            if (ch == '\r')
            {
                // now treat \r\n, \r, \n as EOL:
                var peek = _tr.Peek();
                if (peek == '\n')
                    _tr.Read(); // found \r\n, so consume \n
                return ScannerBase.EOL; // found \r + \n or something else, so return EOL
            }
            return ch; // in any other case return the char read.
        }

    }


    public class CircularBuffer<T> : IEnumerable<T>, IDisposable
    {
        private int capacity;
        private int size;
        private int head;
        private int tail;
        private T[] buffer;

        public CircularBuffer(int capacity)
            : this(capacity, false)
        {
        }

        public CircularBuffer(int capacity, bool allowOverflow)
        {
            this.capacity = capacity;
            size = 0;
            head = 0;
            tail = 0;
            buffer = new T[capacity];
            AllowOverflow = allowOverflow;
        }

        public readonly bool AllowOverflow;

        public int Capacity => capacity;

        public int Count => size;

        public bool Contains(T item)
        {
            var bufferIndex = head;
            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < size; i++, bufferIndex++)
            {
                if (bufferIndex == capacity)
                    bufferIndex = 0;

                if (item == null && buffer[bufferIndex] == null)
                    return true;
                else if ((buffer[bufferIndex] != null) &&
                    comparer.Equals(buffer[bufferIndex], item))
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            size = 0;
            head = 0;
            tail = 0;
        }

        public int Put(T[] src) => Put(src, 0, src.Length);

        public int Put(T[] src, int offset, int count)
        {
            if (!AllowOverflow && count > capacity - size)
                throw new InvalidOperationException("MessageBufferOverflow");

            var srcIndex = offset;
            for (var i = 0; i < count; i++, tail++, srcIndex++)
            {
                if (tail == capacity)
                    tail = 0;
                buffer[tail] = src[srcIndex];
            }
            size = Math.Min(size + count, capacity);
            return count;
        }


        public void Skip(int count)
        {
            head += count;
            if (head >= capacity)
                head -= capacity;
        }

        public T[] Get(int count)
        {
            var dst = new T[count];
            Get(dst);
            return dst;
        }

        public int Get(T[] dst) => Get(dst, 0, dst.Length);

        public int Get(T[] dst, int offset, int count)
        {
            var realCount = Math.Min(count, size);
            var dstIndex = offset;
            for (var i = 0; i < realCount; i++, head++, dstIndex++)
            {
                if (head == capacity)
                    head = 0;
                dst[dstIndex] = buffer[head];
            }
            size -= realCount;
            return realCount;
        }


        public void CopyTo(T[] array) => CopyTo(array, 0);

        public void CopyTo(T[] array, int arrayIndex) => CopyTo(0, array, arrayIndex, size);

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            if (count > size)
                throw new ArgumentOutOfRangeException("count", "MessageReadCountTooLarge");

            var bufferIndex = head;
            for (var i = 0; i < count; i++, bufferIndex++, arrayIndex++)
            {
                if (bufferIndex == capacity)
                    bufferIndex = 0;
                array[arrayIndex] = buffer[bufferIndex];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            var bufferIndex = head;
            for (var i = 0; i < size; i++, bufferIndex++)
            {
                if (bufferIndex == capacity)
                    bufferIndex = 0;

                yield return buffer[bufferIndex];
            }
        }

        public T[] GetBuffer() => buffer;

        public T[] ToArray()
        {
            var dst = new T[size];
            CopyTo(dst);
            return dst;
        }


        public void Put(T item)
        {
            if (!AllowOverflow && size == capacity)
                throw new InvalidOperationException("MessageBufferOverflow");

            buffer[tail] = item;
            if (++tail == capacity)
                tail = 0;
            size++;
        }

        public T Get()
        {
            if (size == 0)
                throw new InvalidOperationException("MessageBufferEmpty");

            var item = buffer[head];
            if (++head == capacity)
                head = 0;
            size--;
            return item;
        }

        public T ItemAt(int index)
        {
            if (index >= 0)
            { // look up index items on the Get/head end
                var i = index + head;
                if (i > capacity)
                    i -= capacity;
                return buffer[i];
            }
            else
            { // look up index items on the Put/tail end
                var i = index + tail;
                if (i < 0)
                    i += capacity;
                return buffer[i];
            }
        }

        public T[] Slice(int index, int count)
        {
            var ar = new T[count];
            for (var i = 0; i < count; i++)
                ar[i] = ItemAt(index + i);
            return ar;
        }

        public void Dispose() => buffer = null;
    }

    //-----------------------------------------------------------------------------------
    // SlidingBuffer
    //-----------------------------------------------------------------------------------
    public class SlidingBuffer : IDisposable
    {
        private readonly CircularBuffer<char> _q;
        public int pos { get; private set; }
        public SlidingBuffer(int capacity)
        {
            pos = 0;
            _q = new CircularBuffer<char>(capacity, allowOverflow: true);
        }

        public void Put(char c)
        {
            pos++;
            _q.Put(c);
        }

        public char CharAt(int p) => _q.ItemAt(p - pos);

        public string String(int start, int end)
        {
            var startInQ = start - pos; // is negative
            if (-startInQ >= _q.Capacity) throw new FatalError("This text is no more buffered");
            if (pos < end) throw new FatalError("This text is not yet buffered");
            if (start > end) throw new FatalError("Start can't be after end");
            var ar = _q.Slice(startInQ, end - start);
            return new string(ar);
        }

        public void Dispose() => _q.Dispose();
    }


    //-----------------------------------------------------------------------------------
    // StringBuilderReader
    //-----------------------------------------------------------------------------------
    public class StringBuilderReader : TextReader
    {
        private readonly StringBuilder _sb;
        private int _pos = 0;

        public StringBuilderReader(StringBuilder stringBuilder) => _sb = stringBuilder;

        public override int Peek()
        {
            if (_pos < _sb.Length)
                return _sb[_pos];
            return -1;
        }
        public override int Read()
        {
            try
            {
                return Peek();
            }
            finally
            {
                _pos++;
            }
        }

        public override int Read(char[] buffer, int index, int count)
        {
            var result = Math.Min(count, _sb.Length - _pos);
            if (result > 0)
            {
                _sb.CopyTo(_pos, buffer, index, result);
                _pos += result;
            }
            return result;
        }
    }


    // --------------------------------
    // FatalError
    // --------------------------------
    public class FatalError : Exception
    {
        public FatalError(string m) : base(m)
        {
        }

        public FatalError(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}