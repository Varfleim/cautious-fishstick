
using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean.UI
{
    public enum MapObjectType : byte
    {
        Island,
        ShipGroup
    }

    public struct CMapObject
    {
        public CMapObject(
            EcsPackedEntity selfPE, MapObjectType objectType,
            Transform transform,
            MeshRenderer meshRenderer)
        {
            this.selfPE = selfPE;
            this.objectType = objectType;

            Transform = transform;

            this.meshRenderer = meshRenderer;
        }

        public readonly EcsPackedEntity selfPE;

        public readonly MapObjectType objectType;

        public Transform Transform;

        public Vector3 Position
        {
            get
            {
                return Transform.position;
            }
            set
            {
                Transform.position = value;
            }
        }
        public Vector3 LocalPosition
        {
            get
            {
                return Transform.localPosition;
            }
            set
            {
                Transform.localPosition = value;
            }
        }

        public MeshRenderer meshRenderer;
    }
}