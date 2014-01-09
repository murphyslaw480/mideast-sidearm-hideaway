using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceGame.graphics;
using SpaceGame.units;
using SpaceGame.utilities;
using SpaceGame.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.equipment
{
    class LaserData : WeaponData
    {
        public float MinDamage, MaxDamage;
        public float MinDuration, MaxDuration;
        public float Range;
        public string LaserTexture;
    }

    class Laser : Weapon
    {
        float _range;
        float _minDamage, _maxDamage;
        float _minDuration, _maxDuration;
        Texture2D _laserTexture;

        public override void CheckAndApplyCollision(PhysicalBody unit, TimeSpan time)
        {
            if (!_firing || !unit.Collides)
            {
                return;
            }
            else
            {
                Point laserStart = new Point((int)_fireLocation.X, (int)_fireLocation.Y);
                Vector2 path = _fireDirection * _range;
                Point laserEnd = new Point(laserStart.X + (int)path.X, laserStart.Y + (int)path.Y);
                if (XnaHelper.SegmentIntersectsRect(laserStart, laserEnd, unit.HitRect))
                {
                    bool wasAlive = unit.Health > 0;
                    unit.ApplyDamage(MathHelper.Lerp(_minDamage, _maxDamage, _currentCharge / _maxCharge);
                    if (wasAlive && unit.Health <= 0)
                    {   //this attack killed it
                        ScoreManager.RegisterScore(unit.Position, ScoreType.RangeKill);
                    }
                }
            }
        }
    }
}
