
using System;

using SandOcean.Warfare.Ship;

namespace SandOcean.Designer.Game
{
    public struct DShipClass : IContentObject, IShipClass
    {
        public DShipClass(
            string shipClassName,
            DShipClassComponent[] engines,
            DShipClassComponent[] reactors,
            DShipClassComponent[] fuelTanks,
            DShipClassComponent[] extractionEquipmentSolids,
            DShipClassComponent[] energyGuns,
            DShipClassPart[] shipParts)
        {
            this.shipClassName = shipClassName;

            this.engines = engines;
            this.reactors = reactors;
            this.fuelTanks = fuelTanks;
            this.extractionEquipmentSolids = extractionEquipmentSolids;
            this.energyGuns = energyGuns;

            this.shipParts = shipParts;

            //this.shipMass = 0;

            shipRadius = 0;
            //this.shipDiameter = 0;
            shipArea = 0;
            shipSize = 0;

            shipMaxSpeed = 0;
        }

        public string ObjectName
        {
            get
            {
                return shipClassName;
            }
            set
            {
                shipClassName
                    = value;
            }
        }
        string shipClassName;

        public DShipClassComponent[] engines;
        public DShipClassComponent[] reactors;
        public DShipClassComponent[] fuelTanks;
        public DShipClassComponent[] extractionEquipmentSolids;
        public DShipClassComponent[] energyGuns;

        public DShipClassPart[] shipParts;

        //public float shipMass;

        public float ShipRadius
        {
            get
            {
                return shipRadius;
            }
            set
            {
                shipRadius
                    = value;
            }
        }
        float shipRadius;
        //public float shipDiameter;
        public float ShipArea
        {
            get
            {
                return shipArea;
            }
            set
            {
                shipArea
                    = value;
            }
        }
        float shipArea;
        public float ShipSize
        {
            get
            {
                return shipSize;
            }
            set
            {
                shipSize
                    = value;
            }
        }
        float shipSize;

        public float ShipMaxSpeed
        {
            get
            {
                return shipMaxSpeed;
            }
            set
            {
                shipMaxSpeed
                    = value;
            }
        }
        float shipMaxSpeed;

        public void CalculateCharacteristics(
            ContentData contentData)
        {
            //������������ ����� �������
            /*CalculateMass(
                contentData);*/

            //������������ ����� �������
            CalculateShape(
                contentData);

            //������������ �������� �������
            CalculateSpeed(
                contentData);
        }

        /*public void CalculateMass(
            ContentData contentData)
        {
            //�������� ����� �������
            shipMass
                = 0;

            //��� ������� ��������� �������
            for (int a = 0; a < engines.Length; a++)
            {
                //���� ������ �� ������ ���������
                ref readonly DEngine engine
                    = ref contentData
                    .contentSets[engines[a].contentSetIndex]
                    .engines[engines[a].modelIndex];

                //���������� ����� ��������� � ����� �������
                shipSize
                    += engine.engineSize;
            }
        }*/

        public void CalculateShape(
            ContentData contentData)
        {
            //������������ ������ �������
            CalculateSize(
                contentData);

            //������������ ������ � ������� �������
            this.shipRadius
                = (float)Math.Cbrt(
                    3f * shipSize
                    / 4 * Math.PI);

            //������������ ������� ����������� �������
            this.shipArea
                = (float)(4f
                * Math.PI
                * shipRadius * shipRadius);
        }

        public void CalculateSize(
            ContentData contentData)
        {
            //�������� ������ �������
            this.shipSize
                = 0;

            //��� ������� ���������
            for (int a = 0; a < engines.Length; a++)
            {
                //���� ������ �� ������ ���������
                ref readonly DEngine engine
                    = ref contentData
                    .contentSets[engines[a].ContentSetIndex]
                    .engines[engines[a].ObjectIndex];

                //���������� ������ ��������� � ������� �������
                this.shipSize
                    += engine.EngineSize
                    * engines[a].numberOfComponents;
            }

            //��� ������� ��������
            for (int a = 0; a < reactors.Length; a++)
            {
                //���� ������ �� ������ ��������
                ref readonly DReactor reactor
                    = ref contentData
                    .contentSets[reactors[a].ContentSetIndex]
                    .reactors[reactors[a].ObjectIndex];

                //���������� ������ �������� � ������� �������
                this.shipSize
                    += reactor.ReactorSize
                    * reactors[a].numberOfComponents;
            }

            //��� ������� ���������� ����
            for (int a = 0; a < fuelTanks.Length; a++)
            {
                //���� ������ �� ������ ���������� ����
                ref readonly DHoldFuelTank fuelTank
                    = ref contentData
                    .contentSets[fuelTanks[a].ContentSetIndex]
                    .fuelTanks[fuelTanks[a].ObjectIndex];

                //���������� ������ ���������� ���� � ������� �������
                this.shipSize
                    += fuelTank.Size
                    * fuelTanks[a].numberOfComponents;
            }

            //��� ������� ������������ ��� ������ ������
            for (int a = 0; a < extractionEquipmentSolids.Length; a++)
            {
                //���� ������ �� ������ ������������ ��� ������ ������
                ref readonly DExtractionEquipment extractionEquipmentSolid
                    = ref contentData
                    .contentSets[extractionEquipmentSolids[a].ContentSetIndex]
                    .solidExtractionEquipments[extractionEquipmentSolids[a].ObjectIndex];

                //���������� ������ ������������ ��� ������ ������ � ������� �������
                this.shipSize
                    += extractionEquipmentSolid.Size
                    * extractionEquipmentSolids[a].numberOfComponents;
            }

            //��� ������� ��������������� ������
            for (int a = 0; a < energyGuns.Length; a++)
            {
                //���� ������ �� ������ ��������������� ������
                ref readonly DGunEnergy energyGun
                    = ref contentData
                    .contentSets[energyGuns[a].ContentSetIndex]
                    .energyGuns[energyGuns[a].ObjectIndex];

                //���������� ������ ��������������� ������ � ������� �������
                this.shipSize
                    += energyGun.Size
                    * energyGuns[a].numberOfComponents;
            }
        }

        public void CalculateSpeed(
            ContentData contentData)
        {
            //������ ���������� ��� ��������� ������
            float totalEnginePower
                = 0;

            //��� ������� ���������
            for (int a = 0; a < engines.Length; a++)
            {
                //���� ������ �� ������ ���������
                ref readonly DEngine engine
                    = ref contentData
                    .contentSets[engines[a].ContentSetIndex]
                    .engines[engines[a].ObjectIndex];

                //���������� �������� ��������� � ����� �������� ����������
                totalEnginePower
                    += engine.EnginePower
                    * engines[a].numberOfComponents;
            }

            //������������ ������������ �������� �������
            this.shipMaxSpeed
                = Formulas.ShipClassMaxSpeed(
                    totalEnginePower,
                    shipSize);
        }
    }
}