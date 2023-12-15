using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    private static UIManager instance;
    public static UIManager Instance {
        get {
            if (instance == null) instance = FindObjectOfType<UIManager>();
            return instance;
        }
        set {
            instance = value;
        }
    }

    [Header("*Panels")]
    public UI_Panel_Inventory inventoryPanel;
    public UI_Panel_Creatures creaturesPanel;
    public UI_Panel_CreatureInfo creatureInfoPanel;
    public UI_Panel_Diary diaryPanel;
    [Space(20)]
    // UI 잠금해제 여부.
    public bool availableInventoryPanel;
    public bool availableCreaturesPanel;
    public GameObject slotCursorObject;
    public InventorySlot draggingSlot;

    void Awake() {

    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.T)) if (availableInventoryPanel) TogglePanel(inventoryPanel);
        if (Input.GetKeyDown(KeyCode.Y)) if (availableCreaturesPanel) TogglePanel(diaryPanel);

        if (slotCursorObject) {
            slotCursorObject.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    // TogglePanel: 해당 패널이 열려있으면 닫고, 닫혀있으면 연다.
    private void TogglePanel(UI_Panel panel) {
        if (!panel.gameObject.activeSelf) panel.OpenPanel();
        else panel.ClosePanel();
    }


    public void CreateSlotCursorObject(InventorySlot slot) {
        // 마우스를 따라다니는 오브젝트 생성.
        slotCursorObject = new GameObject("slotCursor");
        RectTransform rectTransform = slotCursorObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);
        slotCursorObject.transform.SetParent(this.transform.parent);
        if (!slot.IsEmpty) {
            Image cursorImage = slotCursorObject.AddComponent<Image>();
            cursorImage.sprite = ItemManager.Instance.GetIcon(slot.ID);
            cursorImage.raycastTarget = false;
        }

        // 드래그중인 슬롯 설정.
        draggingSlot = slot;
    }
    public void RemoveSlotCursorObject() {
        if (slotCursorObject) {
            Destroy(slotCursorObject);
            slotCursorObject = null;
            draggingSlot = null;
        }
    }

}