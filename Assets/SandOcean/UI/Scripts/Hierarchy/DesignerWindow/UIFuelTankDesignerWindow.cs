
using System;
using System.Collections.Generic;

using TMPro;

namespace SandOcean.UI.DesignerWindow
{
    public class UIFuelTankDesignerWindow : UIComponentDesignerWindow
    {
        public UIComponentCoreTechnologyPanel compressionCoreTechnologyPanel;
        public List<Tuple<int, int, float>> compressionCoreTechnologiesList
            = new();

        public UIComponentParameterPanel sizeParameterPanel;

        public TextMeshProUGUI fuelTankCapacityText;
        public float fuelTankCapacityValue;

        public void DisplayParameters(
            float fuelTankSize)
        {
            sizeParameterPanel.currentParameterValue
                = fuelTankSize;
            sizeParameterPanel.currentParameterText.text
                = fuelTankSize.ToString();
        }

        public override void CalculateCharacteristics()
        {
            //������������ ����������� ���������� ����
            CalculateCapacity();
        }

        void CalculateCapacity()
        {
            //������������ ����������� ���������� ����
            fuelTankCapacityValue
                = Formulas.FuelTankCapacity(
                    compressionCoreTechnologiesList[compressionCoreTechnologyPanel.currentTechnologyIndex].Item3,
                    sizeParameterPanel.currentParameterValue);

            //���� ����������� ������ ����
            if (fuelTankCapacityValue
                > 0)
            {
                //���������� �
                fuelTankCapacityText.text
                    = fuelTankCapacityValue.ToString();
            }
            //�����
            else
            {
                //���������� ������ ������
                fuelTankCapacityText.text
                    = "";
            }
        }
    }
}