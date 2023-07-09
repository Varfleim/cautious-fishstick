
using System.Collections.Generic;

namespace SandOcean.Technology
{
    public struct TempTechnologyData
    {
        public TempTechnologyData(
            int a)
        {
            this.technologiesEnginePowerPerSize = new List<DTechnologyModifierGlobalSort>();

            this.technologiesReactorEnergyPerSize = new List<DTechnologyModifierGlobalSort>();

            this.technologiesFuelTankCompression = new List<DTechnologyModifierGlobalSort>();

            this.technologiesExtractionEquipmentSolidSpeedPerSize = new List<DTechnologyModifierGlobalSort>();

            this.technologiesEnergyGunRecharge = new List<DTechnologyModifierGlobalSort>();
        }

        public List<DTechnologyModifierGlobalSort> technologiesEnginePowerPerSize;

        public List<DTechnologyModifierGlobalSort> technologiesReactorEnergyPerSize;

        public List<DTechnologyModifierGlobalSort> technologiesFuelTankCompression;

        public List<DTechnologyModifierGlobalSort> technologiesExtractionEquipmentSolidSpeedPerSize;

        public List<DTechnologyModifierGlobalSort> technologiesEnergyGunRecharge;
    }
}