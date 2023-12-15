using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InventorySlot : MonoBehaviour, IPointerClickHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IEventSystemHandler {
    [Header("*UI Components")]
    public Image iconImage;
    public Text countText;
    [Header("*SlotType")]
    public InventorySlotType type = InventorySlotType.Items;
    [Space(20)]
    public InventorySlot slot;

    private UI_Panel_Inventory inventoryPanel;

    public int Index => slot.index;

    public void SetSlot(UI_Panel_Inventory panel, InventorySlot slot) {
        inventoryPanel = panel;
        this.slot = slot;
        this.slot.SetUI(this);
        UpdateUI();
    }
    public void UpdateUI() {
        if (slot == null || slot.IsEmpty) {
            iconImage.sprite = null;
            iconImage.DOFade(0, 0);
            countText.text = "";
        }
        else {
            iconImage.sprite = ItemManager.Instance.GetIcon(slot.ID);
            iconImage.DOFade(1, 0);
            countText.text = slot.count > 1 ? slot.count.ToString() : "";
        }
    }

    // Select: 슬롯을 선택했을 때, 슬롯의 종류에 따라 함수를 부른다.
    private void Select() {
        if (slot.IsEmpty) return;
        switch (type) {
            case InventorySlotType.Items:
                SelectItem();
                break;
            case InventorySlotType.Equipments:
                break;
            case InventorySlotType.Weapons:
                UnequipItem();
                break;
        }
    }

    // SelectItem: 인벤토리 안의 아이템을 선택했을 때.
    private void SelectItem() {
        Item item = slot.item;
        ItemData data = ItemManager.Instance.GetItemData(item.id);

        switch (data.Type) {
            case ItemType.Default: break;
            case ItemType.Equipment: break;
            case ItemType.Weapon:
                GameManager.Instance.player.EquipItem(slot);
                break;
        }
    }

    // UnequipItem: 장비를 해제한다.
    private void UnequipItem() {
        Item item = slot.item;
        ItemData data = ItemManager.Instance.GetItemData(item.id);

        GameManager.Instance.player.UnequipItem(slot);
    }
    

    #region Event

    public void OnPointerClick(PointerEventData eventData) {
        switch(eventData.pointerId){
            case -1: // 왼쪽 클릭시.
                break;
            case -2: // 오른쪽 클릭시.
                Select();
                break;
            case -3: // 휠 클릭시.
                break;
            default:
                Debug.LogError("ERROR");
                break;
        }
    }
    public void OnBeginDrag(PointerEventData eventData) {
        if (eventData.pointerId != -1) return;
        if (slot.IsEmpty) return;

        // 해당 아이템 드래그 시작.
        if (!slot.IsEmpty) {
            UIManager.Instance.CreateSlotCursorObject(slot);
            inventoryPanel.dragStartSlot = this;
        }
    }
    public void OnDrag(PointerEventData eventData) {

    }
    public void OnDrop(PointerEventData eventData) {
        if (eventData.pointerId != -1) return;
        if (slot.IsEmpty) return;
        // 해당 아이템 드래그 종료. 슬롯 교체.
        inventoryPanel.SwapInventorySlot(inventoryPanel.dragStartSlot, this);


    }
    public void OnEndDrag(PointerEventData eventData) {
        // 해당 아이템 드래그 종료. 원래대로 되돌려놓기.
        UIManager.Instance.RemoveSlotCursorObject();
    }






    #endregion

}