namespace FillPatternEditor.Utils
{
    // Static class to store constant values related to pattern sizes
    public static class Constants
    {
        // Minimum size for drafting patterns (in units, possibly millimeters)
        public static double DraftingPatternMinSize => 0.0508;

        // Minimum size for model patterns
        public static double ModelPatternMinSize => 12.7;

        // Maximum size for drafting patterns
        public static double DraftingPatternMaxSize => 3048.0;

        // Maximum size for model patterns
        public static double ModelPatternMaxSize => 30480.0;

        // Default precision for tolerance-based comparisons
        public static double DefaultPrecision => 1E-06;
    }
}
