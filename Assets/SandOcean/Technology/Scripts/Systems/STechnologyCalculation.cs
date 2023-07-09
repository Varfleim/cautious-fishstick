
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
        //Миры
        readonly EcsWorldInject world = default;

        //Фракции
        readonly EcsPoolInject<COrganization> factionPool = default;

        //Общие события
        readonly EcsFilterInject<Inc<EStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<EStartNewGame> startNewGameEventPool = default;

        //События технологий
        readonly EcsFilterInject<Inc<ETechnologyCalculateModifiers>> technologyCalculateModifiersEventFilter = default;
        readonly EcsPoolInject<ETechnologyCalculateModifiers> technologyCalculateModifiersEventPool = default;

        //Данные
        readonly EcsCustomInject<ContentData> contentData = default;

        public void Init(IEcsSystems systems)
        {
            //Производим глобальный рассчёт технологий в данных мастерской
            TechnologyGlobalCalculation(
                false);
        }

        public void Run(IEcsSystems systems)
        {
            //Для каждого события начала новой игры
            foreach (int startNewGameEventEntity
                in startNewGameEventFilter.Value)
            {
                //Берём компонент события начала новой игры
                ref EStartNewGame startNewGameEvent
                    = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //Если технологии не были рассчитаны
                if (startNewGameEvent.isTechnologiesCalculated
                    == false)
                {
                    //Производим глобальный рассчёт технологий в данных игры
                    TechnologyGlobalCalculation(
                        true);

                    //Отмечаем, что технологии были рассчитаны
                    startNewGameEvent.isTechnologiesCalculated
                        = true;
                }
            }

            //Для каждого события пересчёта модификаторов технологий фракции
            foreach (int technologyCalculateModifiersEventEntity
                in technologyCalculateModifiersEventFilter.Value)
            {
                //Берём компонент события
                ref ETechnologyCalculateModifiers technologyCalculateModifiersEvent
                    = ref technologyCalculateModifiersEventPool.Value.Get(technologyCalculateModifiersEventEntity);

                //Берём компонент фракции
                technologyCalculateModifiersEvent.organizationPE.Unpack(
                    world.Value, 
                    out int factionEntity);
                ref COrganization faction
                    = ref factionPool.Value.Get(factionEntity);

                //Запрашиваем пересчёт модификаторов технологий для указанной фракции
                TechnologyFactionCalculation(
                    ref faction);

                world.Value.DelEntity(technologyCalculateModifiersEventEntity);
            }
        }

        void TechnologyGlobalCalculation(
            bool isInGameCalculating)
        {
            //Очищаем структуру для хранения модификаторов
            contentData.Value.globalTechnologyModifiers
                = new DTechnologyModifiers(0);

            //Берём ссылку на общие модификаторы технологий
            ref DTechnologyModifiers totalTechnologyModifiers
                = ref contentData.Value.globalTechnologyModifiers;

            //Создаём структуру для временных данных
            TempTechnologyData tempTechnologyData
                = new(0);

            //Если запрошен глобальный рассчёт технологий в режиме игры
            if (isInGameCalculating
                == true)
            {
                //Берём число наборов контента с описаниями - это те наборы контента, что могут иметь технологии
                int contentSetDescriptionsNumber
                    = contentData.Value.contentSetDescriptions.Length;

                //Очищаем массив словарей глобальных "исследованных" технологий
                contentData.Value.globalTechnologies
                    = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

                //Для каждого набора контента меньше данного числа
                for (int a = 0; a < contentSetDescriptionsNumber; a++)
                {
                    //Создаём новый словарь
                    contentData.Value.globalTechnologies[a]
                        = new Dictionary<int, DOrganizationTechnology>();

                    //Для каждой технологии
                    for (int b = 0; b < contentData.Value.contentSets[a].technologies.Length; b++)
                    {
                        //Берём ссылку на данные технологии
                        ref readonly DTechnology technology
                            = ref contentData.Value.contentSets[a].technologies[b];

                        //Для каждого основного модификатора компонента
                        for (int c = 0; c < technology.technologyComponentCoreModifiers.Length; c++)
                        {
                            //Определяем тип модификатора и соответствующее действие
                            TechnologyGlobalComponentCoreModifiersTypeCalculation(
                                technology.technologyComponentCoreModifiers[c],
                                a,
                                b,
                                ref tempTechnologyData);
                        }

                        //Для каждого модификатора
                        for (int c = 0; c < technology.technologyModifiers.Length; c++)
                        {
                            //Определяем тип модификатора и соответствующие действия
                            TechnologyModifierTypeCalculation(
                                technology.technologyModifiers[c],
                                ref totalTechnologyModifiers);
                        }

                        //Заносим технологию в словарь глобальных технологий как исследованную
                        contentData.Value.globalTechnologies[a].Add(
                            b,
                            new DOrganizationTechnology(
                                true));
                    }
                }
            }
            //Иначе запрошен глобальный рассчёт технологий в режиме мастерской
            else
            {
                //Берём число наборов контента с описаниями - это те наборы контента, что могут иметь технологии
                int contentSetDescriptionsNumber
                    = contentData.Value.wDContentSetDescriptions.Length;

                //Очищаем массив словарей глобальных "исследованных" технологий
                contentData.Value.globalTechnologies
                    = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

                //Для каждого набора контента меньше данного числа
                for (int a = 0; a < contentSetDescriptionsNumber; a++)
                {
                    //Создаём новый словарь
                    contentData.Value.globalTechnologies[a]
                        = new Dictionary<int, DOrganizationTechnology>();

                    //Для каждой технологии
                    for (int b = 0; b < contentData.Value.wDContentSets[a].technologies.Length; b++)
                    {
                        //Берём ссылку на данные технологии
                        ref readonly WDTechnology technology
                            = ref contentData.Value.wDContentSets[a].technologies[b];

                        //Для каждого основного модификатора компонента
                        for (int c = 0; c < technology.technologyComponentCoreModifiers.Length; c++)
                        {
                            //Определяем тип модификатора и соответствующие действия
                            TechnologyGlobalComponentCoreModifiersTypeCalculation(
                                technology.technologyComponentCoreModifiers[c],
                                a,
                                b,
                                ref tempTechnologyData);
                        }

                        //Для каждого модификатора
                        for (int c = 0; c < technology.technologyModifiers.Length; c++)
                        {
                            //Определяем тип модификатора и соответствующие действия
                            TechnologyModifierTypeCalculation(
                                technology.technologyModifiers[c],
                                ref totalTechnologyModifiers);
                        }

                        //Заносим технологию в словарь глобальных технологий как исследованную
                        contentData.Value.globalTechnologies[a].Add(
                            b,
                            new DOrganizationTechnology(
                                true));
                    }
                }
            }

            //Сортируем список технологий, определяющих мощность двигателя на единицу размера,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesEnginePowerPerSize
                = tempTechnologyData.technologiesEnginePowerPerSize.OrderBy(
                    x => x.modifierValue).ToArray();

            //Сортируем список технологий, определяющих энергию реактора на единицу размера,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesReactorEnergyPerSize
                = tempTechnologyData.technologiesReactorEnergyPerSize.OrderBy(
                    x => x.modifierValue).ToArray();

            //Сортируем список технологий, определяющих сжатие топливного бака,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesFuelTankCompression
                = tempTechnologyData.technologiesFuelTankCompression.OrderBy(
                    x => x.modifierValue).ToArray();

            //Сортируем список технологий, определяющих скорость добычи оборудования для твёрдой добычи на единицу размера,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesSolidExtractionEquipmentSpeedPerSize
                = tempTechnologyData.technologiesExtractionEquipmentSolidSpeedPerSize.OrderBy(
                    x => x.modifierValue).ToArray();

            //Сортируем список технологий, определяющих перезарядку энергетических орудий,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesEnergyGunRecharge
                = tempTechnologyData.technologiesEnergyGunRecharge.OrderBy(
                    x => x.modifierValue).ToArray();
        }

        void TechnologyFactionCalculation(
            ref COrganization faction)
        {
            //Для каждого набора контента в массиве технологий фракции
            for (int a = 0; a < faction.technologies.Length; a++)
            {
                //Для каждой технологии фракции
                foreach (KeyValuePair<int, DOrganizationTechnology> kVPTechnology
                    in faction.technologies[a])
                {
                    //Если технология полностью исследована
                    if (kVPTechnology.Value.isResearched
                        == true)
                    {
                        //Берём ссылку на данные технологии
                        ref readonly DTechnology technology
                            = ref contentData.Value
                            .contentSets[a]
                            .technologies[kVPTechnology.Key];

                        //Для каждого модификатора технологии
                        for (int b = 0; b < technology.technologyModifiers.Length; b++)
                        {
                            //Определяем тип модификатора и соответствующие действия
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

        void TechnologyModifierTypeCalculation(
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

        float ModifierValueCalculation(
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