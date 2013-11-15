using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.units;
using SpaceGame.utility;

namespace SpaceGame.equipment
{
    class ProjectileWeapon : Weapon
    {
        #region constant
        #endregion

        #region static
        public static Matrix tempMatrix;
        #endregion

        #region properties
        public override float Range
        {
            get 
            { 
				float v = _projectileInfo.Speed;
				float a = _projectileInfo.Acceleration;
				float t = _projectileInfo.SecondsToLive;
                return 0.5f * a * (float)Math.Pow(t,2) + v * t; 
            }
        } 
        #endregion

        #region fields
        string _name;
        int _projectilesPerFire;
        float _spread;
        float _sourceVelFactor;
        ProjectileData _projectileInfo;
        ParticleEffect _fireParticleEffect;
        ParticleEffect _shellParticleEffect;
        Projectile[] _projectiles;
        ProjectileEffect _contactEffect;
        ProjectileEffect _proximityEffect;
        ProjectileEffect _destinationEffect;
        #endregion

        #region constructor
        public ProjectileWeapon(string name, PhysicalUnit owner)
            : this(DataManager.GetData<ProjectileWeaponData>(name), owner, name)
        { }

        protected ProjectileWeapon(ProjectileWeaponData data, PhysicalUnit owner, string spriteName)
            :base(data, owner, spriteName)
        {
            _name = data.Name;
            _projectilesPerFire = data.ProjectilesPerFire;
            _projectileInfo = data.ProjectileInfo;
            _spread = data.Spread;
            _sourceVelFactor = data.SourceVelocityFactor;

            _contactEffect = _projectileInfo.ContactEffect == null ?
                ProjectileEffect.NullEffect : new ProjectileEffect(_projectileInfo.ContactEffect);
            _proximityEffect = _projectileInfo.ProximityEffect == null ?
                 ProjectileEffect.NullEffect : new ProjectileEffect(_projectileInfo.ProximityEffect);
            _destinationEffect = _projectileInfo.DestinationEffect == null ? 
                ProjectileEffect.NullEffect : new ProjectileEffect(_projectileInfo.DestinationEffect);

            _fireParticleEffect = data.FireParticleEffectName == null ? 
                null : new ParticleEffect(data.FireParticleEffectName);

            _shellParticleEffect = data.ShellParticleEffectName == null ? 
                null : new ParticleEffect(data.ShellParticleEffectName);

            float maxProjLife = data.ProjectileInfo.SecondsToLive +
                Math.Max((float)_contactEffect.Duration.TotalSeconds, (float)_destinationEffect.Duration.TotalSeconds);
            float maxProjectiles = data.FireRate * maxProjLife * data.ProjectilesPerFire;
            maxProjectiles = Math.Max(maxProjectiles, _projectilesPerFire);
            _projectiles = new Projectile[(int)maxProjectiles + 1];
            for (int i = 0; i < _projectiles.Length; i++)
            {
                _projectiles[i] = new Projectile(data.ProjectileInfo.SpriteName);
            }
        }
        #endregion

        #region methods
        public override void CheckAndApplyCollision(PhysicalBody unit, TimeSpan time)
        {
            if (!unit.Collides)
                return;

            foreach (Projectile p in _projectiles)
            {
                p.CheckAndApplyCollision(unit, time);
            }
        }

        protected override void UpdateWeapon(GameTime gameTime)
        {
            _contactEffect.Update(gameTime);
            _destinationEffect.Update(gameTime);
            _proximityEffect.Update(gameTime);

            int projectilesToSpawn = _firing ? _projectilesPerFire : 0;

            foreach (Projectile p in _projectiles)
            {
                if (p.ProjectileState == Projectile.State.Dormant
                    && projectilesToSpawn > 0)
                {
                    float rotAngle = XnaHelper.RandomAngle(0, _spread);
                    Matrix.CreateRotationZ(MathHelper.ToRadians(rotAngle), out tempMatrix);
                    p.Initialize(_fireLocation, Vector2.Transform(_fireDirection, tempMatrix),
                        _projectileInfo, _targetDestination, _owner.Velocity * _sourceVelFactor,
                        _contactEffect, _destinationEffect,
                        _proximityEffect);
                    projectilesToSpawn--;
                }

                p.Update(gameTime);
            }

            System.Diagnostics.Debug.Assert(projectilesToSpawn == 0, "did not spawn all projectiles", "Number left: " + projectilesToSpawn, 
                new object[] {this});

            if (_fireParticleEffect != null)
            {
                if (_firing)
                {
                    _fireParticleEffect.Spawn(
                        _fireLocation, XnaHelper.DegreesFromVector(_fireDirection),
                        gameTime.ElapsedGameTime, _owner.Velocity);
                }
                _fireParticleEffect.Update(gameTime);
            }

            if (_shellParticleEffect != null)
            {
                if (_firing)
                {
                    //spawn shells halfway between owner and muzzle
                    Vector2 spawnShellLocation = (_fireLocation + _owner.Position) / 2;
                    _shellParticleEffect.Spawn(
                        spawnShellLocation, XnaHelper.DegreesFromVector(_fireDirection),
                        gameTime.ElapsedGameTime, _owner.Velocity);
                }
                _shellParticleEffect.Update(gameTime);
            }
        }

        /// <summary>
        /// Draw the projectiles and particle effects of the weapon
        /// Drawing of the WeaponSprite is handled by UnitSprite
        /// </summary>
        /// <param name="sb"></param>
        public override void Draw(SpriteBatch sb)
        {
            if (_fireParticleEffect != null)
                _fireParticleEffect.Draw(sb);

            if (_shellParticleEffect != null)
                _shellParticleEffect.Draw(sb);

            _contactEffect.Draw(sb);
            _proximityEffect.Draw(sb);
            _destinationEffect.Draw(sb);

            foreach (Projectile p in _projectiles)
            {
                p.Draw(sb);
            }
        }
        #endregion
    }

    public class ProjectileWeaponData : WeaponData
    {
        public float Spread;
        public int ProjectilesPerFire;
        public float SourceVelocityFactor;      //how much weapon's velocity affects projectile velocity
        public ProjectileData ProjectileInfo;
        public string FireParticleEffectName;   //particle effect that exits barrel
        public string ShellParticleEffectName;  //particle effect for ejected shell

        public ProjectileWeaponData()
        {
            ProjectilesPerFire = 1;
            SourceVelocityFactor = 1;
        }
    }

}
