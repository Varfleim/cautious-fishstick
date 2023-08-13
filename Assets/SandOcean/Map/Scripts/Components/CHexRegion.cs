
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean.Map
{
    public struct CHexRegion
    {
        public CHexRegion(
            EcsPackedEntity selfPE, int index,
            DHexaspherePoint centerPoint)
        {
            this.selfPE = selfPE;
            this.Index = index;

            this.centerPoint = centerPoint;
            center = this.centerPoint.ProjectedVector3;
            this.centerPoint.regionPE = this.selfPE;

            int facesCount = centerPoint.GetOrderedTriangles(tempTriangles);
            vertexPoints = new DHexaspherePoint[facesCount];
            //Для каждой вершины
            for (int a = 0; a < facesCount; a++)
            {
                vertexPoints[a] = tempTriangles[a].GetCentroid();
            }
            //Переупорядочиваем, если неверные порядок
            if (facesCount == 6)
            {
                Vector3 p0 = (Vector3)vertexPoints[0];
                Vector3 p1 = (Vector3)vertexPoints[1];
                Vector3 p5 = (Vector3)vertexPoints[5];
                Vector3 v0 = p1 - p0;
                Vector3 v1 = p5 - p0;
                Vector3 cp = Vector3.Cross(v0, v1);
                float dp = Vector3.Dot(cp, p1);
                if (dp < 0)
                {
                    DHexaspherePoint aux;
                    aux = vertexPoints[0];
                    vertexPoints[0] = vertexPoints[5];
                    vertexPoints[5] = aux;
                    aux = vertexPoints[1];
                    vertexPoints[1] = vertexPoints[4];
                    vertexPoints[4] = aux;
                    aux = vertexPoints[2];
                    vertexPoints[2] = vertexPoints[3];
                    vertexPoints[3] = aux;
                }
            }
            else if (facesCount == 5)
            {
                Vector3 p0 = (Vector3)vertexPoints[0];
                Vector3 p1 = (Vector3)vertexPoints[1];
                Vector3 p4 = (Vector3)vertexPoints[4];
                Vector3 v0 = p1 - p0;
                Vector3 v1 = p4 - p0;
                Vector3 cp = Vector3.Cross(v0, v1);
                float dp = Vector3.Dot(cp, p1);
                if (dp < 0)
                {
                    DHexaspherePoint aux;
                    aux = vertexPoints[0];
                    vertexPoints[0] = vertexPoints[4];
                    vertexPoints[4] = aux;
                    aux = vertexPoints[1];
                    vertexPoints[1] = vertexPoints[3];
                    vertexPoints[3] = aux;
                }
            }

            vertices = new Vector3[vertexPoints.Length];
            //Для каждой вершины
            for (int a = 0; a < vertexPoints.Length; a++)
            {
                vertices[a] = vertexPoints[a].ProjectedVector3;
            }

            renderer = null;

            customMaterial = null;
            tempMaterial = null;

            extrudeAmount = 0f;

            uvShadedChunkIndex = 0;
            uvShadedChunkStart = 0;
            uvShadedChunkLength = 0;

            crossCost = 1;

            elevation = 0;
            waterLevel = 0;
            terrainTypeIndex = 0;
            neighbourRegionPEs = new EcsPackedEntity[6];
            mapDistance = 0;
            shipGroups = new LinkedList<EcsPackedEntity>();
            ownershipChangeShipGroups = new List<EcsPackedEntity>();

        }

        static readonly DHexasphereTriangle[] tempTriangles = new DHexasphereTriangle[20];
        public static readonly List<EcsPackedEntity> tempNeighbours = new List<EcsPackedEntity>(6);

        public readonly EcsPackedEntity selfPE;

        public readonly int Index
        {
            get;
        }


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

                    /*ShaderData.ViewElevationChanged(ref this);

                    position.y
                        = value * MapGenerationData.elevationStep;
                    position.y
                        += (MapGenerationData.SampleNoise(position).y * 2f - 1f)
                        * MapGenerationData.elevationPerturbStrength;*/
                }
            }
        }
        int elevation;
        public int ViewElevation 
        {
            get
            {
                return elevation >= waterLevel ? elevation : waterLevel;
            }
        }

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

                //ShaderData.ViewElevationChanged(ref this);
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

                    //ShaderData.RefreshTerrain(ref this);
                }
            }
        }
        int terrainTypeIndex;

        public EcsPackedEntity[] neighbourRegionPEs;

        public DHexaspherePoint centerPoint;
        public Vector3 center;
        public DHexaspherePoint[] vertexPoints;
        public Vector3[] vertices;

        public Renderer renderer;

        public Material customMaterial;
        public Material tempMaterial;

        public float ExtrudeAmount 
        {
            get
            {
                return extrudeAmount;
            }
            set
            {
                extrudeAmount = value;

                //Обновляем рендерер региона
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                Mesh mesh = meshFilter.sharedMesh;

                //Рассчитываем новое положение вершин рендерера
                Vector3[] extrudedVertices = new Vector3[vertices.Length];
                for (int a = 0; a < vertices.Length; a++)
                {
                    extrudedVertices[a] = vertices[a] * (1f + this.extrudeAmount * MapGenerationData.extrudeMultiplier);
                }
                mesh.vertices = extrudedVertices;
                mesh.normals = vertices;
                mesh.RecalculateNormals();

                meshFilter.sharedMesh = null;
                meshFilter.sharedMesh = mesh;
            }
        }
        float extrudeAmount;

        public int uvShadedChunkIndex;
        public int uvShadedChunkStart;
        public int uvShadedChunkLength;

        public float crossCost;

        public int mapDistance;

        public LinkedList<EcsPackedEntity> shipGroups;
        public List<EcsPackedEntity> ownershipChangeShipGroups;

        public Vector3 GetCenter(
            bool worldSpace = true)
        {
            //Определяем центр региона с учётом высоты
            Vector3 position = center * (1.0f + extrudeAmount * MapGenerationData.extrudeMultiplier);

            //Если требуются мировые координаты
            if (worldSpace == true)
            {
                //Конвертируем точку
                position = SceneData.HexasphereGO.transform.TransformPoint(position);
            }

            //Возвращаем точку
            return position;
        }

        public Color GetRegionColor()
        {
            //Если регион имеет собственный материал
            if (customMaterial != null)
            {
                //Возвращаем цвет материала
                return customMaterial.color;
            }

            //Иначе возвращаем стандартный цвет региона
            return MapGenerationData.DefaultShadedColor;
        }
    }
}