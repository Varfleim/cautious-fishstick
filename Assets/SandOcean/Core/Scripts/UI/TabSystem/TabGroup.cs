using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public TabButton selectedTab;

    public List<GameObject> objectsToSwap;

    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabSelected;

    public void Subscribe(TabButton button)
    {
        if(tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }

        tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();

        if (selectedTab == null
            || selectedTab != button)
        {
            button.backgroundImage.sprite = tabHover;
        }
    }

    public void OnTabExit(TabButton button)
    {
        ResetTabs();

        button.backgroundImage.sprite = tabIdle;
    }

    public void OnTabSelected(TabButton button)
    {
        if(selectedTab != null)
        {
            selectedTab.Deselect();
        }

        selectedTab = button;

        selectedTab.Select();

        ResetTabs();

        button.backgroundImage.sprite = tabSelected;

        int index = button.transform.GetSiblingIndex();
        for(int a = 0; a < objectsToSwap.Count; a++)
        {
            if (a == index)
            {
                objectsToSwap[a].SetActive(true);
            }
            else
            {
                objectsToSwap[a].SetActive(false);
            }
        }
    }

    public void ResetTabs()
    {
        foreach(TabButton tabButton in tabButtons)
        {
            if(selectedTab != null
                && selectedTab == tabButton)
            {
                continue;
            }

            tabButton.backgroundImage.sprite = tabIdle;
        }
    }
}
