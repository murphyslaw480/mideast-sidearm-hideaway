using Microsoft.Xna.Framework;
using SpaceGame.units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.graphics
{
    class UnitSprite : Sprite
    {
        PhysicalUnit _unit;

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

        public UnitSprite(string name, PhysicalUnit unit)
			:base(name, SpriteType.Unit)
        {
            _unit = unit;
            if (_numStates % 2 == 0)
            {
                throw new Exception(String.Format("UnitSprite {0} has an even number of states along the horizontal axis", name));
            }
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
			//update sprite based on unit's x velocity
            float velocityFactor = _unit.Velocity.X / _unit.MaxSpeed;

            if (!FlipH)	//if flipped, velocity response is opposite
                velocityFactor *=  -1;

			//convert to positive scale
            velocityFactor = MathHelper.Clamp(velocityFactor, -1, 1) + 1;	//scale from 0 to 2 (1 being stationary)

			//set state based on velocity
            _currentState = (int)(velocityFactor / 2 * _numStates);
        }

    }
}
