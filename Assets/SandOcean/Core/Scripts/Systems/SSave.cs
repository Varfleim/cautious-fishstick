using System.IO;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Technology;
using SandOcean.Designer;
using SandOcean.Designer.Save;
using SandOcean.Designer.Workshop;
using SandOcean.Designer.Game;

namespace SandOcean
{
    public class SSave : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Общие события
        readonly EcsFilterInject<Inc<RSaveContentSet>> saveContentSetRequestFilter = default;
        readonly EcsPoolInject<RSaveContentSet> saveContentSetRequestPool = default;

        //Данные
        readonly EcsCustomInject<ContentData> contentData = default;
        readonly EcsCustomInject<DesignerData> designerData = default;

        public void Run(IEcsSystems systems)
        {
            //Проверяем, не требуется ли сохранить списки наборов контента
            SaveContentSets();
        }

        void SaveContentSets()
        {
            //Для каждого запроса сохранения списка набора контента
            foreach (int saveRequestEntity in saveContentSetRequestFilter.Value)
            {
                //Берём компонент запроса
                ref RSaveContentSet saveRequest = ref saveContentSetRequestPool.Value.Get(saveRequestEntity);

                //Формируем путь для сохранения
                string path = "";

                //Берём путь из описания набора контента
                path = contentData.Value.wDContentSetDescriptions[saveRequest.contentSetIndex].contentSetDirectoryPath;

                //Запрашиваем сохранение набора контента
                WorkshopSaveContentSet(
                    saveRequest.contentSetIndex,
                    path);

                world.Value.DelEntity(saveRequestEntity);
            }
        }

        //Данные мастерской
        void WorkshopSaveContentSet(
            int contentSetIndex,
            string path)
        {
            //Собираем путь к файлу набора контента
            path = Path.Combine(path, "ContentSet.json");

            //Берём ссылку на сохраняемый набор контента
            ref readonly WDContentSet contentSet = ref contentData.Value.wDContentSets[contentSetIndex];

            //Создаём класс сохраняемого набора контента
            SDContentSetClass contentSetClass
                = new()
                {
                    contentSet = new(
                        new SDTechnology[contentSet.technologies.Length],
                        new SDShipType[contentSet.shipTypes.Length],
                        new SDShipClass[contentSet.shipClasses.Length],
                        new SDEngine[contentSet.engines.Length],
                        new SDReactor[contentSet.reactors.Length],
                        new SDHoldFuelTank[contentSet.fuelTanks.Length],
                        new SDExtractionEquipment[contentSet.solidExtractionEquipments.Length],
                        new SDGunEnergy[contentSet.energyGuns.Length])
                };

            //Сохраняем технологии
            WorkshopSaveTechnologies(
                in contentSet.technologies,
                ref contentSetClass.contentSet.technologies);


            //Сохраняем типы кораблей
            WorkshopSaveShipTypes(
                in contentSet.shipTypes,
                ref contentSetClass.contentSet.shipTypes);

            //Сохраняем классы кораблей
            WorkshopSaveShipClasses(
                in contentSet.shipClasses,
                ref contentSetClass.contentSet.shipClasses);

            //Сохраняем двигатели
            WorkshopSaveEngines(
                in contentSet.engines,
                ref contentSetClass.contentSet.engines);

            //Сохраняем реакторы
            WorkshopSaveReactors(
                in contentSet.reactors,
                ref contentSetClass.contentSet.reactors);

            //Сохраняем топливные баки
            WorkshopSaveFuelTank(
                in contentSet.fuelTanks,
                ref contentSetClass.contentSet.fuelTanks);

            //Сохраняем оборудование для твёрдой добычи
            WorkshopSaveExtractionEquipment(
                in contentSet.solidExtractionEquipments,
                ref designerData.Value.solidExtractionEquipmentCoreModifierTypes,
                ref contentSetClass.contentSet.extractionEquipmentSolids);

            //Сохраняем энергетические орудия
            WorkshopSaveEnergyGuns(
                in contentSet.energyGuns,
                ref contentSetClass.contentSet.energyGuns);


            //Сохраняем набор контента в файл
            File.WriteAllText(
                path,
                JsonUtility.ToJson(
                    contentSetClass,
                    true));
        }

        //Технологии
        void WorkshopSaveTechnologies(
            in WDTechnology[] savingTechnologies,
            ref SDTechnology[] technologies)
        {
            //Создаём списки для временных данных
            List<SDTechnologyModifier> technologyModifiers
                = new();
            List<SDTechnologyComponentCoreModifier> technologyComponentCoreModifiers
                = new();

            //Для каждой сохраняемой технологии
            for (int a = 0; a < savingTechnologies.Length; a++)
            {
                //Берём ссылку на сохраняемую технологию
                ref readonly WDTechnology savingTechnology
                    = ref savingTechnologies[a];

                //Очищаем временный список модификаторов
                technologyModifiers.Clear();
                //Для каждого модификатора технологии
                for (int b = 0; b < savingTechnology.technologyModifiers.Length; b++)
                {
                    //Записываем сохраняемые данные модификатора
                    SDTechnologyModifier technologyModifier
                        = new(
                            savingTechnology.technologyModifiers[b].modifierName,
                            savingTechnology.technologyModifiers[b].ModifierValue);

                    //Заносим его во временный список
                    technologyModifiers.Add(
                        technologyModifier);
                }

                //Очищаем временный список основных модификаторов компонентов
                technologyComponentCoreModifiers.Clear();
                //Для каждого основного модификатора компонента
                for (int b = 0; b < savingTechnology.technologyComponentCoreModifiers.Length; b++)
                {
                    //Записываем сохраняемые данные модификатора
                    SDTechnologyComponentCoreModifier technologyComponentCoreModifier
                        = new(
                            savingTechnology.technologyComponentCoreModifiers[b].modifierName,
                            savingTechnology.technologyComponentCoreModifiers[b].ModifierValue);

                    //Заносим его во временный список
                    technologyComponentCoreModifiers.Add(
                        technologyComponentCoreModifier);
                }

                //Записываем сохраняемые данные технологии
                SDTechnology technology
                    = new(
                        savingTechnology.ObjectName,
                        savingTechnology.IsBaseTechnology,
                        technologyModifiers.ToArray(),
                        technologyComponentCoreModifiers.ToArray());

                //Заносим её в сохраняемый массив по соответствующему индексу
                technologies[a]
                    = technology;
            }
        }


        //Типы кораблей
        void WorkshopSaveShipTypes(
            in WDShipType[] savingShipTypes,
            ref SDShipType[] shipTypes)
        {
            //Для каждого сохраняемого типа корабля
            for (int a = 0; a < savingShipTypes.Length; a++)
            {
                //Берём ссылку на сохраняемый тип 
                ref readonly WDShipType savingShipType = ref savingShipTypes[a];

                //Определяем боевую группу типа
                string battleGroupName;

                //Если тип относится к боевой группе большой дальности
                if(savingShipType.BattleGroup == TaskForceBattleGroup.LongRange)
                {
                    battleGroupName = "LongRange";
                }
                //Иначе, если тип относится к боевой группе средней дальности
                else if(savingShipType.BattleGroup == TaskForceBattleGroup.MediumRange)
                {
                    battleGroupName = "MediumRange";
                }
                //Иначе тип относится к боевой группе малой дальности
                else
                {
                    battleGroupName = "ShortRange";
                }

                //Записываем данные типа
                SDShipType shipType = new(
                    savingShipType.ObjectName,
                    battleGroupName);

                //Заносим его в сохраняемый массив по соответствующему индексу
                shipTypes[a] = shipType;
            }
        }

        //Классы кораблей
        void WorkshopSaveShipClasses(
            in WDShipClass[] savingShipClasses,
            ref SDShipClass[] shipClasses)
        {
            //Создаём списки для временных данных
            List<SDShipClassComponent> installedEngines
                = new();
            List<SDShipClassComponent> installedReactors
                = new();
            List<SDShipClassComponent> installedFuelTanks
                = new();
            List<SDShipClassComponent> installedSolidExtractionEquipments
                = new();
            List<SDShipClassComponent> installedEnergyGuns
                = new();

            //Для каждого сохраняемого класса корабля
            for (int a = 0; a < savingShipClasses.Length; a++)
            {
                //Берём ссылку на сохраняемый корабль
                ref readonly WDShipClass savingShipClass
                    = ref savingShipClasses[a];

                //Очищаем временный список двигателей
                installedEngines.Clear();
                //Для каждого двигателя
                for (int b = 0; b < savingShipClass.engines.Length; b++)
                {
                    //Записываем сохраняемые данные двигателя
                    SDShipClassComponent engine
                        = new(
                            savingShipClass.engines[b].contentSetName,
                            savingShipClass.engines[b].componentName,
                            savingShipClass.engines[b].numberOfComponents);

                    //Заносим его во временный список
                    installedEngines.Add(
                        engine);
                }

                //Очищаем временный список реакторов
                installedReactors.Clear();
                //Для каждого реактора
                for (int b = 0; b < savingShipClass.reactors.Length; b++)
                {
                    //Записываем сохраняемые данные реактора
                    SDShipClassComponent reactor
                        = new(
                            savingShipClass.reactors[b].contentSetName,
                            savingShipClass.reactors[b].componentName,
                            savingShipClass.reactors[b].numberOfComponents);

                    //Заносим его во временный список
                    installedReactors.Add(
                        reactor);
                }

                //Очищаем временный список топливных баков
                installedFuelTanks.Clear();
                //Для каждого топливного бака
                for (int b = 0; b < savingShipClass.fuelTanks.Length; b++)
                {
                    //Записываем сохраняемые данные топливного бака
                    SDShipClassComponent fuelTank
                        = new(
                            savingShipClass.fuelTanks[b].contentSetName,
                            savingShipClass.fuelTanks[b].componentName,
                            savingShipClass.fuelTanks[b].numberOfComponents);

                    //Заносим его во временный список
                    installedFuelTanks.Add(
                        fuelTank);
                }

                //Очищаем временный список оборудования для твёрдой добычи
                installedSolidExtractionEquipments.Clear();
                //Для каждого оборудования для твёрдой добычи
                for (int b = 0; b < savingShipClass.extractionEquipmentSolids.Length; b++)
                {
                    //Записываем сохраняемые данные оборудования для твёрдой добычи
                    SDShipClassComponent solidExtractionEquipment
                        = new(
                            savingShipClass.extractionEquipmentSolids[b].contentSetName,
                            savingShipClass.extractionEquipmentSolids[b].componentName,
                            savingShipClass.extractionEquipmentSolids[b].numberOfComponents);

                    //Заносим его во временный список
                    installedSolidExtractionEquipments.Add(
                        solidExtractionEquipment);
                }

                //Очищаем временный список энергетических орудий
                installedEnergyGuns.Clear();
                //Для каждого энергетического орудия
                for (int b = 0; b < savingShipClass.energyGuns.Length; b++)
                {
                    //Записываем сохраняемые данные энергетического орудия
                    SDShipClassComponent energyGun
                        = new(
                            savingShipClass.energyGuns[b].contentSetName,
                            savingShipClass.energyGuns[b].componentName,
                            savingShipClass.energyGuns[b].numberOfComponents);

                    //Заносим его во временный список
                    installedEnergyGuns.Add(
                        energyGun);
                }

                //Записываем сохраняемые данные корабля
                SDShipClass shipClass
                    = new(
                        savingShipClass.ObjectName,
                        installedEngines.ToArray(),
                        installedReactors.ToArray(),
                        installedFuelTanks.ToArray(),
                        installedSolidExtractionEquipments.ToArray(),
                        installedEnergyGuns.ToArray());

                //Заносим его в сохраняемый массив по соответствующему индексу
                shipClasses[a]
                    = shipClass;
            }
        }

        //Компоненты кораблей
        void WorkshopSaveEngines(
            in WDEngine[] savingEngines,
            ref SDEngine[] engines)
        {
            //Создаём массив для временных данных
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length]; 

            //Для каждого сохраняемого двигателя
            for (int a = 0; a < savingEngines.Length; a++)
            {
                //Берём ссылку на сохраняемый двигатель
                ref readonly WDEngine savingEngine
                    = ref savingEngines[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new SDComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length];
                //Для каждой основной технологии
                for (int b = 0; b < savingEngine.coreTechnologies.Length; b++)
                {
                    //Записываем сохраняемые данные основной технологии
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingEngine.coreTechnologies[b].modifierName,
                            savingEngine.coreTechnologies[b].contentSetName,
                            savingEngine.coreTechnologies[b].technologyName);

                    //Заносим её во временный массив
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //Записываем сохраняемые данные двигателя
                SDEngine engine
                    = new(
                        savingEngine.ObjectName,
                        coreTechnologies,
                        savingEngine.EngineSize,
                        savingEngine.EngineBoost);

                //Заносим его в сохраняемый список по соответствующему индексу
                engines[a]
                    = engine;
            }
        }

        void WorkshopSaveReactors(
            in WDReactor[] savingReactors,
            ref SDReactor[] reactors)
        {
            //Создаём массив для временных данных
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length];

            //Для каждого сохраняемого реактора
            for (int a = 0; a < savingReactors.Length; a++)
            {
                //Берём ссылку на сохраняемый реактор
                ref readonly WDReactor savingReactor
                    = ref savingReactors[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new SDComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length];
                //Для каждой основной технологии
                for (int b = 0; b < savingReactor.coreTechnologies.Length; b++)
                {
                    //Записываем сохраняемые данные основной технологии
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingReactor.coreTechnologies[b].modifierName,
                            savingReactor.coreTechnologies[b].contentSetName,
                            savingReactor.coreTechnologies[b].technologyName);

                    //Заносим её во временный массив
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //Записываем сохраняемые данные реактора
                SDReactor reactor
                    = new(
                        savingReactor.ObjectName,
                        coreTechnologies,
                        savingReactor.ReactorSize,
                        savingReactor.ReactorBoost);

                //Заносим его в сохраняемый список по соответствующему индексу
                reactors[a]
                    = reactor;
            }
        }

        void WorkshopSaveFuelTank(
            in WDHoldFuelTank[] savingFuelTanks,
            ref SDHoldFuelTank[] fuelTanks)
        {
            //Создаём массив для временных данных
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length];

            //Для каждого сохраняемого топливного бака
            for (int a = 0; a < savingFuelTanks.Length; a++)
            {
                //Берём ссылку на сохраняемый топливный бак
                ref readonly WDHoldFuelTank savingFuelTank
                    = ref savingFuelTanks[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new SDComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length];
                //Для каждой основной технологии
                for (int b = 0; b < savingFuelTank.coreTechnologies.Length; b++)
                {
                    //Записываем сохраняемые данные основной технологии
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingFuelTank.coreTechnologies[b].modifierName,
                            savingFuelTank.coreTechnologies[b].contentSetName,
                            savingFuelTank.coreTechnologies[b].technologyName);

                    //Заносим её во временный массив
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //Записываем сохраняемые данные топливного бака
                SDHoldFuelTank fuelTank
                    = new(
                        savingFuelTank.ObjectName,
                        coreTechnologies,
                        savingFuelTank.Size);

                //Заносим его в сохраняемый список по соответствующему индексу
                fuelTanks[a]
                    = fuelTank;
            }
        }

        void WorkshopSaveExtractionEquipment(
            in WDExtractionEquipment[] savingExtractionEquipments,
            ref TechnologyComponentCoreModifierType[] extractionEquipmentCoreModifierTypes,
            ref SDExtractionEquipment[] extractionEquipments)
        {
            //Создаём массив для временных данных
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[extractionEquipmentCoreModifierTypes.Length];

            //Для каждого сохраняемого оборудования для добычи
            for (int a = 0; a < savingExtractionEquipments.Length; a++)
            {
                //Берём ссылку на сохраняемое оборудование для добычи
                ref readonly WDExtractionEquipment savingExtractionEquipment
                    = ref savingExtractionEquipments[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new SDComponentCoreTechnology[extractionEquipmentCoreModifierTypes.Length];
                //Для каждой основной технологии
                for (int b = 0; b < savingExtractionEquipment.coreTechnologies.Length; b++)
                {
                    //Записываем сохраняемые данные основной технологии
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingExtractionEquipment.coreTechnologies[b].modifierName,
                            savingExtractionEquipment.coreTechnologies[b].contentSetName,
                            savingExtractionEquipment.coreTechnologies[b].technologyName);

                    //Заносим её во временный массив
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //Записываем сохраняемые данные оборудования для добычи
                SDExtractionEquipment extractionEquipment
                    = new(
                        savingExtractionEquipment.ObjectName,
                        coreTechnologies,
                        savingExtractionEquipment.Size);

                //Заносим его в сохраняемый список по соответствующему индексу
                extractionEquipments[a]
                    = extractionEquipment;
            }
        }

        void WorkshopSaveEnergyGuns(
            in WDGunEnergy[] savingEnergyGuns,
            ref SDGunEnergy[] energyGuns)
        {
            //Создаём массив для временных данных
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length];

            //Для каждого сохраняемого энергетического орудия
            for (int a = 0; a < savingEnergyGuns.Length; a++)
            {
                //Берём ссылку на сохраняемое энергетическое орудие
                ref readonly WDGunEnergy savingEnergyGun
                    = ref savingEnergyGuns[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new SDComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length];
                //Для каждой основной технологии
                for (int b = 0; b < savingEnergyGun.coreTechnologies.Length; b++)
                {
                    //Записываем сохраняемые данные основной технологии
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingEnergyGun.coreTechnologies[b].modifierName,
                            savingEnergyGun.coreTechnologies[b].contentSetName,
                            savingEnergyGun.coreTechnologies[b].technologyName);

                    //Заносим её во временный массив
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //Записываем сохраняемые данные энергетического орудия
                SDGunEnergy energyGun
                    = new(
                        savingEnergyGun.ObjectName,
                        coreTechnologies,
                        savingEnergyGun.GunCaliber,
                        savingEnergyGun.GunBarrelLength);

                //Заносим его в сохраняемый список по соответствующему индексу
                energyGuns[a]
                    = energyGun;
            }
        }

        void SaveComponentCoreTechnologies(
            in DComponentCoreTechnology[] savingComponentCoreTechnologies,
            in TechnologyComponentCoreModifierType[] componentCoreModifierTypes,
            ref SDComponentCoreTechnology[] componentCoreTechnologies)
        {
            //Для каждой основной технологии компонента
            for (int a = 0; a < savingComponentCoreTechnologies.Length; a++)
            {
                //Если технология существует
                if (savingComponentCoreTechnologies[a].ContentObjectLink.ContentSetIndex
                    >= 0
                    && savingComponentCoreTechnologies[a].ContentObjectLink.ObjectIndex
                    >= 0)
                {
                    //Берём ссылку на данные технологии
                    ref readonly DTechnology technology
                        = ref contentData.Value
                        .contentSets[savingComponentCoreTechnologies[a].ContentObjectLink.ContentSetIndex]
                        .technologies[savingComponentCoreTechnologies[a].ContentObjectLink.ObjectIndex];

                    //Берём название набора контента
                    string contentSetName
                        = contentData.Value
                        .contentSets[savingComponentCoreTechnologies[a].ContentObjectLink.ContentSetIndex].ContentSetName;

                    //Создаём новую структуру для сохраняемых данных основной технологии
                    SDComponentCoreTechnology componentCoreTechnology
                        = new(
                            contentData.Value.technologyComponentCoreModifiersNames[(int)componentCoreModifierTypes[a]],
                            contentSetName,
                            technology.ObjectName);

                    //Заносим её в массив основных технологий
                    componentCoreTechnologies[a]
                        = componentCoreTechnology;
                }
            }
        }
    }
}