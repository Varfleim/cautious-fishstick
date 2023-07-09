using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean
{
    public class Conversions
    {
        public const double aUToM = 149597870700d;

        public const double sMToKG = 1988920000000000000000000000000d;
        public const double eMToKG = 5974200000000000000000000d;

        public const double secPerYear = 31536000d;
        public const double secPerTick = 3600d;

        public const double G = 0.000000000066743d;

        public const int orbitalResolution = 360;

        public const double timeStep = 1d;

        public const double shipSpeedModifier = 1000000d;

        public static Vector3 ConvertDVector3ToUVector3(DVector3 sVector3)
        {
            return new Vector3(
                (float)sVector3.x,
                (float)sVector3.y,
                (float)sVector3.z);

        }

        public static DVector3 ConvertUVector3ToDVector3(Vector3 sVector3)
        {
            return new DVector3(
                sVector3.x,
                sVector3.y,
                sVector3.z);

        }
    }
}