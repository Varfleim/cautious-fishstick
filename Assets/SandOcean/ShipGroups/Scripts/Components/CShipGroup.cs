
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

using SandOcean.Ship.Moving;

namespace SandOcean.Ship
{
    public enum ShipGroupMovingMode : byte
    {
        None,
        Idle,
        Moving
    }

    public enum ShipGroupDockingMode : byte
    {
        None,
        Ether,
        RAEO,
        EconomicORAEO
    }

    public struct CShipGroup
    {
        public CShipGroup(
            EcsPackedEntity selfPE, 
            EcsPackedEntity ownerOrganizationPE,
            EcsPackedEntity parentRegionPE,
            Vector3 position)
        {
            this.selfPE = selfPE;

            this.ownerOrganizationPE = ownerOrganizationPE;

            this.parentRegionPE = parentRegionPE;

            movingMode = ShipGroupMovingMode.Idle;

            this.position = position;

            dockingMode = ShipGroupDockingMode.None;
            dockingPE = new();
        }

        public readonly EcsPackedEntity selfPE;

        public readonly EcsPackedEntity ownerOrganizationPE;

        public EcsPackedEntity parentRegionPE;

        public ShipGroupMovingMode movingMode;

        public Vector3 position;

        public ShipGroupDockingMode dockingMode;
        public EcsPackedEntity dockingPE;
    }
}