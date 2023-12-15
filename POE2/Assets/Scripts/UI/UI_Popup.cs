using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Popup : MonoBehaviour {

    public string popupName;
    public string popupDescription;

    // Callbacks.
    public Action cbOnOpenPopup;
    public Action cbOnClosePopup;

    public virtual void OpenPopup() {
        this.gameObject.SetActive(true);
        SetMessage();
        cbOnOpenPopup?.Invoke();
    }

    public virtual void ClosePopup() {
        this.gameObject.SetActive(false);
        cbOnClosePopup?.Invoke();
    }

    private void SetMessage() {
        for (int i = 0; i < this.transform.childCount; i++) {
            if (this.transform.GetChild(i).name == "Description") {
                this.transform.GetChild(i).GetComponentInChildren<Text>().text = popupDescription;
            }
        }
    }
}