using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SpaceGame.utility;

namespace SpaceGame.units
{
    class ObstacleData : PhysicalData
    {
        //spawn frequencies
        public float MinSpawnTime;
        public float MaxSpawnTime;
    }

    class Obstacle : PhysicalBody
    {
        public float MinSpawnTime;
        public float MaxSpawnTime;

        TimeSpan _respawnTimer;
        bool _isSpawned;

        public Obstacle(ObstacleData data)
            :base(data, graphics.Sprite.SpriteType.Obstacle)
        {
            MinSpawnTime = data.MinSpawnTime;
            MaxSpawnTime = data.MaxSpawnTime;
            _isSpawned = false;
        }

        public override bool CanRespawn
        {
            get
            {
                return base.CanRespawn && _respawnTimer <= TimeSpan.Zero;
            }
        }

        public override void Update(GameTime gameTime, Rectangle levelBounds)
        {
            base.Update(gameTime, levelBounds);
            if (_lifeState == LifeState.Destroyed)
            {
                if (_isSpawned)
                {   //just destroyed
                    _isSpawned = false;
                    _respawnTimer = XnaHelper.RandomTime(MinSpawnTime, MaxSpawnTime);
                }
                _respawnTimer -= gameTime.ElapsedGameTime;
            }
        }
    }
}
