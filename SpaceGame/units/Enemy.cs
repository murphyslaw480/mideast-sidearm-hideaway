using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.utility;
using SpaceGame.equipment;

namespace SpaceGame.units
{
    public class EnemyData : PhysicalData
    {
        public string MeleeWeaponName;
        public int Difficulty;
    }

    class Enemy : PhysicalUnit
    {
        #region static
        public static Dictionary<string, EnemyData> EnemyDataDict;
        #endregion

        public int Difficulty { get; private set; }

        #region constructor
        public Enemy(string unitName, Rectangle levelBounds)
            :this(DataManager.GetData<EnemyData>(unitName), levelBounds)
        {
        }

        protected Enemy(EnemyData data, Rectangle levelBounds)
            : base(data)
        {
            if (data.MeleeWeaponName != null)
                CurrentWeapon = new MeleeWeapon(data.MeleeWeaponName, this);
            Difficulty = data.Difficulty;
        }
        #endregion

        #region methods
        public virtual void Update(GameTime gameTime, Vector2 playerPosition, Vector2 blackHolePosition, Rectangle levelBounds)
        {
            Vector2 directionToPlayer = XnaHelper.DirectionBetween(Position, playerPosition);
            MoveDirection = directionToPlayer;
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

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            CurrentWeapon.Draw(sb);
        }
        #endregion
    }
}
