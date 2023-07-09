
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Map;

namespace SandOcean.Ship.Moving
{
    public class SRegionShipGroupOwnershipChange : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //������� �����
        readonly EcsPoolInject<CMapObject> mapObjectPool = default;

        //�����
        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //readonly EcsFilterInject<Inc<CRegion>> regionFilter = default;
        //readonly EcsPoolInject<CRegion> regionPool = default;

        //������ ��������
        readonly EcsPoolInject<CShipGroup> shipGroupPool = default;


        public void Run(IEcsSystems systems)
        {
            //��� ������� �������
            foreach (int regionEntity
                in regionFilter.Value)
            {
                //���� ��������� �������
                ref CHexRegion region
                    = ref regionPool.Value.Get(regionEntity);

                //��� ������ ������ �������� � ������ �������� ������
                for (int a = 0; a < region.ownershipChangeShipGroups.Count; a++)
                {
                    //���� ��������� ������ ��������
                    region.ownershipChangeShipGroups[a].Unpack(world.Value, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool.Value.Get(shipGroupEntity);
                    ref CMapObject shipGroupMO = ref mapObjectPool.Value.Get(shipGroupEntity);

                    //���� ��������� �������, � ������� ��������� ������ ��������
                    shipGroup.parentRegionPE.Unpack(world.Value, out int newRegionEntity);
                    ref CHexRegion newRegion = ref regionPool.Value.Get(newRegionEntity);

                    //������� ������ �������� � ������ ����� � �������
                    newRegion.shipGroups.AddLast(shipGroup.selfPE);

                    //����������� GO ������ �������� � GO �������
                    //shipGroupEO.Transform.SetParent(newRegion.transform, true);
                }
                //������� ������ 
                region.ownershipChangeShipGroups.Clear();
            }
        }
    }
}