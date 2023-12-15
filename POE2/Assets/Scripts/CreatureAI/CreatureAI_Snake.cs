using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Snake : CreatureAI {

    [Header("공격 주기")]
    public float attackWaitingTime = 2f;
    private Timer attackWaitingTimer;
    private bool passedAttackWaitingTime;

    private bool isAttacking;

    // Components.

    #region MonoBehaviours

    protected override void Awake() {
        base.Awake();

        if (attackWaitingTimer == null) attackWaitingTimer = new Timer(attackWaitingTime);

        this.GetComponent<Controller2D>().onTriggerEnterEvent += OnAttacking;
        this.GetComponent<Controller2D>().onTriggerStayEvent += OnAttacking;
    }

    protected override void Update() {
        base.Update();

        // 공격 대기 시간 카운팅.
        if (!passedAttackWaitingTime) {
            if (attackWaitingTimer.Counting(Time.deltaTime)) {
                passedAttackWaitingTime = true;
            }
        }

        Flip();
    }

    #endregion

    public override void Initialize() {
        base.Initialize();
    }

    #region States

    protected override void OnIdle() {
        base.OnIdle();
        targetingBrain.FindEnemyInCircle();
    }

    protected override void OnCognition() {
        base.OnCognition();
        Attack();
    }

    #endregion

    #region Action

    private void Flip() {
        if (targetingBrain.CurrentTarget == null) return;
        if (targetingBrain.CurrentTarget.transform.position.x >= this.transform.position.x)
            this.transform.localScale = new Vector3(1, 1, 1);
        else this.transform.localScale = new Vector3(-1, 1, 1);
    }

    private void Attack() {
        // #0. 조건 검사.
        // Target이 없으면 공격하지 않음.
        if (targetingBrain.CurrentTarget == null) return;
        // 공격이 가능하지 않으면 실행하지 않음.
        if (!CanAttack) return;
        // 이미 공격중이라면 실행하지 않음.
        if (isAttacking) return;

        // #1. 공격 상태 설정.
        isAttacking = true;

        // #2. 공격.
        this.animator.SetTrigger("Attack");

        Invoke("AttackEnd", 0.25f);
    }

    private void OnAttacking(Collider2D collision) {
        // #1. 충돌한 대상이 크리쳐인지 확인.
        Creature collidedCreature = collision.transform.GetComponent<Creature>();
        if (collidedCreature) {
            // #2. 해당 크리쳐가 적인지 확인.
            if (creature.IsEnemy(collidedCreature)) {
                // #3. 공격중이라면, 해당 크리쳐에게 공격 판정을 내림.
                if (isAttacking) {
                    collidedCreature.Hit(creature, creature.status.damage, Vector2.zero);
                }
            }
        }
    }

    private void AttackEnd() {
        // #3. 공격 상태 즉시 해제.
        isAttacking = false; // 사실 1,3번은 생략해도 되는 것이 아닌가...

        // #4. 공격 시간 초기화.
        passedAttackWaitingTime = false;
    }
    #endregion

    private bool CanAttack => passedAttackWaitingTime;
}