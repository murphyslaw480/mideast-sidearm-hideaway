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

		Vector2 _handlePos, muzzlePos;

        public WeaponSprite(string name)
			:this(WeaponSpriteDict[name])
        {}

		protected WeaponSprite(WeaponSpriteData data)
			:base(data, SpriteType.None)
        {
			_handlePos = new Vector2(data.
        }
    }

    class WeaponSpriteData : SpriteData
    {
        int handleX, handleY;
        int muzzleX, muzzleY;
    }
}
