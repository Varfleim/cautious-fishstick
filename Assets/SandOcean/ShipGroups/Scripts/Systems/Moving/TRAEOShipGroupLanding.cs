
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
            //Для каждого RAEO в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём компонент RAEO
                int lAEOEntity = regionAEOEntities[a];
                ref CRegionAEO rAEO = ref regionAEOPool[regionAEOIndices[lAEOEntity]];

                //Для каждой садящейся группы кораблей в обратном порядке
                for (int b = rAEO.landingShipGroups.Count - 1; b >= 0; b--)
                {
                    //Берём компонент группы кораблей
                    rAEO.landingShipGroups[b].Unpack(world, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[shipGroupEntity]];

                    //Указываем, что группа кораблей находится на текущем RAEO
                    shipGroup.dockingMode = ShipGroupDockingMode.RAEO;
                    shipGroup.dockingPE = rAEO.selfPE;

                    //Заносим группу кораблей в список групп, находящихся на RAEO
                    rAEO.landedShipGroups.AddLast(shipGroup.selfPE);

                    //Удаляем группу кораблей из списка садящихся
                    rAEO.landingShipGroups.RemoveAt(b);
                }
            }
        }
    }
}