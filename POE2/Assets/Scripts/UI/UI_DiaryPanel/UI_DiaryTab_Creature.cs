using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DiaryTab_Creature : UI_DiaryTab {

    [Header("*슬롯 리스트")]
    public List<UI_DiaryTab_Creature_Slot> slotList;
    [Header("*페이지 버튼")]
    public GameObject btnPrevPage;
    public GameObject btnNextPage;

    private int currentPage;    // 현재 페이지.
    private int maxPage;        // 최대 페이지.
    private int slotCount;      // 한 페이지에 보여지는 슬롯 수.
    private List<CreatureData> creatureList;

    public override void Initialize() {
        base.Initialize();

        // 기록한 크리쳐 리스트를 받아온다.
        creatureList = CreatureManager.Instance.GetRecordedCreatureList();

        // 페이지 계산.
        slotCount = slotList.Count;
        currentPage = 0;
        maxPage = (creatureList.Count / slotList.Count) + 1;

        // 현재 페이지 슬롯들 설정.
        RefreshPage();
    }
    
    // RefreshPage: 현재 페이지 새로고침. (슬롯 재설정)
    private void RefreshPage() {
        // 슬롯 재설정.
        for (int i = 0; i < slotList.Count; i++) {
            int index = currentPage * slotCount + i;
            slotList[i].Initialize((index < creatureList.Count) ? creatureList[index] : null);
        }

        // 페이지 버튼 설정.
        btnPrevPage.SetActive(currentPage > 0);
        btnNextPage.SetActive(currentPage < maxPage);
    }

    // NextPage:
    public void NextPage() {
        if (currentPage >= maxPage) return;

        currentPage++;
        RefreshPage();
    }
    // PrevPage:
    public void PrevPage() {
        if (currentPage <= 0) return;

        currentPage--;
        RefreshPage();
    }
}