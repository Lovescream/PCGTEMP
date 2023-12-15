using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [Header("*정신력 감소 속도")]
    public float sanityWaste;

    [Header("*Hands")]
    public CharacterHand[] hands_set = new CharacterHand[4];
    [Header("*BasicMainHandIndex")]
    public int basicMainHandIndex;
    [Header("*BasicSubHandIndex")]
    public int basicSubHandIndex;
    [Header("*상호작용 범위")]
    public float interactionRange;
    [Header("*시작 인벤토리 크기")]
    public int defaultInventorySize;
    [Header("*상호작용할 레이어")]
    public LayerMask layerMask_Interactive;
    [Space(20)]

    // 인벤토리 정보.
    public int inventorySize; // 현재 인벤토리 크기.
    public Inventory inventory; // 인벤토리.
    public InventorySlot[] weaponSlot_set;

    // 현재 세트. 1 또는 2.
    private int currentSet;

    // 현재 상호작용 타겟.
    public Transform interactionTarget;
    private UI_Info_KeyDown keyDownUI;

    // 현재 Player가 있는 Room.
    //public Room room;

    // Callbacks.
    public Action<Item> cbOnEquipItem;

    // Components.
    public Creature creature;

    void Awake() {
        creature = this.GetComponent<Creature>();

        for (int i = 0; i < hands_set.Length; i++) hands_set[i].SetOwner(creature);

        currentSet = 1;
        weaponSlot_set = new InventorySlot[4];
        for (int i = 0; i < weaponSlot_set.Length; i++) weaponSlot_set[i] = new InventorySlot(i);
    }

    void Update() {
        Interaction();

        // 정신력 소모. 0이 되면 죽는다.
        if (creature.status.DecreaseSanity(sanityWaste)) {
            creature.Die(null);
        }
    }

    public void Initialize() {
        // 콜백 함수 등록.
        creature.cbOnDie += GameManager.Instance.GameOver;

        // 데이터 로드.
        LoadSaveData();
    }

    private void LoadSaveData() {
        // Player Name.
        creature.creatureName = SaveManager.CurrentData().LoadData("PlayerName", "Player");
        // Inventory.
        inventorySize = SaveManager.CurrentData().LoadData("InventorySize", defaultInventorySize);
        inventory = new Inventory();
        inventory.Initialize(inventorySize);
        // HandSet.
        currentSet = SaveManager.CurrentData().LoadData("CurrentHandSet", 1);
        // Tutorial
        if (SaveManager.CurrentData().LoadData("MrScareCrow_IsClearAllTutorial", false)) {
            GameManager.Instance.player.GetComponent<CharacterController2D>().availableDash = true;
            UIManager.Instance.availableInventoryPanel = true;
            UIManager.Instance.availableCreaturesPanel = true;
        }
    }

    #region Room



    #endregion

    #region Interactive

    // FindInteractiveInCircle: 상호작용 범위 안의 상호작용 가능한 오브젝트를 찾는다.
    private void FindInteractiveInCircle() {
        // #1. Raycast 정보 설정.
        Vector2 rayOrigin = this.transform.position;
        float rayRadius = interactionRange;

        // #2. Raycast 수행.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(rayOrigin, rayRadius, Vector2.zero, 0, layerMask_Interactive);
        // #3. Target 설정.
        if (hits.Length != 0) {
            Transform target = null;
            for (int i = 0; i < hits.Length; i++) {
                // 자기 자신 제외.
                if (hits[i].transform.gameObject == this.gameObject) continue;
                // 맞은 개체가 상호작용이 가능하다면,
                if (hits[i].transform.GetComponent<IInteractiveF>() != null) {
                    // 기존 Target이 없다면 이 오브젝트를 Target으로 설정.
                    if (interactionTarget == null) target = hits[i].transform;
                    // 기존 Target이 있다면 둘 중 가까운 오브젝트를 Target으로 설정.
                    else {
                        float distanceToNewHitCreature = (hits[i].transform.position - this.transform.position).magnitude;
                        float distanceToTargetCreature = (interactionTarget.transform.position - this.transform.position).magnitude;
                        if (distanceToNewHitCreature < distanceToTargetCreature) target = hits[i].transform;
                        else target = interactionTarget;
                    }
                }
            }
            SetInteractionTarget(target);
        }
    }
    // SetInteractionTarget: 상호작용 타겟을 설정한다. KeyDownUI를 표시한다.
    private void SetInteractionTarget(Transform target) {
        // #0. 새 Target이 기존 Target과 같다면 아무것도 하지 않음.
        if (interactionTarget != null && interactionTarget == target) return;

        // #1. 기존에 다른 Target과 상호작용 중이었다면,
        if (interactionTarget != null) {
            // keyDownUI를 표시중이라면 지운다.
            if (keyDownUI != null) {
                keyDownUI.Stop();
                keyDownUI = null;
            }
        }

        // #2. target이 null 이면 Target 해제
        if (target == null) { interactionTarget = null; return; }

        // #3. 새 Target의 종류를 확인.
        InteractiveCreatureAI ai = target.GetComponent<InteractiveCreatureAI>();
        RoomGate gate = target.GetComponent<RoomGate>();

        // #4. 새 Target의 종류에 따라 상호작용한다.
        if (ai != null) {
            if (ai.CanActionF()) {
                ShowKeyDownUI(target, new Vector2(0, 1.5f), 0.5f);
            }
        }
        if (gate != null) {
            if (gate.isOpened) {
                ShowKeyDownUI(target, new Vector2(0, 1.5f), 0.5f);
            }
        }


        // #5. Target 설정.
        interactionTarget = target;
    }

    // ShowKeyDownUI: 해당 Target에게 KeyDownUI를 표시한다.
    private void ShowKeyDownUI(Transform target, Vector2 offset, float intervalTime) {
        if (keyDownUI == null) {
            keyDownUI = InfoUIManager.Instance.ShowKeyDownFAnimation(target, offset, intervalTime);
        }
    }


    // (Update에서 부름)
    // Interaction: 상호작용한다.
    private void Interaction() {
        FindInteractiveInCircle();
        if (interactionTarget == null) return;

        IInteractiveF interaction = interactionTarget.GetComponent<IInteractiveF>();
        if (interaction.CanActionF()) {
            if (Input.GetKeyDown(KeyCode.F)) interaction.OnActionF();
        }
        else {
            if (keyDownUI != null) {
                keyDownUI.Stop();
                keyDownUI = null;
            }
        }
    }

    #endregion

    #region HandSet

    public int AnotherSet => (currentSet == 1) ? 2 : 1;
    public void ChangeSet() {
        if (currentSet == 1) currentSet = 2;
        else currentSet = 1;
    }
    public int GetHandIndex(int setIndex, bool isSubHand = false) => (setIndex - 1) * 2 + ((isSubHand) ? 1 : 0);
    public CharacterHand CurrentMainHand => hands_set[GetHandIndex(currentSet)];
    public CharacterHand CurrentSubHand => hands_set[GetHandIndex(currentSet) + 1];
    public CharacterHand AnotherMainHand => hands_set[GetHandIndex(AnotherSet)];
    public CharacterHand AnotherSubHand => hands_set[GetHandIndex(AnotherSet) + 1];
    #endregion

    #region WeaponSlot

    #endregion

    #region Equip

    // EquipItem: 해당 인벤토리 슬롯 아이템 장착.
    public void EquipItem(InventorySlot itemSlot) {
        Item item = itemSlot.item;
        ItemData data = ItemManager.Instance.GetItemData(item.id);

        if (data.Type == ItemType.Weapon) {
            WeaponData weaponData = data as WeaponData;
            // 아이템을 장착할 무기 슬롯 번호 정하기.
            int handIndex = -1;
            if (weaponData.handType == HandType.Main || weaponData.handType == HandType.TwoHanded) {
                handIndex = GetHandIndex(currentSet);
                if (!weaponSlot_set[handIndex].IsEmpty) {
                    handIndex = GetHandIndex(AnotherSet);
                    if (!weaponSlot_set[handIndex].IsEmpty)
                        handIndex = GetHandIndex(currentSet);
                }
            }
            else if (weaponData.handType == HandType.Sub) {
                handIndex = GetHandIndex(currentSet, true);
                if (!weaponSlot_set[handIndex].IsEmpty) {
                    handIndex = GetHandIndex(AnotherSet, true);
                    if (!weaponSlot_set[handIndex].IsEmpty)
                        handIndex = GetHandIndex(currentSet, true);
                }
            }
            if (handIndex == -1) Debug.LogError("[Player] EquipItem: 잘못된 무기 슬롯!");

            // 양손무기를 장착하는데 주무기, 보조무기가 둘 다 있다면 둘 다 비워야 함.
            // 인벤토리 공간에 한 개 이상의 빈 공간이 더 필요한데, 그러지 못하면 장착 실패.
            int anotherEmptyInventorySlotIndex = -1;
            if (weaponData.handType == HandType.TwoHanded && !weaponSlot_set[handIndex].IsEmpty && !weaponSlot_set[handIndex + 1].IsEmpty) {
                InventorySlot _slot = inventory.GetEmptySlot();
                if (_slot == null) {
                    Debug.LogError("[Player] EquipItem: 인벤토리에 빈 공간이 없음!");
                    return;
                }
                anotherEmptyInventorySlotIndex = _slot.index;
            }

            // 슬롯 교환.
            InventorySlot tempSlot = new InventorySlot(itemSlot);
            inventory.GetInventorySlot(itemSlot.index).UpdateSlot(weaponSlot_set[handIndex]);
            weaponSlot_set[handIndex].UpdateSlot(tempSlot);
            if (weaponData.handType == HandType.TwoHanded) {
                inventory.GetInventorySlot(anotherEmptyInventorySlotIndex).UpdateSlot(weaponSlot_set[handIndex + 1]);
                weaponSlot_set[handIndex + 1].SetEmpty();
            }

            // 실제 무기 장착.
            EquipWeapon(handIndex, weaponData);

            // Hand 활성화.
            hands_set[handIndex].Initialize(weaponData);

            // UI 업데이트.
            UIManager.Instance.inventoryPanel.UpdatePanel();

            // 
            UpdateActivatedWeapon();
        }

        // Callback.
        cbOnEquipItem?.Invoke(item);
    }

    // UnequipItem: 해당 슬롯의 아이템을 장비 해제한다.
    public void UnequipItem(InventorySlot itemSlot) {
        ItemData data = ItemManager.Instance.GetItemData(itemSlot.item.id);

        if (data.Type == ItemType.Weapon) {
            // 아이템을 들고 있는 슬롯.
            int handIndex = -1;
            for(int i = 0; i < weaponSlot_set.Length; i++) {
                if (weaponSlot_set[i] == itemSlot) {
                    handIndex = i;
                    break;
                }
            }
            if (handIndex == -1) Debug.LogError("[Player] EquipItem: 잘못된 무기 슬롯!");

            // 아이템을 놓을 빈 슬롯 구하기.
            InventorySlot emptySlot = inventory.GetEmptySlot();
            if (emptySlot == null) {
                Debug.LogError("[Player] UnequipItem: 빈 슬롯이 없음!");
                return;
            }

            // 슬롯 교환.
            Debug.Log(itemSlot.UI.name + "에 있던 " + itemSlot.item.itemName + "을 " + emptySlot.UI.name + "에 넣을 것임.");
            emptySlot.UpdateSlot(itemSlot);
            Debug.Log("그 결과, " + emptySlot.UI.name + "에는 " + emptySlot.item.itemName + "이 들어가게 되었음.");
            itemSlot.SetEmpty();

            // 실제 무기 해제. (기본 무기 장착)
            EquipBasicWeapon(handIndex);

            // UI 업데이트.
            UIManager.Instance.inventoryPanel.UpdatePanel();

            UpdateActivatedWeapon();
        }

    }

    // EquipBasicWeapon: 기본 무기로 장착시킨다.
    private void EquipBasicWeapon(int handIndex) {
        int weaponIndex = (handIndex % 2 == 0) ? basicMainHandIndex : basicSubHandIndex;
        WeaponData data = ItemManager.Instance.GetItemData(weaponIndex) as WeaponData;
        EquipWeapon(handIndex, data).GetComponent<CharacterHand>().Initialize(data);
        UpdateActivatedWeapon();
    }
    
    // EquipWeapon: 새 Weapon Object를 생성하고, 기존의 Hand를 새 Hand로 교체한다.
    private GameObject EquipWeapon(int handIndex, WeaponData data) {
        // weaponPrefab 생성.
        GameObject weaponPrefab = data.objectPrefab;
        GameObject weaponObject = Instantiate(weaponPrefab, this.transform.position, weaponPrefab.transform.rotation);
        weaponObject.transform.SetParent(this.transform);
        weaponObject.transform.localPosition = weaponPrefab.transform.localPosition;
        weaponObject.transform.localScale = Vector3.one;
        weaponObject.transform.localRotation = Quaternion.Euler(0, 0, weaponPrefab.transform.eulerAngles.z);

        // Hand 교체.
        CharacterHand newHand = weaponObject.GetComponent<CharacterHand>();
        if (data.handType == HandType.Main) {
            DestroyHand(hands_set[handIndex]);
            ChangeHand(handIndex, newHand);
        }
        else if (data.handType == HandType.Sub) {
            DestroyHand(hands_set[handIndex]);
            ChangeHand(handIndex, newHand);
        }
        else if (data.handType == HandType.TwoHanded) {
            DestroyHand(hands_set[handIndex]);
            DestroyHand(hands_set[handIndex + 1]);
            ChangeHand(handIndex, newHand);
        }

        return weaponObject;
    }

    #endregion

    #region Hand

    // UnEquipHand: 해당 손을 비활성화하고 파기한다.
    private void DestroyHand(CharacterHand hand) {
        hand.Inactivate();
        hand.OnUnequip();
        Destroy(hand.gameObject);
    }
    // ChangeHand: 해당 손을 교체한다.
    private void ChangeHand(int handIndex, CharacterHand newHand) {
        hands_set[handIndex] = newHand;
        hands_set[handIndex].SetOwner(creature);
        hands_set[handIndex].Inactivate();
    }

    #endregion
    // SwapWeapon:
    public void SwapWeapon() {
        ChangeSet();
        UpdateActivatedWeapon();
    }

    private void UpdateActivatedWeapon() {
        int handIndex = GetHandIndex(AnotherSet);
        hands_set[handIndex].Inactivate();
        hands_set[handIndex + 1].Inactivate();

        handIndex = GetHandIndex(currentSet);
        hands_set[handIndex].Activate();
        hands_set[handIndex + 1].Activate();
    }
}