
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Map;
using SandOcean.Diplomacy;

namespace SandOcean.AEO.RAEO
{
    public class SRAEOChangeOwner : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //������� �����
        //readonly EcsPoolInject<CMapObject> mapObjectPool = default;

        //�����
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //����������
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //���������������-������������� �������
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        //������� ���������������-������������� ��������
        readonly EcsFilterInject<Inc<SRORAEOAction, CExplorationORAEO>> oRAEOActionSelfRequestFilter = default;
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;

        readonly EcsPoolInject<SRRefreshRAEOObjectPanel> refreshRAEOObjectPanelSRPool = default;

        readonly EcsPoolInject<EEcORAEONewCreated> ecORAEONewCreatedEventPool = default;

        //������
        //readonly EcsCustomInject<Ether.MapData> mapData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ORAEO, ��� �������� ��������� ��������� ��������
            foreach (int oRAEOEntity in oRAEOActionSelfRequestFilter.Value)
            {
                //���� ��������� ������� � ExORAEO
                ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
                ref SRORAEOAction oRAEOActionSR = ref oRAEOActionSelfRequestPool.Value.Get(oRAEOEntity);

                //���� ��������� ������������� RAEO
                exORAEO.parentRAEOPE.Unpack(world.Value, out int rAEOEntity);
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                //���� ��������� �������������� RAEO
                if (oRAEOActionSR.actionType == ORAEOActionType.Colonization)
                {
                    //���� ��������� ������������ �����������
                    exORAEO.organizationPE.Unpack(world.Value, out int organizationEntity);
                    ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                    //������������ RAEO
                    RAEOColonize(
                        ref organization,
                        ref rAEO,
                        ref exORAEO);

                    //���� ��������� �������
                    ref CHexRegion region = ref regionPool.Value.Get(rAEOEntity);

                    //����������� ��������� �������
                    //region.IncreaseVisibility();
                }

                //���� �������� RAEO �� ����� ���������� ����������� ���������� ������ �������
                if (refreshRAEOObjectPanelSRPool.Value.Has(rAEOEntity) == false)
                {
                    //��������� �������� ��������� ����������� ���������� ������ �������
                    refreshRAEOObjectPanelSRPool.Value.Add(rAEOEntity);
                }

                //������� � �������� ORAEO ��������� ����������� 
                oRAEOActionSelfRequestPool.Value.Del(oRAEOEntity);
            }

            //��� ������� ������� ����� ���������
            /*foreach (int eventEntity in oRAEOActionSelfRequestFilter.Value)
            {
                //���� ��������� ������� ����� ���������
                ref SRORAEOAction rAEOChangeOwnerEvent = ref oRAEOActionSelfRequestPool.Value.Get(eventEntity);

                //���� ��������� RAEO
                rAEOChangeOwnerEvent.rAEOPE.Unpack(world.Value, out int rAEOEntity);
                ref CLocarAEO rAEO = ref locarAEOPool.Value.Get(rAEOEntity);

                //���� ��������� �����������
                rAEOChangeOwnerEvent.organizationPE.Unpack(world.Value, out int organizationEntity);
                ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                //���� ������� ����������� ����������� RAEO
                if (rAEOChangeOwnerEvent.actionType == ORAEOActionType.Colonization)
                {
                    //������������ RAEO
                    RAEOColonize(
                        ref rAEO,
                        ref organization);
                }

                //���� �������� RAEO �� ����� ���������� ����������� ���������� ������ �������
                if (refreshRAEOObjectPanelSRPool.Value.Has(rAEOEntity) == false)
                {
                    //��������� �������� ��������� ����������� ���������� ������ �������
                    refreshRAEOObjectPanelSRPool.Value.Add(rAEOEntity);
                }

                world.Value.DelEntity(eventEntity);
            }*/
        }

        void RAEOColonize(
            ref COrganization organization,
            ref CRegionAEO rAEO,
            ref CExplorationORAEO exORAEO)
        {
            //��������� �����������-��������� RAEO
            rAEO.ownerOrganizationPE = organization.selfPE;
            rAEO.ownerOrganizationIndex = organization.selfIndex;

            //���������, ��� ORAEO ����� ������������� ���������
            rAEO.organizationRAEOs[organization.selfIndex].organizationRAEOType = ORAEOType.Economic;

            //���� �������� ORAEO � ��������� �� ��������� EcORAEO
            rAEO.organizationRAEOs[organization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
            ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Add(oRAEOEntity);

            //��������� �������� ������ EcORAEO
            ecORAEO = new(world.Value.PackEntity(oRAEOEntity));

            //������� PE ORAEO � ������ �����������
            organization.ownedORAEOPEs.Add(ecORAEO.selfPE);

            //������ �������, ���������� � �������� ������ EcORAEO
            EcORAEONewCreatedEvent(
                ref organization,
                ref ecORAEO);
        }

        void EcORAEONewCreatedEvent(
            ref COrganization organization,
            ref CEconomicORAEO ecORAEO)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������ EcORAEO
            int eventEntity = world.Value.NewEntity();
            ref EEcORAEONewCreated ecORAEONewCreatedEvent = ref ecORAEONewCreatedEventPool.Value.Add(eventEntity);

            //��������� ������ �������
            ecORAEONewCreatedEvent = new(
                organization.selfPE,
                ecORAEO.selfPE);
        }
    }
}