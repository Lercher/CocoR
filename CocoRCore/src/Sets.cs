using System.Collections;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // Sets 
    //=====================================================================

    public class Sets
    {

        public static int Elements(BitArray s)
        {
            int max = s.Length;
            int n = 0;
            for (int i = 0; i < max; i++)
                if (s[i]) n++;
            return n;
        }

        public static bool Equals(BitArray a, BitArray b)
        {
            int max = a.Length;
            for (int i = 0; i < max; i++)
                if (a[i] != b[i]) return false;
            return true;
        }

        public static bool Intersect(BitArray a, BitArray b)
        { // a * b != {}
            int max = a.Length;
            for (int i = 0; i < max; i++)
                if (a[i] && b[i]) return true;
            return false;
        }

        public static void Subtract(BitArray a, BitArray b)
        { // a = a - b
            var c = new BitArray(b);
            a.And(c.Not());
        }

    }

} // end namespace
