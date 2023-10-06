
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SandOcean.Organization;
using SandOcean.Technology;
using SandOcean.Designer.Workshop;

namespace SandOcean
{
    public enum TechnologyModifierType
    {
        DesignerMinEngineSize,
        DesignerMaxEngineSize,
        DesignerMinEngineBoost,
        DesignerMaxEngineBoost,
        DesignerMinReactorSize,
        DesignerMaxReactorSize,
        DesignerMinReactorBoost,
        DesignerMaxReactorBoost,
        DesignerMinFuelTankSize,
        DesignerMaxFuelTankSize,
        DesignerMinExtractionEquipmentSolidSize,
        DesignerMaxExtractionEquipmentSolidSize,
        DesignerMinGunEnergyCaliber,
        DesignerMaxGunEnergyCaliber,
        DesignerMinGunEnergyBarrelLength,
        DesignerMaxGunEnergyBarrelLength,
        None
    }

    public enum TechnologyComponentCoreModifierType
    {
        EnginePowerPerSize,
        ReactorEnergyPerSize,
        FuelTankCompression,
        ExtractionEquipmentSolidSpeedPerSize,
        GunEnergyRecharge,
        None
    }

    public enum BuildingCategory : byte
    {
        Test
    }

    public enum TaskForceBattleGroup : byte
    {
        ShortRange,
        MediumRange,
        LongRange
    }

    public class ContentData : MonoBehaviour
    {
        public WDContentSetDescription[] wDContentSetDescriptions;
        public WDContentSet[] wDContentSets;

        public Dictionary<int, DOrganizationTechnology>[] globalTechnologies;

        public DContentSetDescription[] contentSetDescriptions;
        public DContentSet[] contentSets;

        public string[] contentSetNames;


        

        public DTechnologyModifiers globalTechnologyModifiers;

        public string[] technologyComponentCoreModifiersNames;
        public DTechnologyModifierGlobalSort[] technologiesEnginePowerPerSize;
        public DTechnologyModifierGlobalSort[] technologiesReactorEnergyPerSize;
        public DTechnologyModifierGlobalSort[] technologiesFuelTankCompression;
        public DTechnologyModifierGlobalSort[] technologiesSolidExtractionEquipmentSpeedPerSize;
        public DTechnologyModifierGlobalSort[] technologiesEnergyGunRecharge;

        public string[] technologyModifiersNames;
    }

    public abstract class DContentObject
    {
        public DContentObject(
            string objectName)
        {
            this.objectName = objectName;
        }

        public string ObjectName
        {
            get
            {
                return objectName;
            }
            set
            {
                objectName = value;
            }
        }
        protected string objectName;
    }

    public abstract class WDContentObject : DContentObject
    {
        public WDContentObject(
            string objectName) : base(objectName)
        {
            gameObjectIndex = -1;
            isValidObject = false;
        }

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
        protected int gameObjectIndex;
        
        public bool IsValidObject
        {
            get
            {
                return IsValidObject;
            }
            set
            {
                isValidObject = value;
            }
        }
        protected bool isValidObject;
    }

    public interface IShipClassPart
    {
        public ContentObjectLink Part
        {
            get;
            set;
        }

        public ContentObjectLink CoreTechnology
        {
            get;
            set;
        }

        public ContentObjectLink[] Improvements
        {
            get;
            set;
        }
    }

    public interface IShipPart
    {
        ContentObjectLink[] CoreTechnologies
        {
            get;
            set;
        }
    }

    public interface IShipPartCoreTechnology
    {
        ContentObjectLink[] DirectionsOfImprovement
        {
            get;
            set;
        }
    }

    public interface IShipPartDirectionOfImprovement
    {
        ContentObjectLink[] Improvements
        {
            get;
            set;
        }
    }

    public interface IShipPartImprovement
    {

    }


    public interface IContentSetDescription
    {
        public string ContentSetName
        {
            get;
            set;
        }

        public string ContentSetVersion
        {
            get;
            set;
        }

        public string GameVersion
        {
            get;
            set;
        }
    }

    public interface IContentSet
    {
        public string ContentSetName
        {
            get;
            set;
        }
    }

    public interface IContentObject
    {
        public string ObjectName
        {
            get;
            set;
        }
    }

    public interface IWorkshopContentObject
    {
        public int GameObjectIndex
        {
            get;
            set;
        }

        public bool IsValidObject
        {
            get;
            set;
        }
    }

    public interface IContentObjectLink
    {
        public int ContentSetIndex
        {
            get;
            set;
        }

        public int ObjectIndex
        {
            get;
            set;
        }
    }

    public interface IWorkshopContentObjectLink
    {
        public string ContentSetName
        {
            get;
            set;
        }

        public string ObjectName
        {
            get;
            set;
        }

        public bool IsValidLink
        {
            get;
            set;
        }
    }

    public interface ISectorGenerationTemplate
    {
        public int MinNumberPlanetSystems
        {
            get;
            set;
        }

        public int MaxNumberPlanetSystems
        {
            get;
            set;
        }

        public float MinDistanceBetweenPlanetSystems
        {
            get;
            set;
        }
    }

    public interface ITechnology
    {
        public bool IsBaseTechnology
        {
            get;
            set;
        }
    }

    public interface ITechnologyModifier
    {
        public TechnologyModifierType ModifierType
        {
            get;
            set;
        }
        public float ModifierValue
        {
            get;
            set;
        }
    }

    public interface ITechnologyComponentCoreModifier
    {
        public TechnologyComponentCoreModifierType ModifierType
        {
            get;
            set;
        }
        public float ModifierValue
        {
            get;
            set;
        }
    }

    public interface IComponentCoreTechnology
    {
        public DContentObjectLink ContentObjectLink
        {
            get;
            set;
        }

        public float ModifierValue
        {
            get;
            set;
        }
    }

    public interface IWorkshopComponent
    {
        public List<WDContentObjectLink> ShipClasses
        {
            get;
            set;
        }
    }

    public interface IGameComponent
    {
        public List<DContentObjectLink> ShipClasses
        {
            get;
            set;
        }
    }

    public interface IBuildingType
    {
        public BuildingCategory BuildingCategory
        {
            get;
            set;
        }
    }

    public interface IEngine
    {
        public float EngineSize
        {
            get;
            set;
        }
        public float EngineBoost
        {
            get;
            set;
        }

        
        public float EnginePower
        {
            get;
            set;
        }

        public void CalculateCharacteristics();

        public void CalculatePower();
    }

    public interface IReactor
    {
        public float ReactorSize
        {
            get;
            set;
        }
        public float ReactorBoost
        {
            get;
            set;
        }

        public float ReactorEnergy
        {
            get;
            set;
        }

        public void CalculateCharacteristics();

        public void CalculateEnergy();
    }

    public interface IHold
    {
        public float Size
        {
            get;
            set;
        }

        public float Capacity
        {
            get;
            set;
        }

        public void CalculateCharacteristics();

        public void CalculateCapacity();
    }

    public interface IHoldFuelTank
    {

    }

    public interface IExtractionEquipment
    {
        public float Size
        {
            get;
            set;
        }

        public float ExtractionSpeed
        {
            get;
            set;
        }

        public void CalculateCharacteristics();

        public void CalculateExtractionSpeed();
    }

    public interface IGun
    {
        public float GunCaliber
        {
            get;
            set;
        }

        public float GunBarrelLength
        {
            get;
            set;
        }

        public float Size
        {
            get;
            set;
        }

        public void CalculateCharacteristics();

        public void CalculateSize();
    }

    public interface IGunProjectile
    {

    }

    public interface IGunHybrid
    {

    }

    public interface IGunEnergy
    {
        public float GunEnergyDamage
        {
            get;
            set;
        }

        public void CalculateDamage();
    }

    public interface IShipType
    {
        public TaskForceBattleGroup BattleGroup
        {
            get;
            set;
        }
    }

    public interface IShipClass
    {
        public float ShipRadius
        {
            get;
            set;
        }
        public float ShipArea
        {
            get;
            set;
        }
        public float ShipSize
        {
            get;
            set;
        }

        public float ShipMaxSpeed
        {
            get;
            set;
        }

        public void CalculateCharacteristics(ContentData contentData);

        public void CalculateShape(ContentData contentData);

        public void CalculateSize(ContentData contentData);

        public void CalculateSpeed(ContentData contentData);
    }

    /*public float AAAAA
    {
        get
        {
            return aaaaa;
        }
        set
        {
            aaaaa
                = value;
        }
    }*/
}