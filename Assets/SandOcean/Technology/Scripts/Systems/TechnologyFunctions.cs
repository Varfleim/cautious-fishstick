
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
            //Определяем действие, соответствующее типу модификатора
            switch (modifier.ModifierType)
            {
                case TechnologyComponentCoreModifierType.EnginePowerPerSize:
                    //Заносим индекс набора контента и технологии в список технологий, определяющих
                    //мощность двигателя на единицу размера
                    tempTechnologyData.technologiesEnginePowerPerSize.Add(
                        new DTechnologyModifierGlobalSort(
                            contentSetIndex,
                            technologyIndex,
                            modifier.ModifierValue));
                    break;
                case TechnologyComponentCoreModifierType.ReactorEnergyPerSize:
                    //Заносим индекс набора контента и технологии в список технологий, определяющих 
                    //энергию реактора не единицу размера
                    tempTechnologyData.technologiesReactorEnergyPerSize.Add(
                        new DTechnologyModifierGlobalSort(
                            contentSetIndex,
                            technologyIndex,
                            modifier.ModifierValue));
                    break;
                case TechnologyComponentCoreModifierType.FuelTankCompression:
                    //Заносим индекс набора контента и технологии в список технологий, определяющих 
                    //сжатие топливного бака
                    tempTechnologyData.technologiesFuelTankCompression.Add(
                        new DTechnologyModifierGlobalSort(
                            contentSetIndex,
                            technologyIndex,
                            modifier.ModifierValue));
                    break;
                case TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize:
                    //Заносим индекс набора контента и технологии в список технологий, определяющих 
                    //скорость добычи оборудования для твёрдой добычи на единицу размера
                    tempTechnologyData.technologiesExtractionEquipmentSolidSpeedPerSize.Add(
                        new DTechnologyModifierGlobalSort(
                            contentSetIndex,
                            technologyIndex,
                            modifier.ModifierValue));
                    break;
                case TechnologyComponentCoreModifierType.GunEnergyRecharge:
                    //Заносим индекс набора контента и технологии в список технологий, определяющих 
                    //перезарядку энергетических орудий
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
            //Определяем действие, соответствующее типу модификатора
            switch (modifier.ModifierType)
            {
                case TechnologyModifierType.DesignerMinEngineSize:
                    //Обновляем модификатор минимального размера двигателя
                    factionTechnologyModifiers.designerMinEngineSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinEngineSize,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxEngineSize:
                    //Обновляем модификатор максимального размера двигателя
                    factionTechnologyModifiers.designerMaxEngineSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxEngineSize,
                            true);
                    break;
                case TechnologyModifierType.DesignerMinEngineBoost:
                    //Обновляем модификатор минимального разгона двигателя
                    factionTechnologyModifiers.designerMinEngineBoost
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinEngineBoost,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxEngineBoost:
                    //Обновляем модификатор максимального разгона двигателя
                    factionTechnologyModifiers.designerMaxEngineBoost
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxEngineBoost,
                            true);
                    break;

                case TechnologyModifierType.DesignerMinReactorSize:
                    //Обновляем модификатор минимального размера реактора
                    factionTechnologyModifiers.designerMinReactorSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinReactorSize,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxReactorSize:
                    //Обновляем модификатор максимального размера реактора
                    factionTechnologyModifiers.designerMaxReactorSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxReactorSize,
                            true);
                    break;
                case TechnologyModifierType.DesignerMinReactorBoost:
                    //Обновляем модификатор минимального разгона реактора
                    factionTechnologyModifiers.designerMinReactorBoost
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinReactorBoost,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxReactorBoost:
                    //Обновляем модификатор максимального разгона реактора
                    factionTechnologyModifiers.designerMaxReactorBoost
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxReactorBoost,
                            true);
                    break;

                case TechnologyModifierType.DesignerMinFuelTankSize:
                    //Обновляем модификатор минимального размера топливного бака
                    factionTechnologyModifiers.designerMinFuelTankSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinFuelTankSize,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxFuelTankSize:
                    //Обновляем модификатор максимального размера топливного бака
                    factionTechnologyModifiers.designerMaxFuelTankSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxFuelTankSize,
                            true);
                    break;

                case TechnologyModifierType.DesignerMinExtractionEquipmentSolidSize:
                    //Обновляем модификатор минимального размера оборудования для твёрдой добычи
                    factionTechnologyModifiers.designerMinSolidExtractionEquipmentSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinSolidExtractionEquipmentSize,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxExtractionEquipmentSolidSize:
                    //Обновляем модификатор максимального размера оборудования для твёрдой добычи
                    factionTechnologyModifiers.designerMaxSolidExtractionEquipmentSize
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxSolidExtractionEquipmentSize,
                            true);
                    break;

                case TechnologyModifierType.DesignerMinGunEnergyCaliber:
                    //Обновляем модификатор минимального калибра энергетических орудий
                    factionTechnologyModifiers.designerMinEnergyGunCaliber
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinEnergyGunCaliber,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxGunEnergyCaliber:
                    //Обновляем модификатор максимального калибра энергетических орудий
                    factionTechnologyModifiers.designerMaxEnergyGunCaliber
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMaxEnergyGunCaliber,
                            true);
                    break;
                case TechnologyModifierType.DesignerMinGunEnergyBarrelLength:
                    //Обновляем модификатор минимальной длины ствола энергетических орудий
                    factionTechnologyModifiers.designerMinEnergyGunBarrelLength
                        = ModifierValueCalculation(
                            ref modifier,
                            factionTechnologyModifiers.designerMinEnergyGunBarrelLength,
                            false);
                    break;
                case TechnologyModifierType.DesignerMaxGunEnergyBarrelLength:
                    //Обновляем модификатор максимальной длины ствола энергетических орудий
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
            //Если нужно выбрать наибольшее число
            if (isGreatest
                == true)
            {
                //Если новое значение модификатора больше предыдущего
                if (modifier.ModifierValue
                    > currentValue)
                {
                    //Возвращаем новое значение
                    return modifier.ModifierValue;
                }
                //Иначе
                else
                {
                    //Возвращаем старое значение
                    return currentValue;
                }
            }
            //Иначе нужно выбрать наименьшее число
            else
            {
                //Если новое значение меньше предыдущего
                if (modifier.ModifierValue
                    < currentValue)
                {
                    //Возвращаем новое значение
                    return modifier.ModifierValue;
                }
                //Иначе
                else
                {
                    //Возвращаем старое значение
                    return currentValue;
                }
            }
        }
    }
}