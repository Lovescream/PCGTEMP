using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveCreatureAI : CreatureAI, IInteractiveF {

    [Space(20)]

    // 현재 이 AI가 플레이어를 보고있는지.
    public Player player;

    // Callbacks.
    public Action cbOnFoundPlayer;
    public Action cbOnAwayPlayer;

    protected override void Awake() {
        base.Awake();
    }

    protected override void Update() {
        base.Update();

        dialogueBrain.CheckTalk();


        // 플레이어를 포착하지 못하면 시야 안에서 플레이어를 찾는다.
        if (!player) {
            FindPlayerInCircle();
        }
        // 플레이어가 들어온 상태라면,
        else {
            // 플레이어가 멀어지면,
            if ((player.transform.position - this.transform.position).magnitude > sight + 1) {
                player = null;
                cbOnAwayPlayer?.Invoke();
            }
        }

    }

    #region FindTarget

    // FindEnemyInCircle: 시야 반경으로 적이 있는지 탐색한다.
    // Target이 없으면 포착한 적을 Target으로 설정하고,
    // Target이 있으면 거리를 비교하여 더 가까운 적을 Target으로 설정한다.
    protected void FindPlayerInCircle() {
        // #1. Raycast 정보 설정.
        Vector2 rayOrigin = this.transform.position;
        float rayRadius = sight;

        // #2. Raycast 수행.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(rayOrigin, rayRadius, Vector2.zero, 0, layerMask);
        // #3. Target 설정.
        if (hits.Length != 0) {
            for (int i = 0; i < hits.Length; i++) {
                // 자기 자신 제외.
                if (hits[i].transform.gameObject == this.gameObject) continue;
                // 맞은 개체가 플레이어라면,
                Player player = hits[i].transform.GetComponent<Player>();
                if (player != null) {
                    SetPlayer(player);
                }
            }
        }
    }
    private void SetPlayer(Player p) {
        player = p;
        cbOnFoundPlayer?.Invoke();
    }

    #endregion

    // 현재 F 액션이 가능한지 여부.
    public virtual bool CanActionF() => true;
    public virtual void OnActionF() { InteractionF(); }
    public virtual void InteractionF() {

    }

    
}