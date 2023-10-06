
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean.Designer
{
    public enum DesignerType : byte
    {
        SectorGenerationTemplate,
        Technologies,
        BuildingType,
        ShipType,
        ShipClass,
        ComponentEngine,
        ComponentReactor,
        ComponentHoldFuelTank,
        ComponentExtractionEquipmentSolid,
        ComponentGunProjectile,
        ComponentGunHybrid,
        ComponentGunEnergy,
        ShipPart,
        ShipPartCoreTechnology,
        ShipPartDirectionOfImprovement,
        ShipPartImprovement,
        None
    }

    public enum DesignerDisplayContentType : byte
    {
        ContentSetsAll,
        ContentSet,
        ShipComponents
    }

    public enum ShipComponentType : byte
    {
        Engine,
        Reactor,
        HoldFuelTank,
        ExtractionEquipmentSolid,
        GunProjectile,
        GunHybrid,
        GunEnergy,
        None
    }

    public class DesignerData : MonoBehaviour
    {
        public DComponentDesignerType[] componentDesignerTypes;

        public TechnologyComponentCoreModifierType[] engineCoreModifierTypes;
        public TechnologyComponentCoreModifierType[] reactorCoreModifierTypes;
        public TechnologyComponentCoreModifierType[] fuelTankCoreModifierTypes;
        public TechnologyComponentCoreModifierType[] solidExtractionEquipmentCoreModifierTypes;
        public TechnologyComponentCoreModifierType[] energyGunCoreModifierTypes;

        //public DEngineModelsArray[] engineModelsContentSetsArrays;
        //public DEngineModel[] playerEngineModelsArray;

        //public DShipClassesArray[] shipClassesContentSetsArrays;
        //public DShipClass[] playerShipClassesArray;

        public DesignerType ShipComponentToDesignerType(
            ShipComponentType componentType)
        {
            //Если тип компонента - двигатель
            if (componentType
                == ShipComponentType.Engine)
            {
                return DesignerType.ComponentEngine;
            }
            //Иначе, если тип компонента - реактор
            else if (componentType
                == ShipComponentType.Reactor)
            {
                return DesignerType.ComponentReactor;
            }
            //Иначе, если тип компонента - топливный бак
            else if(componentType
                == ShipComponentType.HoldFuelTank)
            {
                return DesignerType.ComponentHoldFuelTank;
            }
            //Иначе, если тип компонента - оборудование для твёрдой добычи
            else if(componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                return DesignerType.ComponentExtractionEquipmentSolid;
            }
            //Иначе, если тип компонента - энергетическое орудие
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                return DesignerType.ComponentGunEnergy;
            }
            //Иначе
            {
                return DesignerType.None;
            }
        }
    }
}