using System;
using System.Collections.Generic;
using UnityEngine;

public class CreatureLog : MonoBehaviour {

    public bool firstEncounter;                     // 처음 마주쳤을 때.
    public bool firstTalking;                       // 처음 대화했을 때.
    public int[] hits;                              // 플레이어에게 n대 맞았을 때.
    public string[] dialogues;                      // 크리쳐의 해당 대사를 들었을 때. (key로 입력)

    protected Creature creature;
    protected CreatureData creatureData;
    protected DialogueBrain dialogueBrain;

    protected Dictionary<string, object> actionDic;

    public virtual void Initialize() {
        creature = this.GetComponent<Creature>();
        creatureData = CreatureManager.Instance.GetCreatureData(creature.id);
        dialogueBrain = this.GetComponent<CreatureAI>().dialogueBrain;

        actionDic = new Dictionary<string, object>();

        if (firstEncounter)
            AddLogAction("FirstEncounter", new Ref<Action>(
                () => creature.cbOnFirstEncountered, 
                x => creature.cbOnFirstEncountered = x));
        if (firstTalking)
            AddLogAction("FirstTalking", new Ref<Action>(
                () => dialogueBrain.cbOnFirstTalking,
                x => dialogueBrain.cbOnFirstTalking = x));
        if (hits.Length > 0) {
            for (int i = 0; i < hits.Length; i++) {
                string key = hits[i].ToString() + "Hit"; Debug.Log(key);
                int amount = hits[i];
                AddLogAction(key, new Ref<Action<int>>(() => creature.cbOnBeHitByPlayer, x => creature.cbOnBeHitByPlayer = x), new Func<int, bool>(x => x == amount));
            }
        }
        if (dialogues.Length > 0) {
            for (int i = 0; i < dialogues.Length; i++) {
                string key = "dialogue_" + dialogues[i];
                string dialogue = DialogueManager.Instance.GetDialogue(creature.creatureName, dialogues[i]);
                AddLogAction(key, new Ref<Action<string>>(
                    () => dialogueBrain.cbOnSpeakDialogue,
                    x => dialogueBrain.cbOnSpeakDialogue = x),
                    new Func<string, bool>(x => x == dialogue));
            }
        }

    }



    public void AddLogAction(string action, Ref<Action> callback) {
        Action act = () => {
            CreatureLogManager.Instance.AddLog(creatureData, action);
            callback.Value -= (Action)actionDic[action];
            actionDic.Remove(action);
        };
        actionDic.Add(action, act);
        callback.Value += (Action)actionDic[action];
    }
    public void AddLogAction(string action, Ref<Action<string>> callback, Func<string, bool> func) {
        Action<string> act = txt => {
            if (func(txt)) {
                CreatureLogManager.Instance.AddLog(creatureData, action);
                callback.Value -= (Action<string>)actionDic[action];
                actionDic.Remove(action);
            }
        };
        actionDic.Add(action, act);
        callback.Value += (Action<string>)actionDic[action];
    }
    public void AddLogAction(string action, Ref<Action<int>> callback, Func<int, bool> func) {
        Action<int> act = amount => {
            if (func(amount)) {
                CreatureLogManager.Instance.AddLog(creatureData, action);
                callback.Value -= (Action<int>)actionDic[action];
                actionDic.Remove(action);
            }
        };
        actionDic.Add(action, act);
        callback.Value += (Action<int>)actionDic[action];
    }

    public class Ref<T> {
        private Func<T> getter;
        private Action<T> setter;
        public Ref(Func<T> getter, Action<T> setter) {
            this.getter = getter;
            this.setter = setter;
        }
        public T Value {
            get => getter();
            set => setter(value);
        }
    }

}