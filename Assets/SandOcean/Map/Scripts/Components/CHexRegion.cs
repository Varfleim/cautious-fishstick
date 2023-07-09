
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean.Map
{
    public struct CHexRegion
    {
        public CHexRegion(
            EcsPackedEntity selfPE, 
            Vector3 position, DHexCoordinates coordinates,
            Transform transform, TMPro.TextMeshProUGUI uiTransform, UnityEngine.UI.Image highlight,
            EcsPackedEntity parentChunkPE)
        {
            this.selfPE = selfPE;

            this.position = position;
            this.coordinates = coordinates;

            this.transform = transform;
            this.uiRect = uiTransform;
            this.highlight = highlight;

            this.parentChunkPE = parentChunkPE;

            elevation = 0;

            waterLevel = 0;

            terrainTypeIndex = 3;

            urbanLevel = 0;
            farmLevel = 0;
            plantLevel = 0;
            specialIndex = 0;
            walled = false;

            hasIncomingRiver = false;
            hasOutgoingRiver = false;
            incomingRiver = HexDirection.NE;
            outgoingRiver = HexDirection.NE;

            roads = new bool[6];

            neighbourRegionPEs = new EcsPackedEntity[6];

            distance = 0;
            PathFromPE = new();
            SearchHeuristic = 0;
            SearchPhase = 0;

            mapDistance = 0;

            islandPEs = new EcsPackedEntity[0];

            shipGroups = new LinkedList<EcsPackedEntity>();
            ownershipChangeShipGroups = new List<EcsPackedEntity>();

            Elevation = elevation;
            WaterLevel = waterLevel;
            TerrainTypeIndex = terrainTypeIndex;

            DisableHighlight();
        }

        public readonly EcsPackedEntity selfPE;

        public Vector3 Position
        {
            get
            {
                return position;
            }
        }
        Vector3 position;

        public DHexCoordinates coordinates;


        public readonly EcsPackedEntity parentChunkPE;


        public Transform transform;
        public TMPro.TextMeshProUGUI uiRect;
        public UnityEngine.UI.Image highlight;


        public int Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                if (elevation != value)
                {
                    elevation = value;

                    position.y
                        = value * SpaceGenerationData.elevationStep;
                    position.y
                        += (SpaceGenerationData.SampleNoise(position).y * 2f - 1f)
                        * SpaceGenerationData.elevationPerturbStrength;
                }
            }
        }
        int elevation;

        public int WaterLevel
        {
            get
            {
                return waterLevel;
            }
            set
            {
                if (waterLevel == value)
                {
                    return;
                }
                waterLevel = value;
            }
        }
        int waterLevel;
        public bool IsUnderwater
        {
            get
            {
                return waterLevel > elevation;
            }
        }

        public float StreamBedY
        {
            get
            {
                return (elevation + SpaceGenerationData.streamBedElevationOffset)
                    * SpaceGenerationData.elevationStep;
            }
        }
        public float RiverSurfaceY
        {
            get
            {
                return (elevation + SpaceGenerationData.waterElevationOffset)
                    * SpaceGenerationData.elevationStep;
            }
        }
        public float WaterSurfaceY
        {
            get
            {
                return (waterLevel + SpaceGenerationData.waterElevationOffset)
                    * SpaceGenerationData.elevationStep;
            }
        }

        public int TerrainTypeIndex
        {
            get
            {
                return terrainTypeIndex;
            }
            set
            {
                if (terrainTypeIndex != value)
                {
                    terrainTypeIndex = value;
                }
            }
        }
        int terrainTypeIndex;


        public int UrbanLevel
        {
            get
            {
                return urbanLevel;
            }
            set
            {
                if (urbanLevel != value)
                {
                    urbanLevel = value;
                }
            }
        }
        int urbanLevel;

        public int FarmLevel
        {
            get
            {
                return farmLevel;
            }
            set
            {
                if (farmLevel != value)
                {
                    farmLevel = value;
                }
            }
        }
        int farmLevel;

        public int PlantLevel
        {
            get
            {
                return plantLevel;
            }
            set
            {
                if (plantLevel != value)
                {
                    plantLevel = value;
                }
            }
        }
        int plantLevel;

        public int SpecialIndex
        {
            get
            {
                return specialIndex;
            }
            set
            {
                if (specialIndex != value
                    && HasRiver == false)
                {
                    specialIndex = value;
                }
            }
        }
        int specialIndex;
        public bool IsSpecial
        {
            get
            {
                return specialIndex > 0;
            }
        }

        public bool Walled
        {
            get
            {
                return walled;
            }
            set
            {
                if (walled != value)
                {
                    walled = value;
                }
            }
        }
        bool walled;


        public bool HasIncomingRiver
        {
            set
            {
                hasIncomingRiver = value;
            }
            get
            {
                return hasIncomingRiver;
            }
        }
        bool hasIncomingRiver;
        public bool HasOutgoingRiver
        {
            set
            {
                hasOutgoingRiver = value;
            }
            get
            {
                return hasOutgoingRiver;
            }
        }
        bool hasOutgoingRiver;
        public bool HasRiver
        {
            get
            {
                return hasIncomingRiver || hasOutgoingRiver;
            }
        }
        public bool HasRiverBeginOrEnd
        {
            get
            {
                return hasIncomingRiver != hasOutgoingRiver;
            }
        }
        public HexDirection IncomingRiver
        {
            set
            {
                incomingRiver = value;
            }
            get
            {
                return incomingRiver;
            }
        }
        HexDirection incomingRiver;
        public HexDirection OutgoingRiver
        {
            set
            {
                outgoingRiver = value;
            }
            get
            {
                return outgoingRiver;
            }
        }
        HexDirection outgoingRiver;
        public HexDirection RiverBeginOrEndDirection
        {
            get
            {
                return hasIncomingRiver ? incomingRiver : outgoingRiver;
            }
        }
        public bool HasRiverThroughEdge(
            HexDirection direction)
        {
            return (hasIncomingRiver && incomingRiver == direction)
                || (hasOutgoingRiver && outgoingRiver == direction);
        }
        public bool IsValidRiverDestination(
            ref CHexRegion neighbourCell)
        {
            return elevation >= neighbourCell.elevation || waterLevel == neighbourCell.elevation;
        }


        public bool[] roads;
        public bool HasRoads
        {
            get
            {
                for (int a = 0; a < roads.Length; a++)
                {
                    if (roads[a] == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        public bool HasRoadThroughEdge(
            HexDirection direction)
        {
            return roads[(int)direction];
        }

        public EcsPackedEntity[] neighbourRegionPEs;
        public EcsPackedEntity GetNeighbour(
            HexDirection direction)
        {
            return neighbourRegionPEs[(int)direction];
        }

        public int Distance
        {
            get
            {
                return distance;
            }
            set
            {
                distance = value;
            }
        }
        int distance;

        public void SetLabel(
            string text)
        {
            uiRect.text = text;
        }

        public void DisableHighlight()
        {
            highlight.enabled = false;
        }
        public void EnableHighlight(
            Color color)
        {
            highlight.enabled = true;
            highlight.color = color;
        }

        public EcsPackedEntity PathFromPE
        {
            get; set;
        }
        public int SearchHeuristic 
        { 
            get; set; 
        }
        public int SearchPriority
        {
            get
            {
                return distance + SearchHeuristic;
            }
        }
        public byte SearchPhase
        {
            get;
            set;
        }

        public int mapDistance;

        public EcsPackedEntity[] islandPEs;

        public LinkedList<EcsPackedEntity> shipGroups;
        public List<EcsPackedEntity> ownershipChangeShipGroups;
    }
}