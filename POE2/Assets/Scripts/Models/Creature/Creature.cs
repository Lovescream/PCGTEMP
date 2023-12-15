using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour {
    [Header("*크리쳐 기본 정보")]
    public int id;
    public string creatureName;
    public Status status;
    [Header("*크리쳐 로그 정보")]
    public List<Log> logList = new List<Log>();
    [Header("무적")]
    public bool isInvincible;           // 무적 여부.
    private bool isInvincibleOnHit;     // 피격 무적 여부.
    public float invincibleTimeOnHit;   // 맞은 후 무적시간 텀.
    private float invincibleTimerOnHit;
    [Space(20)]
    public bool isEncountered;
    // 이 크리쳐의 말풍선.
    public UI_Info_SpeechBubble speechBubble;
    // 플레이어에게 맞은 횟수.
    public int hitCountByPlayer;

    public bool IsDie { get; set; }

    // 현재 크리쳐가 있는 방.
    public Room room;

    // Initialized 여부.
    private bool isInitialized;

    // Components.
    private SpriteRenderer spriteRenderer;
    private CharacterController2D CC2D;

    // Callbacks.
    public Action<Creature> cbOnBeHit;        // 맞을 때.
    public Action<int> cbOnBeHitByPlayer;     // 플레이어에게 맞을 때.
    public Action<Creature> cbOnHitTarget;    // 타겟을 공격할 때.
    public Action<Creature> cbOnDie;
    public Action<Creature> cbOnKill;
    public Action cbOnFirstEncountered;

    #region MonoBehaviours

    void Awake() {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        CC2D = this.GetComponent<CharacterController2D>();
        

        if (id > 0) { // 이 크리쳐가 Player가 아니라면,
            //if (!isEncountered) cbOnFirstEncountered += AddEncounteredCreatureList;
            if (!isEncountered) cbOnFirstEncountered += () => CreatureManager.Instance.AddEncounterCreature(this);
        }

        CC2D.controller.cbOnGround += StopOnKnockbackOnGrounded;

        //Initialize();
    }

    void Update() {
        CountInvincibleTimer();

        if (!isEncountered) {
            if (GameCamera.instance.CheckCreatureInCamera(this)) {
                isEncountered = true;
                cbOnFirstEncountered?.Invoke();
            }
        }
    }

    #endregion

    #region Invincible

    public bool IsInvincible => isInvincible || isInvincibleOnHit;

    private void CountInvincibleTimer() {
        if (isInvincibleOnHit) {
            invincibleTimerOnHit += Time.deltaTime;
            if (invincibleTimerOnHit >= invincibleTimeOnHit) {
                SetInvincible(false);
            }
        }
    }
    
    private void SetInvincible(bool isInvincible) {
        this.isInvincibleOnHit = isInvincible;

        Color color = spriteRenderer.color;
        color.a = (isInvincible) ? 0.3f : 1f;
        spriteRenderer.color = color;

        if (!isInvincible) invincibleTimerOnHit = 0;
    }

    #endregion

    public void Initialize() {
        if (isInitialized) return;

        room = GetRoom();
    }

    // GetRoom: 해당 크리쳐가 위치하는 Room 받아오기.
    public Room GetRoom() => RoomManager.Instance.GetRoom(this.transform.position);

    // IsEnemy: 해당 크리쳐가 적인지 판단.
    public bool IsEnemy(Creature creature) {
        if (CompareTag("Enemy")) {
            if (creature.CompareTag("Player")) {
                return true;
            }
        }
        else if (CompareTag("Player")) {
            if (creature.CompareTag("Enemy")) {
                return true;
            }
        }
        return false;
    }
    // IsPlayer: 이 크리쳐가 플레이어인지 여부.
    public bool IsPlayer() => GameManager.Instance.player.creature == this;

    public void Hit(Creature hitter, float damage, Vector2 knockBackPower) {
        // #0. 조건 확인.
        // 죽으면 아무것도 하지 않는다.
        if (IsDie) return;
        // 무적 상태에서는 맞지 않는다.
        if (IsInvincible) return;

        // #1. 맞기 콜백.
        cbOnBeHit?.Invoke(hitter);
        if (hitter == GameManager.Instance.player.creature) {
            cbOnBeHitByPlayer?.Invoke(++hitCountByPlayer);
        }
        hitter.cbOnHitTarget?.Invoke(this);

        // #2. 데미지 적용.
        damage = status.DamageShield(damage);
        if (damage > 0) {
            status.HP -= damage;
        }

        // #3. 넉백 적용.
        CC2D.AddVelocity(knockBackPower * (5 - status.knockBackResist));
        

        // #4. 맞는 효과.
        if (this.GetComponent<Player>() != null) {
            GameCamera.instance.SetShakeCamera(0.1f, 0.3f);
        }
        else {
            // 빨간색 깜빡이기.
            spriteRenderer.DOColor(Color.red, 0).OnComplete(() => { spriteRenderer.DOColor(Color.white, 0).SetDelay(0.1f); });
            // 체력바 표시.
            InfoUIManager.Instance.ShowHPBar(this, 3);
            // 데미지 텍스트 표시.
            InfoUIManager.Instance.ShowDamageText(this, damage);
        }

        // #4. 죽음 검사.
        if (status.HP <= 0) {
            status.HP = 0;
            Die(hitter);
        }

        SetInvincible(true);
    }

    public void Die(Creature hitter) {
        // #0. 조건 확인.
        if (IsDie) return;
        // #1. 상태 변경.
        IsDie = true;

        // #2. 죽음 콜백.
        cbOnDie?.Invoke(hitter);
        if (hitter != null) hitter.cbOnKill?.Invoke(this);
    }

    public void Panic(Creature hitter, float duration) {
        Debug.Log("PANIC...");
    }

    //public void AddEncounteredCreatureList() => CreatureManager.Instance.AddEncounterCreature(this);

    private void StopOnKnockbackOnGrounded() {
        CC2D.SetVelocityX(0);
        CC2D.SetVelocityY(0);
    }

    public float GetCreatureSize() => this.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit / 32f;



    



    private void TEMP() {
        // Info.
        Sprite sprite = null;
        float slopeLimit = 70;
        float jumpThreshHold = 0.05f;
        LayerMask layerMask_Platform = 1 << 10;
        LayerMask layerMask_OneWayPlatform = 1 << 11;
        float gravityScale = 25;
        float airResistance = 2;
        float friction = 12;
        // ==
        int id = 0;
        string creatureName = "a";
        float hp = 0;
        float shield = 0;
        float sanity = 0;
        float damage = 0;
        float knockbackPower = 0;
        float defense = 0;
        float knockbackResist = 0;
        bool isInvincible = false;
        float invincibleTimeOnHit = 0.1f;

        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        BoxCollider2D boxCollider = this.GetComponent<BoxCollider2D>();
        Animator animator = this.GetComponent<Animator>();
        Rigidbody2D rb2d = this.GetComponent<Rigidbody2D>();
        Controller2D controller = this.GetComponent<Controller2D>();
        CharacterController2D CC2D = this.GetComponent<CharacterController2D>();
        Creature creature = this.GetComponent<Creature>();

        this.gameObject.layer = 1 << 8;
        this.gameObject.name = creatureName;
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = "Ground";
        spriteRenderer.sortingOrder = 0;
        boxCollider.isTrigger = true;
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb2d.freezeRotation = true;
        controller.slopeLimit = slopeLimit;
        controller.jumpThreshHold = jumpThreshHold;
        controller.layerMask_Platform = layerMask_Platform;
        controller.layerMask_OneWayPlatform = layerMask_OneWayPlatform;
        CC2D.gravityScale = gravityScale;
        CC2D.airResistance = airResistance;
        CC2D.friction = friction;
        creature.id = id;
        creature.creatureName = creatureName;
        creature.status = new Status() {
            maxHP = hp, HP = hp,
            maxShield = shield, shield = shield,
            maxSanity = sanity, sanity = sanity,
            damage = damage, knockBackPower = knockbackPower,
            defense = defense, knockBackResist = knockbackResist
        };
        creature.isInvincible = isInvincible;
        creature.invincibleTimeOnHit = invincibleTimeOnHit;

    }
}