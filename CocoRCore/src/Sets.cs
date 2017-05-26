using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // Sets 
    //=====================================================================

    public static class Sets
    {
        public static IEnumerable<bool> Seq(this BitArray s) 
            => s.Cast<bool>();

        public static bool Any(this BitArray s) 
            => s.Seq().Any(b => b);

        public static int ElementCount(this BitArray s) 
            => s.Seq().Count(b => b);

        public static bool Equals(this BitArray a, BitArray b) 
            => a.Seq().SequenceEqual(b.Seq());

        public static bool Intersects(this BitArray a, BitArray b) 
            => Enumerable.Zip(a.Seq(), b.Seq(), (x, y) => x == y).Any(z => z);

        public static void Subtract(this BitArray a, BitArray b)
            => a.And(new BitArray(b).Not());

   }
} // end namespace
