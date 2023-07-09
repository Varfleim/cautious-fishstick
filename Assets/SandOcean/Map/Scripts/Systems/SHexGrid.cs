
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
        //Миры
        readonly EcsWorldInject world = default;


        //Объекты карты
        //readonly EcsPoolInject<CMapObject> mapObjectPool = default;

        //Карта
        readonly EcsPoolInject<CHexChunk> chunkPool = default;

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
        readonly EcsCustomInject<SpaceGenerationData> spaceGenerationData = default;
        //readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события генерации карты
            foreach (int mapGenerationEventEntity in mapGenerationEventFilter.Value)
            {
                //Берём компонент события генерации карты
                ref EMapGeneration mapGenerationEvent= ref mapGenerationEventPool.Value.Get(mapGenerationEventEntity);

                //Инициализируем хэш-таблицу
                SpaceGenerationData.InitializeHashGrid();

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

            //Создаём структуру, хранящую данные для генерации карты
            //TempSectorGenerationData tempSectorGenerationData = new();

            //Распределяем богатство карты
            MapWealthDistribution();

            //Генерируем карту 
            MapGeneration(ref mapGenerationEvent);
        }

        void MapInitialization(
            ref EMapGeneration mapGenerationEvent)
        {
            //Назначаем текстуру шума
            SpaceGenerationData.noiseSource
                = spaceGenerationData.Value.noiseTexture;

            //Определяем количество чанков
            spaceGenerationData.Value.chunkCountX
                = mapGenerationEvent.chunkCountX;
            spaceGenerationData.Value.chunkCountZ
                = mapGenerationEvent.chunkCountZ;

            //Определяем размер массива PE чанков
            spaceGenerationData.Value.chunkPEs = new EcsPackedEntity[spaceGenerationData.Value.chunkCountX * spaceGenerationData.Value.chunkCountZ];

            //Определяем количество регионов
            spaceGenerationData.Value.regionCountX
                = mapGenerationEvent.chunkCountX
                * SpaceGenerationData.chunkSizeX;
            spaceGenerationData.Value.regionCountZ
                = mapGenerationEvent.chunkCountZ
                * SpaceGenerationData.chunkSizeZ;

            //Определяем размер массива PE регионов
            spaceGenerationData.Value.regionPEs = new EcsPackedEntity[spaceGenerationData.Value.regionCountX * spaceGenerationData.Value.regionCountZ];
        }

        void MapWealthDistribution()
        {

        }

        void MapGeneration(
            ref EMapGeneration mapGenerationEvent)
        {
            //Создаём чанки
            MapCreateChunks(ref mapGenerationEvent);

            //Создаём регионы
            MapCreateRegions(ref mapGenerationEvent);

            //Создаём острова
            //MapCreateIslands(ref mapGenerationEvent);
        }

        void MapCreateChunks(
            ref EMapGeneration mapGenerationEvent)
        {
            //Для каждого чанка по высоте
            for (int z = 0, i = 0; z < spaceGenerationData.Value.chunkCountZ; z++)
            {
                //Для каждого чанка по ширине
                for (int x = 0; x < spaceGenerationData.Value.chunkCountX; x++)
                {
                    //Создаём чанк
                    ChunkCreate(
                        ref mapGenerationEvent,
                        x, z,
                        i++);
                }
            }

            //Создаём стартовые чанки
            /*ChunkCreate(
                0, 0);*/
        }

        void MapCreateRegions(
            ref EMapGeneration mapGenerationEvent)
        {
            //Для каждого региона по высоте
            for (int z = 0, i = 0; z < spaceGenerationData.Value.regionCountZ; z++)
            {
                //Для каждого региона по ширине
                for (int x = 0; x < spaceGenerationData.Value.regionCountX; x++)
                {
                    //Создаём регион
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
                SpaceGenerationData.ChunkSize);

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

            //Присоединяем чанк к центральному объекту
            chunk.transform.SetParent(sceneData.Value.coreObject, false);

            //Заносим чанк в массив чанков
            spaceGenerationData.Value.chunkPEs[chunkIndex] = chunk.selfPE;

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
            int parentChunkX = regionGlobalX / SpaceGenerationData.chunkSizeX;
            int parentChunkZ = regionGlobalZ / SpaceGenerationData.chunkSizeZ;
            spaceGenerationData.Value.chunkPEs[parentChunkX + parentChunkZ * spaceGenerationData.Value.chunkCountX].Unpack(
                world.Value, out int parentChunkEntity);
            ref CHexChunk parentChunk = ref chunkPool.Value.Get(parentChunkEntity);

            int regionLocalX = regionGlobalX - parentChunkX * SpaceGenerationData.chunkSizeX;
            int regionLocalZ = regionGlobalZ - parentChunkZ * SpaceGenerationData.chunkSizeZ;

            //Создаём новую сущность и назначаем ей компонент региона и RAEO
            int regionEntity = world.Value.NewEntity();
            ref CHexRegion currentRegion = ref regionPool.Value.Add(regionEntity);
            ref CRegionAEO currentRAEO = ref regionAEOPool.Value.Add(regionEntity);

            //Определяем позицию региона
            Vector3 position = new(
                (regionGlobalX + regionGlobalZ * 0.5f - regionGlobalZ / 2) * (SpaceGenerationData.innerRadius * 2f),
                0f,
                regionGlobalZ * (SpaceGenerationData.outerRadius * 1.5f));
            //Определяем координаты региона
            DHexCoordinates regionCoordinates = DHexCoordinates.FromOffsetCoordinates(regionGlobalX, regionGlobalZ);

            //Создаём объект региона
            GORegion regionGO = GameObject.Instantiate(staticData.Value.regionPrefab);

            //Заполняем основные данные региона
            currentRegion = new(
                world.Value.PackEntity(regionEntity),
                position, regionCoordinates,
                regionGO.regionTransform, regionGO.regionLabel, regionGO.regionHighlight,
                parentChunk.selfPE);

            //Перемещаем объект региона на соответствующую позицию
            currentRegion.transform.localPosition = currentRegion.Position;

            //Перемещаем объект метки на соответствующую позицию и отображаем координаты региона
            currentRegion.uiRect.rectTransform.anchoredPosition = new(currentRegion.Position.x, currentRegion.Position.z);

            //Заносим регион в массив регионов родительского чанка
            parentChunk.regionPEs[regionLocalX + regionLocalZ * SpaceGenerationData.chunkSizeX] = currentRegion.selfPE;

            //Прикрепляем объекты региона к объекту чанка
            currentRegion.transform.SetParent(parentChunk.transform, false);
            currentRegion.uiRect.rectTransform.SetParent(parentChunk.canvas.transform, false);
            currentRegion.highlight.rectTransform.SetParent(parentChunk.canvas.transform);

            //Заносим регион в массив регионов
            spaceGenerationData.Value.regionPEs[regionIndex] = currentRegion.selfPE;

            //Если регион не находится в крайнем левом столбце
            if (regionGlobalX > 0)
            {
                //Берём соседа с запада
                spaceGenerationData.Value.regionPEs[regionIndex - 1].Unpack(world.Value, out int wNeighbourRegionEntity);
                ref CHexRegion wNeighbourRegion = ref regionPool.Value.Get(wNeighbourRegionEntity);

                //Создаём соседство
                RegionSetNeighbour(
                    ref currentRegion, ref wNeighbourRegion,
                    HexDirection.W);
            }
            //Если регион не находится в крайнем нижнем ряду
            if (regionGlobalZ > 0)
            {
                //Если регион находится в нечётном ряду
                if ((regionGlobalZ & 1) == 0)
                {
                    //Берём соседа с юго-востока
                    spaceGenerationData.Value.regionPEs[regionIndex - spaceGenerationData.Value.regionCountX].Unpack(world.Value, out int sENeighbourRegionEntity);
                    ref CHexRegion sENeighbourRegion = ref regionPool.Value.Get(sENeighbourRegionEntity);

                    //Создаём соседство
                    RegionSetNeighbour(
                        ref currentRegion, ref sENeighbourRegion,
                        HexDirection.SE);

                    //Если регион не находится в крайнем левом столбце
                    if (regionGlobalX > 0)
                    {
                        //Берём соседа с юго-запада
                        spaceGenerationData.Value.regionPEs[regionIndex - spaceGenerationData.Value.regionCountX - 1].Unpack(world.Value, out int sWNeighbourRegionEntity);
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
                    spaceGenerationData.Value.regionPEs[regionIndex - spaceGenerationData.Value.regionCountX].Unpack(world.Value, out int sWNeighbourRegionEntity);
                    ref CHexRegion sWNeighbourRegion = ref regionPool.Value.Get(sWNeighbourRegionEntity);

                    //Создаём соседство
                    RegionSetNeighbour(
                        ref currentRegion, ref sWNeighbourRegion,
                        HexDirection.SW);

                    //Если регион не находится в крайнем правом столбце
                    if (regionGlobalX < spaceGenerationData.Value.regionCountX - 1)
                    {
                        //Берём соседа с юго-востока
                        spaceGenerationData.Value.regionPEs[regionIndex - spaceGenerationData.Value.regionCountX + 1].Unpack(world.Value, out int sENeighbourRegionEntity);
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