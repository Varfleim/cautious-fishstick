using Leopotam.EcsLite;

using SandOcean.UI.Events;

namespace SandOcean.Economy.Building
{
    public static class BuildingFunctions
    {
        public static void RefreshBuildingUISelfRequest(
            EcsWorld world,
            EcsPool<CBuildingDisplayedSummaryPanel> buildingDisplayedSummaryPanelPool, EcsPool<SRObjectRefreshUI> refreshUISelfRequestPool,
            ref CBuilding building,
            RefreshUIType requestType = RefreshUIType.Refresh)
        {
            //���� �������� ����������
            building.selfPE.Unpack(world, out int buildingEntity);

            //���� ���������� ����� ������������ �������� ������
            if (buildingDisplayedSummaryPanelPool.Has(buildingEntity) == true)
            {
                //���� ���������� �� ����� ����������� ���������� ����������
                if (refreshUISelfRequestPool.Has(buildingEntity))
                {
                    //�� ��������� �� ���������� ���������� ����������
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(buildingEntity);

                    //��������� ������ �����������
                    selfRequestComp = new(requestType);
                }
                //�����
                else
                {
                    //���� ���������� ���������� ����������
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(buildingEntity);

                    //���� ������������� �� ��������
                    if (selfRequestComp.requestType != RefreshUIType.Delete)
                    {
                        //�� ��������� ����������
                        selfRequestComp.requestType = requestType;
                    }
                }
            }
        }
    }
}