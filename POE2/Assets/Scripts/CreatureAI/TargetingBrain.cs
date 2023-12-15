using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingBrain {

    // 탐색할 시야 반경.
    private float sight;
    // 탐색할 레이어.
    private LayerMask layerMask;
    // 현재 타겟.
    public Creature CurrentTarget { get; set; }
    public bool IsTargeting => CurrentTarget != null;

    // Components.
    private Creature creature;

    // Callbacks.
    public Action<Creature> cbOnSetTarget;

    private TargetingBrain() { }
    public TargetingBrain(Creature creature, float sight, LayerMask layerMask) {
        this.creature = creature;
        this.sight = sight;
        this.layerMask = layerMask;

        Initialize();
    }

    public void Initialize() {
        
    }

    // FindEnemyInCircle: 시야 반경으로 적이 있는지 탐색한다.
    // Target이 없으면 포착한 적을 Target으로 설정하고,
    // Target이 있으면 거리를 비교하여 더 가까운 적을 Target으로 설정한다.
    public void FindEnemyInCircle() {
        // #1. Raycast 정보 설정.
        Vector2 rayOrigin = creature.transform.position;
        float rayRadius = sight;

        // #2. Raycast 수행.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(rayOrigin, rayRadius, Vector2.zero, 0, layerMask);
        // #3. Target 설정.
        if (hits.Length != 0) {
            Creature targetCreature = null;
            for (int i = 0; i < hits.Length; i++) {
                // 자기 자신 제외.
                if (hits[i].transform.gameObject == creature.gameObject) continue;
                // 맞은 개체가 크리쳐라면,
                Creature hitCreature = hits[i].transform.GetComponent<Creature>();
                if (hitCreature != null) {
                    // 해당 크리쳐가 적이면,
                    if (this.creature.IsEnemy(hitCreature)) {
                        if (!hitCreature.IsDie) {
                            // target이 없으면 hitCreature를 target으로 설정.
                            if (CurrentTarget == null) {
                                //Debug.Log(creature.transform.name + "은 " + CurrentTarget.transform.name + "을 타깃으로 지정함.");
                                targetCreature = hitCreature;
                            }
                            // target이 있으면 가까운 크리쳐를 target으로 설정.
                            else {
                                float distanceToNewHitCreature = (hitCreature.transform.position - creature.transform.position).magnitude;
                                float distanceToTargetCreature = (CurrentTarget.transform.position - creature.transform.position).magnitude;
                                if (distanceToNewHitCreature < distanceToTargetCreature) {
                                    targetCreature = hitCreature;
                                    //Debug.Log(creature.transform.name + "은 " + CurrentTarget.transform.name + "을 타깃으로 갱신함.");
                                }
                                else {
                                    //Debug.Log(creature.transform.name + "은 " + CurrentTarget.transform.name + "을 타깃으로 유지함.");
                                    targetCreature = CurrentTarget;
                                }
                            }
                        }
                    }
                }
            }

            CurrentTarget = targetCreature;
            cbOnSetTarget?.Invoke(targetCreature);
        }
    }
}