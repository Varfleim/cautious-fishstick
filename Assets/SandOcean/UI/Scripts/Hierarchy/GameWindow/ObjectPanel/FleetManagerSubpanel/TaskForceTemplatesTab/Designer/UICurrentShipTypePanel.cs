
using UnityEngine;

using TMPro;

namespace SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates.Designer
{
    public class UICurrentShipTypePanel : UIAShipTypePanel
    {
        public UIAvailableShipTypePanel siblingAvailableShipTypePanel;

        public TextMeshProUGUI currentShipCount;

        public int CurrentShipCountValue
        {
            get
            {
                return currentShipCountValue;
            }
            set
            {
                currentShipCountValue = value;

                currentShipCount.text = currentShipCountValue.ToString();
            }
        }
        int currentShipCountValue;

        public int shipMinCount;
        public int shipMaxCount;

        public void ShipCountMinus()
        {
            CurrentShipCountValue -= 1;

            CurrentShipCountValue = Mathf.Clamp(
                CurrentShipCountValue,
                shipMinCount, shipMaxCount);

            //≈сли количество кораблей снизилось до нул€
            if (CurrentShipCountValue <= 0)
            {
                //«апрашиваем удаление имеющегос€ типа корабл€ у родительского дизайнера шаблонов оперативных групп
                parentTaskForceTemplateDesignerSubtab.RemoveCurrentShipType(this);
            }
        }

        public void ShipCountPlus()
        {
            CurrentShipCountValue += 1;

            CurrentShipCountValue = Mathf.Clamp(
                CurrentShipCountValue,
                shipMinCount, shipMaxCount);

            currentShipCount.text = CurrentShipCountValue.ToString();
        }
    }
}