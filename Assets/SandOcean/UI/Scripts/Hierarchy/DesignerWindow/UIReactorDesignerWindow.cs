using System;
using System.Collections.Generic;

using TMPro;

namespace SandOcean.UI.DesignerWindow
{
    public class UIReactorDesignerWindow : UIComponentDesignerWindow
    {
        public UIComponentCoreTechnologyPanel energyPerSizeCoreTechnologyPanel;
        public List<Tuple<int, int, float>> energyPerSizeCoreTechnologiesList = new();

        public UIComponentParameterPanel sizeParameterPanel;
        public UIComponentParameterPanel boostParameterPanel;

        public TextMeshProUGUI reactorEnergyText;
        public float reactorEnergyValue;

        public void DisplayParameters(
            float reactorSize,
            float reactorBoost)
        {
            sizeParameterPanel.currentParameterValue
                = reactorSize;
            sizeParameterPanel.currentParameterText.text
                = reactorSize.ToString();

            boostParameterPanel.currentParameterValue
                = reactorBoost;
            boostParameterPanel.currentParameterText.text
                = reactorBoost.ToString();
        }

        public override void CalculateCharacteristics()
        {
            //Рассчитываем энергию реактора
            CalculateEnergy();
        }

        void CalculateEnergy()
        {
            //Рассчитываем энергию реактора
            reactorEnergyValue
                = Formulas.ReactorEnergy(
                    energyPerSizeCoreTechnologiesList[energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item3,
                    sizeParameterPanel.currentParameterValue,
                    boostParameterPanel.currentParameterValue);

            //Если энергия больше нуля
            if (reactorEnergyValue
                > 0)
            {
                //Отображаем её
                reactorEnergyText.text
                    = reactorEnergyValue.ToString();
            }
            //Иначе
            else
            {
                //Отображаем пустую строку
                reactorEnergyText.text
                    = "";
            }
        }
    }
}