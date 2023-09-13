using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using SandOcean.Designer;
using SandOcean.Designer.Game;
using SandOcean.UI.DesignerWindow;
using SandOcean.UI.DesignerWindow.ShipClassDesigner;
using SandOcean.UI.DesignerWindow.ReactorDesigner;
using SandOcean.UI.DesignerWindow.EngineDesigner;
using SandOcean.UI.DesignerWindow.FuelTankDesigner;
using SandOcean.UI.DesignerWindow.ExtractionEquipmentDesigner;
using SandOcean.UI.DesignerWindow.GunDesigner;

namespace SandOcean.UI
{
    public class UIDesignerWindow : MonoBehaviour
    {
        public TextMeshProUGUI designerTypeName;
        public DesignerType designerType = DesignerType.None;
        public bool isInGameDesigner;

        public int currentContentSetIndex;


        public GameObject activeDesigner;

        public UIShipClassDesignerWindow shipClassDesigner;
        //public UIComponentDesignerWindow componentDesigner;
        public UIEngineDesignerWindow engineDesigner;
        public UIReactorDesignerWindow reactorDesigner;
        public UIFuelTankDesignerWindow fuelTankDesigner;
        public UIExtractionEquipmentDesignerWindow extractionEquipmentDesigner;
        public UIGunEnergyDesignerWindow energyGunDesigner;


        public UIDesignerOtherContentSetsList otherContentSetsList;
        public UIDesignerCurrentContentSetList currentContentSetList;


        public UIShipClassBriefInfoPanel shipClassBriefInfoPanelPrefab;
        public UIEngineBriefInfoPanel engineBriefInfoPanelPrefab;
        public UIReactorBriefInfoPanel reactorBriefInfoPanelPrefab;
        public UIFuelTankBriefInfoPanel fuelTankBriefInfoPanelPrefab;
        public UIExtractionEquipmentBriefInfoPanel extractionEquipmentBriefInfoPanelPrefab;
        public UIGunEnergyBriefInfoPanel energyGunBriefInfoPanelPrefab;

        public UIComponentParameterPanel componentParameterPanelPrefab;

        public UIShipClassBriefInfoPanel InstantiateShipBriefInfoPanel(
            IContentObject shipClassContentObject,
            int contentSetIndex, int classIndex,
            ToggleGroup parentToggleGroup,
            VerticalLayoutGroup parentLayoutGroup,
            List<GameObject> parentPanelsList)
        {
            UIShipClassBriefInfoPanel shipClassBriefInfoPanel 
                = Instantiate(shipClassBriefInfoPanelPrefab);

            //��������� ������ ������ ��������
            shipClassBriefInfoPanel.contentSetIndex
                = contentSetIndex;

            //��������� ������ ������
            shipClassBriefInfoPanel.objectIndex
                = classIndex;

            //��������� �������� ������
            shipClassBriefInfoPanel.objectName.text
                = shipClassContentObject.ObjectName;

            //��������� ������������ ������� ����������� ������
            shipClassBriefInfoPanel.panelToggle.group 
                = parentToggleGroup;
            parentPanelsList.Add(
                shipClassBriefInfoPanel.gameObject);
            shipClassBriefInfoPanel.transform.SetParent(
                parentLayoutGroup.transform);

            return shipClassBriefInfoPanel;
        }

        public UIEngineBriefInfoPanel InstantiateEngineBriefInfoPanel(
            IContentObject engineContentObject,
            int contentSetIndex, int modelIndex,
            ToggleGroup parentToggleGroup,
            VerticalLayoutGroup parentLayoutGroup,
            List<GameObject> parentPanelsList)
        {
            UIEngineBriefInfoPanel engineBriefInfoPanel 
                = Instantiate(engineBriefInfoPanelPrefab);

            //��������� ������ ������ ��������
            engineBriefInfoPanel.contentSetIndex 
                = contentSetIndex;

            //��������� ������ ���������
            engineBriefInfoPanel.objectIndex
                = modelIndex;

            //��������� �������� ���������
            engineBriefInfoPanel.objectName.text
                = engineContentObject.ObjectName;


            //��������� ������������ ������� ����������� ������
            engineBriefInfoPanel.panelToggle.group 
                = parentToggleGroup;
            parentPanelsList.Add(
                engineBriefInfoPanel.gameObject);
            engineBriefInfoPanel.transform.SetParent(
                parentLayoutGroup.transform);

            return engineBriefInfoPanel;
        }

        public UIReactorBriefInfoPanel InstantiateReactorBriefInfoPanel(
            IContentObject reactorContentObject,
            int contentSetIndex, int modelIndex,
            ToggleGroup parentToggleGroup,
            VerticalLayoutGroup parentLayoutGroup,
            List<GameObject> parentPanelsList)
        {
            UIReactorBriefInfoPanel reactorBriefInfoPanel
                = Instantiate(reactorBriefInfoPanelPrefab);

            //��������� ������ ������ ��������
            reactorBriefInfoPanel.contentSetIndex
                = contentSetIndex;

            //��������� ������ ��������
            reactorBriefInfoPanel.objectIndex
                = modelIndex;

            //��������� �������� ��������
            reactorBriefInfoPanel.objectName.text
                = reactorContentObject.ObjectName;


            //��������� ������������ ������� ����������� ������
            reactorBriefInfoPanel.panelToggle.group
                = parentToggleGroup;
            parentPanelsList.Add(
                reactorBriefInfoPanel.gameObject);
            reactorBriefInfoPanel.transform.SetParent(
                parentLayoutGroup.transform);

            return reactorBriefInfoPanel;
        }

        public UIFuelTankBriefInfoPanel InstantiateFuelTankBriefInfoPanel(
            IContentObject fuelTankContentObject,
            int contentSetIndex, int modelIndex,
            ToggleGroup parentToggleGroup,
            VerticalLayoutGroup parentLayoutGroup,
            List<GameObject> parentPanelsList)
        {
            UIFuelTankBriefInfoPanel fuelTankBriefInfoPanel
                = Instantiate(fuelTankBriefInfoPanelPrefab);

            //��������� ������ ������ ��������
            fuelTankBriefInfoPanel.contentSetIndex
                = contentSetIndex;

            //��������� ������ ���������� ����
            fuelTankBriefInfoPanel.objectIndex
                = modelIndex;

            //��������� �������� ���������� ����
            fuelTankBriefInfoPanel.objectName.text
                = fuelTankContentObject.ObjectName;


            //��������� ������������ ������� ����������� ������
            fuelTankBriefInfoPanel.panelToggle.group
                = parentToggleGroup;
            parentPanelsList.Add(
                fuelTankBriefInfoPanel.gameObject);
            fuelTankBriefInfoPanel.transform.SetParent(
                parentLayoutGroup.transform);

            return fuelTankBriefInfoPanel;
        }

        public UIExtractionEquipmentBriefInfoPanel InstantiateExtractionEquipmentBriefInfoPanel(
            IContentObject extractionEquipmentContentObject,
            int contentSetIndex, int modelIndex,
            ToggleGroup parentToggleGroup,
            VerticalLayoutGroup parentLayoutGroup,
            List<GameObject> parentPanelsList)
        {
            UIExtractionEquipmentBriefInfoPanel extractionEquipmentBriefInfoPanel
                = Instantiate(extractionEquipmentBriefInfoPanelPrefab);

            //��������� ������ ������ ��������
            extractionEquipmentBriefInfoPanel.contentSetIndex
                = contentSetIndex;

            //��������� ������ ������������ ��� ������
            extractionEquipmentBriefInfoPanel.objectIndex
                = modelIndex;

            //��������� �������� ������������ ��� ������
            extractionEquipmentBriefInfoPanel.objectName.text
                = extractionEquipmentContentObject.ObjectName;


            //��������� ������������ ������� ����������� ������
            extractionEquipmentBriefInfoPanel.panelToggle.group
                = parentToggleGroup;
            parentPanelsList.Add(
                extractionEquipmentBriefInfoPanel.gameObject);
            extractionEquipmentBriefInfoPanel.transform.SetParent(
                parentLayoutGroup.transform);

            return extractionEquipmentBriefInfoPanel;
        }

        public UIGunEnergyBriefInfoPanel InstantiateEnergyGunBriefInfoPanel(
            IContentObject energyGunContentObject,
            int contentSetIndex, int modelIndex,
            ToggleGroup parentToggleGroup,
            VerticalLayoutGroup parentLayoutGroup,
            List<GameObject> parentPanelsList)
        {
            UIGunEnergyBriefInfoPanel energyGunBriefInfoPanel
                = Instantiate(energyGunBriefInfoPanelPrefab);

            //��������� ������ ������ ��������
            energyGunBriefInfoPanel.contentSetIndex
                = contentSetIndex;

            //��������� ������ ��������
            energyGunBriefInfoPanel.objectIndex
                = modelIndex;

            //��������� �������� ��������������� ������
            energyGunBriefInfoPanel.objectName.text
                = energyGunContentObject.ObjectName;


            //��������� ������������ ������� ����������� ������
            energyGunBriefInfoPanel.panelToggle.group
                = parentToggleGroup;
            parentPanelsList.Add(
                energyGunBriefInfoPanel.gameObject);
            energyGunBriefInfoPanel.transform.SetParent(
                parentLayoutGroup.transform);

            return energyGunBriefInfoPanel;
        }


        public void ComponentCoreTechnologyPanelControl(
            List<string> technologyNames,
            UIComponentCoreTechnologyPanel componentCoreTechnologyPanel)
        {
            componentCoreTechnologyPanel.technologiesDropdown.ClearOptions();

            componentCoreTechnologyPanel.technologiesDropdown.AddOptions(
                technologyNames);


        }

        public void ComponentParameterPanelControl(
            in DCDTParameter parameter,
            float parameterMinValue,
            float parameterMaxValue,
            UIComponentParameterPanel componentParameterPanel)
        {
            componentParameterPanel.currentParameterValue
                = parameter.parameterBaseValue;
            componentParameterPanel.currentParameterText.text
                = parameter.parameterBaseValue.ToString();

            componentParameterPanel.parameterMinValue
                = parameterMinValue;
            componentParameterPanel.parameterMaxValue
                = parameterMaxValue;

            componentParameterPanel.parameterChangeSteps
                = parameter.parameterChangeSteps;

            componentParameterPanel.minusMinusMinusButtonText.text 
                = parameter.parameterChangeSteps[0].ToString();
            componentParameterPanel.minusMinusButtonText.text 
                = parameter.parameterChangeSteps[1].ToString();
            componentParameterPanel.minusButtonText.text 
                = parameter.parameterChangeSteps[2].ToString();

            componentParameterPanel.plusButtonText.text 
                = parameter.parameterChangeSteps[3].ToString();
            componentParameterPanel.plusPlusButtonText.text 
                = parameter.parameterChangeSteps[4].ToString();
            componentParameterPanel.plusPlusPlusButtonText.text 
                = parameter.parameterChangeSteps[5].ToString();
        }
    }
}