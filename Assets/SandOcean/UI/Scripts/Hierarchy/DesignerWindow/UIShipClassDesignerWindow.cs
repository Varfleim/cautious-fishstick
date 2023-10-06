
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using SandOcean.Designer;
using SandOcean.Designer.Workshop;
using SandOcean.Designer.Game;
using SandOcean.UI.DesignerWindow.ShipClassDesigner;

namespace SandOcean.UI.DesignerWindow
{
    public class UIShipClassDesignerWindow : MonoBehaviour
    {


        public TMP_Dropdown availableComponentTypeDropdown;
        public ShipComponentType currentAvailableComponentsType;

        public VerticalLayoutGroup availableComponentsListLayoutGroup;
        public ToggleGroup availableComponentsListToggleGroup;
        public List<GameObject> availableComponentsPanelsList = new();

        public VerticalLayoutGroup installedComponentsListLayoutGroup;
        public ToggleGroup installedComponentsListToggleGroup;
        public List<GameObject> installedComponentsPanelsList = new();

        public TextMeshProUGUI currentComponentName;
        public TextMeshProUGUI currentComponentType;
        public UIComponentDetailedInfoPanel activeComponentDetailedInfoPanel;
        public UIEngineDetailedInfoPanel engineDetailedInfoPanel;
        public UIReactorDetailedInfoPanel reactorDetailedInfoPanel;
        public UIFuelTankDetailedInfoPanel fuelTankDetailedInfoPanel;
        public UIExtractionEquipmentDetailedInfoPanel extractionEquipmentDetailedInfoPanel;
        public UIGunEnergyDetailedInfoPanel energyGunDetailedInfoPanel;

        public TextMeshProUGUI shipSizeText;
        public TextMeshProUGUI shipMaxSpeedText;

        public WDShipClass currentWorkshopShipClass
            = new(
                "",
                new WDShipClassComponent[0],
                new WDShipClassComponent[0],
                new WDShipClassComponent[0],
                new WDShipClassComponent[0],
                new WDShipClassComponent[0],
                new Warfare.Ship.WDShipClassPart[0]);
        public DShipClass currentGameShipClass
            = new(
                "",
                new DShipClassComponent[0],
                new DShipClassComponent[0],
                new DShipClassComponent[0],
                new DShipClassComponent[0],
                new DShipClassComponent[0], 
                new Warfare.Ship.DShipClassPart[0]);


        public UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanelPrefab;

        public UIInstalledComponentBriefInfoPanel InstantiateInstalledComponentBriefInfoPanel(
            int contentSetIndex, int modelIndex,
            ShipComponentType componentType,
            ToggleGroup parentToggleGroup,
            VerticalLayoutGroup parentLayoutGroup,
            List<GameObject> parentPanelsList)
        {
            UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanel
                = Instantiate(installedComponentBriefInfoPanelPrefab);

            //��������� ������ ������ ��������
            installedComponentBriefInfoPanel.contentSetIndex
                = contentSetIndex;

            //��������� ������ ������
            installedComponentBriefInfoPanel.componentIndex
                = modelIndex;

            //��������� ��� ����������
            installedComponentBriefInfoPanel.componentType
                = componentType;

            //��������� ������������ ������� ����������� ������
            installedComponentBriefInfoPanel.panelToggle.group
                = parentToggleGroup;
            parentPanelsList.Add(installedComponentBriefInfoPanel.gameObject);
            installedComponentBriefInfoPanel.transform.SetParent(parentLayoutGroup.transform);

            return installedComponentBriefInfoPanel;
        }

        public void CalculateCharacteristics(
            ContentData contentData,
            bool isInGameDesigner)
        {
            //���� ������� ������������� ��������
            if (isInGameDesigner
                == true)
            {
                //������������� �������������� �������
                currentGameShipClass.CalculateCharacteristics(
                    contentData);

                //���������� �������������� �������
                DisplayCharacteristics(
                    currentGameShipClass.ShipSize,
                    currentGameShipClass.ShipMaxSpeed);
            }
            //�����
            else
            {
                //������������� �������������� �������
                currentWorkshopShipClass.CalculateCharacteristics(
                    contentData);

                //���������� �������������� �������
                DisplayCharacteristics(
                    currentWorkshopShipClass.ShipSize,
                    currentWorkshopShipClass.ShipMaxSpeed);
            }
        }

        void DisplayCharacteristics(
            float shipSize,
            float shipMaxSpeed)
        {
            //���� ������ ������� ������ ����
            if (shipSize
                > 0)
            {
                //���������� ������ �������
                shipSizeText.text
                    = shipSize.ToString();
            }
            //�����
            else
            {
                //���������� ������ ������
                shipSizeText.text
                    = "";
            }

            //���� ������������ �������� ������� ������ ����
            if (shipSize
                > 0)
            {
                //���������� ������������ �������� �������
                shipMaxSpeedText.text
                    = shipMaxSpeed.ToString();
            }
            //�����
            else
            {
                //���������� ������ ������
                shipMaxSpeedText.text
                    = "";
            }
        }


        public void EngineDetailedInfoPanelControl(
            IContentObject engineContentObject,
            IEngine engine)
        {
            //������ ������ ��������� ���������� � ��������� ��������
            engineDetailedInfoPanel.gameObject.SetActive(true);
            //��������� � ��� ��������� ������ ��������� ����������
            activeComponentDetailedInfoPanel
                = engineDetailedInfoPanel;

            //���������� ������� ������ ���������
            ComponentDetailedInfoPanelControl(
                ref engineContentObject,
                ShipComponentType.Engine);

            //���������� ������ ���������
            engineDetailedInfoPanel.componentSizeText.text
                = engine.EngineSize.ToString();

            //���������� ������ ���������
            engineDetailedInfoPanel.engineBoostText.text
                = engine.EngineBoost.ToString();

            //���������� �������� ���������
            engineDetailedInfoPanel.enginePowerText.text
                = engine.EnginePower.ToString();
        }

        public void ReactorDetailedInfoPanelControl(
            IContentObject reactorContentObject,
            IReactor reactor)
        {
            //������ ������ ��������� ���������� � �������� ��������
            reactorDetailedInfoPanel.gameObject.SetActive(true);
            //��������� � ��� ��������� ������ ��������� ����������
            activeComponentDetailedInfoPanel
                = reactorDetailedInfoPanel;

            //���������� ������� ������ ��������
            ComponentDetailedInfoPanelControl(
                ref reactorContentObject,
                ShipComponentType.Reactor);

            //���������� ������ ��������
            reactorDetailedInfoPanel.componentSizeText.text
                = reactor.ReactorSize.ToString();

            //���������� ������ ��������
            reactorDetailedInfoPanel.reactorBoostText.text
                = reactor.ReactorBoost.ToString();

            //���������� ������� ��������
            reactorDetailedInfoPanel.reactorEnergyText.text
                = reactor.ReactorEnergy.ToString();
        }

        public void FuelTankDetailedInfoPanelControl(
            IContentObject fuelTankContentObject,
            IHold fuelTankHold,
            IHoldFuelTank fuelTank)
        {
            //������ ������ ��������� ���������� � ��������� ���� ��������
            fuelTankDetailedInfoPanel.gameObject.SetActive(true);
            //��������� � ��� ��������� ������ ��������� ����������
            activeComponentDetailedInfoPanel
                = fuelTankDetailedInfoPanel;

            //���������� ������� ������ ���������� ����
            ComponentDetailedInfoPanelControl(
                ref fuelTankContentObject,
                ShipComponentType.HoldFuelTank);

            //���������� ������ ���������� ����
            fuelTankDetailedInfoPanel.componentSizeText.text
                = fuelTankHold.Size.ToString();

            //���������� ����������� ���������� ����
            fuelTankDetailedInfoPanel.fuelTankCapacityText.text
                = fuelTankHold.Capacity.ToString();
        }

        public void ExtractionEquipmentDetailedInfoPanelControl(
            IContentObject extractionEquipmentContentObject,
            IExtractionEquipment extractionEquipment)
        {
            //������ ������ ��������� ���������� � ���������� ������������ ��������
            extractionEquipmentDetailedInfoPanel.gameObject.SetActive(true);
            //��������� � ��� ��������� ������ ��������� ����������
            activeComponentDetailedInfoPanel
                = extractionEquipmentDetailedInfoPanel;

            //���������� ������� ������ ����������� ������������
            ComponentDetailedInfoPanelControl(
                ref extractionEquipmentContentObject,
                ShipComponentType.ExtractionEquipmentSolid);

            //���������� ������ ����������� ������������
            extractionEquipmentDetailedInfoPanel.componentSizeText.text
                = extractionEquipment.Size.ToString();

            //���������� �������� ������ ����������� ������������
            extractionEquipmentDetailedInfoPanel.extractionSpeedText.text
                = extractionEquipment.ExtractionSpeed.ToString();
        }

        public void EnergyGunDetailedInfoPanelControl(
            IContentObject energyGunContentObject,
            IGun energyGunGun,
            IGunEnergy energyGun)
        {
            //������ ������ ��������� ���������� � �������������� ������
            energyGunDetailedInfoPanel.gameObject.SetActive(true);
            //��������� � ��� ��������� ������ ��������� ����������
            activeComponentDetailedInfoPanel
                = energyGunDetailedInfoPanel;

            //���������� ������� ������ ��������������� ������
            ComponentDetailedInfoPanelControl(
                ref energyGunContentObject,
                ShipComponentType.GunEnergy);

            //���������� ������ ��������������� ������
            energyGunDetailedInfoPanel.componentSizeText.text
                = energyGunGun.Size.ToString();

            //���������� ���� ��������������� ������
            energyGunDetailedInfoPanel.damageText.text
                = energyGun.GunEnergyDamage.ToString();
        }

        void ComponentDetailedInfoPanelControl(
            ref IContentObject contentObject,
            ShipComponentType componentType)
        {
            //���������� �������� ����������
            currentComponentName.text
                = contentObject.ObjectName;

            //���������� ��� ����������
            currentComponentType.text
                = componentType.ToString();
        }
    }
}