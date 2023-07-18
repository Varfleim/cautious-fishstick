
using System;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
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

        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
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
        readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;
        readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ��������� �����
            foreach (int mapGenerationEventEntity in mapGenerationEventFilter.Value)
            {
                //���� ��������� ������� ��������� �����
                ref EMapGeneration mapGenerationEvent= ref mapGenerationEventPool.Value.Get(mapGenerationEventEntity);

                //�������������� ���-�������
                MapGenerationData.InitializeHashGrid();

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


            //������ �����
            MapCreateChunks(ref mapGenerationEvent);

            //������ �������
            MapCreateRegions(ref mapGenerationEvent);


            //������������ ��������� �����
            MapWealthDistribution();

            //���������� ����� 
            MapGeneration(ref mapGenerationEvent);
        }

        void MapInitialization(
            ref EMapGeneration mapGenerationEvent)
        {
            //��������� �������� ����
            MapGenerationData.noiseSource
                = mapGenerationData.Value.noiseTexture;

            //���������� ���������� ������
            mapGenerationData.Value.chunkCountX
                = mapGenerationEvent.chunkCountX;
            mapGenerationData.Value.chunkCountZ
                = mapGenerationEvent.chunkCountZ;

            //���������� ������ ������� PE ������
            mapGenerationData.Value.chunkPEs = new EcsPackedEntity[mapGenerationData.Value.chunkCountX * mapGenerationData.Value.chunkCountZ];

            //���������� ���������� ��������
            mapGenerationData.Value.regionCountX
                = mapGenerationEvent.chunkCountX
                * MapGenerationData.chunkSizeX;
            mapGenerationData.Value.regionCountZ
                = mapGenerationEvent.chunkCountZ
                * MapGenerationData.chunkSizeZ;
            mapGenerationData.Value.regionCount = mapGenerationData.Value.regionCountX * mapGenerationData.Value.regionCountZ;

            //���������� ������ ������
            MapGenerationData.wrapSize = mapGenerationData.Value.regionCountX;

            //���������� ������ ������� PE ��������
            mapGenerationData.Value.regionPEs = new EcsPackedEntity[mapGenerationData.Value.regionCount];

            //�������������� ������ �������
            mapGenerationData.Value.regionShaderData.Initialize(mapGenerationData.Value.regionCountX, mapGenerationData.Value.regionCountZ);
        }

        void MapCreateChunks(
            ref EMapGeneration mapGenerationEvent)
        {
            //������ ������ �������� �����
            mapGenerationData.Value.columns = new Transform[mapGenerationData.Value.chunkCountX];
            //��� ������� ����� �� ������
            for (int x = 0; x < mapGenerationData.Value.chunkCountX; x++)
            {
                //������ ����� ������� � ������������ ��� � ������������ �������
                mapGenerationData.Value.columns[x] = new GameObject("Column").transform;
                mapGenerationData.Value.columns[x].SetParent(sceneData.Value.coreObject);
            }


            //��� ������� ����� �� ������
            for (int z = 0, i = 0; z < mapGenerationData.Value.chunkCountZ; z++)
            {
                //��� ������� ����� �� ������
                for (int x = 0; x < mapGenerationData.Value.chunkCountX; x++)
                {
                    //������ ����
                    ChunkCreate(
                        ref mapGenerationEvent,
                        x, z,
                        i++);
                }
            }
        }

        void MapCreateRegions(
            ref EMapGeneration mapGenerationEvent)
        {
            //��� ������� ������� �� ������
            for (int z = 0, i = 0; z < mapGenerationData.Value.regionCountZ; z++)
            {
                //��� ������� ������� �� ������
                for (int x = 0; x < mapGenerationData.Value.regionCountX; x++)
                {
                    //������ ������
                    RegionCreate(
                        x, z,
                        i++);
                }
            }
        }

        void MapWealthDistribution()
        {

        }

        void MapGeneration(
            ref EMapGeneration mapGenerationEvent)
        {
            //������ ������� ������������ ������� ��� ��������� ����������� ������ ����
            HexRegionPriorityQueue searchFrontier = new();

            //��� ������� �������
            foreach (int regionEntity in regionFilter.Value)
            {
                //���� ��������� �������
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //������������� ������� ����
                region.WaterLevel = mapGenerationData.Value.waterLevel;
            }

            //������ ���� �����
            MapCreateAreas();

            //������ ����
            MapCreateLand(searchFrontier);

            //�������� ������
            MapErodeLand();

            //������ ������
            List<DHexRegionClimate> climate = MapCreateClimat();

            //������������� ���� ���������
            MapSetTerrainType(climate);

            //��� ������� �������
            foreach (int regionEntity in regionFilter.Value)
            {
                //���� ��������� �������
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //������������� ���� ������ �� ����
                region.SearchPhase = 0;
            }
        }

        void MapCreateAreas()
        {
            int borderX = mapGenerationData.Value.areaBorder;

            //������ ���� �����
            DMapArea mapArea;

            switch (mapGenerationData.Value.areaCount)
            {
                default:
                    borderX = 0;

                    mapGenerationData.Value.mapAreas = new DMapArea[1];

                    mapArea.xMin = borderX;
                    mapArea.xMax = mapGenerationData.Value.regionCountX - borderX;
                    mapArea.zMin = mapGenerationData.Value.mapBorderZ;
                    mapArea.zMax = mapGenerationData.Value.regionCountZ - mapGenerationData.Value.mapBorderZ;
                    mapGenerationData.Value.mapAreas[0] = mapArea;
                    break;
                case 2:
                    mapGenerationData.Value.mapAreas = new DMapArea[2];

                    if (UnityEngine.Random.value < 0.5f)
                    {
                        mapArea.xMin = borderX;
                        mapArea.xMax = mapGenerationData.Value.regionCountX / 2 - mapGenerationData.Value.areaBorder;
                        mapArea.zMin = mapGenerationData.Value.mapBorderZ;
                        mapArea.zMax = mapGenerationData.Value.regionCountZ - mapGenerationData.Value.mapBorderZ;
                        //������� ����� ���� � ������
                        mapGenerationData.Value.mapAreas[0] = mapArea;

                        mapArea.xMin = mapGenerationData.Value.regionCountX / 2 + mapGenerationData.Value.areaBorder;
                        mapArea.xMax = mapGenerationData.Value.regionCountX - borderX;
                        //������� ����� ���� � ������
                        mapGenerationData.Value.mapAreas[1] = mapArea;
                    }
                    else
                    {
                        borderX = 0;

                        mapArea.xMin = borderX;
                        mapArea.xMax = mapGenerationData.Value.regionCountX - borderX;
                        mapArea.zMin = mapGenerationData.Value.mapBorderZ;
                        mapArea.zMax = mapGenerationData.Value.regionCountZ / 2 - mapGenerationData.Value.areaBorder;
                        //������� ����� ���� � ������
                        mapGenerationData.Value.mapAreas[0] = mapArea;

                        mapArea.zMin = mapGenerationData.Value.regionCountZ / 2 + mapGenerationData.Value.areaBorder;
                        mapArea.zMax = mapGenerationData.Value.regionCountZ - mapGenerationData.Value.mapBorderZ;
                        //������� ����� ���� � ������
                        mapGenerationData.Value.mapAreas[1] = mapArea;
                    }
                    break;
            }
        }

        void MapCreateLand(
            HexRegionPriorityQueue searchFrontier)
        {
            //���������� ���������� �������� ����
            int landBudget = Mathf.RoundToInt(mapGenerationData.Value.regionCount * mapGenerationData.Value.landPercentage * 0.01f);

            //���� ������ ���� ������ ���� � ������ ����� 10000 ��������
            for(int a = 0; a < 10000; a++)
            {
                //����������, ����� �� ������� ����
                bool sink = UnityEngine.Random.value < mapGenerationData.Value.sinkProbability;

                //��� ������ ���� �����
                for (int b = 0; b < mapGenerationData.Value.mapAreas.Length; b++)
                {
                    //���������� ������ �����
                    int chunkSize = UnityEngine.Random.Range(mapGenerationData.Value.chunkSizeMin, mapGenerationData.Value.chunkSizeMax + 1);

                    //���� ��������� ������� ����
                    if (sink == true)
                    {
                        //����� ����
                        MapSinkTerrain(
                            searchFrontier,
                            ref mapGenerationData.Value.mapAreas[b],
                            chunkSize,
                            landBudget,
                            out landBudget);
                    }
                    //�����
                    else
                    {
                        //��������� ����
                        MapRaiseTerrain(
                            searchFrontier,
                            ref mapGenerationData.Value.mapAreas[b],
                            chunkSize,
                            landBudget,
                            out landBudget);

                        //���� ������ ���� ����� ����
                        if (landBudget == 0)
                        {
                            return;
                        }
                    }
                }
            }

            //���� ������ ���� ������ ����
            if (landBudget > 0)
            {
                Debug.LogWarning("Failed to use up " + landBudget + " land budget.");
            }
        }

        void MapRaiseTerrain(
            HexRegionPriorityQueue searchFrontier,
            ref DMapArea mapArea,
            int chunkSize,
            int landBudget,
            out int currentLandBudget)
        {
            //����������� ���� ������
            inputData.Value.searchFrontierPhase += 1;

            //���� ��������� ������� ������� ��������
            RegionGetRandom(ref mapArea).Unpack(world.Value, out int firstRegionEntity);
            ref CHexRegion firstRegion = ref regionPool.Value.Get(firstRegionEntity);

            //������������� ��� ���� ������ �� �������
            firstRegion.SearchPhase = inputData.Value.searchFrontierPhase;
            firstRegion.Distance = 0;
            firstRegion.SearchHeuristic = 0;
            //�������� ������ � ������� ������
            searchFrontier.Enqueue(
                firstRegion.selfPE,
                firstRegion.SearchPriority);

            //���������� ���������� ������� �������
            DHexCoordinates center = firstRegion.coordinates;

            //����������, ����� ��� �������� ��������� ���� ��� �������
            int rise = UnityEngine.Random.value < mapGenerationData.Value.highRiseProbability ? 2 : 1;
            //������ ������� �������
            int currentSize = 0;
            //���� ������� ������ ���������� ������� � ������� �� �����
            while (currentSize < chunkSize && searchFrontier.Count > 0)
            {
                //���� ��������� ������� ������� � �������
                searchFrontier.Dequeue().cellPE.Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //���������� �������� ������ �������
                int originalElevation = region.Elevation;
                //���������� ����� ������ �������
                int newElevation = originalElevation + rise;

                //���� ����� ������ ������ ������������
                if (newElevation > mapGenerationData.Value.elevationMaximum)
                {
                    continue;
                }

                //����������� ������ �������
                region.Elevation = newElevation;

                //���� �������� ������ ������� ������ ������ ����
                if(originalElevation < mapGenerationData.Value.waterLevel
                    //� ����� ������ ������ ��� ����� ������ ����
                    && newElevation >= mapGenerationData.Value.waterLevel
                    //� ����������� ������ ���� ����� ����
                    && --landBudget == 0)
                {
                    break;
                }

                //����������� �������
                currentSize += 1;

                //��� ������� �����������
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    //���� ����� � ������� ����������� ����������
                    if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                    {
                        //���� ��������� ��������� �������
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //���� ���� ������ ������ ������ �������
                        if (neighbourRegion.SearchPhase < inputData.Value.searchFrontierPhase)
                        {
                            //������������� ��� ���� ������ �� �������
                            neighbourRegion.SearchPhase = inputData.Value.searchFrontierPhase;
                            neighbourRegion.Distance = neighbourRegion.coordinates.DistanceTo(center);
                            neighbourRegion.SearchHeuristic = UnityEngine.Random.value < mapGenerationData.Value.jitterProbability ? 1 : 0;
                            //�������� ������ � ������� ������
                            searchFrontier.Enqueue(
                                neighbourRegion.selfPE, 
                                neighbourRegion.SearchPriority);
                        }
                    }
                }
            }
            //������� ������� ������
            searchFrontier.Clear();

            //���������� ���������� ������ ����
            currentLandBudget = landBudget;
        }
        
        void MapSinkTerrain(
            HexRegionPriorityQueue searchFrontier,
            ref DMapArea mapArea,
            int chunkSize,
            int landBudget,
            out int currentLandBudget)
        {
            //����������� ���� ������
            inputData.Value.searchFrontierPhase += 1;

            //���� ��������� ������� ������� ��������
            RegionGetRandom(ref mapArea).Unpack(world.Value, out int firstRegionEntity);
            ref CHexRegion firstRegion = ref regionPool.Value.Get(firstRegionEntity);

            //������������� ��� ���� ������ �� �������
            firstRegion.SearchPhase = inputData.Value.searchFrontierPhase;
            firstRegion.Distance = 0;
            firstRegion.SearchHeuristic = 0;
            //�������� ������ � ������� ������
            searchFrontier.Enqueue(
                firstRegion.selfPE,
                firstRegion.SearchPriority);

            //���������� ���������� ������� �������
            DHexCoordinates center = firstRegion.coordinates;

            //����������, ����� ��� �������� ���������� ���� ��� �������
            int sink = UnityEngine.Random.value < mapGenerationData.Value.highRiseProbability ? 2 : 1;
            //������ ������� �������
            int currentSize = 0;
            //���� ������� ������ ���������� ������� � ������� �� �����
            while (currentSize < chunkSize && searchFrontier.Count > 0)
            {
                //���� ��������� ������� ������� � �������
                searchFrontier.Dequeue().cellPE.Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //���������� �������� ������ �������
                int originalElevation = region.Elevation;
                //���������� ����� ������ �������
                int newElevation = region.Elevation - sink;

                //���� ����� ������ ������� ������ �����������
                if (newElevation < mapGenerationData.Value.elevationMinimum)
                {
                    continue;
                }

                //��������� ������ �������
                region.Elevation = newElevation;

                //���� �������� ������ ������� ������ ��� ����� ������ ����
                if(originalElevation >= mapGenerationData.Value.waterLevel
                    //� ����� ������ ������ ������ ����
                    && newElevation < mapGenerationData.Value.waterLevel)
                {
                    //����������� ������ ����
                    landBudget += 1;
                }

                //����������� �������
                currentSize += 1;

                //��� ������� �����������
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    //���� ����� � ������� ����������� ����������
                    if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                    {
                        //���� ��������� ��������� �������
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //���� ���� ������ ������ ������ �������
                        if (neighbourRegion.SearchPhase < inputData.Value.searchFrontierPhase)
                        {
                            //������������� ��� ���� ������ �� �������
                            neighbourRegion.SearchPhase = inputData.Value.searchFrontierPhase;
                            neighbourRegion.Distance = neighbourRegion.coordinates.DistanceTo(center);
                            neighbourRegion.SearchHeuristic = UnityEngine.Random.value < mapGenerationData.Value.jitterProbability ? 1 : 0;
                            //�������� ������ � ������� ������
                            searchFrontier.Enqueue(
                                neighbourRegion.selfPE, 
                                neighbourRegion.SearchPriority);
                        }
                    }
                }
            }
            //������� ������� ������
            searchFrontier.Clear();

            //���������� ���������� ������ ����
            currentLandBudget = landBudget;
        }

        void MapErodeLand()
        {
            //������ ������ PE ��������, ���������� ������
            List<int> erodibleRegions = ListPool<int>.Get();

            //��� ������� �������
            foreach (int regionEntity in regionFilter.Value)
            {
                //���� ��������� �������
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //���� ������ �������� ������
                if (RegionIsErodible(ref region) == true)
                {
                    //������� ��� � ������ 
                    erodibleRegions.Add(regionEntity);
                }
            }

            //����������, ������� �������� ������ ������������� ������
            int erodibleRegionsCount = (int)(erodibleRegions.Count * (100 - mapGenerationData.Value.erosionPercentage) * 0.01f);

            //���� ������ PE �������� ������ ���������� �����
            while (erodibleRegions.Count > erodibleRegionsCount)
            {
                //�������� ��������� ������ � ������
                int index = UnityEngine.Random.Range(0, erodibleRegions.Count);
                ref CHexRegion region = ref regionPool.Value.Get(erodibleRegions[index]);

                //�������� ������ ������ ��� ���� ������
                RegionGetErosionTarget(ref region).Unpack(world.Value, out int targetRegionEntity);
                ref CHexRegion targetRegion = ref regionPool.Value.Get(targetRegionEntity);

                //��������� ������ ��������� ������� � ����������� ������ ��������
                region.Elevation -= 1;
                targetRegion.Elevation += 1;

                //���� ������ ������ �� �������� ������
                if (RegionIsErodible(ref region) == false)
                {
                    //������� ��� PE �� ������, ������� ��������� �������� � ������
                    erodibleRegions[index] = erodibleRegions[erodibleRegions.Count - 1];
                    erodibleRegions.RemoveAt(erodibleRegions.Count - 1);
                }

                //��� ������� �����������
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    //���� ����� ����������
                    if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                    {
                        //���� ��������� ������
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //���� ������ ������ �� 2 ������ ������������ �������
                        if (neighbourRegion.Elevation == region.Elevation + 2
                            //���� ����� �������� ������
                            && RegionIsErodible(ref neighbourRegion) == true
                            //� ���� ������ �� �������� PE ������
                            && erodibleRegions.Contains(neighbourRegionEntity) == false)
                        {
                            //������� ������ � ������
                            erodibleRegions.Add(neighbourRegionEntity);
                        }
                    }
                }

                //���� ������� ������ �������� ������
                if (RegionIsErodible(ref targetRegion)
                    //� ������ PE ���������� ������ �������� �� �������� ���
                    && erodibleRegions.Contains(targetRegionEntity) == false)
                {
                    //������� ������� ������ � ������
                    erodibleRegions.Add(targetRegionEntity);
                }

                //��� ������� �����������
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    //���� ����� ����������
                    if (targetRegion.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity)
                        //� ���� ����� - �� �������� ����������� ������
                        && region.selfPE.EqualsTo(targetRegion.GetNeighbour(d)) == false)
                    {
                        //���� ��������� ������
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //���� ������ ������ �� ������� ������, ��� ������ �������� �������
                        if (neighbourRegion.Elevation == targetRegion.Elevation + 1
                            //� ���� ����� �� �������� ������
                            && RegionIsErodible(ref neighbourRegion) == false
                            //� ���� ������ �������� PE ������
                            && erodibleRegions.Contains(neighbourRegionEntity) == true)
                        {
                            //������� ������ �� ������
                            erodibleRegions.Remove(neighbourRegionEntity);
                        }
                    }
                }
            }

            //������� ������ � ���
            ListPool<int>.Add(erodibleRegions);
        }

        List<DHexRegionClimate> MapCreateClimat()
        {
            //������ ������ ��� �������� �������
            List<DHexRegionClimate> climate = new();
            List<DHexRegionClimate> nextClimate = new();

            //��� ������� �������
            foreach (int regionEntity in regionFilter.Value)
            {
                //���� ��������� �������
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //������ ������ ��� ���� � ����� �������
                climate.Add(new(0, mapGenerationData.Value.startingMoisture));
                nextClimate.Add(new(0, 0));
            }

            //��������� ���
            for (int a = 0; a < 40; a++)
            {
                //��� ������� �������
                foreach (int regionEntity in regionFilter.Value)
                {
                    //���� ��������� �������
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    //������������ ������������� ������
                    RegionEvolveClimate(
                        ref region,
                        climate, nextClimate);
                }

                //������ ������ �������
                List<DHexRegionClimate> swap = climate;
                climate = nextClimate;
                nextClimate = swap;
            }

            return climate;
        }

        void MapSetTerrainType(
            List<DHexRegionClimate> climate)
        {
            //������������ ������, ����������� ��� ��������� ���������� �������
            int rockDesertElevation = mapGenerationData.Value.elevationMaximum 
                - (mapGenerationData.Value.elevationMaximum - mapGenerationData.Value.waterLevel) / 2;

            //��� ������� �������
            foreach (int regionEntity in regionFilter.Value)
            {
                //���� ��������� ������� � ������ �������
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                DHexRegionClimate regionClimate = climate[region.Index];

                //������������ ����������� �������
                float temperature = RegionDetermineTemperature(ref region);

                //���� ������ �� ��� �����
                if (region.IsUnderwater == false)
                {
                    //���������� ������� ����������� �������
                    int t = 0;

                    //��� ������� ������ �����������
                    for (; t < MapGenerationData.temperatureBands.Length; t++)
                    {
                        //���� ����������� ������ ������
                        if (temperature < MapGenerationData.temperatureBands[t])
                        {
                            break;
                        }
                    }

                    //���������� ������� ��������� �������
                    int m = 0;

                    //��� ������� ������ ���������
                    for (; m < MapGenerationData.moistureBands.Length; m++)
                    {
                        //���� ��������� ������ ������
                        if (regionClimate.moisture < MapGenerationData.moistureBands[m])
                        {
                            break;
                        }
                    }

                    //���������� ���� �������
                    ref DBiome biome = ref MapGenerationData.biomes[t * 4 + m];

                    //���� ��� ��������� ����� - �������
                    if (biome.terrainTypeIndex == 0)
                    {
                        //���� ������ ������� ������ ��� ����� ������, ��������� ��� ���������� �������
                        if (region.Elevation >= rockDesertElevation)
                        {
                            //������������� ��� ��������� ������� �� ������
                            region.TerrainTypeIndex = 3;
                        }
                        //�����
                        else
                        {
                            //������������� ��� ��������� �� ����������� ��������
                            region.TerrainTypeIndex = biome.terrainTypeIndex;
                        }
                    }
                    //�����, ���� ������ ������� ����� ������������
                    else if(region.Elevation == mapGenerationData.Value.elevationMaximum)
                    {
                        Debug.LogWarning("!");

                        //������������� ��� ��������� ������� �� ����
                        region.TerrainTypeIndex = 4;
                    }
                    //�����
                    else
                    {
                        //������������� ��� ��������� �������
                        region.TerrainTypeIndex = biome.terrainTypeIndex;
                    }

                    //���� ������ ������� �� ����� ������������
                    if (region.Elevation != mapGenerationData.Value.elevationMaximum)
                    {
                        //������������� ������� �������������� �������������� �����
                        region.PlantLevel = biome.plant;
                    }
                    //�����
                    else
                    {
                        //������� �������������� ����� ����
                        region.PlantLevel = 0;
                    }
                }
                //�����
                else
                {
                    //���������� ��� ���������
                    int terrainTypeIndex;

                    //���� ������ �� ���� ������� ���� ������ ����
                    if (region.Elevation == mapGenerationData.Value.waterLevel - 1)
                    {
                        //���������� ���������� ������� �� �������� � ��������
                        int cliffs = 0, slopes = 0;

                        //��� ������� �����������
                        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                        {
                            //���� ����� ����������
                            if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                            {
                                //���� ��������� ������
                                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                                //���������� ������� � ������ � ������� ����
                                int elevationDelta = neighbourRegion.Elevation - region.WaterLevel;

                                //���� ������� ����� ����
                                if (elevationDelta == 0)
                                {
                                    //�� ����� �����
                                    slopes += 1;
                                }
                                //�����, ���� ������� ������ ����
                                else if (elevationDelta > 0)
                                {
                                    //�� ����� �����
                                    cliffs += 1;
                                }
                            }
                        }

                        //���� ����� ������� � ������� ������ ���
                        if (cliffs + slopes > 3)
                        {
                            //��� ��������� - �����
                            terrainTypeIndex = 1;
                        }
                        //�����, ���� ������� ������ ����
                        else if(cliffs > 0)
                        {
                            //��� ��������� - ������
                            terrainTypeIndex = 3;
                        }
                        //�����, ���� ������� ������ ����
                        else if(slopes > 0)
                        {
                            //��� ��������� - �������
                            terrainTypeIndex = 0;
                        }
                        //�����
                        else
                        {
                            //��� ��������� - �����
                            terrainTypeIndex = 1;
                        }
                    }
                    //�����, ���� ������ ������� ������ ������ ����
                    else if(region.Elevation >= mapGenerationData.Value.waterLevel)
                    {
                        //��� ��������� - �����
                        terrainTypeIndex = 1;
                    }
                    //�����, ���� ������ ������� ������������
                    else if(region.Elevation < 0)
                    {
                        //��� ��������� - ������
                        terrainTypeIndex = 3;
                    }
                    //�����
                    else
                    {
                        //��� ��������� - �����
                        terrainTypeIndex = 2;
                    }
                    
                    //���� ��� ��������� - ����� � ����������� ��������� � ����� ������ ���������
                    if (terrainTypeIndex == 1 && temperature < MapGenerationData.temperatureBands[0])
                    {
                        //��� ��������� - �����
                        terrainTypeIndex = 2;
                    }

                    //������������� ��� ���������
                    region.TerrainTypeIndex = terrainTypeIndex;
                }

                //������������� ������ �����
                //region.SetMapData(temperature);
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
                MapGenerationData.ChunkSize);

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

            //������������ ���� � ���������������� �������
            chunk.transform.SetParent(mapGenerationData.Value.columns[chunkX], false);

            //������� ���� � ������ ������
            mapGenerationData.Value.chunkPEs[chunkIndex] = chunk.selfPE;

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
            int parentChunkX = regionGlobalX / MapGenerationData.chunkSizeX;
            int parentChunkZ = regionGlobalZ / MapGenerationData.chunkSizeZ;
            mapGenerationData.Value.chunkPEs[parentChunkX + parentChunkZ * mapGenerationData.Value.chunkCountX].Unpack(
                world.Value, out int parentChunkEntity);
            ref CHexChunk parentChunk = ref chunkPool.Value.Get(parentChunkEntity);

            int regionLocalX = regionGlobalX - parentChunkX * MapGenerationData.chunkSizeX;
            int regionLocalZ = regionGlobalZ - parentChunkZ * MapGenerationData.chunkSizeZ;

            //������ ����� �������� � ��������� �� ��������� ������� � RAEO
            int regionEntity = world.Value.NewEntity();
            ref CHexRegion currentRegion = ref regionPool.Value.Add(regionEntity);
            ref CRegionAEO currentRAEO = ref regionAEOPool.Value.Add(regionEntity);

            //���������� ������� �������
            Vector3 position = new(
                (regionGlobalX + regionGlobalZ * 0.5f - regionGlobalZ / 2) * (MapGenerationData.innerRadius * 2f),
                0f,
                regionGlobalZ * (MapGenerationData.outerRadius * 1.5f));
            //���������� ���������� �������
            DHexCoordinates regionCoordinates = DHexCoordinates.FromOffsetCoordinates(regionGlobalX, regionGlobalZ);

            //������ ������ �������
            GORegion regionGO = GameObject.Instantiate(staticData.Value.regionPrefab);

            //��������� �������� ������ �������
            currentRegion = new(
                world.Value.PackEntity(regionEntity), regionIndex,
                position, regionCoordinates,
                regionGO.regionTransform, regionGO.regionLabel, regionGO.regionHighlight,
                regionGlobalX / MapGenerationData.chunkSizeX, parentChunk.selfPE,
                mapGenerationData.Value.regionShaderData);

            //���������� ������ ������� �� ��������������� �������
            currentRegion.transform.localPosition = currentRegion.Position;

            //���������� ������ ����� �� ��������������� ������� � ���������� ���������� �������
            currentRegion.uiRect.rectTransform.anchoredPosition = new(currentRegion.Position.x, currentRegion.Position.z);

            //������� ������ � ������ �������� ������������� �����
            parentChunk.regionPEs[regionLocalX + regionLocalZ * MapGenerationData.chunkSizeX] = currentRegion.selfPE;

            //����������� ������� ������� � ������� �����
            currentRegion.transform.SetParent(parentChunk.transform, false);
            currentRegion.uiRect.rectTransform.SetParent(parentChunk.canvas.transform, false);
            currentRegion.highlight.rectTransform.SetParent(parentChunk.canvas.transform);

            //������� ������ � ������ ��������
            mapGenerationData.Value.regionPEs[regionIndex] = currentRegion.selfPE;

            //���� ������ �� ��������� � ������� ����� �������
            if (regionGlobalX > 0)
            {
                //���� ������ � ������
                mapGenerationData.Value.regionPEs[regionIndex - 1].Unpack(world.Value, out int wNeighbourRegionEntity);
                ref CHexRegion wNeighbourRegion = ref regionPool.Value.Get(wNeighbourRegionEntity);

                //������ ���������
                RegionSetNeighbour(
                    ref currentRegion, ref wNeighbourRegion,
                    HexDirection.W);

                //���� ������ ��������� � ������� ������ �������
                if (regionGlobalX == mapGenerationData.Value.regionCountX - 1)
                {
                    //���� ������ � �������
                    mapGenerationData.Value.regionPEs[regionIndex - regionGlobalX].Unpack(world.Value, out int eNeighbourRegionEntity);
                    ref CHexRegion eNeighbourRegion = ref regionPool.Value.Get(eNeighbourRegionEntity);

                    //������ ��������� 
                    RegionSetNeighbour(
                        ref currentRegion, ref eNeighbourRegion,
                        HexDirection.E);
                }
            }
            //���� ������ �� ��������� � ������� ������ ����
            if (regionGlobalZ > 0)
            {
                //���� ������ ��������� � �������� ����
                if ((regionGlobalZ & 1) == 0)
                {
                    //���� ������ � ���-�������
                    mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX].Unpack(world.Value, out int sENeighbourRegionEntity);
                    ref CHexRegion sENeighbourRegion = ref regionPool.Value.Get(sENeighbourRegionEntity);

                    //������ ���������
                    RegionSetNeighbour(
                        ref currentRegion, ref sENeighbourRegion,
                        HexDirection.SE);

                    //���� ������ �� ��������� � ������� ����� �������
                    if (regionGlobalX > 0)
                    {
                        //���� ������ � ���-������
                        mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX - 1].Unpack(world.Value, out int sWNeighbourRegionEntity);
                        ref CHexRegion sWNeighbourRegion = ref regionPool.Value.Get(sWNeighbourRegionEntity);

                        //������ ���������
                        RegionSetNeighbour(
                            ref currentRegion, ref sWNeighbourRegion,
                            HexDirection.SW);
                    }
                    //�����
                    else
                    {
                        //���� ������ � ���-������
                        mapGenerationData.Value.regionPEs[regionIndex - 1].Unpack(world.Value, out int sWNeighbourRegionEntity);
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
                    mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX].Unpack(world.Value, out int sWNeighbourRegionEntity);
                    ref CHexRegion sWNeighbourRegion = ref regionPool.Value.Get(sWNeighbourRegionEntity);

                    //������ ���������
                    RegionSetNeighbour(
                        ref currentRegion, ref sWNeighbourRegion,
                        HexDirection.SW);

                    //���� ������ �� ��������� � ������� ������ �������
                    if (regionGlobalX < mapGenerationData.Value.regionCountX - 1)
                    {
                        //���� ������ � ���-�������
                        mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX + 1].Unpack(world.Value, out int sENeighbourRegionEntity);
                        ref CHexRegion sENeighbourRegion = ref regionPool.Value.Get(sENeighbourRegionEntity);

                        //������ ���������
                        RegionSetNeighbour(
                            ref currentRegion, ref sENeighbourRegion,
                            HexDirection.SE);
                    }
                    //�����
                    else
                    {
                        //���� ������ � ���-�������
                        mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX * 2 + 1].Unpack(world.Value, out int sENeighbourRegionEntity);
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

        EcsPackedEntity RegionGet(
            int xOffset, int zOffset)
        {
            //���������� PE ������������ �������
            return mapGenerationData.Value.regionPEs[xOffset + zOffset * mapGenerationData.Value.regionCountX];
        }

        EcsPackedEntity RegionGet(
            int regionIndex)
        {
            //���������� PE ������������ �������
            return mapGenerationData.Value.regionPEs[regionIndex];
        }

        EcsPackedEntity RegionGetRandom(
            ref DMapArea mapArea)
        {
            //���������� PE ���������� �������
            return RegionGet(
                UnityEngine.Random.Range(mapArea.xMin, mapArea.xMax),
                UnityEngine.Random.Range(mapArea.zMin, mapArea.zMax));
        }

        bool RegionIsErodible(
            ref CHexRegion region)
        {
            //���������� ������, ��� ������� ��������� ������
            int erodibleElevation = region.Elevation - 2;

            //��� ������� �����������
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //���� ����� ����������
                if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //���� ��������� ������
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //���� ������ ������ ������ ��� ����� ������ ������
                    if (neighbourRegion.Elevation <= erodibleElevation)
                    {
                        //����������, ��� ������ ��������
                        return true;
                    }
                }
            }

            //����������, ��� ������ ����������
            return false;
        }

        EcsPackedEntity RegionGetErosionTarget(
            ref CHexRegion region)
        {
            //������ ������ ���������� ����� ������
            List<int> candidates = ListPool<int>.Get();

            //���������� ������, ��� ������� ��������� ������
            int erodibleElevation = region.Elevation - 2;

            //��� ������� �����������
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //���� ����� ����������
                if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //���� ��������� ������
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //���� ������ ������ ������ ��� ����� ������ ������
                    if (neighbourRegion.Elevation <= erodibleElevation)
                    {
                        //������� ������ � ������
                        candidates.Add(neighbourRegionEntity);
                    }
                }
            }

            //�������� �������� ���� �� ������
            ref CHexRegion targetRegion = ref regionPool.Value.Get(candidates[UnityEngine.Random.Range(0, candidates.Count)]);

            //������� ������ � ���
            ListPool<int>.Add(candidates);

            //���������� PE �������� �������
            return targetRegion.selfPE;
        }

        void RegionEvolveClimate(
            ref CHexRegion region,
            List<DHexRegionClimate> climate,
            List<DHexRegionClimate> nextClimate)
        {
            //���� ������ �������
            DHexRegionClimate regionClimate = climate[region.Index];

            //���� ������ ��������� ��� �����
            if (region.IsUnderwater == true)
            {
                //���������� ���������
                regionClimate.moisture = 1f;

                //������������ ���������
                regionClimate.clouds += mapGenerationData.Value.evaporationFactor;
            }
            //�����
            else
            {
                //������������ ��������� 
                float evaporation = regionClimate.moisture * mapGenerationData.Value.evaporationFactor;
                regionClimate.moisture -= evaporation;
                regionClimate.clouds += evaporation;
            }

            //������������, ������� ������� ����������� � ������
            float precipitation = regionClimate.clouds * mapGenerationData.Value.precipitationFactor;
            //��������� ������ � ������� � ���������
            regionClimate.clouds -= precipitation;
            regionClimate.moisture += precipitation;

            //������������ �������� ������� ��� �������
            float cloudMaximum = 1f - region.ViewElevation / (mapGenerationData.Value.elevationMaximum + 1);
            //���� ������� ������ ����������
            if (regionClimate.clouds > cloudMaximum)
            {
                //�� ������� ��������� �� �����
                regionClimate.moisture += regionClimate.clouds - cloudMaximum;
                regionClimate.clouds = cloudMaximum;
            }

            //���������� ����������� �����
            HexDirection mainDispersalDirection = mapGenerationData.Value.windDirection.Opposite();
            //������������, ������� ������� ���������
            float cloudDispersal = regionClimate.clouds * (1f / (5f + mapGenerationData.Value.windStrength));

            //������������, ������� ������� � ������� ����
            float runoff = regionClimate.moisture * mapGenerationData.Value.runoffFactor * (1f / 6f);
            //������������, ������� ����� �������������
            float seepage = regionClimate.moisture * mapGenerationData.Value.seepageFactor * (1f / 6f);

            //��� ������� �����������
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //���� ����� ����������
                if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //���� ��������� ������ � ��������� ������ �������
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);
                    DHexRegionClimate neighbourRegionClimate = nextClimate[neighbourRegion.Index];

                    //���� ����������� ����� �������� ����������� �����
                    if (d == mainDispersalDirection)
                    {
                        //������������ ����� �������, ����������� ����� �����
                        neighbourRegionClimate.clouds += cloudDispersal * mapGenerationData.Value.windStrength;
                    }
                    //�����
                    else
                    {
                        //������������ ������� ����� �������
                        neighbourRegionClimate.clouds += cloudDispersal;
                    }

                    //������������ ������� � ������ ����� ���������
                    int elevationDelta = neighbourRegion.ViewElevation - region.ViewElevation;
                    //���� ������� ������ ����
                    if (elevationDelta < 0)
                    {
                        //���� ������ � �������� ������
                        neighbourRegionClimate.moisture -= runoff;
                        neighbourRegionClimate.moisture += runoff;
                    }
                    //�����, ���� ������� ����� ����
                    else if (elevationDelta == 0)
                    {
                        //������������ ������ � �������� �������
                        neighbourRegionClimate.moisture -= seepage;
                        neighbourRegionClimate.moisture += seepage;
                    }

                    //��������� ������ ������� ������ � ������
                    nextClimate[neighbourRegion.Index] = neighbourRegionClimate;
                }
            }

            //��������� ������ �������
            DHexRegionClimate regionNextClimate = nextClimate[region.Index];
            regionNextClimate.moisture += regionClimate.moisture;

            //���� ������� ��������� ������ �������
            if (regionNextClimate.moisture > 1f)
            {
                //������������� � �� �������
                regionNextClimate.moisture = 1f;
            }

            nextClimate[region.Index] = regionNextClimate;
            climate[region.Index] = new();
        }

        float RegionDetermineTemperature(
            ref CHexRegion region)
        {
            //������������ ������ �������
            float latitude = (float)region.coordinates.Z / mapGenerationData.Value.regionCountZ;
            latitude *= 2f;
            //���� ������ ������ �������, �� ��� ������ ���������
            if (latitude > 1f)
            {
                latitude = 2f - latitude;
            }

            //������������ ����������� �������
            //�� ������
            float temperature = Mathf.LerpUnclamped(mapGenerationData.Value.lowTemperature, mapGenerationData.Value.highTemperature, latitude);
            //�� ������
            temperature *= 1f - (region.ViewElevation - mapGenerationData.Value.waterLevel)
                / (mapGenerationData.Value.elevationMaximum - mapGenerationData.Value.waterLevel + 1f);
            //�� ���������� ������������
            temperature += (MapGenerationData.SampleNoise(region.Position * 0.1f).w * 2f - 1f) * mapGenerationData.Value.temperatureJitter;

            //���������� �����������
            return temperature;
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