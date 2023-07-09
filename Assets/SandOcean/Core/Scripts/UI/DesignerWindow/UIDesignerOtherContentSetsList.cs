
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace SandOcean.UI
{
    public class UIDesignerOtherContentSetsList : MonoBehaviour
    {
        public GameObject listPanel;

        public TMP_Dropdown dropdown;

        public VerticalLayoutGroup layoutGroup;
        public ToggleGroup toggleGroup;

        public List<GameObject> panelsList = new();

        public Button displayButton;
        public Button hideButton;

        public Button loadButton;

        /*public void ClearPanelsList()
        {
            //Для каждой дочерней панели в списке
            for (int a = 0; a < panelsList.Count; a++)
            {
                Destroy(panelsList[a].gameObject);
            }

            //Очищаем список
            panelsList.Clear();
        }*/
    }
}