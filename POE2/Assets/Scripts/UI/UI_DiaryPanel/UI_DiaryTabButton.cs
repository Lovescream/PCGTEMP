using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DiaryTabButton : MonoBehaviour {

    public UI_DiaryTab tab;

    public bool IsActivated {
        get {
            return IsActivated;
        }
        set {
            IsActivated = value;
            spriteRenderer.sprite = diaryPanel.tabSprites[value ? 1 : 0];
        }
    }

    private UI_Panel_Diary diaryPanel;
    private SpriteRenderer spriteRenderer;

    void Awake() {
        diaryPanel = UIManager.Instance.diaryPanel;
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public void OnBtn() {
        diaryPanel.ActivateTab(tab);
    }
}