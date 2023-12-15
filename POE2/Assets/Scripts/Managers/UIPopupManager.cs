using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIPopupManager : MonoBehaviour {

    private static UIPopupManager instance;
    public static UIPopupManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<UIPopupManager>();
            }
            return instance;
        }
        set {
            instance = value;
        }
    }

    public UI_Popup_2Btn popup2btnPrefab;

    private List<UI_Popup_2Btn> popList_2btn;

    void Awake() {
        popList_2btn = new List<UI_Popup_2Btn>();
    }

    public void OpenPopup2Btn(string description, string btnText0, UnityAction act0, string btnText1, UnityAction act1) {
        UI_Popup_2Btn popup = GetAvailablePopup2Btn();
        if (popup == null) {
            popup = Instantiate(popup2btnPrefab);
            popup.transform.SetParent(this.transform);
            popup.transform.localPosition = Vector2.zero;
            popList_2btn.Add(popup);
        }
        popup.OpenPopup(description, btnText0, act0, btnText1, act1);
    }

    // GetAvailablePopup: 열려있지 않은 팝업을 받는다. 없으면 Null.
    private UI_Popup_2Btn GetAvailablePopup2Btn() {
        for (int i = 0; i < popList_2btn.Count; i++) {
            if (!popList_2btn[i].gameObject.activeSelf) return popList_2btn[i];
        }
        return null;
    }

}