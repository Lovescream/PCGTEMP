using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer {

    private bool autoReset;
    private float currentTime;  // 현재 시간.
    private float targetTime;   // 목표 시간.

    private Timer() { }
    public Timer(float targetTime, bool autoReset = true) {
        SetTimer(targetTime);
        SetAutoReset(autoReset);
    }

    // SetTimer: 목표 시간을 설정하고 리셋.
    public void SetTimer(float targetTime) {
        this.targetTime = targetTime;
        Reset();
    }

    // Reset: 현재 시간을 0으로.
    public void Reset() {
        currentTime = 0;
    }

    // SetCurrentToTarget: 현재 시간을 목표 시간으로 즉시 설정.
    public void SetCurrentToTarget() => currentTime = targetTime;

    public void SetAutoReset(bool autoReset) => this.autoReset = autoReset;

    // Counting: 업데이트에서 부른다.
    // 타이머를 한 프레임 작동시킨 후, 목표 시간이 되었는지 여부를 리턴한다.
    // autoReset = true일 경우, 목표 시간이 되면 자동으로 초기화한다.
    public bool Counting(float deltaTime) {
        if (currentTime < targetTime) {
            currentTime += Time.deltaTime;
            return false;
        }
        else {
            if (autoReset) currentTime = 0;
            return true;
        }
    }
}