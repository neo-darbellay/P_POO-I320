namespace ShootMeUp.Helpers
{
    internal static class TextHelpers
    {
        private static readonly Dictionary<int, Font> CachedFonts = new()
        {
            [48] = new Font("Consolas", 48, FontStyle.Bold),
            [32] = new Font("Consolas", 32, FontStyle.Bold),
            [24] = new Font("Consolas", 24, FontStyle.Bold),
            [18] = new Font("Consolas", 18, FontStyle.Bold),
            [16] = new Font("Consolas", 16, FontStyle.Bold),
            [12] = new Font("Consolas", 12, FontStyle.Bold),
            [8] = new Font("Consolas", 8, FontStyle.Bold)
        };

        public static Font drawFont = new("Arial", 8, FontStyle.Bold);
        public static SolidBrush writingBrush = new(Color.White);

        public static Font GetCachedFont(int size)
        {
            if (!CachedFonts.TryGetValue(size, out var font))
            {
                font = new Font("Consolas", size, FontStyle.Bold);
                CachedFonts[size] = font;
            }

            return font;
        }
    }
}
