using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Info_KeyDown : MonoBehaviour {

    public Transform target;

    public Sprite keyUpSprite;
    public Sprite keyDownSprite;

    private Vector2 offset;
    private float intervalTime;
    private bool isActivated;
    private IEnumerator ieAnimation;

    private Image image;

    void Awake() {
        image = this.GetComponent<Image>();
    }

    void Update() {

    }

    void LateUpdate() {
        if (target) {
            Vector2 position = (Vector2)target.transform.position + offset;
            this.transform.position = Camera.main.WorldToScreenPoint(position);
        }
    }

    public void Initialize(Transform target, Vector2 offset, float intervalTime) {
        this.target = target;
        this.offset = offset;
        this.intervalTime = intervalTime;
        isActivated = true;

        ieAnimation = IEAnimation();
        StartCoroutine(ieAnimation);

    }
    public void Stop() {
        isActivated = false;
        if (ieAnimation != null) StopCoroutine(ieAnimation);
        Destroy(this.gameObject);
    }
    private IEnumerator IEAnimation() {
        while (isActivated) {
            image.sprite = keyUpSprite;
            yield return new WaitForSeconds(intervalTime);
            image.sprite = keyDownSprite;
            yield return new WaitForSeconds(intervalTime);
        }
    }
}