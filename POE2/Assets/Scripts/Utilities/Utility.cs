using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility {

    // GetAngle; v1에서 v2로 가는 각도. x=0을 기준으로, 위로 (+).
    public static float GetAngle(Vector2 v1, Vector2 v2) {
        float angle = Vector2.Angle(v2-v1, Vector2.right);
        if (v2.y < v1.y) angle *= -1;
        return angle;
    }

}