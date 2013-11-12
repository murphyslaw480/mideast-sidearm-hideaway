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
        float _minSpawnTime;
        float _maxSpawnTime;

        TimeSpan _respawnTimer;
        bool _isSpawned;

        public Obstacle(ObstacleData data)
            :base(data, graphics.Sprite.SpriteType.Obstacle)
        {
            _minSpawnTime = data.MinSpawnTime;
            _maxSpawnTime = data.MaxSpawnTime;
            _respawnTimer = XnaHelper.RandomTime(_minSpawnTime, _maxSpawnTime);
            _isSpawned = false;
        }

        public override bool CanRespawn
        {
            get
            {
                return base.CanRespawn && _respawnTimer <= TimeSpan.Zero;
            }
        }

        public void UpdateRespawnTimer(TimeSpan time)
        {
            if (_lifeState == LifeState.Destroyed || _lifeState == LifeState.Dormant)
            {
                if (_isSpawned)
                {   //just destroyed
                    _isSpawned = false;
                    _respawnTimer = XnaHelper.RandomTime(_minSpawnTime, _maxSpawnTime);
                }
                _respawnTimer -= time;
            }
        }
    }
}
