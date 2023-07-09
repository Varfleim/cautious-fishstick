
using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean.Map
{
    public struct CHexChunk
    {
        public CHexChunk(
            EcsPackedEntity selfPE,
            int coordinateX, int coordinateZ,
            Transform transform, Canvas canvas,
            GOMapMesh terrain, GOMapMesh rivers, GOMapMesh roads, GOMapMesh water, GOMapMesh waterShore, GOMapMesh estuaries,
            GOMapFeatureManager features,
            int cellCount)
        {
            this.selfPE = selfPE;
            this.coordinateX = coordinateX;
            this.coordinateZ = coordinateZ;

            this.transform = transform;
            this.canvas = canvas;

            this.terrain = terrain;
            this.rivers = rivers;
            this.roads = roads;
            this.water = water;
            this.waterShore = waterShore;
            this.estuaries = estuaries;

            this.features = features;

            regionPEs = new EcsPackedEntity[cellCount];
        }

        public readonly EcsPackedEntity selfPE;
        public readonly int coordinateX;
        public readonly int coordinateZ;

        public Transform transform;
        public Canvas canvas;

        public GOMapMesh terrain;
        public GOMapMesh rivers;
        public GOMapMesh roads;
        public GOMapMesh water;
        public GOMapMesh waterShore;
        public GOMapMesh estuaries;

        public GOMapFeatureManager features;

        public EcsPackedEntity[] regionPEs;
    }
}