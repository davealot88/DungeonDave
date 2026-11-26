namespace DungeonDave.Core
{
    /// <summary>Source of randomness for game rules, so tests can be deterministic.</summary>
    public interface IRandom
    {
        /// <summary>Returns a value in [0, 1).</summary>
        float NextFloat();
    }

    /// <summary>Small deterministic xorshift generator. Same seed, same sequence, on every platform.</summary>
    public sealed class SeededRandom : IRandom
    {
        private uint state;

        public SeededRandom(uint seed)
        {
            state = seed == 0 ? 0x9E3779B9u : seed;
        }

        public float NextFloat()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (state >> 8) * (1f / 16777216f);
        }
    }

    public static class RandomExtensions
    {
        /// <summary>Whole number between min and max, both included.</summary>
        public static int Range(this IRandom rng, int minInclusive, int maxInclusive)
        {
            if (maxInclusive <= minInclusive) return minInclusive;
            return minInclusive + (int)(rng.NextFloat() * (maxInclusive - minInclusive + 1));
        }

        public static bool Chance(this IRandom rng, float probability)
        {
            return rng.NextFloat() < probability;
        }
    }
}
