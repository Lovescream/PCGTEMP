using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Panel : MonoBehaviour {

    public string panelName;

    // Callbacks.
    public Action cbOnOpenPanel;
    public Action cbOnClosePanel;

    public virtual void OpenPanel() {
        SetTitle();
        this.gameObject.SetActive(true);
        Initialize();
        cbOnOpenPanel?.Invoke();
    }
    public virtual void ClosePanel() {
        this.gameObject.SetActive(false);
        cbOnClosePanel?.Invoke();
    }
    public virtual void Initialize() {

    }

    private void SetTitle() {
        for (int i = 0; i < this.transform.childCount; i++) {
            if (this.transform.GetChild(i).name == "Title") {
                this.transform.GetChild(i).GetComponentInChildren<Text>().text = panelName;
            }
        }
    }
}