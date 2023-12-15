using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    private static GameManager instance;
    public static GameManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
        set {
            instance = value;
        }
    }
    [Header("임시: 생성할 위치")]
    public Vector2 mrScarecrowPosition;
    [Header("임시: 생성할 방 개수")]
    public int maxRoomCount;
    [Header("Prefabs")]
    public GameObject playerPrefab;

    [Space(20)]

    public Player player;

    public bool isOver;

    void Awake() {
        //DontDestroyOnLoad(this);
    }
    
    void Start() {
        // 던전 생성.
        RoomManager.Instance.GenerateDungeon(maxRoomCount);
        // 첫 번째 방에 플레이어를 생성.
        CreatePlayer(RoomManager.Instance.GetRoom(0));
        // 현재 생성된 모든 CreatureAI에 대해 Initialize.
        CreatureManager.Instance.AllCurrentCreatureInitialize();
        CreatureManager.Instance.AllCurrentCreatureAIInitialize();
        //CreateMrScarecrow();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Item item = ItemManager.Instance.GetItem(2);
            ItemManager.Instance.GivePlayerItem(item);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            player.inventory.SetSlotSize(player.defaultInventorySize);
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            for(int i = 0; i < RoomManager.Instance.dungeonRoomList.Count; i++) {
                RoomManager.Instance.dungeonRoomList[i].OpenAllGate();
            }
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            for (int i = 0; i < RoomManager.Instance.dungeonRoomList.Count; i++) {
                RoomManager.Instance.dungeonRoomList[i].CloseAllGate();
            }
        }
        if (Input.GetMouseButton(0)) {
            //Vector2 origin = Vector2.Lerp(min, max, 0.5f);
            //Vector2 direction = new Vector2(-1, -1);
            //float distance = 0.01f;
            //RaycastHit2D hit1 = Physics2D.Raycast(origin, direction, distance, tempLayerMask);
            //int tryCount = 0;
            //while (!hit1) {
            //    distance += 0.01f;
            //    hit1 = Physics2D.Raycast(origin, direction, distance, tempLayerMask);
            //    if (++tryCount >= 1000) break;
            //}
            //Debug.DrawRay(origin, distance * direction, Color.red);
            //UnityEditor.EditorApplication.isPaused = true;
            //Debug.Log("ASFASFASF");
            Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 rayDirection = Vector2.down;
            float rayDistance = 0.25f;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, 1 << 10);
            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
            if (hit) {
                //Debug.Log(hit.transform.name);
                UnityEditor.EditorApplication.isPaused = true;
            }
        }
    }

    private void CreatePlayer(Room room) {
        // #1. Player 생성할 위치 정하기: 해당 Room의 무작위 Gate 위치에 생성.
        Vector2 createPosition = room.GetRandomGate().transform.position;

        // #2. Player 생성.
        player = Instantiate(playerPrefab, createPosition, Quaternion.identity).GetComponent<Player>();
        player.transform.name = "Player";

        // #3. Camera 설정.
        GameCamera.instance.target = player.transform;
        GameCamera.instance.SetRoom(room);

        // #4. Player 방 설정.
        //player.room = room;
        player.creature.room = room;

        // #5. 플레이어 정보 로드 및 초기화..
        player.Initialize();
    }

    private void CreateMrScarecrow() {
        Creature newMrScarecrow = CreatureManager.Instance.CreateCreature2(1001);
        Transform mrScareCrow = newMrScarecrow.transform;

        mrScareCrow.transform.SetParent(this.transform);
        mrScareCrow.position = mrScarecrowPosition;
    }

    // GameOver: HP가 0이 되거나 하면 호출됨.
    public void GameOver(Creature hitter) {
        // #1. Player 죽음 처리.

        // #2. UI 및 화면 전환.
        SceneManager.LoadScene("MainWorld");

        isOver = true;
    }

}