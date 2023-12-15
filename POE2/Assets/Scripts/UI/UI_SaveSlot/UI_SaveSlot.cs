using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SaveSlot : MonoBehaviour {

    public int slotIndex;

    public Text slotText;

    private SaveData data;

    public void Initialize() {
        string fileName = "save" + slotIndex;

        if (SaveManager.Instance.IsExist(fileName)) {
            SaveManager.Instance.Initialize(fileName);

            data = SaveManager.CurrentData();
        }
        else {
            data = null;
        }

        UpdateSlotUI();
    }

    private void UpdateSlotUI() {
        if (data != null) {
            slotText.text = "데이터 있음.";
        }
        else {
            slotText.text = "데이터 없음.";
        }
    }

    public void OnSelect() {
        if (data != null) {
            UIPopupManager.Instance.OpenPopup2Btn("이 게임을 이어서 할까요?", "좋아", GameStart, "아니", null);
        }
        else {
            UIPopupManager.Instance.OpenPopup2Btn("새 게임을 시작할까요?", "좋아", GameStart, "아니", null);
        }
    }
    
    public void GameStart() {
        if (data == null) {
            // 새세이브파일 만들기.
            string fileName = "save" + slotIndex;
            SaveManager.Instance.Initialize(fileName);
            data = SaveManager.CurrentData();
            // UI 업데이트.
            UpdateSlotUI();
        }

        TitleManager.Instance.GameStart(data);
    }

}