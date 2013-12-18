using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.units
{
    public class PhysicalData
    {
        //stat effect decrease per second
        public const float DEFAULT_STAT_RESIST = 10;

        public String Name;
        public String MovementParticleEffectName;
        public float Mass;
        public float GravitySensitivity;
        public float MoveForce;
        public float MaxSpeed;
        public float DecelerationFactor;
        public float Health;
        public float FireResist;
        public float ShockResist;
        public float CryoResist;

        public PhysicalData()
        {
            FireResist = DEFAULT_STAT_RESIST;
            CryoResist = DEFAULT_STAT_RESIST;
            ShockResist = DEFAULT_STAT_RESIST;
            GravitySensitivity = 1f;
        }
    }
}
