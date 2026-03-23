namespace DataFilter.Wpf.Demo.Models
{
    public struct XorShift128
    {
        private uint x, y, z, w;

        public XorShift128(uint seed)
        {
            x = seed;
            y = 362436069;
            z = 521288629;
            w = 88675123;
        }

        public uint NextUInt()
        {
            uint t = x ^ (x << 11);
            x = y;
            y = z;
            z = w;
            w ^= (w >> 19) ^ t ^ (t >> 8);
            return w;
        }

        public int Next(int max)
            => (int)(NextUInt() % max);

        public int Next(int min, int max)
            => min + Next(max - min);

        public double NextDouble()
            => NextUInt() / (double)uint.MaxValue;
    }
}
