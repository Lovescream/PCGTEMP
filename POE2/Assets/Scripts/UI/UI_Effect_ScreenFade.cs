using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Effect_ScreenFade : MonoBehaviour {
    private static UI_Effect_ScreenFade instance;
    public static UI_Effect_ScreenFade Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<UI_Effect_ScreenFade>();
            }
            return instance;
        }
        set {
            instance = value;
        }
    }

    // Components.
    private Image image;

    // Callbacks.

    void Awake() {
        image = this.GetComponent<Image>();
    }

    public void FadeOut(Action onCompleteFadeOut) {
        image.raycastTarget = true;
        image.DOFade(1, 1).OnComplete(() => onCompleteFadeOut());
    }
    public void FadeIn(Action onCompleteFadeIn) {
        image.raycastTarget = false;
        image.DOFade(0, 1).OnComplete(() => onCompleteFadeIn());
    }
}
