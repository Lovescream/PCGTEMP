using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour {

    public static GameCamera instance;

    public Transform target;
    public float softCorrection;

    private float cameraWidth;
    private float cameraHeight;
    private float minX, maxX, minY, maxY;

    private Camera mainCamera;

    void Awake() {
        if (!instance) instance = this;

        mainCamera = Camera.main;

        SetCameraSizeFromViewport();
    }

    void Start() {

    }

    void LateUpdate() {
        if (target) FollowTarget(softCorrection);
        ShakeCamera();
    }

    private void SetCameraSizeFromViewport() {
        Vector2 min = mainCamera.ViewportToWorldPoint(Vector2.zero);
        Vector2 max = mainCamera.ViewportToWorldPoint(Vector2.one);
        cameraWidth = max.x - min.x;
        cameraHeight = max.y - min.y;
    }

    private void FollowTarget(float softCorrection = 0) {
        if (GameManager.Instance.isOver) return;

        Vector3 position = target.position;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        position.z = -10;
        
        if (mainCamera != null) mainCamera.transform.DOMove(position, softCorrection);
    }

    #region Room

    public void SetRoom(Room room) {
        minX = room.min.x + cameraWidth / 2;
        maxX = room.max.x - cameraWidth / 2;
        minY = room.min.y + cameraHeight / 2;
        maxY = room.max.y - cameraHeight / 2;
    }

    #endregion

    #region Camera Shake

    private float shakeTimer;
    private float shakeStrength;
    private void ShakeCamera() {
        if (shakeTimer > 0) {
            // 타이머 카운트.
            shakeTimer -= Time.deltaTime;
            // 화면 흔들기.
            this.transform.position += Random.insideUnitSphere * shakeStrength;
        }
    }
    public void SetShakeCamera(float duration = 1, float strength = 3) {
        shakeTimer = duration;
        shakeStrength = strength;
    }

    #endregion

    #region Check Object InCamera

    public bool CheckCreatureInCamera(Creature creature) {
        Vector2 worldPosition = creature.transform.position;
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(worldPosition);

        return (0 <= viewportPosition.x && viewportPosition.x <= 1 && 0 <= viewportPosition.y && viewportPosition.y <= 1);
    }

    #endregion
}