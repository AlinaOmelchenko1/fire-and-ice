using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace fire_and_ice
{
    public static class LevelPlatforms
    {
        // Define platforms manually for Level 1
        public static List<Rectangle> GetLevel1Platforms()
        {
            List<Rectangle> platforms = new List<Rectangle>();

            // BOTTOM FLOOR - Full width brown platform
            platforms.Add(new Rectangle(0, 400, 800, 100));

            // LEFT ELEVATED PLATFORM (brown/dirt - upper left)
            platforms.Add(new Rectangle(0, 115, 220, 70));

            // RIGHT ELEVATED PLATFORM (brown/dirt - upper right)
            platforms.Add(new Rectangle(515, 120, 285, 70));

            // LARGE CENTER PLATFORM (brown - middle area)
            platforms.Add(new Rectangle(170, 260, 400, 80));

            // LEFT WOODEN CRATE (bottom left)
            platforms.Add(new Rectangle(65, 335, 90, 65));

            // RIGHT WOODEN CRATE (bottom right)
            platforms.Add(new Rectangle(590, 335, 95, 65));

            // CENTER TWO WOODEN CRATES (on the large center platform)
            platforms.Add(new Rectangle(270, 195, 95, 75));
            platforms.Add(new Rectangle(380, 195, 95, 75));

            return platforms;
        }

        // You can add methods for other levels here
        public static List<Rectangle> GetLevel2Platforms()
        {
            // Define level 2 platforms when you create that level
            return new List<Rectangle>();
        }
    }
}