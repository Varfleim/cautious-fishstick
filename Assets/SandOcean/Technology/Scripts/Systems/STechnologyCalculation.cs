
using System.Collections.Generic;
using System.Linq;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Diplomacy;
using SandOcean.Designer.Workshop;

namespace SandOcean.Technology
{
    public class STechnologyCalculation : IEcsInitSystem, IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //�������
        readonly EcsPoolInject<COrganization> factionPool = default;

        //����� �������
        readonly EcsFilterInject<Inc<EStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<EStartNewGame> startNewGameEventPool = default;

        //������� ����������
        readonly EcsFilterInject<Inc<ETechnologyCalculateModifiers>> technologyCalculateModifiersEventFilter = default;
        readonly EcsPoolInject<ETechnologyCalculateModifiers> technologyCalculateModifiersEventPool = default;

        //������
        readonly EcsCustomInject<ContentData> contentData = default;

        public void Init(IEcsSystems systems)
        {
            //���������� ���������� ������� ���������� � ������ ����������
            TechnologyGlobalCalculation(
                false);
        }

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ������ ����� ����
            foreach (int startNewGameEventEntity
                in startNewGameEventFilter.Value)
            {
                //���� ��������� ������� ������ ����� ����
                ref EStartNewGame startNewGameEvent
                    = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //���� ���������� �� ���� ����������
                if (startNewGameEvent.isTechnologiesCalculated
                    == false)
                {
                    //���������� ���������� ������� ���������� � ������ ����
                    TechnologyGlobalCalculation(
                        true);

                    //��������, ��� ���������� ���� ����������
                    startNewGameEvent.isTechnologiesCalculated
                        = true;
                }
            }

            //��� ������� ������� ��������� ������������� ���������� �������
            foreach (int technologyCalculateModifiersEventEntity
                in technologyCalculateModifiersEventFilter.Value)
            {
                //���� ��������� �������
                ref ETechnologyCalculateModifiers technologyCalculateModifiersEvent
                    = ref technologyCalculateModifiersEventPool.Value.Get(technologyCalculateModifiersEventEntity);

                //���� ��������� �������
                technologyCalculateModifiersEvent.organizationPE.Unpack(
                    world.Value, 
                    out int factionEntity);
                ref COrganization faction
                    = ref factionPool.Value.Get(factionEntity);

                //����������� �������� ������������� ���������� ��� ��������� �������
                TechnologyFactionCalculation(
                    ref faction);

                world.Value.DelEntity(technologyCalculateModifiersEventEntity);
            }
        }

        void TechnologyGlobalCalculation(
            bool isInGameCalculating)
        {
            //������� ��������� ��� �������� �������������
            contentData.Value.globalTechnologyModifiers
                = new DTechnologyModifiers(0);

            //���� ������ �� ����� ������������ ����������
            ref DTechnologyModifiers totalTechnologyModifiers
                = ref contentData.Value.globalTechnologyModifiers;

            //������ ��������� ��� ��������� ������
            TempTechnologyData tempTechnologyData
                = new(0);

            //���� �������� ���������� ������� ���������� � ������ ����
            if (isInGameCalculating
                == true)
            {
                //���� ����� ������� �������� � ���������� - ��� �� ������ ��������, ��� ����� ����� ����������
                int contentSetDescriptionsNumber
                    = contentData.Value.contentSetDescriptions.Length;

                //������� ������ �������� ���������� "�������������" ����������
                contentData.Value.globalTechnologies
                    = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

                //��� ������� ������ �������� ������ ������� �����
                for (int a = 0; a < contentSetDescriptionsNumber; a++)
                {
                    //������ ����� �������
                    contentData.Value.globalTechnologies[a]
                        = new Dictionary<int, DOrganizationTechnology>();

                    //��� ������ ����������
                    for (int b = 0; b < contentData.Value.contentSets[a].technologies.Length; b++)
                    {
                        //���� ������ �� ������ ����������
                        ref readonly DTechnology technology
                            = ref contentData.Value.contentSets[a].technologies[b];

                        //��� ������� ��������� ������������ ����������
                        for (int c = 0; c < technology.technologyComponentCoreModifiers.Length; c++)
                        {
                            //���������� ��� ������������ � ��������������� ��������
                            TechnologyGlobalComponentCoreModifiersTypeCalculation(
                                technology.technologyComponentCoreModifiers[c],
                                a,
                                b,
                                ref tempTechnologyData);
                        }

                        //��� ������� ������������
                        for (int c = 0; c < technology.technologyModifiers.Length; c++)
                        {
                            //���������� ��� ������������ � ��������������� ��������
                            TechnologyModifierTypeCalculation(
                                technology.technologyModifiers[c],
                                ref totalTechnologyModifiers);
                        }

                        //������� ���������� � ������� ���������� ���������� ��� �������������
                        contentData.Value.globalTechnologies[a].Add(
                            b,
                            new DOrganizationTechnology(
                                true));
                    }
                }
            }
            //����� �������� ���������� ������� ���������� � ������ ����������
            else
            {
                //���� ����� ������� �������� � ���������� - ��� �� ������ ��������, ��� ����� ����� ����������
                int contentSetDescriptionsNumber
                    = contentData.Value.wDContentSetDescriptions.Length;

                //������� ������ �������� ���������� "�������������" ����������
                contentData.Value.globalTechnologies
                    = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

                //��� ������� ������ �������� ������ ������� �����
                for (int a = 0; a < contentSetDescriptionsNumber; a++)
                {
                    //������ ����� �������
                    contentData.Value.globalTechnologies[a]
                        = new Dictionary<int, DOrganizationTechnology>();

                    //��� ������ ����������
                    for (int b = 0; b < contentData.Value.wDContentSets[a].technologies.Length; b++)
                    {
                        //���� ������ �� ������ ����������
                        ref readonly WDTechnology technology
                            = ref contentData.Value.wDContentSets[a].technologies[b];

                        //��� ������� ��������� ������������ ����������
                        for (int c = 0; c < technology.technologyComponentCoreModifiers.Length; c++)
                        {
                            //���������� ��� ������������ � ��������������� ��������
                            TechnologyGlobalComponentCoreModifiersTypeCalculation(
                                technology.technologyComponentCoreModifiers[c],
                                a,
                                b,
                                ref tempTechnologyData);
                        }

                        //��� ������� ������������
                        for (int c = 0; c < technology.technologyModifiers.Length; c++)
                        {
                            //���������� ��� ������������ � ��������������� ��������
                            TechnologyModifierTypeCalculation(
                                technology.technologyModifiers[c],
                                ref totalTechnologyModifiers);
                        }

                        //������� ���������� � ������� ���������� ���������� ��� �������������
                        contentData.Value.globalTechnologies[a].Add(
                            b,
                            new DOrganizationTechnology(
                                true));
                    }
                }
            }

            //��������� ������ ����������, ������������ �������� ��������� �� ������� �������,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesEnginePowerPerSize
                = tempTechnologyData.technologiesEnginePowerPerSize.OrderBy(
                    x => x.modifierValue).ToArray();

            //��������� ������ ����������, ������������ ������� �������� �� ������� �������,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesReactorEnergyPerSize
                = tempTechnologyData.technologiesReactorEnergyPerSize.OrderBy(
                    x => x.modifierValue).ToArray();

            //��������� ������ ����������, ������������ ������ ���������� ����,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesFuelTankCompression
                = tempTechnologyData.technologiesFuelTankCompression.OrderBy(
                    x => x.modifierValue).ToArray();

            //��������� ������ ����������, ������������ �������� ������ ������������ ��� ������ ������ �� ������� �������,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesSolidExtractionEquipmentSpeedPerSize
                = tempTechnologyData.technologiesExtractionEquipmentSolidSpeedPerSize.OrderBy(
                    x => x.modifierValue).ToArray();

            //��������� ������ ����������, ������������ ����������� �������������� ������,
            //� ������� ��� � ��������������� ������
            contentData.Value.technologiesEnergyGunRecharge
                = tempTechnologyData.technologiesEnergyGunRecharge.OrderBy(
                    x => x.modifierValue).ToArray();
        }

        void TechnologyFactionCalculation(
            ref COrganization faction)
        {
            //��� ������� ������ �������� � ������� ���������� �������
            for (int a = 0; a < faction.technologies.Length; a++)
            {
                //��� ������ ���������� �������
                foreach (KeyValuePair<int, DOrganizationTechnology> kVPTechnology
                    in faction.technologies[a])
                {
                    //���� ���������� ��������� �����������
                    if (kVPTechnology.Value.isResearched
                        == true)
                    {
                        //���� ������ �� ������ ����������
                        ref readonly DTechnology technology
                            = ref contentData.Value
                            .contentSets[a]
                            .technologies[kVPTechnology.Key];

                        //��� ������� ������������ ����������
                        for (int b = 0; b < technology.technologyModifiers.Length; b++)
                        {
                            //���������� ��� ������������ � ��������������� ��������
                            TechnologyModifierTypeCalculation(
                                technology.technologyModifiers[b],
                                ref faction.technologyModifiers);
                        }
                    }
                }
            }
        }

        void TechnologyGlobalComponentCoreModifiersTypeCalculation(
            ITechnologyComponentCoreModifier modifier,
            int contentSetIndex,
            int technologyIndex,
            ref TempTechnologyData tempTechnologyData)
        {
            //���������� ��������, ��������������� ���� ������������
            switch (modifier.ModifierType)
            {
                case TechnologyComponentCoreModifierType.EnginePowerPerSize:
                    //������� ������ ������ �������� � ���������� � ������ ����������, ������������
                    //�������� ��������� �� ������� �������
                    tempTechnologyData.technologiesEnginePowerPerSize.Add(
                        new DTechnologyModifierGlobalSort(
                            contentSetIndex,
                            technologyIndex,
                            modifier.ModifierValue));
                    break;
                case TechnologyComponentCoreModifierType.ReactorEnergyPerSize:
                    //������� ������ ������ �������� � ���������� � ������ ����������, ������������ 
                    //������� �������� �� ������� �������
                    tempTechnologyData.technologiesReactorEnergyPerSize.Add(
                        new DTechnologyModifierGlobalSort(
                            contentSetIndex,
                            technologyIndex,
                            modifier.ModifierValue));
                    break;
                case TechnologyComponentCoreModifierType.FuelTankCompression:
                    //������� ������ ������ �������� � ���������� � ������ ����������, ������������ 
                    //������ ���������� ����
                    tempTechnologyData.technologiesFuelTankCompression.Add(
                        new DTechnologyModifierGlobalSort(
                            contentSetIndex,
                            technologyIndex,
                            modifier.ModifierValue));
                    break;
                case TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize:
                    //������� ������ ������ �������� � ���������� � ������ ����������, ������������ 
                    //�������� ������ ������������ ��� ������ ������ �� ������� �������
                    tempTechnologyData.technologiesExtractionEquipmentSolidSpeedPerSize.Add(
                        new DTechnologyModifierGlobalSort(
                            contentSetIndex,
                            technologyIndex,
                            modifier.ModifierValue));
                    break;
                case TechnologyComponentCoreModifierType.GunEnergyRecharge:
                    //������� ������ ������ �������� � ���������� � ������ ����������, ������������ 
                    //����������� �������������� ������
                    tempTechnologyData.technologiesEnergyGunRecharge.Add(
                        new DTechnologyModifierGlobalSort(
                            contentSetIndex,
                            technologyIndex,
                            modifier.ModifierValue));
                    break;
            }
        }

        void TechnologyModifierTypeCalculation(
            ITechnologyModifier modifier,
            ref DTechnologyModifiers factionTechnologyModifiers)
        {
            //���������� ��������, ��������������� ���� ������������
            switch (modifier.ModifierType)
            {
                case TechnologyModifierType.DesignerMinEngineSize:
                    //��������� ����������� ������������ ������� ���������
                    factionTechnologyModifiers.designerMinEngineSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinEngineSize,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxEngineSize:
                    //��������� ����������� ������������� ������� ���������
                    factionTechnologyModifiers.designerMaxEngineSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxEngineSize,
                            true);
                    break;
                case TechnologyModifierType.DesignerMinEngineBoost:
                    //��������� ����������� ������������ ������� ���������
                    factionTechnologyModifiers.designerMinEngineBoost
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinEngineBoost,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxEngineBoost:
                    //��������� ����������� ������������� ������� ���������
                    factionTechnologyModifiers.designerMaxEngineBoost
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxEngineBoost,
                            true);
                    break;

                case TechnologyModifierType.DesignerMinReactorSize:
                    //��������� ����������� ������������ ������� ��������
                    factionTechnologyModifiers.designerMinReactorSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinReactorSize,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxReactorSize:
                    //��������� ����������� ������������� ������� ��������
                    factionTechnologyModifiers.designerMaxReactorSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxReactorSize,
                            true);
                    break;
                case TechnologyModifierType.DesignerMinReactorBoost:
                    //��������� ����������� ������������ ������� ��������
                    factionTechnologyModifiers.designerMinReactorBoost
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinReactorBoost,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxReactorBoost:
                    //��������� ����������� ������������� ������� ��������
                    factionTechnologyModifiers.designerMaxReactorBoost
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxReactorBoost,
                            true);
                    break;

                case TechnologyModifierType.DesignerMinFuelTankSize:
                    //��������� ����������� ������������ ������� ���������� ����
                    factionTechnologyModifiers.designerMinFuelTankSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinFuelTankSize,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxFuelTankSize:
                    //��������� ����������� ������������� ������� ���������� ����
                    factionTechnologyModifiers.designerMaxFuelTankSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxFuelTankSize,
                            true);
                    break;

                case TechnologyModifierType.DesignerMinExtractionEquipmentSolidSize:
                    //��������� ����������� ������������ ������� ������������ ��� ������ ������
                    factionTechnologyModifiers.designerMinSolidExtractionEquipmentSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinSolidExtractionEquipmentSize,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxExtractionEquipmentSolidSize:
                    //��������� ����������� ������������� ������� ������������ ��� ������ ������
                    factionTechnologyModifiers.designerMaxSolidExtractionEquipmentSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxSolidExtractionEquipmentSize,
                            true);
                    break;

                case TechnologyModifierType.DesignerMinGunEnergyCaliber:
                    //��������� ����������� ������������ ������� �������������� ������
                    factionTechnologyModifiers.designerMinEnergyGunCaliber
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinEnergyGunCaliber,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxGunEnergyCaliber:
                    //��������� ����������� ������������� ������� �������������� ������
                    factionTechnologyModifiers.designerMaxEnergyGunCaliber
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxEnergyGunCaliber,
                            true);
                    break;
                case TechnologyModifierType.DesignerMinGunEnergyBarrelLength:
                    //��������� ����������� ����������� ����� ������ �������������� ������
                    factionTechnologyModifiers.designerMinEnergyGunBarrelLength
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinEnergyGunBarrelLength,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxGunEnergyBarrelLength:
                    //��������� ����������� ������������ ����� ������ �������������� ������
                    factionTechnologyModifiers.designerMaxEnergyGunBarrelLength
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxEnergyGunBarrelLength,
                            true);
                    break;
            }
        }

        float ModifierValueCalculation(
            ref ITechnologyModifier modifier,
            float currentValue,
            bool isGreatest)
        {
            //���� ����� ������� ���������� �����
            if (isGreatest
                == true)
            {
                //���� ����� �������� ������������ ������ �����������
                if (modifier.ModifierValue
                    > currentValue)
                {
                    //���������� ����� ��������
                    return modifier.ModifierValue;
                }
                //�����
                else
                {
                    //���������� ������ ��������
                    return currentValue;
                }
            }
            //����� ����� ������� ���������� �����
            else
            {
                //���� ����� �������� ������ �����������
                if (modifier.ModifierValue
                    < currentValue)
                {
                    //���������� ����� ��������
                    return modifier.ModifierValue;
                }
                //�����
                else
                {
                    //���������� ������ ��������
                    return currentValue;
                }
            }
        }
    }
}