using System;
using UnityEngine;

[Serializable]
public class CharacterHand : MonoBehaviour {

    [Header("*공격 대기 시간")]
    public float attackWaitingTime;
    // 공격 대기 시간이 지났는지 여부.
    private bool passedAttackWaitingTime;
    [Header("*공격 기준 위치")]
    public Transform attackPosition;
    [Header("*회전 기준")]
    public Transform rotationTransform;
    [Header("*휘두르는 효과")]
    public Animator swingFX;
    [Space(20)]

    // 이 손이 들고 있는 무기.
    public WeaponData currentWeaponData;

    // 각도.
    public float angle;
    // 현재 공격중인지 여부.
    public bool isAttacking;

    // Components.
    private Animator animator;
    private Timer attackWaitingTimer;
    private Creature owner;

    // Callbacks.
    public Action cbOnBeginAttack;

    void Awake() {
        animator = this.GetComponentInChildren<Animator>();

        if (attackWaitingTimer == null) attackWaitingTimer = new Timer(attackWaitingTime);
    }

    void Update() {
        if (!passedAttackWaitingTime) {
            if (attackWaitingTimer.Counting(Time.deltaTime)) {
                passedAttackWaitingTime = true;
            }
        }
    }

    public bool IsEmpty() => currentWeaponData == null;
    public bool CanAttack() {
        if (!isAttacking && passedAttackWaitingTime) return true;
        return false;
    }

    public void SetOwner(Creature creature) => this.owner = creature;
    public void SetAngle(float angle) {
        this.angle = angle;

        float cAngle = angle;
        if (cAngle > 90f) cAngle = 180 - cAngle;
        else if (cAngle < -90f) cAngle = -180 - cAngle;

        rotationTransform.localRotation = Quaternion.Euler(0, 0, cAngle);
    }

    #region Attack

    public void OrderAttack() {
        // #0. 조건 확인
        // 공격 가능해야 한다.
        if (!CanAttack()) return;
        // 빈손이면 공격할 수 없음.
        if (IsEmpty()) return;

        // #1. 상태 변경.
        isAttacking = true;

        // #2. 공격 시작 콜백.
        cbOnBeginAttack?.Invoke();

        // #3. 공격 명령.
        Attack();
    }

    private void Attack() {
        // #1. 상태 변경.
        isAttacking = false;

        // #2. 애니메이션 설정.
        animator.SetTrigger("Attack");
        swingFX.SetTrigger("Attack");

        // #3. 공격 대기시간 초기화.
        passedAttackWaitingTime = false;

        // #4. 충돌 체크.
        CheckCollision();
    }

    private void CheckCollision() {
        // #0. 조건 검사.

        // #1. 레이어.
        LayerMask layerMask = LayerMask.GetMask("Creature");

        // #2. 공격 위치로부터 Scale 반경 원형 범위 레이캐스트 실행.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(attackPosition.position, attackPosition.localScale.z, Vector2.zero, layerMask);
        for (int i = 0; i < hits.Length; i++) {
            //// 충돌체 태그 검사: 적 여부 확인.
            //if (hits[i].transform.tag == "Enemy") {
            //    // 해당 적을 공격.
            //    Creature creature = hits[i].transform.GetComponent<Creature>();
            //    AttackTarget(creature);
            //}
            Creature creature = hits[i].transform.GetComponent<Creature>();
            if (creature != null && creature != owner) AttackTarget(creature);
        }
    }

    private void AttackTarget(Creature target) {
        // #1. 데미지 계산.
        float damage = owner.status.damage;
        // #2. 넉백 계산.
        Vector2 knockBackPower = (target.transform.position - this.transform.position).normalized;
        knockBackPower += Vector2.up;
        knockBackPower.Normalize();
        knockBackPower *= owner.status.knockBackPower;

        // #3. Target 공격.
        target.Hit(owner, damage, knockBackPower);
    }
    #endregion

    public void Activate() {
        this.gameObject.SetActive(true);
    }
    public void Inactivate() {
        this.gameObject.SetActive(false);
    }
    public void Initialize(WeaponData weapon) {
        currentWeaponData = weapon;
    }
    public void OnUnequip() { }
}