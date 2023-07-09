
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using SandOcean.Map.Events;
using SandOcean.Diplomacy;
using SandOcean.AEO.RAEO;

namespace SandOcean.Map
{
    public class SMapControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Карта
        //readonly EcsFilterInject<Inc<CHexChunk>> chunkFilter = default;
        readonly EcsPoolInject<CHexChunk> chunkPool = default;

        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Организации
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Административно-экономические объекты
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        //События карты
        readonly EcsFilterInject<Inc<RChangeMapMode>> changeMapModeRequestFilter = default;
        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        readonly EcsFilterInject<Inc<SRMapChunkRefresh, CHexChunk>> refreshChunkSelfRequestFilter = default;
        readonly EcsPoolInject<SRMapChunkRefresh> mapChunkRefreshSelfRequestPool = default;

        //Данные
        readonly EcsCustomInject<SpaceGenerationData> spaceGenerationData = default;
        readonly EcsCustomInject<UI.InputData> inputData;

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса смены режима карты
            foreach (int changeMapModeREntity in changeMapModeRequestFilter.Value)
            {
                //Берём компонент запроса
                ref RChangeMapMode changeMapModeR = ref changeMapModeRequestPool.Value.Get(changeMapModeREntity);

                //Если требуется отобразить режим расстояния
                if (changeMapModeR.mapMode == UI.MapMode.Distance)
                {
                    //Рассчитываем расстояния
                    MapModeDistanceCalculate();

                    //Указываем, что активный режим карты - режим расстояния
                    inputData.Value.mapMode = UI.MapMode.Distance;
                }

                //Удаляем сущность запроса
                world.Value.DelEntity(changeMapModeREntity);
            }

            //Для каждого чанка, который требуется обновить
            foreach (int refreshChunkEntity in refreshChunkSelfRequestFilter.Value)
            {
                //Берём компонент чанка
                ref CHexChunk chunk = ref chunkPool.Value.Get(refreshChunkEntity);

                //Триангулируем меш чанка
                //ChunkTriangulate(ref chunk);
                ChunkSimpleTriangulate(ref chunk);

                Debug.LogWarning("Chunk refreshed!");

                //Удаляем с сущности чанка самозапрос обновления
                mapChunkRefreshSelfRequestPool.Value.Del(refreshChunkEntity);
            }
        }

        void MapModeDistanceCalculate()
        {
            //Берём компонент организации игрока
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //Если активен не режим расстояния
            if (inputData.Value.mapMode != UI.MapMode.Distance)
            {
                //Для каждого региона
                foreach (int regionEntity in regionFilter.Value)
                {
                    //Берём компонент региона
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    //Обнуляем расстояние до региона
                    region.mapDistance = 0;

                    //Запрашиваем триангуляцию чанка
                    ChunkRefreshSelfRequest(ref region);
                }
            }

            //Для каждого ORAEO организации
            for (int a = 0; a < organization.ownedORAEOPEs.Count; a++)
            {
                //Берём компонент ORAEO
                organization.ownedORAEOPEs[a].Unpack(world.Value, out int oRAEOEntity);
                ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
                ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

                //Берём родительский регион и RAEO
                exORAEO.parentRAEOPE.Unpack(world.Value, out int rAEOEntity);
                ref CHexRegion region = ref regionPool.Value.Get(rAEOEntity); 
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);


                //Определяем радиус 
                int radius = 2;

                //Берём координаты ячейки
                int centerX = region.coordinates.X;
                int centerZ = region.coordinates.Z;

                //Если уже активен режим расстояния
                if (inputData.Value.mapMode == UI.MapMode.Distance)
                {
                    //Если расстояние до региона не равно новому
                    if (region.mapDistance != 2)
                    {
                        //Отмечаем расстояние до региона
                        region.mapDistance = 2;

                        //Запрашиваем триангуляцию чанка
                        ChunkRefreshSelfRequest(ref region);
                    }

                    for (int r = 0, z = centerZ - radius; z <= centerZ; z++, r++)
                    {
                        for (int x = centerX - r; x <= centerX + radius; x++)
                        {
                            //Если существует регион с такими координатами
                            if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
                            {
                                //Берём компонент региона
                                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(cellEntity);

                                //Есил расстояние до региона не равно новому
                                if (neighbourRegion.mapDistance == 0)
                                {
                                    //Указываем расстояние до региона
                                    neighbourRegion.mapDistance = 1;

                                    //Запрашиваем триангуляцию чанка
                                    ChunkRefreshSelfRequest(ref neighbourRegion);
                                }
                            }
                        }
                    }

                    for (int r = 0, z = centerZ + radius; z > centerZ; z--, r++)
                    {
                        for (int x = centerX - radius; x <= centerX + r; x++)
                        {
                            //Если существует регион с такими координатами
                            if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
                            {
                                //Берём компонент региона
                                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(cellEntity);

                                //Есил расстояние до региона не равно новому
                                if (neighbourRegion.mapDistance == 0)
                                {
                                    //Указываем расстояние до региона
                                    neighbourRegion.mapDistance = 1;

                                    //Запрашиваем триангуляцию чанка
                                    ChunkRefreshSelfRequest(ref neighbourRegion);
                                }
                            }
                        }
                    }
                }
                //Иначе
                else
                {
                    //Отмечаем расстояние до региона
                    region.mapDistance = 2;

                    for (int r = 0, z = centerZ - radius; z <= centerZ; z++, r++)
                    {
                        for (int x = centerX - r; x <= centerX + radius; x++)
                        {
                            //Если существует регион с такими координатами
                            if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
                            {
                                //Берём компонент региона
                                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(cellEntity);

                                //Указываем расстояние до региона
                                neighbourRegion.mapDistance = 1;
                            }
                        }
                    }

                    for (int r = 0, z = centerZ + radius; z > centerZ; z--, r++)
                    {
                        for (int x = centerX - radius; x <= centerX + r; x++)
                        {
                            //Если существует регион с такими координатами
                            if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
                            {
                                //Берём компонент региона
                                ref CHexRegion neighbourRegion = ref regionPool.Value.Get(cellEntity);

                                //Указываем расстояние до региона
                                neighbourRegion.mapDistance = 1;
                            }
                        }
                    }
                }
            }
        }

        void ChunkTriangulate(
            ref CHexChunk chunk)
        {
            //Очищаем меши
            chunk.terrain.Clear();
            chunk.rivers.Clear();
            chunk.roads.Clear();
            chunk.water.Clear();
            chunk.waterShore.Clear();
            chunk.estuaries.Clear();

            chunk.features.Clear();

            //Для каждой ячейки чанка
            for (int a = 0; a < chunk.regionPEs.Length; a++)
            {
                //Берём компонент ячейки
                chunk.regionPEs[a].Unpack(world.Value, out int cellEntity);
                ref CHexRegion cell
                    = ref regionPool.Value.Get(cellEntity);

                //Триангулируем ячейку
                CellTriangulate(
                    ref chunk,
                    ref cell);
            }

            //Заполняем меши
            chunk.terrain.Apply();
            chunk.rivers.Apply();
            chunk.roads.Apply();
            chunk.water.Apply();
            chunk.waterShore.Apply();
            chunk.estuaries.Apply();

            chunk.features.Apply();
        }

        void ChunkSimpleTriangulate(
            ref CHexChunk chunk)
        {
            //Очищаем меши
            chunk.terrain.Clear();
            chunk.rivers.Clear();
            chunk.roads.Clear();
            chunk.water.Clear();
            chunk.waterShore.Clear();
            chunk.estuaries.Clear();

            chunk.features.Clear();

            //Для каждой ячейки чанка
            for (int a = 0; a < chunk.regionPEs.Length; a++)
            {
                //Берём компонент ячейки
                chunk.regionPEs[a].Unpack(world.Value, out int cellEntity);
                ref CHexRegion cell = ref regionPool.Value.Get(cellEntity);

                //Триангулируем ячейку
                CellSimpleTriangulate(
                    ref chunk,
                    ref cell);
            }

            //Заполняем меши
            chunk.terrain.Apply();
            chunk.rivers.Apply();
            chunk.roads.Apply();
            chunk.water.Apply();
            chunk.waterShore.Apply();
            chunk.estuaries.Apply();

            chunk.features.Apply();
        }

        void CellTriangulate(
            ref CHexChunk chunk,
            ref CHexRegion cell)
        {
            //Обновляем положение метки ячейки
            Vector3 uiPosition
                = cell.uiRect.rectTransform.localPosition;
            uiPosition.z
                = cell.Elevation * -SpaceGenerationData.elevationStep;
            cell.uiRect.rectTransform.localPosition
                = uiPosition;

            //Для каждого треугольника ячейки
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //Триангулируем треугольник
                TriangulateEdge(
                    ref chunk,
                    ref cell,
                    d);
            }

            //Если ячейка не находится под водой
            if (cell.IsUnderwater == false)
            {
                //Если ячейка свободна
                if (cell.HasRiver == false
                    && cell.HasRoads == false)
                {
                    //Добавляем объект
                    chunk.features.AddFeature(
                        ref cell,
                        cell.Position);
                }
                //Если ячейка имеет особый объект
                if (cell.IsSpecial == true)
                {
                    chunk.features.AddSpecialFeature(
                        ref cell,
                        cell.Position);
                }
            }
        }

        void CellSimpleTriangulate(
            ref CHexChunk chunk,
            ref CHexRegion cell)
        {
            //Обновляем положение метки ячейки
            Vector3 uiPosition = cell.uiRect.rectTransform.localPosition;
            uiPosition.z = cell.Elevation * -SpaceGenerationData.elevationStep;
            cell.uiRect.rectTransform.localPosition = uiPosition;

            //Для каждого треугольника ячейки
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //Триангулируем треугольник
                SimpleTriangulateEdge(
                    ref chunk,
                    ref cell,
                    d);
            }
        }

        void TriangulateEdge(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction)
        {
            //Берём основные вершины треугольника
            Vector3 center = cell.Position;
            DHexEdgeVertices e
                = new(
                    center + SpaceGenerationData.GetFirstSolidCorner(
                        direction),
                    center + SpaceGenerationData.GetSecondSolidCorner(
                        direction));

            //Если ячейка имеет реку
            if (cell.HasRiver == true)
            {
                //Если ячейка имеет реку через данное ребро
                if (cell.HasRiverThroughEdge(direction) == true)
                {
                    //Опускаем центральную вершину ребра
                    e.v3.y = cell.StreamBedY;

                    //Если ячейка имеет начало или конец реки
                    if (cell.HasRiverBeginOrEnd == true)
                    {
                        //Триангулируем ребро с началом или концом реки
                        TriangulateWithRiverBeginOrEnd(
                            ref chunk,
                            ref cell,
                            direction,
                            center,
                            e);
                    }
                    //Иначе
                    else
                    {
                        //Триангулируем ребро с рекой
                        TriangulateWithRiver(
                            ref chunk,
                            ref cell,
                            direction,
                            center,
                            e);
                    }
                }
                //Иначе
                else
                {
                    //Триангулируем ребро около реки
                    TriangulateAdjacentToRiver(
                        ref chunk,
                        ref cell,
                        direction,
                        center,
                        e);
                }
            }
            //Иначе
            else
            {
                //Триангулируем ребро без реки
                TriangulateWithoutRiver(
                    ref chunk,
                    ref cell,
                    direction,
                    center,
                    e);

                //Если ячейка не под водой и не имеет дороги через ребро
                if (cell.IsUnderwater == false
                    && cell.HasRoadThroughEdge(direction) == false)
                {
                    //Добавляем объект
                    chunk.features.AddFeature(
                        ref cell,
                        (center + e.v1 + e.v5) * (1f / 3f));
                }
            }

            //Если направление меньше или равно юго-востоку
            if (direction <= HexDirection.SE)
            {
                //Триангулируем соединение
                TriangulateConnection(
                    ref chunk,
                    ref cell,
                    direction, 
                    e);
            }

            //Если ячейка находится под водой
            if (cell.IsUnderwater == true)
            {
                //Триангулируем воду
                TriangulateWater(
                    ref chunk,
                    ref cell,
                    direction,
                    center);
            }
        }

        void SimpleTriangulateEdge(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction)
        {
            //Берём основные вершины треугольника
            Vector3 center = cell.Position;
            DHexEdgeVertices e = new(
                center + SpaceGenerationData.GetFirstCorner(direction),
                center + SpaceGenerationData.GetSecondCorner(direction));

            //Триангулируем треугольник без реки
            SimpleTriangulateWithoutRiver(
                ref chunk,
                ref cell,
                direction,
                center,
                e);
        }

        void TriangulateWithoutRiver(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //Триангулируем веер ребра
            TriangulateEdgeFan(
                ref chunk,
                center,
                e,
                cell.TerrainTypeIndex);

            //Если ячейка имеет дорогу через данное ребро
            if (cell.HasRoads == true)
            {
                //Определяем смешение
                Vector2 interpolators
                    = GetRoadInterpolators(
                        ref cell,
                        direction);

                //Триангулируем дорогу
                TriangulateRoad(
                    ref chunk,
                    center,
                    Vector3.Lerp(
                        center, e.v1,
                        interpolators.x),
                    Vector3.Lerp(
                        center, e.v5,
                        interpolators.y), 
                    e,
                    cell.HasRoadThroughEdge(direction));
            }
        }

        void SimpleTriangulateWithoutRiver(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //Определяем, какую текстуру требуется отобразить
            float textureIndex = -1;

            //Если не активен какой-либо режим карты
            if (inputData.Value.mapMode == UI.MapMode.Default)
            {
                textureIndex = cell.TerrainTypeIndex;
            }
            //Иначе, если активен режим расстояния
            else if (inputData.Value.mapMode == UI.MapMode.Distance)
            {
                textureIndex = cell.mapDistance;
            }

            //Триангулируем веер ребра
            TriangulateEdgeFan(
                ref chunk,
                center,
                e,
                textureIndex);
        }

        void TriangulateConnection(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            DHexEdgeVertices e1)
        {
            //Если у ячейки есть сосед с данного направления
            if (cell.GetNeighbour(direction).Unpack(world.Value, out int neighbourCellEntity))
            {
                //Берём компонент соседней ячейки
                ref CHexRegion neighbourCell
                    = ref regionPool.Value.Get(neighbourCellEntity);

                //Берём основные данные квада
                Vector3 bridge
                    = SpaceGenerationData.GetBridge(
                        direction);
                bridge.y = neighbourCell.Position.y - cell.Position.y;
                DHexEdgeVertices e2
                    = new(
                        e1.v1 + bridge,
                        e1.v5 + bridge);

                //Определяем, имеет ли ячейка реку или дорогу через ребро
                bool hasRiver = cell.HasRiverThroughEdge(direction);
                bool hasRoad = cell.HasRoadThroughEdge(direction);

                //Если ячейка имеет реку через данное ребро
                if (hasRiver == true)
                {
                    //Опускаем центральную вершину ребра
                    e2.v3.y = neighbourCell.StreamBedY;

                    //Если ячейка не находится под водой
                    if (cell.IsUnderwater == false)
                    {
                        //Если сосед не находится под водой
                        if (neighbourCell.IsUnderwater == false)
                        {
                            //Триангулируем течение реки
                            TriangulateRiverQuad(
                                ref chunk,
                                e1.v2, e1.v4, e2.v2, e2.v4,
                                cell.RiverSurfaceY, neighbourCell.RiverSurfaceY,
                                0.8f,
                                cell.HasIncomingRiver && cell.IncomingRiver == direction);
                        }
                        //Иначе
                        else
                        {
                            //Триангулируем водопад
                            TriangulateWaterfallInWater(
                                ref chunk,
                                e1.v2, e1.v4, e2.v2, e2.v4,
                                cell.RiverSurfaceY, neighbourCell.RiverSurfaceY,
                                neighbourCell.WaterSurfaceY
                                );
                        }
                    }
                    //Иначе, если сосед не находится под водой
                    else if (neighbourCell.IsUnderwater == false
                        //И высота соседа больше уровня воды ячейки
                        && neighbourCell.Elevation > cell.WaterLevel)
                    {
                        //Триангулируем водопад
                        TriangulateWaterfallInWater(
                            ref chunk,
                            e2.v4, e2.v2, e1.v4, e1.v2,
                            neighbourCell.RiverSurfaceY, cell.RiverSurfaceY,
                            cell.WaterSurfaceY);
                    }
                }

                //Если тип ребра - наклон
                if (SpaceGenerationData.GetEdgeType(cell.Elevation, neighbourCell.Elevation) == HexEdgeType.Slope)
                {
                    //Триангулируем террасы ребра
                    TriangulateEdgeTerraces(
                        ref chunk,
                        ref cell, e1,
                        ref neighbourCell, e2,
                        hasRoad);
                }
                //Иначе
                else
                {
                    //Триангулируем полосу ребра
                    TriangulateEdgeStrip(
                        ref chunk,
                        e1, SpaceGenerationData.cellColor1, cell.TerrainTypeIndex,
                        e2, SpaceGenerationData.cellColor2, neighbourCell.TerrainTypeIndex,
                        hasRoad);
                }

                //Заносим стену в меш
                chunk.features.AddWall(
                    ref cell, e1,
                    ref neighbourCell, e2,
                    hasRiver,
                    hasRoad);

                //Если направление меньше или равно востоку
                if (direction <= HexDirection.E
                    //И у ячейки есть сосед со следующего направления
                    && cell.GetNeighbour(direction.Next()).Unpack(world.Value, out int nextNeighbourCellEntity))
                {
                    //Берём компонент следующего соседа
                    ref CHexRegion nextNeighbourCell
                        = ref regionPool.Value.Get(nextNeighbourCellEntity);

                    //Определяем высоту вершины треугольника
                    Vector3 v5 
                        = e1.v5 + SpaceGenerationData.GetBridge(
                            direction.Next());
                    v5.y = nextNeighbourCell.Position.y;

                    //Если высота ячейки меньше высоты соседа
                    if (cell.Elevation <= neighbourCell.Elevation)
                    {
                        //Если высота ячейки меньше высоты следующего соседа
                        if (cell.Elevation <= nextNeighbourCell.Elevation)
                        {
                            //Триангулируем угол
                            TriangulateCorner(
                                ref chunk,
                                ref cell, e1.v5,
                                ref neighbourCell, e2.v5,
                                ref nextNeighbourCell, v5);
                        }
                        //Иначе
                        else
                        {
                            //Триангулируем угол
                            TriangulateCorner(
                                ref chunk,
                                ref nextNeighbourCell, v5,
                                ref cell, e1.v5,
                                ref neighbourCell, e2.v5);
                        }
                    }
                    //Иначе, если высота соседа меньше высоты следующего соседа
                    else if (neighbourCell.Elevation <= nextNeighbourCell.Elevation)
                    {
                        //Триангулируем угол
                        TriangulateCorner(
                            ref chunk,
                            ref neighbourCell, e2.v5,
                            ref nextNeighbourCell, v5,
                            ref cell, e1.v5);
                    }
                    //Иначе
                    else
                    {
                        //Триангулируем угол
                        TriangulateCorner(
                            ref chunk,
                            ref nextNeighbourCell, v5,
                            ref cell, e1.v5,
                            ref neighbourCell, e2.v5);
                    }
                }
            }
        }

        void TriangulateWithRiver(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //Определяем боковые вершины "центра"
            Vector3 centerL;
            Vector3 centerR;
            //Если ячейка имеет реку через противоположное ребро
            if (cell.HasRiverThroughEdge(direction.Opposite()) == true)
            {
                //Определяем положения вершин
                centerL
                    = center 
                    + SpaceGenerationData.GetFirstSolidCorner(direction.Previous()) * 0.25f;
                centerR
                    = center 
                    + SpaceGenerationData.GetSecondSolidCorner(direction.Next()) * 0.25f;
            }
            //Иначе, если ячейка имеет реку через следующее ребро
            else if(cell.HasRiverThroughEdge(direction.Next()) == true)
            {
                //Определяем положения вершин
                centerL = center;
                centerR
                    = Vector3.Lerp(
                        center, e.v5, 
                        2f / 3f);
            }
            //Иначе, если ячейка имеет реку через предыдущее ребро
            else if(cell.HasRiverThroughEdge(direction.Previous()) == true)
            {
                //Определяем положения вершин
                centerL
                    = Vector3.Lerp(
                        center, e.v1,
                        2f / 3f);
                centerR = center;
            }
            //Иначе, если ячейка имеет реку через дважды следующее ребро
            else if(cell.HasRiverThroughEdge(direction.Next2()) == true)
            {
                //Определяем положение вершин
                centerL = center;
                centerR 
                    = center 
                    + SpaceGenerationData.GetSolidEdgeMiddle(direction.Next()) 
                    * (0.5f * SpaceGenerationData.innerToOuter);
            }
            //Иначе
            else
            {
                //Определяем положения вершин
                centerL 
                    = center 
                    + SpaceGenerationData.GetSolidEdgeMiddle(direction.Previous()) 
                    * (0.5f * SpaceGenerationData.innerToOuter);
                centerR = center;
            }
            //Уточняем положение центральной вершины
            center
                = Vector3.Lerp(
                    centerL, centerR, 
                    0.5f);

            //Определяем среднюю линию
            DHexEdgeVertices m
                = new(
                    Vector3.Lerp(
                        centerL, e.v1,
                        0.5f),
                    Vector3.Lerp(
                        centerR, e.v5,
                        0.5f),
                    1f / 6f);

            //Опускаем центральные вершины
            m.v3.y = center.y = e.v3.y;

            //Триангулируем русло
            TriangulateEdgeStrip(
                ref chunk,
                m, SpaceGenerationData.cellColor1, cell.TerrainTypeIndex,
                e, SpaceGenerationData.cellColor1, cell.TerrainTypeIndex);

            //Заносим русло в меш
            chunk.terrain.AddTriangle(
                centerL, m.v1, m.v2);
            chunk.terrain.AddQuad(
                centerL, center, m.v2, m.v3);
            chunk.terrain.AddQuad(
                center, centerR, m.v3, m.v4);
            chunk.terrain.AddTriangle(
                centerR, m.v4, m.v5);

            chunk.terrain.AddTriangleColor(
                SpaceGenerationData.cellColor1);
            chunk.terrain.AddQuadColor(
                SpaceGenerationData.cellColor1);
            chunk.terrain.AddQuadColor(
                SpaceGenerationData.cellColor1);
            chunk.terrain.AddTriangleColor(
                SpaceGenerationData.cellColor1);

            Vector3 types;
            types.x = types.y = types.z = cell.TerrainTypeIndex;
            chunk.terrain.AddTriangleTerrainTypes(types);
            chunk.terrain.AddQuadTerrainTypes(types);
            chunk.terrain.AddQuadTerrainTypes(types);
            chunk.terrain.AddTriangleTerrainTypes(types);

            //Если ячейка не находится под водой
            if (cell.IsUnderwater == false)
            {
                //Определяем, должна ли быть развёрнута река
                bool reversed
                    = cell.IncomingRiver == direction;

                //Заносим течение реки в меш
                TriangulateRiverQuad(
                    ref chunk,
                    centerL, centerR, m.v2, m.v4,
                    cell.RiverSurfaceY,
                    0.4f,
                    reversed);
                TriangulateRiverQuad(
                    ref chunk,
                    m.v2, m.v4, e.v2, e.v4,
                    cell.RiverSurfaceY,
                    0.6f,
                    reversed);
            }
        }

        void TriangulateWithRiverBeginOrEnd(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //Определяем среднюю линию
            DHexEdgeVertices m 
                = new(
                    Vector3.Lerp(center, e.v1, 0.5f),
                    Vector3.Lerp(center, e.v5, 0.5f));

            //Опускаем центральную вершину
            m.v3.y = e.v3.y;

            //Триангулируем исток
            TriangulateEdgeStrip(
                ref chunk,
                m, SpaceGenerationData.cellColor1, cell.TerrainTypeIndex,
                e, SpaceGenerationData.cellColor1, cell.TerrainTypeIndex);
            TriangulateEdgeFan(
                ref chunk,
                center,
                m,
                cell.TerrainTypeIndex);

            //Если ячейка не находится под водой
            if (cell.IsUnderwater == false)
            {
                //Определяем, должна ли быть развёрнута река
                bool reversed = cell.HasIncomingRiver;

                //Заносим течение реки в меш
                TriangulateRiverQuad(
                    ref chunk,
                    m.v2, m.v4, e.v2, e.v4,
                    cell.RiverSurfaceY,
                    0.6f,
                    reversed);

                //Определяем положене центра течения
                center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;

                //Заносим треугольник в меш
                chunk.rivers.AddTriangle(center, m.v2, m.v4);
                //Если течение развёрнуто
                if (reversed == true)
                {
                    //Заносим UV-координаты в меш
                    chunk.rivers.AddTriangleUV(
                        new(0.5f, 0.4f), new(1f, 0.2f), new(0f, 0.2f));
                }
                else
                {
                    //Заносим UV-координаты в меш
                    chunk.rivers.AddTriangleUV(
                        new(0.5f, 0.4f), new(0f, 0.6f), new(1f, 0.6f));
                }
            }
        }

        void TriangulateAdjacentToRiver(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //Если ячейка имеет дороги
            if(cell.HasRoads == true)
            {
                //Триангулируем дорогу возле реки
                TriangulateRoadAdjacentToRiver(
                    ref chunk,
                    ref cell,
                    direction,
                    center,
                    e);
            }   
            
            //Если ячейка имеет реку через следующее ребро
            if (cell.HasRiverThroughEdge(direction.Next()) == true)
            {
                //Если ячейка имеет реку через предыдущее ребро
                if (cell.HasRiverThroughEdge(direction.Previous()) == true)
                {
                    //Смещаем центр
                    center
                        += SpaceGenerationData.GetSolidEdgeMiddle(direction)
                        * (SpaceGenerationData.innerToOuter * 0.5f);
                }
                //Иначе, если ячейка имеет реку через дважды предыдущее ребро
                else if(cell.HasRiverThroughEdge(direction.Previous2()) == true)
                {
                    //Смещаем центр
                    center
                        += SpaceGenerationData.GetFirstSolidCorner(direction)
                        * 0.25f;
                }
            }
            //Иначе, если ячейка имеет реку через предыдущее и дважды следующее рёбра
            else if(cell.HasRiverThroughEdge(direction.Previous())
                && cell.HasRiverThroughEdge(direction.Next2()))
            {
                //Смещаем центр
                center += SpaceGenerationData.GetSecondSolidCorner(direction) * 0.25f;
            }

            //Определяем среднюю линию
            DHexEdgeVertices m 
                = new(
                    Vector3.Lerp(
                        center, e.v1, 
                        0.5f),
                    Vector3.Lerp(
                        center, e.v5, 
                        0.5f));

            //Триангулируем ребро
            TriangulateEdgeStrip(
                ref chunk,
                m, SpaceGenerationData.cellColor1, cell.TerrainTypeIndex, 
                e, SpaceGenerationData.cellColor1, cell.TerrainTypeIndex);
            TriangulateEdgeFan(
                ref chunk,
                center, 
                m,
                cell.TerrainTypeIndex);

            //Если ячейка не под водой и не имеет дороги через ребро
            if (cell.IsUnderwater == false
                && cell.HasRoadThroughEdge(direction) == false)
            {
                //Добавляем объект
                chunk.features.AddFeature(
                    ref cell,
                    (center + e.v1 + e.v5) * (1f / 3f));
            }
        }

        void TriangulateWaterfallInWater(
            ref CHexChunk chunk,
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y1, float y2, 
            float waterY)
        {
            //Определяем вершины водопада
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;
            v1 = SpaceGenerationData.Perturb(v1);
            v2 = SpaceGenerationData.Perturb(v2);
            v3 = SpaceGenerationData.Perturb(v3);
            v4 = SpaceGenerationData.Perturb(v4);
            float t 
                = (waterY - y2) / (y1 - y2);
            v3 = Vector3.Lerp(
                v3, v1, 
                t);
            v4 = Vector3.Lerp(
                v4, v2, 
                t);

            //Заносим квад в меш
            chunk.rivers.AddQuadUnperturbed(v1, v2, v3, v4);
            chunk.rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
        }

        void TriangulateEdgeTerraces(
            ref CHexChunk chunk,
            ref CHexRegion beginCell, DHexEdgeVertices begin,
            ref CHexRegion endCell, DHexEdgeVertices end,
            bool hasRoad)
        {
            //Определяем конечные вершины первого квада
            DHexEdgeVertices e2 = DHexEdgeVertices.TerraceLerp(
                begin, end,
                1);
            Color c2 = SpaceGenerationData.TerraceLerp(
                SpaceGenerationData.cellColor1, SpaceGenerationData.cellColor2, 
                1);
            float t1 = beginCell.TerrainTypeIndex;
            float t2 = endCell.TerrainTypeIndex;

            //Заносим первую полосу в меш
            TriangulateEdgeStrip(
                ref chunk,
                begin, SpaceGenerationData.cellColor1, t1,
                e2, c2, t2,
                hasRoad);

            //Для каждого промежуточного квада
            for (int a = 2; a < SpaceGenerationData.terraceSteps; a++)
            {
                //Определяем начальные данные квада
                DHexEdgeVertices e1 = e2;
                Color c1 = c2;

                //Определяем конечные данные квада
                e2 = DHexEdgeVertices.TerraceLerp(
                    begin, end,
                    a);
                c2 = SpaceGenerationData.TerraceLerp(
                    SpaceGenerationData.cellColor1, SpaceGenerationData.cellColor2,
                    a);

                //Заносим промежуточную полосу в меш
                TriangulateEdgeStrip(
                    ref chunk,
                    e1, c1, t1,
                    e2, c2, t2,
                    hasRoad);
            }

            //Заносим последнюю полосу в меш
            TriangulateEdgeStrip(
                ref chunk,
                e2, c2, t1,
                end, SpaceGenerationData.cellColor2, t2,
                hasRoad);
        }

        void TriangulateCorner(
            ref CHexChunk chunk,
            ref CHexRegion bottomCell, Vector3 bottom,
            ref CHexRegion leftCell, Vector3 left,
            ref CHexRegion rightCell, Vector3 right)
        {
            //Определяем типы рёбер
            HexEdgeType leftEdgeType
                = SpaceGenerationData.GetEdgeType(
                    bottomCell.Elevation, leftCell.Elevation);
            HexEdgeType rightEdgeType
                = SpaceGenerationData.GetEdgeType(
                    bottomCell.Elevation, rightCell.Elevation);

            //Если тип левого ребра - склон
            if (leftEdgeType == HexEdgeType.Slope)
            {
                //Если тип правого ребра - склон
                if (rightEdgeType == HexEdgeType.Slope)
                {
                    //Триангулируем угловые террасы
                    TriangulateCornerTerraces(
                        ref chunk,
                        ref bottomCell, bottom,
                        ref leftCell, left,
                        ref rightCell, right);
                }
                //Иначе, если тип правого ребра - плоскость
                else if (rightEdgeType == HexEdgeType.Flat)
                {
                    //Триангулируем угловые террасы
                    TriangulateCornerTerraces(
                        ref chunk,
                        ref leftCell, left,
                        ref rightCell, right,
                        ref bottomCell, bottom);
                }
                //Иначе
                else
                {
                    //Триангулируем стык террас и обрыва
                    TriangulateCornerTerracesCliff(
                        ref chunk,
                        ref bottomCell, bottom,
                        ref leftCell, left,
                        ref rightCell, right);
                }
            }
            //Иначе, если тип правого ребра - склон
            else if (rightEdgeType == HexEdgeType.Slope)
            {
                //Если тип левого ребра - плоскость
                if (leftEdgeType == HexEdgeType.Flat)
                {
                    //Триангулируем угловые террасы
                    TriangulateCornerTerraces(
                        ref chunk,
                        ref rightCell, right,
                        ref bottomCell, bottom,
                        ref leftCell, left);
                }
                //Иначе
                else
                {
                    //Триангулируем угловые террасы
                    TriangulateCornerCliffTerraces(
                        ref chunk,
                        ref bottomCell, bottom,
                        ref leftCell, left,
                        ref rightCell, right);
                }
            }
            //Иначе, если тип ребра между левым и правым соседом - склон
            else if (SpaceGenerationData.GetEdgeType(leftCell.Elevation, rightCell.Elevation) == HexEdgeType.Slope)
            {
                //Если высота левого соседа меньше высоты правого
                if (leftCell.Elevation < rightCell.Elevation)
                {
                    //Триангулируем угловые террасы
                    TriangulateCornerCliffTerraces(
                        ref chunk,
                        ref rightCell, right,
                        ref bottomCell, bottom,
                        ref leftCell, left);
                }
                //Иначе
                else
                {
                    //Триангулируем угловые террасы
                    TriangulateCornerTerracesCliff(
                        ref chunk,
                        ref leftCell, left,
                        ref rightCell, right,
                        ref bottomCell, bottom);
                }
            }
            //Иначе
            else
            {
                //Заносим треугольник в меш
                chunk.terrain.AddTriangle(
                    bottom,left,right);
                chunk.terrain.AddTriangleColor(
                    SpaceGenerationData.cellColor1, SpaceGenerationData.cellColor2, SpaceGenerationData.cellColor3);

                Vector3 types;
                types.x = bottomCell.TerrainTypeIndex;
                types.y = leftCell.TerrainTypeIndex;
                types.z = rightCell.TerrainTypeIndex;
                chunk.terrain.AddTriangleTerrainTypes(types);
            }

            //Триангулируем стену
            chunk.features.AddWall(
                ref bottomCell, bottom,
                ref leftCell, left,
                ref rightCell, right);
        }

        void TriangulateCornerTerraces(
            ref CHexChunk chunk,
            ref CHexRegion beginCell, Vector3 begin,
            ref CHexRegion leftCell, Vector3 left,
            ref CHexRegion rightCell, Vector3 right)
        {
            //Определяем вершины треугольника
            Vector3 v3 
                = SpaceGenerationData.TerraceLerp(
                    begin, left, 
                    1);
            Vector3 v4 
                = SpaceGenerationData.TerraceLerp(
                    begin, right, 
                    1);
            Color c3 
                = SpaceGenerationData.TerraceLerp(
                    SpaceGenerationData.cellColor1, SpaceGenerationData.cellColor2, 
                    1);
            Color c4 
                = SpaceGenerationData.TerraceLerp(
                    SpaceGenerationData.cellColor1, SpaceGenerationData.cellColor3, 
                    1);
            Vector3 types;
            types.x = beginCell.TerrainTypeIndex;
            types.y = leftCell.TerrainTypeIndex;
            types.z = rightCell.TerrainTypeIndex;

            //Заносим первый треугольник в меш
            chunk.terrain.AddTriangle(
                begin, v3, v4);
            chunk.terrain.AddTriangleColor(
                SpaceGenerationData.cellColor1, c3, c4);
            chunk.terrain.AddTriangleTerrainTypes(types);

            //Для каждого промежуточного квада
            for (int a = 2; a < SpaceGenerationData.terraceSteps; a++)
            {
                //Определяем начальные данные квада
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;

                //Определяем конечные данные квада
                v3 = SpaceGenerationData.TerraceLerp(
                    begin, left, 
                    a);
                v4 = SpaceGenerationData.TerraceLerp(
                    begin, right, 
                    a);
                c3 = SpaceGenerationData.TerraceLerp(
                    SpaceGenerationData.cellColor1, SpaceGenerationData.cellColor2, 
                    a);
                c4 = SpaceGenerationData.TerraceLerp(
                    SpaceGenerationData.cellColor1, SpaceGenerationData.cellColor3, 
                    a);

                //Заносим промежуточный квад в меш
                chunk.terrain.AddQuad(
                    v1, v2, v3, v4);
                chunk.terrain.AddQuadColor(
                    c1, c2, c3, c4);
                chunk.terrain.AddQuadTerrainTypes(types);
            }

            //Заносим последний квад в меш
            chunk.terrain.AddQuad(
                v3, v4, left, right);
            chunk.terrain.AddQuadColor(
                c3, c4, SpaceGenerationData.cellColor2, SpaceGenerationData.cellColor3);
            chunk.terrain.AddQuadTerrainTypes(types);
        }

        void TriangulateCornerTerracesCliff(
            ref CHexChunk chunk,
            ref CHexRegion beginCell, Vector3 begin,
            ref CHexRegion leftCell, Vector3 left,
            ref CHexRegion rightCell, Vector3 right)
        {
            //Определяем положение вершины схождения террас
            float b = 1f 
                / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            Vector3 boundary
                = Vector3.Lerp(
                    SpaceGenerationData.Perturb(begin), SpaceGenerationData.Perturb(right),
                    b);
            Color boundaryColor
                = Color.Lerp(
                    SpaceGenerationData.cellColor1, SpaceGenerationData.cellColor3,
                    b);
            Vector3 types;
            types.x = beginCell.TerrainTypeIndex;
            types.y = leftCell.TerrainTypeIndex;
            types.z = rightCell.TerrainTypeIndex;

            //Триангулируем пограничный треугольник
            TriangulateBoundaryTriangle(
                ref chunk,
                SpaceGenerationData.cellColor1, begin,
                SpaceGenerationData.cellColor2, left,
                boundaryColor, boundary,
                types);
            
            //Если граница между левым и правым соседом - склон
            if (SpaceGenerationData.GetEdgeType(leftCell.Elevation, rightCell.Elevation) == HexEdgeType.Slope)
            {
                //Триангулируем пограничный треугольник
                TriangulateBoundaryTriangle(
                    ref chunk,
                    SpaceGenerationData.cellColor2, left,
                    SpaceGenerationData.cellColor3, right,
                    boundaryColor, boundary,
                    types);
            }
            //Иначе
            else
            {
                //Заносим треугольник в меш
                chunk.terrain.AddTriangleUnperturbed(
                    SpaceGenerationData.Perturb(left), SpaceGenerationData.Perturb(right), boundary);
                chunk.terrain.AddTriangleColor(
                    SpaceGenerationData.cellColor2, SpaceGenerationData.cellColor3, boundaryColor);
                chunk.terrain.AddTriangleTerrainTypes(types);
            }
        }

        void TriangulateCornerCliffTerraces(
            ref CHexChunk chunk,
            ref CHexRegion beginCell, Vector3 begin,
            ref CHexRegion leftCell, Vector3 left,
            ref CHexRegion rightCell, Vector3 right)
        {
            //Определяем положение вершины схождения террас
            float b = 1f
                / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            Vector3 boundary
                = Vector3.Lerp(
                    SpaceGenerationData.Perturb(begin), SpaceGenerationData.Perturb(left),
                    b);
            Color boundaryColor
                = Color.Lerp(
                    SpaceGenerationData.cellColor1, SpaceGenerationData.cellColor2,
                    b);
            Vector3 types;
            types.x = beginCell.TerrainTypeIndex;
            types.y = leftCell.TerrainTypeIndex;
            types.z = rightCell.TerrainTypeIndex;

            //Триангулируем пограничный треугольник
            TriangulateBoundaryTriangle(
                ref chunk,
                SpaceGenerationData.cellColor3, right,
                SpaceGenerationData.cellColor1, begin,
                boundaryColor, boundary,
                types);

            //Если граница между левым и правым соседом - склон
            if (SpaceGenerationData.GetEdgeType(leftCell.Elevation, rightCell.Elevation) == HexEdgeType.Slope)
            {
                //Триангулируем пограничный треугольник
                TriangulateBoundaryTriangle(
                    ref chunk,
                    SpaceGenerationData.cellColor2, left,
                    SpaceGenerationData.cellColor3, right,
                    boundaryColor, boundary,
                    types);
            }
            //Иначе
            else
            {
                //Заносим треугольник в меш
                chunk.terrain.AddTriangleUnperturbed(
                    SpaceGenerationData.Perturb(left), SpaceGenerationData.Perturb(right), boundary);
                chunk.terrain.AddTriangleColor(
                    SpaceGenerationData.cellColor2, SpaceGenerationData.cellColor3, boundaryColor);
                chunk.terrain.AddTriangleTerrainTypes(types);
            }
        }

        void TriangulateBoundaryTriangle(
            ref CHexChunk chunk,
            Color beginColor, Vector3 begin,
            Color leftColor, Vector3 left,
            Color boundaryColor, Vector3 boundary,
            Vector3 types)
        {
            //Определяем начальную вершину схождения
            Vector3 v2 = SpaceGenerationData.Perturb(
                SpaceGenerationData.TerraceLerp(
                    begin, left,
                    1));
            Color c2 = SpaceGenerationData.TerraceLerp(
                beginColor, leftColor,
                1);

            //Заносим первый треугольник в меш
            chunk.terrain.AddTriangleUnperturbed(
                SpaceGenerationData.Perturb(begin), v2, boundary);
            chunk.terrain.AddTriangleColor(
                beginColor, c2, boundaryColor);
            chunk.terrain.AddTriangleTerrainTypes(types);

            //Для каждого промежуточного треугольника
            for (int a = 2; a < SpaceGenerationData.terraceSteps; a++)
            {
                //Определяем начальные данные треугольника
                Vector3 v1 = v2;
                Color c1 = c2;

                //Определяем конечные данные треугольника
                v2 = SpaceGenerationData.Perturb(
                    SpaceGenerationData.TerraceLerp(
                        begin, left,
                        a));
                c2 = SpaceGenerationData.TerraceLerp(
                    beginColor, leftColor,
                    a);

                //Заносим промежуточный треугольник в меш
                chunk.terrain.AddTriangleUnperturbed(
                    v1, v2, boundary);
                chunk.terrain.AddTriangleColor(
                    c1, c2, boundaryColor);
                chunk.terrain.AddTriangleTerrainTypes(types);
            }

            //Заносим последний треугольник в меш
            chunk.terrain.AddTriangleUnperturbed(
                v2, SpaceGenerationData.Perturb(left), boundary);
            chunk.terrain.AddTriangleColor(
                c2, leftColor, boundaryColor);
            chunk.terrain.AddTriangleTerrainTypes(types);
        }

        void TriangulateEdgeFan(
            ref CHexChunk chunk,
            Vector3 center,
            DHexEdgeVertices edge,
            float type)
        {
            //Заносим треугольники в меш
            chunk.terrain.AddTriangle(
                center, edge.v1, edge.v2);
            chunk.terrain.AddTriangle(
                center, edge.v2, edge.v3);
            chunk.terrain.AddTriangle(
                center, edge.v3, edge.v4);
            chunk.terrain.AddTriangle(
                center, edge.v4, edge.v5);

            chunk.terrain.AddTriangleColor(SpaceGenerationData.cellColor1); 
            chunk.terrain.AddTriangleColor(SpaceGenerationData.cellColor1); 
            chunk.terrain.AddTriangleColor(SpaceGenerationData.cellColor1); 
            chunk.terrain.AddTriangleColor(SpaceGenerationData.cellColor1);

            Vector3 types;
            types.x = types.y = types.z = type;
            chunk.terrain.AddTriangleTerrainTypes(types);
            chunk.terrain.AddTriangleTerrainTypes(types);
            chunk.terrain.AddTriangleTerrainTypes(types);
            chunk.terrain.AddTriangleTerrainTypes(types);
        }

        void TriangulateEdgeStrip(
            ref CHexChunk chunk,
            DHexEdgeVertices e1, Color c1, float type1,
            DHexEdgeVertices e2, Color c2, float type2,
            bool hasRoad = false)
        {
            //Заносим квады в меш
            chunk.terrain.AddQuad(
                e1.v1, e1.v2, e2.v1, e2.v2);
            chunk.terrain.AddQuad(
                e1.v2, e1.v3, e2.v2, e2.v3);
            chunk.terrain.AddQuad(
                e1.v3, e1.v4, e2.v3, e2.v4);
            chunk.terrain.AddQuad(
                e1.v4, e1.v5, e2.v4, e2.v5);

            chunk.terrain.AddQuadColor(
                c1, c2);
            chunk.terrain.AddQuadColor(
                c1, c2);
            chunk.terrain.AddQuadColor(
                c1, c2);
            chunk.terrain.AddQuadColor(
                c1, c2);

            Vector3 types;
            types.x = types.z = type1;
            types.y = type2;
            chunk.terrain.AddQuadTerrainTypes(types);
            chunk.terrain.AddQuadTerrainTypes(types);
            chunk.terrain.AddQuadTerrainTypes(types);
            chunk.terrain.AddQuadTerrainTypes(types);

            //Если в данном направлении присутствует дорога
            if (hasRoad == true)
            {
                //Триангулируем сегмент дороги
                TriangulateRoadSegment(
                    ref chunk,
                    e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4);
            }
        }

        void TriangulateRiverQuad(
            ref CHexChunk chunk,
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y,
            float v,
            bool reversed)
        {
            //Триангулируем течение реки
            TriangulateRiverQuad(
                ref chunk,
                v1, v2, v3, v4,
                y, y,
                v,
                reversed);
        }

        void TriangulateRiverQuad(
            ref CHexChunk chunk,
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y1, float y2,
            float v,
            bool reversed)
        {
            //Опускаем вершины
            v1.y = v2.y = y1; 
            v3.y = v4.y = y2;

            //Заносим квад в меш
            chunk.rivers.AddQuad(
                v1, v2, v3, v4);
            
            //Если река развёрнута
            if (reversed == true)
            {
                //Заносим UV-координаты в меш
                chunk.rivers.AddQuadUV(
                    1f, 0f, 0.8f - v, 0.6f - v);
            }
            //Иначе
            else
            {
                //Заносим UV-координаты в меш
                chunk.rivers.AddQuadUV(
                    0f, 1f, v, v + 0.2f);
            }
        }

        void TriangulateRoad(
            ref CHexChunk chunk,
            Vector3 center, Vector3 mL, Vector3 mR,
            DHexEdgeVertices e,
            bool hasRoadThroughEdge)
        {
            //Если ячейка имеет дорогу через ребро
            if(hasRoadThroughEdge == true)
            {
                //Определяем центральную вершину
                Vector3 mC
                    = Vector3.Lerp(
                        mL, mR,
                        0.5f);

                //Триангулируем сегмент дороги
                TriangulateRoadSegment(
                    ref chunk,
                    mL, mC, mR, e.v2, e.v3, e.v4);

                //Заносим треугольники в меш
                chunk.roads.AddTriangle(
                    center, mL, mC);
                chunk.roads.AddTriangle(
                    center, mC, mR);
                chunk.roads.AddTriangleUV(
                    new(1f, 0f), new(0f, 0f), new(1f, 0f));
                chunk.roads.AddTriangleUV(
                    new(1f, 0f), new(1f, 0f), new(0f, 0f));
            }
            //Иначе
            else
            {
                //Триангулируем ребро дороги
                TriangulateRoadEdge(
                    ref chunk,
                    center, mL, mR);

            }
        }

        void TriangulateRoadAdjacentToRiver(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //Определяем, имеет ли ячейка дорогу через данное ребро
            bool hasRoadThroughEdge
                = cell.HasRoadThroughEdge(direction);
            //Определяем, имеет ли ячейка реки через предыдущее и следующее рёбра
            bool previousHasRiver
                = cell.HasRiverThroughEdge(direction.Previous());
            bool nextHasRiver
                = cell.HasRiverThroughEdge(direction.Next());

            //Определяем смешение
            Vector2 interpolators
                = GetRoadInterpolators(
                    ref cell,
                    direction);

            //Определяем вершины дороги
            Vector3 roadCenter = center;
            //Если ячейка имеет начало или конец реки
            if (cell.HasRiverBeginOrEnd == true)
            {
                //Смещаем центр дороги к твёрдому ребру ячейки
                roadCenter
                    += SpaceGenerationData.GetSolidEdgeMiddle(
                        cell.RiverBeginOrEndDirection.Opposite()) * (1f / 3f);
            }
            //Иначе, если река идёт в противоположных направлениях
            else if (cell.IncomingRiver == cell.OutgoingRiver.Opposite())
            {
                //Определяем угол
                Vector3 corner;

                //Если ячейка имеет реку через предыдущее ребро
                if (previousHasRiver == true)
                {
                    //Если ячейка не имеет дорогу через текущее ребро 
                    if(hasRoadThroughEdge == false
                        //И не имеет дорогу через следующее ребро
                        && cell.HasRoadThroughEdge(direction.Next()) == false)
                    {
                        //Выходим из функции
                        return;
                    }

                    corner = SpaceGenerationData.GetSecondSolidCorner(direction);
                }
                //Иначе
                else
                {
                    //Если ячейка не имеет дорогу через текущее ребро 
                    if (hasRoadThroughEdge == false
                        //И не имеет дорогу через предыдущее ребро
                        && cell.HasRoadThroughEdge(direction.Previous()) == false)
                    {
                        //Выходим из функции
                        return;
                    }

                    corner = SpaceGenerationData.GetFirstSolidCorner(direction);
                }

                //Смещаем центр дороги
                roadCenter
                    += corner * 0.5f;

                //Если исходящая река идёт через следующее направление
                if (cell.IncomingRiver == direction.Next()
                    //И дорога есть по обе стороны от реки
                    && (cell.HasRoadThroughEdge(direction.Next2())
                    || cell.HasRoadThroughEdge(direction.Opposite())))
                {
                    //Создаём мост
                    chunk.features.AddBridge(
                        roadCenter, center - corner * 0.5f);
                }

                center
                    += corner * 0.25f;
            }
            //Иначе, если ячейка имеет входящую реку через ребро, предыдущее исходящей
            else if(cell.IncomingRiver == cell.OutgoingRiver.Previous() == true)
            {
                //Смещаем центр дороги
                roadCenter
                    -= SpaceGenerationData.GetSecondCorner(cell.IncomingRiver) * 0.2f;
            }
            //Иначе, если ячейка имеет входящую реку через ребро, следующее исходящей
            else if(cell.IncomingRiver == cell.OutgoingRiver.Next() == true)
            {
                //Смещаем центр дороги
                roadCenter
                    -= SpaceGenerationData.GetFirstCorner(cell.IncomingRiver) * 0.2f;
            }
            //Иначе, если ячейка имеет реки через оба соседних ребра
            else if(previousHasRiver && nextHasRiver == true)
            {
                //Если ячейка не имеет дороги в данном направлении
                if (hasRoadThroughEdge == false)
                {
                    //Выходим из функции
                    return;
                }

                //Смещаем центр дороги
                Vector3 offset
                    = SpaceGenerationData.GetSolidEdgeMiddle(direction)
                    * SpaceGenerationData.innerToOuter;
                roadCenter
                    += offset * 0.7f;
                center
                    += offset * 0.5f;
            }
            //Иначе
            else
            {
                //Определяем центральное направление
                HexDirection middle;
                //Если ячейка имеет реку через предыдущее направление
                if (previousHasRiver == true)
                {
                    middle = direction.Next();
                }
                //Иначе, если ячейка имеет реку через следующее направление
                else if (nextHasRiver == true)
                {
                    middle = direction.Previous();
                }
                //Иначе
                else
                {
                    middle = direction;
                }

                //Если ячейка не имеет дороги ни через одно направление из соседних
                if (cell.HasRoadThroughEdge(middle) == false
                    && cell.HasRoadThroughEdge(middle.Previous()) == false
                    && cell.HasRoadThroughEdge(middle.Next()) == false)
                {
                    //Выходим из функции
                    return;
                }

                //Определяем смещение
                Vector3 offset = SpaceGenerationData.GetSolidEdgeMiddle(middle);
                //Смещаем центр дороги
                roadCenter += offset * 0.25f;

                //Если направление - центральное
                if (direction == middle
                    //И дорога есть по обе стороны от реки
                    && cell.HasRoadThroughEdge(direction.Opposite()) == true)
                {
                    //Создаём мост
                    chunk.features.AddBridge(
                        roadCenter, center - offset * (SpaceGenerationData.innerToOuter * 0.7f));
                }
            }

            Vector3 mL = Vector3.Lerp(
                roadCenter, e.v1,
                interpolators.x);
            Vector3 mR = Vector3.Lerp(
                roadCenter, e.v5,
                interpolators.y);

            //Триангулируем дорогу
            TriangulateRoad(
                ref chunk,
                roadCenter, mL, mR,
                e, 
                hasRoadThroughEdge);

            //Если ячейка имеет реку через предыдущее ребро
            if (previousHasRiver == true)
            {
                //Триангулируем ребро дороги
                TriangulateRoadEdge(
                    ref chunk,
                    roadCenter, center, mL);
            }
            //Если ячейка имеет реку через следующее ребро
            if (nextHasRiver == true)
            {
                //Триангулируем ребро дороги
                TriangulateRoadEdge(
                    ref chunk,
                    roadCenter, mR, center);
            }
        }

        void TriangulateRoadSegment(
            ref CHexChunk chunk,
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6)
        {
            //Заносим квады в меш
            chunk.roads.AddQuad(
                v1, v2, v4, v5);
            chunk.roads.AddQuad(
                v2, v3, v5, v6);
            chunk.roads.AddQuadUV(
                0f, 1f, 0f, 0f);
            chunk.roads.AddQuadUV(
                1f, 0f, 0f, 0f);
        }

        void TriangulateRoadEdge(
            ref CHexChunk chunk,
            Vector3 center, Vector3 mL, Vector3 mR)
        {
            //Заносим треугольник в меш
            chunk.roads.AddTriangle(
                center, mL, mR);
            chunk.roads.AddTriangleUV(
                new(1f, 0f), new(0f, 0f), new(0f, 0f));
        }

        void TriangulateWater(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center)
        {
            //Определяем положение центра
            center.y = cell.WaterSurfaceY;

            //Если у ячейки есть сосед с данного направления
            if (cell.GetNeighbour(direction).Unpack(world.Value, out int neighbourCellEntity))
            {
                //Берём компонент соседа
                ref CHexRegion neighbourCell
                    = ref regionPool.Value.Get(neighbourCellEntity);

                //Если сосед находится под водой
                if (neighbourCell.IsUnderwater == true)
                {
                    //Триангулируем водоём
                    TriangulateOpenWater(
                        ref chunk,
                        ref cell, ref neighbourCell,
                        direction,
                        center);
                }
                //Иначе
                else
                {
                    //Триангулируем побережье
                    TriangulateShore(
                        ref chunk,
                        ref cell, ref neighbourCell,
                        direction,
                        center);
                }
            }
        }

        void TriangulateOpenWater(
            ref CHexChunk chunk,
            ref CHexRegion cell, ref CHexRegion neighbourCell,
            HexDirection direction,
            Vector3 center)
        {
            //Определяем положение углов
            Vector3 c1
                = center + SpaceGenerationData.GetFirstWaterCorner(direction);
            Vector3 c2
                = center + SpaceGenerationData.GetSecondWaterCorner(direction);

            //Заносим треугольник в меш
            chunk.water.AddTriangle(
                center, c1, c2);

            //Если направление меньше или равно юго-востоку
            if (direction <= HexDirection.SE)
            {
                //Определяем вершины соединения
                Vector3 bridge = SpaceGenerationData.GetWaterBridge(direction);
                Vector3 e1 
                    = c1 + bridge;
                Vector3 e2 
                    = c2 + bridge;

                //Заносим квад в меш
                chunk.water.AddQuad(
                    c1, c2, e1, e2);

                //Если направление меньше или равно востоку
                if (direction <= HexDirection.E)
                {
                    //Если у ячейки есть следующий сосед
                    if (cell.GetNeighbour(direction.Next()).Unpack(world.Value, out int nextNeighbourCellEntity))
                    {
                        //Берём компонент следующего соседа
                        ref CHexRegion nextNeighbourCell
                            = ref regionPool.Value.Get(nextNeighbourCellEntity);

                        //Если сосед не находится под водой
                        if (nextNeighbourCell.IsUnderwater == false)
                        {
                            //Выходим из функции
                            return;
                        }

                        //Заносим треугольник в меш
                        chunk.water.AddTriangle(
                            c2, e2, c2 + SpaceGenerationData.GetWaterBridge(direction.Next()));
                    }
                }
            }
        }

        void TriangulateShore(
            ref CHexChunk chunk,
            ref CHexRegion cell, ref CHexRegion neighbourCell,
            HexDirection direction,
            Vector3 center)
        {
            //Определяем вершины ребра
            DHexEdgeVertices e1
                = new(
                    center + SpaceGenerationData.GetFirstWaterCorner(direction),
                    center + SpaceGenerationData.GetSecondWaterCorner(direction));

            //Заносим треугольники в меш
            chunk.water.AddTriangle(
                center, e1.v1, e1.v2);
            chunk.water.AddTriangle(
                center, e1.v2, e1.v3);
            chunk.water.AddTriangle(
                center, e1.v3, e1.v4);
            chunk.water.AddTriangle(
                center, e1.v4, e1.v5);

            //Определяем вершины соединения
            Vector3 center2 = neighbourCell.Position;
            center2.y = center.y;
            DHexEdgeVertices e2 = new(
                center2 + SpaceGenerationData.GetSecondSolidCorner(direction.Opposite()),
                center2 + SpaceGenerationData.GetFirstSolidCorner(direction.Opposite()));

            //Если ячейка имеет реку через данное ребро
            if (cell.HasRiverThroughEdge(direction) == true)
            {
                //Триангулируем устье
                TriangulateEstuary(
                    ref chunk,
                    e1, e2,
                    cell.IncomingRiver == direction);
            }
            else
            {
                //Заносим квады в меш
                chunk.waterShore.AddQuad(
                    e1.v1, e1.v2, e2.v1, e2.v2);
                chunk.waterShore.AddQuad(
                    e1.v2, e1.v3, e2.v2, e2.v3);
                chunk.waterShore.AddQuad(
                    e1.v3, e1.v4, e2.v3, e2.v4);
                chunk.waterShore.AddQuad(
                    e1.v4, e1.v5, e2.v4, e2.v5);
                chunk.waterShore.AddQuadUV(
                    0f, 0f, 0f, 1f);
                chunk.waterShore.AddQuadUV(
                    0f, 0f, 0f, 1f);
                chunk.waterShore.AddQuadUV(
                    0f, 0f, 0f, 1f);
                chunk.waterShore.AddQuadUV(
                    0f, 0f, 0f, 1f);
            }

            //Если у ячейки есть следующий сосед
            if (cell.GetNeighbour(direction.Next()).Unpack(world.Value, out int nextNeighbourCellEntity))
            {
                //Берём компонент следующего соседа
                ref CHexRegion nextNeighbourCell
                    = ref regionPool.Value.Get(nextNeighbourCellEntity);

                //Определяем вершину треугольника
                Vector3 v3 
                    = nextNeighbourCell.Position 
                    + (nextNeighbourCell.IsUnderwater 
                    ? SpaceGenerationData.GetFirstWaterCorner(direction.Previous()) 
                    : SpaceGenerationData.GetFirstSolidCorner(direction.Previous()));
                v3.y = center.y;

                //Заносим треугольник в меш
                chunk.waterShore.AddTriangle(
                    e1.v5, e2.v5, v3);
                chunk.waterShore.AddTriangleUV(
                    new(0f, 0f), new(0f, 1f), new(0f, nextNeighbourCell.IsUnderwater ? 0f : 1f));
            }
        }

        void TriangulateEstuary(
            ref CHexChunk chunk,
            DHexEdgeVertices e1, DHexEdgeVertices e2,
            bool incomingRiver)
        {
            //Заносим треугольники в меш
            chunk.waterShore.AddTriangle(
                e2.v1, e1.v2, e1.v1);
            chunk.waterShore.AddTriangle(
                e2.v5, e1.v5, e1.v4);
            chunk.waterShore.AddTriangleUV(
                new(0f, 1f), new(0f, 0f), new(0f, 0f));
            chunk.waterShore.AddTriangleUV(
                new(0f, 1f), new(0f, 0f), new(0f, 0f));

            //Заносим объекты в меш
            chunk.estuaries.AddQuad(
                e2.v1, e1.v2, e2.v2, e1.v3);
            chunk.estuaries.AddTriangle(
                e1.v3, e2.v2, e2.v4);
            chunk.estuaries.AddQuad(
                e1.v3, e1.v4, e2.v4, e2.v5);

            chunk.estuaries.AddQuadUV(
                new Vector2(0f, 1f), new (0f, 0f), new (1f, 1f), new (0f, 0f));
            chunk.estuaries.AddTriangleUV(
                new (0f, 0f), new (1f, 1f), new (1f, 1f));
            chunk.estuaries.AddQuadUV(
                new Vector2(0f, 0f), new (0f, 0f), new (1f, 1f), new (0f, 1f));

            //Если река входящая
            if (incomingRiver)
            {
                chunk.estuaries.AddQuadUV2(
                    new Vector2(1.5f, 1f), new (0.7f, 1.15f), new (1f, 0.8f), new (0.5f, 1.1f));
                chunk.estuaries.AddTriangleUV2(
                    new (0.5f, 1.1f), new (1f, 0.8f), new (0f, 0.8f));
                chunk.estuaries.AddQuadUV2(
                    new Vector2(0.5f, 1.1f), new (0.3f, 1.15f), new (0f, 0.8f), new (-0.5f, 1f));
            }
            //Иначе
            else
            {
                chunk.estuaries.AddQuadUV2(
                    new Vector2(-0.5f, -0.2f), new (0.3f, -0.35f), new (0f, 0f), new (0.5f, -0.3f));
                chunk.estuaries.AddTriangleUV2(
                    new (0.5f, -0.3f), new (0f, 0f), new (1f, 0f));
                chunk.estuaries.AddQuadUV2(
                    new Vector2(0.5f, -0.3f), new (0.7f, -0.35f), new (1f, 0f), new (1.5f, -0.2f));
            }
        }

        Vector2 GetRoadInterpolators(
            ref CHexRegion cell,
            HexDirection direction)
        {
            //Определяем смешение
            Vector2 interpolators;

            //Если ячейка имеет дорогу через данное ребро
            if (cell.HasRoadThroughEdge(direction))
            {
                //Определяем положение вершины
                interpolators.x = interpolators.y = 0.5f;
            }
            //Иначе
            else
            {
                //Определяем положение вершины
                interpolators.x =
                    cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
                interpolators.y =
                    cell.HasRoadThroughEdge(direction.Next()) ? 0.5f : 0.25f;
            }
            return interpolators;
        }

        EcsPackedEntity GetRegionPE(
            DHexCoordinates coordinates)
        {
            int z = coordinates.Z;

            //Если координата выходит за границы карты
            if (z < 0 || z >= spaceGenerationData.Value.regionCountZ)
            {
                return new();
            }

            int x = coordinates.X + z / 2;

            //Если координата выходит за границы ячейки
            if (x < 0 || x >= spaceGenerationData.Value.regionCountX)
            {
                return new();
            }

            return spaceGenerationData.Value.regionPEs[x + z * spaceGenerationData.Value.regionCountX];
        }


        void ChunkRefreshSelfRequest(
            ref CHexRegion region)
        {
            //Берём компонент родительского чанка региона
            region.parentChunkPE.Unpack(world.Value, out int parentChunkEntity);
            ref CHexChunk parentChunk = ref chunkPool.Value.Get(parentChunkEntity);

            //Если ещё не существует самозапроса обновления чанка
            if (mapChunkRefreshSelfRequestPool.Value.Has(parentChunkEntity) == false)
            {
                //Назначаем сущности самозапрос обновления чанка
                mapChunkRefreshSelfRequestPool.Value.Add(parentChunkEntity);
            }

            //Для каждого соседа региона
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //Если сосед существует 
                if (region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //Берём компонент соседа
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //Берём сущность родительского чанка региона
                    neighbourRegion.parentChunkPE.Unpack(world.Value, out int neighbourParentChunkEntity);

                    //Если ещё не существует самозапроса обновления чанка
                    if (mapChunkRefreshSelfRequestPool.Value.Has(neighbourParentChunkEntity) == false)
                    {
                        //Назначаем сущности самозапрос обновления чанка
                        mapChunkRefreshSelfRequestPool.Value.Add(neighbourParentChunkEntity);
                    }
                }
            }
        }
    }
}