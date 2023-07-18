
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean.Map
{
    public enum HexDirection
    {
        NE,
        E,
        SE,
        SW,
        W,
        NW
    }

    public enum HexEdgeType : byte
    {
        Flat,
        Slope,
        Cliff
    }

    public class MapGenerationData : MonoBehaviour
    {
        //Сектора
        public float sectorSizeModifier;


        //Карта
        public static int ChunkSize
        {
            get
            {
                return chunkSizeX * chunkSizeZ;
            }
        }
        public const int chunkSizeX = 5;
        public const int chunkSizeZ = 5;
        public int chunkCountX;
        public int chunkCountZ;
        public int regionCountX;
        public int regionCountZ;
        public int regionCount;

        public const float outerToInner = 0.866025404f;
        public const float innerToOuter = 1f / outerToInner;
        public const float outerRadius = 10f;
        public const float innerRadius = outerRadius * outerToInner;
        public const float innerDiameter = innerRadius * 2f;

        public const float solidFactor = 0.8f;
        public const float blendFactor = 1f - solidFactor;
        public const float waterFactor = 0.6f;
        public const float waterBlendFactor = 1f - waterFactor;

        public const float elevationStep = 3f;
        public const int terracesPerSlope = 2;
        public const int terraceSteps = terracesPerSlope * 2 + 1;
        public const float horizontalTerraceStepSize = 1f / terraceSteps;
        public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

        public const float streamBedElevationOffset = -1.75f;
        public const float waterElevationOffset = -0.5f;

        static Vector3[] corners =
        {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(0f, 0f, outerRadius)
        };
        public static Vector3 GetFirstCorner(
            HexDirection direction)
        {
            return corners[(int)direction];
        }
        public static Vector3 GetSecondCorner(
            HexDirection direction)
        {
            return corners[(int)direction + 1];
        }
        public static Vector3 GetFirstSolidCorner(
            HexDirection direction)
        {
            return corners[(int)direction] * solidFactor;
        }
        public static Vector3 GetSecondSolidCorner(
            HexDirection direction)
        {
            return corners[(int)direction + 1] * solidFactor;
        }
        public static Vector3 GetSolidEdgeMiddle(
            HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction + 1])
                * (0.5f * solidFactor);
        }
        public static Vector3 GetBridge(
            HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction + 1])
                * blendFactor;
        }
        public static Vector3 GetFirstWaterCorner(
            HexDirection direction)
        {
            return corners[(int)direction] * waterFactor;
        }
        public static Vector3 GetSecondWaterCorner(
            HexDirection direction)
        {
            return corners[(int)direction + 1] * waterFactor;
        }
        public static Vector3 GetWaterBridge(
            HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction + 1])
                * waterBlendFactor;
        }

        public static Vector3 TerraceLerp(
            Vector3 a,
            Vector3 b,
            int step)
        {
            float h
                = step * horizontalTerraceStepSize;
            a.x
                += (b.x - a.x) * h;
            a.z
                += (b.z - a.z) * h;

            float v
                = (step + 1) / 2
                * verticalTerraceStepSize;
            a.y
                += (b.y - a.y)
                * v;

            return a;
        }
        public static Color TerraceLerp(
            Color a,
            Color b,
            int step)
        {
            float h
                = step
                * horizontalTerraceStepSize;

            return Color.Lerp(a, b, h);
        }

        public static HexEdgeType GetEdgeType(
            int elevation1,
            int elevation2)
        {
            //Если высоты равны
            if (elevation1
                == elevation2)
            {
                //То тип ребра - плоскость
                return HexEdgeType.Flat;
            }

            //Определяем разницу высот
            int delta
                = elevation2 - elevation1;

            //Если разница равна единице
            if (delta == 1
                || delta == -1)
            {
                //То тип ребра - наклон
                return HexEdgeType.Slope;
            }
            //Иначе
            else
            {
                //Тип ребра - обрыв
                return HexEdgeType.Cliff;
            }
        }

        public const float cellPerturbStrength = 0f;//4f;
        public const float elevationPerturbStrength = 1.5f;
        public const float noiseScale = 0.003f;

        public Texture2D noiseTexture;
        public static Texture2D noiseSource;

        public static Vector4 SampleNoise(
            Vector3 position)
        {
            Vector4 sample = noiseSource.GetPixelBilinear(
                position.x * noiseScale,
                position.z * noiseScale);

            //Если позиция находится внутри диаметра
            if (position.x < innerDiameter * 1.5f)
            {
                Vector4 sample2 = noiseSource.GetPixelBilinear(
                    (position.x + wrapSize * innerDiameter) * noiseScale,
                    position.z * noiseScale);

                sample = Vector4.Lerp(sample2, sample, position.x * (1f / innerDiameter) - 0.5f);
            }

            return sample;
        }
        public static Vector3 Perturb(
            Vector3 position)
        {
            //Берём шум
            Vector4 sample = SampleNoise(position);

            //Смещаем вершину по горизонтали
            position.x
                += (sample.x * 2f - 1f)
                * cellPerturbStrength;
            position.z
                += (sample.z * 2f - 1f)
                * cellPerturbStrength;

            //Возвращаем смещённую вершину
            return position;
        }

        public const int hashGridSize = 256;
        public const float hashGridScale = 0.25f;
        static DHexHash[] hashGrid;

        public static void InitializeHashGrid()
        {
            hashGrid = new DHexHash[hashGridSize * hashGridSize];

            for (int a = 0; a < hashGrid.Length; a++)
            {
                hashGrid[a] = DHexHash.Create();
            }
        }
        public static DHexHash SampleHashGrid(
            Vector3 postition)
        {
            int x = (int)(postition.x + hashGridScale) % hashGridSize;
            if (x < 0)
            {
                x += hashGridSize;
            }
            int z = (int)(postition.z + hashGridScale) % hashGridSize;
            if (z < 0)
            {
                z += hashGridSize;
            }

            return hashGrid[x + z * hashGridSize];
        }

        static float[][] featureThresholds =
            {
            new float[] {0.0f, 0.0f, 0.4f},
            new float[] {0.0f, 0.4f, 0.6f},
            new float[] {0.4f, 0.6f, 0.8f}
        };
        public static float[] GetFeatureThresholds(int level)
        {
            return featureThresholds[level];
        }

        public const float wallHeight = 4f;
        public const float wallYOffset = -1f;
        public const float wallThickness = 1.25f;
        public const float wallElevationOffset = verticalTerraceStepSize;
        public const float wallTowerTreshhold = 0.5f;

        public static Vector3 WallThicknessOffset(
            Vector3 near, Vector3 far)
        {
            //Определяем смещение 
            Vector3 offset;
            offset.x = far.x - near.x;
            offset.y = 0f;
            offset.z = far.z - near.z;

            //Возвращаем смещение
            return offset.normalized * (wallThickness * 0.5f);
        }
        public static Vector3 WallLerp(
            Vector3 near, Vector3 far)
        {
            //Определяем смещение вершины
            near.x += (far.x - near.x) * 0.5f;
            near.z += (far.z - near.z) * 0.5f;

            float v = near.y < far.y ? wallElevationOffset : 1f - wallElevationOffset;

            near.y += (far.y - near.y) * v + wallYOffset;

            //Возвращаем смещённую вершину
            return near;
        }

        public const float bridgeDesignLength = 7f;

        public static Vector2[] chunkNeighbours =
        {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(1, -1),
            new Vector2(0, -1),
            new Vector2(-1, -1),
            new Vector2(-1, 0),
            new Vector2(-1, 1)
        };

        public Transform[] columns;
        public EcsPackedEntity[] chunkPEs;
        public EcsPackedEntity[] regionPEs;
        public EcsPackedEntity GetRegionPEFromPosition(
            Vector3 position)
        {
            //Вычисляем координаты ячейки
            DHexCoordinates coordinates = DHexCoordinates.FromPosition(position);

            //Вычисляем индекс региона 
            int index = coordinates.X + coordinates.Z * regionCountX + coordinates.Z / 2;

            //Возвращаем PE региона
            return regionPEs[index];
        }

        public static Color weights1 = new Color(1f, 0f, 0f);
        public static Color weights2 = new Color(0f, 1f, 0f);
        public static Color weights3 = new Color(0f, 0f, 1f);

        public Material terrainMaterial;

        public HexRegionShaderData regionShaderData;


        [Range(0f, 0.5f)]
        public float jitterProbability = 0.25f; 
        [Range(20, 200)]
        public int chunkSizeMin = 30;
        [Range(20, 200)]
        public int chunkSizeMax = 100;

        [Range(5, 100)]
        public int landPercentage = 50;
        [Range(1, 5)]
        public int waterLevel = 3;
        [Range(0f, 1f)]
        public float highRiseProbability = 0.25f;
        [Range(0f, 0.4f)]
        public float sinkProbability = 0.2f;

        [Range(-4, 0)]
        public int elevationMinimum = -2;
        [Range(6, 10)]
        public int elevationMaximum = 8;

        [Range(0, 10)]
        public int mapBorderX = 5;
        [Range(0, 10)]
        public int mapBorderZ = 5;

        public DMapArea[] mapAreas; 
        [Range(0, 10)]
        public int areaBorder = 5; 
        [Range(1, 2)]
        public int areaCount = 1; 
        
        [Range(0, 100)]
        public int erosionPercentage = 50;

        [Range(0f, 1f)]
        public float startingMoisture = 0.1f;
        [Range(0f, 1f)]
        public float evaporationFactor = 0.5f; 
        [Range(0f, 1f)]
        public float precipitationFactor = 0.25f;
        [Range(0f, 1f)]
        public float runoffFactor = 0.25f;
        [Range(0f, 1f)]
        public float seepageFactor = 0.125f;
        public HexDirection windDirection = HexDirection.NW;
        [Range(1f, 10f)]
        public float windStrength = 4f;

        [Range(0f, 1f)]
        public float lowTemperature = 0f;
        [Range(0f, 1f)]
        public float highTemperature = 1f; 
        [Range(0f, 1f)]
        public float temperatureJitter = 0.1f;

        public static float[] temperatureBands = { 0.1f, 0.3f, 0.6f };
        public static float[] moistureBands = { 0.12f, 0.28f, 0.85f };
        public static DBiome[] biomes =
        {
        new DBiome(0, 0), new DBiome(4, 0), new DBiome(4, 0), new DBiome(4, 0),
        new DBiome(0, 0), new DBiome(2, 0), new DBiome(2, 1), new DBiome(2, 2),
        new DBiome(0, 0), new DBiome(1, 0), new DBiome(1, 1), new DBiome(1, 2),
        new DBiome(0, 0), new DBiome(1, 1), new DBiome(1, 2), new DBiome(1, 3)
        };

        public static int wrapSize;
    }

    public static class HexDirectionExtensions
    {
        public static HexDirection Opposite(
            this HexDirection direction)
        {
            return (int)direction < 3 ? direction + 3 : direction - 3;
        }

        public static HexDirection Previous(
            this HexDirection direction)
        {
            return direction == HexDirection.NE ? HexDirection.NW : direction - 1;
        }

        public static HexDirection Previous2(
            this HexDirection direction)
        {
            direction -= 2;
            return direction >= HexDirection.NE ? direction : direction + 6;
        }

        public static HexDirection Next(
            this HexDirection direction)
        {
            return direction == HexDirection.NW ? HexDirection.NE : direction + 1;
        }

        public static HexDirection Next2(
            this HexDirection direction)
        {
            direction += 2;
            return direction <= HexDirection.NW ? direction : direction - 6;
        }
    }
}