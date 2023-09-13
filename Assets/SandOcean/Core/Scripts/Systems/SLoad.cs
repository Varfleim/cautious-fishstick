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
        //����� �������
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<RStartNewGame> startNewGameEventPool = default;

        //������
        readonly EcsCustomInject<ContentData> contentData = default;
        readonly EcsCustomInject<DesignerData> designerData = default;

        public void Init(IEcsSystems systems)
        {
            //��������� ������ ������� �������� � ������ ����������

            //������ ������ �������� ������� ��������
            contentData.Value.wDContentSetDescriptions
                = new WDContentSetDescription[0];

            //������ ������ �������� ������� ��������
            contentData.Value.contentSetNames
                = new string[0];

            //������ ������ ������� ��������
            contentData.Value.wDContentSets
                = new WDContentSet[0];


            //������ ���� ��� ��������� ������ ��������
            TempLoadingWorkshopData tempLoadingWorkshopData
                = new(0);

            //�������� ���� �� �����, � ������� ��������� ������ ��������
            string contentSetDirectoryPath
                = Path.Combine(Application.dataPath, "ContentSets");

            //���� ���� �����, � �������, ��������, ��������� ������ ��������
            string[] contentSetDirectoriesPaths
                = Directory.GetDirectories(contentSetDirectoryPath);

            //��� ������� ����
            for (int a = 0; a < contentSetDirectoriesPaths.Length; a++)
            {
                //���� � ������ ����� ���������� ���� �������� ������ �������
                if (File.Exists(Path.Combine(
                    contentSetDirectoriesPaths[a], "ContentSetDescription.json")))
                {
                    //��������� �������� ������ ��������
                    SDContentSetDescription contentSetDescription
                        = LoadContentSetDescription(
                            Path.Combine(
                                contentSetDirectoriesPaths[a], "ContentSetDescription.json"));

                    //��������� ������ �������� ������� ��������
                    Array.Resize(
                        ref contentData.Value.wDContentSetDescriptions,
                        contentData.Value.wDContentSetDescriptions.Length + 1);

                    //������� �������� ������ �������� � ������
                    contentData.Value.wDContentSetDescriptions[contentData.Value.wDContentSetDescriptions.Length - 1]
                        = new WDContentSetDescription(
                            contentSetDescription.contentSetName,
                            contentSetDescription.contentSetVersion,
                            contentSetDescription.gameVersion,
                            contentSetDirectoriesPaths[a]);

                    Debug.LogWarning(contentSetDirectoriesPaths[a]);

                    //��������� ������ �������� ������� ��������
                    Array.Resize(
                        ref contentData.Value.contentSetNames,
                        contentData.Value.contentSetNames.Length + 1);

                    //������� �������� ������ �������� � ������
                    contentData.Value.contentSetNames[contentData.Value.contentSetNames.Length - 1]
                        = contentSetDescription.contentSetName;


                    //��������� ������ ������� ��������
                    Array.Resize(
                        ref contentData.Value.wDContentSets,
                        contentData.Value.wDContentSets.Length + 1);

                    //������ ������ ����� �������� � ������� ��� � ������
                    contentData.Value.wDContentSets[contentData.Value.wDContentSets.Length - 1]
                        = new WDContentSet(contentSetDescription.contentSetName);

                    //���� � ������ ����� ���������� ���� ������ ��������
                    if (File.Exists(Path.Combine(
                        contentSetDirectoriesPaths[a], "ContentSet.json")))
                    {
                        //������ ������
                        SDContentSetClass contentSetClass
                            = JsonUtility.FromJson<SDContentSetClass>(
                                File.ReadAllText(
                                    Path.Combine(
                                        contentSetDirectoriesPaths[a], "ContentSet.json")));

                        //��������� ����� ��������
                        WorkshopLoadContentSet(
                            ref contentSetClass.contentSet,
                            ref contentData.Value.wDContentSets[contentData.Value.wDContentSets.Length - 1],
                            ref tempLoadingWorkshopData);
                    }
                }
            }

            //������������ ������ �������� ���� �� �����, ������� � ������� ������ ��������
            WorkshopContentObjectRefsCalculating(
                0);
        }

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ������ ����� ����
            foreach (int startNewGameEventEntity
                in startNewGameEventFilter.Value)
            {
                //���� ��������� ������� ������ ����� ����
                ref RStartNewGame startNewGameEvent
                    = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //��������� ������ ���������� � ������ ����
                GameDataLoad();
            }

            //��� ������� �������� ���������� � ������� ����
            if (false)
            {
                //��������� ���� ����������

                //��� ������� ������ �������� � ����������

                //��������� ����� �������� � WDContentSet �� ������� ��������


                //������ ��������� �������� ������ ��������, ���� �� ���� ��������� ������ ����� ��������, ������� �������


                //������ ���� ��� ��������� ������ ��������
                TempLoadingData tempLoadingData
                    = new(0);

                //��� ������� ������������� �������� ������ ��������
                for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                {
                    //���� ����� �������� �������
                    if (contentData.Value.wDContentSets[a].isActive
                        == true)
                    {
                        //��������� ������ ������� ��������
                        Array.Resize(
                            ref contentData.Value.contentSets,
                            contentData.Value.contentSets.Length + 1);

                        //������ ������ ����� �������� � ������� ��� � ������
                        contentData.Value.contentSets[contentData.Value.contentSets.Length - 1]
                            = new DContentSet(contentData.Value.wDContentSets[a].ContentSetName);

                        //��������� ��� ������ �� ������ ���������� � ������ ����
                        GameLoadContentSet(
                            ref contentData.Value.wDContentSets[a],
                            ref contentData.Value.contentSets[contentData.Value.contentSets.Length - 1],
                            ref tempLoadingData);

                        //��������� � ������ ������ �������� ��� ������ � ����
                        contentData.Value.wDContentSets[a].gameContentSetIndex
                            = contentData.Value.contentSets.Length - 1;
                    }
                }

                //������������ ������ �������� ���� �� �����
                GameContentObjectRefsCalculating();
            }
        }

        //���� ������-���� �� �����, ��������� ������
        //string fileName = "�������������";
        //string path = Path.Combine(����� ����� (Application.persistentDataPath), fileName + ".json");

        SDContentSetDescription LoadContentSetDescription(
            string path)
        {
            //������ ������
            SDContentSetDescriptionClass contentSetDescriptionClass
                = JsonUtility.FromJson<SDContentSetDescriptionClass>(File.ReadAllText(path));

            //���������� ��������� �������� ������ ��������
            return contentSetDescriptionClass.contentSetDescription;
        }

        void GameDataLoad()
        {
            //������ ���� ��� ��������� ������ ��������
            TempLoadingData tempLoadingData
                = new(0);

            //������ ������ ��� ��������� ������
            List<DContentSetDescription> contentSetDescriptions
                = new();
            List<string> contentSetNames
                = new();

            //��� ������� ������ �������� ����������
            for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
            {
                //���� ����� �������� �������
                if (contentData.Value.wDContentSets[a].isActive
                    == true)
                {
                    //���� ����� �������� ����� ��������
                    if (a 
                        < contentData.Value.wDContentSetDescriptions.Length)
                    {
                        //������� �������� �������� ������ �������� �� ��������� ������
                        contentSetDescriptions.Add(
                            new(
                                contentData.Value.wDContentSetDescriptions[a].ContentSetName,
                                contentData.Value.wDContentSetDescriptions[a].ContentSetVersion,
                                contentData.Value.wDContentSetDescriptions[a].GameVersion));
                    }

                    //��������� ������ ������� ��������
                    Array.Resize(
                        ref contentData.Value.contentSets,
                        contentData.Value.contentSets.Length + 1);

                    //������ ������ ����� �������� � ������� ��� � ������
                    contentData.Value.contentSets[contentData.Value.contentSets.Length - 1]
                        = new DContentSet(contentData.Value.wDContentSets[a].ContentSetName);

                    //������� �������� ������ �������� �� ��������� ������
                    contentSetNames.Add(
                        contentData.Value.contentSets[contentData.Value.contentSets.Length - 1].ContentSetName);

                    //��������� ��� ������ �� ������ ���������� � ������ ����
                    GameLoadContentSet(
                        ref contentData.Value.wDContentSets[a],
                        ref contentData.Value.contentSets[contentData.Value.contentSets.Length - 1],
                        ref tempLoadingData);

                    //��������� � ������ ������ �������� ��� ������ � ����
                    contentData.Value.wDContentSets[a].gameContentSetIndex
                        = contentData.Value.contentSets.Length - 1;
                }
            }

            //������������ ������ �������� ���� �� �����
            GameContentObjectRefsCalculating();

            //��������� ������ �������� ������� ��������
            contentData.Value.contentSetDescriptions
                = contentSetDescriptions.ToArray();

            //�������������� ������ �������� ������� ��������
            contentData.Value.contentSetNames
                = contentSetNames.ToArray();

            //������� ������� ������ ����������
            contentData.Value.wDContentSetDescriptions
                = new WDContentSetDescription[0];
            contentData.Value.wDContentSets
                = new WDContentSet[0];
        }

        //������ ��� ����������
        void WorkshopLoadContentSet(
            ref SDContentSet loadingContentSet,
            ref WDContentSet contentSet,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //���� ������ ���������� �� ����
            if (loadingContentSet.technologies != null)
            {
                //��������� ���������� �� ��������� ������
                WorkshopLoadTechnologies(
                    in loadingContentSet.technologies,
                    ref tempLoadingWorkshopData);
            }
            //��������� ������ ������ ��������
            contentSet.technologies = tempLoadingWorkshopData.technologies.ToArray();


            //���� ������ ����� �������� �� ����
            if (loadingContentSet.shipTypes != null)
            {
                //��������� ���� �� ��������� ������
                WorkshopLoadShipTypes(
                    in loadingContentSet.shipTypes,
                    ref tempLoadingWorkshopData);
            }
            //��������� ������ ������ ��������
            contentSet.shipTypes = tempLoadingWorkshopData.shipTypes.ToArray();

            //���� ������ ���������� �� ����
            if (loadingContentSet.engines != null)
            {
                //��������� ��������� �� ��������� ������
                WorkshopLoadEngines(
                    in loadingContentSet.engines,
                    ref tempLoadingWorkshopData);
            }
            //��������� ������ ������ ��������
            contentSet.engines = tempLoadingWorkshopData.engines.ToArray();

            //���� ������ ��������� �� ����
            if (loadingContentSet.reactors != null)
            {
                //��������� �������� �� ��������� ������
                WorkshopLoadReactors(
                    in loadingContentSet.reactors,
                    ref tempLoadingWorkshopData);
            }
            //��������� ������ ������ ��������
            contentSet.reactors = tempLoadingWorkshopData.reactors.ToArray();

            //���� ������ ��������� ����� �� ����
            if (loadingContentSet.fuelTanks != null)
            {
                //��������� ��������� ���� �� ��������� ������
                WorkshopLoadFuelTanks(
                    in loadingContentSet.fuelTanks,
                    ref tempLoadingWorkshopData);
            }
            //��������� ������ ������ ��������
            contentSet.fuelTanks = tempLoadingWorkshopData.fuelTanks.ToArray();

            //���� ������ ������������ ��� ������ ������ �� ����
            if (loadingContentSet.extractionEquipmentSolids != null)
            {
                //��������� ������������ ��� ������ ������ �� ��������� ������
                WorkshopLoadExtractionEquipments(
                    in loadingContentSet.extractionEquipmentSolids,
                    ref designerData.Value.solidExtractionEquipmentCoreModifierTypes,
                    tempLoadingWorkshopData.solidExtractionEquipments);
            }
            //��������� ������ ������ ��������
            contentSet.solidExtractionEquipments = tempLoadingWorkshopData.solidExtractionEquipments.ToArray();

            //���� ������ �������������� ������ �� ����
            if (loadingContentSet.energyGuns != null)
            {
                //��������� �������������� ������ �� ��������� ������
                WorkshopLoadEnergyGuns(
                    in loadingContentSet.energyGuns,
                    ref tempLoadingWorkshopData);
            }
            //��������� ������ ������ ��������
            contentSet.energyGuns = tempLoadingWorkshopData.energyGuns.ToArray();


            //���� ������ ������� �������� �� ����
            if (loadingContentSet.shipClasses != null)
            {
                //��������� ������ �������� �� ��������� ������
                WorkshopLoadShipClasses(
                    in loadingContentSet.shipClasses,
                    ref tempLoadingWorkshopData);
            }
            //��������� ������ ������ ��������
            contentSet.shipClasses = tempLoadingWorkshopData.shipClasses.ToArray();

            //������� ��������� ������
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
            //��� ������� ������ � ������� ������ ����������
            for (int a = startIndex; a < contentData.Value.wDContentSets.Length; a++)
            {
                //���� ������ �� ����� ��������
                ref WDContentSet contentSet
                    = ref contentData.Value
                    .wDContentSets[a];

                //������������ ������ ����������
                WorkshopRefCalculatingEngines(
                    //ref contentSet,
                    ref contentSet.engines,
                    a);

                //������������ ������ ���������
                WorkshopRefCalculatingReactors(
                    //ref contentSet,
                    ref contentSet.reactors,
                    a);

                //������������ ������ ��������� �����
                WorkshopRefCalculatingFuelTanks(
                    ref contentSet.fuelTanks,
                    a);

                //������������ ������ ������������ ��� ������ ������
                WorkshopRefCalculatingExtractionEquipments(
                    ref contentSet.solidExtractionEquipments,
                    ShipComponentType.ExtractionEquipmentSolid,
                    a);

                //������������ ������ �������������� ������
                WorkshopRefCalculatingEnergyGuns(
                    ref contentSet.energyGuns,
                    a);
            }
            
            //��� ������� ������ � ������� ������ ����������
            for (int a = startIndex; a < contentData.Value.wDContentSets.Length; a++)
            {
                //���� ������ �� ����� ��������
                ref WDContentSet contentSet
                    = ref contentData.Value
                    .wDContentSets[a];

                //������������ ������ ������� ��������
                WorkshopRefCalculatingShipClasses(
                    ref contentSet.shipClasses,
                    a);
            }
        }

        //����������
        void WorkshopLoadTechnologies(
            in SDTechnology[] loadingTechnologies,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //������ ������ ��� ��������� ������
            List<WDTechnologyComponentCoreModifier> technologyComponentCoreModifiers
                = new();
            List<WDTechnologyModifier> technologyModifiers
                = new();

            //��� ������ ����������� ����������
            for (int a = 0; a < loadingTechnologies.Length; a++)
            {
                //���� ������ �� ����������� ����������
                ref readonly SDTechnology loadingTechnology
                    = ref loadingTechnologies[a];

                //������� ��������� ������ �������������
                technologyModifiers.Clear();
                //��� ������� ������������ ����������
                for (int b = 0; b < loadingTechnology.technologyModifiers.Length; b++)
                {
                    //������ ���������� ��� ����������� ���� ������������ 
                    TechnologyModifierType modifierType
                        = TechnologyModifierType.None;

                    //��� ������� �������� � ������� �������� ������������� ����������
                    for (int c = 0; c < contentData.Value.technologyModifiersNames.Length; c++)
                    {
                        //���� �������� ������������ ��������� 
                        if (loadingTechnology.technologyModifiers[b].modifierName
                            == contentData.Value.technologyModifiersNames[c])
                        {
                            //���������, ��� ����������� ����� ���, ��������������� �������� ������� "c"
                            modifierType
                                = (TechnologyModifierType)c;
                        }
                    }

                    //���� ��� ������������ ��� ��������
                    if (modifierType
                        != TechnologyModifierType.None)
                    {
                        //���������� ����������� ������ ������������
                        WDTechnologyModifier modifier
                            = new(
                                loadingTechnology.technologyModifiers[b].modifierName,
                                modifierType,
                                loadingTechnology.technologyModifiers[b].modifierValue);

                        //������� ��� �� ��������� ������
                        technologyModifiers.Add(
                            modifier);
                    }
                    //�����
                    else
                    {
                        //���������� ����������� ��� ���������
                        WDTechnologyModifier modifier
                            = new(
                                loadingTechnology.technologyModifiers[b].modifierName,
                                TechnologyModifierType.None,
                                0);

                        //������� ��� �� ��������� ������
                        technologyModifiers.Add(
                            modifier);
                    }
                }

                //������� ��������� ������ �������� ������������� �����������
                technologyComponentCoreModifiers.Clear();
                //��� ������� ��������� ������������ ����������
                for (int b = 0; b < loadingTechnology.technologyComponentCoreModifiers.Length; b++)
                {
                    //���������� ��� ������������
                    TechnologyComponentCoreModifierType modifierType
                        = TechnologyDefineComponentCoreModifierType(
                            loadingTechnology.technologyComponentCoreModifiers[b].modifierName);

                    //���� ��� ������������ ��� ��������
                    if (modifierType
                        != TechnologyComponentCoreModifierType.None)
                    {
                        //���������� ����������� ������ ������������
                        WDTechnologyComponentCoreModifier modifier
                            = new(
                                loadingTechnology.technologyComponentCoreModifiers[b].modifierName,
                                modifierType,
                                loadingTechnology.technologyComponentCoreModifiers[b].modifierValue);

                        //������� ��� �� ��������� ������
                        technologyComponentCoreModifiers.Add(
                            modifier);
                    }
                    //�����
                    else
                    {
                        //���������� ����������� ��� ���������
                        WDTechnologyComponentCoreModifier modifier
                            = new(
                                loadingTechnology.technologyComponentCoreModifiers[b].modifierName,
                                TechnologyComponentCoreModifierType.None,
                                0);

                        //������� ��� �� ��������� ������
                        technologyComponentCoreModifiers.Add(
                            modifier);
                    }
                }

                //���������� ����������� ������ ����������
                WDTechnology technology
                    = new(
                        loadingTechnology.technologyName,
                        loadingTechnology.isBaseTechnology,
                        technologyModifiers.ToArray(),
                        technologyComponentCoreModifiers.ToArray());

                //������� � �� ��������� ������
                tempLoadingWorkshopData.technologies.Add(
                    technology);
            }
        }

        //���� ��������
        void WorkshopLoadShipTypes(
            in SDShipType[] loadingShipTypes,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //��� ������� ������������ ���� �������
            for (int a = 0; a < loadingShipTypes.Length; a++)
            {
                //���� ������ �� ����������� ���
                ref readonly SDShipType loadingShipType = ref loadingShipTypes[a];

                //���������� ������ ������, � ������� ��������� ���
                TaskForceBattleGroup taskForceBattleGroup = ShipTypeDefineTaskForceBattleGroup(loadingShipType.battleGroupName);

                //���������� ����������� ������ ����
                WDShipType shipType = new(
                    loadingShipType.shipTypeName,
                    taskForceBattleGroup);

                //������� ��� �� ��������� ������
                tempLoadingWorkshopData.shipTypes.Add(shipType);
            }
        }

        //���������� ��������
        void WorkshopLoadEngines(
            in SDEngine[] loadingEngines,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //������ ������ ��� ��������� ������
            WDComponentCoreTechnology[] coreTechnologies;

            //��� ������� ������������ ���������
            for (int a = 0; a < loadingEngines.Length; a++)
            {
                //���� ������ ����������� ���������
                ref readonly SDEngine loadingEngine
                    = ref loadingEngines[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new WDComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length];

                //��������� �������� ���������� ���������
                WorkshopLoadComponentCoreTechnologies(
                    in loadingEngine.coreTechnologies,
                    ref designerData.Value.engineCoreModifierTypes,
                    ref coreTechnologies);

                //���������� ����������� ������ ���������
                WDEngine engine
                    = new(
                        loadingEngine.modelName,
                        coreTechnologies,
                        loadingEngine.engineSize,
                        loadingEngine.engineBoost);

                //������� ��� �� ��������� ������
                tempLoadingWorkshopData.engines.Add(
                    engine);
            }
        }

        void WorkshopLoadReactors(
            in SDReactor[] loadingReactors,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //������ ������ ��� ��������� ������
            WDComponentCoreTechnology[] coreTechnologies;

            //��� ������� ������������ ��������
            for (int a = 0; a < loadingReactors.Length; a++)
            {
                //���� ������ �� ����������� �������
                ref readonly SDReactor loadingReactor
                    = ref loadingReactors[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new WDComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length];

                //��������� �������� ���������� ��������
                WorkshopLoadComponentCoreTechnologies(
                    in loadingReactor.coreTechnologies,
                    ref designerData.Value.reactorCoreModifierTypes,
                    ref coreTechnologies);

                //���������� ����������� ������ ��������
                WDReactor reactor
                    = new(
                        loadingReactor.modelName,
                        coreTechnologies,
                        loadingReactor.reactorSize,
                        loadingReactor.reactorBoost);

                //������� ��� �� ��������� ������
                tempLoadingWorkshopData.reactors.Add(
                    reactor);
            }
        }

        void WorkshopLoadFuelTanks(
            in SDHoldFuelTank[] loadingFuelTanks,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //������ ������ ��� ��������� ������
            WDComponentCoreTechnology[] coreTechnologies;

            //��� ������� ������������ ���������� ����
            for (int a = 0; a < loadingFuelTanks.Length; a++)
            {
                //���� ������ �� ����������� ��������� ���
                ref readonly SDHoldFuelTank loadingFuelTank
                    = ref loadingFuelTanks[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new WDComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length];

                //��������� �������� ���������� ���������� ����
                WorkshopLoadComponentCoreTechnologies(
                    in loadingFuelTank.coreTechnologies,
                    ref designerData.Value.fuelTankCoreModifierTypes,
                    ref coreTechnologies);

                //���������� ����������� ������ ���������� ����
                WDHoldFuelTank fuelTank
                    = new(
                        loadingFuelTank.modelName,
                        coreTechnologies,
                        loadingFuelTank.fuelTankSize);

                //������� ��� �� ��������� ������
                tempLoadingWorkshopData.fuelTanks.Add(
                    fuelTank);
            }
        }

        void WorkshopLoadExtractionEquipments(
            in SDExtractionEquipment[] loadingExtractionEquipments,
            ref TechnologyComponentCoreModifierType[] extractionEquipmentCoreModifierTypes,
            List<WDExtractionEquipment> extractionEquipments)
        {
            //������ ������ ��� ��������� ������
            WDComponentCoreTechnology[] coreTechnologies;

            //��� ������� ������������ ������������ ��� ������
            for (int a = 0; a < loadingExtractionEquipments.Length; a++)
            {
                //���� ������ �� ����������� ������������ ��� ������
                ref readonly SDExtractionEquipment loadingExtractionEquipment
                    = ref loadingExtractionEquipments[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new WDComponentCoreTechnology[extractionEquipmentCoreModifierTypes.Length];

                //��������� �������� ������������ ��� ������
                WorkshopLoadComponentCoreTechnologies(
                    in loadingExtractionEquipment.coreTechnologies,
                    ref extractionEquipmentCoreModifierTypes,
                    ref coreTechnologies);

                //���������� ����������� ������ ������������ ��� ������
                WDExtractionEquipment extractionEquipment
                    = new(
                        loadingExtractionEquipment.modelName,
                        coreTechnologies,
                        loadingExtractionEquipment.size);

                //������� ��� �� ��������� ������
                extractionEquipments.Add(
                    extractionEquipment);
            }
        }

        void WorkshopLoadEnergyGuns(
            in SDGunEnergy[] loadingEnergyGuns,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //������ ������ ��� ��������� ������
            WDComponentCoreTechnology[] coreTechnologies;

            //��� ������� ������������ ��������������� ������
            for (int a = 0; a < loadingEnergyGuns.Length; a++)
            {
                //���� ������ �� ����������� �������������� ������
                ref readonly SDGunEnergy loadingEnergyGun
                    = ref loadingEnergyGuns[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new WDComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length];

                //��������� �������� ���������� ��������������� ������
                WorkshopLoadComponentCoreTechnologies(
                    in loadingEnergyGun.coreTechnologies,
                    ref designerData.Value.energyGunCoreModifierTypes,
                    ref coreTechnologies);

                //���������� ����������� ������ ��������������� ������
                WDGunEnergy energyGun
                    = new(
                        loadingEnergyGun.modelName,
                        coreTechnologies,
                        loadingEnergyGun.gunCaliber,
                        loadingEnergyGun.gunBarrelLength);

                //������� ��� �� ��������� ������
                tempLoadingWorkshopData.energyGuns.Add(
                    energyGun);
            }
        }

        void WorkshopLoadComponentCoreTechnologies(
            in SDComponentCoreTechnology[] loadingCoreTechnologies,
            ref TechnologyComponentCoreModifierType[] coreModifiersTypes,
            ref WDComponentCoreTechnology[] coreTechnologies)
        {
            //������ ������ ��� ��������� ������
            List<WDComponentCoreTechnology> tempCoreTechnologies
                = new();

            //��� ������ ����������� �������� ����������
            for (int a = 0; a < loadingCoreTechnologies.Length; a++)
            {
                //���������� ��� ������������
                TechnologyComponentCoreModifierType modifierType
                    = TechnologyDefineComponentCoreModifierType(
                        loadingCoreTechnologies[a].modifierName);

                //���������� ������� ������ �������� ����������
                WDComponentCoreTechnology coreTechnology
                    = new(
                        loadingCoreTechnologies[a].modifierName,
                        modifierType,
                        loadingCoreTechnologies[a].contentSetName,
                        loadingCoreTechnologies[a].technologyName);

                //������� � �� ��������� ������
                tempCoreTechnologies.Add(
                    coreTechnology);
            }

            //��� ������ ����������� �������� ����������
            for (int a = 0; a < tempCoreTechnologies.Count; a++)
            {
                //��� ������� ������������ � ���������� ������
                for (int b = 0; b < coreModifiersTypes.Length; b++)
                {
                    //���� ��� ������������ ���������
                    if (tempCoreTechnologies[a].modifierType
                        == coreModifiersTypes[b])
                    {
                        //������� ���������� �� ��������������� ������� ��������� �������
                        coreTechnologies[b]
                            = tempCoreTechnologies[a];

                        //������� �� �����
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
            //��� ������� ���������
            for (int a = 0; a < engines.Length; a++)
            {
                //���� ������ �� ���������
                ref WDEngine engine
                    = ref engines[a];

                //������������ ������ �������� ���������� ���������
                WorkshopRefCalculatingCoreTechnologies(
                    ref engine.coreTechnologies,
                    ShipComponentType.Engine,
                    contentSetIndex,
                    a);

                //������������ �������������� ���������
                engine.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingReactors(
            //ref WDContentSet contentSet,
            ref WDReactor[] reactors,
            int contentSetIndex)
        {
            //��� ������� ��������
            for (int a = 0; a < reactors.Length; a++)
            {
                //���� ������ �� �������
                ref WDReactor reactor
                    = ref reactors[a];

                //������������ ������ �������� ���������� ��������
                WorkshopRefCalculatingCoreTechnologies(
                    ref reactor.coreTechnologies,
                    ShipComponentType.Reactor,
                    contentSetIndex,
                    a);


                //������������ �������������� ��������
                reactor.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingFuelTanks(
            ref WDHoldFuelTank[] fuelTanks,
            int contentSetIndex)
        {
            //��� ������� ���������� ����
            for (int a = 0; a < fuelTanks.Length; a++)
            {
                //���� ������ �� ��������� ���
                ref WDHoldFuelTank fuelTank
                    = ref fuelTanks[a];

                //������������ ������ �������� ���������� ���������� ����
                WorkshopRefCalculatingCoreTechnologies(
                    ref fuelTank.coreTechnologies,
                    ShipComponentType.HoldFuelTank,
                    contentSetIndex,
                    a);

                //������������ �������������� ���������� ����
                fuelTank.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingExtractionEquipments(
            ref WDExtractionEquipment[] extractionEquipments,
            ShipComponentType extractionEquipmentType,
            int contentSetIndex)
        {
            //��� ������� ������������ ��� ������
            for (int a = 0; a < extractionEquipments.Length; a++)
            {
                //���� ������ �� ������������ ��� ������
                ref WDExtractionEquipment extractionEquipment
                    = ref extractionEquipments[a];

                //������������ ������ �������� ���������� ������������ ��� ������
                WorkshopRefCalculatingCoreTechnologies(
                    ref extractionEquipment.coreTechnologies,
                    extractionEquipmentType,
                    contentSetIndex,
                    a);

                //������������ �������������� ������������ ��� ������
                extractionEquipment.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingEnergyGuns(
            ref WDGunEnergy[] energyGuns,
            int contentSetIndex)
        {
            //��� ������� ��������������� ������
            for (int a = 0; a < energyGuns.Length; a++)
            {
                //���� ������ �� �������������� ������
                ref WDGunEnergy energyGun
                    = ref energyGuns[a];

                //������������ ������ �������� ���������� ��������������� ������
                WorkshopRefCalculatingCoreTechnologies(
                    ref energyGun.coreTechnologies,
                    ShipComponentType.GunEnergy,
                    contentSetIndex,
                    a);

                //������������ �������������� ��������������� ������
                energyGun.CalculateCharacteristics();
            }
        }

        void WorkshopRefCalculatingCoreTechnologies(
            ref WDComponentCoreTechnology[] coreTechnologies,
            ShipComponentType componentType,
            int componentContentSetIndex,
            int componentIndex)
        {
            //��� ������ �������� ���������� ����������
            for (int a = 0; a < coreTechnologies.Length; a++)
            {
                //���� ������ �� �������� ����������
                ref WDComponentCoreTechnology coreTechnology
                    = ref coreTechnologies[a];

                //������ ���������� ��� ������������, ������ �� ������� ����� ��������
                bool isContentSetFinded
                    = false;

                //������ ���������� ��� ������������ ������� ������ ��������
                int contentSetIndex
                    = -1;

                //��� ������� ������ ��������
                for (int b = 0; b < contentData.Value.wDContentSets.Length; b++)
                {
                    //���� �������� ������ �������� ������������� ��������
                    if (contentData.Value.wDContentSets[b].ContentSetName
                        == coreTechnology.contentSetName)
                    {
                        //��������� ������ �������� ������ ��������
                        contentSetIndex
                            = b;

                        //���������, ��� ����� �������� ��� ������
                        isContentSetFinded
                            = true;

                        //������� �� �����
                        break;
                    }
                }

                //���� ����� �������� ��� ������
                if (isContentSetFinded
                    == true)
                {
                    //������ ���������� ��� ������������, ������� �� ������� ����������
                    bool isTechnologyFinded
                        = false;

                    //������ ���������� ��� ������������ ������� ����������
                    int technologyIndex
                        = -1;

                    //������ ���������� ��� ������������ �������� ������������ ����������
                    float technologyModifierValue
                        = 0;

                    //��� ������ ���������� � ��������� ������ ��������
                    for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].technologies.Length; b++)
                    {
                        //���� �������� ���������� ������������� ��������
                        if (contentData.Value.wDContentSets[contentSetIndex].technologies[b].ObjectName
                            == coreTechnology.technologyName)
                        {
                            //��� ������� ��������� ������������ ����������
                            for (int c = 0; c < contentData.Value.wDContentSets[contentSetIndex].technologies[b].technologyComponentCoreModifiers.Length; c++)
                            {
                                //���� ��� ������������ ������������� ��������
                                if (contentData.Value.wDContentSets[contentSetIndex].technologies[b].technologyComponentCoreModifiers[c].ModifierType
                                    == coreTechnology.modifierType)
                                {
                                    //��������� � ������ ����������, ��� �� �� ��������� ������� ������
                                    //���� ��� ���������� - ���������
                                    if (componentType
                                        == ShipComponentType.Engine)
                                    {
                                        //������� ������ �� ���� � ������ ����������, ����������� �� ����������
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].engines.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }
                                    //�����, ���� ��� ���������� - �������
                                    else if(componentType
                                        == ShipComponentType.Reactor)
                                    {
                                        //������� ������ �� ���� � ������ ���������, ����������� �� ����������
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].reactors.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }
                                    //�����, ���� ��� ���������� - ��������� ���
                                    else if(componentType
                                        == ShipComponentType.HoldFuelTank)
                                    {
                                        //������� ������ �� ���� � ������ ��������� �����, ����������� �� ����������
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].fuelTanks.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }
                                    //�����, ���� ��� ���������� - ������������ ��� ������ ������
                                    else if (componentType
                                        == ShipComponentType.ExtractionEquipmentSolid)
                                    {
                                        //������� ������ �� ���� � ������ ������������ ��� ������ ������, ����������� �� ����������
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].extractionEquipmentSolids.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }
                                    //�����, ���� ��� ���������� - �������������� ������
                                    else if (componentType
                                        == ShipComponentType.GunEnergy)
                                    {
                                        //������� ������ �� ���� � ������ �������������� ������, ����������� �� ����������
                                        contentData.Value
                                            .wDContentSets[contentSetIndex]
                                            .technologies[b].energyGuns.Add(
                                            new(
                                                componentContentSetIndex,
                                                componentIndex));
                                    }

                                    //��������� ������ ������� ����������
                                    technologyIndex
                                        = b;

                                    //���������, ��� ���������� ���� �������
                                    isTechnologyFinded
                                        = true;

                                    //��������� �������� ������������
                                    technologyModifierValue
                                        = contentData.Value
                                        .wDContentSets[contentSetIndex]
                                        .technologies[b]
                                        .technologyComponentCoreModifiers[c].ModifierValue;

                                    //������� �� �����
                                    break;
                                }
                            }

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ���������� ���� �������
                    if (isTechnologyFinded
                        == true)
                    {
                        //�������������� ������ �������� ����������
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
                    //�����
                    else
                    {
                        //�������������� ������ �������� ����������, ��������, ��� ������ ��������, �� �������� ������ �� ����� ��������
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
                //�����
                else
                {
                    //�������������� ������ �������� ����������, ��������, ��� ������ ��������
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

        //������ ��������
        void WorkshopLoadShipClasses(
            in SDShipClass[] loadingShipClasses,
            ref TempLoadingWorkshopData tempLoadingWorkshopData)
        {
            //������ ������ ��� ��������� ������
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

            //��� ������� ������������ ������
            for (int a = 0; a < loadingShipClasses.Length; a++)
            {
                //���� ������ �� ����������� �����
                ref readonly SDShipClass loadingShipClass
                    = ref loadingShipClasses[a];

                //���� ������ ���������� �� ����
                if (loadingShipClass.engines
                    != null)
                {
                    //������� ��������� ������ ����������
                    shipClassEngines.Clear();
                    //��������� ��������� �������
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.engines,
                        shipClassEngines);
                }
                
                //���� ������ ��������� �� ����
                if (loadingShipClass.reactors
                    != null)
                {
                    //������� ��������� ������ ���������
                    shipClassReactors.Clear();
                    //��������� �������� �������
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.reactors,
                        shipClassReactors);
                }

                //���� ������ ��������� ����� �� ����
                if (loadingShipClass.fuelTanks
                    != null)
                {
                    //������� ��������� ������ ��������� �����
                    shipClassFuelTanks.Clear();
                    //��������� ��������� ���� �������
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.fuelTanks,
                        shipClassFuelTanks);
                }

                //���� ������ ������������ ��� ������ ������ �� ����
                if (loadingShipClass.extractionEquipmentSolids
                    != null)
                {
                    //������� ��������� ������ ������������ ��� ������ ������
                    shipClassExtractionEquipmentSolids.Clear();
                    //��������� ������������ ��� ������ ������ �������
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.extractionEquipmentSolids,
                        shipClassExtractionEquipmentSolids);
                }

                //���� ������ �������������� ������ �� ����
                if (loadingShipClass.energyGuns
                    != null)
                {
                    //������� ��������� ������ �������������� ������
                    shipClassEnergyGuns.Clear();
                    //��������� �������������� ������ �������
                    WorkshopLoadShipClassComponents(
                        in loadingShipClass.energyGuns,
                        shipClassEnergyGuns);
                }

                //���������� ����������� ������ ������ �������
                WDShipClass shipClass
                    = new(
                        loadingShipClass.className,
                        shipClassEngines.ToArray(),
                        shipClassReactors.ToArray(),
                        shipClassFuelTanks.ToArray(),
                        shipClassExtractionEquipmentSolids.ToArray(),
                        shipClassEnergyGuns.ToArray());

                //������� ����������� ������ �� ��������� ������
                tempLoadingWorkshopData.shipClasses.Add(
                    shipClass);
            }
        }

        void WorkshopLoadShipClassComponents(
            in SDShipClassComponent[] loadingShipClassComponents,
            List<WDShipClassComponent> shipClassComponents)
        {
            //��� ������� ������������ ����������
            for (int a = 0; a < loadingShipClassComponents.Length; a++)
            {
                //���������� ������� ������ ���������� �������
                WDShipClassComponent shipClassComponent
                    = new(
                        loadingShipClassComponents[a].contentSetName,
                        loadingShipClassComponents[a].componentName,
                        loadingShipClassComponents[a].numberOfComponents);

                //������� ��� � ������
                shipClassComponents.Add(
                    shipClassComponent);
            }
        }

        void WorkshopRefCalculatingShipClasses(
            ref WDShipClass[] shipClasses,
            int contentSetIndex)
        {
            //��� ������� ������ �������
            for (int a = 0; a < shipClasses.Length; a++)
            {
                //���� ������ �� ����� �������
                ref WDShipClass shipClass
                    = ref shipClasses[a];

                //������������ ������ �� ���������
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.engines,
                    contentSetIndex,
                    a,
                    ShipComponentType.Engine);

                //������������ ������ �� ��������
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.reactors,
                    contentSetIndex,
                    a,
                    ShipComponentType.Reactor);

                //������������ ������ �� ��������� ����
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.fuelTanks,
                    contentSetIndex,
                    a,
                    ShipComponentType.HoldFuelTank);

                //������������ ������ �� ������������ ��� ������ ������
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.extractionEquipmentSolids,
                    contentSetIndex,
                    a,
                    ShipComponentType.ExtractionEquipmentSolid);

                //������������ ������ �� �������������� ������
                WorkshopRefCalculatingShipClassComponents(
                    ref shipClass.energyGuns,
                    contentSetIndex,
                    a,
                    ShipComponentType.GunEnergy);

                //������������ �������������� ������ �������
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
            //��� ������� ���������� �������
            for (int a = 0; a < shipClassComponents.Length; a++)
            {
                //���� ������ �� ���������
                ref WDShipClassComponent shipClassComponent
                    = ref shipClassComponents[a];

                //������ ���������� ��� ������������, ������ �� ������� ����� ��������
                bool isContentSetFinded
                    = false;

                //������ ���������� ��� ������������ ������� ������ ��������
                int contentSetIndex
                    = -1;

                //��� ������� ������ ��������
                for (int b = 0; b < contentData.Value.wDContentSets.Length; b++)
                {
                    //���� �������� ������ �������� ������������� ��������
                    if (contentData.Value.wDContentSets[b].ContentSetName
                        == shipClassComponent.contentSetName)
                    {
                        //��������� ������ �������� ������ ��������
                        contentSetIndex
                            = b;

                        //���������, ��� ����� �������� ��� ������
                        isContentSetFinded
                            = true;

                        //������� �� �����
                        break;
                    }
                }

                //���� ������ �������� ��� ������
                if (isContentSetFinded
                    == true)
                {
                    //������ ���������� ��� ������������, ������ �� ������� ���������
                    bool isComponentFinded
                        = false;

                    //������ ���������� ��� ������������ ������� ����������
                    int componentIndex
                        = -1;

                    //���� ��� ���������� - ���������
                    if (componentType
                        == ShipComponentType.Engine)
                    {
                        //��� ������� ��������� � ��������� ������ ��������
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].engines.Length; b++)
                        {
                            //���� �������� ��������� ������������� ��������
                            if (contentData.Value.wDContentSets[contentSetIndex].engines[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .engines[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //��������� ������ �������� ����������
                                componentIndex
                                    = b;

                                //���������, ��� ��������� ��� ������
                                isComponentFinded
                                    = true;

                                //������� �� �����
                                break;
                            }
                        }
                    }
                    //�����, ���� ��� ���������� - �������
                    else if (componentType
                        == ShipComponentType.Reactor)
                    {
                        //��� ������� �������� � ��������� ������ ��������
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].reactors.Length; b++)
                        {
                            //���� �������� �������� ������������� ��������
                            if (contentData.Value.wDContentSets[contentSetIndex].reactors[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .reactors[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //��������� ������ �������� ����������
                                componentIndex
                                    = b;

                                //���������, ��� ��������� ��� ������
                                isComponentFinded
                                    = true;

                                //������� �� �����
                                break;
                            }
                        }
                    }
                    //�����, ���� ��� ���������� - ��������� ���
                    else if (componentType
                        == ShipComponentType.HoldFuelTank)
                    {
                        //��� ������� ���������� ���� � ��������� ������ ��������
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length; b++)
                        {
                            //���� �������� ���������� ���� ������������� ��������
                            if (contentData.Value.wDContentSets[contentSetIndex].fuelTanks[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .fuelTanks[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //��������� ������ �������� ����������
                                componentIndex
                                    = b;

                                //���������, ��� ��������� ��� ������
                                isComponentFinded
                                    = true;

                                //������� �� �����
                                break;
                            }
                        }
                    }
                    //�����, ���� ��� ���������� - ������������ ��� ������ ������
                    else if (componentType
                        == ShipComponentType.ExtractionEquipmentSolid)
                    {
                        //��� ������� ������������ ��� ������ ������ � ��������� ������ ��������
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length; b++)
                        {
                            //���� �������� ������������ ��� ������ ������ ������������� ��������
                            if (contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .solidExtractionEquipments[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //��������� ������ �������� ����������
                                componentIndex
                                    = b;

                                //���������, ��� ��������� ��� ������
                                isComponentFinded
                                    = true;

                                //������� �� �����
                                break;
                            }
                        }
                    }
                    //�����, ���� ��� ���������� - �������������� ������
                    else if (componentType
                        == ShipComponentType.GunEnergy)
                    {
                        //��� ������� ��������������� ������ � ��������� ������ ��������
                        for (int b = 0; b < contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length; b++)
                        {
                            //���� �������� ��������������� ������ ������������� ��������
                            if (contentData.Value.wDContentSets[contentSetIndex].energyGuns[b].ObjectName
                                == shipClassComponent.componentName)
                            {
                                //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                                contentData.Value
                                    .wDContentSets[contentSetIndex]
                                    .energyGuns[b].ShipClasses.Add(
                                    new(
                                        shipClassContentSetIndex,
                                        shipClassIndex));

                                //��������� ������ �������� ����������
                                componentIndex
                                    = b;

                                //���������, ��� ��������� ��� ������
                                isComponentFinded
                                    = true;

                                //������� �� �����
                                break;
                            }
                        }
                    }

                    //���� ��������� ��� ������
                    if (isComponentFinded
                        == true)
                    {
                        //�������������� ������ ����������
                        shipClassComponents[a]
                            = new(
                                shipClassComponent.contentSetName,
                                shipClassComponent.componentName,
                                shipClassComponent.numberOfComponents,
                                contentSetIndex,
                                componentIndex,
                                true);
                    }
                    //�����
                    else
                    {
                        //�������������� ������ ����������, ��������, ��� ������ ��������, �� �������� ������ �� ����� ��������
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
                //�����
                else
                {
                    //�������������� ������ ����������, ��������, ��� ������ ��������
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

        //������ ��� ����
        void GameLoadContentSet(
            ref WDContentSet loadingContentSet,
            ref DContentSet contentSet,
            ref TempLoadingData tempLoadingData)
        {
            //���� ������ ���������� �� ����
            if (loadingContentSet.technologies != null)
            {
                //��������� ���������� �� ��������� ������
                GameLoadTechnologies(
                    ref loadingContentSet.technologies,
                    ref tempLoadingData);
            }
            //��������� ������ ������ ��������
            contentSet.technologies = tempLoadingData.technologies.ToArray();


            //���� ������ ����� �������� �� ����
            if (loadingContentSet.shipTypes != null)
            {
                //��������� ���� �� ��������� ������
                GameLoadShipTypes(
                    ref loadingContentSet.shipTypes,
                    ref tempLoadingData);
            }
            //��������� ������ �����
            contentSet.shipTypes = tempLoadingData.shipTypes.ToArray();

            //���� ������ ���������� �� ����
            if (loadingContentSet.engines != null)
            {
                //��������� ��������� �� ��������� ������
                GameLoadEngines(
                    ref loadingContentSet.engines,
                    ref tempLoadingData);
            }
            //��������� ������ ������ ��������
            contentSet.engines = tempLoadingData.engines.ToArray();

            //���� ������ ��������� �� ����
            if (loadingContentSet.reactors != null)
            {
                //��������� �������� �� ��������� ������
                GameLoadReactors(
                    ref loadingContentSet.reactors,
                    ref tempLoadingData);
            }
            //��������� ������ ������ ��������
            contentSet.reactors = tempLoadingData.reactors.ToArray();

            //���� ������ ��������� ����� �� ����
            if (loadingContentSet.fuelTanks != null)
            {
                //�������� ��������� ���� �� ��������� ������
                GameLoadFuelTanks(
                    ref loadingContentSet.fuelTanks,
                    ref tempLoadingData);
            }
            //��������� ������ ������ ��������
            contentSet.fuelTanks = tempLoadingData.fuelTanks.ToArray();

            //���� ������ ������������ ��� ������ ������ �� ����
            if (loadingContentSet.solidExtractionEquipments != null)
            {
                //�������� ������������ ��� ������ ������ �� ��������� ������
                GameLoadExtractionEquipments(
                    ref loadingContentSet.solidExtractionEquipments,
                    ref designerData.Value.solidExtractionEquipmentCoreModifierTypes,
                    tempLoadingData.solidExtractionEquipments);
            }
            //��������� ������ ������ ��������
            contentSet.solidExtractionEquipments = tempLoadingData.solidExtractionEquipments.ToArray();

            //���� ������ �������������� ������ �� ����
            if (loadingContentSet.energyGuns != null)
            {
                //�������� �������������� ������ �� ��������� ������
                GameLoadEnergyGuns(
                    ref loadingContentSet.energyGuns,
                    ref tempLoadingData);
            }
            //��������� ������ ������ ��������
            contentSet.energyGuns = tempLoadingData.energyGuns.ToArray();


            //���� ������ ������� �������� �� ����
            if (loadingContentSet.shipClasses != null)
            {
                //��������� ������ �������� �� ��������� ������
                GameLoadShipClasses(
                    ref loadingContentSet.shipClasses,
                    ref tempLoadingData);
            }
            //��������� ������ ������ ��������
            contentSet.shipClasses = tempLoadingData.shipClasses.ToArray();

            //������� ��������� ������
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
            //��� ������� ������ �������� � ������� ������ ����
            for (int a = 0; a < contentData.Value.contentSets.Length; a++)
            {
                //���� ������ �� ����� ��������
                ref DContentSet contentSet
                    = ref contentData.Value
                    .contentSets[a];

                //������������ ������ ����������
                GameRefCalculatingEngines(
                    ref contentSet.engines,
                    a);

                //������������ ������ ���������
                GameRefCalculatingReactors(
                    ref contentSet.reactors,
                    a);

                //������������ ������ ��������� �����
                GameRefCalculatingFuelTanks(
                    ref contentSet.fuelTanks,
                    a);

                //������������ ������ ������������ ��� ������ ������
                GameRefCalculatingExtractionEquipment(
                    ref contentSet.solidExtractionEquipments,
                    ShipComponentType.ExtractionEquipmentSolid,
                    a);

                //������������ ������ �������������� ������
                GameRefCalculatingEnergyGuns(
                    ref contentSet.energyGuns,
                    a);
            }

            //��� ������� ������ �������� � ������� ������ ����
            for (int a = 0; a < contentData.Value.contentSets.Length; a++)
            {
                //���� ������ �� ����� ��������
                ref DContentSet contentSet
                    = ref contentData.Value
                    .contentSets[a];

                //������������ ������ ������� ��������
                GameRefCalculatingShipClasses(
                    ref contentSet.shipClasses,
                    a);
            }
        }

        //����������
        void GameLoadTechnologies(
            ref WDTechnology[] loadingTechnologies,
            ref TempLoadingData tempLoadingData)
        {
            //������ ������ ��� ��������� ������
            List<DTechnologyComponentCoreModifier> technologyComponentCoreModifiers
                = new();
            List<DTechnologyModifier> technologyModifiers
                = new();

            //��� ������ ����������� ����������
            for (int a = 0; a < loadingTechnologies.Length; a++)
            {
                //���� ���������� �������� ������
                if (loadingTechnologies[a].IsValidObject
                    == true)
                {
                    //���� ������ �� ����������� ����������
                    ref WDTechnology loadingTechnology
                        = ref loadingTechnologies[a];

                    //������� ��������� ������ �������������
                    technologyModifiers.Clear();
                    //��� ������� ������������ ����������
                    for (int b = 0; b < loadingTechnology.technologyModifiers.Length; b++)
                    {
                        //���� ��� ������������ ��������
                        if (loadingTechnology.technologyModifiers[b].ModifierType
                            != TechnologyModifierType.None)
                        {
                            //���������� ������ ������������
                            DTechnologyModifier modifier
                                = new(
                                    loadingTechnology.technologyModifiers[b].ModifierType,
                                    loadingTechnology.technologyModifiers[b].ModifierValue);

                            //������� ��� �� ��������� ������
                            technologyModifiers.Add(
                                modifier);
                        }
                    }

                    //������� ��������� ������ �������� ������������� �����������
                    technologyComponentCoreModifiers.Clear();
                    //��� ������� ��������� ������������ ����������
                    for (int b = 0; b < loadingTechnology.technologyComponentCoreModifiers.Length; b++)
                    {
                        //���� ��� ������������ ��������
                        if (loadingTechnology.technologyModifiers[b].ModifierType
                            != TechnologyModifierType.None)
                        {
                            //���������� ������ ������������
                            DTechnologyComponentCoreModifier modifier
                                = new(
                                    loadingTechnology.technologyComponentCoreModifiers[b].ModifierType,
                                    loadingTechnology.technologyComponentCoreModifiers[b].ModifierValue);

                            //������� ��� �� ��������� ������
                            technologyComponentCoreModifiers.Add(
                                modifier);
                        }
                    }

                    //���������� ������ ����������
                    DTechnology technology
                        = new(
                            loadingTechnology.ObjectName,
                            loadingTechnology.IsBaseTechnology,
                            technologyModifiers.ToArray(),
                            technologyComponentCoreModifiers.ToArray());

                    //������� � �� ��������� ������
                    tempLoadingData.technologies.Add(
                        technology);

                    //�������� � ������ ���������� � ������ � ����
                    loadingTechnology.GameObjectIndex
                        = tempLoadingData.technologies.Count - 1;
                }
            }
        }

        //���� ��������
        void GameLoadShipTypes(
            ref WDShipType[] loadingShipTypes,
            ref TempLoadingData tempLoadingData)
        {
            //��� ������� ������������ ���� �������
            for (int a = 0; a < loadingShipTypes.Length; a++)
            {
                //���� ��� �������� ������
                if (loadingShipTypes[a].IsValidObject == true)
                {
                    //���� ������ �� ����������� ���
                    ref WDShipType loadingShipType = ref loadingShipTypes[a];

                    //���������� ������ ����
                    DShipType shipType = new(
                        loadingShipType.ObjectName,
                        loadingShipType.BattleGroup);

                    //������� ��� �� ��������� ������
                    tempLoadingData.shipTypes.Add(shipType);

                    //�������� � ������ ���� ��� ������ � ����
                    loadingShipType.GameObjectIndex = tempLoadingData.shipTypes.Count - 1;
                }
            }
        }

        //���������� ��������
        void GameLoadEngines(
            ref WDEngine[] loadingEngines,
            ref TempLoadingData tempLoadingData)
        {
            //������ ������ ��� ��������� ������
            DComponentCoreTechnology[] coreTechnologies;

            //��� ������ ������������ ���������
            for (int a = 0; a < loadingEngines.Length; a++)
            {
                //���� ��������� �������� ������
                if (loadingEngines[a].IsValidObject
                    == true)
                {
                    //���� ������ �� ����������� ���������
                    ref WDEngine loadingEngine
                        = ref loadingEngines[a];

                    //������� ��������� ������ �������� ����������
                    coreTechnologies
                        = new DComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length];

                    //��������� �������� ���������� ���������
                    GameLoadComponentCoreTechnologies(
                        in loadingEngine.coreTechnologies,
                        ref coreTechnologies);

                    //���������� ������ ���������
                    DEngine engine
                        = new(
                            loadingEngine.ObjectName,
                            coreTechnologies,
                            loadingEngine.EngineSize,
                            loadingEngine.EngineBoost);

                    //������� ��� �� ��������� ������
                    tempLoadingData.engines.Add(
                        engine);

                    //�������� � ������ ��������� ��� ������ � ����
                    loadingEngine.GameObjectIndex
                        = tempLoadingData.engines.Count - 1;
                }
            }
        }

        void GameLoadReactors(
            ref WDReactor[] loadingReactors,
            ref TempLoadingData tempLoadingData)
        {
            //������ ������ ��� ��������� ������
            DComponentCoreTechnology[] coreTechnologies;

            //��� ������� ������������ ��������
            for (int a = 0; a < loadingReactors.Length; a++)
            {
                //���� ������� �������� ������
                if (loadingReactors[a].IsValidObject
                    == true)
                {
                    //���� ������ �� ����������� �������
                    ref WDReactor loadingReactor
                        = ref loadingReactors[a];

                    //������� ��������� ������ �������� ����������
                    coreTechnologies
                        = new DComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length];

                    //��������� �������� ���������� ��������
                    GameLoadComponentCoreTechnologies(
                        in loadingReactor.coreTechnologies,
                        ref coreTechnologies);

                    //���������� ������ ��������
                    DReactor reactor
                        = new(
                            loadingReactor.ObjectName,
                            coreTechnologies,
                            loadingReactor.ReactorSize,
                            loadingReactor.ReactorBoost);

                    //������� ��� �� ��������� ������
                    tempLoadingData.reactors.Add(
                        reactor);

                    //�������� � ������ �������� ��� ������ � ����
                    loadingReactor.GameObjectIndex
                        = tempLoadingData.reactors.Count - 1;
                }
            }
        }

        void GameLoadFuelTanks(
            ref WDHoldFuelTank[] loadingFuelTanks,
            ref TempLoadingData tempLoadingData)
        {
            //������ ������ ��� ��������� ������
            DComponentCoreTechnology[] coreTechnologies;

            //��� ������� ������������ ���������� ����
            for (int a = 0; a < loadingFuelTanks.Length; a++)
            {
                //���� ��������� ��� �������� ������
                if (loadingFuelTanks[a].IsValidObject
                    == true)
                {
                    //���� ������ �� ����������� ��������� ���
                    ref WDHoldFuelTank loadingFuelTank
                        = ref loadingFuelTanks[a];

                    //������� ��������� ������ �������� ����������
                    coreTechnologies
                        = new DComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length];

                    //��������� �������� ���������� ���������� ����
                    GameLoadComponentCoreTechnologies(
                        in loadingFuelTank.coreTechnologies,
                        ref coreTechnologies);

                    //���������� ������ ���������� ����
                    DHoldFuelTank fuelTank
                        = new(
                            loadingFuelTank.ObjectName,
                            coreTechnologies,
                            loadingFuelTank.Size);

                    //������� ��� �� ��������� ������
                    tempLoadingData.fuelTanks.Add(
                        fuelTank);

                    //�������� � ������ ���������� ���� ��� ������ � ����
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
            //������ ������ ��� ��������� ������
            DComponentCoreTechnology[] coreTechnologies;

            //��� ������� ������������ ������������ ��� ������
            for (int a = 0; a < loadingExtractionEquipments.Length; a++)
            {
                //���� ������������ ��� ������ �������� ������
                if (loadingExtractionEquipments[a].IsValidObject
                    == true)
                {
                    //���� ������ �� ����������� ������������ ��� ������
                    ref WDExtractionEquipment loadingExtractionEquipment
                        = ref loadingExtractionEquipments[a];

                    //������� ��������� ������ �������� ����������
                    coreTechnologies
                        = new DComponentCoreTechnology[extractionEquipmentCoreModifierTypes.Length];

                    //��������� �������� ���������� ������������ ��� ������
                    GameLoadComponentCoreTechnologies(
                        in loadingExtractionEquipment.coreTechnologies,
                        ref coreTechnologies);

                    //���������� ������ ������������ ��� ������
                    DExtractionEquipment extractionEquipment
                        = new(
                            loadingExtractionEquipment.ObjectName,
                            coreTechnologies,
                            loadingExtractionEquipment.Size);

                    //������� ��� �� ��������� ������
                    extractionEquipments.Add(
                        extractionEquipment);

                    //�������� � ������ ������������ ��� ������ ��� ������ � ����
                    loadingExtractionEquipment.GameObjectIndex
                        = extractionEquipments.Count - 1;
                }
            }
        }

        void GameLoadEnergyGuns(
            ref WDGunEnergy[] loadingEnergyGuns,
            ref TempLoadingData tempLoadingData)
        {
            //������ ������ ��� ��������� ������
            DComponentCoreTechnology[] coreTechnologies;

            //��� ������� ������������ ��������������� ������
            for (int a = 0; a < loadingEnergyGuns.Length; a++)
            {
                //���� �������������� ������ �������� ������
                if (loadingEnergyGuns[a].IsValidObject
                    == true)
                {
                    //���� ������ �� ����������� �������������� ������
                    ref WDGunEnergy loadingEnergyGun
                        = ref loadingEnergyGuns[a];

                    //������� ��������� ������ �������� ����������
                    coreTechnologies
                        = new DComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length];

                    //��������� �������� ���������� ��������������� ������
                    GameLoadComponentCoreTechnologies(
                        in loadingEnergyGun.coreTechnologies,
                        ref coreTechnologies);

                    //���������� ������ ��������������� ������
                    DGunEnergy fuelTank
                        = new(
                            loadingEnergyGun.ObjectName,
                            coreTechnologies,
                            loadingEnergyGun.GunCaliber,
                            loadingEnergyGun.GunBarrelLength);

                    //������� ��� �� ��������� ������
                    tempLoadingData.energyGuns.Add(
                        fuelTank);

                    //�������� � ������ ��������������� ������ ��� ������ � ����
                    loadingEnergyGun.GameObjectIndex
                        = tempLoadingData.energyGuns.Count - 1;
                }
            }
        }

        void GameLoadComponentCoreTechnologies(
            in WDComponentCoreTechnology[] loadingCoreTechnologiesArray,
            ref DComponentCoreTechnology[] coreTechnologies)
        {
            //��� ������ �������� ���������� ����������
            for (int a = 0; a < loadingCoreTechnologiesArray.Length; a++)
            {
                //���� �������� ���������� �������� ������
                if (loadingCoreTechnologiesArray[a].IsValidLink
                    == true)
                {
                    //���������� ������ �������� ���������� ����������
                    DComponentCoreTechnology coreTechnology
                        = new(
                            new(loadingCoreTechnologiesArray[a].ContentObjectLink.ContentSetIndex, loadingCoreTechnologiesArray[a].ContentObjectLink.ObjectIndex),
                            loadingCoreTechnologiesArray[a].ModifierValue);

                    //������� � �� ��������������� ������� �������
                    coreTechnologies[a]
                        = coreTechnology;
                }
            }
        }

        void GameRefCalculatingEngines(
            ref DEngine[] engines,
            int contentSetIndex)
        {
            //��� ������� ���������
            for (int a = 0; a < engines.Length; a++)
            {
                //���� ������ �� ���������
                ref DEngine engine
                    = ref engines[a];

                //������������ ������ �������� ���������� ���������
                GameRefCalculatingCoreTechnologies(
                    ref engine.coreTechnologies,
                    ShipComponentType.Engine,
                    contentSetIndex,
                    a);


                //������������ �������������� ���������
                engine.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingReactors(
            ref DReactor[] reactors,
            int contentSetIndex)
        {
            //��� ������� ��������
            for (int a = 0; a < reactors.Length; a++)
            {
                //���� ������ �� �������
                ref DReactor reactor
                    = ref reactors[a];

                //������������ ������ �������� ���������� ��������
                GameRefCalculatingCoreTechnologies(
                    ref reactor.coreTechnologies,
                    ShipComponentType.Reactor,
                    contentSetIndex,
                    a);


                //������������ �������������� ��������
                reactor.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingFuelTanks(
            ref DHoldFuelTank[] fuelTanks,
            int contentSetIndex)
        {
            //��� ������� ���������� ����
            for (int a = 0; a < fuelTanks.Length; a++)
            {
                //���� ������ �� ��������� ���
                ref DHoldFuelTank fuelTank
                    = ref fuelTanks[a];

                //������������ ������ �������� ���������� ���������� ����
                GameRefCalculatingCoreTechnologies(
                    ref fuelTank.coreTechnologies,
                    ShipComponentType.HoldFuelTank,
                    contentSetIndex,
                    a);


                //������������ �������������� ���������� ����
                fuelTank.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingExtractionEquipment(
            ref DExtractionEquipment[] extractionEquipments,
            ShipComponentType extractionEquipmentType,
            int contentSetIndex)
        {
            //��� ������� ������������ ��� ������
            for (int a = 0; a < extractionEquipments.Length; a++)
            {
                //���� ������ �� ������������ ��� ������
                ref DExtractionEquipment extractionEquipment
                    = ref extractionEquipments[a];

                //������������ ������ �������� ���������� ������������ ��� ������
                GameRefCalculatingCoreTechnologies(
                    ref extractionEquipment.coreTechnologies,
                    extractionEquipmentType,
                    contentSetIndex,
                    a);

                //������������ �������������� ������������ ��� ������
                extractionEquipment.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingEnergyGuns(
            ref DGunEnergy[] energyGuns,
            int contentSetIndex)
        {
            //��� ������� ��������������� ������
            for (int a = 0; a < energyGuns.Length; a++)
            {
                //���� ������ �� �������������� ������
                ref DGunEnergy fuelTank
                    = ref energyGuns[a];

                //������������ ������ �������� ���������� ��������������� ������
                GameRefCalculatingCoreTechnologies(
                    ref fuelTank.coreTechnologies,
                    ShipComponentType.GunEnergy,
                    contentSetIndex,
                    a);


                //������������ �������������� ��������������� ������
                fuelTank.CalculateCharacteristics();
            }
        }

        void GameRefCalculatingCoreTechnologies(
            ref DComponentCoreTechnology[] coreTechnologies,
            ShipComponentType componentType,
            int componentContentSetIndex,
            int componentIndex)
        {
            //��� ������ �������� ���������� ����������
            for (int a = 0; a < coreTechnologies.Length; a++)
            {
                //���� ������ �� �������� ����������
                ref DComponentCoreTechnology coreTechnology
                    = ref coreTechnologies[a];

                //���������� ������ ������ �������� � ����
                int gameContentSetIndex
                    = contentData.Value
                    .wDContentSets[coreTechnology.ContentObjectLink.ContentSetIndex].gameContentSetIndex;

                //���������� ������ ���������� � ����
                int gameTechnologyIndex
                    = contentData.Value
                    .wDContentSets[coreTechnology.ContentObjectLink.ContentSetIndex]
                    .technologies[coreTechnology.ContentObjectLink.ObjectIndex].GameObjectIndex;

                //���� ��� ���������� - ���������
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //������� ������ �� ���� � ������ ����������, ����������� �� ����������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].engines.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - �������
                else if(componentType
                    == ShipComponentType.Reactor)
                {
                    //������� ������ �� ���� � ������ ���������, ����������� �� ����������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].reactors.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - ��������� ���
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //������� ������ �� ���� � ������ ��������� �����, ����������� �� ����������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].fuelTanks.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - ������������ ��� ������ ������
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //������� ������ �� ���� � ������ ������������ ��� ������ ������, ����������� �� ����������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].extractionEquipmentSolids.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - �������������� ������
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //������� ������ �� ���� � ������ �������������� ������, ����������� �� ����������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .technologies[gameTechnologyIndex].energyGuns.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }

                //�������������� ������
                coreTechnology.ContentObjectLink = new(gameContentSetIndex, gameTechnologyIndex);
            }
        }

        //������ ��������
        void GameLoadShipClasses(
            ref WDShipClass[] loadingShipClasses,
            ref TempLoadingData tempLoadingData)
        {
            //������ ������ ��� ��������� ������
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

            //��� ������� ������������ ������
            for (int a = 0; a < loadingShipClasses.Length; a++)
            {
                //���� ����� ������� �������� ������
                if (loadingShipClasses[a].IsValidObject
                    == true)
                {
                    //���� ������ �� ����������� �����
                    ref WDShipClass loadingShipClass
                        = ref loadingShipClasses[a];

                    //������� ��������� ������ ����������
                    shipClassEngines.Clear();
                    //��������� ��������� ������
                    GameLoadShipClassComponents(
                        in loadingShipClass.engines,
                        shipClassEngines);

                    //������� ��������� ������ ���������
                    shipClassReactors.Clear();
                    //��������� �������� ������ 
                    GameLoadShipClassComponents(
                        in loadingShipClass.reactors,
                        shipClassReactors);

                    //������� ��������� ������ ��������� �����
                    shipClassFuelTanks.Clear();
                    //��������� ��������� ���� ������
                    GameLoadShipClassComponents(
                        in loadingShipClass.fuelTanks,
                        shipClassFuelTanks);

                    //������� ��������� ������ ������������ ��� ������ ������
                    shipClassExtractionEquipmentSolids.Clear();
                    //��������� ������������ ��� ������ ������ ������
                    GameLoadShipClassComponents(
                        in loadingShipClass.extractionEquipmentSolids,
                        shipClassExtractionEquipmentSolids);

                    //������� ��������� ������ �������������� ������
                    shipClassExtractionEquipmentSolids.Clear();
                    //��������� �������������� ������ ������
                    GameLoadShipClassComponents(
                        in loadingShipClass.energyGuns,
                        shipClassEnergyGuns);

                    //���������� ������ ������ �������
                    DShipClass shipClass
                        = new(
                            loadingShipClass.ObjectName,
                            shipClassEngines.ToArray(),
                            shipClassReactors.ToArray(),
                            shipClassFuelTanks.ToArray(),
                            shipClassExtractionEquipmentSolids.ToArray(),
                            shipClassEnergyGuns.ToArray());

                    //������� ����������� ������ �� ��������� ������
                    tempLoadingData.shipClasses.Add(
                        shipClass);

                    //�������� � ������ ������ ������� ��� ������ � ����
                    loadingShipClass.GameObjectIndex
                        = tempLoadingData.shipClasses.Count - 1;
                }
            }
        }

        void GameLoadShipClassComponents(
            in WDShipClassComponent[] loadingShipClassComponentsArray,
            List<DShipClassComponent> shipClassComponents)
        {
            //��� ������� ������������ ����������
            for (int a = 0; a < loadingShipClassComponentsArray.Length; a++)
            {
                //���� ��������� �������� ������
                if (loadingShipClassComponentsArray[a].IsValidLink
                    == true)
                {
                    //���������� ������ ����������
                    DShipClassComponent shipClassComponent
                        = new(
                            loadingShipClassComponentsArray[a].ContentSetIndex,
                            loadingShipClassComponentsArray[a].ObjectIndex,
                            loadingShipClassComponentsArray[a].numberOfComponents);

                    //������� ��� � ������
                    shipClassComponents.Add(
                        shipClassComponent);
                }
            }
        }

        void GameRefCalculatingShipClasses(
            ref DShipClass[] shipClasses,
            int contentSetIndex)
        {
            //��� ������� ������ �������
            for (int a = 0; a < shipClasses.Length; a++)
            {
                //���� ������ �� ����� �������
                ref DShipClass shipClass
                    = ref shipClasses[a];

                //������������ ������ �� ���������
                GameRefCalculatingShipClassComponents(
                    ref shipClass.engines,
                    contentSetIndex,
                    a,
                    ShipComponentType.Engine);

                //������������ ������ �� ��������
                GameRefCalculatingShipClassComponents(
                    ref shipClass.reactors,
                    contentSetIndex,
                    a,
                    ShipComponentType.Reactor);

                //������������ ������ �� ��������� ����
                GameRefCalculatingShipClassComponents(
                    ref shipClass.fuelTanks,
                    contentSetIndex,
                    a,
                    ShipComponentType.HoldFuelTank);

                //������������ ������ �� ������������ ��� ������ ������
                GameRefCalculatingShipClassComponents(
                    ref shipClass.extractionEquipmentSolids,
                    contentSetIndex,
                    a,
                    ShipComponentType.ExtractionEquipmentSolid);

                //������������ ������ �� �������������� ������
                GameRefCalculatingShipClassComponents(
                    ref shipClass.energyGuns,
                    contentSetIndex,
                    a,
                    ShipComponentType.GunEnergy);

                //������������ �������������� ������ �������
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
            //��� ������� ���������� �������
            for (int a = 0; a < shipClassComponents.Length; a++)
            {
                //���� ������ �� ���������
                ref DShipClassComponent shipClassComponent
                    = ref shipClassComponents[a];

                //���������� ������ ������ �������� � ����
                int gameContentSetIndex
                    = contentData.Value
                    .wDContentSets[shipClassComponent.ContentSetIndex].gameContentSetIndex;

                //���������� ������ ���������� � ����
                int gameComponentIndex
                    = 0;
                //���� ��� ���������� - ���������
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //���������� ������ ���������� � ����
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .engines[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .engines[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //�����, ���� ��� ���������� - �������
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //���������� ������ ���������� � ����
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .reactors[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .reactors[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //�����, ���� ��� ���������� - ��������� ���
                else if(componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //���������� ������ ���������� � ����
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .fuelTanks[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .fuelTanks[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //�����, ���� ��� ���������� - ������������ ��� ������ ������
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //���������� ������ ���������� � ����
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .solidExtractionEquipments[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //�����, ���� ��� ���������� - �������������� ������
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //���������� ������ ���������� � ����
                    gameComponentIndex
                        = contentData.Value
                        .wDContentSets[shipClassComponent.ContentSetIndex]
                        .energyGuns[shipClassComponent.ObjectIndex].GameObjectIndex;

                    //��������� � ������ ����������, ��� �� ���� ��������� ������� ����� �������
                    contentData.Value
                        .contentSets[gameContentSetIndex]
                        .energyGuns[gameComponentIndex].ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }

                //�������������� ������ ������ ��������
                shipClassComponent.ContentSetIndex
                    = gameContentSetIndex;

                //� ������ ����������
                shipClassComponent.ObjectIndex
                    = gameComponentIndex;
            }
        }


        //������ �������
        TechnologyComponentCoreModifierType TechnologyDefineComponentCoreModifierType(
            string modifierName)
        {
            //��� ������� �������� � ������� �������� �������� ������������� �����������
            for (int a = 0; a < contentData.Value.technologyComponentCoreModifiersNames.Length; a++)
            {
                //���� �������� ������������ ���������
                if (contentData.Value.technologyComponentCoreModifiersNames[a]
                    == modifierName)
                {
                    //���������� ��������������� ��� ��������� ������������ ����������
                    return (TechnologyComponentCoreModifierType)a;
                }
            }

            //���������� ������ ��� ��������� ������������ ����������
            return TechnologyComponentCoreModifierType.None;
        }

        TaskForceBattleGroup ShipTypeDefineTaskForceBattleGroup(
            string battleGroupName)
        {
            //���� �������� ������ ������ ������������� �������� ������ ������� ���������
            if (battleGroupName == "LongRange")
            {
                return TaskForceBattleGroup.LongRange;
            }
            //�����, ���� �������� ������ ������ ������������� �������� ������ ������� ���������
            else if (battleGroupName == "MediumRange")
            {
                return TaskForceBattleGroup.MediumRange;
            }
            //����� ���������� ����������� ������ - ����� ���������
            else //"ShortRange"
            {
                return TaskForceBattleGroup.ShortRange;
            }
        }
    }
}