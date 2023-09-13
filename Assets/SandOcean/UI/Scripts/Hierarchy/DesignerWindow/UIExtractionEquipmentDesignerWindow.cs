
using System;
using System.Collections.Generic;

using TMPro;

namespace SandOcean.UI.DesignerWindow
{
    public class UIExtractionEquipmentDesignerWindow : UIComponentDesignerWindow
    {
        public UIComponentCoreTechnologyPanel speedPerSizeCoreTechnologyPanel;
        public List<Tuple<int, int, float>> speedPerSizeCoreTechnologiesList
            = new();

        public UIComponentParameterPanel sizeParameterPanel;

        public TextMeshProUGUI extractionSpeedText;
        public float extractionSpeedValue;

        public void DisplayParameters(
            float extractionEquipmentSize)
        {
            sizeParameterPanel.currentParameterValue
                = extractionEquipmentSize;
            sizeParameterPanel.currentParameterText.text
                = extractionEquipmentSize.ToString();
        }

        public override void CalculateCharacteristics()
        {
            //Рассчитываем скорость добычи
            CalculateExtractionSpeed();
        }

        void CalculateExtractionSpeed()
        {
            //Рассчитываем скорость добычи
            extractionSpeedValue
                = Formulas.ExtractionEquipmentExtractionSpeed(
                    speedPerSizeCoreTechnologiesList[speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item3,
                    sizeParameterPanel.currentParameterValue);

            //Если скорость добычи больше нуля
            if (extractionSpeedValue
                > 0)
            {
                //Отображаем её
                extractionSpeedText.text
                    = extractionSpeedValue.ToString();
            }
            //Иначе
            else
            {
                //Отображаем пустую строку
                extractionSpeedText.text
                    = "";
            }
        }
    }
}