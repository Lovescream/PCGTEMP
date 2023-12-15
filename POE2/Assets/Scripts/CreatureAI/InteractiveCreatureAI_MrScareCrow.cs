using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveCreatureAI_MrScareCrow : InteractiveCreatureAI {

    // 튜토리얼 순서, 튜토리얼 대사 순서.
    public int tutorialIndex = 0;
    public int tutorialDialogueIndex = 0;
    // 준비된 다음 튜토리얼 대사.
    public string nextTutorialDialogue;
    // 튜토리얼 클리어 조건들.
    public bool isCompleteCheckInventory;
    public bool isCompleteEquipSword;
    public bool isCompleteAttack;
    public bool isCompleteKillAllSlime;
    public bool isCompleteCheckCreatures;

    private int countSummonedSlime;
    private int countDiedSlime;

    // Callbacks.
    public Action cbOnClearTutorial;
    public Action cbOnClearAllTutorial;

    [ShowInInspector]
    CreatureLog creatureLog;

    #region MonoBehaviours

    void Start() {
    }

    #endregion

    public override void InteractionF() {
        base.InteractionF();
        
        // F 액션이 가능한 상태라면,
        if (CanActionF()) {
            // 대화 상태 설정.
            dialogueBrain.SetTalking();
            // 대사 하기.
            dialogueBrain.TalkDialogue(nextTutorialDialogue);
            // 다음 대사 준비.
            ReadyNextTutorialDialogue();

            // 4-2 Tutorial 대화를 마친 후 Slime 소환.
            if (tutorialIndex==5 && tutorialDialogueIndex == 1) {
                Invoke("SummonSlime", 1f);
                Invoke("SummonSlime", 1.5f);
                Invoke("SummonSlime", 2f);
                Invoke("SummonSlime", 2.5f);
            }


        }
    }

    public override void Initialize() {
        base.Initialize();
        cbOnClearTutorial += GetTutorialReward;
        if (!SaveManager.CurrentData().LoadData("MrScareCrow_IsClearAllTutorial", false)) {
            creature.isInvincible = true;
            UIManager.Instance.inventoryPanel.cbOnOpenPanel += SetCompleteCheckInventory;
            GameManager.Instance.player.cbOnEquipItem += SetCompleteEquipSword;
            GameManager.Instance.player.GetComponent<Creature>().cbOnKill += AddCountDiedSlime;
            UIManager.Instance.diaryPanel.cbOnOpenPanel += SetCompleteCheckCreatures;
        }
        else {
            tutorialIndex = 99;
            tutorialDialogueIndex = 99;
        }

        ReadyNextTutorialDialogue();
    }


    // CanActionF: F 액션이 가능한지 검사한다.
    // 다음 튜토리얼 대사가 있고, 그 튜토리얼로 넘어갈 수 있다면 true,
    // 그렇지 않으면 false.
    public override bool CanActionF() {
        if (nextTutorialDialogue.Length <= 0) return false;
        else {
            if (IsReadyToNextTutorial()) return true;
            else return false;
        }
    }

    #region TutorialDialogue

    // ReadyNextTutorialDialogue: 다음 튜토리얼 대사를 준비한다.
    private void ReadyNextTutorialDialogue() {
        // 다음 대사를 준비한다.
        string dialogue = GetCurrentTutorialDialogue();
        if (dialogue.Length>0) {
            nextTutorialDialogue = dialogue;
            tutorialDialogueIndex++;
        }
        // 다음 대사가 없다면,
        else {
            // 해당 튜토리얼을 모두 마친 것으로 간주하고 보상을 지급한다.
            cbOnClearTutorial?.Invoke();
            // 다음 튜토리얼로 넘어가서,
            tutorialIndex++;
            tutorialDialogueIndex = 0;
            // 다음 대사를 준비한다.
            dialogue = GetCurrentTutorialDialogue();
            if (dialogue.Length > 0) {
                nextTutorialDialogue = dialogue;
                tutorialDialogueIndex++;
            }
            // 다음 대사가 없다면,
            else {
                // 튜토리얼을 모두 마친 것이므로, 튜토리얼 종료 신호를 보내고 다음 대사는 없다.
                nextTutorialDialogue = string.Empty;

                SaveManager.Instance.SaveData("MrScareCrow_IsClearAllTutorial", true);
                creature.isInvincible = false;
                cbOnClearAllTutorial?.Invoke();
            }
        }
    }

    // GetCurrentTutorialDiaglogue: 'tutorialIndex'번째 튜토리얼의 'tutorialDialogueIndex'번째 대사를 구함.
    private string GetCurrentTutorialDialogue() {
        string actionKey = "Tutorial" + tutorialIndex.ToString();
        string dialogue = dialogueBrain.GetDialogue(actionKey, tutorialDialogueIndex);
        return dialogue;
    }
 
    // GetTutorialReward: 'tutorialIndex'번째 튜토리얼을 완료할 시 받는 보상.
    private void GetTutorialReward() {
        switch (tutorialIndex) {
            case 0:
                // 대시기능 잠금 해제.
                GameManager.Instance.player.GetComponent<CharacterController2D>().availableDash = true;
                break;
            case 1:
                // 소지품창 잠금 해제.
                UIManager.Instance.availableInventoryPanel = true;
                break;
            case 2:
                // 칼 획득.
                ItemManager.Instance.GivePlayerItem(ItemManager.Instance.GetItem(2));
                break;
            case 5:
                // 크리쳐 목록 잠금 해제.
                UIManager.Instance.availableCreaturesPanel = true;
                break;
        }
    }

    // IsReadyToNextTutorial: 'tutorialIndex'번째 튜토리얼로 넘어가기 위한 목표를 달성했는지 검사한다.
    private bool IsReadyToNextTutorial() {
        switch (tutorialIndex) {
            case 0:
                return true;
            case 1:
                // 해야하는 것 없음.
                return true;
            case 2:
                // 소지품창을 열었다 닫았는지 확인.
                return isCompleteCheckInventory;
            case 3:
                // 칼 장착했는지 확인.
                return isCompleteEquipSword;
            case 4:
                // 공격을 1회 이상 했는지 확인.
                return isCompleteAttack;
            case 5:
                // 슬라임을 모두 죽였는지 확인.
                return isCompleteKillAllSlime;
            case 6:
                // 크리쳐 목록을 열었다 닫았는지 확인.
                return isCompleteCheckCreatures;
            case 7:
                // 해야하는 것 없음.
                return true;
        }
        return false;
    }

    #endregion

    #region Tutorial Checking

    public void SetCompleteCheckInventory() {
        isCompleteCheckInventory = true;
        UIManager.Instance.inventoryPanel.cbOnOpenPanel -= SetCompleteCheckInventory;
    }
    public void SetCompleteEquipSword(Item item) {
        if (item.id == 2) {
            isCompleteEquipSword = true;
            GameManager.Instance.player.cbOnEquipItem -= SetCompleteEquipSword;
            GameManager.Instance.player.CurrentMainHand.cbOnBeginAttack += SetCompleteAttack;
            GameManager.Instance.player.AnotherMainHand.cbOnBeginAttack += SetCompleteAttack;
        }
    }
    public void SetCompleteAttack() {
        isCompleteAttack = true;
        GameManager.Instance.player.CurrentMainHand.cbOnBeginAttack -= SetCompleteAttack;
        GameManager.Instance.player.AnotherMainHand.cbOnBeginAttack -= SetCompleteAttack;
    }
    public void AddCountDiedSlime(Creature creature) {
        if (creature.creatureName == "Slime") countDiedSlime++;
        if (countDiedSlime == countSummonedSlime) {
            isCompleteKillAllSlime = true;
            GameManager.Instance.player.creature.cbOnKill -= AddCountDiedSlime;
        }
    }
    public void SetCompleteCheckCreatures() {
        isCompleteCheckCreatures = true;
        UIManager.Instance.diaryPanel.cbOnOpenPanel -= SetCompleteCheckCreatures;
    }

    #endregion

    public void SummonSlime() {
        Vector2 position = this.transform.position;
        position.x += UnityEngine.Random.Range(-3f, 3f);
        position.y += UnityEngine.Random.Range(2f, 4f);
        
        Creature newSlime = CreatureManager.Instance.CreateCreature2(2001);
        Transform slime = newSlime.transform;

        slime.SetParent(this.transform);
        slime.position = position;
        countSummonedSlime++;
    }

}