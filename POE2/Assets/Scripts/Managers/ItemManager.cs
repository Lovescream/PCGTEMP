using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {
    NONE = -1,
    Default = 0,
    Equipment = 1,
    Weapon,
}
public enum HandType {
    NONE = -1,
    Main = 0,
    Sub,
    TwoHanded,
}

public class ItemManager : MonoBehaviour {

    public static ItemManager Instance { get; set; }

    public Dictionary<int, ItemData> database;

    void Awake() {
        if (!Instance) Instance = this;

        Initialize();
    }

    // Initialize: Resources/Items의 모든 리소스를 불러와 database에 넣는다.
    public void Initialize() {
        database = new Dictionary<int, ItemData>();

        ItemData[] items = Resources.LoadAll<ItemData>("Items");
        for(int i = 0; i < items.Length; i++) {
            if (items[i].id == 0) Debug.LogError("Invaild Item!");
            else {
                if (!database.ContainsKey(items[i].id)) {
                    database.Add(items[i].id, items[i]);
                }
            }
        }
    }

    // GetItemData: 해당 id를 가진 ItemData를 받아온다.
    public ItemData GetItemData(int id) => database[id];

    // GetItem: 해당 id를 가진 Item를 생성한다.
    public Item GetItem(int id) {
        return GetItemData(id).CreateItem();
    }

    // GetIcon: 해당 id를 가진 아이템의 아이콘을 받아온다.
    public Sprite GetIcon(int id) {
        if (!database.ContainsKey(id)) { Debug.LogError("[ItemManager] " + id + "는 없는 아이템 ID."); return null; }
        ItemData item = database[id];
        if (item == null) {
            Debug.LogError("[ItemManager] 없는 아이템 참조.");
            return null;
        }
        return item.icon;
    }



    public void GivePlayerItem(Item item) {
        GameManager.Instance.player.inventory.AddItem(item);
        UIManager.Instance.inventoryPanel.UpdatePanel();
    }
}