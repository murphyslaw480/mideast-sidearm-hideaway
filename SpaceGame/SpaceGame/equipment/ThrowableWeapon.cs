using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SpaceGame.units;

namespace SpaceGame.equipment
{
    class ThrowableWeapon : ProjectileWeapon, IConsumable
    {
        public static Dictionary<string, ProjectileWeaponData> DataDict;

        public ThrowableWeapon(string name, PhysicalUnit owner)
            : base(DataDict[name], owner)
        { }

        public int NumUses 
        {
            get { return Ammo; }
            set { Ammo = value; }
        }

        public void Use(Vector2 target)
        {
            base.Trigger(_owner.Position, target);
        }
    }
}
