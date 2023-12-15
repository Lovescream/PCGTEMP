using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Info_SpeechBubble : MonoBehaviour {

    public float maxWidth = 300f;

    public Creature target;
    private DialogueBrain brain;

    public string dialogue;

    private float showTime;

    private RectTransform rectTransform;
    private Image image;
    private Text text;
    private string tweenID;

    // Callbacks.
    public Action cbOnEndTyping;
    public Action cbOnEndSpeechBubble;

    private void ClearCallbacks() {
        cbOnEndTyping = null;
        cbOnEndSpeechBubble = null;
    }

    void Awake() {
        rectTransform = this.GetComponent<RectTransform>();
        image = this.GetComponent<Image>();
        text = this.GetComponentInChildren<Text>();
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
            //this.transform.localScale = Vector3.one * size.x;
            float yOffset = size.y * 0.6f;

            this.transform.position = Camera.main.WorldToScreenPoint(target.transform.position + new Vector3(0, yOffset, 0));
            //rectTransform.sizeDelta = new Vector2(text.preferredWidth + 20, text.preferredHeight + 20);
            SetSize();
        }
    }


    public bool IsCreature(Creature creature) {
        return target == creature;
    }
    public bool IsInUse => target != null;

    public void Initialize(Creature creature, string dialogue, float time = 5, int fontSize = 24) {
        cbOnEndTyping = null;

        image.DOFade(0, 0);
        this.gameObject.SetActive(true);
        target = creature;
        brain = target.GetComponent<CreatureAI>().dialogueBrain;
        target.speechBubble = this;
        tweenID = "Tween_" + target.creatureName;
        this.dialogue = dialogue;
        image.DOFade(1, 0.1f);
        showTime = time;
        text.fontSize = fontSize;
        //DOTween.Kill(tweenID);
        text.DOText("", 0);
        text.DOText(this.dialogue, time * dialogue.Length / 100f).SetEase(Ease.Linear).SetId(tweenID).OnComplete(() => cbOnEndTyping?.Invoke());
    }

    public void Deactive() {
        image.DOFade(0, 0.1f).OnComplete(() => {

            text.text = "";
            brain.cbOnEndDialogue?.Invoke();
            cbOnEndSpeechBubble?.Invoke();
            target.speechBubble = null;
            target = null;
            this.gameObject.SetActive(false);
            ClearCallbacks();
        });
    }
    public void DeactiveImmediately() {
        image.DOFade(0, 0);
        text.text = "";
        brain.cbOnEndDialogue?.Invoke();
        cbOnEndSpeechBubble?.Invoke();
        target.speechBubble = null;
        target = null;
        this.gameObject.SetActive(false);
        ClearCallbacks();
    }

    private void SetSize() {
        rectTransform.sizeDelta = new Vector2(text.preferredWidth + 20, text.preferredHeight + 20);

        Vector2 size = rectTransform.sizeDelta;
        float x = text.preferredWidth + 20;
        x = Mathf.Clamp(x, 0, maxWidth);
        size.x = x;
        rectTransform.sizeDelta = size;
        float y = text.preferredHeight + 20;
        size.y = y;
        rectTransform.sizeDelta = size;
    }
}