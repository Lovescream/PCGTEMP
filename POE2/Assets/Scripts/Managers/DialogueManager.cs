using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour {

    public static DialogueManager Instance { get; set; }

    void Awake() {
        if (!Instance) Instance = this;
        Localization l = Localization.Instance;
    }

    public string GetDialogue(string creatureName, string creatureState, int index = -1) {
        if (index == -1) {

            // 해당 크리쳐 및 상태의 대화 리스트 작성.
            string dialogue;
            int currentIndex = 0;
            List<string> dialogueList = new List<string>();
            while (currentIndex<1000) {
                string key = "Dialogue_" + creatureName + "_" + creatureState + "_" + currentIndex.ToString();
                dialogue = Localization.Instance.GetText(key);
                if (dialogue == string.Empty) break;
                dialogueList.Add(dialogue);
                currentIndex++;
            }
            // 대화 리스트가 비어있다면 빈 문장 리턴.
            if (dialogueList.Count == 0) return "";
            // 대화 리스트에서 랜덤으로 리턴.
            return dialogueList[Random.Range(0, dialogueList.Count)];
        }
        // 해당 크리쳐 및 상태의 index 번째 대사 리턴. (없으면 빈 문장)
        else {
            string key = "Dialogue_" + creatureName + "_" + creatureState + "_" + index.ToString();
            string dialogue = Localization.Instance.GetText(key);
            return dialogue;
        }
    }
    
    public string GetDialogue(string creatureName, string key) {
        return Localization.Instance.GetText("Dialogue_" + creatureName + "_" + key);
    }
}