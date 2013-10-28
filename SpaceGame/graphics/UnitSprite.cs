using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceGame.units;
using SpaceGame.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.graphics
{
    class UnitSprite : Sprite
    {
        protected const string c_armSpritePath = c_unitSpritePath + "arms/";

        public static Dictionary<string, UnitSpriteData> UnitSpriteData;
        PhysicalUnit _unit;
        Texture2D _armTexture;

        /* Explaination of these crazy vectors
         * 
         * unitShoulderOffset is unit's shoulder position relative to center
         * armShoulderPos is the arm texture's shoulder position relative to top-left
         * cornet of the texture.
         * The arm is drawn at absShoulderPos where
         * absShoulderPos = unitShoulderOffset + unit.Position
         * its origin is armShoulderPos
         * 
         * shoulderToHand is the vector from armShoulderPos to the hand.
         * It is all relative to the arm.
         * 
         * Weapon.HandleOffset is the handle position relative to topleft of the texture
         * The weapon is drawn at absShoulderPos (from above)
         * The weapon origin is 
         * _weaponOrigin = Weapon.Handle - shoulderToHand
         */

        Vector2 _unitShoulderOffset, _flippedUnitShoulderOffset;    //unit vectors
        Vector2 _shoulderToHand, _armShoulderPos;                   //arm vectors
        Vector2 _absShoulderPos;             //absolute shoulder draw position. dynamic
        Vector2 _weaponOrigin;                                      //weapon vectors

        public override int AnimationState
        {
            get
            {
                return base.AnimationState;
            }
            set
            {
                return;		//cannot set animation state on UnitSprite - determined by velocity
            }
        }

        public Vector2 WeaponMuzzlePos
        {
            get 
            {
                float angle = XnaHelper.RadiansFromVector(_unit.LookDirection);
                Matrix rot = Matrix.CreateRotationZ(angle);
                Vector2 muzzlePos = _shoulderToHand + _unit.WeaponSprite.HandleToMuzzle;
                return _absShoulderPos + Vector2.Transform(muzzlePos, rot);
            }
        }

        protected UnitSprite(UnitSpriteData data, PhysicalUnit unit)
			:base(data, SpriteType.Unit)
        {
            _unit = unit;
            if (_numStates % 2 == 0)
            {
                throw new Exception(String.Format("UnitSprite {0} has an even number of states along the horizontal axis", data.Name));
            }
            _unitShoulderOffset = new Vector2(data.ShoulderX, data.ShoulderY);
            _flippedUnitShoulderOffset = new Vector2(-data.ShoulderX, data.ShoulderY);
            _armTexture = Content.Load<Texture2D>(c_armSpritePath + data.SpriteArmData.Name);
            _armShoulderPos = new Vector2(data.SpriteArmData.ShoulderX, data.SpriteArmData.ShoulderY);
            _shoulderToHand = new Vector2(data.SpriteArmData.HandX, data.SpriteArmData.HandY);
        }

        public UnitSprite(string name, PhysicalUnit unit)
            : this(UnitSpriteData[name], unit)
        { }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);

            //update current weapon origin
            _weaponOrigin = _unit.WeaponSprite.HandleOffset - _shoulderToHand;

			//update sprite based on unit's x velocity
            float velocityFactor = _unit.Velocity.X / _unit.MaxSpeed;

            if (!FlipH)	//if flipped, velocity response is opposite
            {
                velocityFactor *= -1;
            }

			//convert to positive scale
            velocityFactor = MathHelper.Clamp(velocityFactor, -1, 1) + 1;	//scale from 0 to 2 (1 being stationary)

			//set state based on velocity
            _currentState = (int)(velocityFactor / 2 * _numStates);
        }

        public override void Draw(SpriteBatch batch, Vector2 position)
        {
            base.Draw(batch, position);

            _absShoulderPos = position + _unitShoulderOffset;
            float aimAngle = XnaHelper.RadiansFromVector(_unit.LookDirection);

            //draw arm
            if (FlipH)
            {
                batch.Draw(_armTexture, _absShoulderPos, null, Color.White, aimAngle, _armShoulderPos, Scale, SpriteEffects.None, 0);
            }
            else
            {
                batch.Draw(_armTexture, _absShoulderPos, null, Color.White, aimAngle, _armShoulderPos, Scale, SpriteEffects.None, 0);
            }
            //draw weapon
            if (_unit.WeaponSprite != null)
            {
                _unit.WeaponSprite.Draw(batch, _absShoulderPos, aimAngle, _weaponOrigin);
            }
        }

    }

    class UnitSpriteData : SpriteData
    {
        public int ShoulderX, ShoulderY; //where to anchor arm sprite
        public SpriteArmData SpriteArmData;
    }

	class SpriteArmData
    {
        public string Name;					//name of texture to use as arm
        public int ShoulderX, ShoulderY;	//place to anchor to unitsprite's shoulder
        public int HandX, HandY;		//place to anchor weapon handle
    }

}
