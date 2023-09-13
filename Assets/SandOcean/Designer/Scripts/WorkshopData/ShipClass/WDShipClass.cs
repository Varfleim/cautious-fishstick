
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
            //Рассчитываем массу корабля
            /*CalculateMass(
                contentData);*/

            //Рассчитываем форму корабля
            CalculateShape(
                contentData);

            //Рассчитываем скорость корабля
            CalculateSpeed(
                contentData);
        }

        /*public void CalculateMass(
            ContentData contentData)
        {
            //Обнуляем массу корабля
            shipMass
                = 0;

            //Для каждого двигателя корабля
            for (int a = 0; a < engines.Length; a++)
            {
                //Берём ссылку на данные двигателя
                ref readonly DEngine engine
                    = ref contentData
                    .contentSets[engines[a].contentSetIndex]
                    .engines[engines[a].modelIndex];

                //Прибавляем массу двигателя к массе корабля
                shipSize
                    += engine.engineSize;
            }
        }*/

        public void CalculateShape(
            ContentData contentData)
        {
            //Рассчитываем размер корабля
            CalculateSize(
                contentData);

            //Рассчитываем радиус и диаметр корабля
            shipRadius
                = (float)Math.Cbrt(
                    3f * shipSize
                    / 4 * Math.PI);

            //Рассчитываем площадь поверхности корабля
            shipArea
                = (float)(4f
                * Math.PI
                * shipRadius * shipRadius);
        }

        public void CalculateSize(
            ContentData contentData)
        {
            //Обнуляем размер корабля
            shipSize
                = 0;

            //Для каждого двигателя
            for (int a = 0; a < engines.Length; a++)
            {
                //Если ссылка на двигатель действительна
                if (engines[a].IsValidLink
                    == true)
                {
                    //Берём ссылку на двигатель
                    ref readonly WDEngine engine
                        = ref contentData
                        .wDContentSets[engines[a].ContentSetIndex]
                        .engines[engines[a].ObjectIndex];

                    //Прибавляем размер двигателя к размеру корабля
                    shipSize
                        += engine.EngineSize
                        * engines[a].numberOfComponents;
                }
            }

            //Для каждого реактора
            for (int a = 0; a < reactors.Length; a++)
            {
                //Если ссылка на реактор действительна
                if (reactors[a].IsValidLink
                    == true)
                {
                    //Берём ссылку на реактор
                    ref readonly WDReactor reactor
                        = ref contentData
                        .wDContentSets[reactors[a].ContentSetIndex]
                        .reactors[reactors[a].ObjectIndex];

                    //Прибавляем размер реактора к размеру корабля
                    shipSize
                        += reactor.ReactorSize
                        * reactors[a].numberOfComponents;
                }
            }

            //Для каждого топливного бака
            for (int a = 0; a < fuelTanks.Length; a++)
            {
                //Если ссылка на топливный бак действительна
                if (fuelTanks[a].IsValidLink
                    == true)
                {
                    //Берём ссылку на топливный бак
                    ref readonly WDHoldFuelTank fuelTank
                        = ref contentData
                        .wDContentSets[fuelTanks[a].ContentSetIndex]
                        .fuelTanks[fuelTanks[a].ObjectIndex];

                    //Прибавляем размер топливного бака к размеру корабля
                    shipSize
                        += fuelTank.Size
                        * fuelTanks[a].numberOfComponents;
                }
            }

            //Для каждого оборудования для твёрдой добычи
            for (int a = 0; a < extractionEquipmentSolids.Length; a++)
            {
                //Если ссылка на оборудование для твёрдой добычи действительна
                if (extractionEquipmentSolids[a].IsValidLink
                    == true)
                {
                    //Берём ссылку на оборудование для твёрдой добычи
                    ref readonly WDExtractionEquipment fuelTank
                        = ref contentData
                        .wDContentSets[extractionEquipmentSolids[a].ContentSetIndex]
                        .solidExtractionEquipments[extractionEquipmentSolids[a].ObjectIndex];

                    //Прибавляем размер оборудования для твёрдой добычи к размеру корабля
                    shipSize
                        += fuelTank.Size
                        * extractionEquipmentSolids[a].numberOfComponents;
                }
            }

            //Для каждого энергетического орудия
            for (int a = 0; a < energyGuns.Length; a++)
            {
                //Если ссылку на энергетическое орудие действительна
                if (energyGuns[a].IsValidLink
                    == true)
                {
                    //Берём ссылку на данные энергетического орудия
                    ref readonly WDGunEnergy energyGun
                        = ref contentData
                        .wDContentSets[energyGuns[a].ContentSetIndex]
                        .energyGuns[energyGuns[a].ObjectIndex];

                    //Прибавляем размер энергетического орудия к размеру корабля
                    this.shipSize
                        += energyGun.Size
                        * energyGuns[a].numberOfComponents;
                }
            }
        }

        public void CalculateSpeed(
            ContentData contentData)
        {
            //Создаём переменные для временных данных
            float totalEnginePower
                = 0;

            //Для каждого двигателя
            for (int a = 0; a < engines.Length; a++)
            {
                //Если ссылка на двигатель действительна
                if (engines[a].IsValidLink
                    == true)
                {
                    //Берём ссылку на данные двигателя
                    ref readonly WDEngine engine
                        = ref contentData
                        .wDContentSets[engines[a].ContentSetIndex]
                        .engines[engines[a].ObjectIndex];

                    //Прибавляем мощность двигателя к общей мощности двигателей
                    totalEnginePower
                        += engine.EnginePower
                        * engines[a].numberOfComponents;
                }
            }

            //Рассчитываем максимальную скорость корабля
            shipMaxSpeed
                = Formulas.ShipClassMaxSpeed(
                    totalEnginePower,
                    shipSize);
        }
    }
}