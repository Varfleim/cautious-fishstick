
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

            //Указываем индекс набора контента
            installedComponentBriefInfoPanel.contentSetIndex
                = contentSetIndex;

            //Указываем индекс модели
            installedComponentBriefInfoPanel.componentIndex
                = modelIndex;

            //Указываем тип компонента
            installedComponentBriefInfoPanel.componentType
                = componentType;

            //Указываем родительские объекты создаваемой панели
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
            //Если активен внутриигровой дизайнер
            if (isInGameDesigner
                == true)
            {
                //Пересчитываем характеристики корабля
                currentGameShipClass.CalculateCharacteristics(
                    contentData);

                //Отображаем характеристики корабля
                DisplayCharacteristics(
                    currentGameShipClass.ShipSize,
                    currentGameShipClass.ShipMaxSpeed);
            }
            //Иначе
            else
            {
                //Пересчитываем характеристики корабля
                currentWorkshopShipClass.CalculateCharacteristics(
                    contentData);

                //Отображаем характеристики корабля
                DisplayCharacteristics(
                    currentWorkshopShipClass.ShipSize,
                    currentWorkshopShipClass.ShipMaxSpeed);
            }
        }

        void DisplayCharacteristics(
            float shipSize,
            float shipMaxSpeed)
        {
            //Если размер корабля больше нуля
            if (shipSize
                > 0)
            {
                //Отображаем размер корабля
                shipSizeText.text
                    = shipSize.ToString();
            }
            //Иначе
            else
            {
                //Отображаем пустую строку
                shipSizeText.text
                    = "";
            }

            //Если максимальная скорость корабля больше нуля
            if (shipSize
                > 0)
            {
                //Отображаем максимальную скорость корабля
                shipMaxSpeedText.text
                    = shipMaxSpeed.ToString();
            }
            //Иначе
            else
            {
                //Отображаем пустую строку
                shipMaxSpeedText.text
                    = "";
            }
        }


        public void EngineDetailedInfoPanelControl(
            IContentObject engineContentObject,
            IEngine engine)
        {
            //Делаем панель подробной информации о двигателе активной
            engineDetailedInfoPanel.gameObject.SetActive(true);
            //Указываем её как активнную панель подробной информации
            activeComponentDetailedInfoPanel
                = engineDetailedInfoPanel;

            //Отображаем базовые данные двигателя
            ComponentDetailedInfoPanelControl(
                ref engineContentObject,
                ShipComponentType.Engine);

            //Отображаем размер двигателя
            engineDetailedInfoPanel.componentSizeText.text
                = engine.EngineSize.ToString();

            //Отображаем разгон двигателя
            engineDetailedInfoPanel.engineBoostText.text
                = engine.EngineBoost.ToString();

            //Отображаем мощность двигателя
            engineDetailedInfoPanel.enginePowerText.text
                = engine.EnginePower.ToString();
        }

        public void ReactorDetailedInfoPanelControl(
            IContentObject reactorContentObject,
            IReactor reactor)
        {
            //Делаем панель подробной информации о реакторе активной
            reactorDetailedInfoPanel.gameObject.SetActive(true);
            //Указываем её как активнную панель подробной информации
            activeComponentDetailedInfoPanel
                = reactorDetailedInfoPanel;

            //Отображаем базовые данные реактора
            ComponentDetailedInfoPanelControl(
                ref reactorContentObject,
                ShipComponentType.Reactor);

            //Отображаем размер реактора
            reactorDetailedInfoPanel.componentSizeText.text
                = reactor.ReactorSize.ToString();

            //Отображаем разгон реактора
            reactorDetailedInfoPanel.reactorBoostText.text
                = reactor.ReactorBoost.ToString();

            //Отображаем энергию реактора
            reactorDetailedInfoPanel.reactorEnergyText.text
                = reactor.ReactorEnergy.ToString();
        }

        public void FuelTankDetailedInfoPanelControl(
            IContentObject fuelTankContentObject,
            IHold fuelTankHold,
            IHoldFuelTank fuelTank)
        {
            //Делаем панель подробной информации о топливном баке активной
            fuelTankDetailedInfoPanel.gameObject.SetActive(true);
            //Указываем её как активнную панель подробной информации
            activeComponentDetailedInfoPanel
                = fuelTankDetailedInfoPanel;

            //Отображаем базовые данные топливного бака
            ComponentDetailedInfoPanelControl(
                ref fuelTankContentObject,
                ShipComponentType.HoldFuelTank);

            //Отображаем размер топливного бака
            fuelTankDetailedInfoPanel.componentSizeText.text
                = fuelTankHold.Size.ToString();

            //Отображаем вместимость топливного бака
            fuelTankDetailedInfoPanel.fuelTankCapacityText.text
                = fuelTankHold.Capacity.ToString();
        }

        public void ExtractionEquipmentDetailedInfoPanelControl(
            IContentObject extractionEquipmentContentObject,
            IExtractionEquipment extractionEquipment)
        {
            //Делаем панель подробной информации о добывающем оборудовании активной
            extractionEquipmentDetailedInfoPanel.gameObject.SetActive(true);
            //Указываем её как активнную панель подробной информации
            activeComponentDetailedInfoPanel
                = extractionEquipmentDetailedInfoPanel;

            //Отображаем базовые данные добывающего оборудования
            ComponentDetailedInfoPanelControl(
                ref extractionEquipmentContentObject,
                ShipComponentType.ExtractionEquipmentSolid);

            //Отображаем размер добывающего оборудования
            extractionEquipmentDetailedInfoPanel.componentSizeText.text
                = extractionEquipment.Size.ToString();

            //Отображаем скорость добычи добывающего оборудования
            extractionEquipmentDetailedInfoPanel.extractionSpeedText.text
                = extractionEquipment.ExtractionSpeed.ToString();
        }

        public void EnergyGunDetailedInfoPanelControl(
            IContentObject energyGunContentObject,
            IGun energyGunGun,
            IGunEnergy energyGun)
        {
            //Делаем панель подробной информации о энергетическом орудии
            energyGunDetailedInfoPanel.gameObject.SetActive(true);
            //Указываем её как активнную панель подробной информации
            activeComponentDetailedInfoPanel
                = energyGunDetailedInfoPanel;

            //Отображаем базовые данные энергетического орудия
            ComponentDetailedInfoPanelControl(
                ref energyGunContentObject,
                ShipComponentType.GunEnergy);

            //Отображаем размер энергетического орудия
            energyGunDetailedInfoPanel.componentSizeText.text
                = energyGunGun.Size.ToString();

            //Отображаем урон энергетического орудия
            energyGunDetailedInfoPanel.damageText.text
                = energyGun.GunEnergyDamage.ToString();
        }

        void ComponentDetailedInfoPanelControl(
            ref IContentObject contentObject,
            ShipComponentType componentType)
        {
            //Отображаем название компонента
            currentComponentName.text
                = contentObject.ObjectName;

            //Отображаем тип компонента
            currentComponentType.text
                = componentType.ToString();
        }
    }
}