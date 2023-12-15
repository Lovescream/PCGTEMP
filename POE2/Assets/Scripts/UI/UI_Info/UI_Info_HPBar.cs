using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_Info_HPBar : MonoBehaviour {

    public Creature target;

    public Image barImage;

    private float showTime;

    private Image image;

    void Awake() {
        image = this.GetComponent<Image>();
    }

    void Update() {
        if (showTime > 0) {
            showTime -= Time.deltaTime;
            if (showTime <= 0) {
                Deactive();
            }
        }
    }

    void LateUpdate() {
        if (target) {
            if (target.IsDie && showTime > 0.5f) showTime = 0.5f;
            Vector2 size = target.GetComponent<BoxCollider2D>().size;
            this.transform.localScale = Vector3.one * size.x;
            float yOffset = size.y;

            this.transform.position = Camera.main.WorldToScreenPoint(target.transform.position + new Vector3(0, yOffset, 0));
            barImage.fillAmount = target.status.HPratio;
        }
    }

    public bool IsCreature(Creature creature) {
        return target == creature;
    }
    public bool IsInUse => target != null;

    public void Initialize(Creature creature, float time) {
        showTime = time;
        if (!target) {
            target = creature;
            this.gameObject.SetActive(true);
            image.DOFade(0, 0).OnComplete(() => image.DOFade(1, 0.25f));
            barImage.DOFade(0, 0).OnComplete(() => barImage.DOFade(1, 0.25f));
        }
        
    }

    public void Deactive() {
        barImage.DOFade(0, 0.25f);
        image.DOFade(0, 0.25f).OnComplete(() => {
            target = null;
            this.gameObject.SetActive(false);
        });
    }
}