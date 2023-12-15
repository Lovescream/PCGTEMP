using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class CharacterController2D : MonoBehaviour {
    public bool isPlayer;
    public bool isInputMoveX;

    [Header("중력/저항력 정보")]
    public float gravityScale;
    public float airResistance;
    public float friction;
    [Header("속도 정보")]
    public float speed;
    public float speedBonusRatio;
    public float speedSlowRatio;
    [Header("점프 정보")]
    public float jumpHeight;
    public int currentJumpCount;
    public int maxJumpCount;
    [Header("비행 정보")]
    public bool isFlying;
    [Header("대쉬 정보")]
    public bool availableDash; // 대쉬 잠금 해제: 튜토리얼에서 잠금해제됨.
    public float dashSpeed;
    public int remainDashCount;
    public int maxDashCount;
    public float dashChargeTime;
    private float dashChargeTimer;
    public float dashTime;
    private float dashTimer;
    public bool isDashing;
    private bool canDash;
    private bool canPassOneWayPlatformBeforeDash;
    public State state;

    private Vector2 velocity_Input;
    public Vector2 velocity;
    private Vector2 velocity_Move;


    // AnimatorHashes.
    public int ahIsGrounded;
    public int ahIsHorizontalMove;
    public int ahIsRun;

    // Callbacks.
    private Animator animator;
    public Controller2D controller;
    private Creature creature;

    // Callbacks.
    public Action cbOnBeginJump;
    public Action cbOnBeginLand;
    public Action cbOnBeginFall;
    public Action cbOnIdle;
    public Action cbOnRun;
    public Action cbOnBeginDash;
    public Action cbOnEndDash;

    #region MonoBehaviour

    void Awake() {
        animator = this.GetComponent<Animator>();
        controller = this.GetComponent<Controller2D>();
        creature = this.GetComponent<Creature>();

        creature.cbOnDie += OnDie;
    }

    void Update() {
        ChargeDash();
        // #1. 해당 방향으로 이동할 수 없을 때 속도를 0으로 만든다.
        if (controller.CollisionRight && velocity.x > 0) velocity.x = 0;
        if (controller.CollisionLeft && velocity.x < 0) velocity.x = 0;
        if (controller.CollisionAbove && velocity.y > 0) velocity.y = 0;
        if (controller.CollisionBelow && velocity.y < 0 && !controller.canPassOneWayPlatform) velocity.y = 0;
        
        // #2. 수평 속도 계산.
        float velocityX = 0;
        if (isInputMoveX) {
            // #2-1. 입력 속도를 바탕으로 속도를 계산.
            velocityX = GetVelocity(velocity_Input.x);

            // #2-2. velocityX가 0인지 아닌지에 따라 Run, Idle 둘 중 하나의 상태를 가진다.
            // 따라서, velocityX를 0으로 만드는 작업은 이 위에서 이루어져야 한다.
            SetStateRunOrIdleBasedOnVelocityX(velocityX);

            // #2-3. 최종 속도 계산.
            float resistance = controller.CollisionBelow ? friction : airResistance;
            velocity.x = Mathf.Lerp(velocity.x, velocityX, resistance * Time.deltaTime);
        }

        // #3. 수직 속도 계산: 비행중이 아니면 중력 효과를 받게 한다.
        if (!isFlying) velocity.y -= gravityScale * Time.deltaTime;

        // 실제 이동.
        controller.Move(ref velocity);
        DashOnUpdate();
        HandlingAboutGrounded();
        // 애니메이션 설정.
        if (isPlayer) {
            animator.SetBool("IsGrounded", controller.CollisionBelow);
            animator.SetBool("IsHorizontalMove", velocityX != 0);
        }
        // 상태에 따른 액션.
        switch (state) {
            case State.IDLE:
                cbOnIdle?.Invoke();
                break;
            case State.RUN:
                cbOnRun?.Invoke();
                break;
        }
    }

    #endregion

    #region Jump
    
    public void Jump() {
        // #0. 조건 확인.
        // IDLE, RUN, JUMP 상태에서만 점프가 가능하고,
        // JUMP 상태에서는, 점프 회수가 최대치에 도달하지 않아야 가능하다.
        if ((state != State.IDLE) && (state != State.RUN) && (state != State.JUMP || currentJumpCount >= maxJumpCount)) return;

        // #1. 상태 변경.
        state = State.JUMP;

        // #2. 점프 횟수 증가.
        currentJumpCount++;

        // #3. 속도 변경.
        velocity.y = Mathf.Sqrt(2 * gravityScale * jumpHeight);

        // #4. 점프 콜백.
        cbOnBeginJump?.Invoke();
    }

    public void DownJump() {
        // #0. 조건 확인.
        // IDLE, RUN 상태여야 한다.
        if (state != State.IDLE && state != State.RUN) return;

        // #1. 단방향 플랫폼을 내려갈 수 있도록 한다.
        controller.SetPassOneWayPlatform(true);

        // #2. 속도 변경.
        velocity.y = -Mathf.Sqrt(2 * gravityScale * jumpHeight) / 2;
    }

    public void Land() {
        // #0. 조건 확인.
        // JUMP 상태에서만 착지가 가능하다.
        if (state != State.JUMP) return;

        // #1. 상태 변경.
        state = State.IDLE;

        // #2. 점프 횟수 초기화.
        currentJumpCount = 0;

        // #3. 착지 콜백.
        cbOnBeginLand?.Invoke();
    }

    public void Fall() {
        // #0. 조건 확인.
        if (state != State.IDLE && state != State.RUN) return;

        // #1. 상태 변경.
        state = State.JUMP;

        // #2. 점프 횟수 증가.
        currentJumpCount++;

        // #3. 떨어짐 콜백.
        cbOnBeginFall?.Invoke();
    }

    // HandlingAboutGrounded: 접지 상태에 관한 처리.
    // 접지 상태에 따라 착지인지(Land) 떨어지는 중인지(Fall) 판단.
    public void HandlingAboutGrounded() {
        if (controller.CollisionBelow) {
            Land();
        }
        else {
            Fall();
        }
    }

    #endregion

    #region Fly

    public void SetFlying(bool isFlying) => this.isFlying = isFlying;

    #endregion

    #region Dash

    private void ChargeDash() {
        if (remainDashCount < maxDashCount) {
            dashChargeTimer += Time.deltaTime;
            if (dashChargeTimer >= dashChargeTime) {
                dashChargeTimer = 0;
                if (++remainDashCount > maxDashCount) remainDashCount = maxDashCount;
            }
        }
    }

    public void Dash(Vector2 dashVelocity) {
        // #0. 조건 확인.
        // 대쉬가 잠금해제되어야 함.
        if (!availableDash) return;
        // 대쉬를 사용할 수 있어야 함.
        if (canDash) return;
        // 기절, 죽은 상태에서는 사용할 수 없다.
        if (state == State.STUN || state == State.DIE) return;
        // 남은 대쉬 횟수가 잇어야 한다.
        if (remainDashCount <= 0) return;
        // 대쉬중이 아니어야 한다.
        if (isDashing) return;

        // #1. 대쉬 시간 초기화.
        dashTimer = 0;
        // #2. 대쉬 시작 콜백.
        cbOnBeginDash?.Invoke();
        // #3. 남은 대쉬 횟수 감소.
        remainDashCount--;
        // #4. 상태 변경.
        isDashing = true;

        // #5. 속도 변경.
        velocity = dashVelocity * dashSpeed;

        // #6. CC2D 설정.
        canPassOneWayPlatformBeforeDash = controller.canPassOneWayPlatform; // 대쉬 전 '단방향무시여부' 값 저장.
        controller.canPassOneWayPlatform = true;                            // 대쉬중엔 단방향 무시.
        controller.isDashing = true;
    }

    private void DashOnUpdate() {
        if (isDashing) {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashTime) StopDashOnDashMove();
        }
    }

    private void StopDash() {
        if (isDashing) {
            isDashing = false;
            velocity = Vector2.zero;
            controller.canPassOneWayPlatform = canPassOneWayPlatformBeforeDash;
            controller.isDashing = false;
            cbOnEndDash?.Invoke();
        }
    }
    private void StopDashOnDashMove() {
        if (isDashing) {
            isDashing = false;
            velocity /= 3f;
            controller.canPassOneWayPlatform = canPassOneWayPlatformBeforeDash;
            controller.isDashing = false;
            cbOnEndDash?.Invoke();
        }
    }

    #endregion

    #region Die
    public void OnDie(Creature hitter) {
        this.animator.Play("Die");
        state = State.DIE;
    }
    #endregion

    #region Fx

    private void CreateDashFx() {

    }

    #endregion

    public void SetVelocityY(float velocityY) => velocity.y = velocityY;
    public void SetVelocityX(float velocityX) => velocity.x = velocityX;
    public void AddVelocity(Vector2 velocity) { this.velocity += velocity; }
    public void Move(ref Vector2 velocity) { controller.Move(ref velocity); }

    public void Flip(bool isRight) {
        Vector3 scale = this.transform.localScale;
        scale.x = isRight ? 1 : -1;
        this.transform.localScale = scale;
    }
    
    // SetStateRunOrIdleBasedOnVelocityX: x 속도를 기준으로 IDLE, RUN 사이의 상태 변경.
    public void SetStateRunOrIdleBasedOnVelocityX(float velocityX) {
        if (velocityX == 0) {
            if (state == State.RUN)
                state = State.IDLE;
        }
        else {
            if (state == State.IDLE)
                state = State.RUN;
        }
    }

    // SetInputVelocity: 입력 속도 설정.
    public void SetInputVelocity(float x, float y) {
        velocity_Input = new Vector2(x, y);
    }
    // GetVelocity: 이동속도에 보너스 및 둔화를 적용함.
    private float GetVelocity(float inputVelocity) {
        float bonus = 1 + speedBonusRatio;
        float slow = 1 - speedSlowRatio;

        float velocity = inputVelocity * speed * bonus * slow;

        return velocity;
    }

    public enum State {
        NONE = -1,
        IDLE = 0,
        RUN,
        JUMP,
        ATTACK,
        STUN,
        DIE,
    }
}