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
            //������������ ������� ��������
            CalculateEnergy();
        }

        void CalculateEnergy()
        {
            //������������ ������� ��������
            reactorEnergyValue
                = Formulas.ReactorEnergy(
                    energyPerSizeCoreTechnologiesList[energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item3,
                    sizeParameterPanel.currentParameterValue,
                    boostParameterPanel.currentParameterValue);

            //���� ������� ������ ����
            if (reactorEnergyValue
                > 0)
            {
                //���������� �
                reactorEnergyText.text
                    = reactorEnergyValue.ToString();
            }
            //�����
            else
            {
                //���������� ������ ������
                reactorEnergyText.text
                    = "";
            }
        }
    }
}