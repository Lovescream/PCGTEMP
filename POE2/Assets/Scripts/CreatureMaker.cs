using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreatureMaker : MonoBehaviour {
    [Header("크리쳐 기본 프리팹")]
    public GameObject creaturePrefab;
    [Header("크리쳐 기본정보")]
    public int id;
    public string creatureName;
    [TextArea]
    public string description;
    public Sprite sprite;
    public Sprite portrait;
    [Header("크리쳐 능력치")]
    public Status status;
    [Header("컨트롤러 정보")]
    public float slopeLimit = 70;
    public float jumpThreshHold = 0.05f;
    [Header("레이어 정보")]
    public LayerMask layerMask_Platform = 1 << 10;
    public LayerMask layerMask_OneWayPlatform = 1 << 11;
    [Header("CC2D 정보")]
    public float gravityScale = 25;
    public float airResistance = 2;
    public float friction = 12;
    [Header("무적 정보")]
    public bool isInvincible = false;
    public float invincibleTimeOnHit = 0.1f;

    [Button]
    public void SaveCreature() {
#if UNITY_EDITOR
        string prefabName = "Assets/Resources/Prefabs/Creatures/" + creatureName + ".prefab";
        string dataName = "Assets/Resources/Prefabs/Creatures/Data/" + id + "." + creatureName + ".prefab";
        
        CreatureData data = ScriptableObject.CreateInstance<CreatureData>();
        data.id = id;
        data.creatureName = creatureName;
        data.description = description;
        data.sprite = sprite;
        data.portrait = portrait;
        data.status = status;

        GameObject obj = Instantiate(creaturePrefab);
        obj.gameObject.layer = 8;
        obj.gameObject.name = creatureName;
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = "Ground";
        spriteRenderer.sortingOrder = 0;
        BoxCollider2D boxCollider = obj.GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        Animator animator = obj.GetComponent<Animator>();
        Rigidbody2D rb2d = obj.GetComponent<Rigidbody2D>();
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb2d.freezeRotation = true;
        Controller2D controller = obj.GetComponent<Controller2D>();
        controller.slopeLimit = slopeLimit;
        controller.jumpThreshHold = jumpThreshHold;
        controller.layerMask_Platform = layerMask_Platform;
        controller.layerMask_OneWayPlatform = layerMask_OneWayPlatform;
        CharacterController2D CC2D = obj.GetComponent<CharacterController2D>();
        CC2D.gravityScale = gravityScale;
        CC2D.airResistance = airResistance;
        CC2D.friction = friction;
        Creature creature = obj.GetComponent<Creature>();
        creature.id = id;
        creature.creatureName = creatureName;
        creature.status = status;
        creature.isInvincible = isInvincible;
        creature.invincibleTimeOnHit = invincibleTimeOnHit;

        data.objectPrefab = obj;

        //PrefabUtility.SaveAsPrefabAsset(data, dataName);
        PrefabUtility.SaveAsPrefabAsset(obj, prefabName);
#endif
    }
}