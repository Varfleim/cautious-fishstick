
namespace SandOcean.Technology
{
    public static class TechnologyFunctions
    {
        public static void TechnologyGlobalComponentCoreModifiersTypeCalculation(
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

        public static void TechnologyModifierTypeCalculation(
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

        static float ModifierValueCalculation(
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