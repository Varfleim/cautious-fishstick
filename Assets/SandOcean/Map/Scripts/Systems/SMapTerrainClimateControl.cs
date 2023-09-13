
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
        //Миры
        readonly EcsWorldInject world = default;


        //Карта
        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        readonly EcsPoolInject<CHexRegionGenerationData> regionGenerationDataPool = default;

        //Административно-экономические объекты
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;


        //События карты
        readonly EcsFilterInject<Inc<RMapGeneration>> mapGenerationRequestFilter = default;
        readonly EcsPoolInject<RMapGeneration> mapGenerationRequestPool = default;


        //Данные
        readonly EcsCustomInject<StaticData> staticData = default;
        readonly EcsCustomInject<SceneData> sceneData = default;
        readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса генерации карты
            foreach (int mapGenerationRequestEntity in mapGenerationRequestFilter.Value)
            {
                //Берём запрос
                ref RMapGeneration mapGenerationRequest = ref mapGenerationRequestPool.Value.Get(mapGenerationRequestEntity);

                //Создаём карту
                MapCreate(ref mapGenerationRequest);

                world.Value.DelEntity(mapGenerationRequestEntity);
            }
        }

        void MapCreate(
            ref RMapGeneration mapGenerationRequest)
        {
            //Инициализируем карту
            MapInitialization(ref mapGenerationRequest);


            //Генерируем пустую гексасферу
            MapGenerateHexasphere();

            //Отмечаем, что требуется стартовое обновление
            mapGenerationData.Value.isInitializationUpdate = true;

            //Отмечаем, что требуется обновить регионы, цвета, UV-координаты и массивы текстур
            mapGenerationData.Value.isMaterialUpdated = true;
            mapGenerationData.Value.isRegionUpdated = true;
            mapGenerationData.Value.isColorUpdated = true;
            mapGenerationData.Value.isUVUpdatedFast = true;
            //mapGenerationData.Value.isTextureArrayUpdated = true;


            //Генерируем ландшафт и климат
            MapGenerateTerrainClimate();
        }

        void MapInitialization(
            ref RMapGeneration mapGenerationRequest)
        {
            //Ограничиваем число подразделений
            mapGenerationData.Value.subdivisions = Mathf.Max(1, mapGenerationRequest.subdivisions);

            //Устанавливаем множитель выдавленности
            MapGenerationData.extrudeMultiplier = 0.05f;

            //Устанавливаем масштаб гексасферы
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
            //Определяем вершины изначального икосаэдра
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

            //Определяем треугольники изначального икосаэдра
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

            //Очищаем словарь точек
            MapGenerationData.points.Clear();

            //Заносим вершины икосаэдра в словарь
            for (int a = 0; a < corners.Length; a++)
            {
                MapGenerationData.points[corners[a]] = corners[a];
            }

            //Создаём список точек нижнего ребра треугольника
            List<DHexaspherePoint> bottom = new();
            //Определяем количество треугольников
            int triangleCount = triangles.Length;
            //Для каждого треугольника
            for (int f = 0; f < triangleCount; f++)
            {
                //Создаём пустой список точек
                List<DHexaspherePoint> previous = null;

                //Берём первую вершину треугольника
                DHexaspherePoint point0 = triangles[f].points[0];

                //Очищаем временный список точек
                bottom.Clear();

                //Заносим в список первую вершину треугольника
                bottom.Add(point0);

                //Создаём список точек левого ребра треугольника
                List<DHexaspherePoint> left = PointSubdivide(
                    point0, triangles[f].points[1], 
                    mapGenerationData.Value.subdivisions);
                //Создаём список точек правого ребра треугольника
                List<DHexaspherePoint> right = PointSubdivide(
                    point0, triangles[f].points[2],
                    mapGenerationData.Value.subdivisions);

                //Для каждого подразделения
                for (int i = 1; i <= mapGenerationData.Value.subdivisions; i++)
                {
                    //Переносим список точек нижнего ребра
                    previous = bottom;

                    //Подразделяем перемычку с левого ребра до правого
                    bottom = PointSubdivide(
                        left[i], right[i],
                        i);

                    //Создаём новый треугольник
                    new DHexasphereTriangle(previous[0], bottom[0], bottom[1]);

                    //Для каждого ...
                    for (int j = 1; j < i; j++)
                    {
                        //Создаём два новых треугольника
                        new DHexasphereTriangle(previous[j], bottom[j], bottom[j + 1]);
                        new DHexasphereTriangle(previous[j - 1], previous[j], bottom[j]);
                    }
                }
            }

            //Определяем количество точек
            int meshPointCount = MapGenerationData.points.Values.Count;

            //Создаём регионы
            //Берём индекс первого региона
            int regionIndex = 0;
            //Обнуляем флаг точек
            DHexaspherePoint.flag = 0;

            //Определяем размер массива регионов
            RegionsData.regionPEs = new EcsPackedEntity[meshPointCount];

            //Создаём родительский объект для GO регионов
            Transform regionsRoot = MapCreateGOAndParent(
                sceneData.Value.coreObject,
                MapGenerationData.regionsRootName).transform;

            //Для каждой вершины в словаре
            foreach (DHexaspherePoint point in MapGenerationData.points.Values)
            {
                //Создаём регион
                RegionCreate(
                    regionsRoot,
                    point,
                    regionIndex);

                //Увеличиваем индекс
                regionIndex++;
            }

            //Для каждого региона
            for (int a = 0; a < RegionsData.regionPEs.Length; a++)
            {
                //Берём регион
                RegionsData.regionPEs[a].Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Рассчитываем соседей региона

                //Очищаем временный список соседей
                CHexRegion.tempNeighbours.Clear();
                //Для каждого треугольника в данных центра региона
                for (int b = 0; b < region.centerPoint.triangleCount; b++)
                {
                    //Берём треугольник
                    DHexasphereTriangle triangle = region.centerPoint.triangles[b];

                    //Для каждой вершины треугольника
                    for (int c = 0; c < 3; c++)
                    {
                        //Берём регион вершины
                        triangle.points[c].regionPE.Unpack(world.Value, out int neighbourRegionEntity);
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //Если это не текущий регион и временный список ещё не содержит его
                        if (neighbourRegion.Index != region.Index && CHexRegion.tempNeighbours.Contains(neighbourRegion.selfPE) == false)
                        {
                            //Заносим PE соседа в список
                            CHexRegion.tempNeighbours.Add(neighbourRegion.selfPE);
                        }
                    }
                }

                //Заносим соседей в массив региона
                region.neighbourRegionPEs = CHexRegion.tempNeighbours.ToArray();
            }
        }

        void MapGenerateTerrainClimate()
        {
            //Для каждого региона
            foreach (int regionEntity in regionFilter.Value)
            {
                //Берём регион
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Устанавливаем уровень моря
                region.WaterLevel = mapGenerationData.Value.waterLevel;
            }

            //Создаём массив для поиска
            DPathFindingNodeFast[] regions = new DPathFindingNodeFast[RegionsData.regionPEs.Length];

            //Создаём очередь для поиска
            PathFindingQueueInt queue = new PathFindingQueueInt(
                new PathFindingNodesComparer(regions),
                RegionsData.regionPEs.Length);

            //Создаём сушу
            MapCreateLand(queue, ref regions);

            //Проводим эрозию
            MapErodeLand();

            //Создаём климат
            MapCreateClimate();

            //Устанавливаем типы ландшафта
            MapSetTerrainType();
        }

        void MapCreateLand(
            PathFindingQueueInt queue, ref DPathFindingNodeFast[] regions)
        {
            //Определяем бюджет суши
            int landBudget = Mathf.RoundToInt(
                RegionsData.regionPEs.Length * mapGenerationData.Value.landPercentage * 0.01f);

            //Пока бюджет суши больше нуля и прошло менее 10000 итераций
            for (int a = 0; a < 25000; a++)
            {
                //Определяем, нужно ли утопить сушу
                bool sink = Random.value < mapGenerationData.Value.sinkProbability;

                //Определяем размер чанка
                int chunkSize = Random.Range(mapGenerationData.Value.chunkSizeMin, mapGenerationData.Value.chunkSizeMax + 1);

                //Если требуется утопить сушу
                if (sink == true)
                {
                    //Топим сушу
                    MapSinkTerrain(
                        queue, ref regions,
                        chunkSize,
                        landBudget,
                        out landBudget);
                }
                //Иначе
                else
                {
                    //Поднимаем сушу
                    MapRaiseTerrain(
                        queue, ref regions,
                        chunkSize,
                        landBudget,
                        out landBudget);

                    //Если бюджет суши равен нулю
                    if (landBudget == 0)
                    {
                        return;
                    }
                }
            }

            //Если бюджет суши больше нуля
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
            //Если фаза поиска больше 250
            if (RegionsData.openRegionValue > 250)
            {
                //Обнуляем фазу
                RegionsData.openRegionValue = 1;
                RegionsData.closeRegionValue = 2;
            }
            //Иначе
            else
            {
                //Обновляем фазу
                RegionsData.openRegionValue += 2;
                RegionsData.closeRegionValue += 2;
            }
            //Очищаем очередь
            queue.Clear();

            //Берём случайный регион
            RegionsData.GetRegionRandom().Unpack(world.Value, out int firstRegionEntity);
            ref CHexRegion firstRegion = ref regionPool.Value.Get(firstRegionEntity);

            //Устанавливаем его фазу поиска на текущую
            regions[firstRegion.Index].distance = 0;
            regions[firstRegion.Index].priority = 2;
            regions[firstRegion.Index].status = RegionsData.openRegionValue;

            //Заносим регион в очередь
            queue.Push(firstRegion.Index);

            //Определяем, будет это высоким поднятием суши или обычным
            int rise = Random.value < mapGenerationData.Value.highRiseProbability ? 2 : 1;
            //Создаём счётчик размера
            int currentSize = 0;

            //Пока счётчик меньше требуемого размера и очередь не пуста
            while (currentSize < chunkSize && queue.regionsCount > 0)
            {
                //Берём первый регион в очереди
                int currentRegionIndex = queue.Pop();
                RegionsData.regionPEs[currentRegionIndex].Unpack(world.Value, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                //Запоминаем исходную высоту региона
                int originalElevation = currentRegion.Elevation;
                //Определяем новую высоту региона
                int newElevation = originalElevation + rise;

                //Если новая высота больше максимальной
                if (newElevation > mapGenerationData.Value.elevationMaximum)
                {
                    continue;
                }

                //Увеличиваем высоту региона
                currentRegion.Elevation = newElevation;
                currentRegion.ExtrudeAmount = (float)currentRegion.Elevation / (mapGenerationData.Value.elevationMaximum + 1);

                //Если исходная высота региона меньше уровня моря
                if (originalElevation < mapGenerationData.Value.waterLevel
                    //И новая высота больше или равна уровню моря
                    && newElevation >= mapGenerationData.Value.waterLevel
                    //И уменьшенный бюджет суши равен нулю
                    && --landBudget == 0)
                {
                    break;
                }

                //Увеличиваем счётчик
                currentSize += 1;

                //Для каждого соседа текущего региона
                for (int i = 0; i < currentRegion.neighbourRegionPEs.Length; i++)
                {
                    //Берём соседа
                    currentRegion.neighbourRegionPEs[i].Unpack(world.Value, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //Если фаза поиска соседа меньше текущей
                    if (regions[neighbourRegion.Index].status < RegionsData.openRegionValue)
                    {
                        //Устанавливаем фазу поиска на текущую
                        regions[neighbourRegion.Index].status = RegionsData.openRegionValue;
                        regions[neighbourRegion.Index].distance = 0;
                        regions[neighbourRegion.Index].priority = Vector3.Angle(firstRegion.center, neighbourRegion.center) * 2f;
                        regions[neighbourRegion.Index].priority += Random.value < mapGenerationData.Value.jitterProbability ? 1 : 0;

                        //Заносим регион в очередь
                        queue.Push(neighbourRegion.Index);
                    }
                }
            }

            //Возвращаем бюджет суши
            currentLandBudget = landBudget;
        }

        void MapSinkTerrain(
            PathFindingQueueInt queue, ref DPathFindingNodeFast[] regions,
            int chunkSize,
            int landBudget,
            out int currentLandBudget)
        {
            //Если фаза поиска больше 250
            if (RegionsData.openRegionValue > 250)
            {
                //Обнуляем фазу
                RegionsData.openRegionValue = 1;
                RegionsData.closeRegionValue = 2;
            }
            //Иначе
            else
            {
                //Обновляем фазу
                RegionsData.openRegionValue += 2;
                RegionsData.closeRegionValue += 2;
            }
            //Очищаем очередь
            queue.Clear();

            //Берём случайный регион
            RegionsData.GetRegionRandom().Unpack(world.Value, out int firstRegionEntity);
            ref CHexRegion firstRegion = ref regionPool.Value.Get(firstRegionEntity);

            //Устанавливаем его фазу поиска на текущую
            regions[firstRegion.Index].distance = 0;
            regions[firstRegion.Index].priority = 2;
            regions[firstRegion.Index].status = RegionsData.openRegionValue;

            //Заносим регион в очередь
            queue.Push(firstRegion.Index);

            //Определяем, будет это высотным утоплением суши или обычным
            int sink = Random.value < mapGenerationData.Value.highRiseProbability ? 2 : 1;
            //Создаём счётчик размера
            int currentSize = 0;

            //Пока счётчик меньше требуемого размера и очередь не пуста
            while (currentSize < chunkSize && queue.regionsCount > 0)
            {
                //Берём первый регион в очереди
                int currentRegionIndex = queue.Pop();
                RegionsData.regionPEs[currentRegionIndex].Unpack(world.Value, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                //Запоминаем исходную высоту региона
                int originalElevation = currentRegion.Elevation;
                //Определяем новую высоту региона
                int newElevation = currentRegion.Elevation - sink;

                //Если новая высота региона меньше минимальной
                if (newElevation < mapGenerationData.Value.elevationMinimum)
                {
                    continue;
                }

                //Увеличиваем высоту региона
                currentRegion.Elevation = newElevation;

                if (currentRegion.Elevation > 0)
                {
                    currentRegion.ExtrudeAmount = (float)currentRegion.Elevation / (mapGenerationData.Value.elevationMaximum + 1);
                }
                else
                {
                    currentRegion.ExtrudeAmount = 0;
                }

                //Если исходная высота региона больше или равна уровню моря
                if (originalElevation >= mapGenerationData.Value.waterLevel
                    //И новая высота меньше уровню моря
                    && newElevation < mapGenerationData.Value.waterLevel)
                {
                    //Увеличиваем бюджет суши
                    landBudget += 1;
                }

                //Увеличиваем счётчик
                currentSize += 1;

                //Для каждого соседа текущего региона
                for (int i = 0; i < currentRegion.neighbourRegionPEs.Length; i++)
                {
                    //Берём соседа
                    currentRegion.neighbourRegionPEs[i].Unpack(world.Value, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //Если фаза поиска соседа меньше текущей
                    if (regions[neighbourRegion.Index].status < RegionsData.openRegionValue)
                    {
                        //Устанавливаем фазу поиска на текущую
                        regions[neighbourRegion.Index].status = RegionsData.openRegionValue;
                        regions[neighbourRegion.Index].distance = 0;
                        regions[neighbourRegion.Index].priority = Vector3.Angle(firstRegion.center, neighbourRegion.center) * 2f;
                        regions[neighbourRegion.Index].priority += Random.value < mapGenerationData.Value.jitterProbability ? 1 : 0;

                        //Заносим регион в очередь
                        queue.Push(neighbourRegion.Index);
                    }
                }
            }

            //Возвращаем бюджет суши
            currentLandBudget = landBudget;
        }

        void MapErodeLand()
        {
            //Создаём список сущностей регионов, подлежащих эрозии
            List<int> erodibleRegions = ListPool<int>.Get();

            //Для каждого региона
            foreach (int regionEntity in regionFilter.Value)
            {
                //Берём регион
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Если регион подлежит эрозии, заносим его в список
                if (RegionIsErodible(ref region) == true)
                {
                    erodibleRegions.Add(regionEntity);
                }
            }

            //Определяем, сколько регионов должно подвергнуться эрозии
            int erodibleRegionsCount = (int)(erodibleRegions.Count * (100 - mapGenerationData.Value.erosionPercentage) * 0.01f);

            //Пока список сущностей регионов больше требуемого числа
            while (erodibleRegions.Count > erodibleRegionsCount)
            {
                //Выбираем случайный регион в списке
                int index = Random.Range(0, erodibleRegions.Count);
                ref CHexRegion region = ref regionPool.Value.Get(erodibleRegions[index]);

                //Выбираем одного соседа как цель эрозии
                int targetRegionEntity = RegionGetErosionTarget(ref region);
                ref CHexRegion targetRegion = ref regionPool.Value.Get(targetRegionEntity);

                //Уменьшаем высоту исходного региона и увеличиваем высоту целевого
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

                //Если регион больше не подлежит эрозии
                if (RegionIsErodible(ref region) == false)
                {
                    //Удаляем его сущность из списка, заменяя последним регионом в списке
                    erodibleRegions[index] = erodibleRegions[erodibleRegions.Count - 1];
                    erodibleRegions.RemoveAt(erodibleRegions.Count - 1);
                }

                //Для каждого соседа
                for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
                {
                    //Берём соседа
                    region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //Если высота соседа на 2 больше эрозируемого региона
                    if (neighbourRegion.Elevation == region.Elevation + 2
                        //Если сосед подлежит эрозии
                        && RegionIsErodible(ref neighbourRegion) == true
                        //И если список не содержит сущности соседа
                        && erodibleRegions.Contains(neighbourRegionEntity) == false)
                    {
                        //Заносим соседа в список
                        erodibleRegions.Add(neighbourRegionEntity);
                    }
                }

                //Если целевой регион подлежит эрозии
                if (RegionIsErodible(ref targetRegion)
                    //И список сущностей подлежащих эрозии регионов не содержит его
                    && erodibleRegions.Contains(targetRegionEntity) == false)
                {
                    //Заносим целевой регион в список
                    erodibleRegions.Add(targetRegionEntity);
                }

                //Для каждого соседа
                for (int a = 0; a < targetRegion.neighbourRegionPEs.Length; a++)
                {
                    //Если сосед существует
                    if (targetRegion.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity)
                        //И если сосед - не исходный эрозируемый регион
                        && region.selfPE.EqualsTo(targetRegion.neighbourRegionPEs[a]) == false)
                    {
                        //Берём соседа
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //Если высота соседа на единицу больше, чем высота целевого региона
                        if (neighbourRegion.Elevation == targetRegion.Elevation + 1
                            //И если сосед не подлежит эрозии
                            && RegionIsErodible(ref neighbourRegion) == false
                            //И если список содержит сущность соседа
                            && erodibleRegions.Contains(neighbourRegionEntity) == true)
                        {
                            //Удаляем соседа из списка
                            erodibleRegions.Remove(neighbourRegionEntity);
                        }
                    }
                }
            }

            //Выносим список в пул
            ListPool<int>.Add(erodibleRegions);
        }

        void MapCreateClimate()
        {
            //Несколько раз
            for (int a = 0; a < 40; a++)
            {
                //Для каждого региона
                foreach (int regionEntity in regionFilter.Value)
                {
                    //Берём регион и данные генерации
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                    ref CHexRegionGenerationData regionGenerationData = ref regionGenerationDataPool.Value.Get(regionEntity);

                    //Рассчитываем климатические данные
                    RegionEvolveClimate(
                        ref region,
                        ref regionGenerationData);
                }

                //Для каждого региона
                foreach (int regionEntity in regionFilter.Value)
                {
                    //Берём данные генерации
                    ref CHexRegionGenerationData regionGenerationData = ref regionGenerationDataPool.Value.Get(regionEntity);

                    //Переносим следующие данные климата в текущие, затем следующие приводим к значению по умолчанию
                    regionGenerationData.currentClimate = regionGenerationData.nextClimate;
                    regionGenerationData.nextClimate = new();
                }
            }
        }

        void MapSetTerrainType()
        {
            //Рассчитываем высоту, необходимую для появления каменистой пустыни
            int rockDesertElevation = mapGenerationData.Value.elevationMaximum
                - (mapGenerationData.Value.elevationMaximum - mapGenerationData.Value.waterLevel) / 2;

            //Для каждого региона
            foreach (int regionEntity in regionFilter.Value)
            {
                //Берём регион и данные генерации
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                ref CHexRegionGenerationData regionGenerationData = ref regionGenerationDataPool.Value.Get(regionEntity);

                //Берём данные климата
                ref DHexRegionClimate regionClimate = ref regionGenerationData.currentClimate;

                //Рассчитываем температуру региона
                float temperature = RegionDetermineTemperature(ref region);

                //Если регион не под водой
                if (region.IsUnderwater == false)
                {
                    //Определяем уровень температуры региона
                    int t = 0;

                    //Для каждого уровня температуры
                    for (; t < MapGenerationData.temperatureBands.Length; t++)
                    {
                        //Если температуры меньше уровня
                        if (temperature < MapGenerationData.temperatureBands[t])
                        {
                            break;
                        }
                    }

                    //Опрелеояем уррвень влажности региона
                    int m = 0;

                    //Для каждого уровня влажности
                    for (; m < MapGenerationData.moistureBands.Length; m++)
                    {
                        //Если влажность меньше уровня
                        if (regionClimate.moisture < MapGenerationData.moistureBands[m])
                        {
                            break;
                        }
                    }

                    //Определяем биом региона
                    ref DBiome biome = ref MapGenerationData.biomes[t * 4 + m];

                    //Если тип ландшафта биома - пустыня
                    if (biome.terrainTypeIndex == 0)
                    {
                        //Если высота региона больше или равна высоты, требуемой для каменистой пустыни
                        if (region.Elevation >= rockDesertElevation)
                        {
                            //Устанавливаем тип ландшафта региона на камень
                            region.TerrainTypeIndex = 3;
                        }
                        //Иначе
                        else
                        {
                            //Устанавливаем тип ландшафта по стандартным правилам
                            region.TerrainTypeIndex = biome.terrainTypeIndex;
                        }
                    }
                    //Иначе, если высота региона равна максимальной
                    else if (region.Elevation == mapGenerationData.Value.elevationMaximum)
                    {
                        //Устанавливаем тип ландшафта региона на снег
                        region.TerrainTypeIndex = 4;
                    }
                    //Иначе
                    else
                    {
                        //Устанавливаем тип ландшафта региона
                        region.TerrainTypeIndex = biome.terrainTypeIndex;
                    }
                }
                //Иначе
                else
                {
                    //Определяем тип ландшафта
                    int terrainTypeIndex;

                    //Если регион на один уровень ниже уровня моря
                    if (region.Elevation == mapGenerationData.Value.waterLevel - 1)
                    {
                        //Определяем количество соседей со склонами и обрывами
                        int cliffs = 0, slopes = 0;

                        //Для каждого соседа
                        for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
                        {
                            //Берём соседа
                            region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                            ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                            //Определяем разницу в высоте с уровнем воды
                            int elevationDelta = neighbourRegion.Elevation - region.WaterLevel;

                            //Если разница равна нулю
                            if (elevationDelta == 0)
                            {
                                //То имеем склон
                                slopes += 1;
                            }
                            //Иначе, если разница больше нуля
                            else if (elevationDelta > 0)
                            {
                                //То имеем обрыв
                                cliffs += 1;
                            }
                        }

                        //Если число склонов и обрывов больше трёх
                        if (cliffs + slopes > 3)
                        {
                            //Тип ландшафта - трава
                            terrainTypeIndex = 1;
                        }
                        //Иначе, если обрывов больше нуля
                        else if (cliffs > 0)
                        {
                            //Тип ландшафта - камень
                            terrainTypeIndex = 3;
                        }
                        //Иначе, если склонов больше нуля
                        else if (slopes > 0)
                        {
                            //Тип ландшафта - пустыня
                            terrainTypeIndex = 0;
                        }
                        //Иначе
                        else
                        {
                            //Тип ландшафта - трава
                            terrainTypeIndex = 1;
                        }
                    }
                    //Иначе, если высота региона больше уровня моря
                    else if (region.Elevation >= mapGenerationData.Value.waterLevel)
                    {
                        //Тип ландшафта - трава
                        terrainTypeIndex = 1;
                    }
                    //Иначе, если высота региона отрицательна
                    else if (region.Elevation < 0)
                    {
                        //Тип ландшафта - камень
                        terrainTypeIndex = 3;
                    }
                    //Иначе
                    else
                    {
                        //Тип ландшафта - грязь
                        terrainTypeIndex = 2;
                    }

                    //Если тип ландшафта - трава и температура находится в самом низком диапазоне
                    if (terrainTypeIndex == 1 && temperature < MapGenerationData.temperatureBands[0])
                    {
                        //Тип ландшафта - грязь
                        terrainTypeIndex = 2;
                    }

                    //Устанавливаем тип ландшафта
                    region.TerrainTypeIndex = terrainTypeIndex;
                }

                //Устанавливаем данные карты
                //region.SetMapData(temperature);
            }
        }

        GameObject MapCreateGOAndParent(
        Transform parent,
        string name)
        {
            //Создаём GO
            GameObject gO = new GameObject(name);

            //Заполняем основные данные GO
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
            //Создаём список точек, определяющих сегменты грани, и заносим в него текущую вершину
            List<DHexaspherePoint> segments = new List<DHexaspherePoint>(count + 1);
            segments.Add(startPoint);

            //Рассчитываем координаты точек
            double dx = endPoint.x - startPoint.x;
            double dy = endPoint.y - startPoint.y;
            double dz = endPoint.z - startPoint.z;
            double doublex = (double)startPoint.x;
            double doubley = (double)startPoint.y;
            double doublez = (double)startPoint.z;
            double doubleCount = (double)count;

            //Для каждого подразделения
            for (int a = 1; a < count; a++)
            {
                //Создаём новую вершину
                DHexaspherePoint newPoint = new(
                    (float)(doublex + dx * (double)a / doubleCount),
                    (float)(doubley + dy * (double)a / doubleCount),
                    (float)(doublez + dz * (double)a / doubleCount));

                //Проверяем вершину
                newPoint = PointGetCached(newPoint);

                //Заносим вершину в список
                segments.Add(newPoint);
            }

            //Заносим в список конечную вершину
            segments.Add(endPoint);

            //Возвращаем список точеку
            return segments;
        }

        DHexaspherePoint PointGetCached(
            DHexaspherePoint point)
        {
            DHexaspherePoint thePoint;

            //Если запрошенная вершина существует в словаре
            if (MapGenerationData.points.TryGetValue(point, out thePoint))
            {
                //Возвращаем вершину
                return thePoint;
            }
            //Иначе
            else
            {
                //Обновляем вершину в словаре
                MapGenerationData.points[point] = point;

                //Возвращаем вершину
                return point;
            }
        }

        void RegionCreate(
            Transform parentGO,
            DHexaspherePoint centerPoint,
            int regionIndex)
        {
            //Создаём новую сущность и назначаем ей компоненты региона, RAEO и данных генерации
            int regionEntity = world.Value.NewEntity();
            ref CHexRegion currentRegion = ref regionPool.Value.Add(regionEntity);
            ref CRegionAEO currentRAEO = ref regionAEOPool.Value.Add(regionEntity);
            ref CHexRegionGenerationData currentRegionGenerationData = ref regionGenerationDataPool.Value.Add(regionEntity);

            //Заполняем основные данные региона
            currentRegion = new(
                world.Value.PackEntity(regionEntity), regionIndex,
                centerPoint);

            //Заносим регион в массив регионов
            RegionsData.regionPEs[regionIndex] = currentRegion.selfPE;

            //Создаём GO региона и меш
            GORegionPrefab regionGO = GameObject.Instantiate(staticData.Value.regionPrefab);

            //Заполняем основные данные GO
            regionGO.gameObject.layer = parentGO.gameObject.layer;
            regionGO.transform.SetParent(parentGO, false);
            regionGO.transform.localPosition = Vector3.zero;
            regionGO.transform.localScale = Vector3.one;
            regionGO.transform.localRotation = Quaternion.Euler(0, 0, 0);

            //Создаём рендереры региона
            currentRegion.fleetRenderer = RegionCreateRenderer(ref currentRegion, regionGO.fleetRenderer);
            currentRegion.hoverRenderer = RegionCreateRenderer(ref currentRegion, regionGO.hoverRenderer);
            currentRegion.currentRenderer = RegionCreateRenderer(ref currentRegion, regionGO.currentRenderer);

            //Заполняем основные данные RAEO
            currentRAEO = new(currentRegion.selfPE);

            //Заполняем основные данные генерации
            currentRegionGenerationData = new(new(0, mapGenerationData.Value.startingMoisture), new(0, 0));
        }

        MeshRenderer RegionCreateRenderer(
            ref CHexRegion region, MeshRenderer renderer)
        {
            //Создаём новые мешфильтр и меш
            MeshFilter meshFilter = renderer.gameObject.AddComponent<MeshFilter>();
            Mesh mesh = new();
            mesh.hideFlags = HideFlags.DontSave;

            //Определяем высоту рендерера
            float extrusionAmount = region.ExtrudeAmount * MapGenerationData.extrudeMultiplier;

            //Рассчитываем выдавленные вершины региона
            Vector3[] extrudedVertices = new Vector3[region.vertexPoints.Length];
            //Для каждой вершины
            for (int a = 0; a < region.vertices.Length; a++)
            {
                //Рассчитываем положение выдавленной вершины
                extrudedVertices[a] = region.vertices[a] * (1f + extrusionAmount);
            }
            //Назначаем вершины мешу
            mesh.vertices = extrudedVertices;

            //Если у региона шесть вершин
            if (region.vertices.Length == 6)
            {
                mesh.SetIndices(
                    MapGenerationData.hexagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = MapGenerationData.hexagonUVs;
            }
            //Иначе
            else
            {
                mesh.SetIndices(
                    MapGenerationData.pentagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = MapGenerationData.pentagonUVs;
            }

            //Рассчитываем нормали меша
            mesh.normals = region.vertices;
            mesh.RecalculateNormals();

            //Назначаем меш рендереру и отключаем визуализацию
            meshFilter.sharedMesh = mesh;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.enabled = false;

            //Возвращаем рендерер
            return renderer;
        }

        bool RegionIsErodible(
            ref CHexRegion region)
        {
            //Определяем высоту, при которой произойдёт эрозия
            int erodibleElevation = region.Elevation - 2;

            //Для каждого соседа 
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //Берём соседа
                region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                //Если высота соседа меньше или равна высоте эрозии
                if (neighbourRegion.Elevation <= erodibleElevation)
                {
                    //Возвращаем, что эрозия возможна
                    return true;
                }
            }

            //Возвращаем, что эрозия невозможна
            return false;
        }

        int RegionGetErosionTarget(
            ref CHexRegion region)
        {
            //Создаём список кандидатов целей эрозии
            List<int> candidateEntities = ListPool<int>.Get();

            //Определяем высоту, при которой произойдёт эрозия
            int erodibleElevation = region.Elevation - 2;

            //Для каждого соседа
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //Берём соседа
                region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                //Если высота соседа меньше или равна высоте эрозии
                if (neighbourRegion.Elevation <= erodibleElevation)
                {
                    //Заносим соседа в список
                    candidateEntities.Add(neighbourRegionEntity);
                }
            }

            //Случайно выбираем регион из списка
            int targetRegionEntity = candidateEntities[Random.Range(0, candidateEntities.Count)];

            //Выносим список в пул
            ListPool<int>.Add(candidateEntities);

            //Возвращаем сущность целевого региона
            return targetRegionEntity;
        }

        void RegionEvolveClimate(
            ref CHexRegion region,
            ref CHexRegionGenerationData regionGenerationData)
        {
            //Берём данные климата
            ref DHexRegionClimate regionClimate = ref regionGenerationData.currentClimate;

            //Если регион находится под водой
            if (region.IsUnderwater == true)
            {
                //Определяем влажность
                regionClimate.moisture = 1f;

                //Рассчитываем испарение
                regionClimate.clouds += mapGenerationData.Value.evaporationFactor;
            }
            //Иначе
            else
            {
                //Рассчитываем испарение 
                float evaporation = regionClimate.moisture * mapGenerationData.Value.evaporationFactor;
                regionClimate.moisture -= evaporation;
                regionClimate.clouds += evaporation;
            }

            //Рассчитываем, сколько облаков превратится в осадки
            float precipitation = regionClimate.clouds * mapGenerationData.Value.precipitationFactor;
            //Применяем осадки к облакам и влажности
            regionClimate.clouds -= precipitation;
            regionClimate.moisture += precipitation;

            //Рассчитываем максимум облаков для региона
            float cloudMaximum = 1f - region.ViewElevation / (mapGenerationData.Value.elevationMaximum + 1);
            //Если облаков больше возможного
            if (regionClimate.clouds > cloudMaximum)
            {
                //То избыток переходит во влагу
                regionClimate.moisture += regionClimate.clouds - cloudMaximum;
                regionClimate.clouds = cloudMaximum;
            }

            //Определяем направление ветра
            //HexDirection mainDispersalDirection = mapGenerationData.Value.windDirection.Opposite();
            //Рассчитываем, сколько облаков рассеется
            float cloudDispersal = regionClimate.clouds * (1f / (5f + mapGenerationData.Value.windStrength));

            //Рассчитываем, сколько стекает в регионы ниже
            float runoff = regionClimate.moisture * mapGenerationData.Value.runoffFactor * (1f / 6f);
            //Рассчитываем, сколько влаги просачивается
            float seepage = regionClimate.moisture * mapGenerationData.Value.seepageFactor * (1f / 6f);

            //Для каждого соседа
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //Берём соседа и следующие данные климата
                region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);
                ref CHexRegionGenerationData neighbourRegionGenerationData = ref regionGenerationDataPool.Value.Get(neighbourRegionEntity);
                ref DHexRegionClimate neighbourRegionClimate = ref neighbourRegionGenerationData.nextClimate;

                //Если направление равно главному направлению ветра
                /*if (d == mainDispersalDirection)
                {
                    //Рассеивается объём облаков, увеличенный силой ветра
                    neighbourRegionClimate.clouds += cloudDispersal * mapGenerationData.Value.windStrength;
                }
                //Иначе
                else
                {*/
                    //Рассеивается обычный объём облаков
                    neighbourRegionClimate.clouds += cloudDispersal;
                //}

                //Рассчитываем разницу в высоте между регионами
                int elevationDelta = neighbourRegion.ViewElevation - region.ViewElevation;
                //Если разница меньше нуля
                if (elevationDelta < 0)
                {
                    //Сток уходит в соседа
                    neighbourRegionClimate.moisture -= runoff;
                    neighbourRegionClimate.moisture += runoff;
                }
                //Иначе, если разница равна нулю
                else if (elevationDelta == 0)
                {
                    //Просачивание уходит в соседей
                    neighbourRegionClimate.moisture -= seepage;
                    neighbourRegionClimate.moisture += seepage;
                }
            }

            //Берём следующие данные климата
            ref DHexRegionClimate regionNextClimate = ref regionGenerationData.nextClimate;

            //Переносим влажность из текущего
            regionNextClimate.moisture = regionClimate.moisture;

            //Если будущая влажность больше единицы
            if (regionNextClimate.moisture > 1f)
            {
                //Устанавливаем её на единицу
                regionNextClimate.moisture = 1f;
            }
        }

        float RegionDetermineTemperature(
            ref CHexRegion region)
        {
            //Рассчитываем широту региона
            float latitude = (float)region.centerPoint.z / 1f;
            latitude *= 2f;
            //Если широта больше единицы, то это другое полушарин
            if (latitude > 1f)
            {
                latitude = 2f - latitude;
            }

            //Рассчитываем температуру региона
            //По широте
            float temperature = Mathf.LerpUnclamped(mapGenerationData.Value.lowTemperature, mapGenerationData.Value.highTemperature, latitude);
            //По высоте
            temperature *= 1f - (region.ViewElevation - mapGenerationData.Value.waterLevel)
                / (mapGenerationData.Value.elevationMaximum - mapGenerationData.Value.waterLevel + 1f);
            //По случайному модификатору
            //temperature += (MapGenerationData.SampleNoise(region.Position * 0.1f).w * 2f - 1f) * mapGenerationData.Value.temperatureJitter;

            //Возвращаем температуру
            return temperature;
        }
    }
}