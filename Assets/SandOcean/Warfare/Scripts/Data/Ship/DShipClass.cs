
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
            this.shipRadius
                = (float)Math.Cbrt(
                    3f * shipSize
                    / 4 * Math.PI);

            //Рассчитываем площадь поверхности корабля
            this.shipArea
                = (float)(4f
                * Math.PI
                * shipRadius * shipRadius);
        }

        public void CalculateSize(
            ContentData contentData)
        {
            //Обнуляем размер корабля
            this.shipSize
                = 0;

            //Для каждого двигателя
            for (int a = 0; a < engines.Length; a++)
            {
                //Берём ссылку на данные двигателя
                ref readonly DEngine engine
                    = ref contentData
                    .contentSets[engines[a].ContentSetIndex]
                    .engines[engines[a].ObjectIndex];

                //Прибавляем размер двигателя к размеру корабля
                this.shipSize
                    += engine.EngineSize
                    * engines[a].numberOfComponents;
            }

            //Для каждого реактора
            for (int a = 0; a < reactors.Length; a++)
            {
                //Берём ссылку на данные реактора
                ref readonly DReactor reactor
                    = ref contentData
                    .contentSets[reactors[a].ContentSetIndex]
                    .reactors[reactors[a].ObjectIndex];

                //Прибавляем размер реактора к размеру корабля
                this.shipSize
                    += reactor.ReactorSize
                    * reactors[a].numberOfComponents;
            }

            //Для каждого топливного бака
            for (int a = 0; a < fuelTanks.Length; a++)
            {
                //Берём ссылку на данные топливного бака
                ref readonly DHoldFuelTank fuelTank
                    = ref contentData
                    .contentSets[fuelTanks[a].ContentSetIndex]
                    .fuelTanks[fuelTanks[a].ObjectIndex];

                //Прибавляем размер топливного бака к размеру корабля
                this.shipSize
                    += fuelTank.Size
                    * fuelTanks[a].numberOfComponents;
            }

            //Для каждого оборудования для твёрдой добычи
            for (int a = 0; a < extractionEquipmentSolids.Length; a++)
            {
                //Берём ссылку на данные оборудования для твёрдой добычи
                ref readonly DExtractionEquipment extractionEquipmentSolid
                    = ref contentData
                    .contentSets[extractionEquipmentSolids[a].ContentSetIndex]
                    .solidExtractionEquipments[extractionEquipmentSolids[a].ObjectIndex];

                //Прибавляем размер оборудования для твёрдой добычи к размеру корабля
                this.shipSize
                    += extractionEquipmentSolid.Size
                    * extractionEquipmentSolids[a].numberOfComponents;
            }

            //Для каждого энергетического орудия
            for (int a = 0; a < energyGuns.Length; a++)
            {
                //Берём ссылку на данные энергетического орудия
                ref readonly DGunEnergy energyGun
                    = ref contentData
                    .contentSets[energyGuns[a].ContentSetIndex]
                    .energyGuns[energyGuns[a].ObjectIndex];

                //Прибавляем размер энергетического орудия к размеру корабля
                this.shipSize
                    += energyGun.Size
                    * energyGuns[a].numberOfComponents;
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
                //Берём ссылку на данные двигателя
                ref readonly DEngine engine
                    = ref contentData
                    .contentSets[engines[a].ContentSetIndex]
                    .engines[engines[a].ObjectIndex];

                //Прибавляем мощность двигателя к общей мощности двигателей
                totalEnginePower
                    += engine.EnginePower
                    * engines[a].numberOfComponents;
            }

            //Рассчитываем максимальную скорость корабля
            this.shipMaxSpeed
                = Formulas.ShipClassMaxSpeed(
                    totalEnginePower,
                    shipSize);
        }
    }
}