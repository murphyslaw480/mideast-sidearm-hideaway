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

		public Vector2 HandleOffset, MuzzleOffset;

        public WeaponSprite(string name)
			:this(WeaponSpriteData[name])
        {}

		protected WeaponSprite(WeaponSpriteData data)
			:base(data, SpriteType.Weapon)
        {
            HandleOffset = new Vector2(data.HandleX, data.HandleY);
			MuzzleOffset =  new Vector2(data.MuzzleX, data.MuzzleY);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch, Vector2 position, float rotation, Vector2 origin)
        {
            base.Draw(batch, position, rotation, origin);
            XnaHelper.DrawRect(Color.Blue, position + origin, 5, 5, batch);
            XnaHelper.DrawRect(Color.Green, position, 5, 5, batch);
        }
    }

    class WeaponSpriteData : SpriteData
    {
        public int HandleX, HandleY;
        public int MuzzleX, MuzzleY;
    }
}
