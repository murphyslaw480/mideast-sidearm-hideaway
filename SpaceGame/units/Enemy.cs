using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.utility;
using SpaceGame.equipment;
using SpaceGame.utilities;

namespace SpaceGame.units
{
    public class EnemyData : PhysicalData
    {
        public string Weapon;
        public bool Ranged;
        public float IdealRange;        //preferred range from player
        public int Difficulty;
    }

    class Enemy : PhysicalUnit
    {
        #region static
        public static Dictionary<string, EnemyData> EnemyDataDict;
        //multiple of black hole radius to avoid
        const float c_blackHoleSafety = 2.0f;
        #endregion

        public int Difficulty { get; private set; }
        float _idealRange;

        #region constructor
        public Enemy(string unitName, Rectangle levelBounds)
            :this(DataManager.GetData<EnemyData>(unitName), levelBounds)
        {
        }

        protected Enemy(EnemyData data, Rectangle levelBounds)
            : base(data)
        {
            if (data.Weapon != null)
            {
                if (data.Ranged)
                {
                    CurrentWeapon = new ProjectileWeapon(data.Weapon, this);
                }
                else
                {
                    CurrentWeapon = new MeleeWeapon(data.Weapon, this);
                }
            }
            Difficulty = data.Difficulty;
            _idealRange = data.IdealRange;
        }
        #endregion

        #region methods
        public virtual void Update(GameTime gameTime, Vector2 playerPosition, BlackHole blackHole, Rectangle levelBounds)
        {
            Vector2 playerDisposition = playerPosition - Position;
            Vector2 directionToPlayer;
            Vector2 blackHoleDisposition = blackHole.Position - Position;
            Vector2.Normalize(ref playerDisposition, out directionToPlayer);
            if (blackHoleDisposition.Length() < blackHole.Radius * c_blackHoleSafety)
            {   //too close to black hole
                blackHoleDisposition.Normalize();
                MoveDirection = -blackHoleDisposition;
            }
            else
            {
                MoveDirection = playerDisposition.Length() > _idealRange ? directionToPlayer : -directionToPlayer;
            }
            LookDirection = directionToPlayer;
            if (CurrentWeapon != null)
            {
                CurrentWeapon.Update(gameTime);

                if ((playerPosition - Position).Length() <= CurrentWeapon.Range && _lifeState == LifeState.Living)
                    CurrentWeapon.Trigger(Center, playerPosition);
            }
            base.Update(gameTime, levelBounds);
        }

        public void CheckAndApplyWeaponCollision(PhysicalUnit unit, TimeSpan time)
        {
            if (CurrentWeapon != null)
                CurrentWeapon.CheckAndApplyCollision(unit, time);
        }

        public override float EatByBlackHole(Vector2 blackHolePos, float blackHoleRadius)
        {
            bool wasFrozen = UnitLifeState == LifeState.Frozen;
            float massEaten = base.EatByBlackHole(blackHolePos, blackHoleRadius);
            if (massEaten > 0)
            {   //was eaten. apply scoring
                if (wasFrozen)
                {
                    ScoreManager.RegisterScore(Position, ScoreType.EatFrozen);
                }
                else if (Health > 0)
                {
                    ScoreManager.RegisterScore(Position, ScoreType.EatLiving);
                }
                else if (_statusEffects.Fire > 0)
                {
                    ScoreManager.RegisterScore(Position, ScoreType.EatBurning);
                }
            }
            return massEaten;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            CurrentWeapon.Draw(sb);
        }
        #endregion
    }
}
