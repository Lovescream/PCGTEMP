using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Panel_Diary : UI_Panel {

    [Header("*탭 Sprite")]
    public Sprite[] tabSprites;
    [Header("*Tabs")]
    public UI_DiaryTab_Creature creatureTab;
    public UI_DiaryTab_CreatureInfo creatureInfoTab;

    private UI_DiaryTab currentTab;

    // ActivateTab: 현재 열려있는 탭을 비활성화하고 해당 탭을 활성화한다.
    public void ActivateTab(UI_DiaryTab tab) {
        // #1. 현재 탭 비활성화.
        if(currentTab != null) currentTab.Deactivate();

        // #2. 새 탭 활성화.
        currentTab = tab;
        tab.Activate();
    }
}