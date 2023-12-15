using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CreatureAIState {
    NONE = -1,
    IDLE = 0,
    COGNITION = 1,
    Count,
}
public class CreatureAI : MonoBehaviour {
    [Header("대화 여부")]
    public bool canTalk;
    [Range(2,10)]
    public float talkIntervalTime;
    [Header("적 탐색 시야")]
    public float sight = 5f;

    // 탐색할 적 레이어.
    public LayerMask layerMask;
    // 패스파인딩 정보.
    private LayerMask wallLayer;
    private PathFinder pathFinder;

    // Components.
    public Creature creature;
    protected CharacterController2D CC2D;
    protected Animator animator;
    public DialogueBrain dialogueBrain;
    public TargetingBrain targetingBrain;

    #region MonoBehaviours

    protected virtual void Awake() {
        creature = this.GetComponent<Creature>();
        CC2D = this.GetComponent<CharacterController2D>();
        animator = this.GetComponent<Animator>();

        creature.cbOnDie += OnDie;
    }

    protected virtual void Update() {
        if (creature.IsDie) return;

        if (!targetingBrain.IsTargeting) OnIdle();
        else OnCognition();
    }

    #endregion

    protected void Move(Vector2 velocity) {
        CC2D.Move(ref velocity);
    }

    #region Callback

    protected virtual void OnIdle() { }
    protected virtual void OnCognition() { }
    protected virtual void OnDie(Creature hitter) {
        animator.Play("Die");
    }

    #endregion

    public virtual void Initialize() {
        // 타겟팅 능력 장착.
        targetingBrain = new TargetingBrain(creature, sight, layerMask);

        // 언어 능력 장착.
        if (canTalk) dialogueBrain = new DialogueBrain(creature, talkIntervalTime);

        // 로그 정보 초기화. TODO:: 이거 다른곳에서 하자...
        this.GetComponent<CreatureLog>()?.Initialize();

        // 길 찾기 정보 초기화.
        wallLayer = 1 << 10;
    }
    
    public AIPath GetPath(int startX, int startY, int endX, int endY) {
        //Debug.Log("[CreatureAI.GetPath]");
        return new PathFinder(creature.room, creature.GetCreatureSize() + 0.5f, wallLayer).GetPath(startX, startY, endX, endY);
    }
}