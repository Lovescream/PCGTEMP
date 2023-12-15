using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_CreatureSlot : MonoBehaviour, IPointerClickHandler {
    [Header("*UI Components")]
    public Image creatureImage;
    public Text nameText;
    [Space(20)]
    public CreatureData creatureData;

    private UI_Panel_Creatures creaturesPanel;

    public void SetSlot(UI_Panel_Creatures panel, CreatureData data) {
        creaturesPanel = panel;
        creatureData = data;
        UpdateSlot();
    }
    public void UpdateSlot() {
        creatureImage.sprite = creatureData.sprite;
        nameText.text = creatureData.creatureName;
    }

    // Select: 
    private void Select() {
        UIManager.Instance.creatureInfoPanel.SetCreature(creatureData);
        UIManager.Instance.creatureInfoPanel.OpenPanel();
    }

    #region Event

    public void OnPointerClick(PointerEventData eventData) {
        switch (eventData.pointerId) {
            case -1: // 왼쪽 클릭시.
                Select();
                break;
            case -2: // 오른쪽 클릭시.
                break;
            case -3: // 휠 클릭시.
                break;
            default:
                Debug.LogError("ERROR");
                break;
        }
    }

    #endregion

}