using System;
using System.Collections.Generic;

using TMPro;

using SandOcean.Designer.Workshop;
using SandOcean.Designer.Game;

namespace SandOcean.UI
{
    public class UIGunEnergyDesignerWindow : UIComponentDesignerWindow
    {
        public UIComponentCoreTechnologyPanel rechargeCoreTechnologyPanel;
        public List<Tuple<int, int, float>> rechargeCoreTechnologiesList
            = new();

        public UIComponentParameterPanel gunCaliberParameterPanel;
        public UIComponentParameterPanel gunBarrelLengthParameterPanel;

        public TextMeshProUGUI sizeText;
        public float sizeValue;

        public TextMeshProUGUI gunEnergyDamageText;
        public float gunEnergyDamageValue;

        public void DisplayParameters(
            float gunCaliber,
            float gunBarrelLength)
        {
            gunCaliberParameterPanel.currentParameterValue
                = gunCaliber;
            gunCaliberParameterPanel.currentParameterText.text
                = gunCaliber.ToString();

            gunBarrelLengthParameterPanel.currentParameterValue
                = gunBarrelLength;
            gunBarrelLengthParameterPanel.currentParameterText.text
                = gunBarrelLength.ToString();
        }

        public override void CalculateCharacteristics()
        {
            CalculateSize();

            CalculateDamage();
        }

        void CalculateSize()
        {
            //Рассчитываем размер орудия
            sizeValue
                = Formulas.GunSizeCalculate(
                    gunCaliberParameterPanel.currentParameterValue,
                    gunBarrelLengthParameterPanel.currentParameterValue);

            //Если размер больше нуля
            if (sizeValue
                > 0)
            {
                //Отображаем его
                sizeText.text
                    = sizeValue.ToString();
            }
            //Иначе
            else
            {
                //Отображаем пустую строку
                sizeText.text
                    = "";
            }
        }

        void CalculateDamage()
        {
            //Рассчитываем урон орудия
            gunEnergyDamageValue
                = Formulas.EnergyGunDamageCalculate(
                    gunCaliberParameterPanel.currentParameterValue,
                    gunBarrelLengthParameterPanel.currentParameterValue);

            //Если урон больше нуля
            if (gunEnergyDamageValue
                > 0)
            {
                //Отображаем его
                gunEnergyDamageText.text
                    = gunEnergyDamageValue.ToString();
            }
            //Иначе
            else
            {
                //Отображаем пустую строку
                gunEnergyDamageText.text
                    = "";
            }
        }
    }
}