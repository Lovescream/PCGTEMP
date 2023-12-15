using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGate : MonoBehaviour, IInteractiveF {
    // 이 Gate와 연결된 Gate.
    public RoomGate connectedGate;
    // 이 Gate가 열려있는지 여부.
    public bool isOpened;
    // 이 Gate가 활성화되어있는지 여부.
    public bool isActivated;
    // 이 Gate가 소속된 Room.
    public Room room;

    // Components...
    private Animator animator;

    public bool IsConnected() => connectedGate != null;

    void Awake() {
        animator = this.GetComponent<Animator>();
        isActivated = true;
    }

    // Initialize: 초기화.
    public void Initialize(Room room) {
        this.room = room;
    }

    // ConnectGate: 이 Gate를 해당 Gate와 연결.
    public void ConnectGate(RoomGate gate) {
        connectedGate = gate;
    }

    // Deactivate: 이 Gate를 비활성화. (보통 던전 생성 과정에서, 이 Gate와 연결된 Gate가 없기 때문에 쓰지 않게 되어 비활성화시킨다)
    public void Deactivate() {
        isActivated = false;
        this.gameObject.SetActive(false);
    }


    public void OpenGate() {
        animator.SetBool("IsOpen", true);
        isOpened = true;
    }
    public void CloseGate() {
        animator.SetBool("IsOpen", false);
        isOpened = false;
    }

    public bool CanActionF() {
        return isActivated && isOpened;
    }
    public void OnActionF() {
        //RoomManager.Instance.MovePlayerRoom(connectedGate.room, connectedGate);
        RoomManager.Instance.MoveCreatureRoom(GameManager.Instance.player.creature, connectedGate);
    }
}