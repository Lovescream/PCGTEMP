using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureData", menuName = "Creatures", order = 1)]
public class CreatureData : ScriptableObject, ICloneable {

    public int id;
    public string creatureName;
    [TextArea]
    public string description;
    public Sprite sprite;
    public Sprite portrait;
    public Status status;

    public List<Log> logList;

    public GameObject objectPrefab;

    public void Initialize() {
        // 저장된 데이터를 불러온다.
        LoadLog();
    }

    #region Log
    // IsContainLog: logList에 해당 key를 가진 로그가 있는지 확인.
    public bool IsContainLog(string key) {
        for (int i = 0; i < logList.Count; i++)
            if (logList[i].key == key) return true;
        return false;
    }
    private void LoadLog() {
        logList = SaveManager.CurrentData().LoadData("CreatureLog_" + creatureName, new List<Log>());
    }
    public void AddLog(string key) {
        if (IsContainLog(key)) return;
        logList.Add(new Log(key, Localization.Instance.GetText(key)));
        SaveManager.Instance.SaveData("CreatureLog_" + creatureName, logList);
    }
    #endregion

    // CreateCreature: 이 데이터의 크리쳐 오브젝트를 인스턴스화하여 리턴.
    public Creature CreateCreature() {
        Creature newCreature = Instantiate(objectPrefab).GetComponent<Creature>();

        return newCreature;
    }

    // Clone: 이 데이터의 복사본을 리턴.
    // (프리팹을 바로 로드하지 않고 복사본을 로드하여 프리팹을 수정하지 않게 하기 위함)
    public object Clone() {
        return this.MemberwiseClone();
    }

}