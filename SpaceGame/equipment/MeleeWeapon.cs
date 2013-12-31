using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.units;
using SpaceGame.utility;
using SpaceGame.utilities;

namespace SpaceGame.equipment
{
    class MeleeWeapon : Weapon
    {
        #region static
        public class MeleeWeaponData : WeaponData
        {
            public int Damage;
            public int Impact;
            public float Range;
            public float Recoil;

            public float SwingArc;
            public float SwingSpeed;

            public string AttackParticleEffect;
            public string HitParticleEffect;
        }
        #endregion

        #region fields
        int _damage;
        int _force;
        float _range;
        float _recoil;
        float _swingArc;  //total swing arc in radians
        float _swingAngle;  //current angle of swing
        float _swingSpeed;  //in radians
        ParticleEffect _attackParticleEffect;
        ParticleEffect _hitParticleEffect;
        Vector2 _tempVector;
        #endregion

        #region properties
        public override float Range { get { return _range; } }
        #endregion

        enum SwingState
        {
            Waiting,
            Swinging,
            Backswing
        }
        SwingState _swingState;

        #region constructor
        public MeleeWeapon(string weaponName, PhysicalUnit owner)
            : this(DataManager.GetData<MeleeWeaponData>(weaponName), owner)
        { }

        protected MeleeWeapon(MeleeWeaponData data, PhysicalUnit owner)
            :base(data, owner, data.Name)
        {
            _damage = data.Damage;
            _force = data.Impact;
            _recoil = data.Recoil;
            _swingArc = MathHelper.ToRadians(data.SwingArc);
            _swingSpeed = MathHelper.ToRadians(data.SwingSpeed);
            _range = data.Range;
            _attackParticleEffect = (data.AttackParticleEffect == null) ?
                ParticleEffect.NullEffect : 
                new ParticleEffect(data.AttackParticleEffect);
            _hitParticleEffect = (data.HitParticleEffect == null) ?
                null : new ParticleEffect(data.HitParticleEffect);
            _swingState = SwingState.Waiting;
        }
        #endregion

        #region methods
        public override void CheckAndApplyCollision(PhysicalBody unit, TimeSpan time)
        {
            if (_swingState != SwingState.Swinging || !unit.Collides)
                return;     //don't check collisions if not swinging

            float fireAngle = XnaHelper.RadiansFromVector(_fireDirection);
            if (XnaHelper.RectangleIntersectsArc(unit.HitRect, _owner.Center, _range, fireAngle, _swingArc))
            { 
                _tempVector = unit.Center - _owner.Center;
                _tempVector.Normalize();
                unit.ApplyImpact(_force * _tempVector, 1);
                bool wasAlive = unit.Health > 0;
                unit.ApplyDamage(_damage);
                if (wasAlive && unit.Health <= 0)
                {   //this attack killed it
                    ScoreManager.RegisterScore(unit.Position, ScoreType.MeleeKill);
                }
            }
        }
        protected override void UpdateWeapon(GameTime gameTime)
        {
            switch (_swingState)
            {
                case SwingState.Swinging:
                    {
                        _swingAngle -= _swingSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        Sprite.SetSwingAngle(_swingAngle);
                        if (-_swingAngle > _swingArc)
                        {
                            _swingState = SwingState.Backswing;
                        }
                        break;
                    }
                case SwingState.Backswing:
                    {
                        _swingAngle += _swingSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        Sprite.SetSwingAngle(_swingAngle);
                        if (_swingAngle > 0)
                        {
                            _swingState = SwingState.Waiting;
                            _swingAngle = 0;
                            Sprite.SetSwingAngle(0);
                        }
                        break;
                    }

                case SwingState.Waiting:
                    {
                        if (_firing)
                        {
                            _swingState = SwingState.Swinging;
                            Sprite.SetSwingAngle(0);    //start swing
                            _attackParticleEffect.Spawn(_owner.Center, XnaHelper.DegreesFromVector(_fireDirection),
                                gameTime.ElapsedGameTime, _owner.Velocity);
                        }
                        break;
                    }
            }
            _attackParticleEffect.Update(gameTime);
        }
        public override void Draw(SpriteBatch sb)
        {
            _attackParticleEffect.Draw(sb);
        }
        #endregion
    }
}
