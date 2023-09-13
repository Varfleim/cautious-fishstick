
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDShipClass : IContentObject, IWorkshopContentObject, IShipClass
    {
        public WDShipClass(
            string shipClassName,
            WDShipClassComponent[] engines,
            WDShipClassComponent[] reactors,
            WDShipClassComponent[] fuelTanks,
            WDShipClassComponent[] extractionEquipmentSolids,
            WDShipClassComponent[] energyGuns)
        {
            this.shipClassName = shipClassName;

            this.gameObjectIndex = -1;
            this.isValidObject = true;

            //this.engines = new WDShipClassComponent[engines.Length];
            //engines.CopyTo(this.engines, 0);
            this.engines = engines;
            //this.reactors = new WDShipClassComponent[reactors.Length];
            //reactors.CopyTo(this.reactors, 0);
            this.reactors = reactors;
            //this.fuelTanks = new WDShipClassComponent[fuelTanks.Length];
            //fuelTanks.CopyTo(this.fuelTanks, 0);
            this.fuelTanks = fuelTanks;
            //this.extractionEquipmentSolids = new WDShipClassComponent[extractionEquipmentSolids.Length];
            //extractionEquipmentSolids.CopyTo(this.extractionEquipmentSolids, 0);
            this.extractionEquipmentSolids = extractionEquipmentSolids;
            this.energyGuns = energyGuns;

            //this.shipMass = 0;

            this.shipRadius = 0;
            //this.shipDiameter = 0;
            this.shipArea = 0;
            this.shipSize = 0;

            this.shipMaxSpeed = 0;
        }

        public string ObjectName
        {
            get
            {
                return shipClassName;
            }
            set
            {
                shipClassName = value;
            }
        }
        string shipClassName;

        public int GameObjectIndex
        {
            get
            {
                return gameObjectIndex;
            }
            set
            {
                gameObjectIndex = value;
            }
        }
        int gameObjectIndex;

        public bool IsValidObject
        {
            get
            {
                return isValidObject;
            }
            set
            {
                isValidObject = value;
            }
        }
        bool isValidObject;


        public WDShipClassComponent[] engines;
        public WDShipClassComponent[] reactors;
        public WDShipClassComponent[] fuelTanks;
        public WDShipClassComponent[] extractionEquipmentSolids;
        public WDShipClassComponent[] energyGuns;


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
            shipRadius
                = (float)Math.Cbrt(
                    3f * shipSize
                    / 4 * Math.PI);

            //������������ ������� ����������� �������
            shipArea
                = (float)(4f
                * Math.PI
                * shipRadius * shipRadius);
        }

        public void CalculateSize(
            ContentData contentData)
        {
            //�������� ������ �������
            shipSize
                = 0;

            //��� ������� ���������
            for (int a = 0; a < engines.Length; a++)
            {
                //���� ������ �� ��������� �������������
                if (engines[a].IsValidLink
                    == true)
                {
                    //���� ������ �� ���������
                    ref readonly WDEngine engine
                        = ref contentData
                        .wDContentSets[engines[a].ContentSetIndex]
                        .engines[engines[a].ObjectIndex];

                    //���������� ������ ��������� � ������� �������
                    shipSize
                        += engine.EngineSize
                        * engines[a].numberOfComponents;
                }
            }

            //��� ������� ��������
            for (int a = 0; a < reactors.Length; a++)
            {
                //���� ������ �� ������� �������������
                if (reactors[a].IsValidLink
                    == true)
                {
                    //���� ������ �� �������
                    ref readonly WDReactor reactor
                        = ref contentData
                        .wDContentSets[reactors[a].ContentSetIndex]
                        .reactors[reactors[a].ObjectIndex];

                    //���������� ������ �������� � ������� �������
                    shipSize
                        += reactor.ReactorSize
                        * reactors[a].numberOfComponents;
                }
            }

            //��� ������� ���������� ����
            for (int a = 0; a < fuelTanks.Length; a++)
            {
                //���� ������ �� ��������� ��� �������������
                if (fuelTanks[a].IsValidLink
                    == true)
                {
                    //���� ������ �� ��������� ���
                    ref readonly WDHoldFuelTank fuelTank
                        = ref contentData
                        .wDContentSets[fuelTanks[a].ContentSetIndex]
                        .fuelTanks[fuelTanks[a].ObjectIndex];

                    //���������� ������ ���������� ���� � ������� �������
                    shipSize
                        += fuelTank.Size
                        * fuelTanks[a].numberOfComponents;
                }
            }

            //��� ������� ������������ ��� ������ ������
            for (int a = 0; a < extractionEquipmentSolids.Length; a++)
            {
                //���� ������ �� ������������ ��� ������ ������ �������������
                if (extractionEquipmentSolids[a].IsValidLink
                    == true)
                {
                    //���� ������ �� ������������ ��� ������ ������
                    ref readonly WDExtractionEquipment fuelTank
                        = ref contentData
                        .wDContentSets[extractionEquipmentSolids[a].ContentSetIndex]
                        .solidExtractionEquipments[extractionEquipmentSolids[a].ObjectIndex];

                    //���������� ������ ������������ ��� ������ ������ � ������� �������
                    shipSize
                        += fuelTank.Size
                        * extractionEquipmentSolids[a].numberOfComponents;
                }
            }

            //��� ������� ��������������� ������
            for (int a = 0; a < energyGuns.Length; a++)
            {
                //���� ������ �� �������������� ������ �������������
                if (energyGuns[a].IsValidLink
                    == true)
                {
                    //���� ������ �� ������ ��������������� ������
                    ref readonly WDGunEnergy energyGun
                        = ref contentData
                        .wDContentSets[energyGuns[a].ContentSetIndex]
                        .energyGuns[energyGuns[a].ObjectIndex];

                    //���������� ������ ��������������� ������ � ������� �������
                    this.shipSize
                        += energyGun.Size
                        * energyGuns[a].numberOfComponents;
                }
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
                //���� ������ �� ��������� �������������
                if (engines[a].IsValidLink
                    == true)
                {
                    //���� ������ �� ������ ���������
                    ref readonly WDEngine engine
                        = ref contentData
                        .wDContentSets[engines[a].ContentSetIndex]
                        .engines[engines[a].ObjectIndex];

                    //���������� �������� ��������� � ����� �������� ����������
                    totalEnginePower
                        += engine.EnginePower
                        * engines[a].numberOfComponents;
                }
            }

            //������������ ������������ �������� �������
            shipMaxSpeed
                = Formulas.ShipClassMaxSpeed(
                    totalEnginePower,
                    shipSize);
        }
    }
}