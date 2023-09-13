
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Map.Events;
using SandOcean.Organization;
using SandOcean.AEO.RAEO;

namespace SandOcean.Map
{
    public class SMapInitializerControl : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //�����
        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //readonly EcsPoolInject<CHexRegionGenerationData> regionGenerationDataPool = default;

        //����������
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //���������������-������������� �������
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;


        //������� �����
        readonly EcsFilterInject<Inc<RRegionInitializer>> regionInitializerFilter = default;
        readonly EcsPoolInject<RRegionInitializer> regionInitializerPool = default;

        //������� ���������������-������������� ��������
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;


        //������
        //readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;

        public void Run(IEcsSystems systems)
        {
            //��������� �������������� �����������
            MapRegionOrganizationInitializers();

            //���������� ������ ��������������


            //��������� ������ ��������������
            MapRegionMiscellaneousInitializers();
        }

        void MapRegionOrganizationInitializers()
        {
            //��� ������� �������������� �������
            /*foreach (int regionInitializerEntity in regionInitializerFilter.Value)
            {
                //���� �������������
                ref RRegionInitializer regionInitializer = ref regionInitializerPool.Value.Get(regionInitializerEntity);

                //�������� �� �������� ���������������, ������ ������� � ������� �� � ������ �������� ������ ����� �������,
                //� ����� �������� � �������� PE �������������.
                //����� ������� ��� � ��������� �������

                //����� �������� �� �������� � ���������
            }*/

            //������ ��������� ������� ��� ��������
            //int1 - �������� �������, int2 - �������� ��������������
            Dictionary<int, int> initializedRegions = new();

            //������ ��������� ������ ��� �������� ���������������, ���������� ������� ���������
            List<int> currentRegions = new();

            //������� ������� ��������
            initializedRegions.Clear();

            //��� ������� �������������� �������
            foreach (int regionInitializerEntity in regionInitializerFilter.Value)
            {
                //���� �������������
                ref RRegionInitializer newRegionInitializer = ref regionInitializerPool.Value.Get(regionInitializerEntity);

                //���� �������� ������������� ������� - -1, �� ��� ��������� �������������
                if (newRegionInitializer.parentInitializerEntity == -1)
                {
                    //������� ������ ������� ��������
                    currentRegions.Clear();

                    //������������ ���������� ������� ��� �������������� � ��� �������� ���������������
                    InitializerPositionGeneration(
                        initializedRegions,
                        currentRegions,
                        regionInitializerEntity);
                }
            }

            //��� ������� �������������� ������� � �������
            foreach (KeyValuePair<int, int> kVPRegion in initializedRegions)
            {
                //���� ������ � �������������
                ref CHexRegion region = ref regionPool.Value.Get(kVPRegion.Key);
                ref RRegionInitializer regionInitializer = ref regionInitializerPool.Value.Get(kVPRegion.Value);

                //��������� ������������� � �������
                RegionInitialization(ref region, ref regionInitializer);
            }

            //��� ������� �������������� �������
            foreach (int regionInitializerEntity in regionInitializerFilter.Value)
            {
                world.Value.DelEntity(regionInitializerEntity);
            }
        }

        void MapRegionMiscellaneousInitializers()
        {

        }

        bool InitializerPositionGeneration(
            Dictionary<int, int> initializedRegions,
            List<int> siblingRegions,
            int regionInitializerEntity)
        {
            //���� �������������
            ref RRegionInitializer regionInitializer = ref regionInitializerPool.Value.Get(regionInitializerEntity);

            bool isValidRegion = false;

            //���� �� ������ ����� ������
            while (isValidRegion == false)
            {
                //���� �������� ���������� �������
                int regionEntity;

                //��� ������ ���� �������, ������� ����������, ����� ��������� ������� �������� ������:
                //1. ��� ��������� ��� ������ ��������� �������;
                //2. ��� ��������� ��� ����� ������� � ������� �� �������������
                if (true)
                {
                    //���� �������� ���������� �������
                    RegionsData.GetRegionRandom().Unpack(world.Value, out regionEntity);
                }

                //���� �������� ������� ����������� � �������
                if (initializedRegions.ContainsKey(regionEntity) == false)
                {
                    //���� ������ �������
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    bool isValidConditions = true;

                    //��� ������� ������� ��������������
                    //for ()
                    //{
                    //��� ������� �������, ��� ��������������� � �������
                    foreach (KeyValuePair<int, int> kVPRegion in initializedRegions)
                    {
                        //���� ������������ ������ � ��� �������������
                        ref CHexRegion oldRegion = ref regionPool.Value.Get(kVPRegion.Key);
                        ref RRegionInitializer oldRegionInitializer = ref regionInitializerPool.Value.Get(kVPRegion.Value);

                        //������� ���� ����� ���������
                        List<int> path = RegionsData.FindPath(
                            world.Value,
                            regionFilter.Value, regionPool.Value,
                            ref region, ref oldRegion,
                            regionInitializer.minDistanceBetweenInitializers);

                        //���� ���� ����������, �� ���������� �� ������� ������ �����������
                        if (path != null)
                        {
                            UnityEngine.Debug.LogWarning(path.Count);

                            //���������, ��� ���������� ���������������
                            isValidConditions = false;

                            break;
                        }
                    }
                    //}

                    //���� ������� �������������
                    if (isValidConditions == true)
                    {
                        //������ �������� ����, ���� �� �������� ��������������

                        //������� �������� ������� � �������������� � �������
                        initializedRegions.Add(regionEntity, regionInitializerEntity);

                        //������ ��������� ������ ��� �������� �������� ���������������
                        List<int> childrenInitializerRegions = ListPool<int>.Get();

                        //��� ������� ��������� ��������������
                        /*for ()
                        {
                            //���� �������� ��������������
                            int childerInitializerEntity = -1;

                            //���������� ��������� ��� �� �������
                            isValidConditions = InitializerPositionGeneration(
                                initializedRegions,
                                childrenInitializerRegions,
                                childerInitializerEntity);

                            //���� ������� �����
                            if (isValidConditions == false)
                            {
                                break;
                            }
                        }*/

                        //���� ������� �������������
                        if (isValidConditions == true)
                        {
                            //������� �������� ������� � ������ ����������� ��������
                            siblingRegions.Add(regionEntity);

                            //���������� � ������ ����������� �������� ������ ��������
                            siblingRegions.AddRange(childrenInitializerRegions);

                            //��������, ��� ����� ������ ��� ������
                            isValidRegion = true;
                        }
                        //�����
                        else
                        {
                            //������� ������ � ������� ������� �� �������
                            initializedRegions.Remove(regionEntity);

                            //������� �� ������� ������� �������� ���������������
                            for (int a = 0; a < childrenInitializerRegions.Count; a++)
                            {
                                initializedRegions.Remove(childrenInitializerRegions[a]);
                            }
                        }

                        //���������� ������ � ���
                        ListPool<int>.Add(childrenInitializerRegions);
                    }
                }
            }

            return isValidRegion;
        }

        void RegionInitialization(
            ref CHexRegion region,
            ref RRegionInitializer regionInitializer)
        {
            //���� RAEO
            region.selfPE.Unpack(world.Value, out int regionEntity);
            ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

            //����
            //��� ������� �������
            for (int a = 0; a < regionInitializer.effectLinks.Length; a++)
            {
                //���� ������
                //���� - ������ �� ������
                ref readonly DContentObjectLink effect = ref regionInitializer.effectLinks[a];

                //���� ���������������� �����������
                regionInitializer.initializedOrganizationPE.Unpack(world.Value, out int organizationEntity);
                ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                //���� ExORAEO ������ �����������
                rAEO.organizationRAEOs[organization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
                ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);

                //��������� �������� ORAEO ���������� ��������
                ref SRORAEOAction oRAEOActionSR = ref oRAEOActionSelfRequestPool.Value.Add(oRAEOEntity);

                //��������� ������ �����������
                oRAEOActionSR = new SRORAEOAction(ORAEOActionType.Colonization);
            }
            //����
        }
    }
}