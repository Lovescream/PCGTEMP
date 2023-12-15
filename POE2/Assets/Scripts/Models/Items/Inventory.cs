using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory {

    public int inventorySize;

    public List<InventorySlot> slotList;

    public void Initialize(int size) {
        inventorySize = size;

        slotList = new List<InventorySlot>();
        for (int i = 0; i < inventorySize; i++) {
            slotList.Add(new InventorySlot(i));
        }
    }

    public void SetSlotSize(int size) {
        int delta = size - inventorySize;
        inventorySize = size;

        if (delta > 0) {
            for(int i = 0; i < delta; i++) {
                slotList.Add(new InventorySlot(i));
            }
        }
        else if (delta < 0) {
            for(int i = 0; i < delta; i++) {
                RemoveLastSlot();
            }
        }
    }

    public void AddItem(Item item, int amount = 1) {
        if (item.buffs.Length > 0) {
            SetItemToEmptySlot(item, amount);
            return;
        }
        if (!ItemManager.Instance.GetItemData(item.id).isGatherable) {
            SetItemToEmptySlot(item, amount);
            return;
        }

        InventorySlot slot = GetSlot(item.id);
        if (slot != null) {
            slot.AddAmount(amount);
        }
        else {
            SetItemToEmptySlot(item, amount);
        }
    }
    public void SwapSlot(InventorySlot slot1, InventorySlot slot2) {
        InventorySlot tempSlot = new InventorySlot(slot2);
        slot2.UpdateSlot(slot1.item, slot1.count);
        slot1.UpdateSlot(tempSlot.item, tempSlot.count);
    }
    public void RemoveItem(Item item) {
        InventorySlot slot = GetSlot(item);
        if (slot != null) slot.SetEmpty();
    }

    // RemoveLastSlot: 마지막 슬롯 삭제.
    public void RemoveLastSlot() {
        InventorySlot slot = slotList[slotList.Count - 1];
        if (!slot.IsEmpty) {
            // 슬롯이 비어있지 않으면... TODO:: 어떻게 할까? 아이템 떨구기?
        }
        slotList.Remove(slot);
    }

    // SetItemToEmptySlot: 빈 슬롯에 아이템을 할당. 빈 슬롯이 없으면 null.
    public InventorySlot SetItemToEmptySlot(Item item, int amount = 1) {
        InventorySlot slot = GetEmptySlot();
        if (slot != null) {
            slot.UpdateSlot(item, amount);
            return slot;
        }
        return null;
    }

    // GetSlot: 해당 item을 가진 슬롯을 받는다. 해당 슬롯이 없으면 null.
    public InventorySlot GetSlot(int id) {
        for (int i = 0; i < slotList.Count; i++) {
            InventorySlot slot = slotList[i];
            if (!slot.IsEmpty)
                if (slot.ID == id) return slot;
        }
        return null;
    }
    public InventorySlot GetSlot(Item item) {
        for (int i = 0; i < slotList.Count; i++) {
            if (slotList[i].item == item) return slotList[i];
        }
        return null;
    }
    // GetInventorySlot: 해당 index를 가진 슬롯을 받는다.
    public InventorySlot GetInventorySlot(int index) {
        for (int i = 0; i < slotList.Count; i++) {
            InventorySlot slot = slotList[i];
            if (slot.index == index) return slot;
        }
        return null;
    }

    // GetEmptySlot: 빈 슬롯을 받는다. 빈 슬롯이 없으면 null.
    public InventorySlot GetEmptySlot() {
        for(int i = 0; i < slotList.Count; i++) {
            if (slotList[i].IsEmpty) return slotList[i];
        }
        return null;
    }

}

[Serializable]
public class InventorySlot {
    public int index;
    public Item item;
    public int count;
    public UI_InventorySlot UI;

    private InventorySlot() { }

    public InventorySlot(int index) {
        this.index = index;
        item = null;
        count = 0;
    }
    public InventorySlot(int index, Item item, int amount) {
        this.index = index;
        UpdateSlot(item, amount);
    }
    public InventorySlot(InventorySlot slot) {
        this.index = slot.index;
        UpdateSlot(slot.item, slot.count);
    }

    public void SetEmpty() {
        this.item = null;
        this.count = 0;
    }
    public void UpdateSlot(Item item, int amount) {
        this.item = item;
        this.count = amount;
    }
    public void UpdateSlot(InventorySlot slot) {
        this.item = slot.item;
        this.count = slot.count;
    }

    public int ID => item.id;
    public bool IsEmpty => item == null || item.id == -1 || item.id == 0;
    public void AddAmount(int value) => count += value;

    public void SetUI(UI_InventorySlot ui) => this.UI = ui;
}