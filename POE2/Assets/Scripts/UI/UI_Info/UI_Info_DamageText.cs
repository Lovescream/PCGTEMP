using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_Info_DamageText : MonoBehaviour {

    public float duration;
    public float moveOffset;

    private float timer;

    private Text text;

    void Awake() {
        text = this.GetComponent<Text>();

        timer = duration;

        this.transform.DOMoveY(moveOffset, duration).SetRelative();
        text.DOFade(0, duration);
    }

    void Update() {
        if (timer > 0) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                Destroy(this.gameObject);
            }
        }
    }

    public void SetText(string text) => this.text.text = text;
}