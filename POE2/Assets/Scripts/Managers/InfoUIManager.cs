using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoUIManager : MonoBehaviour {

    public static InfoUIManager Instance { get; set; }

    // HPBar.
    public GameObject healthBarPrefab;
    private List<UI_Info_HPBar> hpBarList;

    // DamageText.
    public GameObject DamageTextPrefab;

    // KeyDownAnimation.
    public GameObject keyDownFPrefab;

    // SpeechBubble.
    public GameObject speechBubblePrefab;
    private List<UI_Info_SpeechBubble> speechBubbleList;


    private void Awake() {
        if (!Instance) {
            Instance = this;
        }
        else if (Instance != this) {
            Destroy(this.gameObject);
            return;
        }

        hpBarList = new List<UI_Info_HPBar>();
        speechBubbleList = new List<UI_Info_SpeechBubble>();
    }

    #region HPBar

    public void ShowHPBar(Creature creature, float time) {
        UI_Info_HPBar hpBar = GetCreatureHPBar(creature);
        hpBar.Initialize(creature, time);
    }
    // GetCreatureHPBar: 해당 크리쳐의 HPBar를 가져온다. 없다면 사용 가능한 HPBar를 가져온다.
    private UI_Info_HPBar GetCreatureHPBar(Creature creature) {
        for (int i = 0; i < hpBarList.Count; i++) {
            if (hpBarList[i].IsCreature(creature)) return hpBarList[i];
        }
        return GetAvailableHPBar();
    }
    // GetAvailableHPBar: 사용 가능한 HPBar를 가져온다. 없다면 새로 만든다.
    private UI_Info_HPBar GetAvailableHPBar() {
        for (int i = 0; i < hpBarList.Count; i++) {
            if (!hpBarList[i].IsInUse) return hpBarList[i];
        }
        return CreateNewHPBar();
    }
    // CreateNewHPBar: 새 HPBar 생성.
    private UI_Info_HPBar CreateNewHPBar() {
        UI_Info_HPBar hpBar = Instantiate(healthBarPrefab).GetComponent<UI_Info_HPBar>();
        hpBar.transform.SetParent(this.transform);
        hpBarList.Add(hpBar);
        return hpBar;
    }

    #endregion

    #region DamageText

    public void ShowDamageText(Creature creature, float damage) {
        UI_Info_DamageText text = Instantiate(DamageTextPrefab).GetComponent<UI_Info_DamageText>();
        text.SetText(damage.ToString());
        text.transform.position = Camera.main.WorldToScreenPoint(creature.transform.position);
        text.transform.SetParent(this.transform);
    }

    #endregion

    #region KeyDownAnimation

    public UI_Info_KeyDown ShowKeyDownFAnimation(Transform target, Vector2 offset, float intervalTime) {
        UI_Info_KeyDown keyDown = Instantiate(keyDownFPrefab).GetComponent<UI_Info_KeyDown>();
        keyDown.transform.SetParent(this.transform);
        keyDown.Initialize(target, offset, intervalTime);
        return keyDown;
    }

    #endregion

    #region SpeechBubble;

    // 
    public UI_Info_SpeechBubble ShowSpeechBubble(Creature creature, string dialogue, float time = 5) {
        UI_Info_SpeechBubble bubble = GetCreatureSpeechBubble(creature);
        bubble.Initialize(creature, dialogue, time);
        return bubble;
    }

    // GetCreatureSpeechBubble: 해당 크리쳐의 SpeechBubble을 가져온다. 없다면 사용 가능한 SpeechBubble을 가져온다.
    private UI_Info_SpeechBubble GetCreatureSpeechBubble(Creature creature) {
        for (int i = 0; i < speechBubbleList.Count; i++) {
            if (speechBubbleList[i].IsCreature(creature)) return speechBubbleList[i];
        }
        return GetAvailableSpeechBubble();
    }

    // GetAvailableSpeechBubble: 사용 가능한 SpeechBubble을 가져온다. 없다면 새로 만든다.
    private UI_Info_SpeechBubble GetAvailableSpeechBubble() {
        for(int i = 0; i < speechBubbleList.Count; i++) {
            if (!speechBubbleList[i].IsInUse) return speechBubbleList[i];
        }
        return CreateNewSpeechBubble();
    }

    // CreateNewSpeechBubble: 새 말풍선 생성.
    public UI_Info_SpeechBubble CreateNewSpeechBubble() {
        UI_Info_SpeechBubble bubble = Instantiate(speechBubblePrefab).GetComponent<UI_Info_SpeechBubble>();
        bubble.transform.SetParent(this.transform);
        speechBubbleList.Add(bubble);
        return bubble;
    }

    #endregion
}