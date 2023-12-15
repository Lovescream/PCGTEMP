using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightController2D : MonoBehaviour {

    [Header("비행 속도")]
    public float flightSpeed;
    [Header("가속 시간")]
    public float increaseModifier = 0.1f;
    [Header("감속 시간")]
    public float decreaseModifier = 0.9f;

    private Vector2 startPosition;
    private Vector2 topPoint;
    private Vector2 endPosition;

    private float velocityY;
    private float velocityX;

    private float height;       // 현재 비행 높이.
    private float angleR;       // 현재 비행 각도.
    private float flightTimer;  // 현재 비행 타이머.
    private float increaseTime; // 현재 비행 가속 종료 시각.
    private float decreaseTime; // 현재 비행 감속 시작 시각.
    private float totalTime;    // 현재 비행 완료 시각.

    private int flightState;    // 0: 정지, 1: 상승중, 2: 하강중.

    // callbacks.
    public Action cbOnEndFlight;

    // components.
    private CharacterController2D CC2D;
    
    void Awake() {
        CC2D = this.GetComponent<CharacterController2D>();
    }

    void Update()  {
        if (flightState == 0) {
            if (startPosition != endPosition) { // 비행중이 아닐 때에는 둘 다 Zero가 되므로 그 때는 진입하지 않도록 하기 위함.
                CC2D.velocity = Vector2.Lerp(CC2D.velocity, Vector2.zero, Time.deltaTime * 5);
            }
        }
        else if (flightState == 1 || flightState == 2) {
            if (flightTimer < increaseTime) {
                // 속도 증가.
                CC2D.velocity.x = velocityX;
                CC2D.velocity.y += velocityY / increaseTime * Time.deltaTime;
                flightTimer += Time.deltaTime;
            }
            else if (flightTimer < decreaseTime) {
                // 속도 일정.
                flightTimer += Time.deltaTime;
            }
            else if (flightTimer < totalTime) {
                float x = this.transform.position.x;
                float y = this.transform.position.y;
                if (Mathf.Abs(x - endPosition.x) < 1f && endPosition.y < y && y < endPosition.y + 2.5f) {
                    CC2D.controller.canPassOneWayPlatform = false;
                }
                else {
                    CC2D.controller.canPassOneWayPlatform = true;
                }
                // 속도 감소.
                CC2D.velocity.x = velocityX;
                CC2D.velocity.y -= velocityY / (totalTime - decreaseTime) * Time.deltaTime;
                flightTimer += Time.deltaTime;
            }
            else {
                // 비행 사이클 완료.
                flightTimer = 0;
                NextFlight();
            }
        }
    }

    // NextFlight: 다음 비행이 상승인지 하강인지 판단하여 각종 정보 계산. Flight 및 Update에서 불림.
    private void NextFlight() {
        // #1. 이동 시작 및 끝점 정하기.
        Vector2 start = Vector2.zero;
        Vector2 end = Vector2.zero;
        if (flightState == 0) {
            start = startPosition;
            end = topPoint;
        }
        else if (flightState == 1) {
            start = topPoint;
            end = endPosition;
        }
        else if (flightState == 2) {
            // 비행 완료.
            flightState = 0;
            EndFlight();
            return;
        }

        // #2. 이동할 높이 계산.
        height = Mathf.Abs(start.y - end.y);

        // #3. 이동할 각도 계산.
        Vector2 direction = (end - start).normalized;
        angleR = Mathf.Atan2(direction.y, direction.x);

        // #4. 수직 속도 계산.
        velocityY = flightSpeed * Mathf.Sin(angleR);

        // #5. 가속 종료 시각, 감속 시작 시각, 비행 완료 시각 계산.
        increaseTime = Mathf.Abs((2 * increaseModifier * height) / velocityY);
        decreaseTime = Mathf.Abs((increaseModifier + decreaseModifier) * height / velocityY);
        totalTime = Mathf.Abs((increaseModifier - decreaseModifier + 2) * height / velocityY);
        
        // #6. 수평 속도 계산.
        velocityX = (end.x - this.transform.position.x) / totalTime;

        // #7. 비행 상태 변경.
        if (++flightState >= 3) flightState = 0;
    }

    // EndFlight: 비행 종료. 각종 정보 초기화.
    private void EndFlight() {
        startPosition = Vector2.zero;
        topPoint = Vector2.zero;
        endPosition = Vector2.zero;
        velocityX = velocityY = height = angleR = flightTimer = increaseTime = decreaseTime = totalTime = 0;
        cbOnEndFlight?.Invoke();
    }

    // Flight: 외부에서 불려 시작, 최고점, 끝점을 계산하고 비행 시작 명령을 내림.
    public void Flight(Vector2 start, Vector2 end) {
        // 시작점, 끝점 설정.
        this.startPosition = start;
        this.endPosition = end;
        
        float deltaDistance = end.x - start.x;          // 수평 변위.
        float deltaHeight = end.y - start.y;            // 수직 변위.
        
        // 최고점 계산.
        if (deltaHeight > 0) {
            // 올라감.
            topPoint = new Vector2(Mathf.Lerp(start.x, end.x, 0.6f), start.y + deltaHeight * 1.125f);
        }
        else if (deltaHeight == 0) {
            topPoint = new Vector2(Mathf.Lerp(start.x, end.x, 0.5f), start.y + 1.25f);
        }
        else {
            // 내려감.
            topPoint = new Vector2(Mathf.Lerp(start.x, end.x, 0.4f), end.y + deltaHeight * (-1.125f));
        }
        NextFlight();
    }
}