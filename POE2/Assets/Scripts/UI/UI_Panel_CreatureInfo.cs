using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Panel_CreatureInfo : UI_Panel {

    [Header("*크리쳐 로그 프리팹")]
    public GameObject logPrefab;
    [Header("*크리쳐 로그 생성할 Transform")]
    public Transform logContent;
    [Header("*크리쳐 이미지")]
    public Image creatureImage;
    
    private CreatureData creature;
    private List<UI_CreatureLog> logList;


    void Awake() {
        UIManager.Instance.creaturesPanel.cbOnClosePanel += ClosePanel;
    }

    public void SetCreature(CreatureData creature) => this.creature = creature;

    // Initialize: 이미지 및 로그 초기화.
    public override void Initialize() {
        if (creature == null) return;
        if (logList == null) logList = new List<UI_CreatureLog>();

        // 이미지 설정.
        creatureImage.sprite = creature.sprite;

        // 로그 비우기.
        RemoveAllLog();

        // 로그 생성.
        for (int i = 0; i < creature.logList.Count; i++) {
            AddLog(i);
        }
    }

    private void AddLog(int index) {
        UI_CreatureLog log = Instantiate(logPrefab, logContent).GetComponent<UI_CreatureLog>();
        log.SetLog(creature, index);
        logList.Add(log);
    }
    private void RemoveAllLog() {
        for (int i = 0; i < logList.Count; i++) {
            Destroy(logList[i].gameObject);
        }
        logList.Clear();
    }
}