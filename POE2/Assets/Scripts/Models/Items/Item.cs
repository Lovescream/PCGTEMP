using System;

[Serializable]
public class Item {
    public int id;
    public string itemName;
    public ItemBuff[] buffs;

    public Item(ItemData data) {
        this.id = data.id;
        this.itemName = data.itemName;
        this.buffs = data.buffs;
    }
}