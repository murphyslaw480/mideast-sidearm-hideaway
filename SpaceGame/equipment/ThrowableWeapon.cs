using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SpaceGame.units;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.equipment
{
    class ThrowableWeaponData : ProjectileWeaponData
    {
        public int UsesPerStack;
    }

    class ThrowableWeapon : ProjectileWeapon, IConsumable
    {
        int _ammo, _maxAmmo;

        public ThrowableWeapon(string name, PhysicalUnit owner)
            : base(DataManager.GetData<ThrowableWeaponData>(name), owner, null)
        {
            _maxAmmo = DataManager.GetData<ThrowableWeaponData>(name).UsesPerStack;
            NumUses = _maxAmmo;
        }

        public int NumUses 
        {
            get { return _ammo; }
            set { _ammo = (int)MathHelper.Clamp(value, 0, _maxAmmo); } 
        }

        public void Use(Vector2 target)
        {
            if (NumUses > 0 && base.Trigger(_owner.Position, target))
            {
                NumUses--;
            }
        }
    }
}
