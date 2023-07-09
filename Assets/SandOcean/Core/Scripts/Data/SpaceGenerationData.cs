
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

    public class SpaceGenerationData : MonoBehaviour
    {
        //�������
        public float sectorSizeModifier;


        //�����
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

        public const float outerToInner = 0.866025404f;
        public const float innerToOuter = 1f / outerToInner;
        public const float outerRadius = 10f;
        public const float innerRadius = outerRadius * outerToInner;

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
            //���� ������ �����
            if (elevation1
                == elevation2)
            {
                //�� ��� ����� - ���������
                return HexEdgeType.Flat;
            }

            //���������� ������� �����
            int delta
                = elevation2 - elevation1;

            //���� ������� ����� �������
            if (delta == 1
                || delta == -1)
            {
                //�� ��� ����� - ������
                return HexEdgeType.Slope;
            }
            //�����
            else
            {
                //��� ����� - �����
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
            return noiseSource.GetPixelBilinear(
                position.x * noiseScale,
                position.z * noiseScale);
        }
        public static Vector3 Perturb(
            Vector3 position)
        {
            //���� ���
            Vector4 sample = SampleNoise(position);

            //������� ������� �� �����������
            position.x
                += (sample.x * 2f - 1f)
                * cellPerturbStrength;
            position.z
                += (sample.z * 2f - 1f)
                * cellPerturbStrength;

            //���������� ��������� �������
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
            //���������� �������� 
            Vector3 offset;
            offset.x = far.x - near.x;
            offset.y = 0f;
            offset.z = far.z - near.z;

            //���������� ��������
            return offset.normalized * (wallThickness * 0.5f);
        }
        public static Vector3 WallLerp(
            Vector3 near, Vector3 far)
        {
            //���������� �������� �������
            near.x += (far.x - near.x) * 0.5f;
            near.z += (far.z - near.z) * 0.5f;

            float v = near.y < far.y ? wallElevationOffset : 1f - wallElevationOffset;

            near.y += (far.y - near.y) * v + wallYOffset;

            //���������� ��������� �������
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

        public EcsPackedEntity[] chunkPEs;
        public EcsPackedEntity[] regionPEs;
        public EcsPackedEntity GetRegionPEFromPosition(
            Vector3 position)
        {
            //��������� ���������� ������
            DHexCoordinates coordinates = DHexCoordinates.FromPosition(position);

            //��������� ������ ������� 
            int index = coordinates.X + coordinates.Z * regionCountX + coordinates.Z / 2;

            //���������� PE �������
            return regionPEs[index];
        }

        public static Color cellColor1 = new Color(1f, 0f, 0f);
        public static Color cellColor2 = new Color(0f, 1f, 0f);
        public static Color cellColor3 = new Color(0f, 0f, 1f);

        public Material terrainMaterial;
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