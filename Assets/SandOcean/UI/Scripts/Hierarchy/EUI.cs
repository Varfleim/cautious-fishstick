
using UnityEngine;

namespace SandOcean.UI
{
    public enum MainWindowType : byte
    {
        None,
        MainMenu,
        NewGameMenu,
        LoadGameMenu,
        Workshop,
        Designer,
        MainSettings,
        Game
    }

    public class EUI : MonoBehaviour
    {
        public MainWindowType activeMainWindowType;

        public GameObject activeMainWindow;

        public UIMainMenuWindow mainMenuWindow;

        public UINewGameMenuWindow newGameMenuWindow;

        public UIWorkshopWindow workshopWindow;

        public UIDesignerWindow designerWindow;

        public UIGameWindow gameWindow;
    }
}