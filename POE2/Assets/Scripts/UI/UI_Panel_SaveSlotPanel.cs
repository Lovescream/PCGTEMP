using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Panel_SaveSlotPanel : UI_Panel {

    public UI_SaveSlot[] slots;

    public override void OpenPanel() {
        base.OpenPanel();

        for(int i = 0; i < slots.Length; i++) {
            slots[i].Initialize();
        }

    }
}