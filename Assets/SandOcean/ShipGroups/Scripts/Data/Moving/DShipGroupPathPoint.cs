
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean.Ship.Moving
{
    public enum MovementTargetType : byte
    {
        None,
        Cell,
        RAEO,
        EconomicORAEO
    }

    public enum DestinationPointRegion : byte
    {
        None,
        CurrentRegion,
        NeighbourRegion,
        OtherRegion
    }

    public enum DestinationPointTask : byte
    {
        None,
        Moving,
        Landing
    }

    public struct DShipGroupPathPoint
    {
        public DShipGroupPathPoint(
            Vector3 targetPosition,
            EcsPackedEntity targetPE,
            MovementTargetType targetType,
            DestinationPointRegion destinationPointRegion,
            DestinationPointTask destinationPointTask)
        {
            this.destinationPoint = targetPosition;

            this.targetPE = targetPE;

            this.targetType = targetType;

            this.destinationPointRegion = destinationPointRegion;

            this.destinationPointTask = destinationPointTask;
        }

        public Vector3 destinationPoint;

        public EcsPackedEntity targetPE;

        public MovementTargetType targetType;

        public DestinationPointRegion destinationPointRegion;

        public DestinationPointTask destinationPointTask;
    }
}