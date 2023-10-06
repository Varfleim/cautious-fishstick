using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Technology;
using SandOcean.Economy.Building;
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

            //Если массив типов сооружений не пуст, загружаем его
            if (loadingContentSet.buildingTypes != null)
            {
                WorkshopLoadBuildingTypes(
                    in loadingContentSet.buildingTypes,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.buildingTypes = tempLoadingWorkshopData.buildingTypes.ToArray();

            //Если массив типов кораблей не пуст, загружаем его
            if (loadingContentSet.shipTypes != null)
            {
                WorkshopLoadShipTypes(
                    in loadingContentSet.shipTypes,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.shipTypes = tempLoadingWorkshopData.shipTypes.ToArray();

            //Если массив частей корабля не пуст, загружаем его
            if (loadingContentSet.shipParts != null)
            {
                WorkshopLoadShipParts(
                    in loadingContentSet.shipParts,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.shipParts = tempLoadingWorkshopData.shipParts.ToArray();

            //Если массив ключевых технологий частей корабля не пуст, загружаем его
            if (loadingContentSet.shipPartCoreTechnologies != null)
            {
                WorkshopLoadShipPartCoreTechnologies(
                    in loadingContentSet.shipPartCoreTechnologies,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.shipPartCoreTechnologies = tempLoadingWorkshopData.shipPartCoreTechnologies.ToArray();

            //Если массив направлений улучшения частей корабля не пуст, загружаем его
            if (loadingContentSet.shipPartDirectionsOfImprovement != null)
            {
                WorkshopLoadShipPartDirectionsOfImprovement(
                    in loadingContentSet.shipPartDirectionsOfImprovement,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.shipPartDirectionsOfImprovement = tempLoadingWorkshopData.shipPartDirectionsOfImprovement.ToArray();

            //Если массив улучшений частей корабля не пуст, загружаем его
            if (loadingContentSet.shipPartImprovements != null)
            {
                WorkshopLoadShipPartImprovements(
                    in loadingContentSet.shipPartImprovements,
                    ref tempLoadingWorkshopData);
            }
            //Заполняем массив набора контента
            contentSet.shipPartImprovements = tempLoadingWorkshopData.shipPartImprovements.ToArray();

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

            tempLoadingWorkshopData.buildingTypes.Clear();

            tempLoadingWorkshopData.shipTypes.Clear();

            tempLoadingWorkshopData.shipParts.Clear();
            tempLoadingWorkshopData.shipPartCoreTechnologies.Clear();
            tempLoadingWorkshopData.shipPartDirectionsOfImprovement.Clear();
            tempLoadingWorkshopData.shipPartImprovements.Clear();

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
                ref WDContentSet contentSet = ref contentData.Value.wDContentSets[a];

                //Рассчитываем ссылки направлений улучшения частей корабля
                WorkshopLinkCalculatingDirectionsOfImprovement(contentSet.shipPartDirectionsOfImprovement);

                //Рассчитываем ссылки ключевых технологий частей корабля
                WorkshopLinkCalculatingCoreTechnologies(contentSet.shipPartCoreTechnologies);

                //Рассчитываем ссылки частей корабля
                WorkshopLinkCalculatingShipParts(contentSet.shipParts);

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
                ref WDContentSet contentSet = ref contentData.Value.wDContentSets[a];

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

        #region Buildings
        void WorkshopLoadBuildingTypes(
            in SDBuildingType[] loadingBuildingTypes,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Для каждого загружаемого типа сооружения
            for (int a = 0; a < loadingBuildingTypes.Length; a++)
            {
                //Берём загружаемый тип
                ref readonly SDBuildingType loadingBuildingType = ref loadingBuildingTypes[a];

                //Определяем категорию, к которой относится тип
                BuildingCategory buildingCategory = BuildingDefineBuildingCategory(loadingBuildingType.buildingCategory);

                //Записываем загруженные данные типа
                WDBuildingType buildingType = new(
                    loadingBuildingType.typeName,
                    buildingCategory);

                //Заносим его во временный список
                tempLoadingWorkshopData.buildingTypes.Add(buildingType);
            }
        }
        #endregion

        #region Ships
        void WorkshopLoadShipTypes(
            in SDShipType[] loadingShipTypes,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Для каждого загружаемого типа корабля
            for (int a = 0; a < loadingShipTypes.Length; a++)
            {
                //Берём загружаемый тип
                ref readonly SDShipType loadingShipType = ref loadingShipTypes[a];

                //Определяем боевую группу, к которой относится тип
                TaskForceBattleGroup taskForceBattleGroup = ShipTypeDefineTaskForceBattleGroup(loadingShipType.battleGroupName);

                //Записываем загруженные данные типа
                WDShipType shipType = new(
                    loadingShipType.typeName,
                    taskForceBattleGroup);

                //Заносим его во временный список
                tempLoadingWorkshopData.shipTypes.Add(shipType);
            }
        }

        #region ShipParts
        void WorkshopLoadShipParts(
            in SDShipPart[] loadingShipParts,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Создаём список для временных данных
            List<WorkshopContentObjectLink> coreTechnologies = new();

            //Для каждой загружаемой части корабля
            for (int a = 0; a < loadingShipParts.Length; a++)
            {
                //Берём загружаемую часть
                ref readonly SDShipPart loadingShipPart = ref loadingShipParts[a];

                //Очищаем временный список ключевых технологий
                coreTechnologies.Clear();

                //Загружаем ключевые технологии
                WorkshopLoadContentObjectLink(
                    in loadingShipPart.coreTechnologies,
                    coreTechnologies);

                //Записываем загруженные данные части
                WDShipPart shipPart = new(
                    loadingShipPart.partName,
                    coreTechnologies.ToArray());

                //Заносим её во временный список
                tempLoadingWorkshopData.shipParts.Add(shipPart);
            }
        }

        void WorkshopLoadShipPartCoreTechnologies(
            in SDShipPartCoreTechnology[] loadingCoreTechnologies,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Создаём список для временных данных
            List<WorkshopContentObjectLink> directionsOfImprovement = new();

            //Для каждой загружаемой ключевой технологии части корабля
            for (int a = 0; a < loadingCoreTechnologies.Length; a++)
            {
                //Берём загружаемую технологию
                ref readonly SDShipPartCoreTechnology loadingCoreTechnology = ref loadingCoreTechnologies[a];

                //Очищаем временный список 
                directionsOfImprovement.Clear();

                //Загружаем направления улучшения
                WorkshopLoadContentObjectLink(
                    in loadingCoreTechnology.directionsOfImprovement,
                    directionsOfImprovement);

                //Записываем загруженные данные технологии
                WDShipPartCoreTechnology coreTechnology = new(
                    loadingCoreTechnology.coreTechnologyName,
                    directionsOfImprovement.ToArray());

                //Заносим её во временный список
                tempLoadingWorkshopData.shipPartCoreTechnologies.Add(coreTechnology);
            }
        }

        void WorkshopLoadShipPartDirectionsOfImprovement(
            in SDShipPartTypeDirectionOfImprovement[] loadingDirectionsOfImprovement,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Создаём список для временных данных
            List<WorkshopContentObjectLink> improvements = new();

            //Для каждого загружаемого направления улучшения
            for (int a = 0; a < loadingDirectionsOfImprovement.Length; a++)
            {
                //Берём загружаемое направление
                ref readonly SDShipPartTypeDirectionOfImprovement loadingDirectionOfImprovement = ref loadingDirectionsOfImprovement[a];

                //Очищаем временный список
                improvements.Clear();

                //Загружаем улучшения
                WorkshopLoadContentObjectLink(
                    in loadingDirectionOfImprovement.improvements,
                    improvements);

                //Записываем загруженные данные направления
                WDShipPartDirectionOfImprovement directionOfImprovement = new(
                    loadingDirectionOfImprovement.directionOfImprovementName,
                    improvements.ToArray());

                //Заносим его во временный список
                tempLoadingWorkshopData.shipPartDirectionsOfImprovement.Add(directionOfImprovement);
            }
        }

        void WorkshopLoadShipPartImprovements(
            in SDShipPartImprovement[] loadingImprovements,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //Для каждого загружаемого улучшения
            for (int a = 0; a < loadingImprovements.Length; a++)
            {
                //Берём загружаемое улучшение
                ref readonly SDShipPartImprovement loadingImprovement = ref loadingImprovements[a];

                //Записываем загруженные данные улучшения
                WDShipPartImprovement improvement = new(
                    loadingImprovement.improvementName);

                //Заносим его во временный список
                tempLoadingWorkshopData.shipPartImprovements.Add(improvement);
            }
        }

        void WorkshopLinkCalculatingShipParts(
            WDShipPart[] shipParts)
        {
            //Для каждой части корабля
            for (int a = 0; a < shipParts.Length; a++)
            {
                //Берём на часть корабля
                WDShipPart shipPart = shipParts[a];

                //Для каждой ключевой технологии
                for (int b = 0; b < shipPart.CoreTechnologies.Length; b++)
                {
                    //Берём ключевую технологию
                    if (shipPart.CoreTechnologies[b] is WorkshopContentObjectLink coreTechnologyLink)
                    {
                        //Если набор контента существует
                        if (WorkshopFindContentSet(coreTechnologyLink, out int findedContentSetIndex))
                        {
                            //Если ключевая технология существует
                            if (WorkshopFindContentObject(coreTechnologyLink, contentData.Value.wDContentSets[findedContentSetIndex].shipPartCoreTechnologies, 
                                out int findedCoreTechnologyIndex))
                            {
                                //Указываем, что ссылка верна
                                coreTechnologyLink.IsValidLink = true;
                                coreTechnologyLink.ContentSetIndex = findedContentSetIndex;
                                coreTechnologyLink.ObjectIndex = findedCoreTechnologyIndex;
                            }
                            //Иначе
                            else
                            {
                                //Указываем, что ссылка ошибочна, но сохраняем индекс набора контента
                                coreTechnologyLink.IsValidLink = false;
                                coreTechnologyLink.ContentSetIndex = findedContentSetIndex;
                                coreTechnologyLink.ObjectIndex = -1;
                            }
                        }
                        //Иначе
                        else
                        {
                            //Указываем, что ссылка ошибочна
                            coreTechnologyLink.IsValidLink = false;
                            coreTechnologyLink.ContentSetIndex = -1;
                            coreTechnologyLink.ObjectIndex = -1;
                        }
                    }
                }
            }
        }

        void WorkshopLinkCalculatingCoreTechnologies(
            WDShipPartCoreTechnology[] coreTechnologies)
        {
            //Для каждой ключевой технологии части корабля
            for (int a = 0; a < coreTechnologies.Length; a++)
            {
                //Берём ключевую технологию
                WDShipPartCoreTechnology coreTechnology = coreTechnologies[a];

                //Для каждого направления улучшения
                for (int b = 0; b < coreTechnology.DirectionsOfImprovement.Length; b++)
                {
                    //Берём направление улучшения
                    if (coreTechnology.DirectionsOfImprovement[b] is WorkshopContentObjectLink directionOfImprovementLink)
                    {
                        //Если набор контента существует
                        if (WorkshopFindContentSet(directionOfImprovementLink, out int findedContentSetIndex))
                        {
                            //Если направление улучшения существует
                            if (WorkshopFindContentObject(directionOfImprovementLink, contentData.Value.wDContentSets[findedContentSetIndex].shipPartDirectionsOfImprovement,
                                out int findedDirectionOfImprovementIndex))
                            {
                                //Указываем, что ссылка верна
                                directionOfImprovementLink.IsValidLink = true;
                                directionOfImprovementLink.ContentSetIndex = findedContentSetIndex;
                                directionOfImprovementLink.ObjectIndex = findedDirectionOfImprovementIndex;
                            }
                            //Иначе
                            else
                            {
                                //Указываем, что ссылка ошибочна, но сохраняем индекс набора контента
                                directionOfImprovementLink.IsValidLink = false;
                                directionOfImprovementLink.ContentSetIndex = findedContentSetIndex;
                                directionOfImprovementLink.ObjectIndex = -1;
                            }
                        }
                        //Иначе
                        else
                        {
                            //Указываем, что ссылка ошибочна
                            directionOfImprovementLink.IsValidLink = false;
                            directionOfImprovementLink.ContentSetIndex = -1;
                            directionOfImprovementLink.ObjectIndex = -1;
                        }
                    }
                }
            }
        }

        void WorkshopLinkCalculatingDirectionsOfImprovement(
            WDShipPartDirectionOfImprovement[] directionsOfImprovement)
        {
            //Для каждого направления улучшения части корабля
            for (int a = 0; a < directionsOfImprovement.Length; a++)
            {
                //Берём направление улучшения
                WDShipPartDirectionOfImprovement directionOfImprovement = directionsOfImprovement[a];

                //Для каждого улучшения
                for (int b = 0; b < directionOfImprovement.Improvements.Length; b++)
                {
                    //Берём улучшение
                    if (directionOfImprovement.Improvements[b] is WorkshopContentObjectLink improvementLink)
                    {
                        //Если набор контента существует
                        if (WorkshopFindContentSet(improvementLink, out int findedContentSetIndex))
                        {
                            //Если улучшение существует
                            if (WorkshopFindContentObject(improvementLink, contentData.Value.wDContentSets[findedContentSetIndex].shipPartImprovements,
                                out int findedImprovementIndex))
                            {
                                //Указываем, что ссылка верна
                                improvementLink.IsValidLink = true;
                                improvementLink.ContentSetIndex = findedContentSetIndex;
                                improvementLink.ObjectIndex = findedImprovementIndex;
                            }
                            //Иначе
                            else
                            {
                                //Указываем, что ссылка ошибочна, но сохраняем индекс набора контента
                                improvementLink.IsValidLink = false;
                                improvementLink.ContentSetIndex = findedContentSetIndex;
                                improvementLink.ObjectIndex = -1;
                            }
                        }
                        //Иначе
                        else
                        {
                            //Указываем, что ссылка ошибочна
                            improvementLink.IsValidLink = false;
                            improvementLink.ContentSetIndex = -1;
                            improvementLink.ObjectIndex = -1;
                        }
                    }
                }
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
        #endregion
        #endregion

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

        void WorkshopLoadContentObjectLink(
            in SDContentObjectLink[] loadingContentObjectLinks,
            List<WorkshopContentObjectLink> contentObjectLinks)
        {
            //Для каждой ссылки на объект
            for (int a = 0; a < loadingContentObjectLinks.Length; a++)
            {
                //Записываем загруженные данные ссылки
                WorkshopContentObjectLink contentObjectLink = new(
                    loadingContentObjectLinks[a].contentSetName,
                    loadingContentObjectLinks[a].objectName);

                //Заносим её в список
                contentObjectLinks.Add(contentObjectLink);
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
            List<WDShipClassPart> shipParts = new();
            List<WorkshopContentObjectLink> shipPartImprovements = new();

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

                //Если массив частей корабля не пуст
                if (loadingShipClass.shipParts != null)
                {
                    //Очищаем временный список частей корабля
                    shipParts.Clear();

                    //Для каждой загружаемой части корабля
                    for (int b = 0; b < loadingShipClass.shipParts.Length; b++)
                    {
                        //Берём загружаемую часть корабля
                        ref readonly SDShipClassPart loadingShipPart = ref loadingShipClass.shipParts[b];

                        //Очищаем временный список улучшений
                        shipPartImprovements.Clear();

                        //Если массив улучшений не пуст
                        if (loadingShipPart.improvements != null)
                        {
                            //Для каждого загружаемого улучшения
                            for (int c = 0; c < loadingShipPart.improvements.Length; c++)
                            {
                                //Записываем загружаемые данные улучшения
                                WorkshopContentObjectLink shipPartImprovement = new(
                                    loadingShipPart.improvements[c].contentSetName,
                                    loadingShipPart.improvements[c].objectName);

                                //Заносим его в список
                                shipPartImprovements.Add(shipPartImprovement);
                            }
                        }

                        //Записываем загружаемые данные части корабля
                        WDShipClassPart shipPart = new(
                            new WorkshopContentObjectLink(loadingShipPart.part.contentSetName, loadingShipPart.part.objectName),
                            new WorkshopContentObjectLink(loadingShipPart.coreTechnology.contentSetName, loadingShipPart.coreTechnology.objectName),
                            shipPartImprovements.ToArray());

                        //Заносим её в список
                        shipParts.Add(shipPart);
                    }
                }

                //Записываем загруженные данные класса корабля
                WDShipClass shipClass
                    = new(
                        loadingShipClass.className,
                        shipClassEngines.ToArray(),
                        shipClassReactors.ToArray(),
                        shipClassFuelTanks.ToArray(),
                        shipClassExtractionEquipmentSolids.ToArray(),
                        shipClassEnergyGuns.ToArray(),
                        shipParts.ToArray());

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

                //Для каждой части корабля
                for (int b = 0; b < shipClass.shipParts.Length; b++)
                {
                    //Указываем, что часть корабля верна
                    shipClass.shipParts[b].IsValidLink = true;

                    //Берём часть корабля
                    if (shipClass.shipParts[b].Part is WorkshopContentObjectLink shipPartLink)
                    {
                        //Если набор контента существует
                        if (WorkshopFindContentSet(shipPartLink, out int findedContentSetIndex))
                        {
                            //Если часть корабля существует
                            if (WorkshopFindContentObject(shipPartLink, contentData.Value.wDContentSets[findedContentSetIndex].shipParts,
                                out int findedShipPartIndex))
                            {
                                //Указываем, что ссылка верна
                                shipPartLink.IsValidLink = true;
                                shipPartLink.ContentSetIndex = findedContentSetIndex;
                                shipPartLink.ObjectIndex = findedContentSetIndex;
                            }
                            //Иначе
                            else
                            {
                                //Указываем, что ссылка ошибочна, но сохраняем индекс набора контента
                                shipPartLink.IsValidLink = false;
                                shipPartLink.ContentSetIndex = findedShipPartIndex;
                                shipPartLink.ObjectIndex = -1;

                                //Указываем, что часть корабля неверна
                                shipClass.shipParts[b].IsValidLink = false;
                            }
                        }
                        //Иначе
                        else
                        {
                            //Указываем, что ссылка ошибочна
                            shipPartLink.IsValidLink = false;
                            shipPartLink.ContentSetIndex = -1;
                            shipPartLink.ObjectIndex = -1;

                            //Указываем, что часть корабля неверна
                            shipClass.shipParts[b].IsValidLink = false;
                        }
                    }

                    //Берём ключевую технологию
                    if (shipClass.shipParts[b].CoreTechnology is WorkshopContentObjectLink coreTechnologyLink)
                    {
                        //Если набор контента существует
                        if (WorkshopFindContentSet(coreTechnologyLink, out int findedContentSetIndex))
                        {
                            //Если ключевая технология существует
                            if (WorkshopFindContentObject(coreTechnologyLink, contentData.Value.wDContentSets[findedContentSetIndex].shipPartCoreTechnologies,
                                out int findedCoreTechnologyIndex))
                            {
                                //Указываем, что ссылка верна
                                coreTechnologyLink.IsValidLink = true;
                                coreTechnologyLink.ContentSetIndex = findedContentSetIndex;
                                coreTechnologyLink.ObjectIndex = findedContentSetIndex;
                            }
                            //Иначе
                            else
                            {
                                //Указываем, что ссылка ошибочна, но сохраняем индекс набора контента
                                coreTechnologyLink.IsValidLink = false;
                                coreTechnologyLink.ContentSetIndex = findedCoreTechnologyIndex;
                                coreTechnologyLink.ObjectIndex = -1;

                                //Указываем, что часть корабля неверна
                                shipClass.shipParts[b].IsValidLink = false;
                            }
                        }
                        //Иначе
                        else
                        {
                            //Указываем, что ссылка ошибочна
                            coreTechnologyLink.IsValidLink = false;
                            coreTechnologyLink.ContentSetIndex = -1;
                            coreTechnologyLink.ObjectIndex = -1;

                            //Указываем, что часть корабля неверна
                            shipClass.shipParts[b].IsValidLink = false;
                        }
                    }

                    //Для каждого улучшения
                    for (int c = 0; c < shipClass.shipParts[b].Improvements.Length; c++)
                    {
                        //Берём улучшение
                        if (shipClass.shipParts[b].Improvements[c] is WorkshopContentObjectLink improvementLink)
                        {
                            //Если набор контента существует
                            if (WorkshopFindContentSet(improvementLink, out int findedContentSetIndex))
                            {
                                //Если улучшение существует
                                if (WorkshopFindContentObject(improvementLink, contentData.Value.wDContentSets[findedContentSetIndex].shipPartImprovements,
                                    out int findedCoreTechnologyIndex))
                                {
                                    //Указываем, что ссылка верна
                                    improvementLink.IsValidLink = true;
                                    improvementLink.ContentSetIndex = findedContentSetIndex;
                                    improvementLink.ObjectIndex = findedContentSetIndex;
                                }
                                //Иначе
                                else
                                {
                                    //Указываем, что ссылка ошибочна, но сохраняем индекс набора контента
                                    improvementLink.IsValidLink = false;
                                    improvementLink.ContentSetIndex = findedCoreTechnologyIndex;
                                    improvementLink.ObjectIndex = -1;

                                    //Указываем, что часть корабля неверна
                                    shipClass.shipParts[b].IsValidLink = false;
                                }
                            }
                            //Иначе
                            else
                            {
                                //Указываем, что ссылка ошибочна
                                improvementLink.IsValidLink = false;
                                improvementLink.ContentSetIndex = -1;
                                improvementLink.ObjectIndex = -1;

                                //Указываем, что часть корабля неверна
                                shipClass.shipParts[b].IsValidLink = false;
                            }
                        }
                    }
                }

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

        bool WorkshopFindContentSet(
            WorkshopContentObjectLink workshopLink,
            out int contentSetIndex)
        {
            //Для каждого набора контента
            for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
            {
                //Если название набора контента соответствует искомому
                if (contentData.Value.wDContentSets[a].ContentSetName == workshopLink.ContentSetName)
                {
                    //Задаём индекс набора контента
                    contentSetIndex = a;

                    //Возвращаем, что набор контента найден
                    return true;
                }
            }

            //Задаём индекс набора контента
            contentSetIndex = -1;

            //Возвращаем, что набор контента не найден
            return false;
        }

        bool WorkshopFindContentObject<TWorkshopContentObject>(
            WorkshopContentObjectLink workshopLink,
            TWorkshopContentObject[] contentObjects,
            out int objectIndex)
        {
            //Для каждого объекта в списке
            for (int a = 0; a < contentObjects.Length; a++)
            {
                //Пытаемся привести объект к WorkshopContentObject
                if (contentObjects[a] is WDContentObject contentObject)
                {
                    //Если название объекта соответствует искомому
                    if (contentObject.ObjectName == workshopLink.ObjectName)
                    {
                        //Задаём индекс объекта
                        objectIndex = a;

                        //Возвращаем, что объект найден
                        return true;
                    }
                }
            }

            //Задаём индекс объекта
            objectIndex = -1;

            //Возвращаем, что объект не найден
            return true;
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

            //Если массив типов сооружений не пуст, загружаем его
            if (loadingContentSet.buildingTypes != null)
            {
                GameLoadBuildingTypes(
                    ref loadingContentSet.buildingTypes,
                    ref tempLoadingData);
            }
            //Заполняем массив типов
            contentSet.buildingTypes = tempLoadingData.buildingTypes.ToArray();

            //Если массив типов кораблей не пуст, загружаем его
            if (loadingContentSet.shipTypes != null)
            {
                GameLoadShipTypes(
                    ref loadingContentSet.shipTypes,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.shipTypes = tempLoadingData.shipTypes.ToArray();

            //Если массив частей корабля не пуст, загружаем его
            if(loadingContentSet.shipParts != null)
            {
                GameLoadShipParts(
                    loadingContentSet.shipParts,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.shipParts = tempLoadingData.shipParts.ToArray();

            //Если массив ключевых технологий частей корабля не пуст, загружаем его
            if (loadingContentSet.shipPartCoreTechnologies != null)
            {
                GameLoadShipPartCoreTechnologies(
                    loadingContentSet.shipPartCoreTechnologies,
                    ref tempLoadingData);
            }
            //Заполняем масссив набора контента
            contentSet.shipPartCoreTechnologies = tempLoadingData.shipPartCoreTechnologies.ToArray();

            //Если массив направлений улучшения частей корабля не пуст, загружаем его
            if (loadingContentSet.shipPartDirectionsOfImprovement != null)
            {
                GameLoadShipPartDirectionsOfImprovement(
                    loadingContentSet.shipPartDirectionsOfImprovement,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.shipPartDirectionsOfImprovement = tempLoadingData.shipPartDirectionsOfImprovement.ToArray();

            //Если массив улучшений частей корабля не пуст, загружаем его
            if (loadingContentSet.shipPartImprovements != null)
            {
                GameLoadShipPartImprovements(
                    loadingContentSet.shipPartImprovements,
                    ref tempLoadingData);
            }
            //Заполняем массив набора контента
            contentSet.shipPartImprovements = tempLoadingData.shipPartImprovements.ToArray();

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

            tempLoadingData.buildingTypes.Clear();

            tempLoadingData.shipTypes.Clear();

            tempLoadingData.shipParts.Clear();
            tempLoadingData.shipPartCoreTechnologies.Clear();
            tempLoadingData.shipPartDirectionsOfImprovement.Clear();
            tempLoadingData.shipPartImprovements.Clear();

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

                //Рассчитываем ссылки направлений улучшения частей корабля
                GameLinkCalculatingDirectionsOfImprovement(ref contentSet.shipPartDirectionsOfImprovement);

                //Рассчитываем ссылки ключевых технологий частей корабля
                GameLinkCalculatingCoreTechnologies(ref contentSet.shipPartCoreTechnologies);

                //Рассчитываем ссылки частей корабля
                GameLinkCalculatingShipParts(ref contentSet.shipParts);

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

        //Типы сооружений
        void GameLoadBuildingTypes(
            ref WDBuildingType[] loadingBuildingTypes,
            ref TempLoadingData tempLoadingData)
        {
            //Для каждого загружаемого типа сооружения
            for (int a = 0; a < loadingBuildingTypes.Length; a++)
            {
                //Если тип является верным
                if (loadingBuildingTypes[a].IsValidObject == true)
                {
                    //Берём ссылку на загружаемый тип
                    ref WDBuildingType loadingBuildingType = ref loadingBuildingTypes[a];

                    //Записываем данные типа
                    DBuildingType buildingType = new(
                        loadingBuildingType.ObjectName,
                        loadingBuildingType.BuildingCategory);

                    //Заносим его во временный список
                    tempLoadingData.buildingTypes.Add(buildingType);

                    //Отмечаем в данных типа его индекс в игре
                    loadingBuildingType.GameObjectIndex = tempLoadingData.shipTypes.Count - 1;
                }
            }
        }

        #region Ships
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

        #region ShipParts
        void GameLoadShipParts(
            WDShipPart[] loadingShipParts,
            ref TempLoadingData tempLoadingData)
        {
            //Создаём список для временных данных
            List<ContentObjectLink> coreTechnologies = new(); 

            //Для каждой загружаемой части корабля
            for (int a = 0; a < loadingShipParts.Length; a++)
            {
                //Если часть корабля является верной
                if (loadingShipParts[a].IsValidObject == true)
                {
                    //Берём загружаемую часть
                    WDShipPart loadingPart = loadingShipParts[a];

                    //Очищаем временный список ссылок на ключевые технологии
                    coreTechnologies.Clear();

                    //Загружаем ключевые технологии
                    GameLoadContentObjectLink(
                        loadingPart.CoreTechnologies,
                        coreTechnologies);

                    //Записываем данные части
                    DShipPart shipPart = new(
                        loadingPart.ObjectName,
                        coreTechnologies.ToArray());

                    //Заносим её во временный список
                    tempLoadingData.shipParts.Add(shipPart);

                    //Отмечаем в данных части её индекс в игре
                    loadingPart.GameObjectIndex = tempLoadingData.shipParts.Count - 1;
                }
            }
        }

        void GameLoadShipPartCoreTechnologies(
            WDShipPartCoreTechnology[] loadingCoreTechnologies,
            ref TempLoadingData tempLoadingData)
        {
            //Создаём список для временных данных
            List<ContentObjectLink> directionsOfImprovement = new();

            //Для каждой загружаемой ключевой технологии части корабля
            for (int a = 0; a < loadingCoreTechnologies.Length; a++)
            {
                //Если ключевая технология является верной
                if (loadingCoreTechnologies[a].IsValidObject == true)
                {
                    //Берём загружаемую ключевую технологию
                    WDShipPartCoreTechnology loadingCoreTechnology = loadingCoreTechnologies[a];

                    //Очищаем временный список ссылок на направления улучшения
                    directionsOfImprovement.Clear();

                    //Загружаем направления улучшения
                    GameLoadContentObjectLink(
                        loadingCoreTechnology.DirectionsOfImprovement,
                        directionsOfImprovement);

                    //Записываем данные технологии
                    DShipPartCoreTechnology coreTechnology = new(
                        loadingCoreTechnology.ObjectName,
                        directionsOfImprovement.ToArray());

                    //Заносим её во временный список
                    tempLoadingData.shipPartCoreTechnologies.Add(coreTechnology);

                    //Отмечаем в данных технологии её индекс в игре
                    loadingCoreTechnology.GameObjectIndex = tempLoadingData.shipPartCoreTechnologies.Count - 1;
                }
            }
        }

        void GameLoadShipPartDirectionsOfImprovement(
            WDShipPartDirectionOfImprovement[] loadingDirectionsOfImprovement,
            ref TempLoadingData tempLoadingData)
        {
            //Для каждого загружаемого направления улучшения части корабля
            List<ContentObjectLink> improvements = new();

            //Для каждого загружаемого направления улучшения части корабля
            for (int a = 0; a < loadingDirectionsOfImprovement.Length; a++)
            {
                //Если направление улучшения является верным
                if (loadingDirectionsOfImprovement[a].IsValidObject == true)
                {
                    //Берём загружаемое направление улучшения
                    WDShipPartDirectionOfImprovement loadingDirectionOfImprovement = loadingDirectionsOfImprovement[a];

                    //Очищаем временный список ссылок на улучшения
                    improvements.Clear();

                    //Загружаем улучшения
                    GameLoadContentObjectLink(
                        loadingDirectionOfImprovement.Improvements,
                        improvements);

                    //Записываем данные направления
                    DShipPartDirectionOfImprovement directionOfImprovement = new(
                        loadingDirectionOfImprovement.ObjectName,
                        improvements.ToArray());

                    //Заносим его во временный список
                    tempLoadingData.shipPartDirectionsOfImprovement.Add(directionOfImprovement);

                    //Отмечаем в данных направления его индекс в игре
                    loadingDirectionOfImprovement.GameObjectIndex = tempLoadingData.shipPartDirectionsOfImprovement.Count - 1;
                }
            }
        }

        void GameLoadShipPartImprovements(
            WDShipPartImprovement[] loadingImprovements,
            ref TempLoadingData tempLoadingData)
        {
            //Для каждого загружаемого улучшения части корабля
            for (int a = 0; a < loadingImprovements.Length; a++)
            {
                //Если улучшение является верным
                if (loadingImprovements[a].IsValidObject == true)
                {
                    //Берём загружаемое улучшение
                    WDShipPartImprovement loadingImprovement = loadingImprovements[a];

                    //Записываем данные улучшения
                    DShipPartImprovement improvement = new(
                        loadingImprovement.ObjectName);

                    //Заносим его во временный список
                    tempLoadingData.shipPartImprovements.Add(improvement);

                    //Отмечаем в данных улучшения его индекс в игре
                    loadingImprovement.GameObjectIndex = tempLoadingData.shipPartImprovements.Count - 1;
                }
            }
        }

        void GameLinkCalculatingShipParts(
            ref DShipPart[] shipParts)
        {
            //Для каждой части корабля
            for (int a = 0; a < shipParts.Length; a++)
            {
                //Берём часть корабля
                ref DShipPart shipPart = ref shipParts[a];

                //Для каждой ключевой технологии
                for (int b = 0; b < shipPart.CoreTechnologies.Length; b++)
                {
                    //Берём ключевую технологию
                    ContentObjectLink coreTechnologyLink = shipPart.CoreTechnologies[b];

                    //Определяем индексы набора контента и ключевой технологии
                    int gameContentSetIndex = contentData.Value.wDContentSets[coreTechnologyLink.ContentSetIndex].gameContentSetIndex;
                    int gameCoreTechnologyIndex = contentData.Value.wDContentSets[coreTechnologyLink.ContentSetIndex]
                        .shipPartCoreTechnologies[coreTechnologyLink.ObjectIndex].GameObjectIndex;

                    //Здесь мы должны давать самой технологии ссылку на части корабля, использующие её

                    //Обновляем ссылку
                    coreTechnologyLink.ContentSetIndex = gameContentSetIndex;
                    coreTechnologyLink.ObjectIndex = gameCoreTechnologyIndex;
                }
            }
        }

        void GameLinkCalculatingCoreTechnologies(
            ref DShipPartCoreTechnology[] coreTechnologies)
        {
            //Для каждой ключевой технологии части корабля
            for (int a = 0; a < coreTechnologies.Length; a++)
            {
                //Берём ключевую технологию
                ref DShipPartCoreTechnology coreTechnology = ref coreTechnologies[a];

                //Для каждого направления улучшения
                for (int b = 0; b < coreTechnology.DirectionsOfImprovement.Length; b++)
                {
                    //Берём направление улучшения
                    ContentObjectLink directionOfImprovementLink = coreTechnology.DirectionsOfImprovement[b];

                    //Определяем индексы набора контента и направления улучшения
                    int gameContentSetIndex = contentData.Value.wDContentSets[directionOfImprovementLink.ContentSetIndex].gameContentSetIndex;
                    int gameDirectionOfImprovementIndex = contentData.Value.wDContentSets[directionOfImprovementLink.ContentSetIndex]
                        .shipPartDirectionsOfImprovement[directionOfImprovementLink.ObjectIndex].GameObjectIndex;

                    //Здесь мы должны давать самому направлению улучшения ссылку на ключевые технологии, использующие его

                    //Обновляем ссылку
                    directionOfImprovementLink.ContentSetIndex = gameContentSetIndex;
                    directionOfImprovementLink.ObjectIndex = gameDirectionOfImprovementIndex;
                }
            }
        }

        void GameLinkCalculatingDirectionsOfImprovement(
            ref DShipPartDirectionOfImprovement[] directionsOfImprovement)
        {
            //Для каждого направления улучшения части корабля
            for (int a = 0; a < directionsOfImprovement.Length; a++)
            {
                //Берём направление улучшения
                ref DShipPartDirectionOfImprovement directionOfImprovement = ref directionsOfImprovement[a];

                //Для каждого улучшения
                for (int b = 0; b < directionOfImprovement.Improvements.Length; b++)
                {
                    //Берём улучшение
                    ContentObjectLink directionOfImprovementLink = directionOfImprovement.Improvements[b];

                    //Определяем индексы набора контента и улучшения
                    int gameContentSetIndex = contentData.Value.wDContentSets[directionOfImprovementLink.ContentSetIndex].gameContentSetIndex;
                    int gameImprovementIndex = contentData.Value.wDContentSets[directionOfImprovementLink.ContentSetIndex]
                        .shipPartDirectionsOfImprovement[directionOfImprovementLink.ObjectIndex].GameObjectIndex;

                    //Здесь мы должны давать самому улучшению ссылку на направления улучшения, использующие его

                    //Обновляем ссылку
                    directionOfImprovementLink.ContentSetIndex = gameContentSetIndex;
                    directionOfImprovementLink.ObjectIndex = gameImprovementIndex;
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
        #endregion
        #endregion

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

        void GameLoadContentObjectLink(
            ContentObjectLink[] loadingContentObjectLinks,
            List<ContentObjectLink> contentObjectLinks)
        {
            //Для каждой ссылки на объект
            for (int a = 0; a < loadingContentObjectLinks.Length; a++)
            {
                //Пытаемся привести ссылку к WDContentObjectLink
                if (loadingContentObjectLinks[a] is WorkshopContentObjectLink workshopLink)
                {
                    //Если ссылка является верной
                    if (workshopLink.IsValidLink == true)
                    {
                        //Записываем загруженные данные ссылки
                        ContentObjectLink contentObjectLink = new(
                            workshopLink.ContentSetIndex,
                            workshopLink.ObjectIndex);

                        //Заносим её в список
                        contentObjectLinks.Add(contentObjectLink);
                    }
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
            List<DShipClassPart> shipParts = new();
            List<ContentObjectLink> shipPartImprovements = new();

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

                    //Очищаем временный список частей корабля
                    shipParts.Clear();
                    //Для каждой загружаемой части корабля
                    for (int b = 0; b < loadingShipClass.shipParts.Length; b++)
                    {
                        //Если часть верна
                        if (loadingShipClass.shipParts[b].IsValidLink == true)
                        {
                            //Берём загружаемую часть корабля
                            WDShipClassPart loadingShipPart = loadingShipClass.shipParts[b];

                            //Очищаем временный список улучшений
                            shipPartImprovements.Clear();

                            //Для каждого загружаемого улучшения
                            for (int c = 0; c < loadingShipPart.Improvements.Length; c++)
                            {
                                //Записываем загружаемые данные улучшения
                                ContentObjectLink shipPartImprovement = new(
                                    loadingShipPart.Improvements[c].ContentSetIndex, loadingShipPart.Improvements[c].ObjectIndex);

                                //Заносим его в список
                                shipPartImprovements.Add(shipPartImprovement);
                            }

                            //Записываем загружаемые данные части корабля
                            DShipClassPart shipPart = new(
                                new ContentObjectLink(loadingShipPart.Part.ContentSetIndex, loadingShipPart.Part.ObjectIndex),
                                new ContentObjectLink(loadingShipPart.CoreTechnology.ContentSetIndex, loadingShipPart.CoreTechnology.ObjectIndex),
                                shipPartImprovements.ToArray());

                            //Заносим её в список
                            shipParts.Add(shipPart);
                        }
                    }

                    //Записываем данные класса корабля
                    DShipClass shipClass
                        = new(
                            loadingShipClass.ObjectName,
                            shipClassEngines.ToArray(),
                            shipClassReactors.ToArray(),
                            shipClassFuelTanks.ToArray(),
                            shipClassExtractionEquipmentSolids.ToArray(),
                            shipClassEnergyGuns.ToArray(),
                            shipParts.ToArray());

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

                //Для каждой части корабля
                for (int b = 0; b < shipClass.shipParts.Length; b++)
                {
                    //Берём часть корабля
                    DShipClassPart shipPart = shipClass.shipParts[b];

                    //Берём часть корабля
                    ContentObjectLink shipPartLink = shipPart.Part;

                    //Определяем индексы набора контента и части корабля
                    int gameShipPartContentSetIndex = contentData.Value.wDContentSets[shipPartLink.ContentSetIndex].gameContentSetIndex;
                    int gameShipPartIndex = contentData.Value.wDContentSets[shipPartLink.ContentSetIndex].
                        shipParts[shipPartLink.ObjectIndex].GameObjectIndex;

                    //Обновляем ссылку на часть корабля
                    shipPartLink.ContentSetIndex = gameShipPartContentSetIndex;
                    shipPartLink.ObjectIndex = gameShipPartIndex;

                    //Берём ключевую технологию
                    ContentObjectLink coreTechnologyLink = shipPart.CoreTechnology;

                    //Определяем индексы набора контента и ключевой технологии
                    int gameCoreTechnologyContentSetIndex = contentData.Value.wDContentSets[coreTechnologyLink.ContentSetIndex].gameContentSetIndex;
                    int gameCoreTechnologyIndex = contentData.Value.wDContentSets[coreTechnologyLink.ContentSetIndex].
                        shipPartCoreTechnologies[coreTechnologyLink.ObjectIndex].GameObjectIndex;

                    //Обновляем ссылку на ключевую технологию
                    coreTechnologyLink.ContentSetIndex = gameCoreTechnologyContentSetIndex;
                    coreTechnologyLink.ObjectIndex = gameCoreTechnologyIndex;

                    //Для каждого улучшения
                    for (int c = 0; c < shipPart.Improvements.Length; c++)
                    {
                        //Берём улучшение
                        ContentObjectLink improvementLink = shipPart.Improvements[c];

                        //Определяем индексы набора контента и улучшения
                        int gameImprovementContentSetIndex = contentData.Value.wDContentSets[improvementLink.ContentSetIndex].gameContentSetIndex;
                        int gameImprovementIndex = contentData.Value.wDContentSets[improvementLink.ContentSetIndex].
                            shipPartImprovements[improvementLink.ObjectIndex].GameObjectIndex;

                        //Обновляем ссылку
                        improvementLink.ContentSetIndex = gameImprovementContentSetIndex;
                        improvementLink.ObjectIndex = gameImprovementIndex;
                    }
                }

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

        BuildingCategory BuildingDefineBuildingCategory(
            string buildingCategoryName)
        {
            //Если назнавание категории соответствует названию тестовой группы
            if (buildingCategoryName == "Test")
            {
                return BuildingCategory.Test;
            }
            //Иначе возвращаем стандартную категорию - 
            else
            {
                return BuildingCategory.Test;
            }
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