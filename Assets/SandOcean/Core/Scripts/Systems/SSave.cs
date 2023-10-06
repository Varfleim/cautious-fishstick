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
using SandOcean.Warfare.Ship;

namespace SandOcean
{
    public class SSave : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //����� �������
        readonly EcsFilterInject<Inc<RSaveContentSet>> saveContentSetRequestFilter = default;
        readonly EcsPoolInject<RSaveContentSet> saveContentSetRequestPool = default;

        //������
        readonly EcsCustomInject<ContentData> contentData = default;
        readonly EcsCustomInject<DesignerData> designerData = default;

        public void Run(IEcsSystems systems)
        {
            //���������, �� ��������� �� ��������� ������ ������� ��������
            SaveContentSets();
        }

        void SaveContentSets()
        {
            //��� ������� ������� ���������� ������ ������ ��������
            foreach (int saveRequestEntity in saveContentSetRequestFilter.Value)
            {
                //���� ��������� �������
                ref RSaveContentSet saveRequest = ref saveContentSetRequestPool.Value.Get(saveRequestEntity);

                //��������� ���� ��� ����������
                string path = "";

                //���� ���� �� �������� ������ ��������
                path = contentData.Value.wDContentSetDescriptions[saveRequest.contentSetIndex].contentSetDirectoryPath;

                //����������� ���������� ������ ��������
                WorkshopSaveContentSet(
                    saveRequest.contentSetIndex,
                    path);

                world.Value.DelEntity(saveRequestEntity);
            }
        }

        //������ ����������
        void WorkshopSaveContentSet(
            int contentSetIndex,
            string path)
        {
            //�������� ���� � ����� ������ ��������
            path = Path.Combine(path, "ContentSet.json");

            //���� ������ �� ����������� ����� ��������
            ref readonly WDContentSet contentSet = ref contentData.Value.wDContentSets[contentSetIndex];

            //������ ����� ������������ ������ ��������
            SDContentSetClass contentSetClass
                = new()
                {
                    contentSet = new(
                        new SDTechnology[contentSet.technologies.Length],
                        new SDBuildingType[contentSet.buildingTypes.Length],
                        new SDShipType[contentSet.shipTypes.Length],
                        new SDShipPart[contentSet.shipParts.Length],
                        new SDShipPartCoreTechnology[contentSet.shipPartCoreTechnologies.Length],
                        new SDShipPartTypeDirectionOfImprovement[contentSet.shipPartDirectionsOfImprovement.Length],
                        new SDShipPartImprovement[contentSet.shipPartImprovements.Length],
                        new SDShipClass[contentSet.shipClasses.Length],
                        new SDEngine[contentSet.engines.Length],
                        new SDReactor[contentSet.reactors.Length],
                        new SDHoldFuelTank[contentSet.fuelTanks.Length],
                        new SDExtractionEquipment[contentSet.solidExtractionEquipments.Length],
                        new SDGunEnergy[contentSet.energyGuns.Length])
                };

            //��������� ����������
            WorkshopSaveTechnologies(
                in contentSet.technologies,
                ref contentSetClass.contentSet.technologies);

            //��������� ���� ����������
            WorkshopSaveBuildingTypes(
                in contentSet.buildingTypes,
                ref contentSetClass.contentSet.buildingTypes);

            //��������� ���� ��������
            WorkshopSaveShipTypes(
                in contentSet.shipTypes,
                ref contentSetClass.contentSet.shipTypes);

            //��������� ����� ��������
            WorkshopSaveShipPart(
                contentSet.shipParts,
                ref contentSetClass.contentSet.shipParts);

            //��������� �������� ���������� ������ ��������
            WorkshopSaveShipPartCoreTechnologies(
                contentSet.shipPartCoreTechnologies,
                ref contentSetClass.contentSet.shipPartCoreTechnologies);

            //��������� ����������� ��������� ������ ��������
            WorkshopSaveShipPartDirectionsOfImprovement(
                contentSet.shipPartDirectionsOfImprovement,
                ref contentSetClass.contentSet.shipPartDirectionsOfImprovement);

            //��������� ��������� ������ ��������
            WorkshopSaveShipPartImprovements(
                contentSet.shipPartImprovements,
                ref contentSetClass.contentSet.shipPartImprovements);

            //��������� ������ ��������
            WorkshopSaveShipClasses(
                in contentSet.shipClasses,
                ref contentSetClass.contentSet.shipClasses);

            //��������� ���������
            WorkshopSaveEngines(
                in contentSet.engines,
                ref contentSetClass.contentSet.engines);

            //��������� ��������
            WorkshopSaveReactors(
                in contentSet.reactors,
                ref contentSetClass.contentSet.reactors);

            //��������� ��������� ����
            WorkshopSaveFuelTank(
                in contentSet.fuelTanks,
                ref contentSetClass.contentSet.fuelTanks);

            //��������� ������������ ��� ������ ������
            WorkshopSaveExtractionEquipment(
                in contentSet.solidExtractionEquipments,
                ref designerData.Value.solidExtractionEquipmentCoreModifierTypes,
                ref contentSetClass.contentSet.extractionEquipmentSolids);

            //��������� �������������� ������
            WorkshopSaveEnergyGuns(
                in contentSet.energyGuns,
                ref contentSetClass.contentSet.energyGuns);


            //��������� ����� �������� � ����
            File.WriteAllText(
                path,
                JsonUtility.ToJson(
                    contentSetClass,
                    true));
        }

        //����������
        void WorkshopSaveTechnologies(
            in WDTechnology[] savingTechnologies,
            ref SDTechnology[] technologies)
        {
            //������ ������ ��� ��������� ������
            List<SDTechnologyModifier> technologyModifiers
                = new();
            List<SDTechnologyComponentCoreModifier> technologyComponentCoreModifiers
                = new();

            //��� ������ ����������� ����������
            for (int a = 0; a < savingTechnologies.Length; a++)
            {
                //���� ������ �� ����������� ����������
                ref readonly WDTechnology savingTechnology
                    = ref savingTechnologies[a];

                //������� ��������� ������ �������������
                technologyModifiers.Clear();
                //��� ������� ������������ ����������
                for (int b = 0; b < savingTechnology.technologyModifiers.Length; b++)
                {
                    //���������� ����������� ������ ������������
                    SDTechnologyModifier technologyModifier
                        = new(
                            savingTechnology.technologyModifiers[b].modifierName,
                            savingTechnology.technologyModifiers[b].ModifierValue);

                    //������� ��� �� ��������� ������
                    technologyModifiers.Add(
                        technologyModifier);
                }

                //������� ��������� ������ �������� ������������� �����������
                technologyComponentCoreModifiers.Clear();
                //��� ������� ��������� ������������ ����������
                for (int b = 0; b < savingTechnology.technologyComponentCoreModifiers.Length; b++)
                {
                    //���������� ����������� ������ ������������
                    SDTechnologyComponentCoreModifier technologyComponentCoreModifier
                        = new(
                            savingTechnology.technologyComponentCoreModifiers[b].modifierName,
                            savingTechnology.technologyComponentCoreModifiers[b].ModifierValue);

                    //������� ��� �� ��������� ������
                    technologyComponentCoreModifiers.Add(
                        technologyComponentCoreModifier);
                }

                //���������� ����������� ������ ����������
                SDTechnology technology
                    = new(
                        savingTechnology.ObjectName,
                        savingTechnology.IsBaseTechnology,
                        technologyModifiers.ToArray(),
                        technologyComponentCoreModifiers.ToArray());

                //������� � � ����������� ������ �� ���������������� �������
                technologies[a]
                    = technology;
            }
        }

        //���� ����������
        void WorkshopSaveBuildingTypes(
            in WDBuildingType[] savingBuildingTypes,
            ref SDBuildingType[] buildingTypes)
        {
            //��� ������� ������������ ���� ����������
            for (int a = 0; a < savingBuildingTypes.Length; a++)
            {
                //���� ������ �� ����������� ���
                ref readonly WDBuildingType savingBuildingType = ref savingBuildingTypes[a];

                //���������� ��������� ����
                string buildingCategoryName;

                //���� ��� ��������� � �������� ���������
                if (savingBuildingType.BuildingCategory == BuildingCategory.Test)
                {
                    buildingCategoryName = "Test";
                }
                //����� ��� ��������� � �������� ���������
                else
                {
                    buildingCategoryName = "Test";
                }

                //���������� ������ ����
                SDBuildingType buildingType = new(
                    savingBuildingType.ObjectName,
                    buildingCategoryName);

                //������� ��� � ����������� ������ �� ���������������� �������
                buildingTypes[a] = buildingType;
            }
        }

        #region Ships
        //���� ��������
        void WorkshopSaveShipTypes(
            in WDShipType[] savingShipTypes,
            ref SDShipType[] shipTypes)
        {
            //��� ������� ������������ ���� �������
            for (int a = 0; a < savingShipTypes.Length; a++)
            {
                //���� ������ �� ����������� ��� 
                ref readonly WDShipType savingShipType = ref savingShipTypes[a];

                //���������� ������ ������ ����
                string battleGroupName;

                //���� ��� ��������� � ������ ������ ������� ���������
                if(savingShipType.BattleGroup == TaskForceBattleGroup.LongRange)
                {
                    battleGroupName = "LongRange";
                }
                //�����, ���� ��� ��������� � ������ ������ ������� ���������
                else if(savingShipType.BattleGroup == TaskForceBattleGroup.MediumRange)
                {
                    battleGroupName = "MediumRange";
                }
                //����� ��� ��������� � ������ ������ ����� ���������
                else
                {
                    battleGroupName = "ShortRange";
                }

                //���������� ������ ����
                SDShipType shipType = new(
                    savingShipType.ObjectName,
                    battleGroupName);

                //������� ��� � ����������� ������ �� ���������������� �������
                shipTypes[a] = shipType;
            }
        }

        #region ShipParts
        void WorkshopSaveShipPart(
            WDShipPart[] savingShipParts,
            ref SDShipPart[] shipParts)
        {
            //������ ������ ��� ��������� ������
            List<SDContentObjectLink> coreTechnologies = new();

            //��� ������ ����������� ����� �������
            for (int a = 0; a < savingShipParts.Length; a++)
            {
                //���� ����������� ����� �������
                WDShipPart savingShipPart = savingShipParts[a];

                //������� ��������� ������
                coreTechnologies.Clear();

                //��� ������ �������� ����������
                for (int b = 0; b < savingShipPart.CoreTechnologies.Length; b++)
                {
                    //�������� �������� ������ � WDContentObjectLink
                    if (savingShipPart.CoreTechnologies[b] is WorkshopContentObjectLink savingCoreTechnologyLink)
                    {
                        //���������� ����������� ������ �������� ����������
                        SDContentObjectLink coreTechnologyLink = new(
                            savingCoreTechnologyLink.ContentSetName,
                            savingCoreTechnologyLink.ObjectName);

                        //������� � �� ��������� ������
                        coreTechnologies.Add(coreTechnologyLink);
                    }
                }

                //���������� ����������� ������ ����� �������
                SDShipPart shipPart = new(
                    savingShipPart.ObjectName,
                    coreTechnologies.ToArray());

                //������� � � ����������� ������ �� ���������������� �������
                shipParts[a] = shipPart;
            } 
        }

        void WorkshopSaveShipPartCoreTechnologies(
            WDShipPartCoreTechnology[] savingCoreTechnologies,
            ref SDShipPartCoreTechnology[] coreTechnologies)
        {
            //������ ������ ��� ��������� ������
            List<SDContentObjectLink> directionsOfImprovement = new();

            //��� ������ ����������� �������� ���������� ����� �������
            for (int a = 0; a < savingCoreTechnologies.Length; a++)
            {
                //���� ����������� �������� ����������
                WDShipPartCoreTechnology savingCoreTechnology = savingCoreTechnologies[a];

                //������� ��������� ������
                directionsOfImprovement.Clear();

                //��� ������� ����������� ���������
                for (int b = 0; b < savingCoreTechnology.DirectionsOfImprovement.Length; b++)
                {
                    //�������� �������� ������ � WDContentObjectLink
                    if (savingCoreTechnology.DirectionsOfImprovement[b] is WorkshopContentObjectLink savingDirectionOfImprovementLink)
                    {
                        //���������� ����������� ������ ����������� ���������
                        SDContentObjectLink coreTechnologyLink = new(
                            savingDirectionOfImprovementLink.ContentSetName,
                            savingDirectionOfImprovementLink.ObjectName);

                        //������� ��� �� ��������� ������
                        directionsOfImprovement.Add(coreTechnologyLink);
                    }
                }

                //���������� ����������� ������ �������� ����������
                SDShipPartCoreTechnology coreTechnology = new(
                    savingCoreTechnology.ObjectName,
                    directionsOfImprovement.ToArray());

                //������� � � ����������� ������ �� ���������������� �������
                coreTechnologies[a] = coreTechnology;
            }
        }

        void WorkshopSaveShipPartDirectionsOfImprovement(
            WDShipPartDirectionOfImprovement[] savingDirectionsOfImprovement,
            ref SDShipPartTypeDirectionOfImprovement[] directionsOfImprovement)
        {
            //������ ������ ��� ��������� ������
            List<SDContentObjectLink> improvements = new();

            //��� ������� ������������ ����������� ��������� ����� �������
            for (int a = 0; a < savingDirectionsOfImprovement.Length; a++)
            {
                //���� ����������� ����������� ���������
                WDShipPartDirectionOfImprovement savingDirectionOfImprovement = savingDirectionsOfImprovement[a];

                //������� ��������� ������
                improvements.Clear();

                //��� ������� ���������
                for (int b = 0; b < savingDirectionOfImprovement.Improvements.Length; b++)
                {
                    //�������� �������� ������ � WDContentObjectLink
                    if (savingDirectionOfImprovement.Improvements[b] is WorkshopContentObjectLink savingImprovementLink)
                    {
                        //���������� ����������� ������ ���������
                        SDContentObjectLink coreTechnologyLink = new(
                            savingImprovementLink.ContentSetName,
                            savingImprovementLink.ObjectName);

                        //������� ��� �� ��������� ������
                        improvements.Add(coreTechnologyLink);
                    }
                }

                //���������� ����������� ������ ����������� ���������
                SDShipPartTypeDirectionOfImprovement directionOfImprovement = new(
                    savingDirectionOfImprovement.ObjectName,
                    improvements.ToArray());

                //������� ��� � ����������� ������ �� ���������������� �������
                directionsOfImprovement[a] = directionOfImprovement;
            }
        }

        void WorkshopSaveShipPartImprovements(
            WDShipPartImprovement[] savingImprovements,
            ref SDShipPartImprovement[] improvements)
        {
            //��� ������� ������������ ��������� ����� �������
            for (int a = 0; a < savingImprovements.Length; a++)
            {
                //���� ����������� ���������
                WDShipPartImprovement savingImprovement = savingImprovements[a];

                //���������� ����������� ������ ����������� ���������
                SDShipPartImprovement improvement = new(
                    savingImprovement.ObjectName);

                //������� ��� � ����������� ������ �� ���������������� �������
                improvements[a] = improvement;
            }
        }
        #endregion

        //������ ��������
        void WorkshopSaveShipClasses(
            in WDShipClass[] savingShipClasses,
            ref SDShipClass[] shipClasses)
        {
            //������ ������ ��� ��������� ������
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
            List<SDShipClassPart> shipParts = new();
            List<SDContentObjectLink> shipPartImprovements = new();

            //��� ������� ������������ ������ �������
            for (int a = 0; a < savingShipClasses.Length; a++)
            {
                //���� ������ �� ����������� �������
                ref readonly WDShipClass savingShipClass
                    = ref savingShipClasses[a];

                //������� ��������� ������ ����������
                installedEngines.Clear();
                //��� ������� ���������
                for (int b = 0; b < savingShipClass.engines.Length; b++)
                {
                    //���������� ����������� ������ ���������
                    SDShipClassComponent engine
                        = new(
                            savingShipClass.engines[b].contentSetName,
                            savingShipClass.engines[b].componentName,
                            savingShipClass.engines[b].numberOfComponents);

                    //������� ��� �� ��������� ������
                    installedEngines.Add(
                        engine);
                }

                //������� ��������� ������ ���������
                installedReactors.Clear();
                //��� ������� ��������
                for (int b = 0; b < savingShipClass.reactors.Length; b++)
                {
                    //���������� ����������� ������ ��������
                    SDShipClassComponent reactor
                        = new(
                            savingShipClass.reactors[b].contentSetName,
                            savingShipClass.reactors[b].componentName,
                            savingShipClass.reactors[b].numberOfComponents);

                    //������� ��� �� ��������� ������
                    installedReactors.Add(
                        reactor);
                }

                //������� ��������� ������ ��������� �����
                installedFuelTanks.Clear();
                //��� ������� ���������� ����
                for (int b = 0; b < savingShipClass.fuelTanks.Length; b++)
                {
                    //���������� ����������� ������ ���������� ����
                    SDShipClassComponent fuelTank
                        = new(
                            savingShipClass.fuelTanks[b].contentSetName,
                            savingShipClass.fuelTanks[b].componentName,
                            savingShipClass.fuelTanks[b].numberOfComponents);

                    //������� ��� �� ��������� ������
                    installedFuelTanks.Add(
                        fuelTank);
                }

                //������� ��������� ������ ������������ ��� ������ ������
                installedSolidExtractionEquipments.Clear();
                //��� ������� ������������ ��� ������ ������
                for (int b = 0; b < savingShipClass.extractionEquipmentSolids.Length; b++)
                {
                    //���������� ����������� ������ ������������ ��� ������ ������
                    SDShipClassComponent solidExtractionEquipment
                        = new(
                            savingShipClass.extractionEquipmentSolids[b].contentSetName,
                            savingShipClass.extractionEquipmentSolids[b].componentName,
                            savingShipClass.extractionEquipmentSolids[b].numberOfComponents);

                    //������� ��� �� ��������� ������
                    installedSolidExtractionEquipments.Add(
                        solidExtractionEquipment);
                }

                //������� ��������� ������ �������������� ������
                installedEnergyGuns.Clear();
                //��� ������� ��������������� ������
                for (int b = 0; b < savingShipClass.energyGuns.Length; b++)
                {
                    //���������� ����������� ������ ��������������� ������
                    SDShipClassComponent energyGun
                        = new(
                            savingShipClass.energyGuns[b].contentSetName,
                            savingShipClass.energyGuns[b].componentName,
                            savingShipClass.energyGuns[b].numberOfComponents);

                    //������� ��� �� ��������� ������
                    installedEnergyGuns.Add(
                        energyGun);
                }

                //������� ��������� ������ ������ �������
                shipParts.Clear();
                //��� ������ ����� �������
                for (int b = 0; b < savingShipClass.shipParts.Length; b++)
                {
                    //���� ����������� ����� �������
                    WDShipClassPart savingShipPart = savingShipClass.shipParts[b];

                    //������� ��������� ������ ��������� ����� �������
                    shipPartImprovements.Clear();

                    //��� ������� ������������ ���������
                    for (int c = 0; c < savingShipClass.shipParts[b].Improvements.Length; c++)
                    {
                        //�������� �������� ������ � WDContentObjectLink
                        if (savingShipClass.shipParts[b].Improvements[c] is WorkshopContentObjectLink savingImprovementLink)
                        {
                            //���������� ����������� ������ ���������
                            SDContentObjectLink improvement = new(
                                savingImprovementLink.ContentSetName, savingImprovementLink.ObjectName);

                            //������� ��� � ������
                            shipPartImprovements.Add(improvement);
                        }
                    }

                    //���������� ����������� ������ ����� �������
                    SDShipClassPart shipPart = new(
                        new SDContentObjectLink((savingShipPart.Part as WorkshopContentObjectLink).ContentSetName, (savingShipPart.Part as WorkshopContentObjectLink).ObjectName),
                        new SDContentObjectLink((savingShipPart.CoreTechnology as WorkshopContentObjectLink).ContentSetName, (savingShipPart.CoreTechnology as WorkshopContentObjectLink).ObjectName),
                        shipPartImprovements.ToArray());

                    //������� � � ������
                    shipParts.Add(shipPart);
                }

                //���������� ����������� ������ �������
                SDShipClass shipClass
                    = new(
                        savingShipClass.ObjectName,
                        installedEngines.ToArray(),
                        installedReactors.ToArray(),
                        installedFuelTanks.ToArray(),
                        installedSolidExtractionEquipments.ToArray(),
                        installedEnergyGuns.ToArray(),
                        shipParts.ToArray());

                //������� ��� � ����������� ������ �� ���������������� �������
                shipClasses[a]
                    = shipClass;
            }
        }

        //���������� ��������
        void WorkshopSaveEngines(
            in WDEngine[] savingEngines,
            ref SDEngine[] engines)
        {
            //������ ������ ��� ��������� ������
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length]; 

            //��� ������� ������������ ���������
            for (int a = 0; a < savingEngines.Length; a++)
            {
                //���� ������ �� ����������� ���������
                ref readonly WDEngine savingEngine
                    = ref savingEngines[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new SDComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length];
                //��� ������ �������� ����������
                for (int b = 0; b < savingEngine.coreTechnologies.Length; b++)
                {
                    //���������� ����������� ������ �������� ����������
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingEngine.coreTechnologies[b].modifierName,
                            savingEngine.coreTechnologies[b].contentSetName,
                            savingEngine.coreTechnologies[b].technologyName);

                    //������� � �� ��������� ������
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //���������� ����������� ������ ���������
                SDEngine engine
                    = new(
                        savingEngine.ObjectName,
                        coreTechnologies,
                        savingEngine.EngineSize,
                        savingEngine.EngineBoost);

                //������� ��� � ����������� ������ �� ���������������� �������
                engines[a]
                    = engine;
            }
        }

        void WorkshopSaveReactors(
            in WDReactor[] savingReactors,
            ref SDReactor[] reactors)
        {
            //������ ������ ��� ��������� ������
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length];

            //��� ������� ������������ ��������
            for (int a = 0; a < savingReactors.Length; a++)
            {
                //���� ������ �� ����������� �������
                ref readonly WDReactor savingReactor
                    = ref savingReactors[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new SDComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length];
                //��� ������ �������� ����������
                for (int b = 0; b < savingReactor.coreTechnologies.Length; b++)
                {
                    //���������� ����������� ������ �������� ����������
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingReactor.coreTechnologies[b].modifierName,
                            savingReactor.coreTechnologies[b].contentSetName,
                            savingReactor.coreTechnologies[b].technologyName);

                    //������� � �� ��������� ������
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //���������� ����������� ������ ��������
                SDReactor reactor
                    = new(
                        savingReactor.ObjectName,
                        coreTechnologies,
                        savingReactor.ReactorSize,
                        savingReactor.ReactorBoost);

                //������� ��� � ����������� ������ �� ���������������� �������
                reactors[a]
                    = reactor;
            }
        }

        void WorkshopSaveFuelTank(
            in WDHoldFuelTank[] savingFuelTanks,
            ref SDHoldFuelTank[] fuelTanks)
        {
            //������ ������ ��� ��������� ������
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length];

            //��� ������� ������������ ���������� ����
            for (int a = 0; a < savingFuelTanks.Length; a++)
            {
                //���� ������ �� ����������� ��������� ���
                ref readonly WDHoldFuelTank savingFuelTank
                    = ref savingFuelTanks[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new SDComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length];
                //��� ������ �������� ����������
                for (int b = 0; b < savingFuelTank.coreTechnologies.Length; b++)
                {
                    //���������� ����������� ������ �������� ����������
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingFuelTank.coreTechnologies[b].modifierName,
                            savingFuelTank.coreTechnologies[b].contentSetName,
                            savingFuelTank.coreTechnologies[b].technologyName);

                    //������� � �� ��������� ������
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //���������� ����������� ������ ���������� ����
                SDHoldFuelTank fuelTank
                    = new(
                        savingFuelTank.ObjectName,
                        coreTechnologies,
                        savingFuelTank.Size);

                //������� ��� � ����������� ������ �� ���������������� �������
                fuelTanks[a]
                    = fuelTank;
            }
        }

        void WorkshopSaveExtractionEquipment(
            in WDExtractionEquipment[] savingExtractionEquipments,
            ref TechnologyComponentCoreModifierType[] extractionEquipmentCoreModifierTypes,
            ref SDExtractionEquipment[] extractionEquipments)
        {
            //������ ������ ��� ��������� ������
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[extractionEquipmentCoreModifierTypes.Length];

            //��� ������� ������������ ������������ ��� ������
            for (int a = 0; a < savingExtractionEquipments.Length; a++)
            {
                //���� ������ �� ����������� ������������ ��� ������
                ref readonly WDExtractionEquipment savingExtractionEquipment
                    = ref savingExtractionEquipments[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new SDComponentCoreTechnology[extractionEquipmentCoreModifierTypes.Length];
                //��� ������ �������� ����������
                for (int b = 0; b < savingExtractionEquipment.coreTechnologies.Length; b++)
                {
                    //���������� ����������� ������ �������� ����������
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingExtractionEquipment.coreTechnologies[b].modifierName,
                            savingExtractionEquipment.coreTechnologies[b].contentSetName,
                            savingExtractionEquipment.coreTechnologies[b].technologyName);

                    //������� � �� ��������� ������
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //���������� ����������� ������ ������������ ��� ������
                SDExtractionEquipment extractionEquipment
                    = new(
                        savingExtractionEquipment.ObjectName,
                        coreTechnologies,
                        savingExtractionEquipment.Size);

                //������� ��� � ����������� ������ �� ���������������� �������
                extractionEquipments[a]
                    = extractionEquipment;
            }
        }

        void WorkshopSaveEnergyGuns(
            in WDGunEnergy[] savingEnergyGuns,
            ref SDGunEnergy[] energyGuns)
        {
            //������ ������ ��� ��������� ������
            SDComponentCoreTechnology[] coreTechnologies
                = new SDComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length];

            //��� ������� ������������ ��������������� ������
            for (int a = 0; a < savingEnergyGuns.Length; a++)
            {
                //���� ������ �� ����������� �������������� ������
                ref readonly WDGunEnergy savingEnergyGun
                    = ref savingEnergyGuns[a];

                //������� ��������� ������ �������� ����������
                coreTechnologies
                    = new SDComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length];
                //��� ������ �������� ����������
                for (int b = 0; b < savingEnergyGun.coreTechnologies.Length; b++)
                {
                    //���������� ����������� ������ �������� ����������
                    SDComponentCoreTechnology coreTechnology
                        = new(
                            savingEnergyGun.coreTechnologies[b].modifierName,
                            savingEnergyGun.coreTechnologies[b].contentSetName,
                            savingEnergyGun.coreTechnologies[b].technologyName);

                    //������� � �� ��������� ������
                    coreTechnologies[b]
                        = coreTechnology;
                }

                //���������� ����������� ������ ��������������� ������
                SDGunEnergy energyGun
                    = new(
                        savingEnergyGun.ObjectName,
                        coreTechnologies,
                        savingEnergyGun.GunCaliber,
                        savingEnergyGun.GunBarrelLength);

                //������� ��� � ����������� ������ �� ���������������� �������
                energyGuns[a]
                    = energyGun;
            }
        }

        void SaveComponentCoreTechnologies(
            in DComponentCoreTechnology[] savingComponentCoreTechnologies,
            in TechnologyComponentCoreModifierType[] componentCoreModifierTypes,
            ref SDComponentCoreTechnology[] componentCoreTechnologies)
        {
            //��� ������ �������� ���������� ����������
            for (int a = 0; a < savingComponentCoreTechnologies.Length; a++)
            {
                //���� ���������� ����������
                if (savingComponentCoreTechnologies[a].ContentObjectLink.ContentSetIndex
                    >= 0
                    && savingComponentCoreTechnologies[a].ContentObjectLink.ObjectIndex
                    >= 0)
                {
                    //���� ������ �� ������ ����������
                    ref readonly DTechnology technology
                        = ref contentData.Value
                        .contentSets[savingComponentCoreTechnologies[a].ContentObjectLink.ContentSetIndex]
                        .technologies[savingComponentCoreTechnologies[a].ContentObjectLink.ObjectIndex];

                    //���� �������� ������ ��������
                    string contentSetName
                        = contentData.Value
                        .contentSets[savingComponentCoreTechnologies[a].ContentObjectLink.ContentSetIndex].ContentSetName;

                    //������ ����� ��������� ��� ����������� ������ �������� ����������
                    SDComponentCoreTechnology componentCoreTechnology
                        = new(
                            contentData.Value.technologyComponentCoreModifiersNames[(int)componentCoreModifierTypes[a]],
                            contentSetName,
                            technology.ObjectName);

                    //������� � � ������ �������� ����������
                    componentCoreTechnologies[a]
                        = componentCoreTechnology;
                }
            }
        }
        #endregion
    }
}