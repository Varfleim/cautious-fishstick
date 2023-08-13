
using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean.Ship
{
    public enum ShipGroupCreatingType : byte
    {
        Space,
        ShipHangar,
        None
    }

    public struct RShipGroupCreating
    {
        public EcsPackedEntity ownerOrganizationPE;

        public EcsPackedEntity parentRegionPE;

        public Vector3 position;

        public DOrderedShipSpace[] orderedShips;
    }
}