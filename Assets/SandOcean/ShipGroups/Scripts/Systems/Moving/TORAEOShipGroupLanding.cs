
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
            //Для каждого ORAEO в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём компонент ORAEO
                int oRAEOEntity = oRAEOEntities[a];
                ref CEconomicORAEO ecORAEO = ref economicORAEOPool[economicORAEOIndices[oRAEOEntity]];

                //Для каждой садящейся группы кораблей в обратном порядке
                for (int b = ecORAEO.landingShipGroups.Count - 1; b >= 0; b--)
                {
                    //Берём компонент группы кораблей
                    ecORAEO.landingShipGroups[b].Unpack(world, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[shipGroupEntity]];

                    //Указываем, что группа кораблей находится на текущем EcORAEO
                    shipGroup.dockingMode = ShipGroupDockingMode.EconomicORAEO;
                    shipGroup.dockingPE = ecORAEO.selfPE;

                    //Заносим группу кораблей в список групп, находящихся на EcORAEO
                    ecORAEO.landedShipGroups.AddLast(shipGroup.selfPE);

                    //Удаляем группу кораблей из списка садящихся
                    ecORAEO.landingShipGroups.RemoveAt(b);
                }
            }
        }
    }
}