using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_Popup_2Btn : UI_Popup {

    public Button[] buttons;

    public void OpenPopup(string description, string btnText0, UnityAction act0, string btnText1, UnityAction act1) {
        popupDescription = description;
        SetButtons(btnText0, act0, btnText1, act1);
        OpenPopup();
    }

    public override void ClosePopup() {
        base.ClosePopup();
        ResetOnBtnAction();
    }

    public void SetButtons(string btnText0, UnityAction onBtnAction0, string btnText1, UnityAction onBtnAction1) {
        SetButton(0, btnText0, onBtnAction0);
        SetButton(1, btnText1, onBtnAction1);
    }

    public void ResetOnBtnAction() {
        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].onClick.RemoveAllListeners();
        }
    }

    private void SetButton(int btnIndex, string btnText, UnityAction onBtnAction) {
        buttons[btnIndex].GetComponentInChildren<Text>().text = btnText;
        if (onBtnAction != null) buttons[btnIndex].onClick.AddListener(onBtnAction);
        buttons[btnIndex].onClick.AddListener(ClosePopup);
    }
}