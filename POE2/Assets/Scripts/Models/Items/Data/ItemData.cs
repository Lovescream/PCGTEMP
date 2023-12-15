using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemData : ScriptableObject {

    public int id;
    public string itemName;
    [TextArea]
    public string description;
    public Sprite icon;
    public ItemBuff[] buffs;
    public bool isGatherable;
    public GameObject objectPrefab;

    public abstract ItemType Type { get; }
    public Item CreateItem() {
        return new Item(this);
    }
}