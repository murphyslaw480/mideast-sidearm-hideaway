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
        protected const string c_legSpritePath = c_unitSpritePath + "legs/";
        protected const float c_legVelFactor = 0.002f;
        protected const float c_maxLegAngle = MathHelper.PiOver4;

        PhysicalUnit _unit;
        Texture2D _armTexture, _legTexture;

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

        Vector2 _unitShoulderOffset, _flippedUnitShoulderOffset;    //unit arm positioning vectors
        Vector2 _unitHipOffset, _flippedUnitHipOffset;              //unit hip vectors
        Vector2 _legOrigin, _flippedLegOrigin;                      //leg vectors
        Vector2 _shoulderToHand, _armShoulderPos;                   //arm vectors
        Vector2 _absShoulderPos, _absHipPos;                        //absolute shoulder/leg draw position. dynamic
        Vector2 _weaponOrigin;                                      //weapon vectors

        Vector2 _exhaustOffset;                             //where to spawn jetpack particles (offset from center)
        public Vector2 ExhaustOffset
        {
            get 
            {   //mirror exhaust position if sprite mirrored
                return FlipH ? new Vector2(-_exhaustOffset.X, _exhaustOffset.Y) : _exhaustOffset; 
            }
        }

        float _legAngle;                                            //angle of legs based on velocity

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
                if (_unit.WeaponSprite == null) { throw new Exception("Cannot ask for WeaponMuzzlePos of null WeaponSprite"); }
                float angle = XnaHelper.RadiansFromVector(_unit.LookDirection);
                Matrix rot = Matrix.CreateRotationZ(angle);
                Vector2 muzzlePos = _shoulderToHand + _unit.WeaponSprite.HandleToMuzzle;
                if (FlipH)
                {
                    muzzlePos.X *= -1;
                }
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
            //shoulder positioning
            _unitShoulderOffset = new Vector2(data.ShoulderX, data.ShoulderY);
            _flippedUnitShoulderOffset = new Vector2(-data.ShoulderX, data.ShoulderY);
            _armTexture = Content.Load<Texture2D>(c_armSpritePath + data.SpriteArmData.Name);
            //hand/weapon positioning
            _armShoulderPos = new Vector2(data.SpriteArmData.ShoulderX, data.SpriteArmData.ShoulderY);
            _shoulderToHand = new Vector2(data.SpriteArmData.HandX, data.SpriteArmData.HandY);
            //leg positioning
            _legTexture = Content.Load<Texture2D>(c_legSpritePath + data.SpriteLegData.Name);
            _unitHipOffset = new Vector2(data.HipX, data.HipY);
            _flippedUnitHipOffset = new Vector2(-data.HipX, data.HipY);
            _legOrigin = new Vector2(data.SpriteLegData.HipX, data.SpriteLegData.HipY);
            _flippedLegOrigin = new Vector2(_legTexture.Width - data.SpriteLegData.HipX, data.SpriteLegData.HipY);

            _exhaustOffset = new Vector2(data.ExhaustX, data.ExhaustY);
        }

        public UnitSprite(string name, PhysicalUnit unit)
            : this(DataManager.GetData<UnitSpriteData>(name), unit)
        { }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);

            //update current weapon origin
            if (_unit.WeaponSprite != null)
            {
                _weaponOrigin = _unit.WeaponSprite.HandleOffset - _shoulderToHand;
            }

			//angle legs based on velocity
            _legAngle = _unit.Velocity.X * c_legVelFactor;

			//cap leg rotation
            _legAngle = MathHelper.Clamp(_legAngle, -c_maxLegAngle, c_maxLegAngle);
        }

        public override void Draw(SpriteBatch batch, Vector2 position)
        {
            SpriteEffects spriteEffect = FlipH ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float aimAngle = XnaHelper.RadiansFromVector(_unit.LookDirection);
            _absShoulderPos = position + _unitShoulderOffset;
            _absHipPos = position + (FlipH ? _flippedUnitHipOffset : _unitHipOffset);

            //draw leg
            batch.Draw(_legTexture, _absHipPos, null, Color.White, _legAngle, (FlipH ? _flippedLegOrigin : _legOrigin), Scale, spriteEffect, 0);

            if (!FlipH)
            {   //draw arm and weapon, and 
                aimAngle += _unit.WeaponSprite.ArmAngleOffset;

                if (_unit.WeaponSprite != null)
                {   //draw weapon behind unit
                    _unit.WeaponSprite.FlipH = false;
                    _unit.WeaponSprite.Draw(batch, _absShoulderPos, aimAngle, _weaponOrigin);
                }
                batch.Draw(_armTexture, _absShoulderPos, null, Color.White, aimAngle, _armShoulderPos, Scale, spriteEffect, 0);
            }

            base.Draw(batch, position);
            
            if (FlipH)
            {   //draw arm and weapon in front of unit, leg still behind
                aimAngle -= _unit.WeaponSprite.ArmAngleOffset;
                if (_unit.WeaponSprite != null)
                {   //draw weapon behind unit
                    _unit.WeaponSprite.FlipH = true;
                    _unit.WeaponSprite.Draw(batch, _absShoulderPos, aimAngle, _weaponOrigin);
                }

                batch.Draw(_armTexture, _absShoulderPos, null, Color.White, aimAngle, _armShoulderPos, Scale, spriteEffect, 0);
            }

            XnaHelper.DrawRect(Color.Red, _absHipPos, 5, 5, batch);
        }

    }

    class UnitSpriteData : SpriteData
    {
        public int ShoulderX, ShoulderY;    //where to anchor arm sprite
        public int HipX, HipY;              //where to anchor leg sprite
        public int ExhaustX, ExhaustY;      //where to spawn jetpack particles (offset from center)
        public SpriteArmData SpriteArmData;
        public SpriteLegData SpriteLegData;
    }

	class SpriteArmData
    {
        public string Name;					//name of texture to use as arm
        public int ShoulderX, ShoulderY;	//place to anchor to unitsprite's shoulder
        public int HandX, HandY;		    //place to anchor weapon handle
    }

	class SpriteLegData
    {
        public string Name;					//name of texture to use as leg
        public int HipX, HipY;	            //place to anchor to unitsprite's hip
    }

}
