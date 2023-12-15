using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Panel_Inventory : UI_Panel {

    [Header("*인벤토리 슬롯 UI 프리팹")]
    public GameObject slotPrefab;
    [Header("*인벤토리 슬롯 생성할 Transform")]
    public Transform inventoryContent;
    [Header("*무기 슬롯 Transform")]
    public Transform weaponSlots;

    [Space(20)]
    public UI_InventorySlot dragStartSlot;

    private Inventory inventory;
    private Dictionary<int, UI_InventorySlot> weaponSlotList;
    private Dictionary<int, UI_InventorySlot> inventorySlotList;
    
    public override void Initialize() {
        // 인벤토리 초기화.
        if (inventorySlotList == null) inventorySlotList = new Dictionary<int, UI_InventorySlot>();
        inventory = GameManager.Instance.player.inventory;
        for (int i = 0; i < inventory.inventorySize; i++) {
            if (inventorySlotList.ContainsKey(i)) {
                inventorySlotList[i].UpdateUI();
            }
            else {
                UI_InventorySlot slot = Instantiate(slotPrefab, inventoryContent).GetComponent<UI_InventorySlot>();
                slot.SetSlot(this, inventory.slotList[i]);
                inventorySlotList.Add(i, slot);
            }
        }
        // 무기 초기화.
        if (weaponSlotList == null) {
            weaponSlotList = new Dictionary<int, UI_InventorySlot>();
            for (int i=0; i < weaponSlots.childCount; i++) {
                UI_InventorySlot slot = weaponSlots.GetChild(i).GetComponent<UI_InventorySlot>();
                weaponSlotList.Add(slot.Index, slot);

                slot.SetSlot(this, GameManager.Instance.player.weaponSlot_set[i]);
            }
        }
        
    }

    public void UpdatePanel() {
        if (inventorySlotList == null) return;
        List<UI_InventorySlot> _slotList = inventorySlotList.Values.ToList();
        for(int i = 0; i < _slotList.Count; i++) {
            _slotList[i].UpdateUI();
        }

        if (weaponSlotList == null) return;
        _slotList = weaponSlotList.Values.ToList();
        for (int i = 0; i < _slotList.Count; i++) {
            _slotList[i].UpdateUI();
        }
    }
    
    // SwapInventorySlot: 인벤토리 슬롯끼리 서로 바꾼다.
    public void SwapInventorySlot(UI_InventorySlot slot1, UI_InventorySlot slot2) {
        // #1. 슬롯 교체.
        inventory.SwapSlot(slot1.slot, slot2.slot);

        // #2. 해당 슬롯 UI 업데이트.
        slot1.UpdateUI();
        slot2.UpdateUI();
    }
}

public enum InventorySlotType {
    NONE = -1,
    Items = 0,
    Equipments = 1,
    Weapons = 2,
}