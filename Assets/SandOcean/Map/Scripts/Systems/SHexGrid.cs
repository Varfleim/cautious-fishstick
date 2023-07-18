
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
        //Миры
        readonly EcsWorldInject world = default;


        //Объекты карты
        //readonly EcsPoolInject<CMapObject> mapObjectPool = default;

        //Карта
        readonly EcsPoolInject<CHexChunk> chunkPool = default;

        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Дипломатия
        readonly EcsFilterInject<Inc<COrganization>> organizationFilter = default;
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Административно-экономические объекты
        readonly EcsFilterInject<Inc<CRegionAEO>> regionAEOFilter = default;
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;


        //События карты
        readonly EcsFilterInject<Inc<EMapGeneration>> mapGenerationEventFilter = default;
        readonly EcsPoolInject<EMapGeneration> mapGenerationEventPool = default;

        readonly EcsPoolInject<SRMapChunkRefresh> mapChunkRefreshSRPool = default;

        //События административно-экономических объектов

        readonly EcsFilterInject<Inc<SRORAEOCreate, COrganization>> oRAEOCreateSRFilter = default;

        //Данные
        readonly EcsCustomInject<StaticData> staticData = default;
        readonly EcsCustomInject<SceneData> sceneData = default;
        readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;
        readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события генерации карты
            foreach (int mapGenerationEventEntity in mapGenerationEventFilter.Value)
            {
                //Берём компонент события генерации карты
                ref EMapGeneration mapGenerationEvent= ref mapGenerationEventPool.Value.Get(mapGenerationEventEntity);

                //Инициализируем хэш-таблицу
                MapGenerationData.InitializeHashGrid();

                //Создаём карту
                MapCreate(ref mapGenerationEvent);

                world.Value.DelEntity(mapGenerationEventEntity);
            }

            //Если фильтр самозапросов создания ORAEO не пуст
            if (oRAEOCreateSRFilter.Value.GetEntitiesCount() > 0)
            {
                //Создаём ORAEO
                ORAEOCreating();
            }

            //Для каждого чанка
            /*foreach (int chunkEntity in chunkFilter.Value)
            {
                //Берём компонент чанка
                ref CHexChunk chunk
                    = ref chunkPool.Value.Get(chunkEntity);

                //Если чанк активен
                if (chunk.isActive == true)
                {
                    //Для каждого соседа чанка по координатам
                    for (int a = 0; a < SpaceGenerationData.chunkNeighbours.Length; a++)
                    {
                        //Определяем координаты соседа
                        Vector2 neighbourCoordinates = new(
                            chunk.coordinateX + SpaceGenerationData.chunkNeighbours[a].x,
                            chunk.coordinateZ + SpaceGenerationData.chunkNeighbours[a].y);

                        //Если существует чанк с такими координатами
                        if (spaceGenerationData.Value.chunks.TryGetValue(neighbourCoordinates, out EcsPackedEntity neighbourPE))
                        {
                            //Берём сущность соседа
                            neighbourPE.Unpack(world.Value, out int neighbourEntity);

                            //Запрашиваем обновление чанка
                            ChunkRefreshEvent(
                                neighbourEntity);
                        }
                        //Иначе
                        else
                        {
                            //Создаём чанк
                            ChunkCreate(
                                (int)neighbourCoordinates.x, (int)neighbourCoordinates.y);
                        }
                    }

                    //Отмечаем, что чанк неактивен
                    chunk.isActive = false;
                }
            }*/
        }

        void MapCreate(
            ref EMapGeneration mapGenerationEvent)
        {
            //Инициализируем карту
            MapInitialization(ref mapGenerationEvent);


            //Создаём чанки
            MapCreateChunks(ref mapGenerationEvent);

            //Создаём регионы
            MapCreateRegions(ref mapGenerationEvent);


            //Распределяем богатство карты
            MapWealthDistribution();

            //Генерируем карту 
            MapGeneration(ref mapGenerationEvent);
        }

        void MapInitialization(
            ref EMapGeneration mapGenerationEvent)
        {
            //Назначаем текстуру шума
            MapGenerationData.noiseSource
                = mapGenerationData.Value.noiseTexture;

            //Определяем количество чанков
            mapGenerationData.Value.chunkCountX
                = mapGenerationEvent.chunkCountX;
            mapGenerationData.Value.chunkCountZ
                = mapGenerationEvent.chunkCountZ;

            //Определяем размер массива PE чанков
            mapGenerationData.Value.chunkPEs = new EcsPackedEntity[mapGenerationData.Value.chunkCountX * mapGenerationData.Value.chunkCountZ];

            //Определяем количество регионов
            mapGenerationData.Value.regionCountX
                = mapGenerationEvent.chunkCountX
                * MapGenerationData.chunkSizeX;
            mapGenerationData.Value.regionCountZ
                = mapGenerationEvent.chunkCountZ
                * MapGenerationData.chunkSizeZ;
            mapGenerationData.Value.regionCount = mapGenerationData.Value.regionCountX * mapGenerationData.Value.regionCountZ;

            //Определяем размер свёртки
            MapGenerationData.wrapSize = mapGenerationData.Value.regionCountX;

            //Определяем размер массива PE регионов
            mapGenerationData.Value.regionPEs = new EcsPackedEntity[mapGenerationData.Value.regionCount];

            //Инициализируем данные шейдера
            mapGenerationData.Value.regionShaderData.Initialize(mapGenerationData.Value.regionCountX, mapGenerationData.Value.regionCountZ);
        }

        void MapCreateChunks(
            ref EMapGeneration mapGenerationEvent)
        {
            //Создаём массив столбцов карты
            mapGenerationData.Value.columns = new Transform[mapGenerationData.Value.chunkCountX];
            //Для каждого чанка по ширине
            for (int x = 0; x < mapGenerationData.Value.chunkCountX; x++)
            {
                //Создаём новый столбец и присоединяем его к центральному объекту
                mapGenerationData.Value.columns[x] = new GameObject("Column").transform;
                mapGenerationData.Value.columns[x].SetParent(sceneData.Value.coreObject);
            }


            //Для каждого чанка по высоте
            for (int z = 0, i = 0; z < mapGenerationData.Value.chunkCountZ; z++)
            {
                //Для каждого чанка по ширине
                for (int x = 0; x < mapGenerationData.Value.chunkCountX; x++)
                {
                    //Создаём чанк
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
            //Для каждого региона по высоте
            for (int z = 0, i = 0; z < mapGenerationData.Value.regionCountZ; z++)
            {
                //Для каждого региона по ширине
                for (int x = 0; x < mapGenerationData.Value.regionCountX; x++)
                {
                    //Создаём регион
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
            //Создаём границу приоритетную очередь для генерации алгоритмами поиска пути
            HexRegionPriorityQueue searchFrontier = new();

            //Для каждого региона
            foreach (int regionEntity in regionFilter.Value)
            {
                //Берём компонент региона
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Устанавливаем уровень моря
                region.WaterLevel = mapGenerationData.Value.waterLevel;
            }

            //Создаём зоны карты
            MapCreateAreas();

            //Создаём сушу
            MapCreateLand(searchFrontier);

            //Проводим эрозию
            MapErodeLand();

            //Создаём климат
            List<DHexRegionClimate> climate = MapCreateClimat();

            //Устанавливаем типы ландшафта
            MapSetTerrainType(climate);

            //Для каждого региона
            foreach (int regionEntity in regionFilter.Value)
            {
                //Берём компонент региона
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Устанавливаем фазу поиска на ноль
                region.SearchPhase = 0;
            }
        }

        void MapCreateAreas()
        {
            int borderX = mapGenerationData.Value.areaBorder;

            //Создаём зону карты
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
                        //Заносим новую зону в массив
                        mapGenerationData.Value.mapAreas[0] = mapArea;

                        mapArea.xMin = mapGenerationData.Value.regionCountX / 2 + mapGenerationData.Value.areaBorder;
                        mapArea.xMax = mapGenerationData.Value.regionCountX - borderX;
                        //Заносим новую зону в массив
                        mapGenerationData.Value.mapAreas[1] = mapArea;
                    }
                    else
                    {
                        borderX = 0;

                        mapArea.xMin = borderX;
                        mapArea.xMax = mapGenerationData.Value.regionCountX - borderX;
                        mapArea.zMin = mapGenerationData.Value.mapBorderZ;
                        mapArea.zMax = mapGenerationData.Value.regionCountZ / 2 - mapGenerationData.Value.areaBorder;
                        //Заносим новую зону в массив
                        mapGenerationData.Value.mapAreas[0] = mapArea;

                        mapArea.zMin = mapGenerationData.Value.regionCountZ / 2 + mapGenerationData.Value.areaBorder;
                        mapArea.zMax = mapGenerationData.Value.regionCountZ - mapGenerationData.Value.mapBorderZ;
                        //Заносим новую зону в массив
                        mapGenerationData.Value.mapAreas[1] = mapArea;
                    }
                    break;
            }
        }

        void MapCreateLand(
            HexRegionPriorityQueue searchFrontier)
        {
            //Определяем количество регионов суши
            int landBudget = Mathf.RoundToInt(mapGenerationData.Value.regionCount * mapGenerationData.Value.landPercentage * 0.01f);

            //Пока бюджет суши больше нуля и прошло менее 10000 итераций
            for(int a = 0; a < 10000; a++)
            {
                //Определяем, нужно ли утопить сушу
                bool sink = UnityEngine.Random.value < mapGenerationData.Value.sinkProbability;

                //Для каждой зоны карты
                for (int b = 0; b < mapGenerationData.Value.mapAreas.Length; b++)
                {
                    //Определяем размер чанка
                    int chunkSize = UnityEngine.Random.Range(mapGenerationData.Value.chunkSizeMin, mapGenerationData.Value.chunkSizeMax + 1);

                    //Если требуется утопить сушу
                    if (sink == true)
                    {
                        //Топим сушу
                        MapSinkTerrain(
                            searchFrontier,
                            ref mapGenerationData.Value.mapAreas[b],
                            chunkSize,
                            landBudget,
                            out landBudget);
                    }
                    //Иначе
                    else
                    {
                        //Поднимаем сушу
                        MapRaiseTerrain(
                            searchFrontier,
                            ref mapGenerationData.Value.mapAreas[b],
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
            }

            //Если бюджет суши больше нуля
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
            //Увеличиваем фазу поиска
            inputData.Value.searchFrontierPhase += 1;

            //Берём компонент первого региона случайно
            RegionGetRandom(ref mapArea).Unpack(world.Value, out int firstRegionEntity);
            ref CHexRegion firstRegion = ref regionPool.Value.Get(firstRegionEntity);

            //Устанавливаем его фазу поиска на текущую
            firstRegion.SearchPhase = inputData.Value.searchFrontierPhase;
            firstRegion.Distance = 0;
            firstRegion.SearchHeuristic = 0;
            //Включаем регион в границу поиска
            searchFrontier.Enqueue(
                firstRegion.selfPE,
                firstRegion.SearchPriority);

            //Запоминаем координаты первого региона
            DHexCoordinates center = firstRegion.coordinates;

            //Определяем, будет это высотным поднятием суши или обычным
            int rise = UnityEngine.Random.value < mapGenerationData.Value.highRiseProbability ? 2 : 1;
            //Создаём счётчик размера
            int currentSize = 0;
            //Пока счётчик меньше требуемого размера и граница не пуста
            while (currentSize < chunkSize && searchFrontier.Count > 0)
            {
                //Берём компонент первого региона в границе
                searchFrontier.Dequeue().cellPE.Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Запоминаем исходную высоту региона
                int originalElevation = region.Elevation;
                //Определяем новую высоту региона
                int newElevation = originalElevation + rise;

                //Если новая высота больше максимальной
                if (newElevation > mapGenerationData.Value.elevationMaximum)
                {
                    continue;
                }

                //Увеличиваем высоту региона
                region.Elevation = newElevation;

                //Если исходная высота региона меньше уровня моря
                if(originalElevation < mapGenerationData.Value.waterLevel
                    //И новая высота больше или равна уровню моря
                    && newElevation >= mapGenerationData.Value.waterLevel
                    //И уменьшенный бюджет суши равен нулю
                    && --landBudget == 0)
                {
                    break;
                }

                //Увеличиваем счётчик
                currentSize += 1;

                //Для каждого направления
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    //Если сосед с данного направления существует
                    if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                    {
                        //Берём компонент соседнего региона
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //Если фаза поиска соседа меньше текущей
                        if (neighbourRegion.SearchPhase < inputData.Value.searchFrontierPhase)
                        {
                            //Устанавливаем его фазу поиска на текущую
                            neighbourRegion.SearchPhase = inputData.Value.searchFrontierPhase;
                            neighbourRegion.Distance = neighbourRegion.coordinates.DistanceTo(center);
                            neighbourRegion.SearchHeuristic = UnityEngine.Random.value < mapGenerationData.Value.jitterProbability ? 1 : 0;
                            //Включаем регион в границу поиска
                            searchFrontier.Enqueue(
                                neighbourRegion.selfPE, 
                                neighbourRegion.SearchPriority);
                        }
                    }
                }
            }
            //Очищаем границу поиска
            searchFrontier.Clear();

            //Возвращаем оставшийся бюджет суши
            currentLandBudget = landBudget;
        }
        
        void MapSinkTerrain(
            HexRegionPriorityQueue searchFrontier,
            ref DMapArea mapArea,
            int chunkSize,
            int landBudget,
            out int currentLandBudget)
        {
            //Увеличиваем фазу поиска
            inputData.Value.searchFrontierPhase += 1;

            //Берём компонент первого региона случайно
            RegionGetRandom(ref mapArea).Unpack(world.Value, out int firstRegionEntity);
            ref CHexRegion firstRegion = ref regionPool.Value.Get(firstRegionEntity);

            //Устанавливаем его фазу поиска на текущую
            firstRegion.SearchPhase = inputData.Value.searchFrontierPhase;
            firstRegion.Distance = 0;
            firstRegion.SearchHeuristic = 0;
            //Включаем регион в границу поиска
            searchFrontier.Enqueue(
                firstRegion.selfPE,
                firstRegion.SearchPriority);

            //Запоминаем координаты первого региона
            DHexCoordinates center = firstRegion.coordinates;

            //Определяем, будет это высотным утоплением суши или обычным
            int sink = UnityEngine.Random.value < mapGenerationData.Value.highRiseProbability ? 2 : 1;
            //Создаём счётчик размера
            int currentSize = 0;
            //Пока счётчик меньше требуемого размера и граница не пуста
            while (currentSize < chunkSize && searchFrontier.Count > 0)
            {
                //Берём компонент первого региона в границе
                searchFrontier.Dequeue().cellPE.Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Запоминаем исходную высоту региона
                int originalElevation = region.Elevation;
                //Определяем новую высоту региона
                int newElevation = region.Elevation - sink;

                //Если новая высота региона меньше минимальной
                if (newElevation < mapGenerationData.Value.elevationMinimum)
                {
                    continue;
                }

                //Уменьшаем высоту региона
                region.Elevation = newElevation;

                //Если исходная высота региона больше или равна уровню моря
                if(originalElevation >= mapGenerationData.Value.waterLevel
                    //И новая высота меньше уровню моря
                    && newElevation < mapGenerationData.Value.waterLevel)
                {
                    //Увеличиваем бюджет суши
                    landBudget += 1;
                }

                //Увеличиваем счётчик
                currentSize += 1;

                //Для каждого направления
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    //Если сосед с данного направления существует
                    if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                    {
                        //Берём компонент соседнего региона
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //Если фаза поиска соседа меньше текущей
                        if (neighbourRegion.SearchPhase < inputData.Value.searchFrontierPhase)
                        {
                            //Устанавливаем его фазу поиска на текущую
                            neighbourRegion.SearchPhase = inputData.Value.searchFrontierPhase;
                            neighbourRegion.Distance = neighbourRegion.coordinates.DistanceTo(center);
                            neighbourRegion.SearchHeuristic = UnityEngine.Random.value < mapGenerationData.Value.jitterProbability ? 1 : 0;
                            //Включаем регион в границу поиска
                            searchFrontier.Enqueue(
                                neighbourRegion.selfPE, 
                                neighbourRegion.SearchPriority);
                        }
                    }
                }
            }
            //Очищаем границу поиска
            searchFrontier.Clear();

            //Возвращаем оставшийся бюджет суши
            currentLandBudget = landBudget;
        }

        void MapErodeLand()
        {
            //Создаём список PE регионов, подлежащих эрозии
            List<int> erodibleRegions = ListPool<int>.Get();

            //Для каждого региона
            foreach (int regionEntity in regionFilter.Value)
            {
                //Берём компонент региона
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Если регион подлежит эрозии
                if (RegionIsErodible(ref region) == true)
                {
                    //Заносим его в список 
                    erodibleRegions.Add(regionEntity);
                }
            }

            //Определяем, сколько регионов должно подвергнуться эрозии
            int erodibleRegionsCount = (int)(erodibleRegions.Count * (100 - mapGenerationData.Value.erosionPercentage) * 0.01f);

            //Пока список PE регионов больше требуемого числа
            while (erodibleRegions.Count > erodibleRegionsCount)
            {
                //Выбираем случайный регион в списке
                int index = UnityEngine.Random.Range(0, erodibleRegions.Count);
                ref CHexRegion region = ref regionPool.Value.Get(erodibleRegions[index]);

                //Выбираем одного соседа как цель эрозии
                RegionGetErosionTarget(ref region).Unpack(world.Value, out int targetRegionEntity);
                ref CHexRegion targetRegion = ref regionPool.Value.Get(targetRegionEntity);

                //Уменьшаем высоту исходного региона и увеличиваем высоту целевого
                region.Elevation -= 1;
                targetRegion.Elevation += 1;

                //Если регион больше не подлежит эрозии
                if (RegionIsErodible(ref region) == false)
                {
                    //Удаляем его PE из списка, заменяя последним регионом в списке
                    erodibleRegions[index] = erodibleRegions[erodibleRegions.Count - 1];
                    erodibleRegions.RemoveAt(erodibleRegions.Count - 1);
                }

                //Для каждого направления
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    //Если сосед существует
                    if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                    {
                        //Берём компонент соседа
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //Если высота соседа на 2 больше эрозируемого региона
                        if (neighbourRegion.Elevation == region.Elevation + 2
                            //Если сосед подлежит эрозии
                            && RegionIsErodible(ref neighbourRegion) == true
                            //И если список не содержит PE соседа
                            && erodibleRegions.Contains(neighbourRegionEntity) == false)
                        {
                            //Заносим соседа в список
                            erodibleRegions.Add(neighbourRegionEntity);
                        }
                    }
                }

                //Если целевой регион подлежит эрозии
                if (RegionIsErodible(ref targetRegion)
                    //И список PE подлежащих эрозии регионов не содержит его
                    && erodibleRegions.Contains(targetRegionEntity) == false)
                {
                    //Заносим целевой регион в список
                    erodibleRegions.Add(targetRegionEntity);
                }

                //Для каждого направления
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    //Если сосед существует
                    if (targetRegion.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity)
                        //И если сосед - не исходный эрозируемый регион
                        && region.selfPE.EqualsTo(targetRegion.GetNeighbour(d)) == false)
                    {
                        //Берём компонент соседа
                        ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                        //Если высота соседа на единицу больше, чем высота целевого региона
                        if (neighbourRegion.Elevation == targetRegion.Elevation + 1
                            //И если сосед не подлежит эрозии
                            && RegionIsErodible(ref neighbourRegion) == false
                            //И если список содержит PE соседа
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

        List<DHexRegionClimate> MapCreateClimat()
        {
            //Создаём списки для рассчёта климата
            List<DHexRegionClimate> climate = new();
            List<DHexRegionClimate> nextClimate = new();

            //Для каждого региона
            foreach (int regionEntity in regionFilter.Value)
            {
                //Берём компонент региона
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Создаём записи для него в обоих списках
                climate.Add(new(0, mapGenerationData.Value.startingMoisture));
                nextClimate.Add(new(0, 0));
            }

            //Несколько раз
            for (int a = 0; a < 40; a++)
            {
                //Для каждого региона
                foreach (int regionEntity in regionFilter.Value)
                {
                    //Берём компонент региона
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    //Рассчитываем климатические данные
                    RegionEvolveClimate(
                        ref region,
                        climate, nextClimate);
                }

                //Меняем списки местами
                List<DHexRegionClimate> swap = climate;
                climate = nextClimate;
                nextClimate = swap;
            }

            return climate;
        }

        void MapSetTerrainType(
            List<DHexRegionClimate> climate)
        {
            //Рассчитываем высоту, необходимую для появления каменистой пустыни
            int rockDesertElevation = mapGenerationData.Value.elevationMaximum 
                - (mapGenerationData.Value.elevationMaximum - mapGenerationData.Value.waterLevel) / 2;

            //Для каждого региона
            foreach (int regionEntity in regionFilter.Value)
            {
                //Берём компонент региона и данные климата
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                DHexRegionClimate regionClimate = climate[region.Index];

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
                    else if(region.Elevation == mapGenerationData.Value.elevationMaximum)
                    {
                        Debug.LogWarning("!");

                        //Устанавливаем тип ландшафта региона на снег
                        region.TerrainTypeIndex = 4;
                    }
                    //Иначе
                    else
                    {
                        //Устанавливаем тип ландшафта региона
                        region.TerrainTypeIndex = biome.terrainTypeIndex;
                    }

                    //Если высота региона не равна максимальной
                    if (region.Elevation != mapGenerationData.Value.elevationMaximum)
                    {
                        //Устанавливаем уровень растительности соответственно биому
                        region.PlantLevel = biome.plant;
                    }
                    //Иначе
                    else
                    {
                        //Уровень растительности равен нулю
                        region.PlantLevel = 0;
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

                        //Для каждого направления
                        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                        {
                            //Если сосед существует
                            if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                            {
                                //Берём компонент соседа
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
                        }

                        //Если число склонов и обрывов больше трёх
                        if (cliffs + slopes > 3)
                        {
                            //Тип ландшафта - трава
                            terrainTypeIndex = 1;
                        }
                        //Иначе, если обрывов больше нуля
                        else if(cliffs > 0)
                        {
                            //Тип ландшафта - камень
                            terrainTypeIndex = 3;
                        }
                        //Иначе, если склонов больше нуля
                        else if(slopes > 0)
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
                    else if(region.Elevation >= mapGenerationData.Value.waterLevel)
                    {
                        //Тип ландшафта - трава
                        terrainTypeIndex = 1;
                    }
                    //Иначе, если высота региона отрицательна
                    else if(region.Elevation < 0)
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

        void ChunkCreate(
            ref EMapGeneration mapGenerationEvent,
            int chunkX, int chunkZ,
            int chunkIndex)
        {
            //Создаём новую сущность и назначаем ей компонент чанка
            int chunkEntity = world.Value.NewEntity();
            ref CHexChunk chunk = ref chunkPool.Value.Add(chunkEntity);

            //Создаём объект чанка
            GOChunk chunkGO = GameObject.Instantiate(staticData.Value.chunkPrefab);

            //Заполняем основные данные чанка
            chunk = new(
                world.Value.PackEntity(chunkEntity),
                chunkX, chunkZ,
                chunkGO.transform, chunkGO.chunkCanvas,
                chunkGO.terrain, chunkGO.rivers, chunkGO.roads, chunkGO.water, chunkGO.waterShore, chunkGO.estuaries,
                chunkGO.features,
                MapGenerationData.ChunkSize);

            //Создаём меши
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

            //Присоединяем чанк к соответствующему столбцу
            chunk.transform.SetParent(mapGenerationData.Value.columns[chunkX], false);

            //Заносим чанк в массив чанков
            mapGenerationData.Value.chunkPEs[chunkIndex] = chunk.selfPE;

            //Запрашиваем обновление чанка
            ChunkRefreshEvent(chunkEntity);
        }

        void ChunkRefreshEvent(
            int chunkEntity)
        {
            //Если ещё не существует самозапроса обновления чанка
            if (mapChunkRefreshSRPool.Value.Has(chunkEntity) == false)
            {
                //Прикрепляем к сущности самозапрос обновления чанка
                mapChunkRefreshSRPool.Value.Add(chunkEntity);
            }
        }

        
        void RegionCreate(
            int regionGlobalX, int regionGlobalZ,
            int regionIndex)
        {
            //Определяем родительский чанк региона
            int parentChunkX = regionGlobalX / MapGenerationData.chunkSizeX;
            int parentChunkZ = regionGlobalZ / MapGenerationData.chunkSizeZ;
            mapGenerationData.Value.chunkPEs[parentChunkX + parentChunkZ * mapGenerationData.Value.chunkCountX].Unpack(
                world.Value, out int parentChunkEntity);
            ref CHexChunk parentChunk = ref chunkPool.Value.Get(parentChunkEntity);

            int regionLocalX = regionGlobalX - parentChunkX * MapGenerationData.chunkSizeX;
            int regionLocalZ = regionGlobalZ - parentChunkZ * MapGenerationData.chunkSizeZ;

            //Создаём новую сущность и назначаем ей компонент региона и RAEO
            int regionEntity = world.Value.NewEntity();
            ref CHexRegion currentRegion = ref regionPool.Value.Add(regionEntity);
            ref CRegionAEO currentRAEO = ref regionAEOPool.Value.Add(regionEntity);

            //Определяем позицию региона
            Vector3 position = new(
                (regionGlobalX + regionGlobalZ * 0.5f - regionGlobalZ / 2) * (MapGenerationData.innerRadius * 2f),
                0f,
                regionGlobalZ * (MapGenerationData.outerRadius * 1.5f));
            //Определяем координаты региона
            DHexCoordinates regionCoordinates = DHexCoordinates.FromOffsetCoordinates(regionGlobalX, regionGlobalZ);

            //Создаём объект региона
            GORegion regionGO = GameObject.Instantiate(staticData.Value.regionPrefab);

            //Заполняем основные данные региона
            currentRegion = new(
                world.Value.PackEntity(regionEntity), regionIndex,
                position, regionCoordinates,
                regionGO.regionTransform, regionGO.regionLabel, regionGO.regionHighlight,
                regionGlobalX / MapGenerationData.chunkSizeX, parentChunk.selfPE,
                mapGenerationData.Value.regionShaderData);

            //Перемещаем объект региона на соответствующую позицию
            currentRegion.transform.localPosition = currentRegion.Position;

            //Перемещаем объект метки на соответствующую позицию и отображаем координаты региона
            currentRegion.uiRect.rectTransform.anchoredPosition = new(currentRegion.Position.x, currentRegion.Position.z);

            //Заносим регион в массив регионов родительского чанка
            parentChunk.regionPEs[regionLocalX + regionLocalZ * MapGenerationData.chunkSizeX] = currentRegion.selfPE;

            //Прикрепляем объекты региона к объекту чанка
            currentRegion.transform.SetParent(parentChunk.transform, false);
            currentRegion.uiRect.rectTransform.SetParent(parentChunk.canvas.transform, false);
            currentRegion.highlight.rectTransform.SetParent(parentChunk.canvas.transform);

            //Заносим регион в массив регионов
            mapGenerationData.Value.regionPEs[regionIndex] = currentRegion.selfPE;

            //Если регион не находится в крайнем левом столбце
            if (regionGlobalX > 0)
            {
                //Берём соседа с запада
                mapGenerationData.Value.regionPEs[regionIndex - 1].Unpack(world.Value, out int wNeighbourRegionEntity);
                ref CHexRegion wNeighbourRegion = ref regionPool.Value.Get(wNeighbourRegionEntity);

                //Создаём соседство
                RegionSetNeighbour(
                    ref currentRegion, ref wNeighbourRegion,
                    HexDirection.W);

                //Если регион находится в крайнем правом столбце
                if (regionGlobalX == mapGenerationData.Value.regionCountX - 1)
                {
                    //Берём соседа с востока
                    mapGenerationData.Value.regionPEs[regionIndex - regionGlobalX].Unpack(world.Value, out int eNeighbourRegionEntity);
                    ref CHexRegion eNeighbourRegion = ref regionPool.Value.Get(eNeighbourRegionEntity);

                    //Создаём соседство 
                    RegionSetNeighbour(
                        ref currentRegion, ref eNeighbourRegion,
                        HexDirection.E);
                }
            }
            //Если регион не находится в крайнем нижнем ряду
            if (regionGlobalZ > 0)
            {
                //Если регион находится в нечётном ряду
                if ((regionGlobalZ & 1) == 0)
                {
                    //Берём соседа с юго-востока
                    mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX].Unpack(world.Value, out int sENeighbourRegionEntity);
                    ref CHexRegion sENeighbourRegion = ref regionPool.Value.Get(sENeighbourRegionEntity);

                    //Создаём соседство
                    RegionSetNeighbour(
                        ref currentRegion, ref sENeighbourRegion,
                        HexDirection.SE);

                    //Если регион не находится в крайнем левом столбце
                    if (regionGlobalX > 0)
                    {
                        //Берём соседа с юго-запада
                        mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX - 1].Unpack(world.Value, out int sWNeighbourRegionEntity);
                        ref CHexRegion sWNeighbourRegion = ref regionPool.Value.Get(sWNeighbourRegionEntity);

                        //Создаём соседство
                        RegionSetNeighbour(
                            ref currentRegion, ref sWNeighbourRegion,
                            HexDirection.SW);
                    }
                    //Иначе
                    else
                    {
                        //Берём соседа с юго-запада
                        mapGenerationData.Value.regionPEs[regionIndex - 1].Unpack(world.Value, out int sWNeighbourRegionEntity);
                        ref CHexRegion sWNeighbourRegion = ref regionPool.Value.Get(sWNeighbourRegionEntity);

                        //Создаём соседство
                        RegionSetNeighbour(
                            ref currentRegion, ref sWNeighbourRegion,
                            HexDirection.SW);
                    }
                }
                //Иначе
                else
                {
                    //Берём соседа с юго-запада
                    mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX].Unpack(world.Value, out int sWNeighbourRegionEntity);
                    ref CHexRegion sWNeighbourRegion = ref regionPool.Value.Get(sWNeighbourRegionEntity);

                    //Создаём соседство
                    RegionSetNeighbour(
                        ref currentRegion, ref sWNeighbourRegion,
                        HexDirection.SW);

                    //Если регион не находится в крайнем правом столбце
                    if (regionGlobalX < mapGenerationData.Value.regionCountX - 1)
                    {
                        //Берём соседа с юго-востока
                        mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX + 1].Unpack(world.Value, out int sENeighbourRegionEntity);
                        ref CHexRegion sENeighbourRegion = ref regionPool.Value.Get(sENeighbourRegionEntity);

                        //Создаём соседство
                        RegionSetNeighbour(
                            ref currentRegion, ref sENeighbourRegion,
                            HexDirection.SE);
                    }
                    //Иначе
                    else
                    {
                        //Берём соседа с юго-востока
                        mapGenerationData.Value.regionPEs[regionIndex - mapGenerationData.Value.regionCountX * 2 + 1].Unpack(world.Value, out int sENeighbourRegionEntity);
                        ref CHexRegion sENeighbourRegion = ref regionPool.Value.Get(sENeighbourRegionEntity);

                        //Создаём соседство
                        RegionSetNeighbour(
                            ref currentRegion, ref sENeighbourRegion,
                            HexDirection.SE);
                    }
                }
            }


            //Заполняем основные данные RAEO
            currentRAEO = new(
                currentRegion.selfPE);
        }

        void RegionSetNeighbour(
            ref CHexRegion current, ref CHexRegion neighbourRegion,
            HexDirection direction)
        {
            //Задаём соседа по указанному направлению текущему региону
            current.neighbourRegionPEs[(int)direction] = neighbourRegion.selfPE;

            //Задаём соседа по противоположному направлению соседу
            neighbourRegion.neighbourRegionPEs[(int)direction.Opposite()] = current.selfPE;
        }

        EcsPackedEntity RegionGet(
            int xOffset, int zOffset)
        {
            //Возвращаем PE запрошенного региона
            return mapGenerationData.Value.regionPEs[xOffset + zOffset * mapGenerationData.Value.regionCountX];
        }

        EcsPackedEntity RegionGet(
            int regionIndex)
        {
            //Возвращаем PE запрошенного региона
            return mapGenerationData.Value.regionPEs[regionIndex];
        }

        EcsPackedEntity RegionGetRandom(
            ref DMapArea mapArea)
        {
            //Возвращаем PE случайного региона
            return RegionGet(
                UnityEngine.Random.Range(mapArea.xMin, mapArea.xMax),
                UnityEngine.Random.Range(mapArea.zMin, mapArea.zMax));
        }

        bool RegionIsErodible(
            ref CHexRegion region)
        {
            //Определяем высоту, при которой произойдёт эрозия
            int erodibleElevation = region.Elevation - 2;

            //Для каждого направления
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //Если сосед существует
                if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //Берём компонент соседа
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //Если высота соседа меньше или равна высоте эрозии
                    if (neighbourRegion.Elevation <= erodibleElevation)
                    {
                        //Возвращаем, что эрозия возможна
                        return true;
                    }
                }
            }

            //Возвращаем, что эрозия невозможна
            return false;
        }

        EcsPackedEntity RegionGetErosionTarget(
            ref CHexRegion region)
        {
            //Создаём список кандидатов целей эрозии
            List<int> candidates = ListPool<int>.Get();

            //Определяем высоту, при которой произойдёт эрозия
            int erodibleElevation = region.Elevation - 2;

            //Для каждого направления
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //Если сосед существует
                if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //Берём компонент соседа
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //Если высота соседа меньше или равна высоте эрозии
                    if (neighbourRegion.Elevation <= erodibleElevation)
                    {
                        //Заносим соседа в список
                        candidates.Add(neighbourRegionEntity);
                    }
                }
            }

            //Случайно выбираем цель из списка
            ref CHexRegion targetRegion = ref regionPool.Value.Get(candidates[UnityEngine.Random.Range(0, candidates.Count)]);

            //Выносим список в пул
            ListPool<int>.Add(candidates);

            //Возвращаем PE целевого региона
            return targetRegion.selfPE;
        }

        void RegionEvolveClimate(
            ref CHexRegion region,
            List<DHexRegionClimate> climate,
            List<DHexRegionClimate> nextClimate)
        {
            //Берём данные климата
            DHexRegionClimate regionClimate = climate[region.Index];

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
            HexDirection mainDispersalDirection = mapGenerationData.Value.windDirection.Opposite();
            //Рассчитываем, сколько облаков рассеется
            float cloudDispersal = regionClimate.clouds * (1f / (5f + mapGenerationData.Value.windStrength));

            //Рассчитываем, сколько стекает в регионы ниже
            float runoff = regionClimate.moisture * mapGenerationData.Value.runoffFactor * (1f / 6f);
            //Рассчитываем, сколько влаги просачивается
            float seepage = regionClimate.moisture * mapGenerationData.Value.seepageFactor * (1f / 6f);

            //Для каждого направления
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //Если сосед существует
                if (region.GetNeighbour(d).Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //Берём компонент соседа и следующие данные климата
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);
                    DHexRegionClimate neighbourRegionClimate = nextClimate[neighbourRegion.Index];

                    //Если направление равно главному направлению ветра
                    if (d == mainDispersalDirection)
                    {
                        //Рассеивается объём облаков, увеличенный силой ветра
                        neighbourRegionClimate.clouds += cloudDispersal * mapGenerationData.Value.windStrength;
                    }
                    //Иначе
                    else
                    {
                        //Рассеивается обычный объём облаков
                        neighbourRegionClimate.clouds += cloudDispersal;
                    }

                    //Рассчитываем разницу в высоте между регионами
                    int elevationDelta = neighbourRegion.ViewElevation - region.ViewElevation;
                    //Если разница меньше нуля
                    if (elevationDelta < 0)
                    {
                        //Сток уходит в соседний регион
                        neighbourRegionClimate.moisture -= runoff;
                        neighbourRegionClimate.moisture += runoff;
                    }
                    //Иначе, если разница равна нулю
                    else if (elevationDelta == 0)
                    {
                        //Просачивание уходит в соседние регионы
                        neighbourRegionClimate.moisture -= seepage;
                        neighbourRegionClimate.moisture += seepage;
                    }

                    //Обновляем данные климата соседа в списке
                    nextClimate[neighbourRegion.Index] = neighbourRegionClimate;
                }
            }

            //Обновляем данные климата
            DHexRegionClimate regionNextClimate = nextClimate[region.Index];
            regionNextClimate.moisture += regionClimate.moisture;

            //Если будущая влажность больше единицы
            if (regionNextClimate.moisture > 1f)
            {
                //Устанавливаем её на единицу
                regionNextClimate.moisture = 1f;
            }

            nextClimate[region.Index] = regionNextClimate;
            climate[region.Index] = new();
        }

        float RegionDetermineTemperature(
            ref CHexRegion region)
        {
            //Рассчитываем широту региона
            float latitude = (float)region.coordinates.Z / mapGenerationData.Value.regionCountZ;
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
            temperature += (MapGenerationData.SampleNoise(region.Position * 0.1f).w * 2f - 1f) * mapGenerationData.Value.temperatureJitter;

            //Возвращаем температуру
            return temperature;
        }


        void ORAEOCreating()
        {
            //Создаём временный список DORAEO
            List<DOrganizationRAEO> tempDORAEO = new();

            //Определяем количество организаций
            int organizationsCount = organizationFilter.Value.GetEntitiesCount();

            //Для каждого RAEO
            foreach (int rAEOEntity in regionAEOFilter.Value)
            {
                //Берём компонент RAEO
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                //Очищаем временный список
                tempDORAEO.Clear();

                //Для каждого самозапроса создания ORAEO
                foreach (int gameCreateORAEOPanelEventEntity in oRAEOCreateSRFilter.Value)
                {
                    //Берём компонент самозапроса и компонент организации
                    ref COrganization organization = ref organizationPool.Value.Get(gameCreateORAEOPanelEventEntity);

                    //Создаём новую сущность и назначаем ей компонент ExplorationORAEO
                    int oRAEOEntity = world.Value.NewEntity();
                    ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Add(oRAEOEntity);

                    //Заполняем основные данные ExORAEO
                    exORAEO = new(
                        world.Value.PackEntity(oRAEOEntity),
                        organization.selfPE,
                        rAEO.selfPE,
                        0);

                    //Создаём структуру для хранения данных организации непосредственно в RAEO
                    DOrganizationRAEO organizationRAEOData = new(
                        exORAEO.selfPE,
                        ORAEOType.Exploration);

                    //Заносим её во временный список
                    tempDORAEO.Add(organizationRAEOData);
                }

                //Сохраняем старый размер массива
                int oldArraySize = rAEO.organizationRAEOs.Length;

                //Расширяем массив DORAEO
                Array.Resize(
                    ref rAEO.organizationRAEOs, 
                    organizationsCount);

                //Для каждого DORAEO во временном массиве
                for (int a = 0; a < tempDORAEO.Count; a++)
                {
                    //Вставляем DORAEO в массив по индексу
                    rAEO.organizationRAEOs[oldArraySize++] = tempDORAEO[a];
                }
            }
        }
    }
}