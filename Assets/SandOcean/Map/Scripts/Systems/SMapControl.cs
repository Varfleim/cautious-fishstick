
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Map.Events;
using SandOcean.Diplomacy;
using SandOcean.AEO.RAEO;

namespace SandOcean.Map
{
    public class SMapControl : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //�����
        //readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //�����������
        //readonly EcsPoolInject<COrganization> organizationPool = default;

        //���������������-������������� �������
        //readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        //readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        //readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        //������� �����
        readonly EcsFilterInject<Inc<RChangeMapMode>> changeMapModeRequestFilter = default;
        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //������
        readonly EcsCustomInject<SceneData> sceneData = default;
        readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;
        readonly EcsCustomInject<InputData> inputData;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ����� ������ �����
            foreach (int changeMapModeREntity in changeMapModeRequestFilter.Value)
            {
                //���� ��������� �������
                ref RChangeMapMode changeMapModeR = ref changeMapModeRequestPool.Value.Get(changeMapModeREntity);

                //���� ��������� ���������� ����� ����������
                if (changeMapModeR.mapMode == UI.MapMode.Distance)
                {
                    //������������ ����������
                    //MapModeDistanceCalculate();

                    //���������, ��� �������� ����� ����� - ����� ����������
                    inputData.Value.mapMode = UI.MapMode.Distance;
                }

                //������� �������� �������
                world.Value.DelEntity(changeMapModeREntity);
            }

            //���� ��������� ���������� ����������
            if (mapGenerationData.Value.isMaterialUpdated == true)
            {
                //��������� �������� ����������
                MapMaterialPropertiesUpdate(mapGenerationData.Value.isInitializationUpdate);

                mapGenerationData.Value.isMaterialUpdated = false;
                mapGenerationData.Value.isRegionUpdated = false;
                mapGenerationData.Value.isInitializationUpdate = false;
            }

            //���� ��������� ���������� ������� �������
            if (mapGenerationData.Value.isTextureArrayUpdated == true)
            {
                //��������� ���������
                MapUpdateShadedMaterials(mapGenerationData.Value.isInitializationUpdate);

                mapGenerationData.Value.isInitializationUpdate = false;
                mapGenerationData.Value.isTextureArrayUpdated = false;
            }
            //�����, ���� ��������� ���������� UV-��������� ��� ������
            else if(mapGenerationData.Value.isUVUpdatedFast == true
                || mapGenerationData.Value.isColorUpdated == true)
            {
                //��������� ��������� ���������
                MapUpdateShadedMaterialsFast();

                mapGenerationData.Value.isColorUpdated = false;
                mapGenerationData.Value.isUVUpdatedFast = false;
            }
        }

        /*void MapModeDistanceCalculate()
        {
            //���� ��������� ����������� ������
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //���� ������� �� ����� ����������
            if (inputData.Value.mapMode != UI.MapMode.Distance)
            {
                //��� ������� �������
                foreach (int regionEntity in regionFilter.Value)
                {
                    //���� ��������� �������
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    //�������� ���������� �� �������
                    region.mapDistance = 0;
                }
            }

            //��� ������� ORAEO �����������
            for (int a = 0; a < organization.ownedORAEOPEs.Count; a++)
            {
                //���� ��������� ORAEO
                organization.ownedORAEOPEs[a].Unpack(world.Value, out int oRAEOEntity);
                ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
                ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

                //���� ������������ ������ � RAEO
                exORAEO.parentRAEOPE.Unpack(world.Value, out int rAEOEntity);
                ref CHexRegion region = ref regionPool.Value.Get(rAEOEntity); 
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);


                //���������� ������ 
                int radius = 2;

                //���� ���������� ������
                int centerX = region.coordinates.X;
                int centerZ = region.coordinates.Z;

                //���� ��� ������� ����� ����������
                if (inputData.Value.mapMode == UI.MapMode.Distance)
                {
                    //���� ���������� �� ������� �� ����� ������
                    if (region.mapDistance != 2)
                    {
                        //�������� ���������� �� �������
                        region.mapDistance = 2;

                        //����������� ������������ �����
                        ChunkRefreshSelfRequest(ref region);
                    }

                    for (int r = 0, z = centerZ - radius; z <= centerZ; z++, r++)
                    {
                        for (int x = centerX - r; x <= centerX + radius; x++)
                        {
                            //���� ���������� ������ � ������ ������������
                            if (mapGenerationData.Value.RegionGet(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
                            {
                                //���� ��������� �������
                                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(cellEntity);

                                //���� ���������� �� ������� �� ����� ������
                                if (neighbourRegion.mapDistance == 0)
                                {
                                    //��������� ���������� �� �������
                                    neighbourRegion.mapDistance = 1;

                                    //����������� ������������ �����
                                    ChunkRefreshSelfRequest(ref neighbourRegion);
                                }
                            }
                        }
                    }

                    for (int r = 0, z = centerZ + radius; z > centerZ; z--, r++)
                    {
                        for (int x = centerX - radius; x <= centerX + r; x++)
                        {
                            //���� ���������� ������ � ������ ������������
                            if (mapGenerationData.Value.RegionGet(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
                            {
                                //���� ��������� �������
                                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(cellEntity);

                                //���� ���������� �� ������� �� ����� ������
                                if (neighbourRegion.mapDistance == 0)
                                {
                                    //��������� ���������� �� �������
                                    neighbourRegion.mapDistance = 1;

                                    //����������� ������������ �����
                                    ChunkRefreshSelfRequest(ref neighbourRegion);
                                }
                            }
                        }
                    }
                }
                //�����
                else
                {
                    //�������� ���������� �� �������
                    region.mapDistance = 2;

                    for (int r = 0, z = centerZ - radius; z <= centerZ; z++, r++)
                    {
                        for (int x = centerX - r; x <= centerX + radius; x++)
                        {
                            //���� ���������� ������ � ������ ������������
                            if (mapGenerationData.Value.RegionGet(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
                            {
                                //���� ��������� �������
                                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(cellEntity);

                                //��������� ���������� �� �������
                                neighbourRegion.mapDistance = 1;
                            }
                        }
                    }

                    for (int r = 0, z = centerZ + radius; z > centerZ; z--, r++)
                    {
                        for (int x = centerX - radius; x <= centerX + r; x++)
                        {
                            //���� ���������� ������ � ������ ������������
                            if (mapGenerationData.Value.RegionGet(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
                            {
                                //���� ��������� �������
                                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(cellEntity);

                                //��������� ���������� �� �������
                                neighbourRegion.mapDistance = 1;
                            }
                        }
                    }
                }
            }
        }*/

        void MapMaterialPropertiesUpdate(
            bool isInitializationUpdate = false)
        {
            //���� ����� �������� �������
            if (mapGenerationData.Value.isRegionUpdated == true)
            {
                //��������� �������
                MapRebuildTiles();

                //��������, ��� ������� ���� ���������
                mapGenerationData.Value.isRegionUpdated = false;
            }

            //��������� ��������� �������� � ����
            MapUpdateShadedMaterials(isInitializationUpdate);
            MapUpdateMeshRenderersShadowSupport();

            //��������� �������� ��������
            mapGenerationData.Value.regionMaterial.SetFloat("_GradientIntensity", 1f - mapGenerationData.Value.gradientIntensity);
            mapGenerationData.Value.regionMaterial.SetFloat("_ExtrusionMultiplier", MapGenerationData.extrudeMultiplier);
            mapGenerationData.Value.regionMaterial.SetColor("_Color", mapGenerationData.Value.tileTintColor);
            mapGenerationData.Value.regionMaterial.SetColor("_AmbientColor", mapGenerationData.Value.ambientColor);
            mapGenerationData.Value.regionMaterial.SetFloat("_MinimumLight", mapGenerationData.Value.minimumLight);

            //��������� ������ ����������
            sceneData.Value.hexasphereCollider.radius = 0.5f * (1.0f + MapGenerationData.extrudeMultiplier);

            //��������� ����
            MapUpdateLightingMode();

            //��������� ����
            MapUpdateBevel();
        }

        void MapRebuildTiles()
        {
            //������� ��������
            GameObject placeholder = GameObject.Find(MapGenerationData.shaderframeName);
            if (placeholder != null)
            {
                GameObject.DestroyImmediate(placeholder);
            }

            //������ ��� ������
            MapBuildTiles();
        }

        void MapBuildTiles()
        {
            //���� ������ ������� �����
            int chunkIndex = 0;

            //������ ������ ��� ������, �������� � UV-���������
            List<Vector3> vertexChunk = MapGenerationData.CheckList(ref MapGenerationData.verticesShaded[chunkIndex]);
            List<int> indicesChunk = MapGenerationData.CheckList(ref MapGenerationData.indicesShaded[chunkIndex]);
            List<Vector4> uv2Chunk = MapGenerationData.CheckList(ref MapGenerationData.uv2Shaded[chunkIndex]);

            //������ ������� ������
            int verticesCount = 0;

            //���������� ���������� ��������
            int tileCount = RegionsData.regionPEs.Length;

            //������ ������� �������� ������ � ����������
            int[] hexIndices = MapGenerationData.hexagonIndicesExtruded;
            int[] pentIndices = MapGenerationData.pentagonIndicesExtruded;

            //��� ������� �������
            for (int k = 0; k < tileCount; k++)
            {
                //���� ��������� �������
                RegionsData.regionPEs[k].Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //���� ���������� ������ ������ ������������� ���������� ������ �� ����
                if (verticesCount > MapGenerationData.maxVertexCountPerChunk)
                {
                    //����������� ������ �����
                    chunkIndex++;

                    //��������� ������ ������, �������� � UV-���������
                    vertexChunk = MapGenerationData.CheckList(ref MapGenerationData.verticesShaded[chunkIndex]);
                    indicesChunk = MapGenerationData.CheckList(ref MapGenerationData.indicesShaded[chunkIndex]);
                    uv2Chunk = MapGenerationData.CheckList(ref MapGenerationData.uv2Shaded[chunkIndex]);

                    //�������� ������� ������
                    verticesCount = 0;
                }

                //���� ������ ������ �������
                DHexaspherePoint[] tileVertices = region.vertexPoints;

                //���������� ���������� ������
                int tileVerticesCount = tileVertices.Length;

                //������ ��������� ��� UV4-���������
                Vector4 gpos = Vector4.zero;

                //��� ������ �������
                for (int b = 0; b < tileVerticesCount; b++)
                {
                    //���� ������� �������
                    DHexaspherePoint point = tileVertices[b];

                    //���� ���������� ������ ������� � ������� � � ������ ������
                    Vector3 vertex = point.ProjectedVector3;
                    vertexChunk.Add(vertex);

                    //��������� gpos
                    gpos.x += vertex.x;
                    gpos.y += vertex.y;
                    gpos.z += vertex.z;
                }

                //������������ gpos
                gpos.x /= tileVerticesCount;
                gpos.y /= tileVerticesCount;
                gpos.z /= tileVerticesCount;

                //������ ������ ��������
                int[] indicesArray;

                //���� ����� ������ ������� ����� �����
                if (tileVerticesCount == 6)
                {
                    //������� ������� ��������� ����� � ������
                    vertexChunk.Add((tileVertices[1].ProjectedVector3 + tileVertices[5].ProjectedVector3) * 0.5f);
                    vertexChunk.Add((tileVertices[2].ProjectedVector3 + tileVertices[4].ProjectedVector3) * 0.5f);

                    //��������� ������� ������
                    tileVerticesCount += 2;

                    //��������� ������� 
                    indicesArray = hexIndices;
                }
                //����� ��� ��������
                else
                {
                    //������� ������� ��������� ��������� � ������
                    vertexChunk.Add((tileVertices[1].ProjectedVector3 + tileVertices[4].ProjectedVector3) * 0.5f);
                    vertexChunk.Add((tileVertices[2].ProjectedVector3 + tileVertices[4].ProjectedVector3) * 0.5f);

                    //��������� ������� ������
                    tileVerticesCount += 2;

                    //�������� bevel ��� ����������
                    gpos.w = 1.0f;

                    //��������� ������� 
                    indicesArray = pentIndices;
                }

                //��� ������ �������
                for (int b = 0; b < tileVerticesCount; b++)
                {
                    //������� gpos � ������ UV-���������
                    uv2Chunk.Add(gpos);
                }

                //��� ������� �������
                for (int b = 0; b < indicesArray.Length; b++)
                {
                    //������� ������ ������� � ������ ��������
                    indicesChunk.Add(verticesCount + indicesArray[b]);
                }

                //��������� ������� ������
                verticesCount += tileVerticesCount;
            }

            //������ ������������ GO
            GameObject partsRoot = MapCreateGOAndParent(
                sceneData.Value.coreObject,
                MapGenerationData.shaderframeName);

            //��� ������� �����
            for (int k = 0; k <= chunkIndex; k++)
            {
                //������ ������ ������
                GameObject go = MapCreateGOAndParent(
                    partsRoot.transform,
                    MapGenerationData.shaderframeGOName);

                //��������� ��� ��������� ���������� � ��������� ������
                MeshFilter mf = go.AddComponent<MeshFilter>();
                MapGenerationData.shadedMFs[k] = mf;
                if (MapGenerationData.shadedMeshes[k] == null)
                {
                    MapGenerationData.shadedMeshes[k] = new Mesh();
                    MapGenerationData.shadedMeshes[k].hideFlags = HideFlags.DontSave;
                }
                MapGenerationData.shadedMeshes[k].Clear();
                MapGenerationData.shadedMeshes[k].SetVertices(MapGenerationData.verticesShaded[k]);
                MapGenerationData.shadedMeshes[k].SetTriangles(MapGenerationData.indicesShaded[k], 0);
                MapGenerationData.shadedMeshes[k].SetUVs(1, MapGenerationData.uv2Shaded[k]);
                mf.sharedMesh = MapGenerationData.shadedMeshes[k];

                //��������� ��� ��������� ������������ � ��������� ������
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                MapGenerationData.shadedMRs[k] = mr;


                mr.sharedMaterial = mapGenerationData.Value.regionMaterial;
            }
        }

        void MapUpdateShadedMaterials(
            bool isInitializationUpdate = false)
        {
            //���� ������ ������� �����
            int chunkIndex = 0;

            //������ ������ ������ � ��������
            List<Vector4> uvChunk = MapGenerationData.CheckList(ref MapGenerationData.uvShaded[chunkIndex]);
            List<Color32> colorChunk = MapGenerationData.CheckList(ref MapGenerationData.colorShaded[chunkIndex]);

            //���� ����� �������� �� ����������, ������ �
            if (mapGenerationData.Value.whiteTex == null)
            {
                mapGenerationData.Value.whiteTex = MapGetCachedSolidTexture(Color.white);
            }

            //������� ������ ������� � ������� � ���� ����� ��������
            MapGenerationData.texArray.Clear();
            MapGenerationData.texArray.Add(mapGenerationData.Value.whiteTex);

            //������ ������� ������
            int verticesCount = 0;

            //���������� ���������� ��������
            int tileCount = RegionsData.regionPEs.Length;

            //���������� ����������� ���� �������
            Color32 color = MapGenerationData.DefaultShadedColor;

            //��� ������� �������
            for (int k = 0; k < tileCount; k++)
            {
                //���� ��������� �������
                RegionsData.regionPEs[k].Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //���� ���������� ������ ������ ������������� ���������� ������ �� ����
                if (verticesCount > MapGenerationData.maxVertexCountPerChunk)
                {
                    //����������� ������ �����
                    chunkIndex++;

                    //��������� ������ ������ � ��������
                    uvChunk = MapGenerationData.CheckList(ref MapGenerationData.uvShaded[chunkIndex]);
                    colorChunk = MapGenerationData.CheckList(ref MapGenerationData.colorShaded[chunkIndex]);

                    //�������� ������� ������
                    verticesCount = 0;
                }

                //���� ������ ������ �������
                DHexaspherePoint[] tileVertices = region.vertexPoints;

                //���������� ���������� ������
                int tileVerticesCount = tileVertices.Length;

                //������ ������ UV-���������
                Vector2[] uvArray;

                //���� ���������� ������ ����� �����
                if (tileVerticesCount == 6)
                {
                    //��������� �������
                    uvArray = MapGenerationData.hexagonUVsExtruded;
                }
                //����� ��� ��������
                else
                {
                    //��������� �������
                    uvArray = MapGenerationData.pentagonUVsExtruded;
                }

                //�������� ���� ������� ��� �������� � ������ �������
                Texture2D tileTexture;

                //���������� ������, ������� � �������� ��������
                int textureIndex = 0;
                Vector2 textureScale;
                Vector2 textureOffset;

                //���� ������ ����� ����������� ��������, ���� �������� ����� �������� � �������� �� �����
                if (region.customMaterial && region.customMaterial.HasProperty(ShaderParameters.MainTex) && region.customMaterial.mainTexture != null)
                {
                    //���� ��������� ���� ��������
                    tileTexture = (Texture2D)region.customMaterial.mainTexture;
                    textureIndex = MapGenerationData.texArray.IndexOf(tileTexture);
                    textureScale = region.customMaterial.mainTextureScale;
                    textureOffset = region.customMaterial.mainTextureOffset;
                }
                //�����
                else
                {
                    //���� ��������� ����������� ��������
                    tileTexture = mapGenerationData.Value.whiteTex;
                    textureScale = Vector2.one;
                    textureOffset = Vector2.zero;
                }

                //���� ������ �������� ������ ����
                if (textureIndex < 0)
                {
                    //������� �������� � ������ � ��������� ������
                    MapGenerationData.texArray.Add(tileTexture);
                    textureIndex = MapGenerationData.texArray.Count - 1;
                }

                //���� ��������� ���������� ������
                if (mapGenerationData.Value.isColorUpdated == true)
                {
                    //���� ������ ����� ����������� ��������, �� ���� ��� ����
                    if (region.customMaterial != null)
                    {
                        color = region.customMaterial.color;
                    }
                    //����� ���� ����������� ����
                    else
                    {
                        color = MapGenerationData.DefaultShadedColor;
                    }
                }

                //���� ��� ���������� �� ����� �������������
                if (isInitializationUpdate == true)
                {
                    //��������� ������� UV-��������� �������
                    region.uvShadedChunkStart = verticesCount;
                    region.uvShadedChunkIndex = chunkIndex;
                    region.uvShadedChunkLength = uvArray.Length;
                }

                //��� ������ UV-��������� � ������
                for (int b = 0; b < uvArray.Length; b++)
                {
                    //�������� UV4-����������
                    Vector4 uv4;
                    uv4.x = uvArray[b].x * textureScale.x + textureOffset.x;
                    uv4.y = uvArray[b].y * textureScale.y + textureOffset.y;
                    uv4.z = textureIndex;
                    uv4.w = region.ExtrudeAmount;

                    //���� ��� �� ���������� �� ����� �������������
                    if (isInitializationUpdate == false)
                    {
                        //��������� ���������� � ������
                        uvChunk[verticesCount] = uv4;

                        //���� ��������� ���������� ������
                        if (mapGenerationData.Value.isColorUpdated == true)
                        {
                            //��������� ���� � ������
                            colorChunk[verticesCount] = color;
                        }
                    }
                    //�����
                    else
                    {
                        //������� ���������� � ���� � ������
                        uvChunk.Add(uv4);
                        colorChunk.Add(color);
                    }
                }

                //��������� ���������� ������
                verticesCount += uvArray.Length;
            }

            //��� ������� �����
            for (int k = 0; k <= chunkIndex; k++)
            {
                //��������� ������ UV-���������
                MapGenerationData.uvShadedDirty[k] = false;
                MapGenerationData.shadedMeshes[k].SetUVs(0, MapGenerationData.uvShaded[k]);

                //���� ��������� ���������� ������
                if (mapGenerationData.Value.isColorUpdated == true)
                {
                    MapGenerationData.colorShadedDirty[k] = false;
                    MapGenerationData.shadedMeshes[k].SetColors(MapGenerationData.colorShaded[k]);
                }

                MapGenerationData.shadedMFs[k].sharedMesh = MapGenerationData.shadedMeshes[k];
                MapGenerationData.shadedMRs[k].sharedMaterial = mapGenerationData.Value.regionMaterial;
            }

            //���� ��������� ���������� ������� �������
            if (mapGenerationData.Value.isTextureArrayUpdated)
            {
                //���������� ���������� ������� � �������
                int textureArrayCount = MapGenerationData.texArray.Count;

                //���������� ������ ��������
                mapGenerationData.Value.currentTextureSize = 256;

                //���� �������� ������ ������� �� ����, ������� ���
                if (mapGenerationData.Value.finalTexArray != null)
                {
                    GameObject.DestroyImmediate(mapGenerationData.Value.finalTexArray);
                }

                //������ ����� ������ �������
                mapGenerationData.Value.finalTexArray = new(
                    mapGenerationData.Value.currentTextureSize, mapGenerationData.Value.currentTextureSize,
                    textureArrayCount,
                    TextureFormat.ARGB32,
                    true);

                //��� ������ ��������
                for (int a = 0; a < textureArrayCount; a++)
                {
                    //���� ������ �������� � ������� �� ������������� ������������
                    if (MapGenerationData.texArray[a].width != mapGenerationData.Value.currentTextureSize
                        || MapGenerationData.texArray[a].height != mapGenerationData.Value.currentTextureSize)
                    {
                        //������ �������� � ��������� � ������
                        MapGenerationData.texArray[a] = GameObject.Instantiate(MapGenerationData.texArray[a]);
                        MapGenerationData.texArray[a].hideFlags = HideFlags.DontSave;
                        TextureScaler.Scale(
                            MapGenerationData.texArray[a],
                            mapGenerationData.Value.currentTextureSize, mapGenerationData.Value.currentTextureSize);
                    }
                    //������� �������� � �������� ������
                    mapGenerationData.Value.finalTexArray.SetPixels32(
                        MapGenerationData.texArray[a].GetPixels32(),
                        a);
                }
                //��������� �������� ������ � ����� ��� ��� ������ ������� ��� ���������
                mapGenerationData.Value.finalTexArray.Apply();
                mapGenerationData.Value.regionMaterial.SetTexture(
                    ShaderParameters.MainTex,
                    mapGenerationData.Value.finalTexArray);

                //��������, ��� ������ ������� ��� �������
                mapGenerationData.Value.isTextureArrayUpdated = false;
            }

            //��������, ��� ����� � UV-���������� ���� ���������
            mapGenerationData.Value.isColorUpdated = false;
            mapGenerationData.Value.isUVUpdatedFast = false;

            //��������� ���������� ������ UV-���������
            mapGenerationData.Value.uvChunkCount = chunkIndex + 1;
        }

        void MapUpdateShadedMaterialsFast()
        {
            //���� ��������� ���������� ������
            if (mapGenerationData.Value.isColorUpdated == true)
            {
                //��� ������� �����
                for (int a = 0; a < mapGenerationData.Value.uvChunkCount; a++)
                {
                    //���� ����� "������"
                    if (MapGenerationData.colorShadedDirty[a] == true)
                    {
                        //��������� ������
                        MapGenerationData.colorShadedDirty[a] = false;
                        MapGenerationData.shadedMeshes[a].SetColors(MapGenerationData.colorShaded[a]);
                    }
                }
            }

            //���� ��������� ���������� UV-���������
            if (mapGenerationData.Value.isUVUpdatedFast == true)
            {
                //��� ������� �����
                for (int a = 0; a < mapGenerationData.Value.uvChunkCount; a++)
                {
                    //���� UV-���������� "������"
                    if (MapGenerationData.uvShadedDirty[a] == true)
                    {
                        //��������� ������
                        MapGenerationData.uvShadedDirty[a] = false;
                        MapGenerationData.shadedMeshes[a].SetUVs(0, MapGenerationData.uvShaded[a]);
                    }
                }
            }
        }

        void MapUpdateLightingMode()
        {
            //��������� ���������
            MapUpdateLightingMaterial(mapGenerationData.Value.regionMaterial);
            MapUpdateLightingMaterial(mapGenerationData.Value.regionColoredMaterial);

            mapGenerationData.Value.regionMaterial.EnableKeyword("HEXA_ALPHA");
        }

        void MapUpdateLightingMaterial(
            Material material)
        {
            //��������� ������ ���������
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            if (material.renderQueue >= 3000)
            {
                material.renderQueue -= 2000;
            }
            material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);

            material.EnableKeyword("HEXA_LIT");
        }

        void MapUpdateBevel()
        {
            //���������� ������ ��������
            const int textureSize = 256;

            //���� �������� ����������� ��� � ������ �������
            if (MapGenerationData.bevelNormals == null || MapGenerationData.bevelNormals.width != textureSize)
            {
                //������ ����� ��������
                MapGenerationData.bevelNormals = new(
                    textureSize, textureSize,
                    TextureFormat.ARGB32,
                    false);
            }

            //���������� ������ ��������
            int textureHeight = MapGenerationData.bevelNormals.height;
            int textureWidth = MapGenerationData.bevelNormals.width;

            //���� ������ ������ �������� ����������� ��� ��� ����� �� ������������� ��������
            if (MapGenerationData.bevelNormalsColors == null || MapGenerationData.bevelNormalsColors.Length != textureHeight * textureWidth)
            {
                //������ ����� ������
                MapGenerationData.bevelNormalsColors = new Color[textureHeight * textureWidth];
            }

            //������ ��������� ��� ��������� �������
            Vector2 texturePixel;

            //���������� ������ ����� � ������� ������
            const float bevelWidth = 0.1f;
            float bevelWidthSqr = bevelWidth * bevelWidth;

            //��� ������� ������� �� ������
            for (int y = 0, index = 0; y < textureHeight; y++)
            {
                //���������� ��� ��������� �� Y
                texturePixel.y = (float)y / textureHeight;

                //��� ������� ������� �� ������
                for (int x = 0; x < textureWidth; x++)
                {
                    //���������� ��� ��������� �� X
                    texturePixel.x = (float)x / textureWidth;

                    //�������� R-���������
                    MapGenerationData.bevelNormalsColors[index].r = 0f;

                    //���������� ���������� �� ������� �������
                    float minDistSqr = float.MaxValue;

                    //��� ������� ����� ��������������
                    for (int a = 0; a < 6; a++)
                    {
                        //���� ������� ������
                        Vector2 t0 = MapGenerationData.hexagonUVsExtruded[a];
                        Vector2 t1 = a < 5 ? MapGenerationData.hexagonUVsExtruded[a + 1] : MapGenerationData.hexagonUVsExtruded[0];

                        //���������� ����� �����
                        float l2 = Vector2.SqrMagnitude(t0 - t1);
                        //
                        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(texturePixel - t0, t1 - t0) / l2));
                        //
                        Vector2 projection = t0 + t * (t1 - t0);
                        //
                        float distSqr = Vector2.SqrMagnitude(texturePixel - projection);

                        //���� ���������� ������ ������������, �� ��������� �����������
                        if (distSqr < minDistSqr)
                        {
                            minDistSqr = distSqr;
                        }
                    }

                    //���������� ��������
                    float f = minDistSqr / bevelWidthSqr;
                    //���� �������� ������ �������, ������������ ���
                    if (f > 1f)
                    {
                        f = 1f;
                    }

                    //��������� R-���������
                    MapGenerationData.bevelNormalsColors[index].r = f;

                    //����������� ������ �������
                    index++;
                }
            }

            //����� ������ �������� ��������
            MapGenerationData.bevelNormals.SetPixels(MapGenerationData.bevelNormalsColors);

            //��������� ��������
            MapGenerationData.bevelNormals.Apply();

            //����� �������� ���������
            mapGenerationData.Value.regionMaterial.SetTexture("_BumpMask", MapGenerationData.bevelNormals);
        }

        void MapUpdateMeshRenderersShadowSupport()
        {
            //��� ������� ������������ ��������
            for (int a = 0; a < MapGenerationData.shadedMRs.Length; a++)
            {
                //���� �������� �� ���� � ��� ��� �����
                if (MapGenerationData.shadedMRs[a] != null
                    && MapGenerationData.shadedMRs[a].name.Equals(MapGenerationData.shaderframeGOName))
                {
                    //����������� ������������ �������� � ������������ �����
                    MapGenerationData.shadedMRs[a].receiveShadows = true;
                    MapGenerationData.shadedMRs[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
            }
        }

        Texture2D MapGetCachedSolidTexture(
            Color color)
        {
            Texture2D texture;

            if (MapGenerationData.solidTexCache.TryGetValue(color, out texture))
            {
                return texture;
            }
            else
            {
                texture = new Texture2D(
                    mapGenerationData.Value.TileTextureSize, mapGenerationData.Value.TileTextureSize,
                    TextureFormat.ARGB32,
                    true);
                texture.hideFlags = HideFlags.DontSave;

                int length = texture.width * texture.height;
                Color32[] colors32 = new Color32[length];
                Color32 color32 = color;

                //��� ������� �������
                for (int a = 0; a < length; a++)
                {
                    colors32[a] = color32;
                }

                texture.SetPixels32(colors32);
                texture.Apply();

                MapGenerationData.solidTexCache[color] = texture;

                return texture;
            }
        }

        GameObject MapCreateGOAndParent(
            Transform parent,
            string name)
        {
            //������ GO
            GameObject gO = new GameObject(name);

            //��������� �������� ������ GO
            gO.layer = parent.gameObject.layer;
            gO.transform.SetParent(parent, false);
            gO.transform.localPosition = Vector3.zero;
            gO.transform.localScale = Vector3.one;
            gO.transform.localRotation = Quaternion.Euler(0, 0, 0);

            return gO;
        }
    }
}