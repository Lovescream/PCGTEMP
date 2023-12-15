using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Sparrow : CreatureAI {

    [Header("다음 행동까지의 시간")]
    public float nextActionTimerMin;
    public float nextActionTimerMax;
    [Header("점프 정보")]
    public float jumpTime;
    public float jumpAngle;
    public float maxJumpDistance;
    private float remainDistance;
    private bool isJumping;
    [Header("비행 정보")]
    public float flySpeed;
    public bool isFlying;
    //public float flyAngle;
    private float originGravityScale;

    private float lookDirection;

    private Timer nextActionTimer;  // 다음 행동 타이머. 접지 상태일 때 동작한다.

    private AIPath path;
    private bool isMoving;

    // Components.
    private FlightController2D FC2D;

    #region MonoBehaviours

    protected override void Awake() {
        base.Awake();
    }

    protected override void Update() {
        base.Update();
    }

    #endregion

    public override void Initialize() {
        base.Initialize();

        FC2D = this.GetComponent<FlightController2D>();
        nextActionTimer = new Timer(1, false);
        SetNextActionTimer();

        FC2D.cbOnEndFlight += SetGravityScaleToOrigin;
        CC2D.controller.cbOnGround += Land;
        creature.cbOnDie += SetGravityScaleToOrigin;
        originGravityScale = CC2D.gravityScale;
    }

    #region States

    protected override void OnIdle() {
        base.OnIdle();
        targetingBrain.FindEnemyInCircle();

        // 죽으면 아무것도 하지 않음.
        if (creature.IsDie) return;

        if (!isMoving && CC2D.controller.CollisionBelow) {
            if (nextActionTimer.Counting(Time.deltaTime)) {
                int actionIndex = Random.Range(0, 3);
                switch (actionIndex) {
                    case 0:
                        LookAround();
                        break;
                    case 1:
                        Pick();
                        break;
                    case 2:
                        PathTile destination = creature.room.GetRandomOnGroundTile();
                        // 이동 시작.
                        MoveInitialize(destination.x, destination.y);
                        break;
                    default:
                        break;
                }
                SetNextActionTimer();
            }
        }
    }

    #endregion

    #region Action

    // LookAround: 두리번거리기.
    // -5: 랜덤, -1: back, 0: 정면, 1: Front.
    private void LookAround(int direction = -5) {
        int num;
        if (direction == -5) num = Random.Range(-1, 2);
        else num = direction;
        lookDirection = num;
        animator.SetFloat("LookDirection", lookDirection);
    }

    // Pick: 쪼기.
    private void Pick() {
        // #1. 정면을 바라보게 함.
        LookAround(0);

        // #2. 쪼기.
        animator.SetTrigger("Pick");
    }

    // MoveInitialize: 현재 위치로부터 (x, y)까지의 Path를 받아 이동 명령.
    private void MoveInitialize(int x, int y) {
        int curX = (int)this.transform.position.x;
        int curY = (int)this.transform.position.y;
        if (curX == x && curY == y) return;
        path = GetPath(curX, curY, x, y);
        if (path == null) {
            return;
        }
        path.Simplify_Flight();
        path.Debug_LogAllPath();
        PathMove();
        isMoving = true;
    }

    // PathMove: 현재 위치에서 다음 path까지 이동 명령.
    private bool PathMove() {
        // #1. 이동 시작 및 도착 지점 정하기.
        Vector2 start = this.transform.position;
        Vector2 destination = path.GetNextPath();

        // #2. 도착 지점이 없다면 이동 끝.
        if (destination.x < -10000000) {
            return false;
        }

        // #3. 높이차가 없으면 폴짝 이동하고, 있다면 날아서 이동하게 한다.
        float deltaHeight = Mathf.Abs(start.y - destination.y);
        if (deltaHeight < 0.5f) JumpInitialize(destination.x - start.x);
        else Fly(start, destination);

        // #4. 이번 이동을 끝낸다.
        return true;
    }

    private void Fly(Vector2 start, Vector2 destination) {
        // #1. 정면을 바라보게 함.
        LookAround(0);

        // #2. 중력 설정.
        originGravityScale = CC2D.gravityScale;
        CC2D.gravityScale = 0;

        // #3. 각도 설정.
        //flyAngle = Random.Range(30f, 75f);

        // #4. 비행 설정.
        isFlying = true;
        //flightTimer.SetTimer(Random.Range(0.125f, 0.5f));)
        //Debug.Log(FC2D);
        //Debug.Log(start);
        //Debug.Log(destination);
        Vector3 scale = this.transform.localScale;
        scale.x = Mathf.Sign(destination.x - start.x) * Mathf.Abs(scale.x);
        if (scale.x != 0) this.transform.localScale = scale;
        FC2D.Flight(start, destination);

        // #5. 애니메이션 설정.
        animator.SetFloat("MoveDistance", flySpeed);
    }

    // JumpInitialize: deltaX 만큼 점프하도록 명령. deltaX < 0 이면 왼쪽으로 점프. 이 개체를 해당 방향으로 바라보게 하고 remainDistance를 설정한다.
    // Jump: 점프한다. 한 번에 maxJumpDistance 만큼만 점프하고, remainDistance가 남아있다면 Land() 등에서 계속 Jump()를 불러 점프하도록 한다.
    // 점프하는 방향은 현재 이 개체가 바라보고 있는 방향을 기준으로 한다. (따라서, 반대 방향으로 점프하고 싶다면 먼저 localScale.x 부호를 바꿔야 하겠다!!)
    // 순서: JumpInitialize(명령) -> Jump -> Jump(float) -> ... -> Jump -> Jump(float) -> ... -> Jump -X
    private void JumpInitialize(float deltaX) {
        // #1. 점프하는 방향으로 바라보도록 한다.
        Vector3 scale = this.transform.localScale;
        if (scale.x * deltaX < 0) scale.x = Mathf.Sign(deltaX) * Mathf.Abs(scale.x);
        this.transform.localScale = scale;

        // #2. 점프 거리 설정.
        remainDistance = Mathf.Abs(deltaX);

        // #3. 점프 명령.
        Jump();
    }
    private void Jump() {
        if (remainDistance > maxJumpDistance) {
            Jump(maxJumpDistance);
            remainDistance -= maxJumpDistance;
        }
        else {
            Jump(remainDistance);
            remainDistance = 0;
        }
    }
    private void Jump(float distance) {
        // #1. 정면을 바라보게 함.
        LookAround(0);

        // #2. 방향 계산.
        bool isLeftDirection = this.transform.localScale.x < 0;

        // #3. 속도 계산.
        float speed = distance / (jumpTime * Mathf.Cos(jumpAngle * Mathf.Deg2Rad));
        float velocityX = speed * Mathf.Cos(jumpAngle * Mathf.Deg2Rad) * ((isLeftDirection) ? -1 : 1);
        float velocityY = speed * Mathf.Sin(jumpAngle * Mathf.Deg2Rad);
        originGravityScale = CC2D.gravityScale;
        CC2D.gravityScale = 2 * distance * Mathf.Tan(jumpAngle * Mathf.Deg2Rad) / (jumpTime * jumpTime);

        // #4. 점프.
        CC2D.SetVelocityX(velocityX);
        CC2D.SetVelocityY(velocityY);
        isJumping = true;

        // #5. 애니메이션 설정.
        animator.SetFloat("MoveDistance", distance);
    }

    // Land: 착지. CC2D에서 이 개체가 접지상태가 되면 불림.
    // 중력 처리 및 각 동작이 끝날 때의 처리.
    private void Land() {
        // #1. 중력을 원래대로.
        SetGravityScaleToOrigin();
        isMoving = false;
        // #2. 동작 후처리.
        if (isJumping) {
            //Debug.Log("[참새] 착지! 점프 끝!");
            if (Mathf.Abs(remainDistance) > 0) {
                Jump();
            }
            else {
                isJumping = false;
                animator.SetFloat("MoveDistance", 0);
                animator.SetTrigger("IsLanding");
                CC2D.SetVelocityX(0);

                PathMove();
            }
        }
        else if (isFlying) {
            //Debug.Log("[참새] 착지! 비행 끝!");
            isFlying = false;
            animator.SetFloat("MoveDistance", 0);
            animator.SetTrigger("IsLanding");
            CC2D.SetVelocityX(0);

            PathMove();
        }
       //UnityEditor.EditorApplication.isPaused = true;
    }
    #endregion





    // SetNextActionTimer: 다음 행동을 하기까지의 시간을 설정한다.
    private void SetNextActionTimer(float targetTime = -1) {
        if (targetTime == -1) targetTime = Random.Range(nextActionTimerMin, nextActionTimerMax);
        nextActionTimer.SetTimer(targetTime);
    }

    private void SetGravityScaleToOrigin(Creature creature = null) => CC2D.gravityScale = originGravityScale;
    private void SetGravityScaleToOrigin() => CC2D.gravityScale = originGravityScale;


















}