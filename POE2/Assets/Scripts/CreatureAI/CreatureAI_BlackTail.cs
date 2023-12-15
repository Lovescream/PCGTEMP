using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_BlackTail : CreatureAI {

    public bool isFlying;
    public bool isMoving;
    private float originGravityScale;

    private AIPath path;

    #region MonoBehaviours

    protected override void Awake() {
        base.Awake();

        this.GetComponent<Controller2D>().onTriggerEnterEvent += OnAttacking;
        this.GetComponent<Controller2D>().onTriggerStayEvent += OnAttacking;
    }

    protected override void Update() {
        base.Update();
    }

    #endregion

    public override void Initialize() {
        base.Initialize();

        originGravityScale = CC2D.gravityScale;
    }

    #region States

    protected override void OnIdle() {
        base.OnIdle();

        if (isFlying) Land();
        targetingBrain.FindEnemyInCircle();
    }
    protected override void OnCognition() {
        base.OnCognition();
        targetingBrain.FindEnemyInCircle();

        if (!isMoving) {
            PathTile destination = creature.room.GetRandomNotWallTile();
            MoveInitialize(destination.x, destination.y);
            isMoving = true;
        }

        if (!isFlying) Fly();
    }

    #endregion

    #region Action

    private void Fly() {
        this.transform.DOMoveY(0.25f, 0.5f).OnComplete(() => PathMove());
        originGravityScale = CC2D.gravityScale;
        CC2D.gravityScale = 0;
        isFlying = true;
        animator.SetTrigger("Jump");
    }

    private void Land() {
        CC2D.gravityScale = originGravityScale;
        isFlying = false;
        animator.SetTrigger("Land");
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
        path.Simplify();
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

        // #3. 이동한다.
        this.transform.DOMove(destination, 0.25f).OnComplete(() => {
            if (PathMove() == false) {
                Debug.Log("ASDASDAS");
                // 이동 종료.
                isMoving = false;
            }
            });

        // #4. 이번 이동을 끝낸다.
        return true;
    }

    private void OnAttacking(Collider2D collision) {
        // #1. 충돌한 대상이 크리쳐인지 확인.
        Creature collidedCreature = collision.transform.GetComponent<Creature>();
        if (collidedCreature) {
            // #2. 해당 크리쳐가 적인지 확인.
            if (creature.IsEnemy(collidedCreature)) {
                // #3. 경계중이라면, 해당 크리쳐를 2초간 Panic 상태에 빠지게 함.
                if (isFlying) {
                    collidedCreature.Panic(creature, 2f);
                }
            }
        }
    }

    #endregion

}