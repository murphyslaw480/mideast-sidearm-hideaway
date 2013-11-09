using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.utility;
using SpaceGame.equipment;

namespace SpaceGame.units
{
    /// <summary>
    /// A unit with physics properties
    /// superclass for player and most enemies (not including unicorns)
    /// </summary>
    class PhysicalUnit : PhysicalBody
    {
        #region constant
        //how much fire effect causes panic (random movement)
        const float FIRE_PANIC_THRESHOLD = 20.0f;
        //how often to change direction while panicking
        const float PANIC_DIRECTION_CHANGE_FREQUENCY = 0.5f;
        //factor of max health used to represent frozen integrity
        //damage is dealt to this while frozen - shatter if < 0
        #endregion

        #region fields
        TimeSpan _panicTimer;   //time till next direction switch
        ParticleEffect _movementParticleEffect;
        #endregion

        #region properties
        public Weapon CurrentWeapon { get; protected set; }
        public WeaponSprite WeaponSprite
        {
            get { return CurrentWeapon == null ? null : CurrentWeapon.Sprite; }
        }

        //determine behavior for next update
        Vector2 _moveDirection;
        Vector2 _lookDirection;
        public Vector2 MoveDirection
        {
            get { return _moveDirection; }
            set { _moveDirection = Panicked ? _moveDirection : value; }
        }
        public Vector2 LookDirection
        {
            get { return _lookDirection; }
            set { _lookDirection = Panicked ? _lookDirection : value; }
        }
        public bool Panicked
        {
            get { return _statusEffects.Fire > FIRE_PANIC_THRESHOLD; }
        }
        #endregion

        #region other members
        #endregion

        #region constructor
        /// <summary>
        /// Create a new physical sprite from data
        /// </summary>
        /// <param name="pd">data from which to construct unit</param>
        protected PhysicalUnit(PhysicalData pd)
            :base(pd, Sprite.SpriteType.Unit)
        {
            MoveDirection = Vector2.Zero;
            LookDirection = Vector2.Zero;
            if (pd.MovementParticleEffectName != null)
                _movementParticleEffect = new ParticleEffect(pd.MovementParticleEffectName);
        }

        #endregion

        #region methods
        #region Update Logic
        public override void Update(GameTime gameTime, Rectangle levelBounds)
        {
            base.Update(gameTime, levelBounds);
            if (CanMove)
            {
                if (Panicked)
                {
                    _panicTimer -= gameTime.ElapsedGameTime;
                    if (_panicTimer <= TimeSpan.Zero)
                    {
                        _panicTimer = TimeSpan.FromSeconds(PANIC_DIRECTION_CHANGE_FREQUENCY);
                        XnaHelper.RandomizeVector(ref _moveDirection, -1, 1, -1, 1);
                        _lookDirection = _moveDirection;
                    }
                }
                lookThisWay(LookDirection);
                if (MoveDirection.Length() > 0)
                    moveThisWay(MoveDirection, gameTime);
            }

            MoveDirection = Vector2.Zero;

            if (_movementParticleEffect != null)
                _movementParticleEffect.Update(gameTime);
        }

        /// <summary>
        /// Move the sprite in the given direction based on its moveForce property
        /// </summary>
        /// <param name="direction">Direction to move. Should be normalized for normal movement.</param>
        private void moveThisWay(Vector2 direction, GameTime gameTime)
        {
            if (CanMove)
            {
                //apply movement force, taking into account cryo effect (which slows)
                ApplyForce(_moveForce * direction * (1 - _statusEffects.Cryo / MAX_STAT_EFFECT));
                if (_movementParticleEffect != null)
                    _movementParticleEffect.Spawn(Center + (_sprite as UnitSprite).ExhaustOffset, XnaHelper.DegreesFromVector(-direction), gameTime.ElapsedGameTime, Velocity);
            }
        }

        protected void lookThisWay(Vector2 direction)
        {
            if (CanMove)
            {
                float angle = XnaHelper.RadiansFromVector(direction);

                if (angle > 0 && angle < Math.PI)
                    _sprite.FlipH = true;
                else
                    _sprite.FlipH = false;
            }
        }

        #endregion

        #region Draw Logic
        public override void Draw(SpriteBatch sb)
        {
            if (_movementParticleEffect != null)
                _movementParticleEffect.Draw(sb);

            base.Draw(sb);
        }

        public virtual void TriggerWeapon(Vector2 target, int num = 0)
        {
            if (CurrentWeapon != null)
            {
                CurrentWeapon.Trigger((_sprite as UnitSprite).WeaponMuzzlePos, target);
            }
        }

        #endregion
        #endregion
    }
}
