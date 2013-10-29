using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SpaceGame.graphics;
using SpaceGame.equipment;

namespace SpaceGame.units
{
    /// <summary>
    /// Player character
    /// </summary>
    class Spaceman : PhysicalUnit
    {
        #region constants
        const string SPACEMAN_NAME = "Spaceman";
        const string THRUSTER_EFFECT_NAME = "SpacemanThruster";
        #endregion

        #region static
        public static PhysicalData AstronautData;
        #endregion

        #region members
        ParticleEffect thrusterParticleEffect;
        Weapon _primaryWeapon, _secondaryWeapon;
        public Weapon PrimaryWeapon { 
            get { return _primaryWeapon; } 
            set { 
                _primaryWeapon = value; 
                if (value != null) {CurrentWeapon = value;}
            }
        }
        public Weapon SecondaryWeapon { 
            get { return _secondaryWeapon; } 
            set { 
                _secondaryWeapon = value; 
                if (value != null) {CurrentWeapon = value;}
            }
        }

        #endregion

        public Spaceman(Vector2 startPosition)
            :base(AstronautData)
        {
            thrusterParticleEffect = new ParticleEffect(THRUSTER_EFFECT_NAME);
            _lifeState = LifeState.Living;      //astronaut starts pre-spawned
            Position = startPosition;
            CurrentWeapon = PrimaryWeapon;
        }

        public override void TriggerWeapon(Vector2 target, int num = 0)
        {
            if (num == 0 && PrimaryWeapon != null)
            {
                CurrentWeapon = PrimaryWeapon;
            }
            else if (num == 1 && SecondaryWeapon != null)
            {
                CurrentWeapon = SecondaryWeapon;
            }

            base.TriggerWeapon(target, num);
        }
    }
}
