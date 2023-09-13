
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
            //������������ �������� ������
            CalculateExtractionSpeed();
        }

        void CalculateExtractionSpeed()
        {
            //������������ �������� ������
            extractionSpeedValue
                = Formulas.ExtractionEquipmentExtractionSpeed(
                    speedPerSizeCoreTechnologiesList[speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item3,
                    sizeParameterPanel.currentParameterValue);

            //���� �������� ������ ������ ����
            if (extractionSpeedValue
                > 0)
            {
                //���������� �
                extractionSpeedText.text
                    = extractionSpeedValue.ToString();
            }
            //�����
            else
            {
                //���������� ������ ������
                extractionSpeedText.text
                    = "";
            }
        }
    }
}