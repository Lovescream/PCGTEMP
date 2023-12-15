using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class CreatureLogManager : MonoBehaviour {

    private static CreatureLogManager instance;
    public static CreatureLogManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<CreatureLogManager>();
            }
            return instance;
        }
        set {
            instance = value;
        }
    }

    

    void Awake() {

    }

    // AddLog: 해당 크리쳐의 'logCondition' 로그 내용을, 해당 크리쳐의 logList에 추가한다.
    public void AddLog(CreatureData data, string logCondition) => data.AddLog(GetLogKey(data.creatureName, logCondition));

    private string GetLogKey(string creatureName, string logCondition) => "CreatureLog_" + creatureName + "_" + logCondition;








}