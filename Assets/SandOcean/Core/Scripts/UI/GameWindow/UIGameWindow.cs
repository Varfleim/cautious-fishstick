
using UnityEngine;

namespace SandOcean.UI
{
    public enum MainPanelType : byte
    {
        None,
        Object
    }

    public class UIGameWindow : MonoBehaviour
    {
        public int activeTerrainTypeIndex;

        public bool applyElevationLevel;
        public int activeElevationLevel;

        public bool applyWaterLevel;
        public int activeWaterLevel;

        public bool applyUrbanLevel;
        public int activeUrbanLevel;

        public bool applyFarmLevel;
        public int activeFarmLevel;

        public bool applyPlantLevel;
        public int activePlantLevel;

        public bool applySpecialIndex;
        public int activeSpecialIndex;

        public OptionalToggle riverMode;
        public OptionalToggle roadMode;
        public OptionalToggle walledMode;

        public MainPanelType activeMainPanelType = MainPanelType.None;
        public GameObject activeMainPanel;

        public UIObjectPanel objectPanel;

        public void SelectTerrainTypeIndex(
            int index)
        {
            activeTerrainTypeIndex = index;
        }

        public void SetApplyElevationLevel(
            bool toggle)
        {
            applyElevationLevel = toggle;
        }

        public void SetElevationLevel(
            float level)
        {
            activeElevationLevel = (int)level;
        }

        public void SetApplyWaterLevel(
            bool toggle)
        {
            applyWaterLevel = toggle;
        }

        public void SetWaterLevel(
            float level)
        {
            activeWaterLevel = (int)level;
        }

        public void SetRiverMode(
            int mode)
        {
            riverMode = (OptionalToggle)mode;
        }

        public void SetRoadMode(
            int mode)
        {
            roadMode = (OptionalToggle)mode;
        }

        public void SetApplyUrbanLevel(
            bool toggle)
        {
            applyUrbanLevel = toggle;
        }

        public void SetUrbanLevel(
            float level)
        {
            activeUrbanLevel = (int)level;
        }

        public void SetApplyFarmLevel(
            bool toggle)
        {
            applyFarmLevel = toggle;
        }

        public void SetFarmLevel(
            float level)
        {
            activeFarmLevel = (int)level;
        }

        public void SetApplyPlantLevel(
            bool toggle)
        {
            applyPlantLevel = toggle;
        }

        public void SetPlantLevel(
            float level)
        {
            activePlantLevel = (int)level;
        }

        public void SetApplySpecialIndex(
            bool toggle)
        {
            applySpecialIndex = toggle;
        }

        public void SetSpecialIndex(
            float level)
        {
            activeSpecialIndex = (int)level;
        }

        public void SetWalledMode(
            int mode)
        {
            walledMode = (OptionalToggle)mode;
        }

        public void ShowGrid(
            bool isVisible)
        {
            if (isVisible == true)
            {
                //spaceGenerationData.terrainMaterial.EnableKeyword("GRID_ON");
            }
            else
            {
                //spaceGenerationData.terrainMaterial.DisableKeyword("GRID_ON");
            }
        }
    }
}