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
            //������������ ������ ������
            sizeValue
                = Formulas.GunSizeCalculate(
                    gunCaliberParameterPanel.currentParameterValue,
                    gunBarrelLengthParameterPanel.currentParameterValue);

            //���� ������ ������ ����
            if (sizeValue
                > 0)
            {
                //���������� ���
                sizeText.text
                    = sizeValue.ToString();
            }
            //�����
            else
            {
                //���������� ������ ������
                sizeText.text
                    = "";
            }
        }

        void CalculateDamage()
        {
            //������������ ���� ������
            gunEnergyDamageValue
                = Formulas.EnergyGunDamageCalculate(
                    gunCaliberParameterPanel.currentParameterValue,
                    gunBarrelLengthParameterPanel.currentParameterValue);

            //���� ���� ������ ����
            if (gunEnergyDamageValue
                > 0)
            {
                //���������� ���
                gunEnergyDamageText.text
                    = gunEnergyDamageValue.ToString();
            }
            //�����
            else
            {
                //���������� ������ ������
                gunEnergyDamageText.text
                    = "";
            }
        }
    }
}