using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.equipment;
using SpaceGame.graphics;
using SpaceGame.utility;

namespace SpaceGame.units
{
    class WaveData
    {
        public string[] Enemies;
        public float EnemySpawnRate;     //difficulty / second
    }

    class BurstWaveData : WaveData
    {
        public int Difficulty;
        public float RestDelay;
    }

    class TrickleWaveData : WaveData
    {
        public float StartTime;     //when to trigger (seconds into level)
        public float Duration;      //#seconds to last
    }
    
    /// <summary>
    /// A wave of enemies
    /// </summary>
    abstract class Wave
    {
        #region constant
        //how far out of bounds trickle waves can spawn enemies
        const int c_outOfBoundsSpawnBuffer = 30;
        //how long between starting to show effect and spawning enemies
        const float c_activationDelaySeconds = 3;
        //minimum distance allowable between spawn location and black hole
        const float c_minBlackholeSpawnDistance = 200;
        #endregion

        #region fields
        protected int _numEnemies;      //total number of enemies in wave
        protected int _enemySpawnIndex;    //next enemy to spawn
        Enemy[] _enemies;
        float _enemySpawnValue;         //spawn when this > next enemy's difficulty
        float _enemySpawnRate;          //rate to accumulate spawnValue
        protected Vector2 _spawnLocation;         //where in level to spawn enemies
        Rectangle _levelBounds;
        protected bool _allDestroyed;
        #endregion

        #region properties
        //set when every enemy in mob has been spawned
        //does not apply to trickle waves
        public abstract bool Active { get; }
        /// <summary>
        /// Set to false when level ends to prevent spawning
        /// </summary>
        public bool SpawnEnable { get; set; }

        public Enemy[] Enemies { get { return _enemies; } }
        #endregion

        #region constructor
        public Wave(WaveData data, bool trickleWave, Rectangle levelBounds)
        {
            _enemies = new Enemy[data.Enemies.Length];
            for (int j = 0; j < _enemies.Length; j++)
            {
                _enemies[j] = new Enemy(data.Enemies[j], levelBounds);
            }
            _numEnemies = _enemies.Length;
            _enemySpawnRate = data.EnemySpawnRate;
            _enemySpawnIndex = 0;
            _enemySpawnValue = 0;
            _spawnLocation = Vector2.Zero;
            SpawnEnable = true;
            _levelBounds = levelBounds;
        }
        #endregion

        #region methods
        protected bool trySpawn(TimeSpan time, Vector2 blackHolePosition)
        {
            Debug.Assert(_enemySpawnIndex <= _enemies.Length - 1, "_enemySpawnIndex out of range");
            if (!SpawnEnable || !_enemies[_enemySpawnIndex].CanRespawn)
            {   //cannot spawn or enemy already spawned
                return false;
            }
            _enemySpawnValue += _enemySpawnRate * (float)time.TotalSeconds;
            Enemy enemy = _enemies[_enemySpawnIndex];
            if (_enemySpawnValue >= enemy.Difficulty)
            {
                _enemies[_enemySpawnIndex++].Respawn(_spawnLocation);
                _enemySpawnValue = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update wave, updating behavior of all enemies.
        /// Check collisions against player and self, but not other waves
        /// Check weapon collisions against player
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="player"></param>
        /// <param name="blackHole"></param>
        /// <param name="weapon1"></param>
        /// <param name="weapon2"></param>
        public virtual void Update(GameTime gameTime, Spaceman player,
            BlackHole blackHole, Weapon weapon1, Weapon weapon2, InventoryManager inventory, Unicorn[] unicorns)
        {
            if (!Active) { return; }

            //update all enemies in wave
            bool allDestroyed = true;   //check if all enemies destroyed
            for (int i = _enemies.Length - 1; i >= 0; i--)
            {
                if (_enemies[i].UnitLifeState != PhysicalBody.LifeState.Destroyed)
                {
                    allDestroyed = false;       //found one that isn't destroyed
                }

                if (!_enemies[i].Updates)
                    continue;   //don't update units that shouldn't be updated

                for (int j = i - 1; j >= 0; j--)
                {
                    //check collision against other enemies in same wave
                    _enemies[i].CheckAndApplyUnitCollision(_enemies[j]);
                }

                for (int j = 0; j < unicorns.Length; j++)
                {
                    //check collision against unicorns
                    unicorns[j].CheckAndApplyCollision(_enemies[i], gameTime);
                }
                _enemies[i].CheckAndApplyUnitCollision(player);
                _enemies[i].CheckAndApplyWeaponCollision(player, gameTime.ElapsedGameTime);

                _enemies[i].Update(gameTime, player.Position, Vector2.Zero, _levelBounds);
                blackHole.ApplyToUnit(_enemies[i], gameTime);
                if (weapon1 != null)
                {
                    weapon1.CheckAndApplyCollision(_enemies[i], gameTime.ElapsedGameTime);
                }
                if (weapon2 != null)
                {
                    weapon2.CheckAndApplyCollision(_enemies[i], gameTime.ElapsedGameTime);
                }
                inventory.CheckCollisions(gameTime, _enemies[i]);
            }

            _allDestroyed = allDestroyed;
        }

        public void CheckAndApplyCollisions(Wave otherWave)
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (!_enemies[i].Collides)
                    continue;   //don't check enemies that shouldn't collide

                for (int j = 0; j < otherWave._enemies.Length; j++)
                {
                    //check collision against other enemies 
                    _enemies[i].CheckAndApplyUnitCollision(otherWave._enemies[j]);
                }
            }
        }

        protected void setPosition(Vector2 blackHolePosition, bool inBounds)
        {   //set bounds on new spawn location
            int minX, maxX, minY, maxY;

            //spawn in bounds -- default for burst wave
            minX = 0;
            maxX = _levelBounds.Width;
            minY = 0;
            maxY = _levelBounds.Height;

            if (!inBounds)     //spawn out of bounds
            {
                switch (XnaHelper.RandomInt(0, 3))
                {
                    case 0:     //top
                        minX = -c_outOfBoundsSpawnBuffer;
                        maxX = maxX + c_outOfBoundsSpawnBuffer;
                        minY = -c_outOfBoundsSpawnBuffer;
                        maxY = 0;
                        break;
                    case 1:     //right
                        minX = _levelBounds.Width;
                        maxX = _levelBounds.Width + c_outOfBoundsSpawnBuffer;
                        minY = -c_outOfBoundsSpawnBuffer;
                        maxY = _levelBounds.Height + c_outOfBoundsSpawnBuffer;
                        break;
                    case 2:     //bottom
                        minX = -c_outOfBoundsSpawnBuffer;
                        maxX = _levelBounds.Width + c_outOfBoundsSpawnBuffer;
                        minY = _levelBounds.Height;
                        maxY = _levelBounds.Height + c_outOfBoundsSpawnBuffer;
                        break;
                    case 3:     //left
                        minX = -c_outOfBoundsSpawnBuffer;
                        maxX = 0;
                        minY = -c_outOfBoundsSpawnBuffer;
                        maxY = _levelBounds.Height + c_outOfBoundsSpawnBuffer;
                        break;
                }
            }

            XnaHelper.RandomizeVector(ref _spawnLocation, minX, maxX, minY, maxY);

            //if spawned too close to black hole, try again
            if ((_spawnLocation - blackHolePosition).Length() < c_minBlackholeSpawnDistance)
                setPosition(blackHolePosition, inBounds);
        }

        public virtual void Draw(SpriteBatch sb)
        {
            foreach (Enemy e in _enemies)
            {
                e.Draw(sb);
            }
        }
        #endregion
    }

    class BurstWave : Wave
    {
        //how fast portal effect rotates (degrees/sec)
        const float c_portalRotationRate = 720;
        const string c_portalEffectName = "SpawnPortal1";

        TimeSpan _restTimer;            //how long to wait after activation before spawning
        ParticleEffect _portalEffect;   //particle effect to play once spawning begins
        float _portalAngle;             //so portal effect can rotate

        BurstWave _previousWave;

        public override bool Active
        {
            get
            {
                return _restTimer <= TimeSpan.Zero && !_allDestroyed &&
                    (_previousWave == null || _previousWave._allDestroyed);
            }
        }

        public BurstWave(BurstWaveData data, Rectangle levelBounds, BurstWave previousWave)
            : base(data, false, levelBounds)
        {
            //set up enemies
            //activation delay is zero for trickle waves
            _restTimer = TimeSpan.FromSeconds(data.RestDelay);
            //assign a portal particle effect if it is a burst wave
            _portalEffect = new ParticleEffect(c_portalEffectName);
            _previousWave = previousWave;
            _allDestroyed = false;
        }

        public override void Update(GameTime gameTime, Spaceman player, BlackHole blackHole, Weapon weapon1, Weapon weapon2, InventoryManager inventory, Unicorn[] unicorns)
        {
            //Waiting for previous or already complete
            if (!Active) { return; }

            //resting stage
            if (_restTimer >= TimeSpan.Zero)        //not started yet
            {
                _restTimer -= gameTime.ElapsedGameTime;
                if (_restTimer < TimeSpan.Zero)
                {
                    setPosition(blackHole.Position, true);        //set portal position
                }
                return;         //not ready to start
            }

            //active stage
            //spawning
            //spawn particles if still spawning enemies
            if (_enemySpawnIndex < _numEnemies && SpawnEnable)
            {
                _portalEffect.Spawn(_spawnLocation, 90.0f + _portalAngle, gameTime.ElapsedGameTime, Vector2.Zero);
                _portalEffect.Spawn(_spawnLocation, -90.0f + _portalAngle, gameTime.ElapsedGameTime, Vector2.Zero);
                trySpawn(gameTime.ElapsedGameTime, blackHole.Position);
            }
            _portalAngle += (float)gameTime.ElapsedGameTime.TotalSeconds * c_portalRotationRate;
            _portalEffect.Update(gameTime);

            //update units
            base.Update(gameTime, player, blackHole, weapon1, weapon2, inventory, unicorns);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Active && _portalEffect != null)
                _portalEffect.Draw(sb);

            base.Draw(sb);
        }
    }

    class TrickleWave : Wave
    {
        TimeSpan _startTimer, _endTimer;

        public override bool Active
        {
            get { return _startTimer <= TimeSpan.Zero && !_allDestroyed; }
        }

        public TrickleWave(TrickleWaveData data, Rectangle levelBounds)
            :base(data, true, levelBounds)
        {
            _startTimer = TimeSpan.FromSeconds(data.StartTime);
            _endTimer = TimeSpan.FromSeconds(data.Duration);
        }

        public override void Update(GameTime gameTime, Spaceman player, BlackHole blackHole, Weapon weapon1, Weapon weapon2, InventoryManager inventory, Unicorn[] unicorns)
        {
            if (_startTimer > TimeSpan.Zero)
            {   //not started yet
                _startTimer -= gameTime.ElapsedGameTime;
                return;
            }
            if (_endTimer <= TimeSpan.Zero)
            {
                SpawnEnable = false;
            }
            else
            {
                _endTimer -= gameTime.ElapsedGameTime;
            }

            if (trySpawn(gameTime.ElapsedGameTime, blackHole.Position))
            {   //reposition if enemy spawned successfully
                setPosition(blackHole.Position, false);
            }

            //cycle through enemy slots
            _enemySpawnIndex = (_enemySpawnIndex + 1) % _numEnemies;

            //update enemies
            base.Update(gameTime, player, blackHole, weapon1, weapon2, inventory, unicorns);
        }
    }
}
