using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Slime : CreatureAI {

    [Header("수평 거리")]
    public float minDistance = 2f;
    public float maxDistance = 5f;
    [Header("점프 높이")]
    public float minJumpHeight = 1f;
    public float maxJumpHeight = 2.5f;
    [Header("공격 주기")]
    public float attackWaitingTime = 2f;
    private Timer attackWaitingTimer;
    private bool passedAttackWaitingTime;

    private bool isAttacking;
    public bool isJumping;
    public bool isFalling;

    // Components.

    #region MonoBehaviours

    protected override void Awake() {
        base.Awake();

        if (attackWaitingTimer == null) attackWaitingTimer = new Timer(attackWaitingTime);

        this.GetComponent<Controller2D>().onTriggerEnterEvent += OnAttacking;
        this.GetComponent<Controller2D>().onTriggerStayEvent += OnAttacking;
        this.GetComponent<Controller2D>().cbOnGround += Land;
    }

    protected override void Update() {
        base.Update();
        
        // 공격 대기 시간 카운팅.
        if (!passedAttackWaitingTime) {
            if (attackWaitingTimer.Counting(Time.deltaTime)) {
                passedAttackWaitingTime = true;
            }
        }

        // 떨어짐 처리.
        if (!CC2D.controller.CollisionBelow && CC2D.velocity.y < 0 && !isFalling) Fall();

        // 점프, 떨어지고 있을 때 다음 프레임에서 착지할 것 같으면 Land 실행.
        if (isJumping) if (CC2D.velocity.y * (CC2D.velocity.y - CC2D.gravityScale * Time.deltaTime) <= 0) Land();

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
        if (CC2D.velocity.x > 0) this.transform.localScale = new Vector3(1, 1, 1);
        else if (CC2D.velocity.x < 0) this.transform.localScale = new Vector3(-1, 1, 1);
    }

    private void Jump(float speedX, float speedY) {
        CC2D.SetVelocityX(speedX);
        CC2D.SetVelocityY(speedY);
        isJumping = true;
        if (isFalling) isFalling = false;
        animator.SetTrigger("Jump");
    }

    private void Fall() {
        isFalling = true;
        animator.SetTrigger("Fall");
    }

    private void Land() {
        if (isJumping && isFalling) {
            isFalling = false;
        }
        else if (isJumping) {
            isJumping = false;
        }
        else if (isFalling) {
            CC2D.SetVelocityX(0);
            isFalling = false;
            if (isAttacking) isAttacking = false;
            // 공격 시간 초기화.
            passedAttackWaitingTime = false;
        }
        animator.SetTrigger("Land");

    }

    private void Attack() {
        // #0. 조건 검사.
        // Target이 없으면 공격하지 않음.
        if (targetingBrain.CurrentTarget == null) return;
        // 공격이 가능하지 않으면 실행햐지 않음.
        if (!CanAttack) return;
        // 이미 공격중이라면 실행하지 않음.
        if (isAttacking) return;

        // #1. 공격 상태 설정.
        isAttacking = true;

        // #2. 점프 정보 계산.
        float deltaX = targetingBrain.CurrentTarget.transform.position.x - this.transform.position.x;
        int xDirection = (int)(Mathf.Sign(deltaX));
        if (deltaX < 0) deltaX *= -1;
        deltaX = Mathf.Clamp(deltaX, minDistance, maxDistance);

        float deltaY = targetingBrain.CurrentTarget.transform.position.y - this.transform.position.y;
        int yDirection = (int)(Mathf.Sign(deltaY));
        if (deltaY < 0) { deltaY = (maxJumpHeight + minJumpHeight) / 2f; }
        else deltaY = Mathf.Clamp(deltaY, minJumpHeight, maxJumpHeight);
        float height = Mathf.Clamp(deltaY + minJumpHeight / 5, minJumpHeight, maxJumpHeight);

        float t = Mathf.Sqrt(2 / CC2D.gravityScale) * (Mathf.Sqrt(height) + Mathf.Sqrt(height - deltaY));
        float xVelocity = deltaX / t;
        float yVelocity = Mathf.Sqrt(2 * CC2D.gravityScale * height);

        // #3. 점프시킨다.
        Jump(xDirection * xVelocity, yVelocity);
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

    #endregion

    private bool CanAttack => passedAttackWaitingTime;

    
}