using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace fire_and_ice
{
    public enum SurfaceType //all types of future interactons dont need now but for th3e future record 
    {
        Empty = 0,        // Walkable air/transparent
        Solid = 1,        // Solid ground movement
        Platform = 2,     // Jump-through platform
        Hazard = 3,       // Damages player
        Water = 4,        // Cold hazard
        Lava = 5         // Hot hazard
    }

}