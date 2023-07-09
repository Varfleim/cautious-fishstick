
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean.Ship
{
    public struct DOrderedShipSpace
    {
        public DOrderedShipSpace(
            DVector3 shipPosition,
            int shipClassContentSetIndex,
            int shipClassIndex)
        {
            this.shipPosition = shipPosition;

            this.shipClassContentSetIndex = shipClassContentSetIndex;
            this.shipClassIndex = shipClassIndex;
        }

        public readonly DVector3 shipPosition;

        public readonly int shipClassContentSetIndex;
        public readonly int shipClassIndex;
    }
}