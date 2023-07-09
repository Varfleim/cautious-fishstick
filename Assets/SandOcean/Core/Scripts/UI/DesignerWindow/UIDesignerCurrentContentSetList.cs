using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace SandOcean.UI
{
    public class UIDesignerCurrentContentSetList : MonoBehaviour
    {
        public GameObject listPanel;

        public TMP_InputField objectName;

        public VerticalLayoutGroup layoutGroup;
        public ToggleGroup toggleGroup;

        public List<GameObject> panelsList = new();

        public Button displayButton;
        public Button hideButton;

        public Button saveButton;
        public Button loadButton;
        public Button deleteButton;

        /*public void ClearPanelsList()
        {
            //��� ������ �������� ������ � ������
            for (int a = 0; a < panelsList.Count; a++)
            {
                Destroy(panelsList[a].gameObject);
            }

            //������� ������
            panelsList.Clear();
        }*/
    }
}