using System;
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Player;
using SandOcean.Technology;
using SandOcean.AEO.RAEO;

namespace SandOcean.Diplomacy
{
    public class SOrganizationInitialization : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //������
        readonly EcsPoolInject<CPlayer> playerPool = default;

        //����������
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //������� ����������
        readonly EcsFilterInject<Inc<EOrganizationCreating>> organizationCreatingEventFilter = default;
        readonly EcsPoolInject<EOrganizationCreating> organizationCreatingEventPool = default;

        //������� ����������
        readonly EcsPoolInject<ETechnologyCalculateModifiers> technologyCalculateModifiersEventPool = default;

        //������� ���������������-������������� ��������
        readonly EcsPoolInject<SRORAEOCreate> oRAEOCreateSRPool = default;

        //������
        readonly EcsCustomInject<ContentData> contentData = default;
        readonly EcsCustomInject<InputData> inputData = default;

        readonly EcsCustomInject<RuntimeData> runtimeData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� �����������
            foreach (int organizationCreatingEventEntity in organizationCreatingEventFilter.Value)
            {
                //���� ��������� ������� �������� �����������
                ref EOrganizationCreating organizationCreatingEvent = ref organizationCreatingEventPool.Value.Get(organizationCreatingEventEntity);

                //������ ����� �����������
                OrganizationCreating(ref organizationCreatingEvent);

                world.Value.DelEntity(organizationCreatingEventEntity);
            }
        }

        void OrganizationCreating(
            ref EOrganizationCreating organizationCreatingEvent)
        {
            //������ ����� �������� � ��������� �� ��������� �����������
            int organizationEntity = world.Value.NewEntity();
            ref COrganization organization = ref organizationPool.Value.Add(organizationEntity);

            //��������� �������� ������ �����������
            organization = new(
                world.Value.PackEntity(organizationEntity),
                runtimeData.Value.organizationsCount,
                organizationCreatingEvent.organizationName);

            //��������� ������� �����������
            runtimeData.Value.organizationsCount++;

            //���� ����������� ����������� ������
            if (organizationCreatingEvent.isPlayerOrganization == true)
            {
                //���� ��������� ������
                organizationCreatingEvent.ownerPlayerPE.Unpack(world.Value, out int playerEntity);
                ref CPlayer player = ref playerPool.Value.Get(playerEntity);

                //������� PE ������������� �����������
                player.ownedOrganizationPE = organization.selfPE;

                //������� PE ������-���������
                organization.ownerPlayerPE = player.selfPE;

                //������� PE ������������� ����������� � ������ �����
                inputData.Value.playerOrganizationPE = organization.selfPE;
            }

            //��������� ����������� � ���������� (������� � ����������� � �������)
            OrganizationAssignTechnologies(ref organization);

            //������������� ������������ ���������� �����������
            TechnologyCalculateOrganizationModifiersEvent(ref organization);

            //������ ����� �������� ��� ������ �����������
            OrganizationContentSetCreating(ref organization);

            //��������� ����������� ��������� ����������� �������� ORAEO
            ref SRORAEOCreate oRAEOCreateEvent = ref oRAEOCreateSRPool.Value.Add(organizationEntity);
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

        void TechnologyCalculateOrganizationModifiersEvent(
            ref COrganization organization)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������������� ����������
            int eventEntity = world.Value.NewEntity();
            ref ETechnologyCalculateModifiers technologyCalculateModifiersEvent
                = ref technologyCalculateModifiersEventPool.Value.Add(eventEntity);

            //��������� PE �����������, ��� ������� ��������� ����������� ������������
            technologyCalculateModifiersEvent.organizationPE
                = organization.selfPE;
        }

        void OrganizationContentSetCreating(
            ref COrganization organization)
        {
            //��������� ����� ������� ������� ��������
            int contentSetsArrayLength
                = contentData.Value.contentSets.Length;

            //��������� � ������ ����������� ������ � ������ ��������
            organization.contentSetIndex
                = contentData.Value.contentSets.Length;

            UnityEngine.Debug.LogWarning(organization.contentSetIndex);

            //������ ����� ����� �������� ��� ������ �����������
            DContentSet organizationContentSet 
                = new(
                    organization.selfName);

            //�������� ������ ������� ������� ��������
            Array.Resize(
                ref contentData.Value.contentSets,
                contentData.Value.contentSets.Length
                + 1);

            //������� ����� ����� �������� � ������
            contentData.Value.contentSets[contentData.Value.contentSets.Length - 1]
                = organizationContentSet;

            //�������� ������ ������� �������� ������� ��������
            Array.Resize(
                ref contentData.Value.contentSetNames,
                contentData.Value.contentSetNames.Length
                + 1);

            //������� ����� �������� � ������
            contentData.Value.contentSetNames[contentData.Value.contentSetNames.Length - 1]
                = organization.selfName;
        }
    }
}