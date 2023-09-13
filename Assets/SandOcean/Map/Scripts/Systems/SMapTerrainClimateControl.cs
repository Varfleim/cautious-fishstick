
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using SandOcean.Map.Pathfinding;
using SandOcean.Map.Events;
using SandOcean.AEO.RAEO;

namespace SandOcean.Map
{
    public class SMapTerrainClimateControl : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //�����
        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        readonly EcsPoolInject<CHexRegionGenerationData> regionGenerationDataPool = default;

        //���������������-������������� �������
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;


        //������� �����
        readonly EcsFilterInject<Inc<RMapGeneration>> mapGenerationRequestFilter = default;
        readonly EcsPoolInject<RMapGeneration> mapGenerationRequestPool = default;


        //������
        readonly EcsCustomInject<StaticData> staticData = default;
        readonly EcsCustomInject<SceneData> sceneData = default;
        readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ��������� �����
            foreach (int mapGenerationRequestEntity in mapGenerationRequestFilter.Value)
            {
                //���� ������
                ref RMapGeneration mapGenerationRequest = ref mapGenerationRequestPool.Value.Get(mapGenerationRequestEntity);

                //������ �����
                MapCreate(ref mapGenerationRequest);

                world.Value.DelEntity(mapGenerationRequestEntity);
            }
        }

        void MapCreate(
            ref RMapGeneration mapGenerationRequest)
        {
            //�������������� �����
            MapInitialization(ref mapGenerationRequest);


            //���������� ������ ����������
            MapGenerateHexasphere();

            //��������, ��� ��������� ��������� ����������
            mapGenerationData.Value.isInitializationUpdate = true;

            //��������, ��� ��������� �������� �������, �����, UV-���������� � ������� �������
            mapGenerationData.Value.isMaterialUpdated = true;
            mapGenerationData.Value.isRegionUpdated = true;
            mapGenerationData.Value.isColorUpdated = true;
            mapGenerationData.Value.isUVUpdatedFast = true;
            //mapGenerationData.Value.isTextureArrayUpdated = true;


            //���������� �������� � ������
            MapGenerateTerrainClimate();
        }

        void MapInitialization(
            ref RMapGeneration mapGenerationRequest)
        {
            //������������ ����� �������������
            mapGenerationData.Value.subdivisions = Mathf.Max(1, mapGenerationRequest.subdivisions);

            //������������� ��������� �������������
            MapGenerationData.extrudeMultiplier = 0.05f;

            //������������� ������� ����������
            MapGenerationData.hexasphereScale = 100f;

            SceneData.HexasphereGO = sceneData.Value.hexasphereGO;

            mapGenerationData.Value.fleetRegionHighlightMaterial.shaderKeywords = null;//new string[] { ShaderParameters.SKW_HIGHLIGHT_TINT_BACKGROUND };
            mapGenerationData.Value.fleetRegionHighlightMaterial.SetFloat(ShaderParameters.ColorShift, 1f);

            mapGenerationData.Value.hoverRegionHighlightMaterial.shaderKeywords = null;//= new string[] { ShaderParameters.SKW_HIGHLIGHT_TINT_BACKGROUND };
            mapGenerationData.Value.hoverRegionHighlightMaterial.SetFloat(ShaderParameters.ColorShift, 1f);

            mapGenerationData.Value.currentRegionHighlightMaterial.shaderKeywords = null;//= new string[] { ShaderParameters.SKW_HIGHLIGHT_TINT_BACKGROUND };
            mapGenerationData.Value.currentRegionHighlightMaterial.SetFloat(ShaderParameters.ColorShift, 1f);


            RegionsData.needRefreshRouteMatrix = true;
            for (int a = 0; a < RegionsData.needRefreshPathMatrix.Length; a++)
            {
                RegionsData.needRefreshPathMatrix[a] = true;
            }
        }

        void MapGenerateHexasphere()
        {
            //���������� ������� ������������ ���������
            DHexaspherePoint[] corners = new DHexaspherePoint[]
            {
                new DHexaspherePoint(1, MapGenerationData.PHI, 0),
                new DHexaspherePoint(-1, MapGenerationData.PHI, 0),
                new DHexaspherePoint(1, -MapGenerationData.PHI, 0),
                new DHexaspherePoint(-1, -MapGenerationData.PHI, 0),
                new DHexaspherePoint(0, 1, MapGenerationData.PHI),
                new DHexaspherePoint(0, -1, MapGenerationData.PHI),
                new DHexaspherePoint(0, 1, -MapGenerationData.PHI),
                new DHexaspherePoint(0, -1, -MapGenerationData.PHI),
                new DHexaspherePoint(MapGenerationData.PHI, 0, 1),
                new DHexaspherePoint(-MapGenerationData.PHI, 0, 1),
                new DHexaspherePoint(MapGenerationData.PHI, 0, -1),
                new DHexaspherePoint(-MapGenerationData.PHI, 0, -1)
            };

            //���������� ������������ ������������ ���������
            DHexasphereTriangle[] triangles = new DHexasphereTriangle[]
            {
                new DHexasphereTriangle(corners [0], corners [1], corners [4], false),
                new DHexasphereTriangle(corners [1], corners [9], corners [4], false),
                new DHexasphereTriangle(corners [4], corners [9], corners [5], false),
                new DHexasphereTriangle(corners [5], corners [9], corners [3], false),
                new DHexasphereTriangle(corners [2], corners [3], corners [7], false),
                new DHexasphereTriangle(corners [3], corners [2], corners [5], false),
                new DHexasphereTriangle(corners [7], corners [10], corners [2], false),
                new DHexasphereTriangle(corners [0], corners [8], corners [10], false),
                new DHexasphereTriangle(corners [0], corners [4], corners [8], false),
                new DHexasphereTriangle(corners [8], corners [2], corners [10], false),
                new DHexasphereTriangle(corners [8], corners [4], corners [5], false),
                new DHexasphereTriangle(corners [8], corners [5], corners [2], false),
                new DHexasphereTriangle(corners [1], corners [0], corners [6], false),
                new DHexasphereTriangle(corners [11], corners [1], corners [6], false),
                new DHexasphereTriangle(corners [3], corners [9], corners [11], false),
                new DHexasphereTriangle(corners [6], corners [10], corners [7], false),
                new DHexasphereTriangle(corners [3], corners [11], corners [7], false),
                new DHexasphereTriangle(corners [11], corners [6], corners [7], false),
                new DHexasphereTriangle(corners [6], corners [0], corners [10], false),
                new DHexasphereTriangle(corners [9], corners [1], corners [11], false)
            };

            //������� ������� �����
            MapGenerationData.points.Clear();

            //������� ������� ��������� � �������
            for (int a = 0; a < corners.Length; a++)
            {
                MapGenerationData.points[corners[a]] = corners[a];
            }

            //������ ������ ����� ������� ����� ������������
            List<DHexaspherePoint> bottom = new();
            //���������� ���������� �������������
            int triangleCount = triangles.Length;
            //��� ������� ������������
            for (int f = 0; f < triangleCount; f++)
            {
                //������ ������ ������ �����
                List<DHexaspherePoint> previous = null;

                //���� ������ ������� ������������
                DHexaspherePoint point0 = triangles[f].points[0];

                //������� ��������� ������ �����
                bottom.Clear();

                //������� � ������ ������ ������� ������������
                bottom.Add(point0);

                //������ ������ ����� ������ ����� ������������
                List<DHexaspherePoint> left = PointSubdivide(
                    point0, triangles[f].points[1], 
                    mapGenerationData.Value.subdivisions);
                //������ ������ ����� ������� ����� ������������
                List<DHexaspherePoint> right = PointSubdivide(
                    point0, triangles[f].points[2],
                    mapGenerationData.Value.subdivisions);

                //��� ������� �������������
                for (int i = 1; i <= mapGenerationData.Value.subdivisions; i++)
                {
                    //��������� ������ ����� ������� �����
                    previous = bottom;

                    //������������ ��������� � ������ ����� �� �������
                    bottom = PointSubdivide(
                        left[i], right[i],
                        i);

                    //������ ����� �����������
                    new DHexasphereTriangle(previous[0], bottom[0], bottom[1]);

                    //��� ������� ...
                    for (int j = 1; j < i; j++)
                    {
                        //������ ��� ����� ������������
                        new DHexasphereTriangle(previous[j], bottom[j], bottom[j + 1]);
                        new DHexasphereTriangle(previous[j - 1], previous[j], bottom[j]);
                    }
                }
            }

            //���������� ���������� �����
            int meshPointCount = MapGenerationData.points.Values.Count;

            //������ �������
            //���� ������ ������� �������
            int regionIndex = 0;
            //�������� ���� �����
            DHexaspherePoint.flag = 0;

            //���������� ������ ������� ��������
            RegionsData.regionPEs = new EcsPackedEntity[meshPointCount];

            //������ ������������ ������ ��� GO ��������
            Transform regionsRoot = MapCreateGOAndParent(
                sceneData.Value.coreObject,
                MapGenerationData.regionsRootName).transform;

            //��� ������ ������� � �������
            foreach (DHexaspherePoint point in MapGenerationData.points.Values)
            {
                //������ ������
                RegionCreate(
                    regionsRoot,
                    point,
                    regionIndex);

                //����������� ������
                regionIndex++;
            }

            //��� ������� �������
            for (int a = 0; a < RegionsData.regionPEs.Length; a++)
            {
                //���� ������
                RegionsData.regionPEs[a].Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //������������ ������� �������

                //������� ��������� ������ �������
                CHexRegion.tempNeighbours.Clear();
                //��� ������� ������������ � ������ ������ �������
                for (int b = 0; b < region.centerPoint.triangleCount; b++)
                {
                    //���� �����������
                    DHexasphereTriangle triangle = region.centerPoint.triangles[b];

                    //��� ������ ������� ������������
                    for (int c = 0; c < 3; c++)
                    {
                        //���� ������ �������
                        triangle.points[c].regionPE.Unpack(world.Value, out int neighbourRegionEntity);
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //���� ��� �� ������� ������ � ��������� ������ ��� �� �������� ���
                        if (neighbourRegion.Index != region.Index && CHexRegion.tempNeighbours.Contains(neighbourRegion.selfPE) == false)
                        {
                            //������� PE ������ � ������
                            CHexRegion.tempNeighbours.Add(neighbourRegion.selfPE);
                        }
                    }
                }

                //������� ������� � ������ �������
                region.neighbourRegionPEs = CHexRegion.tempNeighbours.ToArray();
            }
        }

        void MapGenerateTerrainClimate()
        {
            //��� ������� �������
            foreach (int regionEntity in regionFilter.Value)
            {
                //���� ������
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //������������� ������� ����
                region.WaterLevel = mapGenerationData.Value.waterLevel;
            }

            //������ ������ ��� ������
            DPathFindingNodeFast[] regions = new DPathFindingNodeFast[RegionsData.regionPEs.Length];

            //������ ������� ��� ������
            PathFindingQueueInt queue = new PathFindingQueueInt(
                new PathFindingNodesComparer(regions),
                RegionsData.regionPEs.Length);

            //������ ����
            MapCreateLand(queue, ref regions);

            //�������� ������
            MapErodeLand();

            //������ ������
            MapCreateClimate();

            //������������� ���� ���������
            MapSetTerrainType();
        }

        void MapCreateLand(
            PathFindingQueueInt queue, ref DPathFindingNodeFast[] regions)
        {
            //���������� ������ ����
            int landBudget = Mathf.RoundToInt(
                RegionsData.regionPEs.Length * mapGenerationData.Value.landPercentage * 0.01f);

            //���� ������ ���� ������ ���� � ������ ����� 10000 ��������
            for (int a = 0; a < 25000; a++)
            {
                //����������, ����� �� ������� ����
                bool sink = Random.value < mapGenerationData.Value.sinkProbability;

                //���������� ������ �����
                int chunkSize = Random.Range(mapGenerationData.Value.chunkSizeMin, mapGenerationData.Value.chunkSizeMax + 1);

                //���� ��������� ������� ����
                if (sink == true)
                {
                    //����� ����
                    MapSinkTerrain(
                        queue, ref regions,
                        chunkSize,
                        landBudget,
                        out landBudget);
                }
                //�����
                else
                {
                    //��������� ����
                    MapRaiseTerrain(
                        queue, ref regions,
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

            //���� ������ ���� ������ ����
            if (landBudget > 0)
            {
                Debug.LogWarning("Failed to use up " + landBudget + " land budget.");
            }
        }

        void MapRaiseTerrain(
            PathFindingQueueInt queue, ref DPathFindingNodeFast[] regions,
            int chunkSize,
            int landBudget,
            out int currentLandBudget)
        {
            //���� ���� ������ ������ 250
            if (RegionsData.openRegionValue > 250)
            {
                //�������� ����
                RegionsData.openRegionValue = 1;
                RegionsData.closeRegionValue = 2;
            }
            //�����
            else
            {
                //��������� ����
                RegionsData.openRegionValue += 2;
                RegionsData.closeRegionValue += 2;
            }
            //������� �������
            queue.Clear();

            //���� ��������� ������
            RegionsData.GetRegionRandom().Unpack(world.Value, out int firstRegionEntity);
            ref CHexRegion firstRegion = ref regionPool.Value.Get(firstRegionEntity);

            //������������� ��� ���� ������ �� �������
            regions[firstRegion.Index].distance = 0;
            regions[firstRegion.Index].priority = 2;
            regions[firstRegion.Index].status = RegionsData.openRegionValue;

            //������� ������ � �������
            queue.Push(firstRegion.Index);

            //����������, ����� ��� ������� ��������� ���� ��� �������
            int rise = Random.value < mapGenerationData.Value.highRiseProbability ? 2 : 1;
            //������ ������� �������
            int currentSize = 0;

            //���� ������� ������ ���������� ������� � ������� �� �����
            while (currentSize < chunkSize && queue.regionsCount > 0)
            {
                //���� ������ ������ � �������
                int currentRegionIndex = queue.Pop();
                RegionsData.regionPEs[currentRegionIndex].Unpack(world.Value, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                //���������� �������� ������ �������
                int originalElevation = currentRegion.Elevation;
                //���������� ����� ������ �������
                int newElevation = originalElevation + rise;

                //���� ����� ������ ������ ������������
                if (newElevation > mapGenerationData.Value.elevationMaximum)
                {
                    continue;
                }

                //����������� ������ �������
                currentRegion.Elevation = newElevation;
                currentRegion.ExtrudeAmount = (float)currentRegion.Elevation / (mapGenerationData.Value.elevationMaximum + 1);

                //���� �������� ������ ������� ������ ������ ����
                if (originalElevation < mapGenerationData.Value.waterLevel
                    //� ����� ������ ������ ��� ����� ������ ����
                    && newElevation >= mapGenerationData.Value.waterLevel
                    //� ����������� ������ ���� ����� ����
                    && --landBudget == 0)
                {
                    break;
                }

                //����������� �������
                currentSize += 1;

                //��� ������� ������ �������� �������
                for (int i = 0; i < currentRegion.neighbourRegionPEs.Length; i++)
                {
                    //���� ������
                    currentRegion.neighbourRegionPEs[i].Unpack(world.Value, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //���� ���� ������ ������ ������ �������
                    if (regions[neighbourRegion.Index].status < RegionsData.openRegionValue)
                    {
                        //������������� ���� ������ �� �������
                        regions[neighbourRegion.Index].status = RegionsData.openRegionValue;
                        regions[neighbourRegion.Index].distance = 0;
                        regions[neighbourRegion.Index].priority = Vector3.Angle(firstRegion.center, neighbourRegion.center) * 2f;
                        regions[neighbourRegion.Index].priority += Random.value < mapGenerationData.Value.jitterProbability ? 1 : 0;

                        //������� ������ � �������
                        queue.Push(neighbourRegion.Index);
                    }
                }
            }

            //���������� ������ ����
            currentLandBudget = landBudget;
        }

        void MapSinkTerrain(
            PathFindingQueueInt queue, ref DPathFindingNodeFast[] regions,
            int chunkSize,
            int landBudget,
            out int currentLandBudget)
        {
            //���� ���� ������ ������ 250
            if (RegionsData.openRegionValue > 250)
            {
                //�������� ����
                RegionsData.openRegionValue = 1;
                RegionsData.closeRegionValue = 2;
            }
            //�����
            else
            {
                //��������� ����
                RegionsData.openRegionValue += 2;
                RegionsData.closeRegionValue += 2;
            }
            //������� �������
            queue.Clear();

            //���� ��������� ������
            RegionsData.GetRegionRandom().Unpack(world.Value, out int firstRegionEntity);
            ref CHexRegion firstRegion = ref regionPool.Value.Get(firstRegionEntity);

            //������������� ��� ���� ������ �� �������
            regions[firstRegion.Index].distance = 0;
            regions[firstRegion.Index].priority = 2;
            regions[firstRegion.Index].status = RegionsData.openRegionValue;

            //������� ������ � �������
            queue.Push(firstRegion.Index);

            //����������, ����� ��� �������� ���������� ���� ��� �������
            int sink = Random.value < mapGenerationData.Value.highRiseProbability ? 2 : 1;
            //������ ������� �������
            int currentSize = 0;

            //���� ������� ������ ���������� ������� � ������� �� �����
            while (currentSize < chunkSize && queue.regionsCount > 0)
            {
                //���� ������ ������ � �������
                int currentRegionIndex = queue.Pop();
                RegionsData.regionPEs[currentRegionIndex].Unpack(world.Value, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                //���������� �������� ������ �������
                int originalElevation = currentRegion.Elevation;
                //���������� ����� ������ �������
                int newElevation = currentRegion.Elevation - sink;

                //���� ����� ������ ������� ������ �����������
                if (newElevation < mapGenerationData.Value.elevationMinimum)
                {
                    continue;
                }

                //����������� ������ �������
                currentRegion.Elevation = newElevation;

                if (currentRegion.Elevation > 0)
                {
                    currentRegion.ExtrudeAmount = (float)currentRegion.Elevation / (mapGenerationData.Value.elevationMaximum + 1);
                }
                else
                {
                    currentRegion.ExtrudeAmount = 0;
                }

                //���� �������� ������ ������� ������ ��� ����� ������ ����
                if (originalElevation >= mapGenerationData.Value.waterLevel
                    //� ����� ������ ������ ������ ����
                    && newElevation < mapGenerationData.Value.waterLevel)
                {
                    //����������� ������ ����
                    landBudget += 1;
                }

                //����������� �������
                currentSize += 1;

                //��� ������� ������ �������� �������
                for (int i = 0; i < currentRegion.neighbourRegionPEs.Length; i++)
                {
                    //���� ������
                    currentRegion.neighbourRegionPEs[i].Unpack(world.Value, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //���� ���� ������ ������ ������ �������
                    if (regions[neighbourRegion.Index].status < RegionsData.openRegionValue)
                    {
                        //������������� ���� ������ �� �������
                        regions[neighbourRegion.Index].status = RegionsData.openRegionValue;
                        regions[neighbourRegion.Index].distance = 0;
                        regions[neighbourRegion.Index].priority = Vector3.Angle(firstRegion.center, neighbourRegion.center) * 2f;
                        regions[neighbourRegion.Index].priority += Random.value < mapGenerationData.Value.jitterProbability ? 1 : 0;

                        //������� ������ � �������
                        queue.Push(neighbourRegion.Index);
                    }
                }
            }

            //���������� ������ ����
            currentLandBudget = landBudget;
        }

        void MapErodeLand()
        {
            //������ ������ ��������� ��������, ���������� ������
            List<int> erodibleRegions = ListPool<int>.Get();

            //��� ������� �������
            foreach (int regionEntity in regionFilter.Value)
            {
                //���� ������
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //���� ������ �������� ������, ������� ��� � ������
                if (RegionIsErodible(ref region) == true)
                {
                    erodibleRegions.Add(regionEntity);
                }
            }

            //����������, ������� �������� ������ ������������� ������
            int erodibleRegionsCount = (int)(erodibleRegions.Count * (100 - mapGenerationData.Value.erosionPercentage) * 0.01f);

            //���� ������ ��������� �������� ������ ���������� �����
            while (erodibleRegions.Count > erodibleRegionsCount)
            {
                //�������� ��������� ������ � ������
                int index = Random.Range(0, erodibleRegions.Count);
                ref CHexRegion region = ref regionPool.Value.Get(erodibleRegions[index]);

                //�������� ������ ������ ��� ���� ������
                int targetRegionEntity = RegionGetErosionTarget(ref region);
                ref CHexRegion targetRegion = ref regionPool.Value.Get(targetRegionEntity);

                //��������� ������ ��������� ������� � ����������� ������ ��������
                region.Elevation -= 1;
                if (region.Elevation > 0)
                {
                    region.ExtrudeAmount = (float)region.Elevation / (mapGenerationData.Value.elevationMaximum + 1);
                }
                else
                {
                    region.ExtrudeAmount = 0;
                }
                targetRegion.Elevation += 1;
                if (targetRegion.Elevation > 0)
                {
                    targetRegion.ExtrudeAmount = (float)targetRegion.Elevation / (mapGenerationData.Value.elevationMaximum + 1);
                }
                else
                {
                    targetRegion.ExtrudeAmount = 0;
                }

                //���� ������ ������ �� �������� ������
                if (RegionIsErodible(ref region) == false)
                {
                    //������� ��� �������� �� ������, ������� ��������� �������� � ������
                    erodibleRegions[index] = erodibleRegions[erodibleRegions.Count - 1];
                    erodibleRegions.RemoveAt(erodibleRegions.Count - 1);
                }

                //��� ������� ������
                for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
                {
                    //���� ������
                    region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //���� ������ ������ �� 2 ������ ������������ �������
                    if (neighbourRegion.Elevation == region.Elevation + 2
                        //���� ����� �������� ������
                        && RegionIsErodible(ref neighbourRegion) == true
                        //� ���� ������ �� �������� �������� ������
                        && erodibleRegions.Contains(neighbourRegionEntity) == false)
                    {
                        //������� ������ � ������
                        erodibleRegions.Add(neighbourRegionEntity);
                    }
                }

                //���� ������� ������ �������� ������
                if (RegionIsErodible(ref targetRegion)
                    //� ������ ��������� ���������� ������ �������� �� �������� ���
                    && erodibleRegions.Contains(targetRegionEntity) == false)
                {
                    //������� ������� ������ � ������
                    erodibleRegions.Add(targetRegionEntity);
                }

                //��� ������� ������
                for (int a = 0; a < targetRegion.neighbourRegionPEs.Length; a++)
                {
                    //���� ����� ����������
                    if (targetRegion.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity)
                        //� ���� ����� - �� �������� ����������� ������
                        && region.selfPE.EqualsTo(targetRegion.neighbourRegionPEs[a]) == false)
                    {
                        //���� ������
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //���� ������ ������ �� ������� ������, ��� ������ �������� �������
                        if (neighbourRegion.Elevation == targetRegion.Elevation + 1
                            //� ���� ����� �� �������� ������
                            && RegionIsErodible(ref neighbourRegion) == false
                            //� ���� ������ �������� �������� ������
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

        void MapCreateClimate()
        {
            //��������� ���
            for (int a = 0; a < 40; a++)
            {
                //��� ������� �������
                foreach (int regionEntity in regionFilter.Value)
                {
                    //���� ������ � ������ ���������
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                    ref CHexRegionGenerationData regionGenerationData = ref regionGenerationDataPool.Value.Get(regionEntity);

                    //������������ ������������� ������
                    RegionEvolveClimate(
                        ref region,
                        ref regionGenerationData);
                }

                //��� ������� �������
                foreach (int regionEntity in regionFilter.Value)
                {
                    //���� ������ ���������
                    ref CHexRegionGenerationData regionGenerationData = ref regionGenerationDataPool.Value.Get(regionEntity);

                    //��������� ��������� ������ ������� � �������, ����� ��������� �������� � �������� �� ���������
                    regionGenerationData.currentClimate = regionGenerationData.nextClimate;
                    regionGenerationData.nextClimate = new();
                }
            }
        }

        void MapSetTerrainType()
        {
            //������������ ������, ����������� ��� ��������� ���������� �������
            int rockDesertElevation = mapGenerationData.Value.elevationMaximum
                - (mapGenerationData.Value.elevationMaximum - mapGenerationData.Value.waterLevel) / 2;

            //��� ������� �������
            foreach (int regionEntity in regionFilter.Value)
            {
                //���� ������ � ������ ���������
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                ref CHexRegionGenerationData regionGenerationData = ref regionGenerationDataPool.Value.Get(regionEntity);

                //���� ������ �������
                ref DHexRegionClimate regionClimate = ref regionGenerationData.currentClimate;

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
                    else if (region.Elevation == mapGenerationData.Value.elevationMaximum)
                    {
                        //������������� ��� ��������� ������� �� ����
                        region.TerrainTypeIndex = 4;
                    }
                    //�����
                    else
                    {
                        //������������� ��� ��������� �������
                        region.TerrainTypeIndex = biome.terrainTypeIndex;
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

                        //��� ������� ������
                        for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
                        {
                            //���� ������
                            region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
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

                        //���� ����� ������� � ������� ������ ���
                        if (cliffs + slopes > 3)
                        {
                            //��� ��������� - �����
                            terrainTypeIndex = 1;
                        }
                        //�����, ���� ������� ������ ����
                        else if (cliffs > 0)
                        {
                            //��� ��������� - ������
                            terrainTypeIndex = 3;
                        }
                        //�����, ���� ������� ������ ����
                        else if (slopes > 0)
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
                    else if (region.Elevation >= mapGenerationData.Value.waterLevel)
                    {
                        //��� ��������� - �����
                        terrainTypeIndex = 1;
                    }
                    //�����, ���� ������ ������� ������������
                    else if (region.Elevation < 0)
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

        List<DHexaspherePoint> PointSubdivide(
            DHexaspherePoint startPoint, DHexaspherePoint endPoint,
            int count)
        {
            //������ ������ �����, ������������ �������� �����, � ������� � ���� ������� �������
            List<DHexaspherePoint> segments = new List<DHexaspherePoint>(count + 1);
            segments.Add(startPoint);

            //������������ ���������� �����
            double dx = endPoint.x - startPoint.x;
            double dy = endPoint.y - startPoint.y;
            double dz = endPoint.z - startPoint.z;
            double doublex = (double)startPoint.x;
            double doubley = (double)startPoint.y;
            double doublez = (double)startPoint.z;
            double doubleCount = (double)count;

            //��� ������� �������������
            for (int a = 1; a < count; a++)
            {
                //������ ����� �������
                DHexaspherePoint newPoint = new(
                    (float)(doublex + dx * (double)a / doubleCount),
                    (float)(doubley + dy * (double)a / doubleCount),
                    (float)(doublez + dz * (double)a / doubleCount));

                //��������� �������
                newPoint = PointGetCached(newPoint);

                //������� ������� � ������
                segments.Add(newPoint);
            }

            //������� � ������ �������� �������
            segments.Add(endPoint);

            //���������� ������ ������
            return segments;
        }

        DHexaspherePoint PointGetCached(
            DHexaspherePoint point)
        {
            DHexaspherePoint thePoint;

            //���� ����������� ������� ���������� � �������
            if (MapGenerationData.points.TryGetValue(point, out thePoint))
            {
                //���������� �������
                return thePoint;
            }
            //�����
            else
            {
                //��������� ������� � �������
                MapGenerationData.points[point] = point;

                //���������� �������
                return point;
            }
        }

        void RegionCreate(
            Transform parentGO,
            DHexaspherePoint centerPoint,
            int regionIndex)
        {
            //������ ����� �������� � ��������� �� ���������� �������, RAEO � ������ ���������
            int regionEntity = world.Value.NewEntity();
            ref CHexRegion currentRegion = ref regionPool.Value.Add(regionEntity);
            ref CRegionAEO currentRAEO = ref regionAEOPool.Value.Add(regionEntity);
            ref CHexRegionGenerationData currentRegionGenerationData = ref regionGenerationDataPool.Value.Add(regionEntity);

            //��������� �������� ������ �������
            currentRegion = new(
                world.Value.PackEntity(regionEntity), regionIndex,
                centerPoint);

            //������� ������ � ������ ��������
            RegionsData.regionPEs[regionIndex] = currentRegion.selfPE;

            //������ GO ������� � ���
            GORegionPrefab regionGO = GameObject.Instantiate(staticData.Value.regionPrefab);

            //��������� �������� ������ GO
            regionGO.gameObject.layer = parentGO.gameObject.layer;
            regionGO.transform.SetParent(parentGO, false);
            regionGO.transform.localPosition = Vector3.zero;
            regionGO.transform.localScale = Vector3.one;
            regionGO.transform.localRotation = Quaternion.Euler(0, 0, 0);

            //������ ��������� �������
            currentRegion.fleetRenderer = RegionCreateRenderer(ref currentRegion, regionGO.fleetRenderer);
            currentRegion.hoverRenderer = RegionCreateRenderer(ref currentRegion, regionGO.hoverRenderer);
            currentRegion.currentRenderer = RegionCreateRenderer(ref currentRegion, regionGO.currentRenderer);

            //��������� �������� ������ RAEO
            currentRAEO = new(currentRegion.selfPE);

            //��������� �������� ������ ���������
            currentRegionGenerationData = new(new(0, mapGenerationData.Value.startingMoisture), new(0, 0));
        }

        MeshRenderer RegionCreateRenderer(
            ref CHexRegion region, MeshRenderer renderer)
        {
            //������ ����� ��������� � ���
            MeshFilter meshFilter = renderer.gameObject.AddComponent<MeshFilter>();
            Mesh mesh = new();
            mesh.hideFlags = HideFlags.DontSave;

            //���������� ������ ���������
            float extrusionAmount = region.ExtrudeAmount * MapGenerationData.extrudeMultiplier;

            //������������ ����������� ������� �������
            Vector3[] extrudedVertices = new Vector3[region.vertexPoints.Length];
            //��� ������ �������
            for (int a = 0; a < region.vertices.Length; a++)
            {
                //������������ ��������� ����������� �������
                extrudedVertices[a] = region.vertices[a] * (1f + extrusionAmount);
            }
            //��������� ������� ����
            mesh.vertices = extrudedVertices;

            //���� � ������� ����� ������
            if (region.vertices.Length == 6)
            {
                mesh.SetIndices(
                    MapGenerationData.hexagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = MapGenerationData.hexagonUVs;
            }
            //�����
            else
            {
                mesh.SetIndices(
                    MapGenerationData.pentagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = MapGenerationData.pentagonUVs;
            }

            //������������ ������� ����
            mesh.normals = region.vertices;
            mesh.RecalculateNormals();

            //��������� ��� ��������� � ��������� ������������
            meshFilter.sharedMesh = mesh;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.enabled = false;

            //���������� ��������
            return renderer;
        }

        bool RegionIsErodible(
            ref CHexRegion region)
        {
            //���������� ������, ��� ������� ��������� ������
            int erodibleElevation = region.Elevation - 2;

            //��� ������� ������ 
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //���� ������
                region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                //���� ������ ������ ������ ��� ����� ������ ������
                if (neighbourRegion.Elevation <= erodibleElevation)
                {
                    //����������, ��� ������ ��������
                    return true;
                }
            }

            //����������, ��� ������ ����������
            return false;
        }

        int RegionGetErosionTarget(
            ref CHexRegion region)
        {
            //������ ������ ���������� ����� ������
            List<int> candidateEntities = ListPool<int>.Get();

            //���������� ������, ��� ������� ��������� ������
            int erodibleElevation = region.Elevation - 2;

            //��� ������� ������
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //���� ������
                region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                //���� ������ ������ ������ ��� ����� ������ ������
                if (neighbourRegion.Elevation <= erodibleElevation)
                {
                    //������� ������ � ������
                    candidateEntities.Add(neighbourRegionEntity);
                }
            }

            //�������� �������� ������ �� ������
            int targetRegionEntity = candidateEntities[Random.Range(0, candidateEntities.Count)];

            //������� ������ � ���
            ListPool<int>.Add(candidateEntities);

            //���������� �������� �������� �������
            return targetRegionEntity;
        }

        void RegionEvolveClimate(
            ref CHexRegion region,
            ref CHexRegionGenerationData regionGenerationData)
        {
            //���� ������ �������
            ref DHexRegionClimate regionClimate = ref regionGenerationData.currentClimate;

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
            //HexDirection mainDispersalDirection = mapGenerationData.Value.windDirection.Opposite();
            //������������, ������� ������� ���������
            float cloudDispersal = regionClimate.clouds * (1f / (5f + mapGenerationData.Value.windStrength));

            //������������, ������� ������� � ������� ����
            float runoff = regionClimate.moisture * mapGenerationData.Value.runoffFactor * (1f / 6f);
            //������������, ������� ����� �������������
            float seepage = regionClimate.moisture * mapGenerationData.Value.seepageFactor * (1f / 6f);

            //��� ������� ������
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //���� ������ � ��������� ������ �������
                region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);
                ref CHexRegionGenerationData neighbourRegionGenerationData = ref regionGenerationDataPool.Value.Get(neighbourRegionEntity);
                ref DHexRegionClimate neighbourRegionClimate = ref neighbourRegionGenerationData.nextClimate;

                //���� ����������� ����� �������� ����������� �����
                /*if (d == mainDispersalDirection)
                {
                    //������������ ����� �������, ����������� ����� �����
                    neighbourRegionClimate.clouds += cloudDispersal * mapGenerationData.Value.windStrength;
                }
                //�����
                else
                {*/
                    //������������ ������� ����� �������
                    neighbourRegionClimate.clouds += cloudDispersal;
                //}

                //������������ ������� � ������ ����� ���������
                int elevationDelta = neighbourRegion.ViewElevation - region.ViewElevation;
                //���� ������� ������ ����
                if (elevationDelta < 0)
                {
                    //���� ������ � ������
                    neighbourRegionClimate.moisture -= runoff;
                    neighbourRegionClimate.moisture += runoff;
                }
                //�����, ���� ������� ����� ����
                else if (elevationDelta == 0)
                {
                    //������������ ������ � �������
                    neighbourRegionClimate.moisture -= seepage;
                    neighbourRegionClimate.moisture += seepage;
                }
            }

            //���� ��������� ������ �������
            ref DHexRegionClimate regionNextClimate = ref regionGenerationData.nextClimate;

            //��������� ��������� �� ��������
            regionNextClimate.moisture = regionClimate.moisture;

            //���� ������� ��������� ������ �������
            if (regionNextClimate.moisture > 1f)
            {
                //������������� � �� �������
                regionNextClimate.moisture = 1f;
            }
        }

        float RegionDetermineTemperature(
            ref CHexRegion region)
        {
            //������������ ������ �������
            float latitude = (float)region.centerPoint.z / 1f;
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
            //temperature += (MapGenerationData.SampleNoise(region.Position * 0.1f).w * 2f - 1f) * mapGenerationData.Value.temperatureJitter;

            //���������� �����������
            return temperature;
        }
    }
}