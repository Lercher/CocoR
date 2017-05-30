using System;
using System.Text;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //-----------------------------------------------------------------------------
    //  CharSet
    //-----------------------------------------------------------------------------

    public class CharSet
    {

        public class CharactersRange
        {
            public int from;
            public int to;
            public CharactersRange next;

            public CharactersRange(int from, int to, CharactersRange next) : this(from, to) => this.next = next;


            public CharactersRange(int from, int to)
            {
                this.from = from; this.to = to;
            }

            public override string ToString()
            {
                if (from == to)
                    return from.ToString("X");
                if (from <= 256 && to <= 256)
                    return string.Format("{0:X2}-{1:X2}", from, to);
                return string.Format("{0:X4}-{1:X4}", from, to);
            }
        }

        public CharactersRange head;

        public override string ToString()
        {
            if (head == null) return "[]";
            var sb = new StringBuilder();
            sb.Append('[');
            for (var cur = head; cur != null; cur = cur.next)
            {
                if (cur != head) sb.Append('|');
                sb.Append(cur.ToString());
            }
            sb.Append(']');
            return sb.ToString();
        }

        public bool this[int i]
        {
            get
            {
                for (var p = head; p != null; p = p.next)
                    if (i < p.from)
                        return false;
                    else if (i <= p.to)
                        return true; // p.from <= i <= p.to
                return false;
            }
        }

        public void Set(int i)
        {
            CharactersRange prev = null;
            var cur = head;
            while (cur != null && i >= cur.from - 1)
            {
                if (i <= cur.to + 1)
                { // (cur.from-1) <= i <= (cur.to+1)
                    if (i == cur.from - 1)
                        cur.from--;
                    else if (i == cur.to + 1)
                    {
                        cur.to++;
                        var next = cur.next;
                        if (next != null && cur.to == next.from - 1)
                        {
                            cur.to = next.to;
                            cur.next = next.next;
                        };
                    }
                    return;
                }
                prev = cur; cur = cur.next;
            }
            var n = new CharactersRange(i, i, next: cur);

            if (prev == null)
                head = n;
            else
                prev.next = n;
        }

        public CharSet Clone()
        {
            var s = new CharSet();
            CharactersRange prev = null;
            for (var cur = head; cur != null; cur = cur.next)
            {
                var r = new CharactersRange(cur.from, cur.to);
                if (prev == null)
                    s.head = r;
                else
                    prev.next = r;
                prev = r;
            }
            return s;
        }

        public bool Equals(CharSet s)
        {
            var p = head;
            var q = s.head;
            while (p != null && q != null)
            {
                if (p.from != q.from || p.to != q.to)
                    return false;
                p = p.next; q = q.next;
            }
            return p == q;
        }

        public int Elements()
        {
            var n = 0;
            for (var p = head; p != null; p = p.next)
                n += p.to - p.from + 1;
            return n;
        }

        public int First() => head?.from ?? -1;

        public void Or(CharSet s)
        {
            for (var p = s.head; p != null; p = p.next)
                for (var i = p.from; i <= p.to; i++)
                    Set(i);
        }

        public void And(CharSet s)
        {
            var x = new CharSet();
            for (var p = head; p != null; p = p.next)
                for (var i = p.from; i <= p.to; i++)
                    if (s[i])
                        x.Set(i);
            head = x.head;
        }

        public void Subtract(CharSet s)
        {
            var x = new CharSet();
            for (var p = head; p != null; p = p.next)
                for (var i = p.from; i <= p.to; i++)
                    if (!s[i])
                        x.Set(i);
            head = x.head;
        }

        public bool Includes(CharSet s)
        {
            for (var p = s.head; p != null; p = p.next)
                for (var i = p.from; i <= p.to; i++)
                    if (!this[i])
                        return false;
            return true;
        }

        public bool Intersects(CharSet s)
        {
            for (var p = s.head; p != null; p = p.next)
                for (var i = p.from; i <= p.to; i++)
                    if (this[i])
                        return true;
            return false;
        }

        public void Fill() => head = new CharactersRange(char.MinValue, char.MaxValue);
    }

} // end namespace
