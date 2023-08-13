
using System.Collections.Generic;
using System.Linq;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Diplomacy;

namespace SandOcean.Technology
{
    public class STechnologyNewGameCalculation : IEcsRunSystem
    {
        //����� �������
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<RStartNewGame> startNewGameEventPool = default;


        //������
        readonly EcsCustomInject<ContentData> contentData = default;


        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ������ ����� ����
            foreach (int startNewGameEventEntity in startNewGameEventFilter.Value)
            {
                //���� ��������� ������� ������ ����� ����
                ref RStartNewGame startNewGameEvent = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //���������� ���������� ������� ���������� � ������ ����
                TechnologyNewGameCalculation();
            }
        }

        void TechnologyNewGameCalculation()
        {
            //������� ��������� ��� �������� �������������
            contentData.Value.globalTechnologyModifiers = new DTechnologyModifiers(0);

            //���� ������ �� ����� ������������ ����������
            ref DTechnologyModifiers totalTechnologyModifiers = ref contentData.Value.globalTechnologyModifiers;

            //������ ��������� ��� ��������� ������
            TempTechnologyData tempTechnologyData = new(0);

            //���� ����� ������� �������� � ���������� - ��� �� ������ ��������, ��� ����� ����� ����������
            int contentSetDescriptionsNumber = contentData.Value.contentSetDescriptions.Length;

            //������� ������ �������� ���������� "�������������" ����������
            contentData.Value.globalTechnologies = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

            //��� ������� ������ �������� ������ ������� �����
            for (int a = 0; a < contentSetDescriptionsNumber; a++)
            {
                //������ ����� �������
                contentData.Value.globalTechnologies[a] = new Dictionary<int, DOrganizationTechnology>();

                //��� ������ ����������
                for (int b = 0; b < contentData.Value.contentSets[a].technologies.Length; b++)
                {
                    //���� ������ �� ������ ����������
                    ref readonly DTechnology technology = ref contentData.Value.contentSets[a].technologies[b];

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