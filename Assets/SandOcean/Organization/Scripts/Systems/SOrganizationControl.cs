
using System;
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Player;
using SandOcean.Map.Events;
using SandOcean.Technology;
using SandOcean.AEO.RAEO;
using SandOcean.Warfare.Fleet;

namespace SandOcean.Organization
{
    public class SOrganizationControl : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //������
        readonly EcsPoolInject<CPlayer> playerPool = default;

        //����������
        readonly EcsFilterInject<Inc<COrganization>> organizationFilter = default;
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //���������������-������������� �������
        readonly EcsFilterInject<Inc<CRegionAEO>> regionAEOFilter = default;
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;


        //������� �����
        readonly EcsPoolInject<RRegionInitializer> regionInitializerPool = default;

        //������� �����������
        readonly EcsFilterInject<Inc<ROrganizationCreating>> organizationCreatingRequestFilter = default;
        readonly EcsPoolInject<ROrganizationCreating> organizationCreatingRequestPool = default;

        //������� ����������
        readonly EcsPoolInject<RTechnologyCalculateModifiers> technologyCalculateModifiersRequestPool = default;

        //������� ���������������-������������� ��������
        readonly EcsFilterInject<Inc<SRORAEOCreate, COrganization>> oRAEOCreateSRFilter = default;
        readonly EcsPoolInject<SRORAEOCreate> oRAEOCreateSRPool = default;

        //������� ������
        readonly EcsPoolInject<RFleetCreating> fleetCreatingRequestPool = default;

        //����� �������
        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;


        //������
        readonly EcsCustomInject<ContentData> contentData = default;
        readonly EcsCustomInject<InputData> inputData = default;

        readonly EcsCustomInject<RuntimeData> runtimeData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� �����������
            foreach (int organizationCreatingRequestEntity in organizationCreatingRequestFilter.Value)
            {
                //���� ��������� �������
                ref ROrganizationCreating organizationCreatingRequest = ref organizationCreatingRequestPool.Value.Get(organizationCreatingRequestEntity);

                //������ ����� �����������
                OrganizationCreating(ref organizationCreatingRequest);

                world.Value.DelEntity(organizationCreatingRequestEntity);
            }

            //���� ������ ������������ �������� ORAEO �� ����
            if (oRAEOCreateSRFilter.Value.GetEntitiesCount() > 0)
            {
                //������ ORAEO
                ORAEOCreating();
            }
        }

        void OrganizationCreating(
            ref ROrganizationCreating organizationCreatingRequest)
        {
            //������ ����� �������� � ��������� �� ��������� �����������
            int organizationEntity = world.Value.NewEntity();
            ref COrganization organization = ref organizationPool.Value.Add(organizationEntity);

            //��������� �������� ������ �����������
            organization = new(
                world.Value.PackEntity(organizationEntity), runtimeData.Value.organizationsCount, organizationCreatingRequest.organizationName);

            //��������� ������� �����������
            runtimeData.Value.organizationsCount++;

            //���� ����������� ����������� ������
            if (organizationCreatingRequest.isPlayerOrganization == true)
            {
                //���� ��������� ������
                organizationCreatingRequest.ownerPlayerPE.Unpack(world.Value, out int playerEntity);
                ref CPlayer player = ref playerPool.Value.Get(playerEntity);

                //������� PE ������������� �����������
                player.ownedOrganizationPE = organization.selfPE;

                //������� PE ������-���������
                organization.ownerPlayerPE = player.selfPE;

                //������� PE ����������� ������ � ������ �����
                inputData.Value.playerOrganizationPE = organization.selfPE;
            }

            //��������� ����������� � ���������� (������� � ����������� � �������)
            OrganizationAssignTechnologies(ref organization);

            //������������� ������������ ���������� �����������
            TechnologyCalculateOrganizationModifiersRequest(ref organization);

            //������ ����� �������� ��� ������ �����������
            OrganizationContentSetCreating(ref organization);

            //��������� ����������� ��������� ����������� �������� ORAEO
            ref SRORAEOCreate oRAEOCreateRequest = ref oRAEOCreateSRPool.Value.Add(organizationEntity);

            //������ ����� �������� � ��������� �� ��������� ������� �������������� �������
            int regionInitializerRequestEntity = world.Value.NewEntity();
            ref RRegionInitializer regionInitializerRequest = ref regionInitializerPool.Value.Add(regionInitializerRequestEntity);

            //��������� �������� ������ �������
            regionInitializerRequest = new(
                organization.selfPE, 
                new DContentObjectLink[1], 
                10);

            //����������� �������� ���������� ����� �����������
            FleetCreatingRequest(
                ref organization,
                true);

            //������ �������, ���������� � �������� ����� �����������
            ObjectNewCreatedEvent(organization.selfPE, ObjectNewCreatedType.Organization);
        }

        void OrganizationAssignTechnologies(
            ref COrganization organization)
        {
            //���� ����� �������� ������� ��������
            int contentSetDescriptionsNumber
                = contentData.Value.contentSetDescriptions.Length;

            //������ � ������ ����������� ������ �������� ��� ����������
            organization.technologies
                = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

            //��� ������� ������ �������� ������ ������� �����
            for (int a = 0; a < contentSetDescriptionsNumber; a++)
            {
                //������ �������
                organization.technologies[a]
                    = new Dictionary<int, DOrganizationTechnology>();

                //��� ������ ���������� � ������ ��������
                for (int b = 0; b < contentData.Value.contentSets[a].technologies.Length; b++)
                {
                    //���� ���������� �������� �������
                    if (contentData.Value.contentSets[a].technologies[b].IsBaseTechnology
                        == true)
                    {
                        //������� � � ������ �������� ����������
                        organization.technologies[a].Add(
                            b,
                            new DOrganizationTechnology(
                                true));
                    }
                }
            }
        }

        void TechnologyCalculateOrganizationModifiersRequest(
            ref COrganization organization)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������������� ����������
            int requestEntity = world.Value.NewEntity();
            ref RTechnologyCalculateModifiers technologyCalculateModifiersRequest
                = ref technologyCalculateModifiersRequestPool.Value.Add(requestEntity);

            //��������� PE �����������, ��� ������� ��������� ����������� ������������
            technologyCalculateModifiersRequest.organizationPE
                = organization.selfPE;
        }

        void OrganizationContentSetCreating(
            ref COrganization organization)
        {
            //��������� ����� ������� ������� ��������
            int contentSetsArrayLength = contentData.Value.contentSets.Length;

            //��������� � ������ ����������� ������ � ������ ��������
            organization.contentSetIndex = contentData.Value.contentSets.Length;

            //������ ����� ����� �������� ��� ������ �����������
            DContentSet organizationContentSet = new(organization.selfName);

            //�������� ������ ������� ������� ��������
            Array.Resize(
                ref contentData.Value.contentSets,
                contentData.Value.contentSets.Length + 1);

            //������� ����� ����� �������� � ������
            contentData.Value.contentSets[contentData.Value.contentSets.Length - 1] = organizationContentSet;

            //�������� ������ ������� �������� ������� ��������
            Array.Resize(
                ref contentData.Value.contentSetNames,
                contentData.Value.contentSetNames.Length + 1);

            //������� ����� �������� � ������
            contentData.Value.contentSetNames[contentData.Value.contentSetNames.Length - 1] = organization.selfName;


            //�������� ����� ������� PE �����������
            Array.Resize(
                ref OrganizationsData.organizationPEs,
                OrganizationsData.organizationPEs.Length + 1);

            //������� PE ����������� � ������
            OrganizationsData.organizationPEs[organization.selfIndex] = organization.selfPE;

            //�������� ����� ������� ������ �����������
            Array.Resize(
                ref OrganizationsData.organizationData,
                OrganizationsData.organizationData.Length + 1);

            //������ ����� ������ �����������
            DOrganization organizationData = new(0);

            //������� ����� ������ ����������� � ������
            OrganizationsData.organizationData[organization.selfIndex] = organizationData;
        }

        void ORAEOCreating()
        {
            //������ ��������� ������ DORAEO
            List<DOrganizationRAEO> tempDORAEO = new();

            //���������� ���������� �����������
            int organizationsCount = organizationFilter.Value.GetEntitiesCount();

            //��� ������� RAEO
            foreach (int rAEOEntity in regionAEOFilter.Value)
            {
                //���� ��������� RAEO
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                //������� ��������� ������
                tempDORAEO.Clear();

                //��� ������� ����������� �������� ORAEO
                foreach (int oRAEOCreateSelfRequestEntity in oRAEOCreateSRFilter.Value)
                {
                    //���� ��������� ����������� � ��������� �����������
                    ref COrganization organization = ref organizationPool.Value.Get(oRAEOCreateSelfRequestEntity);

                    //������ ����� �������� � ��������� �� ��������� ExplorationORAEO
                    int oRAEOEntity = world.Value.NewEntity();
                    ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Add(oRAEOEntity);

                    //��������� �������� ������ ExORAEO
                    exORAEO = new(
                        world.Value.PackEntity(oRAEOEntity),
                        organization.selfPE,
                        rAEO.selfPE,
                        0);

                    //������ ��������� ��� �������� ������ ����������� ��������������� � RAEO
                    DOrganizationRAEO organizationRAEOData = new(
                        exORAEO.selfPE,
                        ORAEOType.Exploration);

                    //������� � �� ��������� ������
                    tempDORAEO.Add(organizationRAEOData);
                }

                //��������� ������ ������ �������
                int oldArraySize = rAEO.organizationRAEOs.Length;

                //��������� ������ DORAEO
                Array.Resize(
                    ref rAEO.organizationRAEOs,
                    organizationsCount);

                //��� ������� DORAEO �� ��������� �������
                for (int a = 0; a < tempDORAEO.Count; a++)
                {
                    //��������� DORAEO � ������ �� �������
                    rAEO.organizationRAEOs[oldArraySize++] = tempDORAEO[a];
                }

                UnityEngine.Debug.LogWarning(rAEO.organizationRAEOs.Length);
            }

            //��� ������� ����������� �������� ORAEO
            foreach (int oRAEOCreateSelfRequestEntity in oRAEOCreateSRFilter.Value)
            {
                //������� � �������� ����������� ��������� �����������
                oRAEOCreateSRPool.Value.Del(oRAEOCreateSelfRequestEntity);
            }
        }

        void FleetCreatingRequest(
            ref COrganization organization,
            bool isReserve = false)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� �����
            int requestEntity = world.Value.NewEntity();
            ref RFleetCreating requestComp = ref fleetCreatingRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
            requestComp = new(
                organization.selfPE,
                isReserve);
        }

        void ObjectNewCreatedEvent(
            EcsPackedEntity objectPE, ObjectNewCreatedType objectNewCreatedType)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������ �������
            int eventEntity = world.Value.NewEntity();
            ref EObjectNewCreated eventComp = ref objectNewCreatedEventPool.Value.Add(eventEntity);

            //��������� ������ �������
            eventComp = new(objectPE, objectNewCreatedType);
        }
    }
}