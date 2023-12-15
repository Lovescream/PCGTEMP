using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_StatusBar : MonoBehaviour {

    public enum Type { health, sanity, }
    [Header("*정보 타입")]
    public Type type;
    [Header("*스프라이트 목록")]
    public Sprite[] sprites;

    private Player player;
    private Status status;
    private Image image;
    private Text text;

    void Awake() {
        image = this.GetComponent<Image>();
        text = this.GetComponentInChildren<Text>();

        Initialize();
    }

    void Update() {
        if (player == null) Initialize();
        UpdateUI();
    }

    private void Initialize() {
        // #1. 플레이어 및 Status 연결.
        player = GameManager.Instance.player;
        if (player == null) return;
        status = player.creature.status;
    }

    private void UpdateUI() {
        if (type == Type.health) {
            SetAmount(status.HP, status.maxHP);
        }
        else if (type == Type.sanity) {
            SetAmount(status.sanity, status.maxSanity);
        }
    }

    private void SetAmount(float current, float max) {
        if (max == 0) return;
        float ratio = Mathf.Clamp((current / max), 0, 1);
        int spriteIndex = (int)((sprites.Length - 1) * ratio);
        image.sprite = sprites[sprites.Length - 1 - spriteIndex];

        text.text = ((int)current).ToString() + "/" + ((int)max).ToString();
    }

    
}