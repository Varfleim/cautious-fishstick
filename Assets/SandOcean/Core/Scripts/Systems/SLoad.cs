using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Technology;
using SandOcean.Designer;
using SandOcean.Designer.Workshop;
using SandOcean.Designer.Game;
using SandOcean.Designer.Save;
using SandOcean.Warfare.Ship;

namespace SandOcean
{
    public class SLoad : IEcsInitSystem, IEcsRunSystem
    {
        //Общие события
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<RStartNewGame> startNewGameEventPool = default;

        //Данные
        readonly EcsCustomInject<ContentData> contentData = default;
        readonly EcsCustomInject<DesignerData> designerData = default;

        public void Init(IEcsSystems systems)
        {
            //Загружаем данные наборов контента в данные мастерской

            //Создаём массив описаний наборов контента
            contentData.Value.wDContentSetDescriptions
                = new WDContentSetDescription[0];

            //Создаём массив названий наборов контента
            contentData.Value.contentSetNames
                = new string[0];

            //Создаём массив наборов контента
            contentData.Value.wDContentSets
                = new WDContentSet[0];


            //Создаём файл для временных данных загрузки
            TempLoadingWorkshopData tempLoadingWorkshopData
                = new(0);

            //Собираем путь до папки, в которой находятся наборы контента
            string contentSetDirectoryPath
                = Path.Combine(Application.dataPath, "ContentSets");

            //Берём пути папок, в которых, возможно, находятся наборы контента
            string[] contentSetDirectoriesPaths
                = Directory.GetDirectories(contentSetDirectoryPath);

            //Для каждого пути
            for (int a = 0; a < contentSetDirectoriesPaths.Length; a++)
            {
                //Если в данной папке существует файл описания набора контена
                if (File.Exists(Path.Combine(
                    contentSetDirectoriesPaths[a], "ContentSetDescription.json")))
                {
                    //Загружаем описание набора контента
                    SDContentSetDescription contentSetDescription
                        = LoadContentSetDescription(
                            Path.Combine(
                                contentSetDirectoriesPaths[a], "ContentSetDescription.json"));

                    //Расширяем массив описаний наборов контента
                    Array.Resize(
                        ref contentData.Value.wDContentSetDescriptions,
                        contentData.Value.wDContentSetDescriptions.Length + 1);

                    //Заносим описание набора контента в массив
                    contentData.Value.wDContentSetDescriptions[contentData.Value.wDContentSetDescriptions.Length - 1]
                        = new WDContentSetDescription(
                            contentSetDescription.contentSetName,
                            contentSetDescription.contentSetVersion,
                            contentSetDescription.gameVersion,
                            contentSetDirectoriesPaths[a]);

                    Debug.LogWarning(contentSetDirectoriesPaths[a]);

                    //Расширяем массив названий наборов контента
                    Array.Resize(
                        ref contentData.Value.contentSetNames,
                        contentData.Value.contentSetNames.Length + 1);

                    //Заносим название набора контента в массив
                    contentData.Value.contentSetNames[contentData.Value.contentSetNames.Length - 1]
                        = contentSetDescription.contentSetName;


                    //Расширяем массив наборов контента
                    Array.Resize(
                        ref contentData.Value.wDContentSets,
                        contentData.Value.wDContentSets.Length + 1);

                    //Создаём пустой набор контента и заносим его в массив
                    contentData.Value.wDContentSets[contentData.Value.wDContentSets.Length - 1]
                        = new WDContentSet(contentSetDescription.contentSetName);

                    //Если в данной папке существует файл набора контента
                    if (File.Exists(Path.Combine(
                        contentSetDirectoriesPaths[a], "ContentSet.json")))
                    {
                        //Читаем данные
                        SDContentSetClass contentSetClass
                            = JsonUtility.FromJson<SDContentSetClass>(
                                File.ReadAllText(
                                    Path.Combine(
                                        contentSetDirectoriesPaths[a], "ContentSet.json")));

                        //Загружаем набор контента
                        WorkshopLoadContentSet(
                            ref contentSetClass.contentSet,
                            ref contentData.Value.wDContentSets[contentData.Value.wDContentSets.Length - 1],
                            ref tempLoadingWorkshopData);
                    }
                }
            }

            //Рассчитываем ссылки объектов друг на друга, начиная с первого набора контента
            WorkshopContentObjectRefsCalculating(
                0);
        }

        public void Run(IEcsSystems systems)
        {
            //Для каждого события начала новой игры
            foreach (int startNewGameEventEntity
                in startNewGameEventFilter.Value)
            {
                //Берём компонент события начала новой игры
                ref RStartNewGame startNewGameEvent
                    = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //Загружаем данные мастерской в данные игры
                GameDataLoad();
            }

            //При запросе загрузки сохранения и запуска игры
            if (false)
            {
                //Загружаем файл сохранения

                //Для каждого набора контента в сохранении

                //Загружаем набор контента в WDContentSet по обычным правилам


                //Нельзя отключить загрузку набора контента, если на него ссылается другой набор контента, который включен


                //Создаём файл для временных данных загрузки
                TempLoadingData tempLoadingData
                    = new(0);

                //Для каждого существующего описания набора контента
                for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                {
                    //Если набор контента активен
                    if (contentData.Value.wDContentSets[a].isActive
                        == true)
                    {
                        //Расширяем массив наборов контента
                        Array.Resize(
                            ref contentData.Value.contentSets,
                            contentData.Value.contentSets.Length + 1);

                        //Создаём пустой набор контента и заносим его в массив
                        contentData.Value.contentSets[contentData.Value.contentSets.Length - 1]
                            = new DContentSet(contentData.Value.wDContentSets[a].ContentSetName);

                        //Загружаем его данные из данных мастерской в данные игры
                        GameLoadContentSet(
                            ref contentData.Value.wDContentSets[a],
                            ref contentData.Value.contentSets[contentData.Value.contentSets.Length - 1],
                            ref tempLoadingData);

                        //Указываем в данных набора контента его индекс в игре
                        contentData.Value.wDContentSets[a].gameContentSetIndex
                            = contentData.Value.contentSets.Length - 1;
                    }
                }

                //Рассчитываем ссылки объектов друг на друга
                GameContentObjectRefsCalculating();
            }
        }

        //Берём строку-путь до файла, хранящего данные
        //string fileName = "НазваниеФайла";
        //string path = Path.Combine(Адрес файла (Application.persistentDataPath), fileName + ".json");

        SDContentSetDescription LoadContentSetDescription(
            string path)
        {
            //Читаем данные
            SDContentSetDescriptionClass contentSetDescriptionClass
                = JsonUtility.FromJson<SDContentSetDescriptionClass>(File.ReadAllText(path));

            //Возвращаем считанное описание набора контента
            return contentSetDescriptionClass.contentSetDescription;
        }

        void GameDataLoad()
        {
            //Создаём файл для временных данных загрузки
            TempLoadingData tempLoadingData
                = new(0);

            //Создаём списки для временных данных
            List<DContentSetDescription> contentSetDescriptions
                = new();
            List<string> contentSetNames
                = new();

            //Для каждого набора контента мастерской
            for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
            {
                //Если набор контента активен
                if (contentData.Value.wDContentSets[a].isActive
                    == true)
                {
                    //Если набор контента имеет описание
                    if (a 
                        < contentData.Value.wDContentSetDescriptions.Length)
                    {
                        //Заносим описание текущего набора контента во временный массив
                        contentSetDescriptions.Add(
                            new(
                                contentData.Value.wDContentSetDescriptions[a].ContentSetName,
                                contentData.Value.wDContentSetDescriptions[a].ContentSetVersion,
                                contentData.Value.wDContentSetDescriptions[a].GameVersion));
                    }

                    //Расширяем массив наборов контента
                    Array.Resize(
                        ref contentData.Value.contentSets,
                        contentData.Value.contentSets.Length + 1);

                    //Создаём пустой набор контента и заносим его в массив
                    contentData.Value.contentSets[contentData.Value.contentSets.Length - 1]
                        = new DContentSet(contentData.Value.wDContentSets[a].ContentSetName);

                    //Заносим название набора контента во временный список
                    contentSetNames.Add(
                        contentData.Value.contentSets[contentData.Value.contentSets.Length - 1].ContentSetName);

                    //Загружаем его данные из данных мастерской в данные игры
                    GameLoadContentSet(
                        ref contentData.Value.wDContentSets[a],
                        ref contentData.Value.contentSets[contentData.Value.contentSets.Length - 1],
                        ref tempLoadingData);

                    //Указываем в данных набора контента его индекс в игре
                    contentData.Value.wDContentSets[a].gameContentSetIndex
                        = contentData.Value.contentSets.Length - 1;
                }
            }

            //Рассчитываем ссылки объектов друг на друга
            GameContentObjectRefsCalculating();

            //Заполняем массив описаний наборов контента
            contentData.Value.contentSetDescriptions
                = contentSetDescriptions.ToArray();

            //Перезаписываем массив названий наборов контента
            contentData.Value.contentSetNames
                = contentSetNames.ToArray();

            //Очищаем массивы данных мастерской
            contentData.Value.wDContentSetDescriptions
                = new WDContentSetDescription[0];
            contentData.Value.wDContentSets
                = new WDContentSet[0];
        }

        //Данные для мастерской
        void WorkshopLoadContentSet(
            ref SDContentSet loadingContentSet,
            ref WDContentSet contentSet,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Если массив технологий не пуст
            if (loadingContentSet.technologies != null)
            {
                //Загружаем технологии во временный список
                WorkshopLoadTechnologies(
                    in loadingContentSet.technologies,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.technologies = tempLoadingWorkshopData.technologies.ToArray();


            //Если массив типов кораблей не пуст
            if (loadingContentSet.shipTypes != null)
            {
                //Загружаем типы во временный список
                WorkshopLoadShipTypes(
                    in loadingContentSet.shipTypes,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.shipTypes = tempLoadingWorkshopData.shipTypes.ToArray();

            //Если массив двигателей не пуст
            if (loadingContentSet.engines != null)
            {
                //Загружаем двигатели во временный список
                WorkshopLoadEngines(
                    in loadingContentSet.engines,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.engines = tempLoadingWorkshopData.engines.ToArray();

            //Если массив реакторов не пуст
            if (loadingContentSet.reactors != null)
            {
                //Загружаем реакторы во временный список
                WorkshopLoadReactors(
                    in loadingContentSet.reactors,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.reactors = tempLoadingWorkshopData.reactors.ToArray();

            //Если массив топливных баков не пуст
            if (loadingContentSet.fuelTanks != null)
            {
                //Загружаем топливные баки во временный список
                WorkshopLoadFuelTanks(
                    in loadingContentSet.fuelTanks,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.fuelTanks = tempLoadingWorkshopData.fuelTanks.ToArray();

            //Если массив оборудования для твёрдой добычи не пуст
            if (loadingContentSet.extractionEquipmentSolids != null)
            {
                //Загружаем оборудование для твёрдой добычи во временный список
                WorkshopLoadExtractionEquipments(
                    in loadingContentSet.extractionEquipmentSolids,
                    ref designerData.Value.solidExtractionEquipmentCoreModifierTypes,
                    tempLoadingWorkshopData.solidExtractionEquipments);
            }
            //Заполняем массив набора контента
            contentSet.solidExtractionEquipments = tempLoadingWorkshopData.solidExtractionEquipments.ToArray();

            //Если массив энергетических орудий не пуст
            if (loadingContentSet.energyGuns != null)
            {
                //Загружаем энергетические орудия во временный список
                WorkshopLoadEnergyGuns(
                    in loadingContentSet.energyGuns,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.energyGuns = tempLoadingWorkshopData.energyGuns.ToArray();


            //Если массив классов кораблей не пуст
            if (loadingContentSet.shipClasses != null)
            {
                //Загружаем классы кораблей во временный список
                WorkshopLoadShipClasses(
                    in loadingContentSet.shipClasses,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.shipClasses = tempLoadingWorkshopData.shipClasses.ToArray();

            //Очищаем временные списки
            tempLoadingWorkshopData.technologies.Clear();

            tempLoadingWorkshopData.shipTypes.Clear();

            tempLoadingWorkshopData.engines.Clear();
            tempLoadingWorkshopData.reactors.Clear();
            tempLoadingWorkshopData.fuelTanks.Clear();
            tempLoadingWorkshopData.solidExtractionEquipments.Clear();
            tempLoadingWorkshopData.energyGuns.Clear();

            tempLoadingWorkshopData.shipClasses.Clear();
        }

        void WorkshopContentObjectRefsCalculating(
            int startIndex)
        {
            //Для каждого набора в массиве данных мастерской
            for (int a = startIndex; a < contentData.Value.wDContentSets.Length; a++)
            {
                //Берём ссылку на набор контента
                ref WDContentSet contentSet
                    = ref contentData.Value
                    .wDContentSets[a];

                //Рассчитываем ссылки двигателей
                WorkshopRefCalculatingEngines(
                    //ref contentSet,
                    ref contentSet.engines,
                    a);

                //Рассчитываем ссылки реакторов
                WorkshopRefCalculatingReactors(
                    //ref contentSet,
                    ref contentSet.reactors,
                    a);

                //Рассчитываем ссылки топливных баков
                WorkshopRefCalculatingFuelTanks(
                    ref contentSet.fuelTanks,
                    a);

                //Рассчитываем ссылки оборудования для твёрдой добычи
                WorkshopRefCalculatingExtractionEquipments(
                    ref contentSet.solidExtractionEquipments,
                    ShipComponentType.ExtractionEquipmentSolid,
                    a);

                //Рассчитываем ссылки энергетических орудий
                WorkshopRefCalculatingEnergyGuns(
                    ref contentSet.energyGuns,
                    a);
            }
            
            //Для каждого набора в массиве данных мастерской
            for (int a = startIndex; a < contentData.Value.wDContentSets.Length; a++)
            {
                //Берём ссылку на набор контента
                ref WDContentSet contentSet
                    = ref contentData.Value
                    .wDContentSets[a];

                //Рассчитываем ссылки классов кораблей
                WorkshopRefCalculatingShipClasses(
                    ref contentSet.shipClasses,
                    a);
            }
        }

        //Технологии
        void WorkshopLoadTechnologies(
            in SDTechnology[] loadingTechnologies,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Создаём списки для временных данных
            List<WDTechnologyComponentCoreModifier> technologyComponentCoreModifiers
                = new();
            List<WDTechnologyModifier> technologyModifiers
                = new();

            //Для каждой загружаемой технологии
            for (int a = 0; a < loadingTechnologies.Length; a++)
            {
                //Берём ссылку на загружаемую технологию
                ref readonly SDTechnology loadingTechnology
                    = ref loadingTechnologies[a];

                //Очищаем временный список модификаторов
                technologyModifiers.Clear();
                //Для каждого модификатора технологии
                for (int b = 0; b < loadingTechnology.technologyModifiers.Length; b++)
                {
                    //Создаём переменную для определения типа модификатора 
                    TechnologyModifierType modifierType
                        = TechnologyModifierType.None;

                    //Для каждого названия в массиве названий модификаторов технологий
                    for (int c = 0; c < contentData.Value.technologyModifiersNames.Length; c++)
                    {
                        //Если название модификатора совпадает 
                        if (loadingTechnology.technologyModifiers[b].modifierName
                            == contentData.Value.technologyModifiersNames[c])
                        {
                            //Указываем, что модификатор имеет тип, соответствующий текущему индексу "c"
                            modifierType
                                = (TechnologyModifierType)c;
                        }
                    }

                    //Если тип модификатора был определён
                    if (modifierType
                        != TechnologyModifierType.None)
                    {
                        //Записываем загруженные данные модификатора
                        WDTechnologyModifier modifier
                            = new(
                                loadingTechnology.technologyModifiers[b].modifierName,
                                modifierType,
                                loadingTechnology.technologyModifiers[b].modifierValue);

                        //Заносим его во временный список
                        technologyModifiers.Add(
                            modifier);
                    }
                    //Иначе
                    else
                    {
                        //Записываем модификатор как ошибочный
                        WDTechnologyModifier modifier
                            = new(
                                loadingTechnology.technologyModifiers[b].modifierName,
                                TechnologyModifierType.None,
                                0);

                        //Заносим его во временный список
                        technologyModifiers.Add(
                            modifier);
                    }
                }

                //Очищаем временный список основных модификаторов компонентов
                technologyComponentCoreModifiers.Clear();
                //Для каждого основного модификатора компонента
                for (int b = 0; b < loadingTechnology.technologyComponentCoreModifiers.Length; b++)
                {
                    //Определяем тип модификатора
                    TechnologyComponentCoreModifierType modifierType
                        = TechnologyDefineComponentCoreModifierType(
                            loadingTechnology.technologyComponentCoreModifiers[b].modifierName);

                    //Если тип модификатора был определён
                    if (modifierType
                        != TechnologyComponentCoreModifierType.None)
                    {
                        //Записываем загруженные данные модификатора
                        WDTechnologyComponentCoreModifier modifier
                            = new(
                                loadingTechnology.technologyComponentCoreModifiers[b].modifierName,
                                modifierType,
                                loadingTechnology.technologyComponentCoreModifiers[b].modifierValue);

                        //Заносим его во временный список
                        technologyComponentCoreModifiers.Add(
                            modifier);
                    }
                    //Иначе
                    else
                    {
                        //Записываем модификатор как ошибочный
                        WDTechnologyComponentCoreModifier modifier
                            = new(
                                loadingTechnology.technologyComponentCoreModifiers[b].modifierName,
                                TechnologyComponentCoreModifierType.None,
                                0);

                        //Заносим его во временный список
                        technologyComponentCoreModifiers.Add(
                            modifier);
                    }
                }

                //Записываем загруженные данные технологии
                WDTechnology technology
                    = new(
                        loadingTechnology.technologyName,
                        loadingTechnology.isBaseTechnology,
                        technologyModifiers.ToArray(),
                        technologyComponentCoreModifiers.ToArray());

                //Заносим её во временный список
                tempLoadingWorkshopData.technologies.Add(
                    technology);
            }
        }

        //Типы кораблей
        void WorkshopLoadShipTypes(
            in SDShipType[] loadingShipTypes,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Для каждого загружаемого типа корабля
            for (int a = 0; a < loadingShipTypes.Length; a++)
            {
                //Берём ссылку на загружаемый тип
                ref readonly SDShipType loadingShipType = ref loadingShipTypes[a];

                //Определяем боевую группу, к которой относится тип
                TaskForceBattleGroup taskForceBattleGroup = ShipTypeDefineTaskForceBattleGroup(loadingShipType.battleGroupName);

                //Записываем загруженные данные типа
                WDShipType shipType = new(
                    loadingShipType.shipTypeName,
                    taskForceBattleGroup);

                //Заносим его во временный список
                tempLoadingWorkshopData.shipTypes.Add(shipType);
            }
        }

        //Компоненты кораблей
        void WorkshopLoadEngines(
            in SDEngine[] loadingEngines,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Создаём списки для временных данных
            WDComponentCoreTechnology[] coreTechnologies;

            //Для каждого загружаемого двигателя
            for (int a = 0; a < loadingEngines.Length; a++)
            {
                //Берём ссылку загружаемый двигатель
                ref readonly SDEngine loadingEngine
                    = ref loadingEngines[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new WDComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length];

                //Загружаем основные технологии двигателя
                WorkshopLoadComponentCoreTechnologies(
                    in loadingEngine.coreTechnologies,
                    ref designerData.Value.engineCoreModifierTypes,
                    ref coreTechnologies);

                //Записываем загруженные данные двигателя
                WDEngine engine
                    = new(
                        loadingEngine.modelName,
                        coreTechnologies,
                        loadingEngine.engineSize,
                        loadingEngine.engineBoost);

                //Заносим его во временный список
                tempLoadingWorkshopData.engines.Add(
                    engine);
            }
        }

        void WorkshopLoadReactors(
            in SDReactor[] loadingReactors,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Создаём списки для временных данных
            WDComponentCoreTechnology[] coreTechnologies;

            //Для каждого загружаемого реактора
            for (int a = 0; a < loadingReactors.Length; a++)
            {
                //Берём ссылку на загружаемый реактор
                ref readonly SDReactor loadingReactor
                    = ref loadingReactors[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new WDComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length];

                //Загружаем основные технологии реактора
                WorkshopLoadComponentCoreTechnologies(
                    in loadingReactor.coreTechnologies,
                    ref designerData.Value.reactorCoreModifierTypes,
                    ref coreTechnologies);

                //Записываем загруженные данные реактора
                WDReactor reactor
                    = new(
                        loadingReactor.modelName,
                        coreTechnologies,
                        loadingReactor.reactorSize,
                        loadingReactor.reactorBoost);

                //Заносим его во временный список
                tempLoadingWorkshopData.reactors.Add(
                    reactor);
            }
        }

        void WorkshopLoadFuelTanks(
            in SDHoldFuelTank[] loadingFuelTanks,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Создаём списки для временных данных
            WDComponentCoreTechnology[] coreTechnologies;

            //Для каждого загружаемого топливного бака
            for (int a = 0; a < loadingFuelTanks.Length; a++)
            {
                //Берём ссылку на загружаемый топливный бак
                ref readonly SDHoldFuelTank loadingFuelTank
                    = ref loadingFuelTanks[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new WDComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length];

                //Загружаем основные технологии топливного бака
                WorkshopLoadComponentCoreTechnologies(
                    in loadingFuelTank.coreTechnologies,
                    ref designerData.Value.fuelTankCoreModifierTypes,
                    ref coreTechnologies);

                //Записываем загруженные данные топливного бака
                WDHoldFuelTank fuelTank
                    = new(
                        loadingFuelTank.modelName,
                        coreTechnologies,
                        loadingFuelTank.fuelTankSize);

                //Заносим его во временный список
                tempLoadingWorkshopData.fuelTanks.Add(
                    fuelTank);
            }
        }

        void WorkshopLoadExtractionEquipments(
            in SDExtractionEquipment[] loadingExtractionEquipments,
            ref TechnologyComponentCoreModifierType[] extractionEquipmentCoreModifierTypes,
            List<WDExtractionEquipment> extractionEquipments)
        {
            //Создаём списки для временных данных
            WDComponentCoreTechnology[] coreTechnologies;

            //Для каждого загружаемого оборудования для добычи
            for (int a = 0; a < loadingExtractionEquipments.Length; a++)
            {
                //Берём ссылку на загружаемое оборудование для добычи
                ref readonly SDExtractionEquipment loadingExtractionEquipment
                    = ref loadingExtractionEquipments[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new WDComponentCoreTechnology[extractionEquipmentCoreModifierTypes.Length];

                //Загружаем основные оборудования для добычи
                WorkshopLoadComponentCoreTechnologies(
                    in loadingExtractionEquipment.coreTechnologies,
                    ref extractionEquipmentCoreModifierTypes,
                    ref coreTechnologies);

                //Записываем загруженные данные оборудования для добычи
                WDExtractionEquipment extractionEquipment
                    = new(
                        loadingExtractionEquipment.modelName,
                        coreTechnologies,
                        loadingExtractionEquipment.size);

                //Заносим его во временный список
                extractionEquipments.Add(
                    extractionEquipment);
            }
        }

        void WorkshopLoadEnergyGuns(
            in SDGunEnergy[] loadingEnergyGuns,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Создаём списки для временных данных
            WDComponentCoreTechnology[] coreTechnologies;

            //Для каждого загружаемого энергетического орудия
            for (int a = 0; a < loadingEnergyGuns.Length; a++)
            {
                //Берём ссылку на загружаемое энергетическое орудие
                ref readonly SDGunEnergy loadingEnergyGun
                    = ref loadingEnergyGuns[a];

                //Очищаем временный массив основных технологий
                coreTechnologies
                    = new WDComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length];

                //Загружаем основные технологии энергетического орудия
                WorkshopLoadComponentCoreTechnologies(
                    in loadingEnergyGun.coreTechnologies,
                    ref designerData.Value.energyGunCoreModifierTypes,
                    ref coreTechnologies);

                //Записываем загруженные данные энергетического орудия
                WDGunEnergy energyGun
                    = new(
                        loadingEnergyGun.modelName,
                        coreTechnologies,
                        loadingEnergyGun.gunCaliber,
                        loadingEnergyGun.gunBarrelLength);

                //Заносим его во временный список
                tempLoadingWorkshopData.energyGuns.Add(
                    energyGun);
            }
        }

        void WorkshopLoadComponentCoreTechnologies(
            in SDComponentCoreTechnology[] loadingCoreTechnologies,
            ref TechnologyComponentCoreModifierType[] coreModifiersTypes,
            ref WDComponentCoreTechnology[] coreTechnologies)
        {
            //Создаём списки для временных данных
            List<WDComponentCoreTechnology> tempCoreTechnologies
                = new();

            //Для каждой загружаемой основной технологии
            for (int a = 0; a < loadingCoreTechnologies.Length; a++)
            {
                //Определяем тип модификатора
                TechnologyComponentCoreModifierType modifierType
                    = TechnologyDefineComponentCoreModifierType(
                        loadingCoreTechnologies[a].modifierName);

                //Записываем базовые данные основной технологии
                WDComponentCoreTechnology coreTechnology
                    = new(
                        loadingCoreTechnologies[a].modifierName,
                        modifierType,
                        loadingCoreTechnologies[a].contentSetName,
                        loadingCoreTechnologies[a].technologyName);

                //Заносим её во временный список
                tempCoreTechnologies.Add(
                    coreTechnology);
            }

            //Для каждой загруженный основной технологии
            for (int a = 0; a < tempCoreTechnologies.Count; a++)
            {
                //Для каждого модификатора в переданном списке
                for (int b = 0; b < coreModifiersTypes.Length; b++)
                {
                    //Если тип модификатора совпадает
                    if (tempCoreTechnologies[a].modifierType
                        == coreModifiersTypes[b])
                    {
                        //Заносим технологию на соответствующую позицию итогового массива
                        coreTechnologies[b]
                            = tempCoreTechnologies[a];

                        //Выходим из цикла
                        break;
                    }
                }
            }
        }

        void WorkshopRefCalculatingEngines(
            //ref WDContentSet contentSet,
            ref WDEngine[] engines,
            int contentSetIndex)
        {
            //Для каждого двигателя
            for (int a = 0; a < engines.Length; a++)
            {
                //Берём ссылку на двигатель
                ref WDEngine engine
                    = ref engines[a];

                //Рассчитываем ссылки основных технологий двигателя
                WorkshopRefCalculatingCoreTechnologies(
                    ref engine.coreTechnologies,
                    ShipComponentType.Engine,
                    contentSetIndex,
                    a);

                //Рассчитываем характеристики двигателя
                engine.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingReactors(
            //ref WDContentSet contentSet,
            ref WDReactor[] reactors,
            int contentSetIndex)
        {
            //Для каждого реактора
            for (int a = 0; a < reactors.Length; a++)
            {
                //Берём ссылку на реактор
                ref WDReactor reactor
                    = ref reactors[a];

                //Рассчитываем ссылки основных технологий реактора
                WorkshopRefCalculatingCoreTechnologies(
                    ref reactor.coreTechnologies,
                    ShipComponentType.Reactor,
                    contentSetIndex,
                    a);


                //Рассчитываем характеристики реактора
                reactor.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingFuelTanks(
            ref WDHoldFuelTank[] fuelTanks,
            int contentSetIndex)
        {
            //Для каждого топливного бака
            for (int a = 0; a < fuelTanks.Length; a++)
            {
                //Берём ссылку на топливный бак
                ref WDHoldFuelTank fuelTank
                    = ref fuelTanks[a];

                //Рассчитываем ссылки основных технологий топливного бака
                WorkshopRefCalculatingCoreTechnologies(
                    ref fuelTank.coreTechnologies,
                    ShipComponentType.HoldFuelTank,
                    contentSetIndex,
                    a);

                //Рассчитываем характеристики топливного бака
                fuelTank.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingExtractionEquipments(
            ref WDExtractionEquipment[] extractionEquipments,
            ShipComponentType extractionEquipmentType,
            int contentSetIndex)
        {
            //Для каждого оборудования для добычи
            for (int a = 0; a < extractionEquipments.Length; a++)
            {
                //Берём ссылку на оборудование для добычи
                ref WDExtractionEquipment extractionEquipment
                    = ref extractionEquipments[a];

                //Рассчитываем ссылки основных технологий оборудования для добычи
                WorkshopRefCalculatingCoreTechnologies(
                    ref extractionEquipment.coreTechnologies,
                    extractionEquipmentType,
                    contentSetIndex,
                    a);

                //Рассчитываем характеристики оборудования для добычи
                extractionEquipment.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingEnergyGuns(
            ref WDGunEnergy[] energyGuns,
            int contentSetIndex)
        {
            //Для каждого энергетического орудия
            for (int a = 0; a < energyGuns.Length; a++)
            {
                //Берём ссылку на энергетическое орудие
                ref WDGunEnergy energyGun
                    = ref energyGuns[a];

                //Рассчитываем ссылки основных технологий энергетического орудия
                WorkshopRefCalculatingCoreTechnologies(
                    ref energyGun.coreTechnologies,
                    ShipComponentType.GunEnergy,
                    contentSetIndex,
                    a);

                //Рассчитываем характеристики энергетического орудия
                energyGun.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingCoreTechnologies(
            ref WDComponentCoreTechnology[] coreTechnologies,
            ShipComponentType componentType,
            int componentContentSetIndex,
            int componentIndex)
        {
            //Для каждой основной технологии компонента
            for (int a = 0; a < coreTechnologies.Length; a++)
            {
                //Берём ссылку на основную технологию
                ref WDComponentCoreTechnology coreTechnology
                    = ref coreTechnologies[a];

                //Создаём переменную для отслеживания, найден ли искомый набор контента
                bool isContentSetFinded
                    = false;

                //Создаём переменную для отслеживания индекса набора контента
                int contentSetIndex
                    = -1;

                //Для каждого набора контента
                for (int b = 0; b < contentData.Value.wDContentSets.Length; b++)
                {
                    //Если название набора контента соответствует искомому
                    if (contentData.Value.wDContentSets[b].ContentSetName
                        == coreTechnology.contentSetName)
                    {
                        //Указываем индекс искомого набора контента
                        contentSetIndex
                            = b;

                        //Указываем, что набор контента был найден
                        isContentSetFinded
                            = true;

                        //Выходим из цикла
                        break;
                    }
                }

                //Если набор контента был найден
                if (isContentSetFinded
                    == true)
                {
                    //Создаём переменную для отслеживания, найдена ли искомая технология
                    bool isTechnologyFinded
                        = false;

                    //Создаём переменную для отслеживания индекса технологии
                    int technologyIndex
                        = -1;

                    //Создаём переменную для отслеживания значения модификатора технологии
                    float technologyModifierValue
                        = 0;

                    //Для каждой технологии в найденном наборе контента
                    for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].technologies.Length; b++)
                    {
                        //Если название технологии соответствует искомому
                        if (contentData.Value.wDContentSets[contentSetIndex].technologies[b].ObjectName
                            == coreTechnology.technologyName)
                        {
                            //Для каждого основного модификатора компонента
                            for (int c = 0; c < contentData.Value.wDContentSets[contentSetIndex].technologies[b].technologyComponentCoreModifiers.Length; c++)
                            {
                                //Если тип модификатора соответствует искомому
                                if (contentData.Value.wDContentSets[contentSetIndex].technologies[b].technologyComponentCoreModifiers[c].ModifierType
                                    == coreTechnology.modifierType)
                                {
                                    //Указываем в данных технологии, что на неё ссылается текущий объект
                                    //Если тип компонента - двигатель
                                    if (componentType
                                        == ShipComponentType.Engine)
                                    {
                                        //Заносим ссылку на него в список двигателей, ссылающихся на технологию
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].engines.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }
                                    //Иначе, если тип компонента - реактор
                                    else if(componentType
                                        == ShipComponentType.Reactor)
                                    {
                                        //Заносим ссылку на него в список реакторов, ссылающихся на технологию
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].reactors.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }
                                    //Иначе, если тип компонента - топливный бак
                                    else if(componentType
                                        == ShipComponentType.HoldFuelTank)
                                    {
                                        //Заносим ссылку на него в список топливных баков, ссылающихся на технологию
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].fuelTanks.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }
                                    //Иначе, если тип компонента - оборудование для твёрдой добычи
                                    else if (componentType
                                        == ShipComponentType.ExtractionEquipmentSolid)
                                    {
                                        //Заносим ссылку на него в список оборудования для твёрдой добычи, ссылающихся на технологию
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].extractionEquipmentSolids.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }
                                    //Иначе, если тип компонента - энергетическое орудие
                                    else if (componentType
                                        == ShipComponentType.GunEnergy)
                                    {
                                        //Заносим ссылку на него в список энергетических орудий, ссылающихся на технологию
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].energyGuns.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }

                                    //Указываем индекс искомой технологии
                                    technologyIndex
                                        = b;

                                    //Указываем, что технология была найдена
                                    isTechnologyFinded
                                        = true;

                                    //Сохраняем значение модификатора
                                    technologyModifierValue
                                        = contentData.Value
                                        .wDContentSets[contentSetIndex]
                                        .technologies[b]
                                        .technologyComponentCoreModifiers[c].ModifierValue;

                                    //Выходим из цикла
                                    break;
                                }
                            }

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если технология была найдена
                    if (isTechnologyFinded
                        == true)
                    {
                        //Перезаписываем данные основной технологии
                        coreTechnologies[a]
                            = new(
                                coreTechnology.modifierName,
                                coreTechnology.modifierType,
                                coreTechnology.contentSetName,
                                coreTechnology.technologyName,
                                new(contentSetIndex, technologyIndex),
                                true,
                                technologyModifierValue);
                    }
                    //Иначе
                    else
                    {
                        //Перезаписываем данные основной технологии, указывая, что ссылка ошибочна, но оставляя ссылку на набор контента
                        coreTechnologies[a]
                            = new(
                                coreTechnology.modifierName,
                                coreTechnology.modifierType,
                                coreTechnology.contentSetName,
                                coreTechnology.technologyName,
                                new(contentSetIndex, -1),
                                false,
                                0);
                    }
                }
                //Иначе
                else
                {
                    //Перезаписываем данные основной технологии, указывая, что ссылка ошибочна
                    coreTechnologies[a]
                        = new(
                            coreTechnology.modifierName,
                            coreTechnology.modifierType,
                            coreTechnology.contentSetName,
                            coreTechnology.technologyName,
                            new(-1, -1),
                            false,
                            0);
                }
            }
        }

        //Классы кораблей
        void WorkshopLoadShipClasses(
            in SDShipClass[] loadingShipClasses,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Создаём списки для временных данных
            List<WDShipClassComponent> shipClassEngines
                = new();
            List<WDShipClassComponent> shipClassReactors
                = new();
            List<WDShipClassComponent> shipClassFuelTanks
                = new();
            List<WDShipClassComponent> shipClassExtractionEquipmentSolids
                = new();
            List<WDShipClassComponent> shipClassEnergyGuns
                = new();

            //Для каждого загружаемого класса
            for (int a = 0; a < loadingShipClasses.Length; a++)
            {
                //Берём ссылку на загружаемый класс
                ref readonly SDShipClass loadingShipClass
                    = ref loadingShipClasses[a];

                //Если массив двигателей не пуст
                if (loadingShipClass.engines
                    != null)
                {
                    //Очищаем временный список двигателей
                    shipClassEngines.Clear();
                    //Загружаем двигатели корабля
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.engines,
                        shipClassEngines);
                }
                
                //Если массив реакторов не пуст
                if (loadingShipClass.reactors
                    != null)
                {
                    //Очищаем временный список реакторов
                    shipClassReactors.Clear();
                    //Загружаем реакторы корабля
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.reactors,
                        shipClassReactors);
                }

                //Если массив топливных баков не пуст
                if (loadingShipClass.fuelTanks
                    != null)
                {
                    //Очищаем временный список топливных баков
                    shipClassFuelTanks.Clear();
                    //Загружаем топливные баки корабля
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.fuelTanks,
                        shipClassFuelTanks);
                }

                //Если массив оборудования для твёрдой добычи не пуст
                if (loadingShipClass.extractionEquipmentSolids
                    != null)
                {
                    //Очищаем временный список оборудования для твёрдой добычи
                    shipClassExtractionEquipmentSolids.Clear();
                    //Загружаем оборудование для твёрдой добычи корабля
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.extractionEquipmentSolids,
                        shipClassExtractionEquipmentSolids);
                }

                //Если массив энергетических орудий не пуст
                if (loadingShipClass.energyGuns
                    != null)
                {
                    //Очищаем временный список энергетических орудий
                    shipClassEnergyGuns.Clear();
                    //Загружаем энергетические орудия корабля
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.energyGuns,
                        shipClassEnergyGuns);
                }

                //Записываем загруженные данные класса корабля
                WDShipClass shipClass
                    = new(
                        loadingShipClass.className,
                        shipClassEngines.ToArray(),
                        shipClassReactors.ToArray(),
                        shipClassFuelTanks.ToArray(),
                        shipClassExtractionEquipmentSolids.ToArray(),
                        shipClassEnergyGuns.ToArray());

                //Заносим загруженные данные во временный список
                tempLoadingWorkshopData.shipClasses.Add(
                    shipClass);
            }
        }

        void WorkshopLoadShipClassComponents(
            in SDShipClassComponent[] loadingShipClassComponents,
            List<WDShipClassComponent> shipClassComponents)
        {
            //Для каждого загружаемого компонента
            for (int a = 0; a < loadingShipClassComponents.Length; a++)
            {
                //Записываем базовые данные компонента корабля
                WDShipClassComponent shipClassComponent
                    = new(
                        loadingShipClassComponents[a].contentSetName,
                        loadingShipClassComponents[a].componentName,
                        loadingShipClassComponents[a].numberOfComponents);

                //Заносим его в список
                shipClassComponents.Add(
                    shipClassComponent);
            }
        }

        void WorkshopRefCalculatingShipClasses(
            ref WDShipClass[] shipClasses,
            int contentSetIndex)
        {
            //Для каждого класса корабля
            for (int a = 0; a < shipClasses.Length; a++)
            {
                //Берём ссылку на класс корабля
                ref WDShipClass shipClass
                    = ref shipClasses[a];

                //Рассчитываем ссылки на двигатели
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.engines,
                    contentSetIndex,
                    a,
                    ShipComponentType.Engine);

                //Рассчитываем ссылки на реакторы
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.reactors,
                    contentSetIndex,
                    a,
                    ShipComponentType.Reactor);

                //Рассчитываем ссылки на топливные баки
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.fuelTanks,
                    contentSetIndex,
                    a,
                    ShipComponentType.HoldFuelTank);

                //Рассчитываем ссылки на оборудование для твёрдой добычи
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.extractionEquipmentSolids,
                    contentSetIndex,
                    a,
                    ShipComponentType.ExtractionEquipmentSolid);

                //Рассчитываем ссылки на энергетические орудия
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.energyGuns,
                    contentSetIndex,
                    a,
                    ShipComponentType.GunEnergy);

                //Рассчитываем характеристики класса корабля
                shipClass.CalculateCharacteristics(
                    contentData.Value);
            }
        }

        void WorkshopRefCalculatingShipClassComponents(
            ref WDShipClassComponent[] shipClassComponents,
            int shipClassContentSetIndex,
            int shipClassIndex,
            ShipComponentType componentType)
        {
            //Для каждого компонента корабля
            for (int a = 0; a < shipClassComponents.Length; a++)
            {
                //Берём ссылку на компонент
                ref WDShipClassComponent shipClassComponent
                    = ref shipClassComponents[a];

                //Создаём переменную для отслеживания, найден ли искомый набор контента
                bool isContentSetFinded
                    = false;

                //Создаём переменную для отслеживания индекса набора контента
                int contentSetIndex
                    = -1;

                //Для каждого набора контента
                for (int b = 0; b < contentData.Value.wDContentSets.Length; b++)
                {
                    //Если название набора контента соответствует искомому
                    if (contentData.Value.wDContentSets[b].ContentSetName
                        == shipClassComponent.contentSetName)
                    {
                        //Указываем индекс искомого набора контента
                        contentSetIndex
                            = b;

                        //Указываем, что набор контента был найден
                        isContentSetFinded
                            = true;

                        //Выходим из цикла
                        break;
                    }
                }

                //Если наборк контента был найден
                if (isContentSetFinded
                    == true)
                {
                    //Создаём переменную для отслеживания, найден ли искомый компонент
                    bool isComponentFinded
                        = false;

                    //Создаём переменную для отслеживания индекса компонента
                    int componentIndex
                        = -1;

                    //Если тип компонента - двигатель
                    if (componentType
                        == ShipComponentType.Engine)
                    {
                        //Для каждого двигателя в найденном наборе контента
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].engines.Length; b++)
                        {
                            //Если название двигателя соответствует искомому
                            if (contentData.Value.wDContentSets[contentSetIndex].engines[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //Указываем в данных компонента, что на него ссылается текущий класс корабля
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .engines[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //Указываем индекс искомого компонента
                                componentIndex
                                    = b;

                                //Указываем, что компонент был найден
                                isComponentFinded
                                    = true;

                                //Выходим из цикла
                                break;
                            }
                        }
                    }
                    //Иначе, если тип компонента - реактор
                    else if (componentType
                        == ShipComponentType.Reactor)
                    {
                        //Для каждого реактора в найденном наборе контента
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].reactors.Length; b++)
                        {
                            //Если название реактора соответствует искомому
                            if (contentData.Value.wDContentSets[contentSetIndex].reactors[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //Указываем в данных компонента, что на него ссылается текущий класс корабля
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .reactors[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //Указываем индекс искомого компонента
                                componentIndex
                                    = b;

                                //Указываем, что компонент был найден
                                isComponentFinded
                                    = true;

                                //Выходим из цикла
                                break;
                            }
                        }
                    }
                    //Иначе, если тип компонента - топливный бак
                    else if (componentType
                        == ShipComponentType.HoldFuelTank)
                    {
                        //Для каждого топливного бака в найденном наборе контента
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length; b++)
                        {
                            //Если название топливного бака соответствует искомому
                            if (contentData.Value.wDContentSets[contentSetIndex].fuelTanks[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //Указываем в данных компонента, что на него ссылается текущий класс корабля
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .fuelTanks[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //Указываем индекс искомого компонента
                                componentIndex
                                    = b;

                                //Указываем, что компонент был найден
                                isComponentFinded
                                    = true;

                                //Выходим из цикла
                                break;
                            }
                        }
                    }
                    //Иначе, если тип компонента - оборудование для твёрдой добычи
                    else if (componentType
                        == ShipComponentType.ExtractionEquipmentSolid)
                    {
                        //Для каждого оборудования для твёрдой добычи в найденном наборе контента
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length; b++)
                        {
                            //Если название оборудования для твёрдой добычи соответствует искомому
                            if (contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //Указываем в данных компонента, что на него ссылается текущий класс корабля
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .solidExtractionEquipments[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //Указываем индекс искомого компонента
                                componentIndex
                                    = b;

                                //Указываем, что компонент был найден
                                isComponentFinded
                                    = true;

                                //Выходим из цикла
                                break;
                            }
                        }
                    }
                    //Иначе, если тип компонента - энергетическое орудие
                    else if (componentType
                        == ShipComponentType.GunEnergy)
                    {
                        //Для каждого энергетического орудия в найденном наборе контента
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length; b++)
                        {
                            //Если название энергетического орудия соответствует искомому
                            if (contentData.Value.wDContentSets[contentSetIndex].energyGuns[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //Указываем в данных компонента, что на него ссылается текущий класс корабля
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .energyGuns[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //Указываем индекс искомого компонента
                                componentIndex
                                    = b;

                                //Указываем, что компонент был найден
                                isComponentFinded
                                    = true;

                                //Выходим из цикла
                                break;
                            }
                        }
                    }

                    //Если компонент был найден
                    if (isComponentFinded
                        == true)
                    {
                        //Перезаписываем данные компонента
                        shipClassComponents[a]
                            = new(
                                shipClassComponent.contentSetName,
                                shipClassComponent.componentName,
                                shipClassComponent.numberOfComponents,
                                contentSetIndex,
                                componentIndex,
                                true);
                    }
                    //Иначе
                    else
                    {
                        //Перезаписываем данные компонента, указывая, что ссылка ошибочна, но оставляя ссылку на набор контента
                        shipClassComponents[a]
                            = new(
                                shipClassComponent.contentSetName,
                                shipClassComponent.componentName,
                                shipClassComponent.numberOfComponents,
                                contentSetIndex,
                                -1,
                                false);
                    }
                }
                //Иначе
                else
                {
                    //Перезаписываем данные компонента, указывая, что ссылка ошибочна
                    shipClassComponents[a]
                        = new(
                            shipClassComponent.contentSetName,
                            shipClassComponent.componentName,
                            shipClassComponent.numberOfComponents,
                            contentSetIndex,
                            -1,
                            false);
                }
            }
        }

        //Данные для игры
        void GameLoadContentSet(
            ref WDContentSet loadingContentSet,
            ref DContentSet contentSet,
            ref TempLoadingData tempLoadingData)
        {
            //Если массив технологий не пуст
            if (loadingContentSet.technologies != null)
            {
                //Загружаем технологии во временный список
                GameLoadTechnologies(
                    ref loadingContentSet.technologies,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.technologies = tempLoadingData.technologies.ToArray();


            //Если массив типов кораблей не пуст
            if (loadingContentSet.shipTypes != null)
            {
                //Загружаем типы во временный список
                GameLoadShipTypes(
                    ref loadingContentSet.shipTypes,
                    ref tempLoadingData);
            }
            //Заполняем массив типов
            contentSet.shipTypes = tempLoadingData.shipTypes.ToArray();

            //Если массив двигателей не пуст
            if (loadingContentSet.engines != null)
            {
                //Загружаем двигатели во временный список
                GameLoadEngines(
                    ref loadingContentSet.engines,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.engines = tempLoadingData.engines.ToArray();

            //Если массив реакторов не пуст
            if (loadingContentSet.reactors != null)
            {
                //Загружаем реакторы во временный список
                GameLoadReactors(
                    ref loadingContentSet.reactors,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.reactors = tempLoadingData.reactors.ToArray();

            //Если массив топливных баков не пуст
            if (loadingContentSet.fuelTanks != null)
            {
                //Загруаем топливные баки во временный список
                GameLoadFuelTanks(
                    ref loadingContentSet.fuelTanks,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.fuelTanks = tempLoadingData.fuelTanks.ToArray();

            //Если массив оборудования для твёрдой добычи не пуст
            if (loadingContentSet.solidExtractionEquipments != null)
            {
                //Загруаем оборудование для твёрдой добычи во временный список
                GameLoadExtractionEquipments(
                    ref loadingContentSet.solidExtractionEquipments,
                    ref designerData.Value.solidExtractionEquipmentCoreModifierTypes,
                    tempLoadingData.solidExtractionEquipments);
            }
            //Заполняем массив набора контента
            contentSet.solidExtractionEquipments = tempLoadingData.solidExtractionEquipments.ToArray();

            //Если массив энергетических орудий не пуст
            if (loadingContentSet.energyGuns != null)
            {
                //Загруаем энергетические орудия во временный список
                GameLoadEnergyGuns(
                    ref loadingContentSet.energyGuns,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.energyGuns = tempLoadingData.energyGuns.ToArray();


            //Если массив классов кораблей не пуст
            if (loadingContentSet.shipClasses != null)
            {
                //Загружаем классы кораблей во временный список
                GameLoadShipClasses(
                    ref loadingContentSet.shipClasses,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.shipClasses = tempLoadingData.shipClasses.ToArray();

            //Очищаем временные списки
            tempLoadingData.technologies.Clear();


            tempLoadingData.shipTypes.Clear();

            tempLoadingData.engines.Clear();
            tempLoadingData.reactors.Clear();
            tempLoadingData.fuelTanks.Clear();
            tempLoadingData.solidExtractionEquipments.Clear();
            tempLoadingData.energyGuns.Clear();

            tempLoadingData.shipClasses.Clear();
        }

        void GameContentObjectRefsCalculating()
        {
            //Для каждого набора контента в массиве данных игры
            for (int a = 0; a < contentData.Value.contentSets.Length; a++)
            {
                //Берём ссылку на набор контента
                ref DContentSet contentSet
                    = ref contentData.Value
                    .contentSets[a];

                //Рассчитываем ссылки двигателей
                GameRefCalculatingEngines(
                    ref contentSet.engines,
                    a);

                //Рассчитываем ссылки реакторов
                GameRefCalculatingReactors(
                    ref contentSet.reactors,
                    a);

                //Рассчитываем ссылки топливных баков
                GameRefCalculatingFuelTanks(
                    ref contentSet.fuelTanks,
                    a);

                //Рассчитываем ссылки оборудования для твёрдой добычи
                GameRefCalculatingExtractionEquipment(
                    ref contentSet.solidExtractionEquipments,
                    ShipComponentType.ExtractionEquipmentSolid,
                    a);

                //Рассчитываем ссылки энергетических орудий
                GameRefCalculatingEnergyGuns(
                    ref contentSet.energyGuns,
                    a);
            }

            //Для каждого набора контента в массиве данных игры
            for (int a = 0; a < contentData.Value.contentSets.Length; a++)
            {
                //Берём ссылку на набор контента
                ref DContentSet contentSet
                    = ref contentData.Value
                    .contentSets[a];

                //Рассчитываем ссылки классов кораблей
                GameRefCalculatingShipClasses(
                    ref contentSet.shipClasses,
                    a);
            }
        }

        //Технологии
        void GameLoadTechnologies(
            ref WDTechnology[] loadingTechnologies,
            ref TempLoadingData tempLoadingData)
        {
            //Создаём списки для временных данных
            List<DTechnologyComponentCoreModifier> technologyComponentCoreModifiers
                = new();
            List<DTechnologyModifier> technologyModifiers
                = new();

            //Для каждой загружаемой технологии
            for (int a = 0; a < loadingTechnologies.Length; a++)
            {
                //Если технология является верной
                if (loadingTechnologies[a].IsValidObject
                    == true)
                {
                    //Берём ссылку на загружаемую технологию
                    ref WDTechnology loadingTechnology
                        = ref loadingTechnologies[a];

                    //Очищаем временный список модификаторов
                    technologyModifiers.Clear();
                    //Для каждого модификатора технологии
                    for (int b = 0; b < loadingTechnology.technologyModifiers.Length; b++)
                    {
                        //Если тип модификатора определён
                        if (loadingTechnology.technologyModifiers[b].ModifierType
                            != TechnologyModifierType.None)
                        {
                            //Записываем данные модификатора
                            DTechnologyModifier modifier
                                = new(
                                    loadingTechnology.technologyModifiers[b].ModifierType,
                                    loadingTechnology.technologyModifiers[b].ModifierValue);

                            //Заносим его во временный список
                            technologyModifiers.Add(
                                modifier);
                        }
                    }

                    //Очищаем временный список основных модификаторов компонентов
                    technologyComponentCoreModifiers.Clear();
                    //Для каждого основного модификатора компонента
                    for (int b = 0; b < loadingTechnology.technologyComponentCoreModifiers.Length; b++)
                    {
                        //Если тип модификатора определён
                        if (loadingTechnology.technologyModifiers[b].ModifierType
                            != TechnologyModifierType.None)
                        {
                            //Записываем данные модификатора
                            DTechnologyComponentCoreModifier modifier
                                = new(
                                    loadingTechnology.technologyComponentCoreModifiers[b].ModifierType,
                                    loadingTechnology.technologyComponentCoreModifiers[b].ModifierValue);

                            //Заносим его во временный список
                            technologyComponentCoreModifiers.Add(
                                modifier);
                        }
                    }

                    //Записываем данные технологии
                    DTechnology technology
                        = new(
                            loadingTechnology.ObjectName,
                            loadingTechnology.IsBaseTechnology,
                            technologyModifiers.ToArray(),
                            technologyComponentCoreModifiers.ToArray());

                    //Заносим её во временный список
                    tempLoadingData.technologies.Add(
                        technology);

                    //Отмечаем в данных технологии её индекс в игре
                    loadingTechnology.GameObjectIndex
                        = tempLoadingData.technologies.Count - 1;
                }
            }
        }

        //Типы кораблей
        void GameLoadShipTypes(
            ref WDShipType[] loadingShipTypes,
            ref TempLoadingData tempLoadingData)
        {
            //Для каждого загружаемого типа корабля
            for (int a = 0; a < loadingShipTypes.Length; a++)
            {
                //Если тип является верным
                if (loadingShipTypes[a].IsValidObject == true)
                {
                    //Берём ссылку на загружаемый тип
                    ref WDShipType loadingShipType = ref loadingShipTypes[a];

                    //Записываем данные типа
                    DShipType shipType = new(
                        loadingShipType.ObjectName,
                        loadingShipType.BattleGroup);

                    //Заносим его во временный список
                    tempLoadingData.shipTypes.Add(shipType);

                    //Отмечаем в данных типа его индекс в игре
                    loadingShipType.GameObjectIndex = tempLoadingData.shipTypes.Count - 1;
                }
            }
        }

        //Компоненты кораблей
        void GameLoadEngines(
            ref WDEngine[] loadingEngines,
            ref TempLoadingData tempLoadingData)
        {
            //Создаём списки для временных данных
            DComponentCoreTechnology[] coreTechnologies;

            //Для каждой загружаемого двигателя
            for (int a = 0; a < loadingEngines.Length; a++)
            {
                //Если двигатель является верным
                if (loadingEngines[a].IsValidObject
                    == true)
                {
                    //Берём ссылку на загружаемый двигатель
                    ref WDEngine loadingEngine
                        = ref loadingEngines[a];

                    //Очищаем временный массив основных технологий
                    coreTechnologies
                        = new DComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length];

                    //Загружаем основные технологии двигателя
                    GameLoadComponentCoreTechnologies(
                        in loadingEngine.coreTechnologies,
                        ref coreTechnologies);

                    //Записываем данные двигателя
                    DEngine engine
                        = new(
                            loadingEngine.ObjectName,
                            coreTechnologies,
                            loadingEngine.EngineSize,
                            loadingEngine.EngineBoost);

                    //Заносим его во временный список
                    tempLoadingData.engines.Add(
                        engine);

                    //Отмечаем в данных двигателя его индекс в игре
                    loadingEngine.GameObjectIndex
                        = tempLoadingData.engines.Count - 1;
                }
            }
        }

        void GameLoadReactors(
            ref WDReactor[] loadingReactors,
            ref TempLoadingData tempLoadingData)
        {
            //Создаём списки для временных данных
            DComponentCoreTechnology[] coreTechnologies;

            //Для каждого загружаемого реактора
            for (int a = 0; a < loadingReactors.Length; a++)
            {
                //Если реактор является верным
                if (loadingReactors[a].IsValidObject
                    == true)
                {
                    //Берём ссылку на загружаемый реактор
                    ref WDReactor loadingReactor
                        = ref loadingReactors[a];

                    //Очищаем временный массив основных технологий
                    coreTechnologies
                        = new DComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length];

                    //Загружаем основные технологии реактора
                    GameLoadComponentCoreTechnologies(
                        in loadingReactor.coreTechnologies,
                        ref coreTechnologies);

                    //Записываем данные реактора
                    DReactor reactor
                        = new(
                            loadingReactor.ObjectName,
                            coreTechnologies,
                            loadingReactor.ReactorSize,
                            loadingReactor.ReactorBoost);

                    //Заносим его во временный список
                    tempLoadingData.reactors.Add(
                        reactor);

                    //Отмечаем в данных реактора его индекс в игре
                    loadingReactor.GameObjectIndex
                        = tempLoadingData.reactors.Count - 1;
                }
            }
        }

        void GameLoadFuelTanks(
            ref WDHoldFuelTank[] loadingFuelTanks,
            ref TempLoadingData tempLoadingData)
        {
            //Создаём списки для временных данных
            DComponentCoreTechnology[] coreTechnologies;

            //Для каждого загружаемого топливного бака
            for (int a = 0; a < loadingFuelTanks.Length; a++)
            {
                //Если топливный бак является верным
                if (loadingFuelTanks[a].IsValidObject
                    == true)
                {
                    //Берём ссылку на загружаемый топливный бак
                    ref WDHoldFuelTank loadingFuelTank
                        = ref loadingFuelTanks[a];

                    //Очищаем временный массив основных технологий
                    coreTechnologies
                        = new DComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length];

                    //Загружаем основные технологии топливного бака
                    GameLoadComponentCoreTechnologies(
                        in loadingFuelTank.coreTechnologies,
                        ref coreTechnologies);

                    //Записываем данные топливного бака
                    DHoldFuelTank fuelTank
                        = new(
                            loadingFuelTank.ObjectName,
                            coreTechnologies,
                            loadingFuelTank.Size);

                    //Заносим его во временный список
                    tempLoadingData.fuelTanks.Add(
                        fuelTank);

                    //Отмечаем в данных топливного бака его индекс в игре
                    loadingFuelTank.GameObjectIndex
                        = tempLoadingData.fuelTanks.Count - 1;
                }
            }
        }

        void GameLoadExtractionEquipments(
            ref WDExtractionEquipment[] loadingExtractionEquipments,
            ref TechnologyComponentCoreModifierType[] extractionEquipmentCoreModifierTypes,
            List<DExtractionEquipment> extractionEquipments)
        {
            //Создаём списки для временных данных
            DComponentCoreTechnology[] coreTechnologies;

            //Для каждого загружаемого оборудования для добычи
            for (int a = 0; a < loadingExtractionEquipments.Length; a++)
            {
                //Если оборудование для добычи является верным
                if (loadingExtractionEquipments[a].IsValidObject
                    == true)
                {
                    //Берём ссылку на загружаемое оборудование для добычи
                    ref WDExtractionEquipment loadingExtractionEquipment
                        = ref loadingExtractionEquipments[a];

                    //Очищаем временный массив основных технологий
                    coreTechnologies
                        = new DComponentCoreTechnology[extractionEquipmentCoreModifierTypes.Length];

                    //Загружаем основные технологии оборудования для добычи
                    GameLoadComponentCoreTechnologies(
                        in loadingExtractionEquipment.coreTechnologies,
                        ref coreTechnologies);

                    //Записываем данные оборудования для добычи
                    DExtractionEquipment extractionEquipment
                        = new(
                            loadingExtractionEquipment.ObjectName,
                            coreTechnologies,
                            loadingExtractionEquipment.Size);

                    //Заносим его во временный список
                    extractionEquipments.Add(
                        extractionEquipment);

                    //Отмечаем в данных оборудования для добычи его индекс в игре
                    loadingExtractionEquipment.GameObjectIndex
                        = extractionEquipments.Count - 1;
                }
            }
        }

        void GameLoadEnergyGuns(
            ref WDGunEnergy[] loadingEnergyGuns,
            ref TempLoadingData tempLoadingData)
        {
            //Создаём списки для временных данных
            DComponentCoreTechnology[] coreTechnologies;

            //Для каждого загружаемого энергетического орудия
            for (int a = 0; a < loadingEnergyGuns.Length; a++)
            {
                //Если энергетическое орудие является верным
                if (loadingEnergyGuns[a].IsValidObject
                    == true)
                {
                    //Берём ссылку на загружаемое энергетическое орудие
                    ref WDGunEnergy loadingEnergyGun
                        = ref loadingEnergyGuns[a];

                    //Очищаем временный массив основных технологий
                    coreTechnologies
                        = new DComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length];

                    //Загружаем основные технологии энергетического орудия
                    GameLoadComponentCoreTechnologies(
                        in loadingEnergyGun.coreTechnologies,
                        ref coreTechnologies);

                    //Записываем данные энергетического орудия
                    DGunEnergy fuelTank
                        = new(
                            loadingEnergyGun.ObjectName,
                            coreTechnologies,
                            loadingEnergyGun.GunCaliber,
                            loadingEnergyGun.GunBarrelLength);

                    //Заносим его во временный список
                    tempLoadingData.energyGuns.Add(
                        fuelTank);

                    //Отмечаем в данных энергетического орудия его индекс в игре
                    loadingEnergyGun.GameObjectIndex
                        = tempLoadingData.energyGuns.Count - 1;
                }
            }
        }

        void GameLoadComponentCoreTechnologies(
            in WDComponentCoreTechnology[] loadingCoreTechnologiesArray,
            ref DComponentCoreTechnology[] coreTechnologies)
        {
            //Для каждой основной технологии компонента
            for (int a = 0; a < loadingCoreTechnologiesArray.Length; a++)
            {
                //Если основная технология является верной
                if (loadingCoreTechnologiesArray[a].IsValidLink
                    == true)
                {
                    //Записываем данные основной технологии компонента
                    DComponentCoreTechnology coreTechnology
                        = new(
                            new(loadingCoreTechnologiesArray[a].ContentObjectLink.ContentSetIndex, loadingCoreTechnologiesArray[a].ContentObjectLink.ObjectIndex),
                            loadingCoreTechnologiesArray[a].ModifierValue);

                    //Заносим её на соответствующую позицию массива
                    coreTechnologies[a]
                        = coreTechnology;
                }
            }
        }

        void GameRefCalculatingEngines(
            ref DEngine[] engines,
            int contentSetIndex)
        {
            //Для каждого двигателя
            for (int a = 0; a < engines.Length; a++)
            {
                //Берём ссылку на двигатель
                ref DEngine engine
                    = ref engines[a];

                //Рассчитываем ссылки основных технологий двигателя
                GameRefCalculatingCoreTechnologies(
                    ref engine.coreTechnologies,
                    ShipComponentType.Engine,
                    contentSetIndex,
                    a);


                //Рассчитываем характеристики двигателя
                engine.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingReactors(
            ref DReactor[] reactors,
            int contentSetIndex)
        {
            //Для каждого реактора
            for (int a = 0; a < reactors.Length; a++)
            {
                //Берём ссылку на реактор
                ref DReactor reactor
                    = ref reactors[a];

                //Рассчитываем ссылки основных технологий реактора
                GameRefCalculatingCoreTechnologies(
                    ref reactor.coreTechnologies,
                    ShipComponentType.Reactor,
                    contentSetIndex,
                    a);


                //Рассчитываем характеристики реактора
                reactor.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingFuelTanks(
            ref DHoldFuelTank[] fuelTanks,
            int contentSetIndex)
        {
            //Для каждого топливного бака
            for (int a = 0; a < fuelTanks.Length; a++)
            {
                //Берём ссылку на топливный бак
                ref DHoldFuelTank fuelTank
                    = ref fuelTanks[a];

                //Рассчитываем ссылки основных технологий топливного бака
                GameRefCalculatingCoreTechnologies(
                    ref fuelTank.coreTechnologies,
                    ShipComponentType.HoldFuelTank,
                    contentSetIndex,
                    a);


                //Рассчитываем характеристики топливного бака
                fuelTank.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingExtractionEquipment(
            ref DExtractionEquipment[] extractionEquipments,
            ShipComponentType extractionEquipmentType,
            int contentSetIndex)
        {
            //Для каждого оборудования для добычи
            for (int a = 0; a < extractionEquipments.Length; a++)
            {
                //Берём ссылку на оборудование для добычи
                ref DExtractionEquipment extractionEquipment
                    = ref extractionEquipments[a];

                //Рассчитываем ссылки основных технологий оборудования для добычи
                GameRefCalculatingCoreTechnologies(
                    ref extractionEquipment.coreTechnologies,
                    extractionEquipmentType,
                    contentSetIndex,
                    a);

                //Рассчитываем характеристики оборудования для добычи
                extractionEquipment.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingEnergyGuns(
            ref DGunEnergy[] energyGuns,
            int contentSetIndex)
        {
            //Для каждого энергетического орудия
            for (int a = 0; a < energyGuns.Length; a++)
            {
                //Берём ссылку на энергетическое орудие
                ref DGunEnergy fuelTank
                    = ref energyGuns[a];

                //Рассчитываем ссылки основных технологий энергетического орудия
                GameRefCalculatingCoreTechnologies(
                    ref fuelTank.coreTechnologies,
                    ShipComponentType.GunEnergy,
                    contentSetIndex,
                    a);


                //Рассчитываем характеристики энергетического орудия
                fuelTank.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingCoreTechnologies(
            ref DComponentCoreTechnology[] coreTechnologies,
            ShipComponentType componentType,
            int componentContentSetIndex,
            int componentIndex)
        {
            //Для каждой основной технологии компонента
            for (int a = 0; a < coreTechnologies.Length; a++)
            {
                //Берём ссылку на основную технологию
                ref DComponentCoreTechnology coreTechnology
                    = ref coreTechnologies[a];

                //Определяем индекс набора контента в игре
                int gameContentSetIndex
                    = contentData.Value
                    .wDContentSets[coreTechnology.ContentObjectLink.ContentSetIndex].gameContentSetIndex;

                //Определяем индекс технологии в игре
                int gameTechnologyIndex
                    = contentData.Value
                    .wDContentSets[coreTechnology.ContentObjectLink.ContentSetIndex]
                    .technologies[coreTechnology.ContentObjectLink.ObjectIndex].GameObjectIndex;

                //Если тип компонента - двигатель
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //Заносим ссылку на него в список двигателей, ссылающихся на технологию
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].engines.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - реактор
                else if(componentType
                    == ShipComponentType.Reactor)
                {
                    //Заносим ссылку на него в список реакторов, ссылающихся на технологию
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].reactors.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - топливный бак
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //Заносим ссылку на него в список топливный баков, ссылающихся на технологию
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].fuelTanks.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - оборудование для твёрдой добычи
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //Заносим ссылку на него в список оборудования для твёрдой добычи, ссылающихся на технологию
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].extractionEquipmentSolids.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - энергетическое орудие
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //Заносим ссылку на него в список энергетических орудий, ссылающихся на технологию
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].energyGuns.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }

                //Перезаписываем ссылку
                coreTechnology.ContentObjectLink = new(gameContentSetIndex, gameTechnologyIndex);
            }
        }

        //Классы кораблей
        void GameLoadShipClasses(
            ref WDShipClass[] loadingShipClasses,
            ref TempLoadingData tempLoadingData)
        {
            //Создаём списки для временных данных
            List<DShipClassComponent> shipClassEngines
                = new();
            List<DShipClassComponent> shipClassReactors
                = new();
            List<DShipClassComponent> shipClassFuelTanks
                = new();
            List<DShipClassComponent> shipClassExtractionEquipmentSolids
                = new();
            List<DShipClassComponent> shipClassEnergyGuns
                = new();

            //Для каждого загружаемого класса
            for (int a = 0; a < loadingShipClasses.Length; a++)
            {
                //Если класс корабля является верным
                if (loadingShipClasses[a].IsValidObject
                    == true)
                {
                    //Берём ссылку на загружаемый класс
                    ref WDShipClass loadingShipClass
                        = ref loadingShipClasses[a];

                    //Очищаем временный список двигателей
                    shipClassEngines.Clear();
                    //Загружаем двигатели класса
                    GameLoadShipClassComponents(
                        in loadingShipClass.engines,
                        shipClassEngines);

                    //Очищаем временный список реакторов
                    shipClassReactors.Clear();
                    //Загружаем реакторы класса 
                    GameLoadShipClassComponents(
                        in loadingShipClass.reactors,
                        shipClassReactors);

                    //Очищаем временный список топливных баков
                    shipClassFuelTanks.Clear();
                    //Загружаем топливные баки класса
                    GameLoadShipClassComponents(
                        in loadingShipClass.fuelTanks,
                        shipClassFuelTanks);

                    //Очищаем временный список оборудования для твёрдой добычи
                    shipClassExtractionEquipmentSolids.Clear();
                    //Загружаем оборудование для твёрдой добычи класса
                    GameLoadShipClassComponents(
                        in loadingShipClass.extractionEquipmentSolids,
                        shipClassExtractionEquipmentSolids);

                    //Очищаем временный список энергетических орудий
                    shipClassExtractionEquipmentSolids.Clear();
                    //Загружаем энергетические орудия класса
                    GameLoadShipClassComponents(
                        in loadingShipClass.energyGuns,
                        shipClassEnergyGuns);

                    //Записываем данные класса корабля
                    DShipClass shipClass
                        = new(
                            loadingShipClass.ObjectName,
                            shipClassEngines.ToArray(),
                            shipClassReactors.ToArray(),
                            shipClassFuelTanks.ToArray(),
                            shipClassExtractionEquipmentSolids.ToArray(),
                            shipClassEnergyGuns.ToArray());

                    //Заносим загруженные данные во временный список
                    tempLoadingData.shipClasses.Add(
                        shipClass);

                    //Отмечаем в данных класса корабля его индекс в игре
                    loadingShipClass.GameObjectIndex
                        = tempLoadingData.shipClasses.Count - 1;
                }
            }
        }

        void GameLoadShipClassComponents(
            in WDShipClassComponent[] loadingShipClassComponentsArray,
            List<DShipClassComponent> shipClassComponents)
        {
            //Для каждого загружаемого компонента
            for (int a = 0; a < loadingShipClassComponentsArray.Length; a++)
            {
                //Если компонент является верным
                if (loadingShipClassComponentsArray[a].IsValidLink
                    == true)
                {
                    //Записываем данные компонента
                    DShipClassComponent shipClassComponent
                        = new(
                            loadingShipClassComponentsArray[a].ContentSetIndex,
                            loadingShipClassComponentsArray[a].ObjectIndex,
                            loadingShipClassComponentsArray[a].numberOfComponents);

                    //Заносим его в список
                    shipClassComponents.Add(
                        shipClassComponent);
                }
            }
        }

        void GameRefCalculatingShipClasses(
            ref DShipClass[] shipClasses,
            int contentSetIndex)
        {
            //Для каждого класса корабля
            for (int a = 0; a < shipClasses.Length; a++)
            {
                //Берём ссылку на класс корабля
                ref DShipClass shipClass
                    = ref shipClasses[a];

                //Рассчитываем ссылки на двигатели
                GameRefCalculatingShipClassComponents(
                    ref shipClass.engines,
                    contentSetIndex,
                    a,
                    ShipComponentType.Engine);

                //Рассчитываем ссылки на реакторы
                GameRefCalculatingShipClassComponents(
                    ref shipClass.reactors,
                    contentSetIndex,
                    a,
                    ShipComponentType.Reactor);

                //Рассчитываем ссылки на топливные баки
                GameRefCalculatingShipClassComponents(
                    ref shipClass.fuelTanks,
                    contentSetIndex,
                    a,
                    ShipComponentType.HoldFuelTank);

                //Рассчитываем ссылки на оборудование для твёрдой добычи
                GameRefCalculatingShipClassComponents(
                    ref shipClass.extractionEquipmentSolids,
                    contentSetIndex,
                    a,
                    ShipComponentType.ExtractionEquipmentSolid);

                //Рассчитываем ссылки на энергетические орудия
                GameRefCalculatingShipClassComponents(
                    ref shipClass.energyGuns,
                    contentSetIndex,
                    a,
                    ShipComponentType.GunEnergy);

                //Рассчитываем характеристики класса корабля
                shipClass.CalculateCharacteristics(
                    contentData.Value);
            }
        }

        void GameRefCalculatingShipClassComponents(
            ref DShipClassComponent[] shipClassComponents,
            int shipClassContentSetIndex,
            int shipClassIndex,
            ShipComponentType componentType)
        {
            //Для каждого компонента корабля
            for (int a = 0; a < shipClassComponents.Length; a++)
            {
                //Берём ссылку на компонент
                ref DShipClassComponent shipClassComponent
                    = ref shipClassComponents[a];

                //Определяем индекс набора контента в игре
                int gameContentSetIndex
                    = contentData.Value
                    .wDContentSets[shipClassComponent.ContentSetIndex].gameContentSetIndex;

                //Определяем индекс компонента в игре
                int gameComponentIndex
                    = 0;
                //Если тип компонента - двигатель
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //Определяем индекс компонента в игре
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .engines[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //Указываем в данных компонента, что на него ссылается текущий класс корабля
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .engines[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //Иначе, если тип компонента - реактор
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //Определяем индекс компонента в игре
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .reactors[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //Указываем в данных компонента, что на него ссылается текущий класс корабля
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .reactors[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //Иначе, если тип компонента - топливный бак
                else if(componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //Определяем индекс компонента в игре
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .fuelTanks[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //Указываем в данных компонента, что на него ссылается текущий класс корабля
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .fuelTanks[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //Иначе, если тип компонента - оборудование для твёрдой добычи
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //Определяем индекс компонента в игре
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //Указываем в данных компонента, что на него ссылается текущий класс корабля
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .solidExtractionEquipments[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //Иначе, если тип компонента - энергетическое орудие
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //Определяем индекс компонента в игре
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .energyGuns[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //Указываем в данных компонента, что на него ссылается текущий класс корабля
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .energyGuns[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }

                //Перезаписываем индекс набора контента
                shipClassComponent.ContentSetIndex
                    = gameContentSetIndex;

                //И индекс компонента
                shipClassComponent.ObjectIndex
                    = gameComponentIndex;
            }
        }


        //Прочие функции
        TechnologyComponentCoreModifierType TechnologyDefineComponentCoreModifierType(
            string modifierName)
        {
            //Для каждого названия в массиве названий основных модификаторов компонентов
            for (int a = 0; a < contentData.Value.technologyComponentCoreModifiersNames.Length; a++)
            {
                //Если название модификатора совпадает
                if (contentData.Value.technologyComponentCoreModifiersNames[a]
                    == modifierName)
                {
                    //Возвращаем соответствующий тип основного модификатора компонента
                    return (TechnologyComponentCoreModifierType)a;
                }
            }

            //Возвращаем пустой тип основного модификатора компонента
            return TechnologyComponentCoreModifierType.None;
        }

        TaskForceBattleGroup ShipTypeDefineTaskForceBattleGroup(
            string battleGroupName)
        {
            //Если название боевой группы соответствует названию группы большой дальности
            if (battleGroupName == "LongRange")
            {
                return TaskForceBattleGroup.LongRange;
            }
            //Иначе, если название боевой группы соответствует названию группы средней дальности
            else if (battleGroupName == "MediumRange")
            {
                return TaskForceBattleGroup.MediumRange;
            }
            //Иначе возвращаем стандартную группу - малой дальности
            else //"ShortRange"
            {
                return TaskForceBattleGroup.ShortRange;
            }
        }
    }
}