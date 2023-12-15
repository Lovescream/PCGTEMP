using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueBrain {
    // 대사 간격.
    public float talkInterval = 2;
    // 다음 대사를 하기까지 대기하는 타이머.
    private Timer talkIntervalTimer;

    // 현재 AI가 플레이어와 대화중인지 여부.
    private bool isTalking;
    // 이 AI가 플레이어와 대화를 한 번이라도 했는지 여부.
    private bool firstTalking;
    // 이 AI가 할 말이 있는지 여부.
    private bool hasSomethingToSay;
    // 현재 AI가 말을 하고 있는지 여부.
    private bool isSpeaking;
    // Components...
    private Creature creature;
    private InteractiveCreatureAI interactiveAI;
    // Callbacks...
    public Action<string> cbOnSpeakDialogue;
    public Action cbOnEndDialogue;
    public Action cbOnFirstTalking;
    #region Constructor / Initialize

    private DialogueBrain() { }
    public DialogueBrain(Creature creature, float talkInterval) {
        this.creature = creature;
        interactiveAI = this.creature.GetComponent<InteractiveCreatureAI>();
        this.talkInterval = talkInterval;
        
        Initialize();
    }

    public void Initialize() {
        // 데이터 로드.
        firstTalking = SaveManager.CurrentData().LoadData(creature.creatureName + "_FirstTalking", false);
        // 대사 대기 시간 설정.
        talkIntervalTimer = new Timer(talkInterval, false);
        SetTalkInterval();
        // 콜백 함수 설정.
        cbOnEndDialogue += SetTalkInterval;         // 대사가 끝나면 대사 대기 시간 재설정.
        creature.cbOnBeHit += TalkOnHit;            // 맞을 때 대사.
        creature.cbOnHitTarget += TalkOnAttack;     // 공격할 때 대사.
        interactiveAI.cbOnAwayPlayer += TalkOnAwayPlayer;         // 플레이어와 멀어질 때 대사.
        interactiveAI.cbOnAwayPlayer += SetDontTalking;           // 플레이어와 멀어질 때 대화 중지.
        interactiveAI.cbOnFoundPlayer += SetTalkIntervalNextZero; // 플레이어를 찾으면 즉시 대사.
    }

    #endregion
    
    public void Talk() {
        if (interactiveAI.player == null) TalkIdle();
        else TalkGreeting();
    }

    #region Talk According to State.
    // TalkIdle: Idle 상태 대사.
    private void TalkIdle() {
        TalkDialogue(GetDialogue("Idle"));
    }
    // TalkGreeting: Player가 보일 때 대사. (할 말이 있다면 Calling, 그렇지 않으면 Greeting)
    private void TalkGreeting() {
        if (hasSomethingToSay) TalkDialogue(GetDialogue("CallingPlayer"));
        else TalkDialogue(GetDialogue("GreetingToPlayer"));
    }
    // TalkOnAwayPlayer: Player가 멀어질 때 대사. (대화중/부르는중/작별인사)
    private void TalkOnAwayPlayer() {
        if (isTalking) {
            TalkDialogue(GetDialogue("OnAwayPlayerOnTalking"));
        }
        else {
            if (hasSomethingToSay) {
                TalkDialogue(GetDialogue("OnAwayPlayerOnCalling"));
            }
            else {
                TalkDialogue(GetDialogue("OnAwayPlayerGreeting"));
            }
        }
    }
    // TalkOnHit: 맞을 때 대사.
    private void TalkOnHit(Creature hitter) {
        TalkDialogue(GetDialogue("OnHit"));
    }
    // TalkOnAttack: 공격할 때 대사.
    private void TalkOnAttack(Creature target) {
        TalkDialogue(GetDialogue("OnAttack"));
    }
    #endregion

    // CheckTalk: 대화중 / 말하는 중이 아니고 대사 대기 시간이 모두 지나면 대사.
    public void CheckTalk() {
        if (!isTalking && !isSpeaking && talkIntervalTimer.Counting(Time.deltaTime))
            Talk();
    }

    // GetDialogue: 이 Creature의 action 대사 출력. index가 -1이면 랜덤출력.
    public string GetDialogue(string action, int index = -1) => DialogueManager.Instance.GetDialogue(creature.creatureName, action, index);

    // TalkDialogue: 해당 대사를 말한다.
    public void TalkDialogue(string dialogue) {
        // #0. 대사가 없으면 빠져나온다.
        if (dialogue.Length <= 0) return;

        // #1. 상태 변경.
        SetSpeaking();

        // #2. 이 크리쳐의 말풍선이 아직 떠 있다면 즉시 없앤다.
        if (creature.speechBubble != null) creature.speechBubble.DeactiveImmediately();

        // #3. 말풍선을 띄운다.
        InfoUIManager.Instance.ShowSpeechBubble(creature, dialogue).cbOnEndSpeechBubble += SetDontSpeaking;

        // #4. Callbacks.
        cbOnSpeakDialogue?.Invoke(dialogue);
    }

    // SetSpeaking/DontSpeaking: 말하고 있는 중임을 설정.
    private void SetSpeaking() => isSpeaking = true;
    private void SetDontSpeaking() => isSpeaking = false;
    // SetTalking/DontTalking: 대화중임을 설정.
    public void SetTalking() {
        if (!isTalking) {
            isTalking = true;
            if (!firstTalking) SetFirstTalking();
        }
    }
    private void SetDontTalking() {
        if (isTalking) isTalking = false;
    }
    // SetFirstTalking: 플레이어와 한 번이라도 대화했는지 여부 설정.
    private void SetFirstTalking() {
        firstTalking = true;
        cbOnFirstTalking?.Invoke();
        SaveManager.Instance.SaveData(creature.creatureName + "_FirstTalking", true);
    }
    // SetTalkInterval: 대사 대기 시간 설정.
    private void SetTalkInterval() {
        float time = talkInterval;//UnityEngine.Random.Range(talkInterval - 2, talkInterval + 2);
        talkIntervalTimer.SetTimer(time);
    }
    // SetTalkIntervalNextZero: 다음 대사 대기 시간을 0으로 만들어 즉시 출력하게 함.
    private void SetTalkIntervalNextZero() => talkIntervalTimer.SetCurrentToTarget();
}