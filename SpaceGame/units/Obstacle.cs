using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.units
{
    class Obstacle : PhysicalBody
    {
        public Obstacle(PhysicalData pd)
            :base(pd, graphics.Sprite.SpriteType.Obstacle)
        {
        }
    }
}
