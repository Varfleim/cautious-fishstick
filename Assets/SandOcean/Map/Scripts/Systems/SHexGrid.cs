
using System;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Diplomacy;
using SandOcean.AEO.RAEO;

namespace SandOcean.Map
{
    public class SHexGrid : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //������� �����
        //readonly EcsPoolInject<CMapObject> mapObjectPool = default;

        //�����
        readonly EcsPoolInject<CHexChunk> chunkPool = default;

        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //����������
        readonly EcsFilterInject<Inc<COrganization>> organizationFilter = default;
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //���������������-������������� �������
        readonly EcsFilterInject<Inc<CRegionAEO>> regionAEOFilter = default;
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;


        //������� �����
        readonly EcsFilterInject<Inc<EMapGeneration>> mapGenerationEventFilter = default;
        readonly EcsPoolInject<EMapGeneration> mapGenerationEventPool = default;

        readonly EcsPoolInject<SRMapChunkRefresh> mapChunkRefreshSRPool = default;

        //������� ���������������-������������� ��������

        readonly EcsFilterInject<Inc<SRORAEOCreate, COrganization>> oRAEOCreateSRFilter = default;

        //������
        readonly EcsCustomInject<StaticData> staticData = default;
        readonly EcsCustomInject<SceneData> sceneData = default;
        readonly EcsCustomInject<SpaceGenerationData> spaceGenerationData = default;
        //readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ��������� �����
            foreach (int mapGenerationEventEntity in mapGenerationEventFilter.Value)
            {
                //���� ��������� ������� ��������� �����
                ref EMapGeneration mapGenerationEvent= ref mapGenerationEventPool.Value.Get(mapGenerationEventEntity);

                //�������������� ���-�������
                SpaceGenerationData.InitializeHashGrid();

                //������ �����
                MapCreate(ref mapGenerationEvent);

                world.Value.DelEntity(mapGenerationEventEntity);
            }

            //���� ������ ������������ �������� ORAEO �� ����
            if (oRAEOCreateSRFilter.Value.GetEntitiesCount() > 0)
            {
                //������ ORAEO
                ORAEOCreating();
            }

            //��� ������� �����
            /*foreach (int chunkEntity in chunkFilter.Value)
            {
                //���� ��������� �����
                ref CHexChunk chunk
                    = ref chunkPool.Value.Get(chunkEntity);

                //���� ���� �������
                if (chunk.isActive == true)
                {
                    //��� ������� ������ ����� �� �����������
                    for (int a = 0; a < SpaceGenerationData.chunkNeighbours.Length; a++)
                    {
                        //���������� ���������� ������
                        Vector2 neighbourCoordinates = new(
                            chunk.coordinateX + SpaceGenerationData.chunkNeighbours[a].x,
                            chunk.coordinateZ + SpaceGenerationData.chunkNeighbours[a].y);

                        //���� ���������� ���� � ������ ������������
                        if (spaceGenerationData.Value.chunks.TryGetValue(neighbourCoordinates, out EcsPackedEntity neighbourPE))
                        {
                            //���� �������� ������
                            neighbourPE.Unpack(world.Value, out int neighbourEntity);

                            //����������� ���������� �����
                            ChunkRefreshEvent(
                                neighbourEntity);
                        }
                        //�����
                        else
                        {
                            //������ ����
                            ChunkCreate(
                                (int)neighbourCoordinates.x, (int)neighbourCoordinates.y);
                        }
                    }

                    //��������, ��� ���� ���������
                    chunk.isActive = false;
                }
            }*/
        }

        void MapCreate(
            ref EMapGeneration mapGenerationEvent)
        {
            //�������������� �����
            MapInitialization(ref mapGenerationEvent);

            //������ ���������, �������� ������ ��� ��������� �����
            //TempSectorGenerationData tempSectorGenerationData = new();

            //������������ ��������� �����
            MapWealthDistribution();

            //���������� ����� 
            MapGeneration(ref mapGenerationEvent);
        }

        void MapInitialization(
            ref EMapGeneration mapGenerationEvent)
        {
            //��������� �������� ����
            SpaceGenerationData.noiseSource
                = spaceGenerationData.Value.noiseTexture;

            //���������� ���������� ������
            spaceGenerationData.Value.chunkCountX
                = mapGenerationEvent.chunkCountX;
            spaceGenerationData.Value.chunkCountZ
                = mapGenerationEvent.chunkCountZ;

            //���������� ������ ������� PE ������
            spaceGenerationData.Value.chunkPEs = new EcsPackedEntity[spaceGenerationData.Value.chunkCountX * spaceGenerationData.Value.chunkCountZ];

            //���������� ���������� ��������
            spaceGenerationData.Value.regionCountX
                = mapGenerationEvent.chunkCountX
                * SpaceGenerationData.chunkSizeX;
            spaceGenerationData.Value.regionCountZ
                = mapGenerationEvent.chunkCountZ
                * SpaceGenerationData.chunkSizeZ;

            //���������� ������ ������� PE ��������
            spaceGenerationData.Value.regionPEs = new EcsPackedEntity[spaceGenerationData.Value.regionCountX * spaceGenerationData.Value.regionCountZ];
        }

        void MapWealthDistribution()
        {

        }

        void MapGeneration(
            ref EMapGeneration mapGenerationEvent)
        {
            //������ �����
            MapCreateChunks(ref mapGenerationEvent);

            //������ �������
            MapCreateRegions(ref mapGenerationEvent);

            //������ �������
            //MapCreateIslands(ref mapGenerationEvent);
        }

        void MapCreateChunks(
            ref EMapGeneration mapGenerationEvent)
        {
            //��� ������� ����� �� ������
            for (int z = 0, i = 0; z < spaceGenerationData.Value.chunkCountZ; z++)
            {
                //��� ������� ����� �� ������
                for (int x = 0; x < spaceGenerationData.Value.chunkCountX; x++)
                {
                    //������ ����
                    ChunkCreate(
                        ref mapGenerationEvent,
                        x, z,
                        i++);
                }
            }

            //������ ��������� �����
            /*ChunkCreate(
                0, 0);*/
        }

        void MapCreateRegions(
            ref EMapGeneration mapGenerationEvent)
        {
            //��� ������� ������� �� ������
            for (int z = 0, i = 0; z < spaceGenerationData.Value.regionCountZ; z++)
            {
                //��� ������� ������� �� ������
                for (int x = 0; x < spaceGenerationData.Value.regionCountX; x++)
                {
                    //������ ������
                    RegionCreate(
                        x, z,
                        i++);
                }
            }
        }

        void ChunkCreate(
            ref EMapGeneration mapGenerationEvent,
            int chunkX, int chunkZ,
            int chunkIndex)
        {
            //������ ����� �������� � ��������� �� ��������� �����
            int chunkEntity = world.Value.NewEntity();
            ref CHexChunk chunk = ref chunkPool.Value.Add(chunkEntity);

            //������ ������ �����
            GOChunk chunkGO = GameObject.Instantiate(staticData.Value.chunkPrefab);

            //��������� �������� ������ �����
            chunk = new(
                world.Value.PackEntity(chunkEntity),
                chunkX, chunkZ,
                chunkGO.transform, chunkGO.chunkCanvas,
                chunkGO.terrain, chunkGO.rivers, chunkGO.roads, chunkGO.water, chunkGO.waterShore, chunkGO.estuaries,
                chunkGO.features,
                SpaceGenerationData.ChunkSize);

            //������ ����
            chunk.terrain.mesh = new();
            chunk.terrain.meshFilter.mesh = chunk.terrain.mesh;
            if (chunk.terrain.useCollider == true)
            {
                chunk.terrain.meshCollider = chunk.terrain.gameObject.AddComponent<MeshCollider>();
            }
            chunk.rivers.mesh = new();
            chunk.rivers.meshFilter.mesh = chunk.rivers.mesh;
            if (chunk.rivers.useCollider == true)
            {
                chunk.rivers.meshCollider = chunk.rivers.gameObject.AddComponent<MeshCollider>();
            }
            chunk.roads.mesh = new();
            chunk.roads.meshFilter.mesh = chunk.roads.mesh;
            if (chunk.roads.useCollider == true)
            {
                chunk.roads.meshCollider = chunk.roads.gameObject.AddComponent<MeshCollider>();
            }
            chunk.water.mesh = new();
            chunk.water.meshFilter.mesh = chunk.water.mesh;
            if (chunk.water.useCollider == true)
            {
                chunk.water.meshCollider = chunk.water.gameObject.AddComponent<MeshCollider>();
            }
            chunk.waterShore.mesh = new();
            chunk.waterShore.meshFilter.mesh = chunk.waterShore.mesh;
            if (chunk.waterShore.useCollider == true)
            {
                chunk.waterShore.meshCollider = chunk.waterShore.gameObject.AddComponent<MeshCollider>();
            }
            chunk.estuaries.mesh = new();
            chunk.estuaries.meshFilter.mesh = chunk.estuaries.mesh;
            if (chunk.estuaries.useCollider == true)
            {
                chunk.estuaries.meshCollider = chunk.estuaries.gameObject.AddComponent<MeshCollider>();
            }
            chunk.features.walls.mesh = new();
            chunk.features.walls.meshFilter.mesh = chunk.features.walls.mesh;
            if (chunk.features.walls.useCollider == true)
            {
                chunk.features.walls.meshCollider = chunk.features.walls.gameObject.AddComponent<MeshCollider>();
            }

            //������������ ���� � ������������ �������
            chunk.transform.SetParent(sceneData.Value.coreObject, false);

            //������� ���� � ������ ������
            spaceGenerationData.Value.chunkPEs[chunkIndex] = chunk.selfPE;

            //����������� ���������� �����
            ChunkRefreshEvent(chunkEntity);
        }

        void ChunkRefreshEvent(
            int chunkEntity)
        {
            //���� ��� �� ���������� ����������� ���������� �����
            if (mapChunkRefreshSRPool.Value.Has(chunkEntity) == false)
            {
                //����������� � �������� ���������� ���������� �����
                mapChunkRefreshSRPool.Value.Add(chunkEntity);
            }
        }

        
        void RegionCreate(
            int regionGlobalX, int regionGlobalZ,
            int regionIndex)
        {
            //���������� ������������ ���� �������
            int parentChunkX = regionGlobalX / SpaceGenerationData.chunkSizeX;
            int parentChunkZ = regionGlobalZ / SpaceGenerationData.chunkSizeZ;
            spaceGenerationData.Value.chunkPEs[parentChunkX + parentChunkZ * spaceGenerationData.Value.chunkCountX].Unpack(
                world.Value, out int parentChunkEntity);
            ref CHexChunk parentChunk = ref chunkPool.Value.Get(parentChunkEntity);

            int regionLocalX = regionGlobalX - parentChunkX * SpaceGenerationData.chunkSizeX;
            int regionLocalZ = regionGlobalZ - parentChunkZ * SpaceGenerationData.chunkSizeZ;

            //������ ����� �������� � ��������� �� ��������� ������� � RAEO
            int regionEntity = world.Value.NewEntity();
            ref CHexRegion currentRegion = ref regionPool.Value.Add(regionEntity);
            ref CRegionAEO currentRAEO = ref regionAEOPool.Value.Add(regionEntity);

            //���������� ������� �������
            Vector3 position = new(
                (regionGlobalX + regionGlobalZ * 0.5f - regionGlobalZ / 2) * (SpaceGenerationData.innerRadius * 2f),
                0f,
                regionGlobalZ * (SpaceGenerationData.outerRadius * 1.5f));
            //���������� ���������� �������
            DHexCoordinates regionCoordinates = DHexCoordinates.FromOffsetCoordinates(regionGlobalX, regionGlobalZ);

            //������ ������ �������
            GORegion regionGO = GameObject.Instantiate(staticData.Value.regionPrefab);

            //��������� �������� ������ �������
            currentRegion = new(
                world.Value.PackEntity(regionEntity),
                position, regionCoordinates,
                regionGO.regionTransform, regionGO.regionLabel, regionGO.regionHighlight,
                parentChunk.selfPE);

            //���������� ������ ������� �� ��������������� �������
            currentRegion.transform.localPosition = currentRegion.Position;

            //���������� ������ ����� �� ��������������� ������� � ���������� ���������� �������
            currentRegion.uiRect.rectTransform.anchoredPosition = new(currentRegion.Position.x, currentRegion.Position.z);

            //������� ������ � ������ �������� ������������� �����
            parentChunk.regionPEs[regionLocalX + regionLocalZ * SpaceGenerationData.chunkSizeX] = currentRegion.selfPE;

            //����������� ������� ������� � ������� �����
            currentRegion.transform.SetParent(parentChunk.transform, false);
            currentRegion.uiRect.rectTransform.SetParent(parentChunk.canvas.transform, false);
            currentRegion.highlight.rectTransform.SetParent(parentChunk.canvas.transform);

            //������� ������ � ������ ��������
            spaceGenerationData.Value.regionPEs[regionIndex] = currentRegion.selfPE;

            //���� ������ �� ��������� � ������� ����� �������
            if (regionGlobalX > 0)
            {
                //���� ������ � ������
                spaceGenerationData.Value.regionPEs[regionIndex - 1].Unpack(world.Value, out int wNeighbourRegionEntity);
                ref CHexRegion wNeighbourRegion = ref regionPool.Value.Get(wNeighbourRegionEntity);

                //������ ���������
                RegionSetNeighbour(
                    ref currentRegion, ref wNeighbourRegion,
                    HexDirection.W);
            }
            //���� ������ �� ��������� � ������� ������ ����
            if (regionGlobalZ > 0)
            {
                //���� ������ ��������� � �������� ����
                if ((regionGlobalZ & 1) == 0)
                {
                    //���� ������ � ���-�������
                    spaceGenerationData.Value.regionPEs[regionIndex - spaceGenerationData.Value.regionCountX].Unpack(world.Value, out int sENeighbourRegionEntity);
                    ref CHexRegion sENeighbourRegion = ref regionPool.Value.Get(sENeighbourRegionEntity);

                    //������ ���������
                    RegionSetNeighbour(
                        ref currentRegion, ref sENeighbourRegion,
                        HexDirection.SE);

                    //���� ������ �� ��������� � ������� ����� �������
                    if (regionGlobalX > 0)
                    {
                        //���� ������ � ���-������
                        spaceGenerationData.Value.regionPEs[regionIndex - spaceGenerationData.Value.regionCountX - 1].Unpack(world.Value, out int sWNeighbourRegionEntity);
                        ref CHexRegion sWNeighbourRegion = ref regionPool.Value.Get(sWNeighbourRegionEntity);

                        //������ ���������
                        RegionSetNeighbour(
                            ref currentRegion, ref sWNeighbourRegion,
                            HexDirection.SW);
                    }
                }
                //�����
                else
                {
                    //���� ������ � ���-������
                    spaceGenerationData.Value.regionPEs[regionIndex - spaceGenerationData.Value.regionCountX].Unpack(world.Value, out int sWNeighbourRegionEntity);
                    ref CHexRegion sWNeighbourRegion = ref regionPool.Value.Get(sWNeighbourRegionEntity);

                    //������ ���������
                    RegionSetNeighbour(
                        ref currentRegion, ref sWNeighbourRegion,
                        HexDirection.SW);

                    //���� ������ �� ��������� � ������� ������ �������
                    if (regionGlobalX < spaceGenerationData.Value.regionCountX - 1)
                    {
                        //���� ������ � ���-�������
                        spaceGenerationData.Value.regionPEs[regionIndex - spaceGenerationData.Value.regionCountX + 1].Unpack(world.Value, out int sENeighbourRegionEntity);
                        ref CHexRegion sENeighbourRegion = ref regionPool.Value.Get(sENeighbourRegionEntity);

                        //������ ���������
                        RegionSetNeighbour(
                            ref currentRegion, ref sENeighbourRegion,
                            HexDirection.SE);
                    }
                }
            }


            //��������� �������� ������ RAEO
            currentRAEO = new(
                currentRegion.selfPE);
        }

        void RegionSetNeighbour(
            ref CHexRegion current, ref CHexRegion neighbourRegion,
            HexDirection direction)
        {
            //����� ������ �� ���������� ����������� �������� �������
            current.neighbourRegionPEs[(int)direction] = neighbourRegion.selfPE;

            //����� ������ �� ���������������� ����������� ������
            neighbourRegion.neighbourRegionPEs[(int)direction.Opposite()] = current.selfPE;
        }

        void ORAEOCreating()
        {
            //������ ��������� ������ DORAEO
            List<DOrganizationRAEO> tempDORAEO = new();

            //���������� ���������� �����������
            int organizationsCount = organizationFilter.Value.GetEntitiesCount();

            //��� ������� RAEO
            foreach (int rAEOEntity in regionAEOFilter.Value)
            {
                //���� ��������� RAEO
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                //������� ��������� ������
                tempDORAEO.Clear();

                //��� ������� ����������� �������� ORAEO
                foreach (int gameCreateORAEOPanelEventEntity in oRAEOCreateSRFilter.Value)
                {
                    //���� ��������� ����������� � ��������� �����������
                    ref COrganization organization = ref organizationPool.Value.Get(gameCreateORAEOPanelEventEntity);

                    //������ ����� �������� � ��������� �� ��������� ExplorationORAEO
                    int oRAEOEntity = world.Value.NewEntity();
                    ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Add(oRAEOEntity);

                    //��������� �������� ������ ExORAEO
                    exORAEO = new(
                        world.Value.PackEntity(oRAEOEntity),
                        organization.selfPE,
                        rAEO.selfPE,
                        0);

                    //������ ��������� ��� �������� ������ ����������� ��������������� � RAEO
                    DOrganizationRAEO organizationRAEOData = new(
                        exORAEO.selfPE,
                        ORAEOType.Exploration);

                    //������� � �� ��������� ������
                    tempDORAEO.Add(organizationRAEOData);
                }

                //��������� ������ ������ �������
                int oldArraySize = rAEO.organizationRAEOs.Length;

                //��������� ������ DORAEO
                Array.Resize(
                    ref rAEO.organizationRAEOs, 
                    organizationsCount);

                //��� ������� DORAEO �� ��������� �������
                for (int a = 0; a < tempDORAEO.Count; a++)
                {
                    //��������� DORAEO � ������ �� �������
                    rAEO.organizationRAEOs[oldArraySize++] = tempDORAEO[a];
                }
            }
        }
    }
}