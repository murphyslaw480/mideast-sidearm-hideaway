using Microsoft.Xna.Framework;
using SpaceGame.equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceGame.utility;

namespace SpaceGame.graphics
{
    class WeaponSprite : Sprite
    {
		public static Dictionary<string,WeaponSpriteData> WeaponSpriteData;

		public Vector2 HandleOffset, HandleToMuzzle;

        public WeaponSprite(string name)
			:this(WeaponSpriteData[name])
        {}

		protected WeaponSprite(WeaponSpriteData data)
			:base(data, SpriteType.Weapon)
        {
            HandleOffset = new Vector2(data.HandleX, data.HandleY);
			HandleToMuzzle =  new Vector2(data.MuzzleX, data.MuzzleY) - HandleOffset;
        }

    }

    class WeaponSpriteData : SpriteData
    {
        public int HandleX, HandleY;
        public int MuzzleX, MuzzleY;
    }
}
