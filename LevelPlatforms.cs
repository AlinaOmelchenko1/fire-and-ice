using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace fire_and_ice
{
    public static class LevelPlatforms
    {
        // Define platforms manually for Level 1 with interaction types
        public static List<InteractableObject> GetLevel1Platforms()
        {
            List<InteractableObject> platforms = new List<InteractableObject>();

            // BOTTOM FLOOR - Full width brown platform (Solid ground)
            platforms.Add(new InteractableObject(
                new Rectangle(0, 400, 800, 100),
                SurfaceType.Solid
            ));

            // LEFT ELEVATED PLATFORM (brown/dirt - upper left) - Normal solid
            platforms.Add(new InteractableObject(
                new Rectangle(0, 115, 220, 70),
                SurfaceType.Solid
            ));

            // RIGHT ELEVATED PLATFORM (brown/dirt - upper right) - ICY platform
            platforms.Add(new InteractableObject(
                new Rectangle(515, 120, 285, 70),
                SurfaceType.Ice
            ));

            // LARGE CENTER PLATFORM (brown - middle area) - Normal solid
            platforms.Add(new InteractableObject(
                new Rectangle(170, 260, 400, 80),
                SurfaceType.Solid
            ));

            // LEFT WOODEN CRATE (bottom left) - Temporarily Solid (was Bouncy)
            platforms.Add(new InteractableObject(
                new Rectangle(65, 335, 90, 65),
                SurfaceType.Solid
            ));

            // RIGHT WOODEN CRATE (bottom right) - Sticky
            platforms.Add(new InteractableObject(
                new Rectangle(590, 335, 95, 65),
                SurfaceType.Sticky
            ));

            // CENTER LEFT WOODEN CRATE (on the large center platform) - Normal
            platforms.Add(new InteractableObject(
                new Rectangle(270, 195, 95, 75),
                SurfaceType.Solid
            ));

            // CENTER RIGHT WOODEN CRATE (on the large center platform) - Normal
            platforms.Add(new InteractableObject(
                new Rectangle(380, 195, 95, 75),
                SurfaceType.Solid
            ));

            // Example hazards - add fire/lava areas
            // FIRE HAZARD - small fire on left side
            platforms.Add(new InteractableObject(
                new Rectangle(150, 380, 40, 20),
                SurfaceType.Fire,
                damageAmount: 5f  // 5 damage per hit
            ));

            // SPIKE HAZARD - on center platform
            platforms.Add(new InteractableObject(
                new Rectangle(470, 240, 30, 20),
                SurfaceType.Spike,
                damageAmount: 20f  // 20 damage per hit
            ));

            return platforms;
        }

        // You can add methods for other levels here
        public static List<InteractableObject> GetLevel2Platforms()
        {
            // Define level 2 platforms when you create that level
            return new List<InteractableObject>();
        }
    }
}