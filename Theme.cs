using System.Drawing;

namespace WorstPC
{
    // WORST brand palette (same colors as the W-BOARD app).
    internal static class Theme
    {
        public static readonly Color BG      = FromHex("070707");
        public static readonly Color CARD    = FromHex("0E0E0E");
        public static readonly Color RAISE   = FromHex("161616");
        public static readonly Color LINE    = FromHex("232323");
        public static readonly Color LINE_HI = FromHex("3B3B3B");
        public static readonly Color TEXT    = FromHex("F0F0F0");
        public static readonly Color SUB     = FromHex("8A8A8A");
        public static readonly Color ASH     = FromHex("E6E6E6");
        public static readonly Color INK     = FromHex("0A0A0A");
        public static readonly Color RED     = FromHex("FF3B3B");
        public static readonly Color REDSOFT = FromHex("FF7B7B");
        public static readonly Color GREEN   = FromHex("5FD68A");
        public static readonly Color EMBER   = FromHex("FFAF78");

        public static Color FromHex(string hex)
        {
            return Color.FromArgb(
                255,
                System.Convert.ToInt32(hex.Substring(0, 2), 16),
                System.Convert.ToInt32(hex.Substring(2, 2), 16),
                System.Convert.ToInt32(hex.Substring(4, 2), 16));
        }
    }
}
