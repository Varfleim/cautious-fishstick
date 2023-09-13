using System;
using System.Collections.Generic;

using TMPro;

using SandOcean.Designer.Workshop;
using SandOcean.Designer.Game;

namespace SandOcean.UI.DesignerWindow
{
    public class UIEngineDesignerWindow : UIComponentDesignerWindow
    {
        public UIComponentCoreTechnologyPanel powerPerSizeCoreTechnologyPanel;
        public List<Tuple<int, int, float>> powerPerSizeCoreTechnologiesList
            = new();

        public UIComponentParameterPanel sizeParameterPanel;
        public UIComponentParameterPanel boostParameterPanel;

        public TextMeshProUGUI enginePowerText;
        public float enginePowerValue;

        public void DisplayParameters(
            float engineSize,
            float engineBoost)
        {
            sizeParameterPanel.currentParameterValue
                = engineSize;
            sizeParameterPanel.currentParameterText.text
                = engineSize.ToString();

            boostParameterPanel.currentParameterValue
                = engineBoost;
            boostParameterPanel.currentParameterText.text
                = engineBoost.ToString();
        }

        public override void CalculateCharacteristics()
        {
            //Рассчитываем мощность двигателя
            CalculatePower();
        }

        void CalculatePower()
        {
            //Рассчитываем мощность двигателя
            enginePowerValue
                = Formulas.EnginePower(
                    powerPerSizeCoreTechnologiesList[powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item3,
                    sizeParameterPanel.currentParameterValue,
                    boostParameterPanel.currentParameterValue);

            //Если мощность больше нуля
            if (enginePowerValue
                > 0)
            {
                //Отображаем её
                enginePowerText.text
                    = enginePowerValue.ToString();
            }
            //Иначе
            else
            {
                //Отображаем пустую строку
                enginePowerText.text
                    = "";
            }
        }
    }
}