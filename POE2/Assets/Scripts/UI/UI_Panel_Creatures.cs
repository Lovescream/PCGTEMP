using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Panel_Creatures : UI_Panel {

    [Header("*크리쳐 슬롯 UI 프리팹")]
    public GameObject slotPrefab;
    [Header("*크리쳐 슬롯 생성할 Transform")]
    public Transform slotContent;

    private List<UI_CreatureSlot> slotList;

    public override void Initialize() {
        if (slotList == null) slotList = new List<UI_CreatureSlot>();
        RemoveAllSlot();
        List<CreatureData> list = CreatureManager.Instance.encounteredCreatureList;
        for (int i = 0; i < list.Count; i++) {
            AddSlot(list[i]);
        }
    }

    private void AddSlot(CreatureData creature) {
        UI_CreatureSlot slot = Instantiate(slotPrefab, slotContent).GetComponent<UI_CreatureSlot>();
        slot.SetSlot(this, creature);
        slotList.Add(slot);
    }
    private void RemoveAllSlot() {
        for(int i = 0; i < slotList.Count; i++) {
            Destroy(slotList[i].gameObject);
        }
        slotList.Clear();
    }
}