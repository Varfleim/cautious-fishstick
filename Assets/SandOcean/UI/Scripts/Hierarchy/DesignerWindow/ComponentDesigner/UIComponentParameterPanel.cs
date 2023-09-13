
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace SandOcean.UI.DesignerWindow
{
    public class UIComponentParameterPanel : MonoBehaviour
    {
        public UIComponentDesignerWindow parentComponentDesignerWindow;

        public TextMeshProUGUI parameterName;

        public TextMeshProUGUI currentParameterText;
        public float currentParameterValue;

        public TextMeshProUGUI minusMinusMinusButtonText;
        public TextMeshProUGUI minusMinusButtonText;
        public TextMeshProUGUI minusButtonText;
        public TextMeshProUGUI plusButtonText;
        public TextMeshProUGUI plusPlusButtonText;
        public TextMeshProUGUI plusPlusPlusButtonText;

        public float[] parameterChangeSteps;
        public float parameterMinValue;
        public float parameterMaxValue;

        public void ParameterMinusMinusMinus()
        {
            currentParameterValue += parameterChangeSteps[0];

            currentParameterValue
                = Mathf.Clamp(
                    currentParameterValue,
                    parameterMinValue,
                    parameterMaxValue);

            currentParameterText.text = currentParameterValue.ToString();

            parentComponentDesignerWindow.CalculateCharacteristics();
        }

        public void ParameterMinusMinus()
        {
            currentParameterValue += parameterChangeSteps[1];

            currentParameterValue
                = Mathf.Clamp(
                    currentParameterValue,
                    parameterMinValue,
                    parameterMaxValue);

            currentParameterText.text = currentParameterValue.ToString();

            parentComponentDesignerWindow.CalculateCharacteristics();
        }

        public void ParameterMinus()
        {
            currentParameterValue += parameterChangeSteps[2];

            currentParameterValue
                = Mathf.Clamp(
                    currentParameterValue,
                    parameterMinValue,
                    parameterMaxValue);

            currentParameterText.text = currentParameterValue.ToString();

            parentComponentDesignerWindow.CalculateCharacteristics();
        }

        public void ParameterPlus()
        {
            currentParameterValue += parameterChangeSteps[3];

            currentParameterValue
                = Mathf.Clamp(
                    currentParameterValue,
                    parameterMinValue,
                    parameterMaxValue);

            currentParameterText.text = currentParameterValue.ToString();

            parentComponentDesignerWindow.CalculateCharacteristics();
        }

        public void ParameterPlusPlus()
        {
            currentParameterValue += parameterChangeSteps[4];

            currentParameterValue
                = Mathf.Clamp(
                    currentParameterValue,
                    parameterMinValue,
                    parameterMaxValue);

            currentParameterText.text = currentParameterValue.ToString();

            parentComponentDesignerWindow.CalculateCharacteristics();
        }

        public void ParameterPlusPlusPlus()
        {
            currentParameterValue += parameterChangeSteps[5];

            currentParameterValue
                = Mathf.Clamp(
                    currentParameterValue,
                    parameterMinValue,
                    parameterMaxValue);

            currentParameterText.text = currentParameterValue.ToString();

            parentComponentDesignerWindow.CalculateCharacteristics();
        }
    }
}