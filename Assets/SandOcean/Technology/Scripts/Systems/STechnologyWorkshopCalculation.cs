
using System.Collections.Generic;
using System.Linq;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Organization;
using SandOcean.Designer.Workshop;

namespace SandOcean.Technology
{
    public class STechnologyWorkshopCalculation : IEcsInitSystem
    {
        //������
        readonly EcsCustomInject<ContentData> contentData = default;


        public void Init(IEcsSystems systems)
        {
            //���������� ������� ���������� � ������ ����������
            TechnologyWorkshopCalculation();
        }

        void TechnologyWorkshopCalculation()
        {
            //������� ��������� ��� �������� �������������
            contentData.Value.globalTechnologyModifiers = new DTechnologyModifiers(0);

            //���� ������ �� ����� ������������ ����������
            ref DTechnologyModifiers totalTechnologyModifiers = ref contentData.Value.globalTechnologyModifiers;

            //������ ��������� ��� ��������� ������
            TempTechnologyData tempTechnologyData = new(0);

            //���� ����� ������� �������� � ���������� - ��� �� ������ ��������, ��� ����� ����� ����������
            int contentSetDescriptionsNumber = contentData.Value.wDContentSetDescriptions.Length;

            //������� ������ �������� ���������� "�������������" ����������
            contentData.Value.globalTechnologies = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

            //��� ������� ������ �������� ������ ������� �����
            for (int a = 0; a < contentSetDescriptionsNumber; a++)
            {
                //������ ����� �������
                contentData.Value.globalTechnologies[a] = new Dictionary<int, DOrganizationTechnology>();

                //��� ������ ����������
                for (int b = 0; b < contentData.Value.wDContentSets[a].technologies.Length; b++)
                {
                    //���� ������ �� ������ ����������
                    ref readonly WDTechnology technology = ref contentData.Value.wDContentSets[a].technologies[b];

                    //��� ������� ��������� ������������ ����������
                    for (int c = 0; c < technology.technologyComponentCoreModifiers.Length; c++)
                    {
                        //���������� ��� ������������ � ��������������� ��������
                        TechnologyFunctions.TechnologyGlobalComponentCoreModifiersTypeCalculation(
                            technology.technologyComponentCoreModifiers[c],
                            a,
                            b,
                            ref tempTechnologyData);
                    }

                    //��� ������� ������������
                    for (int c = 0; c < technology.technologyModifiers.Length; c++)
                    {
                        //���������� ��� ������������ � ��������������� ��������
                        TechnologyFunctions.TechnologyModifierTypeCalculation(
                            technology.technologyModifiers[c],
                            ref totalTechnologyModifiers);
                    }

                    //������� ���������� � ������� ���������� ���������� ��� �������������
                    contentData.Value.globalTechnologies[a].Add(b, new DOrganizationTechnology(true));
                }
            }

            //��������� ������ ����������, ������������ �������� ��������� �� ������� �������,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesEnginePowerPerSize = tempTechnologyData.technologiesEnginePowerPerSize.OrderBy(
                x => x.modifierValue).ToArray();

            //��������� ������ ����������, ������������ ������� �������� �� ������� �������,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesReactorEnergyPerSize = tempTechnologyData.technologiesReactorEnergyPerSize.OrderBy(
                x => x.modifierValue).ToArray();

            //��������� ������ ����������, ������������ ������ ���������� ����,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesFuelTankCompression = tempTechnologyData.technologiesFuelTankCompression.OrderBy(
                x => x.modifierValue).ToArray();

            //��������� ������ ����������, ������������ �������� ������ ������������ ��� ������ ������ �� ������� �������,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesSolidExtractionEquipmentSpeedPerSize = tempTechnologyData.technologiesExtractionEquipmentSolidSpeedPerSize.OrderBy(
                x => x.modifierValue).ToArray();

            //��������� ������ ����������, ������������ ����������� �������������� ������,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesEnergyGunRecharge = tempTechnologyData.technologiesEnergyGunRecharge.OrderBy(
                x => x.modifierValue).ToArray();
        }
    }
}