using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.units
{
    class ObstacleData : PhysicalData
    {
    }

    class Obstacle : PhysicalBody
    {
        public Obstacle(string name)
            :this(DataManager.GetData<ObstacleData>(name))
        { }

        protected Obstacle(PhysicalData pd)
            :base(pd, graphics.Sprite.SpriteType.Obstacle)
        {
        }
    }
}
