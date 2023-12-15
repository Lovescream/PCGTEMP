using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DiaryTab : MonoBehaviour {

    public virtual void Activate() {
        this.gameObject.SetActive(true);
        Initialize();
    }

    public virtual void Deactivate() {
        this.gameObject.SetActive(false);
    }

    public virtual void Initialize() {
        
    }
}