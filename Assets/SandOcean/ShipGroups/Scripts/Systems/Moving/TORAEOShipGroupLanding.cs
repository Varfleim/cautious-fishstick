
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.AEO.RAEO;

namespace SandOcean.Ship.Moving
{
    public struct TORAEOShipGroupLanding : IEcsThread<
        CEconomicORAEO,
        CShipGroup>
    {
        public EcsWorld world;

        int[] oRAEOEntities;

        CEconomicORAEO[] economicORAEOPool;
        int[] economicORAEOIndices;

        CShipGroup[] shipGroupPool;
        int[] shipGroupIndices;

        public void Init(
            int[] entities,
            CEconomicORAEO[] pool1, int[] indices1,
            CShipGroup[] pool2, int[] indices2)
        {
            oRAEOEntities = entities;

            economicORAEOPool = pool1;
            economicORAEOIndices = indices1;

            shipGroupPool = pool2;
            shipGroupIndices = indices2;
        }

        public void Execute(int fromIndex, int beforeIndex)
        {
            //��� ������� ORAEO � ������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ��������� ORAEO
                int oRAEOEntity = oRAEOEntities[a];
                ref CEconomicORAEO ecORAEO = ref economicORAEOPool[economicORAEOIndices[oRAEOEntity]];

                //��� ������ ��������� ������ �������� � �������� �������
                for (int b = ecORAEO.landingShipGroups.Count - 1; b >= 0; b--)
                {
                    //���� ��������� ������ ��������
                    ecORAEO.landingShipGroups[b].Unpack(world, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[shipGroupEntity]];

                    //���������, ��� ������ �������� ��������� �� ������� EcORAEO
                    shipGroup.dockingMode = ShipGroupDockingMode.EconomicORAEO;
                    shipGroup.dockingPE = ecORAEO.selfPE;

                    //������� ������ �������� � ������ �����, ����������� �� EcORAEO
                    ecORAEO.landedShipGroups.AddLast(shipGroup.selfPE);

                    //������� ������ �������� �� ������ ���������
                    ecORAEO.landingShipGroups.RemoveAt(b);
                }
            }
        }
    }
}