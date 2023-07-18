
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
        //����
        readonly EcsWorldInject world = default;

        //�����
        //readonly EcsFilterInject<Inc<CHexChunk>> chunkFilter = default;
        readonly EcsPoolInject<CHexChunk> chunkPool = default;

        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //�����������
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //���������������-������������� �������
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        //������� �����
        readonly EcsFilterInject<Inc<RChangeMapMode>> changeMapModeRequestFilter = default;
        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        readonly EcsFilterInject<Inc<SRMapChunkRefresh, CHexChunk>> refreshChunkSelfRequestFilter = default;
        readonly EcsPoolInject<SRMapChunkRefresh> mapChunkRefreshSelfRequestPool = default;

        //������
        readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;
        readonly EcsCustomInject<UI.InputData> inputData;

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
                    MapModeDistanceCalculate();

                    //���������, ��� �������� ����� ����� - ����� ����������
                    inputData.Value.mapMode = UI.MapMode.Distance;
                }

                //������� �������� �������
                world.Value.DelEntity(changeMapModeREntity);
            }

            //��� ������� �����, ������� ��������� ��������
            foreach (int refreshChunkEntity in refreshChunkSelfRequestFilter.Value)
            {
                //���� ��������� �����
                ref CHexChunk chunk = ref chunkPool.Value.Get(refreshChunkEntity);

                //������������� ��� �����
                ChunkTriangulate(ref chunk);
                //ChunkSimpleTriangulate(ref chunk);

                Debug.LogWarning("Chunk refreshed!");

                //������� � �������� ����� ���������� ����������
                mapChunkRefreshSelfRequestPool.Value.Del(refreshChunkEntity);
            }

            //���� ������ ������� ����� ���� ��������
            if (mapGenerationData.Value.regionShaderData.isChanged == true)
            {
                //��������� ������ ������� �����
                mapGenerationData.Value.regionShaderData.Refresh();

                //���������, ��� ������ ������� ����� ���������
                mapGenerationData.Value.regionShaderData.isChanged = false;
            }
        }

        void MapModeDistanceCalculate()
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

                    //����������� ������������ �����
                    ChunkRefreshSelfRequest(ref region);
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
                            if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
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
                            if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
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
                            if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
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
                            if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int cellEntity))
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
        }

        void ChunkTriangulate(
            ref CHexChunk chunk)
        {
            //������� ����
            chunk.terrain.Clear();
            chunk.rivers.Clear();
            chunk.roads.Clear();
            chunk.water.Clear();
            chunk.waterShore.Clear();
            chunk.estuaries.Clear();

            chunk.features.Clear();

            //��� ������ ������ �����
            for (int a = 0; a < chunk.regionPEs.Length; a++)
            {
                //���� ��������� ������
                chunk.regionPEs[a].Unpack(world.Value, out int cellEntity);
                ref CHexRegion cell
                    = ref regionPool.Value.Get(cellEntity);

                //������������� ������
                CellTriangulate(
                    ref chunk,
                    ref cell);
            }

            //��������� ����
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
            //������� ����
            chunk.terrain.Clear();
            chunk.rivers.Clear();
            chunk.roads.Clear();
            chunk.water.Clear();
            chunk.waterShore.Clear();
            chunk.estuaries.Clear();

            chunk.features.Clear();

            //��� ������ ������ �����
            for (int a = 0; a < chunk.regionPEs.Length; a++)
            {
                //���� ��������� ������
                chunk.regionPEs[a].Unpack(world.Value, out int cellEntity);
                ref CHexRegion cell = ref regionPool.Value.Get(cellEntity);

                //������������� ������
                CellSimpleTriangulate(
                    ref chunk,
                    ref cell);
            }

            //��������� ����
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
            //��������� ��������� ����� ������
            Vector3 uiPosition
                = cell.uiRect.rectTransform.localPosition;
            uiPosition.z
                = cell.Elevation * -MapGenerationData.elevationStep;
            cell.uiRect.rectTransform.localPosition
                = uiPosition;

            //��� ������� ������������ ������
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //������������� �����������
                TriangulateEdge(
                    ref chunk,
                    ref cell,
                    d);
            }

            //���� ������ �� ��������� ��� �����
            if (cell.IsUnderwater == false)
            {
                //���� ������ ��������
                if (cell.HasRiver == false
                    && cell.HasRoads == false)
                {
                    //��������� ������
                    chunk.features.AddFeature(
                        ref cell,
                        cell.Position);
                }
                //���� ������ ����� ������ ������
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
            //��������� ��������� ����� ������
            Vector3 uiPosition = cell.uiRect.rectTransform.localPosition;
            uiPosition.z = cell.Elevation * -MapGenerationData.elevationStep;
            cell.uiRect.rectTransform.localPosition = uiPosition;

            //��� ������� ������������ ������
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                //������������� �����������
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
            //���� �������� ������� ������������
            Vector3 center = cell.Position;
            DHexEdgeVertices e
                = new(
                    center + MapGenerationData.GetFirstSolidCorner(
                        direction),
                    center + MapGenerationData.GetSecondSolidCorner(
                        direction));

            //���� ������ ����� ����
            if (cell.HasRiver == true)
            {
                //���� ������ ����� ���� ����� ������ �����
                if (cell.HasRiverThroughEdge(direction) == true)
                {
                    //�������� ����������� ������� �����
                    e.v3.y = cell.StreamBedY;

                    //���� ������ ����� ������ ��� ����� ����
                    if (cell.HasRiverBeginOrEnd == true)
                    {
                        //������������� ����� � ������� ��� ������ ����
                        TriangulateWithRiverBeginOrEnd(
                            ref chunk,
                            ref cell,
                            direction,
                            center,
                            e);
                    }
                    //�����
                    else
                    {
                        //������������� ����� � �����
                        TriangulateWithRiver(
                            ref chunk,
                            ref cell,
                            direction,
                            center,
                            e);
                    }
                }
                //�����
                else
                {
                    //������������� ����� ����� ����
                    TriangulateAdjacentToRiver(
                        ref chunk,
                        ref cell,
                        direction,
                        center,
                        e);
                }
            }
            //�����
            else
            {
                //������������� ����� ��� ����
                TriangulateWithoutRiver(
                    ref chunk,
                    ref cell,
                    direction,
                    center,
                    e);

                //���� ������ �� ��� ����� � �� ����� ������ ����� �����
                if (cell.IsUnderwater == false
                    && cell.HasRoadThroughEdge(direction) == false)
                {
                    //��������� ������
                    chunk.features.AddFeature(
                        ref cell,
                        (center + e.v1 + e.v5) * (1f / 3f));
                }
            }

            //���� ����������� ������ ��� ����� ���-�������
            if (direction <= HexDirection.SE)
            {
                //������������� ����������
                TriangulateConnection(
                    ref chunk,
                    ref cell,
                    direction, 
                    e);
            }

            //���� ������ ��������� ��� �����
            if (cell.IsUnderwater == true)
            {
                //������������� ����
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
            //���� �������� ������� ������������
            Vector3 center = cell.Position;
            DHexEdgeVertices e = new(
                center + MapGenerationData.GetFirstCorner(direction),
                center + MapGenerationData.GetSecondCorner(direction));

            //������������� ����������� ��� ����
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
            //������������� ���� �����
            TriangulateEdgeFan(
                ref chunk,
                center,
                e,
                cell.Index);

            //���� ������ ����� ������ ����� ������ �����
            if (cell.HasRoads == true)
            {
                //���������� ��������
                Vector2 interpolators
                    = GetRoadInterpolators(
                        ref cell,
                        direction);

                //������������� ������
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
                    cell.HasRoadThroughEdge(direction),
                    cell.Index);
            }
        }

        void SimpleTriangulateWithoutRiver(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //����������, ����� �������� ��������� ����������
            float textureIndex = -1;

            //���� �� ������� �����-���� ����� �����
            if (inputData.Value.mapMode == UI.MapMode.Default)
            {
                textureIndex = cell.Index;
            }
            //�����, ���� ������� ����� ����������
            else if (inputData.Value.mapMode == UI.MapMode.Distance)
            {
                textureIndex = cell.mapDistance;
            }

            //������������� ���� �����
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
            //���� � ������ ���� ����� � ������� �����������
            if (cell.GetNeighbour(direction).Unpack(world.Value, out int neighbourCellEntity))
            {
                //���� ��������� �������� ������
                ref CHexRegion neighbourCell
                    = ref regionPool.Value.Get(neighbourCellEntity);

                //���� �������� ������ �����
                Vector3 bridge
                    = MapGenerationData.GetBridge(
                        direction);
                bridge.y = neighbourCell.Position.y - cell.Position.y;
                DHexEdgeVertices e2
                    = new(
                        e1.v1 + bridge,
                        e1.v5 + bridge);

                //����������, ����� �� ������ ���� ��� ������ ����� �����
                bool hasRiver = cell.HasRiverThroughEdge(direction);
                bool hasRoad = cell.HasRoadThroughEdge(direction);

                //���� ������ ����� ���� ����� ������ �����
                if (hasRiver == true)
                {
                    //�������� ����������� ������� �����
                    e2.v3.y = neighbourCell.StreamBedY;

                    //���������� ������ �������
                    Vector3 indices;
                    indices.x = indices.z = cell.Index;
                    indices.y = neighbourCell.Index;

                    //���� ������ �� ��������� ��� �����
                    if (cell.IsUnderwater == false)
                    {
                        //���� ����� �� ��������� ��� �����
                        if (neighbourCell.IsUnderwater == false)
                        {
                            //������������� ������� ����
                            TriangulateRiverQuad(
                                ref chunk,
                                e1.v2, e1.v4, e2.v2, e2.v4,
                                cell.RiverSurfaceY, neighbourCell.RiverSurfaceY,
                                0.8f,
                                cell.HasIncomingRiver && cell.IncomingRiver == direction,
                                indices);
                        }
                        //�����
                        else
                        {
                            //������������� �������
                            TriangulateWaterfallInWater(
                                ref chunk,
                                e1.v2, e1.v4, e2.v2, e2.v4,
                                cell.RiverSurfaceY, neighbourCell.RiverSurfaceY,
                                neighbourCell.WaterSurfaceY,
                                indices);
                        }
                    }
                    //�����, ���� ����� �� ��������� ��� �����
                    else if (neighbourCell.IsUnderwater == false
                        //� ������ ������ ������ ������ ���� ������
                        && neighbourCell.Elevation > cell.WaterLevel)
                    {
                        //������������� �������
                        TriangulateWaterfallInWater(
                            ref chunk,
                            e2.v4, e2.v2, e1.v4, e1.v2,
                            neighbourCell.RiverSurfaceY, cell.RiverSurfaceY,
                            cell.WaterSurfaceY,
                            indices);
                    }
                }

                //���� ��� ����� - ������
                if (MapGenerationData.GetEdgeType(cell.Elevation, neighbourCell.Elevation) == HexEdgeType.Slope)
                {
                    //������������� ������� �����
                    TriangulateEdgeTerraces(
                        ref chunk,
                        ref cell, e1,
                        ref neighbourCell, e2,
                        hasRoad);
                }
                //�����
                else
                {
                    //������������� ������ �����
                    TriangulateEdgeStrip(
                        ref chunk,
                        e1, MapGenerationData.weights1, cell.Index,
                        e2, MapGenerationData.weights2, neighbourCell.Index,
                        hasRoad);
                }

                //������� ����� � ���
                chunk.features.AddWall(
                    ref cell, e1,
                    ref neighbourCell, e2,
                    hasRiver,
                    hasRoad);

                //���� ����������� ������ ��� ����� �������
                if (direction <= HexDirection.E
                    //� � ������ ���� ����� �� ���������� �����������
                    && cell.GetNeighbour(direction.Next()).Unpack(world.Value, out int nextNeighbourCellEntity))
                {
                    //���� ��������� ���������� ������
                    ref CHexRegion nextNeighbourCell
                        = ref regionPool.Value.Get(nextNeighbourCellEntity);

                    //���������� ������ ������� ������������
                    Vector3 v5 
                        = e1.v5 + MapGenerationData.GetBridge(
                            direction.Next());
                    v5.y = nextNeighbourCell.Position.y;

                    //���� ������ ������ ������ ������ ������
                    if (cell.Elevation <= neighbourCell.Elevation)
                    {
                        //���� ������ ������ ������ ������ ���������� ������
                        if (cell.Elevation <= nextNeighbourCell.Elevation)
                        {
                            //������������� ����
                            TriangulateCorner(
                                ref chunk,
                                ref cell, e1.v5,
                                ref neighbourCell, e2.v5,
                                ref nextNeighbourCell, v5);
                        }
                        //�����
                        else
                        {
                            //������������� ����
                            TriangulateCorner(
                                ref chunk,
                                ref nextNeighbourCell, v5,
                                ref cell, e1.v5,
                                ref neighbourCell, e2.v5);
                        }
                    }
                    //�����, ���� ������ ������ ������ ������ ���������� ������
                    else if (neighbourCell.Elevation <= nextNeighbourCell.Elevation)
                    {
                        //������������� ����
                        TriangulateCorner(
                            ref chunk,
                            ref neighbourCell, e2.v5,
                            ref nextNeighbourCell, v5,
                            ref cell, e1.v5);
                    }
                    //�����
                    else
                    {
                        //������������� ����
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
            //���������� ������� ������� "������"
            Vector3 centerL;
            Vector3 centerR;
            //���� ������ ����� ���� ����� ��������������� �����
            if (cell.HasRiverThroughEdge(direction.Opposite()) == true)
            {
                //���������� ��������� ������
                centerL
                    = center 
                    + MapGenerationData.GetFirstSolidCorner(direction.Previous()) * 0.25f;
                centerR
                    = center 
                    + MapGenerationData.GetSecondSolidCorner(direction.Next()) * 0.25f;
            }
            //�����, ���� ������ ����� ���� ����� ��������� �����
            else if(cell.HasRiverThroughEdge(direction.Next()) == true)
            {
                //���������� ��������� ������
                centerL = center;
                centerR
                    = Vector3.Lerp(
                        center, e.v5, 
                        2f / 3f);
            }
            //�����, ���� ������ ����� ���� ����� ���������� �����
            else if(cell.HasRiverThroughEdge(direction.Previous()) == true)
            {
                //���������� ��������� ������
                centerL
                    = Vector3.Lerp(
                        center, e.v1,
                        2f / 3f);
                centerR = center;
            }
            //�����, ���� ������ ����� ���� ����� ������ ��������� �����
            else if(cell.HasRiverThroughEdge(direction.Next2()) == true)
            {
                //���������� ��������� ������
                centerL = center;
                centerR 
                    = center 
                    + MapGenerationData.GetSolidEdgeMiddle(direction.Next()) 
                    * (0.5f * MapGenerationData.innerToOuter);
            }
            //�����
            else
            {
                //���������� ��������� ������
                centerL 
                    = center 
                    + MapGenerationData.GetSolidEdgeMiddle(direction.Previous()) 
                    * (0.5f * MapGenerationData.innerToOuter);
                centerR = center;
            }
            //�������� ��������� ����������� �������
            center
                = Vector3.Lerp(
                    centerL, centerR, 
                    0.5f);

            //���������� ������� �����
            DHexEdgeVertices m
                = new(
                    Vector3.Lerp(
                        centerL, e.v1,
                        0.5f),
                    Vector3.Lerp(
                        centerR, e.v5,
                        0.5f),
                    1f / 6f);

            //�������� ����������� �������
            m.v3.y = center.y = e.v3.y;

            //������������� �����
            TriangulateEdgeStrip(
                ref chunk,
                m, MapGenerationData.weights1, cell.Index,
                e, MapGenerationData.weights1, cell.Index);

            //������� ����� � ���
            chunk.terrain.AddTriangle(
                centerL, m.v1, m.v2);
            chunk.terrain.AddQuad(
                centerL, center, m.v2, m.v3);
            chunk.terrain.AddQuad(
                center, centerR, m.v3, m.v4);
            chunk.terrain.AddTriangle(
                centerR, m.v4, m.v5);

            //������� ������ �������
            Vector3 indices;
            indices.x = indices.y = indices.z = cell.Index;
            chunk.terrain.AddTriangleCellData(
                indices,
                MapGenerationData.weights1);
            chunk.terrain.AddQuadCellData(
                indices,
                MapGenerationData.weights1);
            chunk.terrain.AddQuadCellData(
                indices,
                MapGenerationData.weights1);
            chunk.terrain.AddTriangleCellData(
                indices,
                MapGenerationData.weights1);

            //���� ������ �� ��������� ��� �����
            if (cell.IsUnderwater == false)
            {
                //����������, ������ �� ���� ��������� ����
                bool reversed
                    = cell.IncomingRiver == direction;

                //������� ������� ���� � ���
                TriangulateRiverQuad(
                    ref chunk,
                    centerL, centerR, m.v2, m.v4,
                    cell.RiverSurfaceY,
                    0.4f,
                    reversed,
                    indices);
                TriangulateRiverQuad(
                    ref chunk,
                    m.v2, m.v4, e.v2, e.v4,
                    cell.RiverSurfaceY,
                    0.6f,
                    reversed,
                    indices);
            }
        }

        void TriangulateWithRiverBeginOrEnd(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //���������� ������� �����
            DHexEdgeVertices m 
                = new(
                    Vector3.Lerp(center, e.v1, 0.5f),
                    Vector3.Lerp(center, e.v5, 0.5f));

            //�������� ����������� �������
            m.v3.y = e.v3.y;

            //������������� �����
            TriangulateEdgeStrip(
                ref chunk,
                m, MapGenerationData.weights1, cell.Index,
                e, MapGenerationData.weights1, cell.Index);
            TriangulateEdgeFan(
                ref chunk,
                center,
                m,
                cell.Index);

            //���� ������ �� ��������� ��� �����
            if (cell.IsUnderwater == false)
            {
                //����������, ������ �� ���� ��������� ����
                bool reversed = cell.HasIncomingRiver;

                //���������� ������ �������
                Vector3 indices;
                indices.x = indices.y = indices.z = cell.Index;

                //������� ������� ���� � ���
                TriangulateRiverQuad(
                    ref chunk,
                    m.v2, m.v4, e.v2, e.v4,
                    cell.RiverSurfaceY,
                    0.6f,
                    reversed,
                    indices);

                //���������� �������� ������ �������
                center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;

                //������� ����������� � ���
                chunk.rivers.AddTriangle(center, m.v2, m.v4);
                //���� ������� ���������
                if (reversed == true)
                {
                    //������� UV-���������� � ���
                    chunk.rivers.AddTriangleUV(
                        new(0.5f, 0.4f), new(1f, 0.2f), new(0f, 0.2f));
                }
                else
                {
                    //������� UV-���������� � ���
                    chunk.rivers.AddTriangleUV(
                        new(0.5f, 0.4f), new(0f, 0.6f), new(1f, 0.6f));
                }

                //������� ������ �������
                chunk.rivers.AddTriangleCellData(
                    indices,
                    MapGenerationData.weights1);
            }
        }

        void TriangulateAdjacentToRiver(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //���� ������ ����� ������
            if(cell.HasRoads == true)
            {
                //������������� ������ ����� ����
                TriangulateRoadAdjacentToRiver(
                    ref chunk,
                    ref cell,
                    direction,
                    center,
                    e);
            }   
            
            //���� ������ ����� ���� ����� ��������� �����
            if (cell.HasRiverThroughEdge(direction.Next()) == true)
            {
                //���� ������ ����� ���� ����� ���������� �����
                if (cell.HasRiverThroughEdge(direction.Previous()) == true)
                {
                    //������� �����
                    center
                        += MapGenerationData.GetSolidEdgeMiddle(direction)
                        * (MapGenerationData.innerToOuter * 0.5f);
                }
                //�����, ���� ������ ����� ���� ����� ������ ���������� �����
                else if(cell.HasRiverThroughEdge(direction.Previous2()) == true)
                {
                    //������� �����
                    center
                        += MapGenerationData.GetFirstSolidCorner(direction)
                        * 0.25f;
                }
            }
            //�����, ���� ������ ����� ���� ����� ���������� � ������ ��������� ����
            else if(cell.HasRiverThroughEdge(direction.Previous())
                && cell.HasRiverThroughEdge(direction.Next2()))
            {
                //������� �����
                center += MapGenerationData.GetSecondSolidCorner(direction) * 0.25f;
            }

            //���������� ������� �����
            DHexEdgeVertices m 
                = new(
                    Vector3.Lerp(
                        center, e.v1, 
                        0.5f),
                    Vector3.Lerp(
                        center, e.v5, 
                        0.5f));

            //������������� �����
            TriangulateEdgeStrip(
                ref chunk,
                m, MapGenerationData.weights1, cell.Index, 
                e, MapGenerationData.weights1, cell.Index);
            TriangulateEdgeFan(
                ref chunk,
                center, 
                m,
                cell.Index);

            //���� ������ �� ��� ����� � �� ����� ������ ����� �����
            if (cell.IsUnderwater == false
                && cell.HasRoadThroughEdge(direction) == false)
            {
                //��������� ������
                chunk.features.AddFeature(
                    ref cell,
                    (center + e.v1 + e.v5) * (1f / 3f));
            }
        }

        void TriangulateWaterfallInWater(
            ref CHexChunk chunk,
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y1, float y2, 
            float waterY,
            Vector3 indices)
        {
            //���������� ������� ��������
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;
            v1 = MapGenerationData.Perturb(v1);
            v2 = MapGenerationData.Perturb(v2);
            v3 = MapGenerationData.Perturb(v3);
            v4 = MapGenerationData.Perturb(v4);
            float t 
                = (waterY - y2) / (y1 - y2);
            v3 = Vector3.Lerp(
                v3, v1, 
                t);
            v4 = Vector3.Lerp(
                v4, v2, 
                t);

            //������� ���� � ���
            chunk.rivers.AddQuadUnperturbed(v1, v2, v3, v4);
            chunk.rivers.AddQuadUV(0f, 1f, 0.8f, 1f);

            //������� ������ �������
            chunk.rivers.AddQuadCellData(
                indices,
                MapGenerationData.weights1, MapGenerationData.weights2);
        }

        void TriangulateEdgeTerraces(
            ref CHexChunk chunk,
            ref CHexRegion beginCell, DHexEdgeVertices begin,
            ref CHexRegion endCell, DHexEdgeVertices end,
            bool hasRoad)
        {
            //���������� �������� ������� ������� �����
            DHexEdgeVertices e2 = DHexEdgeVertices.TerraceLerp(
                begin, end,
                1);
            Color w2 = MapGenerationData.TerraceLerp(
                MapGenerationData.weights1, MapGenerationData.weights2, 
                1);
            float i1 = beginCell.Index;
            float i2 = endCell.Index;

            //������� ������ ������ � ���
            TriangulateEdgeStrip(
                ref chunk,
                begin, MapGenerationData.weights1, i1,
                e2, w2, i2,
                hasRoad);

            //��� ������� �������������� �����
            for (int a = 2; a < MapGenerationData.terraceSteps; a++)
            {
                //���������� ��������� ������ �����
                DHexEdgeVertices e1 = e2;
                Color c1 = w2;

                //���������� �������� ������ �����
                e2 = DHexEdgeVertices.TerraceLerp(
                    begin, end,
                    a);
                w2 = MapGenerationData.TerraceLerp(
                    MapGenerationData.weights1, MapGenerationData.weights2,
                    a);

                //������� ������������� ������ � ���
                TriangulateEdgeStrip(
                    ref chunk,
                    e1, c1, i1,
                    e2, w2, i2,
                    hasRoad);
            }

            //������� ��������� ������ � ���
            TriangulateEdgeStrip(
                ref chunk,
                e2, w2, i1,
                end, MapGenerationData.weights2, i2,
                hasRoad);
        }

        void TriangulateCorner(
            ref CHexChunk chunk,
            ref CHexRegion bottomCell, Vector3 bottom,
            ref CHexRegion leftCell, Vector3 left,
            ref CHexRegion rightCell, Vector3 right)
        {
            //���������� ���� ����
            HexEdgeType leftEdgeType
                = MapGenerationData.GetEdgeType(
                    bottomCell.Elevation, leftCell.Elevation);
            HexEdgeType rightEdgeType
                = MapGenerationData.GetEdgeType(
                    bottomCell.Elevation, rightCell.Elevation);

            //���� ��� ������ ����� - �����
            if (leftEdgeType == HexEdgeType.Slope)
            {
                //���� ��� ������� ����� - �����
                if (rightEdgeType == HexEdgeType.Slope)
                {
                    //������������� ������� �������
                    TriangulateCornerTerraces(
                        ref chunk,
                        ref bottomCell, bottom,
                        ref leftCell, left,
                        ref rightCell, right);
                }
                //�����, ���� ��� ������� ����� - ���������
                else if (rightEdgeType == HexEdgeType.Flat)
                {
                    //������������� ������� �������
                    TriangulateCornerTerraces(
                        ref chunk,
                        ref leftCell, left,
                        ref rightCell, right,
                        ref bottomCell, bottom);
                }
                //�����
                else
                {
                    //������������� ���� ������ � ������
                    TriangulateCornerTerracesCliff(
                        ref chunk,
                        ref bottomCell, bottom,
                        ref leftCell, left,
                        ref rightCell, right);
                }
            }
            //�����, ���� ��� ������� ����� - �����
            else if (rightEdgeType == HexEdgeType.Slope)
            {
                //���� ��� ������ ����� - ���������
                if (leftEdgeType == HexEdgeType.Flat)
                {
                    //������������� ������� �������
                    TriangulateCornerTerraces(
                        ref chunk,
                        ref rightCell, right,
                        ref bottomCell, bottom,
                        ref leftCell, left);
                }
                //�����
                else
                {
                    //������������� ������� �������
                    TriangulateCornerCliffTerraces(
                        ref chunk,
                        ref bottomCell, bottom,
                        ref leftCell, left,
                        ref rightCell, right);
                }
            }
            //�����, ���� ��� ����� ����� ����� � ������ ������� - �����
            else if (MapGenerationData.GetEdgeType(leftCell.Elevation, rightCell.Elevation) == HexEdgeType.Slope)
            {
                //���� ������ ������ ������ ������ ������ �������
                if (leftCell.Elevation < rightCell.Elevation)
                {
                    //������������� ������� �������
                    TriangulateCornerCliffTerraces(
                        ref chunk,
                        ref rightCell, right,
                        ref bottomCell, bottom,
                        ref leftCell, left);
                }
                //�����
                else
                {
                    //������������� ������� �������
                    TriangulateCornerTerracesCliff(
                        ref chunk,
                        ref leftCell, left,
                        ref rightCell, right,
                        ref bottomCell, bottom);
                }
            }
            //�����
            else
            {
                //������� ����������� � ���
                chunk.terrain.AddTriangle(
                    bottom,left,right);

                //������� ������ �������
                Vector3 indices;
                indices.x = bottomCell.Index;
                indices.y = leftCell.Index;
                indices.z = rightCell.Index;
                chunk.terrain.AddTriangleCellData(
                    indices,
                    MapGenerationData.weights1, MapGenerationData.weights2, MapGenerationData.weights3);
            }

            //������������� �����
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
            //���������� ������� ������������
            Vector3 v3 
                = MapGenerationData.TerraceLerp(
                    begin, left, 
                    1);
            Vector3 v4 
                = MapGenerationData.TerraceLerp(
                    begin, right, 
                    1);
            Color w3 
                = MapGenerationData.TerraceLerp(
                    MapGenerationData.weights1, MapGenerationData.weights2, 
                    1);
            Color w4 
                = MapGenerationData.TerraceLerp(
                    MapGenerationData.weights1, MapGenerationData.weights3, 
                    1);
            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            //������� ������ ����������� � ���
            chunk.terrain.AddTriangle(
                begin, v3, v4);
            chunk.terrain.AddTriangleCellData(
                indices,
                MapGenerationData.weights1, w3, w4);

            //��� ������� �������������� �����
            for (int a = 2; a < MapGenerationData.terraceSteps; a++)
            {
                //���������� ��������� ������ �����
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color w1 = w3;
                Color w2 = w4;

                //���������� �������� ������ �����
                v3 = MapGenerationData.TerraceLerp(
                    begin, left, 
                    a);
                v4 = MapGenerationData.TerraceLerp(
                    begin, right, 
                    a);
                w3 = MapGenerationData.TerraceLerp(
                    MapGenerationData.weights1, MapGenerationData.weights2, 
                    a);
                w4 = MapGenerationData.TerraceLerp(
                    MapGenerationData.weights1, MapGenerationData.weights3, 
                    a);

                //������� ������������� ���� � ���
                chunk.terrain.AddQuad(
                    v1, v2, v3, v4);
                chunk.terrain.AddQuadCellData(
                    indices,
                    w1, w2, w3, w4);
            }

            //������� ��������� ���� � ���
            chunk.terrain.AddQuad(
                v3, v4, left, right);
            chunk.terrain.AddQuadCellData(
                indices,
                w3, w4, MapGenerationData.weights2, MapGenerationData.weights3);
        }

        void TriangulateCornerTerracesCliff(
            ref CHexChunk chunk,
            ref CHexRegion beginCell, Vector3 begin,
            ref CHexRegion leftCell, Vector3 left,
            ref CHexRegion rightCell, Vector3 right)
        {
            //���������� ��������� ������� ��������� ������
            float b = 1f 
                / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            Vector3 boundary
                = Vector3.Lerp(
                    MapGenerationData.Perturb(begin), MapGenerationData.Perturb(right),
                    b);
            Color boundaryWeights
                = Color.Lerp(
                    MapGenerationData.weights1, MapGenerationData.weights3,
                    b);
            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            //������������� ����������� �����������
            TriangulateBoundaryTriangle(
                ref chunk,
                MapGenerationData.weights1, begin,
                MapGenerationData.weights2, left,
                boundaryWeights, boundary,
                indices);
            
            //���� ������� ����� ����� � ������ ������� - �����
            if (MapGenerationData.GetEdgeType(leftCell.Elevation, rightCell.Elevation) == HexEdgeType.Slope)
            {
                //������������� ����������� �����������
                TriangulateBoundaryTriangle(
                    ref chunk,
                    MapGenerationData.weights2, left,
                    MapGenerationData.weights3, right,
                    boundaryWeights, boundary,
                    indices);
            }
            //�����
            else
            {
                //������� ����������� � ���
                chunk.terrain.AddTriangleUnperturbed(
                    MapGenerationData.Perturb(left), MapGenerationData.Perturb(right), boundary);
                chunk.terrain.AddTriangleCellData(
                    indices,
                    MapGenerationData.weights2, MapGenerationData.weights3, boundaryWeights);
            }
        }

        void TriangulateCornerCliffTerraces(
            ref CHexChunk chunk,
            ref CHexRegion beginCell, Vector3 begin,
            ref CHexRegion leftCell, Vector3 left,
            ref CHexRegion rightCell, Vector3 right)
        {
            //���������� ��������� ������� ��������� ������
            float b = 1f
                / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            Vector3 boundary
                = Vector3.Lerp(
                    MapGenerationData.Perturb(begin), MapGenerationData.Perturb(left),
                    b);
            Color boundaryWeights
                = Color.Lerp(
                    MapGenerationData.weights1, MapGenerationData.weights2,
                    b);
            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            //������������� ����������� �����������
            TriangulateBoundaryTriangle(
                ref chunk,
                MapGenerationData.weights3, right,
                MapGenerationData.weights1, begin,
                boundaryWeights, boundary,
                indices);

            //���� ������� ����� ����� � ������ ������� - �����
            if (MapGenerationData.GetEdgeType(leftCell.Elevation, rightCell.Elevation) == HexEdgeType.Slope)
            {
                //������������� ����������� �����������
                TriangulateBoundaryTriangle(
                    ref chunk,
                    MapGenerationData.weights2, left,
                    MapGenerationData.weights3, right,
                    boundaryWeights, boundary,
                    indices);
            }
            //�����
            else
            {
                //������� ����������� � ���
                chunk.terrain.AddTriangleUnperturbed(
                    MapGenerationData.Perturb(left), MapGenerationData.Perturb(right), boundary);
                chunk.terrain.AddTriangleCellData(
                    indices,
                    MapGenerationData.weights2, MapGenerationData.weights3, boundaryWeights);
            }
        }

        void TriangulateBoundaryTriangle(
            ref CHexChunk chunk,
            Color beginWeights, Vector3 begin,
            Color leftWeights, Vector3 left,
            Color boundaryWeights, Vector3 boundary,
            Vector3 indices)
        {
            //���������� ��������� ������� ���������
            Vector3 v2 = MapGenerationData.Perturb(
                MapGenerationData.TerraceLerp(
                    begin, left,
                    1));
            Color w2 = MapGenerationData.TerraceLerp(
                beginWeights, leftWeights,
                1);

            //������� ������ ����������� � ���
            chunk.terrain.AddTriangleUnperturbed(
                MapGenerationData.Perturb(begin), v2, boundary);
            chunk.terrain.AddTriangleCellData(
                indices,
                beginWeights, w2, boundaryWeights);

            //��� ������� �������������� ������������
            for (int a = 2; a < MapGenerationData.terraceSteps; a++)
            {
                //���������� ��������� ������ ������������
                Vector3 v1 = v2;
                Color w1 = w2;

                //���������� �������� ������ ������������
                v2 = MapGenerationData.Perturb(
                    MapGenerationData.TerraceLerp(
                        begin, left,
                        a));
                w2 = MapGenerationData.TerraceLerp(
                    beginWeights, leftWeights,
                    a);

                //������� ������������� ����������� � ���
                chunk.terrain.AddTriangleUnperturbed(
                    v1, v2, boundary);
                chunk.terrain.AddTriangleCellData(
                    indices,
                    w1, w2, boundaryWeights);
            }

            //������� ��������� ����������� � ���
            chunk.terrain.AddTriangleUnperturbed(
                v2, MapGenerationData.Perturb(left), boundary);
            chunk.terrain.AddTriangleCellData(
                indices,
                w2, leftWeights, boundaryWeights);
        }

        void TriangulateEdgeFan(
            ref CHexChunk chunk,
            Vector3 center,
            DHexEdgeVertices edge,
            float index)
        {
            //������� ������������ � ���
            chunk.terrain.AddTriangle(
                center, edge.v1, edge.v2);
            chunk.terrain.AddTriangle(
                center, edge.v2, edge.v3);
            chunk.terrain.AddTriangle(
                center, edge.v3, edge.v4);
            chunk.terrain.AddTriangle(
                center, edge.v4, edge.v5);

            //������� ������ �������
            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            chunk.terrain.AddTriangleCellData(indices, MapGenerationData.weights1);
            chunk.terrain.AddTriangleCellData(indices, MapGenerationData.weights1);
            chunk.terrain.AddTriangleCellData(indices, MapGenerationData.weights1);
            chunk.terrain.AddTriangleCellData(indices, MapGenerationData.weights1);
        }

        void TriangulateEdgeStrip(
            ref CHexChunk chunk,
            DHexEdgeVertices e1, Color w1, float index1,
            DHexEdgeVertices e2, Color w2, float index2,
            bool hasRoad = false)
        {
            //������� ����� � ���
            chunk.terrain.AddQuad(
                e1.v1, e1.v2, e2.v1, e2.v2);
            chunk.terrain.AddQuad(
                e1.v2, e1.v3, e2.v2, e2.v3);
            chunk.terrain.AddQuad(
                e1.v3, e1.v4, e2.v3, e2.v4);
            chunk.terrain.AddQuad(
                e1.v4, e1.v5, e2.v4, e2.v5);

            //������� ������ �������
            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;
            chunk.terrain.AddQuadCellData(indices, w1, w2);
            chunk.terrain.AddQuadCellData(indices, w1, w2);
            chunk.terrain.AddQuadCellData(indices, w1, w2);
            chunk.terrain.AddQuadCellData(indices, w1, w2);

            //���� � ������ ����������� ������������ ������
            if (hasRoad == true)
            {
                //������������� ������� ������
                TriangulateRoadSegment(
                    ref chunk,
                    e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4,
                    w1, w2,
                    indices);
            }
        }

        void TriangulateRiverQuad(
            ref CHexChunk chunk,
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y,
            float v,
            bool reversed,
            Vector3 indices)
        {
            //������������� ������� ����
            TriangulateRiverQuad(
                ref chunk,
                v1, v2, v3, v4,
                y, y,
                v,
                reversed,
                indices);
        }

        void TriangulateRiverQuad(
            ref CHexChunk chunk,
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y1, float y2,
            float v,
            bool reversed,
            Vector3 indices)
        {
            //�������� �������
            v1.y = v2.y = y1; 
            v3.y = v4.y = y2;

            //������� ���� � ���
            chunk.rivers.AddQuad(
                v1, v2, v3, v4);
            
            //���� ���� ���������
            if (reversed == true)
            {
                //������� UV-���������� � ���
                chunk.rivers.AddQuadUV(
                    1f, 0f, 0.8f - v, 0.6f - v);
            }
            //�����
            else
            {
                //������� UV-���������� � ���
                chunk.rivers.AddQuadUV(
                    0f, 1f, v, v + 0.2f);
            }

            //������� ������ �������
            chunk.rivers.AddQuadCellData(
                indices,
                MapGenerationData.weights1, MapGenerationData.weights2);
        }

        void TriangulateRoad(
            ref CHexChunk chunk,
            Vector3 center, Vector3 mL, Vector3 mR,
            DHexEdgeVertices e,
            bool hasRoadThroughEdge,
            float index)
        {
            //���� ������ ����� ������ ����� �����
            if(hasRoadThroughEdge == true)
            {
                //���������� ������ �������
                Vector3 indices;
                indices.x = indices.y = indices.z = index;

                //���������� ����������� �������
                Vector3 mC
                    = Vector3.Lerp(
                        mL, mR,
                        0.5f);

                //������������� ������� ������
                TriangulateRoadSegment(
                    ref chunk,
                    mL, mC, mR, e.v2, e.v3, e.v4,
                    MapGenerationData.weights1, MapGenerationData.weights1,
                    indices);

                //������� ������������ � ���
                chunk.roads.AddTriangle(
                    center, mL, mC);
                chunk.roads.AddTriangle(
                    center, mC, mR);
                chunk.roads.AddTriangleUV(
                    new(1f, 0f), new(0f, 0f), new(1f, 0f));
                chunk.roads.AddTriangleUV(
                    new(1f, 0f), new(1f, 0f), new(0f, 0f));

                //������� ������ �������
                chunk.roads.AddTriangleCellData(
                    indices, 
                    MapGenerationData.weights1);
                chunk.roads.AddTriangleCellData(
                    indices,
                    MapGenerationData.weights1);
            }
            //�����
            else
            {
                //������������� ����� ������
                TriangulateRoadEdge(
                    ref chunk,
                    center, mL, mR,
                    index);
            }
        }

        void TriangulateRoadAdjacentToRiver(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center,
            DHexEdgeVertices e)
        {
            //����������, ����� �� ������ ������ ����� ������ �����
            bool hasRoadThroughEdge
                = cell.HasRoadThroughEdge(direction);
            //����������, ����� �� ������ ���� ����� ���������� � ��������� ����
            bool previousHasRiver
                = cell.HasRiverThroughEdge(direction.Previous());
            bool nextHasRiver
                = cell.HasRiverThroughEdge(direction.Next());

            //���������� ��������
            Vector2 interpolators
                = GetRoadInterpolators(
                    ref cell,
                    direction);

            //���������� ������� ������
            Vector3 roadCenter = center;
            //���� ������ ����� ������ ��� ����� ����
            if (cell.HasRiverBeginOrEnd == true)
            {
                //������� ����� ������ � ������� ����� ������
                roadCenter
                    += MapGenerationData.GetSolidEdgeMiddle(
                        cell.RiverBeginOrEndDirection.Opposite()) * (1f / 3f);
            }
            //�����, ���� ���� ��� � ��������������� ������������
            else if (cell.IncomingRiver == cell.OutgoingRiver.Opposite())
            {
                //���������� ����
                Vector3 corner;

                //���� ������ ����� ���� ����� ���������� �����
                if (previousHasRiver == true)
                {
                    //���� ������ �� ����� ������ ����� ������� ����� 
                    if(hasRoadThroughEdge == false
                        //� �� ����� ������ ����� ��������� �����
                        && cell.HasRoadThroughEdge(direction.Next()) == false)
                    {
                        //������� �� �������
                        return;
                    }

                    corner = MapGenerationData.GetSecondSolidCorner(direction);
                }
                //�����
                else
                {
                    //���� ������ �� ����� ������ ����� ������� ����� 
                    if (hasRoadThroughEdge == false
                        //� �� ����� ������ ����� ���������� �����
                        && cell.HasRoadThroughEdge(direction.Previous()) == false)
                    {
                        //������� �� �������
                        return;
                    }

                    corner = MapGenerationData.GetFirstSolidCorner(direction);
                }

                //������� ����� ������
                roadCenter
                    += corner * 0.5f;

                //���� ��������� ���� ��� ����� ��������� �����������
                if (cell.IncomingRiver == direction.Next()
                    //� ������ ���� �� ��� ������� �� ����
                    && (cell.HasRoadThroughEdge(direction.Next2())
                    || cell.HasRoadThroughEdge(direction.Opposite())))
                {
                    //������ ����
                    chunk.features.AddBridge(
                        roadCenter, center - corner * 0.5f);
                }

                center
                    += corner * 0.25f;
            }
            //�����, ���� ������ ����� �������� ���� ����� �����, ���������� ���������
            else if(cell.IncomingRiver == cell.OutgoingRiver.Previous() == true)
            {
                //������� ����� ������
                roadCenter
                    -= MapGenerationData.GetSecondCorner(cell.IncomingRiver) * 0.2f;
            }
            //�����, ���� ������ ����� �������� ���� ����� �����, ��������� ���������
            else if(cell.IncomingRiver == cell.OutgoingRiver.Next() == true)
            {
                //������� ����� ������
                roadCenter
                    -= MapGenerationData.GetFirstCorner(cell.IncomingRiver) * 0.2f;
            }
            //�����, ���� ������ ����� ���� ����� ��� �������� �����
            else if(previousHasRiver && nextHasRiver == true)
            {
                //���� ������ �� ����� ������ � ������ �����������
                if (hasRoadThroughEdge == false)
                {
                    //������� �� �������
                    return;
                }

                //������� ����� ������
                Vector3 offset
                    = MapGenerationData.GetSolidEdgeMiddle(direction)
                    * MapGenerationData.innerToOuter;
                roadCenter
                    += offset * 0.7f;
                center
                    += offset * 0.5f;
            }
            //�����
            else
            {
                //���������� ����������� �����������
                HexDirection middle;
                //���� ������ ����� ���� ����� ���������� �����������
                if (previousHasRiver == true)
                {
                    middle = direction.Next();
                }
                //�����, ���� ������ ����� ���� ����� ��������� �����������
                else if (nextHasRiver == true)
                {
                    middle = direction.Previous();
                }
                //�����
                else
                {
                    middle = direction;
                }

                //���� ������ �� ����� ������ �� ����� ���� ����������� �� ��������
                if (cell.HasRoadThroughEdge(middle) == false
                    && cell.HasRoadThroughEdge(middle.Previous()) == false
                    && cell.HasRoadThroughEdge(middle.Next()) == false)
                {
                    //������� �� �������
                    return;
                }

                //���������� ��������
                Vector3 offset = MapGenerationData.GetSolidEdgeMiddle(middle);
                //������� ����� ������
                roadCenter += offset * 0.25f;

                //���� ����������� - �����������
                if (direction == middle
                    //� ������ ���� �� ��� ������� �� ����
                    && cell.HasRoadThroughEdge(direction.Opposite()) == true)
                {
                    //������ ����
                    chunk.features.AddBridge(
                        roadCenter, center - offset * (MapGenerationData.innerToOuter * 0.7f));
                }
            }

            Vector3 mL = Vector3.Lerp(
                roadCenter, e.v1,
                interpolators.x);
            Vector3 mR = Vector3.Lerp(
                roadCenter, e.v5,
                interpolators.y);

            //������������� ������
            TriangulateRoad(
                ref chunk,
                roadCenter, mL, mR,
                e, 
                hasRoadThroughEdge,
                cell.Index);

            //���� ������ ����� ���� ����� ���������� �����
            if (previousHasRiver == true)
            {
                //������������� ����� ������
                TriangulateRoadEdge(
                    ref chunk,
                    roadCenter, center, mL,
                    cell.Index);
            }
            //���� ������ ����� ���� ����� ��������� �����
            if (nextHasRiver == true)
            {
                //������������� ����� ������
                TriangulateRoadEdge(
                    ref chunk,
                    roadCenter, mR, center,
                    cell.Index);
            }
        }

        void TriangulateRoadSegment(
            ref CHexChunk chunk,
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6,
            Color w1, Color w2,
            Vector3 indices)
        {
            //������� ����� � ���
            chunk.roads.AddQuad(v1, v2, v4, v5);
            chunk.roads.AddQuad(v2, v3, v5, v6);
            chunk.roads.AddQuadUV(0f, 1f, 0f, 0f);
            chunk.roads.AddQuadUV(1f, 0f, 0f, 0f);

            //������� ������ �������
            chunk.roads.AddQuadCellData(
                indices, 
                w1, w2);
            chunk.roads.AddQuadCellData(
                indices, 
                w1, w2);
        }

        void TriangulateRoadEdge(
            ref CHexChunk chunk,
            Vector3 center, Vector3 mL, Vector3 mR,
            float index)
        {
            //������� ����������� � ���
            chunk.roads.AddTriangle(center, mL, mR);
            chunk.roads.AddTriangleUV(new(1f, 0f), new(0f, 0f), new(0f, 0f));

            //������� ������ �������
            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            chunk.roads.AddTriangleCellData(
                indices,
                MapGenerationData.weights1);
        }

        void TriangulateWater(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center)
        {
            //���������� ��������� ������
            center.y = cell.WaterSurfaceY;

            //���� � ������ ���� ����� � ������� �����������
            if (cell.GetNeighbour(direction).Unpack(world.Value, out int neighbourCellEntity))
            {
                //���� ��������� ������
                ref CHexRegion neighbourCell
                    = ref regionPool.Value.Get(neighbourCellEntity);

                //���� ����� ��������� ��� �����
                if (neighbourCell.IsUnderwater == true)
                {
                    //������������� �����
                    TriangulateOpenWater(
                        ref chunk,
                        ref cell, ref neighbourCell,
                        direction,
                        center);
                }
                //�����
                else
                {
                    //������������� ���������
                    TriangulateShore(
                        ref chunk,
                        ref cell, ref neighbourCell,
                        direction,
                        center);
                }
            }
            //�����
            else
            {
                //������������� ���� �� ���� �����
                TriangulateOpenWater(
                    ref chunk,
                    ref cell,
                    direction,
                    center);
            }
        }

        void TriangulateOpenWater(
            ref CHexChunk chunk,
            ref CHexRegion cell, ref CHexRegion neighbourCell,
            HexDirection direction,
            Vector3 center)
        {
            //���������� ��������� �����
            Vector3 c1
                = center + MapGenerationData.GetFirstWaterCorner(direction);
            Vector3 c2
                = center + MapGenerationData.GetSecondWaterCorner(direction);

            //������� ����������� � ���
            chunk.water.AddTriangle(
                center, c1, c2);

            //������� ������ �������
            Vector3 indices;
            indices.x = indices.y = indices.z = cell.Index;
            chunk.water.AddTriangleCellData(
                indices,
                MapGenerationData.weights1);

            //���� ����������� ������ ��� ����� ���-�������
            if (direction <= HexDirection.SE)
            {
                //���������� ������� ����������
                Vector3 bridge = MapGenerationData.GetWaterBridge(direction);
                Vector3 e1 
                    = c1 + bridge;
                Vector3 e2 
                    = c2 + bridge;

                //������� ���� � ���
                chunk.water.AddQuad(
                    c1, c2, e1, e2);

                //������� ������ �������
                indices.y = neighbourCell.Index;
                chunk.water.AddQuadCellData(
                    indices,
                    MapGenerationData.weights1, MapGenerationData.weights2);

                //���� ����������� ������ ��� ����� �������
                if (direction <= HexDirection.E)
                {
                    //���� � ������ ���� ��������� �����
                    if (cell.GetNeighbour(direction.Next()).Unpack(world.Value, out int nextNeighbourCellEntity))
                    {
                        //���� ��������� ���������� ������
                        ref CHexRegion nextNeighbourCell
                            = ref regionPool.Value.Get(nextNeighbourCellEntity);

                        //���� ����� �� ��������� ��� �����
                        if (nextNeighbourCell.IsUnderwater == false)
                        {
                            //������� �� �������
                            return;
                        }

                        //������� ����������� � ���
                        chunk.water.AddTriangle(
                            c2, e2, c2 + MapGenerationData.GetWaterBridge(direction.Next()));

                        //������� ������ �������
                        indices.z = nextNeighbourCell.Index;
                        chunk.water.AddTriangleCellData(
                            indices,
                            MapGenerationData.weights1, MapGenerationData.weights2, MapGenerationData.weights3);
                    }
                }
            }
        }

        void TriangulateOpenWater(
            ref CHexChunk chunk,
            ref CHexRegion cell,
            HexDirection direction,
            Vector3 center)
        {
            //���������� ��������� �����
            Vector3 c1
                = center + MapGenerationData.GetFirstWaterCorner(direction);
            Vector3 c2
                = center + MapGenerationData.GetSecondWaterCorner(direction);

            //������� ����������� � ���
            chunk.water.AddTriangle(
                center, c1, c2);

            //������� ������ �������
            Vector3 indices;
            indices.x = indices.y = indices.z = cell.Index;
            chunk.water.AddTriangleCellData(
                indices,
                MapGenerationData.weights1);
        }

        void TriangulateShore(
            ref CHexChunk chunk,
            ref CHexRegion cell, ref CHexRegion neighbourCell,
            HexDirection direction,
            Vector3 center)
        {
            //���������� ������� �����
            DHexEdgeVertices e1
                = new(
                    center + MapGenerationData.GetFirstWaterCorner(direction),
                    center + MapGenerationData.GetSecondWaterCorner(direction));

            //������� ������������ � ���
            chunk.water.AddTriangle(
                center, e1.v1, e1.v2);
            chunk.water.AddTriangle(
                center, e1.v2, e1.v3);
            chunk.water.AddTriangle(
                center, e1.v3, e1.v4);
            chunk.water.AddTriangle(
                center, e1.v4, e1.v5);

            //������� ������ �������
            Vector3 indices;
            indices.x = indices.z = cell.Index;
            indices.y = neighbourCell.Index;
            chunk.water.AddTriangleCellData(
                indices,
                MapGenerationData.weights1);
            chunk.water.AddTriangleCellData(
                indices,
                MapGenerationData.weights1);
            chunk.water.AddTriangleCellData(
                indices,
                MapGenerationData.weights1);
            chunk.water.AddTriangleCellData(
                indices, 
                MapGenerationData.weights1);

            //���������� ������� ����������
            Vector3 center2 = neighbourCell.Position;
            //���� ����� ��������� �� ��������� ���� �����
            if (neighbourCell.ColumnIndex < cell.ColumnIndex - 1)
            {
                center2.x += MapGenerationData.wrapSize * MapGenerationData.innerDiameter;
            }
            //�����, ���� ����� ��������� �� �������� ���� �����
            else if(neighbourCell.ColumnIndex > cell.ColumnIndex + 1)
            {
                center2.x -= MapGenerationData.wrapSize * MapGenerationData.innerDiameter;
            }

            center2.y = center.y;
            DHexEdgeVertices e2 = new(
                center2 + MapGenerationData.GetSecondSolidCorner(direction.Opposite()),
                center2 + MapGenerationData.GetFirstSolidCorner(direction.Opposite()));

            //���� ������ ����� ���� ����� ������ �����
            if (cell.HasRiverThroughEdge(direction) == true)
            {
                //������������� �����
                TriangulateEstuary(
                    ref chunk,
                    e1, e2,
                    cell.HasIncomingRiver && cell.IncomingRiver == direction,
                    indices);
            }
            else
            {
                //������� ����� � ���
                chunk.waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
                chunk.waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
                chunk.waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
                chunk.waterShore.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
                chunk.waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                chunk.waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                chunk.waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                chunk.waterShore.AddQuadUV(0f, 0f, 0f, 1f);

                //������� ������ �������
                chunk.waterShore.AddQuadCellData(
                    indices, 
                    MapGenerationData.weights1, MapGenerationData.weights2);
                chunk.waterShore.AddQuadCellData(
                    indices, 
                    MapGenerationData.weights1, MapGenerationData.weights2);
                chunk.waterShore.AddQuadCellData(
                    indices, 
                    MapGenerationData.weights1, MapGenerationData.weights2);
                chunk.waterShore.AddQuadCellData(
                    indices, 
                    MapGenerationData.weights1, MapGenerationData.weights2);
            }

            //���� � ������ ���� ��������� �����
            if (cell.GetNeighbour(direction.Next()).Unpack(world.Value, out int nextNeighbourCellEntity))
            {
                //���� ��������� ���������� ������
                ref CHexRegion nextNeighbourCell
                    = ref regionPool.Value.Get(nextNeighbourCellEntity);

                //���������� ����� ���������� ������
                Vector3 center3 = nextNeighbourCell.Position;
                //���� ����� ��������� �� ��������� ���� �����
                if (nextNeighbourCell.ColumnIndex < cell.ColumnIndex - 1)
                {
                    center3.x += MapGenerationData.wrapSize * MapGenerationData.innerDiameter;
                }
                //�����, ���� ����� ��������� �� �������� ���� �����
                else if(nextNeighbourCell.ColumnIndex > cell.ColumnIndex +1)
                {
                    center3.x -= MapGenerationData.wrapSize * MapGenerationData.innerDiameter;
                }

                //���������� ������� ������������
                Vector3 v3 
                    = center3 
                    + (nextNeighbourCell.IsUnderwater 
                    ? MapGenerationData.GetFirstWaterCorner(direction.Previous()) 
                    : MapGenerationData.GetFirstSolidCorner(direction.Previous()));
                v3.y = center.y;

                //������� ����������� � ���
                chunk.waterShore.AddTriangle(e1.v5, e2.v5, v3);
                chunk.waterShore.AddTriangleUV(new(0f, 0f), new(0f, 1f), new(0f, nextNeighbourCell.IsUnderwater ? 0f : 1f));

                //������� ������ �������
                indices.z = nextNeighbourCell.Index;
                chunk.waterShore.AddTriangleCellData(
                    indices,
                    MapGenerationData.weights1, MapGenerationData.weights2, MapGenerationData.weights3);
            }
        }

        void TriangulateEstuary(
            ref CHexChunk chunk,
            DHexEdgeVertices e1, DHexEdgeVertices e2,
            bool incomingRiver,
            Vector3 indices)
        {
            //������� ������������ � ���
            chunk.waterShore.AddTriangle(e2.v1, e1.v2, e1.v1);
            chunk.waterShore.AddTriangle(e2.v5, e1.v5, e1.v4);
            chunk.waterShore.AddTriangleUV(new(0f, 1f), new(0f, 0f), new(0f, 0f));
            chunk.waterShore.AddTriangleUV(new(0f, 1f), new(0f, 0f), new(0f, 0f));

            //������� ������ �������
            chunk.waterShore.AddTriangleCellData(
                indices,
                MapGenerationData.weights2, MapGenerationData.weights1, MapGenerationData.weights1);
            chunk.waterShore.AddTriangleCellData(
                indices,
                MapGenerationData.weights2, MapGenerationData.weights1, MapGenerationData.weights1);

            //������� ������� � ���
            chunk.estuaries.AddQuad(e2.v1, e1.v2, e2.v2, e1.v3);
            chunk.estuaries.AddTriangle(e1.v3, e2.v2, e2.v4);
            chunk.estuaries.AddQuad(e1.v3, e1.v4, e2.v4, e2.v5);

            chunk.estuaries.AddQuadUV(new Vector2(0f, 1f), new (0f, 0f), new (1f, 1f), new (0f, 0f));
            chunk.estuaries.AddTriangleUV(new (0f, 0f), new (1f, 1f), new (1f, 1f));
            chunk.estuaries.AddQuadUV(new Vector2(0f, 0f), new (0f, 0f), new (1f, 1f), new (0f, 1f));

            chunk.estuaries.AddQuadCellData(
                indices,
                MapGenerationData.weights2, MapGenerationData.weights1, MapGenerationData.weights2, MapGenerationData.weights1);
            chunk.estuaries.AddTriangleCellData(
                indices,
                MapGenerationData.weights1, MapGenerationData.weights2, MapGenerationData.weights2);
            chunk.estuaries.AddQuadCellData(
                indices,
                MapGenerationData.weights1, MapGenerationData.weights2);

            //���� ���� ��������
            if (incomingRiver)
            {
                chunk.estuaries.AddQuadUV2(new Vector2(1.5f, 1f), new (0.7f, 1.15f), new (1f, 0.8f), new (0.5f, 1.1f));
                chunk.estuaries.AddTriangleUV2(new (0.5f, 1.1f), new (1f, 0.8f), new (0f, 0.8f));
                chunk.estuaries.AddQuadUV2(new Vector2(0.5f, 1.1f), new (0.3f, 1.15f), new (0f, 0.8f), new (-0.5f, 1f));
            }
            //�����
            else
            {
                chunk.estuaries.AddQuadUV2(new Vector2(-0.5f, -0.2f), new (0.3f, -0.35f), new (0f, 0f), new (0.5f, -0.3f));
                chunk.estuaries.AddTriangleUV2(new (0.5f, -0.3f), new (0f, 0f), new (1f, 0f));
                chunk.estuaries.AddQuadUV2(new Vector2(0.5f, -0.3f), new (0.7f, -0.35f), new (1f, 0f), new (1.5f, -0.2f));
            }
        }

        Vector2 GetRoadInterpolators(
            ref CHexRegion cell,
            HexDirection direction)
        {
            //���������� ��������
            Vector2 interpolators;

            //���� ������ ����� ������ ����� ������ �����
            if (cell.HasRoadThroughEdge(direction))
            {
                //���������� ��������� �������
                interpolators.x = interpolators.y = 0.5f;
            }
            //�����
            else
            {
                //���������� ��������� �������
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

            //���� ���������� ������� �� ������� �����
            if (z < 0 || z >= mapGenerationData.Value.regionCountZ)
            {
                return new();
            }

            int x = coordinates.X + z / 2;

            //���� ���������� ������� �� ������� ������
            if (x < 0 || x >= mapGenerationData.Value.regionCountX)
            {
                return new();
            }

            return mapGenerationData.Value.regionPEs[x + z * mapGenerationData.Value.regionCountX];
        }


        void ChunkRefreshSelfRequest(
            ref CHexRegion region)
        {
            //���� ��������� ������������� ����� �������
            region.parentChunkPE.Unpack(world.Value, out int parentChunkEntity);
            ref CHexChunk parentChunk = ref chunkPool.Value.Get(parentChunkEntity);

            //���� ��� �� ���������� ����������� ���������� �����
            if (mapChunkRefreshSelfRequestPool.Value.Has(parentChunkEntity) == false)
            {
                //��������� �������� ���������� ���������� �����
                mapChunkRefreshSelfRequestPool.Value.Add(parentChunkEntity);
            }

            //��� ������� ������ �������
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //���� ����� ���������� 
                if (region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //���� ��������� ������
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //���� �������� ������������� ����� �������
                    neighbourRegion.parentChunkPE.Unpack(world.Value, out int neighbourParentChunkEntity);

                    //���� ��� �� ���������� ����������� ���������� �����
                    if (mapChunkRefreshSelfRequestPool.Value.Has(neighbourParentChunkEntity) == false)
                    {
                        //��������� �������� ���������� ���������� �����
                        mapChunkRefreshSelfRequestPool.Value.Add(neighbourParentChunkEntity);
                    }
                }
            }
        }
    }
}