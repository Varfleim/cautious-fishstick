
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Organization;

namespace SandOcean.Technology
{
    public class STechnologyGameCalculation : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //����������
        readonly EcsPoolInject<COrganization> organizationPool = default;


        //������� ����������
        readonly EcsFilterInject<Inc<RTechnologyCalculateModifiers>> technologyCalculateModifiersEventFilter = default;
        readonly EcsPoolInject<RTechnologyCalculateModifiers> technologyCalculateModifiersEventPool = default;


        //������
        readonly EcsCustomInject<ContentData> contentData = default;


        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ��������� ������������� ���������� �����������
            foreach (int technologyCalculateModifiersEventEntity in technologyCalculateModifiersEventFilter.Value)
            {
                //���� ��������� �������
                ref RTechnologyCalculateModifiers technologyCalculateModifiersEvent = ref technologyCalculateModifiersEventPool.Value.Get(technologyCalculateModifiersEventEntity);

                //���� ��������� �����������
                technologyCalculateModifiersEvent.organizationPE.Unpack(world.Value, out int organizationEntity);
                ref COrganization organization= ref organizationPool.Value.Get(organizationEntity);

                //����������� �������� ������������� ���������� ��� ��������� �����������
                TechnologyOrganizationCalculation(ref organization);

                world.Value.DelEntity(technologyCalculateModifiersEventEntity);
            }
        }

        void TechnologyOrganizationCalculation(
            ref COrganization organization)
        {
            //��� ������� ������ �������� � ������� ���������� �����������
            for (int a = 0; a < organization.technologies.Length; a++)
            {
                //��� ������ ���������� �����������
                foreach (KeyValuePair<int, DOrganizationTechnology> kVPTechnology in organization.technologies[a])
                {
                    //���� ���������� ��������� �����������
                    if (kVPTechnology.Value.isResearched == true)
                    {
                        //���� ������ �� ������ ����������
                        ref readonly DTechnology technology= ref contentData.Value.contentSets[a].technologies[kVPTechnology.Key];

                        //��� ������� ������������ ����������
                        for (int b = 0; b < technology.technologyModifiers.Length; b++)
                        {
                            //���������� ��� ������������ � ��������������� ��������
                            TechnologyFunctions.TechnologyModifierTypeCalculation(
                                technology.technologyModifiers[b],
                                ref organization.technologyModifiers);
                        }
                    }
                }
            }
        }
    }
}