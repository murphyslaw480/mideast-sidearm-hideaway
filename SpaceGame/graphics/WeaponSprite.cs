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
        private Vector2 _handleOffset;
        public Vector2 HandleOffset
        {
            get 
            {
                if (FlipH)
                {
                    return new Vector2(FrameWidth - _handleOffset.X, _handleOffset.Y);
                }
                return _handleOffset; 
            }
            set { _handleOffset = value; }
        }

        private Vector2 _handleToMuzzle;
        public Vector2 HandleToMuzzle
        {
            get 
            {
                return _handleToMuzzle; 
            }
            set { _handleToMuzzle = value; }
        }

        public WeaponSprite(string name)
			:this(DataManager.GetData<WeaponSpriteData>(name))
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
