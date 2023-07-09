
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.AEO.RAEO;

namespace SandOcean.Ship.Moving
{
    public struct TRAEOShipGroupLanding : IEcsThread<
        CRegionAEO,
        CShipGroup>
    {
        public EcsWorld world;

        int[] regionAEOEntities;

        CRegionAEO[] regionAEOPool;
        int[] regionAEOIndices;

        CShipGroup[] shipGroupPool;
        int[] shipGroupIndices;

        public void Init(
            int[] entities,
            CRegionAEO[] pool1, int[] indices1,
            CShipGroup[] pool2, int[] indices2)
        {
            regionAEOEntities = entities;

            regionAEOPool = pool1;
            regionAEOIndices = indices1;

            shipGroupPool = pool2;
            shipGroupIndices = indices2;
        }

        public void Execute(int fromIndex, int beforeIndex)
        {
            //��� ������� RAEO � ������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ��������� RAEO
                int lAEOEntity = regionAEOEntities[a];
                ref CRegionAEO rAEO = ref regionAEOPool[regionAEOIndices[lAEOEntity]];

                //��� ������ ��������� ������ �������� � �������� �������
                for (int b = rAEO.landingShipGroups.Count - 1; b >= 0; b--)
                {
                    //���� ��������� ������ ��������
                    rAEO.landingShipGroups[b].Unpack(world, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[shipGroupEntity]];

                    //���������, ��� ������ �������� ��������� �� ������� RAEO
                    shipGroup.dockingMode = ShipGroupDockingMode.RAEO;
                    shipGroup.dockingPE = rAEO.selfPE;

                    //������� ������ �������� � ������ �����, ����������� �� RAEO
                    rAEO.landedShipGroups.AddLast(shipGroup.selfPE);

                    //������� ������ �������� �� ������ ���������
                    rAEO.landingShipGroups.RemoveAt(b);
                }
            }
        }
    }
}