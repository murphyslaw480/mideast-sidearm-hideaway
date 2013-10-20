using Microsoft.Xna.Framework;
using SpaceGame.equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.graphics
{
    class WeaponSprite : Sprite
    {
		public static Dictionary<string,WeaponSpriteData> WeaponSpriteDict;

		Vector2 _handlePos, _muzzlePos;

        public WeaponSprite(string name)
			:this(WeaponSpriteDict[name])
        {}

		protected WeaponSprite(WeaponSpriteData data)
			:base(data, SpriteType.None)
        {
            _handlePos = new Vector2(data.HandleX, data.HandleY);
			_muzzlePos =  new Vector2(data.MuzzleX, data.MuzzleY);
        }
    }

    class WeaponSpriteData : SpriteData
    {
        public int HandleX, HandleY;
        public int MuzzleX, MuzzleY;
    }
}
