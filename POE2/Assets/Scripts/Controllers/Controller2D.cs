using CreativeSpore.SuperTilemapEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Controller2D : MonoBehaviour {

    // 오를 수 있는 최대 경사 각도.
    public float slopeLimit;

    // 점프로 처리되는 최소 수직 움직임.
    public float jumpThreshHold;

    // 얘가 true인 동안에는 단방향 플랫폼을 통과할 수 있다.
    public bool canPassOneWayPlatform;
    private float downJumpTimer;
    public bool isDashing;

    // 속도.
    public Vector2 velocity;

    public bool CollisionRight { get { return collisionState.right; } }
    public bool CollisionLeft { get { return collisionState.left; } }
    public bool CollisionAbove { get { return collisionState.above; } }
    public bool CollisionBelow { get { return collisionState.below; } }

    public CharacterRaycastOriginPoints raycastOrigins;
    public struct CharacterRaycastOriginPoints {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 botRight;
        public Vector2 botLeft;
    };

    private CharacterCollisionState2D collisionState;
    private class CharacterCollisionState2D {
        // 상하좌우 충돌상태.
        public bool right, left, above, below;
        public Action cbOnCollideBelow;
        // 접지 상태.
        public bool isGrounded, wasGrounded;
        // 경사 상태.
        public bool isMovingSlopeDown, isMovingSlopeUp;
        public float slopeAngle;

        // 충돌중인지 여부.
        public bool hasCollision() { return right || left || above || below; }

        // 충돌 상태 리셋.
        public void reset() {
            right = left = above = below = false;
            isGrounded = false;
            isMovingSlopeDown = isMovingSlopeUp = false;
            slopeAngle = 0;
        }
    }

    // 레이캐스트 카운트.
    private int rayCount_Horizontal = 5;
    private int rayCount_Vertical = 5;
    // 레이 사이 거리.
    private float distanceBetweenHorizontalRays;
    private float distanceBetweenVerticalRays;
    // 레이캐스트 보정 상수.
    private float raycastCorrectionConstant = 0.001f;
    // 이 프레임에서의 RaycastHit 리스트.
    private List<RaycastHit2D> raycastHitList;
    // 레이어 마스크.
    public LayerMask layerMask_Platform;
    public LayerMask layerMask_OneWayPlatform;

    private BoxCollider2D boxCollider;

    #region Event

    public event Action<RaycastHit2D> onCollidedEvent;
    public event Action<Collider2D> onTriggerEnterEvent;
    public event Action<Collider2D> onTriggerStayEvent;
    public event Action<Collider2D> onTriggerExitEvent;
    public Action cbOnGround;
    public void OnTriggerEnter2D(Collider2D collision) {
        onTriggerEnterEvent?.Invoke(collision);
    }
    public void OnTriggerStay2D(Collider2D collision) {
        onTriggerStayEvent?.Invoke(collision);
    }
    public void OnTriggerExit2D(Collider2D collision) {
        onTriggerExitEvent?.Invoke(collision);
    }

    #endregion
    public void SetPassOneWayPlatform(bool isDownJump = false) {
        canPassOneWayPlatform = true;
        downJumpTimer = 0.05f;
    }
    #region MonoBehaviour

    void Awake() {
        boxCollider = this.GetComponent<BoxCollider2D>();

        raycastOrigins = new CharacterRaycastOriginPoints();
        collisionState = new CharacterCollisionState2D();
        raycastHitList = new List<RaycastHit2D>();

        SetDistanceBetweenRay();

        layerMask_Platform |= 1 << 10;           // Platform.
        layerMask_Platform |= 1 << 11;
        layerMask_OneWayPlatform |= 1 << 11;
    }
    //bool temp;
    void Update() {
        if (canPassOneWayPlatform && !isDashing) {
            if (downJumpTimer > 0) {
                downJumpTimer -= Time.deltaTime;
            }

            //else if (downJumpTimer <= 0 && !temp) {
            //    canPassOneWayPlatform = false;
            //}
            else if (downJumpTimer <= 0) {
                canPassOneWayPlatform = false;
            }
        }
    }
    #endregion

    #region MovementAssistMethods

    // SetRaycastOrigin: 레이캐스트 원점 설정 및 보정.
    private void SetRaycastOrigin() {
        // #1. BoxCollider를 통해 원점 설정.
        Bounds bound = boxCollider.bounds;

        // #2. 원점을 경계 안으로 조금 줄인다.
        // (이 오브젝트와 다른 오브젝트가 충돌할 때 Collider가 겹치는 경우,
        // Raycast를 제대로 수행하지 못하는 현상 방지)
        bound.Expand(-2f * raycastCorrectionConstant);

        // #3. 레이캐스트 원점 설정.
        raycastOrigins.topLeft = new Vector2(bound.min.x, bound.max.y);
        raycastOrigins.topRight = bound.max;
        raycastOrigins.botLeft = bound.min;
        raycastOrigins.botRight = new Vector2(bound.max.x, bound.min.y);
    }

    // SetDistanceBetweenRay: 레이캐스트 사이 거리 설정.
    private void SetDistanceBetweenRay() {
        // #1. width, height 구하기.
        float width = boxCollider.size.x * Mathf.Abs(this.transform.localScale.x) - (2 * raycastCorrectionConstant);
        float height = boxCollider.size.y * Mathf.Abs(this.transform.localScale.y) - (2 * raycastCorrectionConstant);

        // #2. 레이 사이 거리 설정.
        distanceBetweenHorizontalRays = width / (rayCount_Horizontal - 1);
        distanceBetweenVerticalRays = height / (rayCount_Vertical - 1);
    }

    // CheckBlock: 진행 방향에 방해물이 있는지 체크.
    private bool CheckBlock(GameObject obj) {
        //// 만난 오브젝트가 Platform_Break 이고, 방향이 일치하면: 방해를 받지 않는다.
        //if (obj.layer == 11) {
        //    if (obj.GetComponent<Platform_Break>().checkDirection(this.gameObject))
        //        return false;
        //}

        // 이 외에는 모두 방해를 받는다.
        return true;
    }

    // HandleSlopeDown: 아래 경사를 체크한다. 경사를 내려갈 때는 속도를 증가시킨다.
    private void HandleSlopeDown(ref Vector2 deltaMovement) {
        // #1. 레이 원점: 오브젝트의 중앙 하단.
        Vector2 rayOrigin = (raycastOrigins.botLeft + raycastOrigins.botRight) / 2f;

        // #2. 레이 방향: 아래로.
        Vector2 rayDirection = Vector2.down;

        // #3. 레이 길이:
        float rayDistance = (raycastOrigins.botRight.x - raycastOrigins.botLeft.x) / 2 * Mathf.Tan(75 * Mathf.Deg2Rad);
        
        // #4. 레이캐스트 수행.
        DebugRay(rayOrigin, rayDirection * rayDistance, Color.yellow);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, layerMask_Platform);

        // #5. 아래에 무언가가 있다면,
        if (hit) {
            // 경사각.
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            
            // 경사각 = 0 이면 경사처리를 하지 않는다.
            if (angle == 0) return;

            // 경사를 내려가고 있는지 검사한다.
            // (부호가 다르면 경사방향!=이동방향 :: 경사를 오르고 있는 중임.)
            if (hit.normal.x * deltaMovement.x > 0) {
                // 속도 조절.
                float slopeModifier = GetSpeedCoefficient(angle, true);
                deltaMovement.x *= slopeModifier;
                deltaMovement.y += hit.point.y - rayOrigin.y - raycastCorrectionConstant;

                // 충돌 상태 업데이트: 경사 상태.
                collisionState.isMovingSlopeDown = true;
                collisionState.slopeAngle = angle;
            }
            // TODO:: 경사에 정지해있을 때 등의 처리도 여기서 해야 함.
            // 단, Move에서 해당 메소드로 진입하는 조건을 좀 수정해야 함.
        }
    }

    // HandleSlopeUp: 경사 오름 처리. 벽을 만나면 false, 경사를 만나면 true.
    private bool HandleSlopeUp(ref Vector2 deltaMovement, float angle) {
        // #1. 각도 체크: 90도이면 경사가 아닌 벽이다.
        if (Mathf.RoundToInt(angle) == 90) return false;

        // #2. 각도 체크: 경사가 걸어 올라갈 수 있을 정도일 때:
        if (angle < slopeLimit) {
            
            // 수직 성분 움직임이 점프가 아닐 때: 속도 조절. (줄인다)
            if (deltaMovement.y < jumpThreshHold) {
                float slopeModifier = GetSpeedCoefficient(angle, false);
                deltaMovement.x *= slopeModifier;
                deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
                
                // 경사 끝에 장애물이 있다면, 장애물을 통과하지 않도록 속도 조절.
                Vector2 slopeRayOrigin = (deltaMovement.x > 0) ? raycastOrigins.botRight : raycastOrigins.botLeft;
                RaycastHit2D slopeHit;
                if (collisionState.wasGrounded)
                    slopeHit = Physics2D.Raycast(slopeRayOrigin, deltaMovement.normalized, deltaMovement.magnitude, layerMask_Platform);
                else
                    slopeHit = Physics2D.Raycast(slopeRayOrigin, deltaMovement.normalized, deltaMovement.magnitude, layerMask_Platform & ~layerMask_OneWayPlatform);

                if (slopeHit) {
                    deltaMovement = slopeHit.point - slopeRayOrigin;
                    deltaMovement.x -= Mathf.Sign(deltaMovement.x) * raycastCorrectionConstant;
                }
                // 충돌 상태 업데이트: 경사 상태.
                collisionState.isMovingSlopeDown = true;
                collisionState.below = true;
            }
        }

        // #3. 각도 체크: 경사가 너무 가파르다면,
        else deltaMovement.x = 0;

        return true;
    }

    // MoveHorizontally: 수평 이동.
    //private void MoveHorizontally(ref Vector2 deltaMovement) {
    //    // #1. 이동 방향 정하기.
    //    int moveDirection = (int)Mathf.Sign(deltaMovement.x);

    //    // #2. 레이 원점.
    //    Vector2 rayOrigin = (moveDirection == 1) ? raycastOrigins.botRight : raycastOrigins.botLeft;

    //    // #3. 레이 방향.
    //    Vector2 rayDirection = new Vector2(moveDirection, 0);

    //    // #4. 레이 길이.
    //    float rayDistance = Mathf.Abs(deltaMovement.x) + raycastCorrectionConstant;

    //    // #5. 레이캐스트 수행.
    //    for (int i = 0; i < rayCount_Horizontal; i++) {
    //        if (i != 0) rayOrigin.y += distanceBetweenVerticalRays;

    //        DebugRay(rayOrigin, rayDirection * rayDistance, Color.red);
    //        RaycastHit2D hit;
    //        if (i == 0 && collisionState.wasGrounded)
    //            hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, layerMask_Platform);
    //        else
    //            hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, layerMask_Platform & ~layerMask_OneWayPlatform);

    //        if (hit) {
    //            // 하단 Ray가 벽이 아닌 경사를 만났을 때: 리스트 추가 후 나가기.
    //            if (i==0 && HandleSlopeUp(ref deltaMovement, Vector2.Angle(hit.normal, Vector2.up))) {
    //                raycastHitList.Add(hit);
    //                break;
    //            }
    //            // 오브젝트가 장애물을 만났을 때.
    //            if (CheckBlock(hit.transform.gameObject)) {
    //                // 장애물을 통과하지 않도록 속도 보정.
    //                deltaMovement.x = hit.point.x - rayOrigin.x;
    //                rayDistance = Mathf.Abs(deltaMovement.x);
    //                deltaMovement.x -= moveDirection * raycastCorrectionConstant;
    //            }

    //            // 이동 방향에 따라 충돌 상태 업데이트.
    //            if (moveDirection == 1) collisionState.right = true;
    //            else collisionState.left = true;

    //            // raycastHit 리스트에 추가.
    //            raycastHitList.Add(hit);

    //            if (rayDistance < raycastCorrectionConstant + 0.001f) break;
    //        }
    //    }
    //}
    private void MoveHorizontally(ref Vector2 deltaMovement) {
        // #1. 이동 방향 정하기.
        bool moveDirection = deltaMovement.x > 0;   // True + Right

        // #2. 레이 원점.
        Vector2 rayOrigin = (moveDirection) ? raycastOrigins.botRight : raycastOrigins.botLeft;

        // #3. 레이 방향.
        Vector2 rayDirection = (moveDirection) ? Vector2.right : Vector2.left;

        // #4. 레이 길이.
        float rayDistance = Mathf.Abs(deltaMovement.x) + raycastCorrectionConstant;

        // #5. 레이캐스트 수행: 정밀한 움직임을 위해 레이를 여러 개 쏜다.
        for (int i = 0; i < rayCount_Horizontal; i++) {
            // #5-1. i번째 레이 원점.
            Vector2 iRayOrigin = new Vector2(rayOrigin.x, rayOrigin.y + (i * distanceBetweenVerticalRays));

            // #5-2. i번째 레이캐스트 수행: Ground 상태일 때 하단 Ray가 단방향 플랫폼을 감지하게 하면, 경사진 단방향 플랫폼을 걸어갈 수 있다.
            DebugRay(iRayOrigin, rayDirection * rayDistance, Color.red);
            RaycastHit2D hit;
            if (i == 0 && collisionState.wasGrounded) {
                hit = Physics2D.Raycast(iRayOrigin, rayDirection, rayDistance, layerMask_Platform);
            }
            else {
                hit = Physics2D.Raycast(iRayOrigin, rayDirection, rayDistance, layerMask_Platform & ~layerMask_OneWayPlatform);
            }

            // #5-3. 무언가 감지한 경우.
            if (hit) {
                // 하단 Ray가 벽이 아닌 경사를 만났을 때.
                if (i == 0 && HandleSlopeUp(ref deltaMovement, Vector2.Angle(hit.normal, Vector2.up))) {
                    // raycastHit 리스트에 추가.
                    raycastHitList.Add(hit);
                    break;
                }
                // 오브젝트가 장애물을 만났을 때.
                if (CheckBlock(hit.transform.gameObject)) {
                    // deltaMovement가 장애물을 통과하지 않도록 하기 위해 새 deltaMovement 설정 및 rayDistance 계산.
                    deltaMovement.x = hit.point.x - iRayOrigin.x;
                    rayDistance = Mathf.Abs(deltaMovement.x);
                    deltaMovement.x += ((moveDirection) ? -1 : 1) * raycastCorrectionConstant;
                }
                // 이동 방향에 따라 충돌 상태 업데이트.
                if (moveDirection)
                    collisionState.right = true;
                else
                    collisionState.left = true;

                // raycastHit 리스트에 추가.
                raycastHitList.Add(hit);

                if (rayDistance < raycastCorrectionConstant + 0.001f) break;
            }
        }
    }
    // MoveVertically: 수직 이동.
    //private void MoveVertically(ref Vector2 deltaMovement) {
    //    // #1. 이동 방향.
    //    int moveDirection = (int)Mathf.Sign(deltaMovement.y);

    //    // #2. 레이 원점.
    //    Vector2 rayOrigin = (moveDirection == 1) ? raycastOrigins.topLeft : raycastOrigins.botLeft;

    //    // #3. 레이 방향.
    //    Vector2 rayDirection = new Vector2(0, moveDirection);

    //    // #4. 레이 길이.
    //    float rayDistance = Mathf.Abs(deltaMovement.y) + raycastCorrectionConstant;

    //    // #5. 수직이동 전 수평이동이 이루어졌으므로, 해당 위치에서 수행하도록 한다.
    //    rayOrigin.x += deltaMovement.x;

    //    // #6. 위로 이동시, 단방향 플랫폼 레이어를 무시하도록 한다. (통과가 가능함)
    //    LayerMask mask = layerMask_Platform;
    //    if ((moveDirection == 1 && !collisionState.wasGrounded) || canPassOneWayPlatform)
    //        mask &= ~layerMask_OneWayPlatform;

    //    // #7. 레이캐스트 수행.
    //    for (int i = 0; i < rayCount_Vertical; i++) {
    //        if (i != 0) rayOrigin.x += distanceBetweenHorizontalRays;

    //        DebugRay(rayOrigin, rayDirection * rayDistance, Color.green);
    //        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, mask);

    //        if (hit) {
    //            // 장애물을 만났을 때 장애물 통과하지 않도록 속도 조정.
    //            if (CheckBlock(hit.transform.gameObject)) {
    //                deltaMovement.y = hit.point.y - rayOrigin.y;
    //                rayDistance = Mathf.Abs(deltaMovement.y);
    //                deltaMovement.y -= raycastCorrectionConstant;
    //            }

    //            // 이동 방향에 따라 충돌 상태 업데이트.
    //            if (moveDirection == 1) collisionState.above = true;
    //            //else collisionState.below = true;
    //            else SetBelowCollisionTrue();

    //            // 리스트 추가.
    //            raycastHitList.Add(hit);

    //            if (rayDistance < raycastCorrectionConstant + 0.001f) break;
    //        }
    //    }

    //}
    private void MoveVertically(ref Vector2 deltaMovement) {
        // #1. 이동 방향 정하기.
        bool moveDirection = deltaMovement.y > 0;

        // #2. 레이 원점.
        Vector2 rayOrigin = (moveDirection) ? raycastOrigins.topLeft : raycastOrigins.botLeft;

        // #3. 레이 방향.
        Vector2 rayDirection = (moveDirection) ? Vector2.up : Vector2.down;
        // #4. 레이 길이.
        float rayDistance = Mathf.Abs(deltaMovement.y) + raycastCorrectionConstant;
        // #5. 수직이동 전 수평 이동이 이루어졌으므로, 해당 위치에서 수행하도록 함.
        rayOrigin.x += deltaMovement.x;

        // #6. 위로 이동시, 단방향 플랫폼 레이어를 무시하도록 한다. (통과가 가능하도록)
        LayerMask mask = layerMask_Platform;
        //if ((moveDirection && !collisionState.wasGrounded) || canPassOneWayPlatform) {
        //    mask &= ~layerMask_OneWayPlatform;
        //}
        if (moveDirection) canPassOneWayPlatform = true;
        if (canPassOneWayPlatform) mask &= ~layerMask_OneWayPlatform;
        //if ((moveDirection) || canPassOneWayPlatform) {
        //    mask &= ~layerMask_OneWayPlatform;
        //}

        // #7. 레이캐스트 수행: 정밀한 움직임을 위해 레이를 여러 개 쏜다.
        for (int i = 0; i < rayCount_Vertical; i++) {
            // #7-1. i번째 레이 원점.
            Vector2 iRayOrigin = new Vector2(rayOrigin.x + (i * distanceBetweenHorizontalRays), rayOrigin.y);
            if (!moveDirection) {
                DebugRay(iRayOrigin, Vector2.up * raycastCorrectionConstant, Color.red);
                RaycastHit2D _hit = Physics2D.Raycast(iRayOrigin, Vector2.up, raycastCorrectionConstant, layerMask_OneWayPlatform);
                if (_hit) continue;
            }

            // #7-2. i번째 레이캐스트 수행.
            DebugRay(iRayOrigin, rayDirection * rayDistance, Color.green);
            RaycastHit2D hit = Physics2D.Raycast(iRayOrigin, rayDirection, rayDistance, mask);

            // #7-3. 무언가 감지한 경우.
            if (hit) {
                //if (this.transform.name == "Player" && hit.transform.gameObject.layer == 11) temp = true;
                //else temp = false;

                // 오브젝트가 장애물을 만났을 때.
                if (CheckBlock(hit.transform.gameObject)) {
                    // deltaMovement가 장애물을 통과하지 않도록 하기 위해 새 deltaMovement 설정 및 rayDistance 계산.
                    deltaMovement.y = hit.point.y - iRayOrigin.y;
                    rayDistance = Mathf.Abs(deltaMovement.y);
                    deltaMovement.y += ((moveDirection) ? -1 : 1) * raycastCorrectionConstant;
                }


                // 이동 방향에 따라 충돌 상태 업데이트.
                if (moveDirection)
                    collisionState.above = true;
                else
                    collisionState.below = true;

                // raycastHit 리스트에 추가.
                raycastHitList.Add(hit);

                if (rayDistance < raycastCorrectionConstant + 0.001f) break;
            }


        }
    }


    private float GetSpeedCoefficient(float Angle, bool isDown) {
        if (isDown) {
            return 1 + (Angle * Angle / 8100f / 5f);
        }
        else {
            return 1 - (Angle * Angle / 8100f / 5f);
        }
        //// 경사 속도 곡선에서, 경사 각도에 해당하는 속도 계수를 얻는다.
        //float slopeModifier = slopeSpeedCoefficient.Evaluate(-Angle);
        //return 1f;
    }
    #endregion

    #region Movement Methods

    public void Move(ref Vector2 velocity) {
        Vector2 deltaMovement = velocity * Time.deltaTime;

        // #1. 접지 상태 저장.
        collisionState.wasGrounded = collisionState.below;

        // #2. 충돌 상태 초기화.
        collisionState.reset();
        raycastHitList.Clear();

        // #3. 레이캐스트 원점 설정.
        SetRaycastOrigin();

        // #4. 경사를 내려가고 있는지 확인.
        if (deltaMovement.y < 0 && collisionState.wasGrounded)
            HandleSlopeDown(ref deltaMovement);

        // #5. 수평, 수직 방향 이동 처리.
        if (deltaMovement.x != 0) MoveHorizontally(ref deltaMovement);
        if (deltaMovement.y != 0) MoveVertically(ref deltaMovement);

        // #6. 실제 이동.
        this.transform.Translate(deltaMovement, Space.World);

        // #7. 속도 계산.
        if (Time.deltaTime > 0) velocity = deltaMovement / Time.deltaTime;

        // #8. 접지 상태 업데이트.
        if (!collisionState.wasGrounded && collisionState.below) {
            collisionState.isGrounded = true;
            cbOnGround?.Invoke();
        }

        // #9. 경사를 올라가는 중이라면, y 속도를 0으로.
        if (collisionState.isMovingSlopeUp) velocity.y = 0;

        // #10. Raycast를 통해 감지된 오브젝트드를 충돌 이벤트 핸들러에 전달.
        if (onCollidedEvent != null) {
            for (int i = 0; i < raycastHitList.Count; i++) {
                onCollidedEvent(raycastHitList[i]);
            }
        }

        this.velocity = velocity;
        // #11. 단방향 플랫폼 통과 가능 여부 초기화.
        //canPassOneWayPlatform = false;

    }

    #endregion

    public void DebugRay(Vector3 origin, Vector3 dirdis, Color color) {
        Debug.DrawRay(origin, dirdis, color);
    }
}