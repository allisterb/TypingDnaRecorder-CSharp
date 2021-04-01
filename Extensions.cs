namespace TypingDNA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public static class Extensions
    {
        public static void Put<K, V>(this Dictionary<K, V> d, K k, V v)
        {
            if (d.ContainsKey(k))
            {
                d[k] = v;
            }
            else 
            {
                d.Add(k, v);
            }
        }

        public static bool IsNaN(this double d) => Double.IsNaN(d);
        
        public static bool IsInfinite(this double d) => Double.IsInfinity(d);
    }
}