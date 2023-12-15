using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour {
    private static RoomManager instance;
    public static RoomManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<RoomManager>();
            }
            return instance;
        }
        set {
            instance = value;
        }
    }

    // 방 하나의 최대 크기는 100x100으로 설정.
    [Header("*MaxRoomSize")]
    public int maxWidth;
    public int maxHeight;
    [Space(20)]
    

    private List<Room> roomResourceList;
    public List<Room> dungeonRoomList;
    private Queue<Room> generateQueue;

    void Awake() {
        LoadRooms();
    }



    // CreateDungeon: 해당 개수만큼의 방으로 구성된 던전을 생성한다.
    public void GenerateDungeon(int roomCount) {
        if (roomResourceList == null) LoadRooms();
        dungeonRoomList = new List<Room>();
        generateQueue = new Queue<Room>();

        // 첫 번째 방을 생성한다.
        generateQueue.Enqueue(CreateRoom(GetIDRoom(1), Vector2.zero));

        // 나머지 방을 생성한다.
        for (int abc = 0; abc < 1000; abc++) {
            // 방 개수가 최대에 도달했다면 던전 생성을 끝낸다.
            if (dungeonRoomList.Count >= roomCount) break;
            // 던전 생성이 끝나지 않았으나 생성 큐가 비어있다면, 모든 방을 다시 한 번 생성 큐에 넣어서 시도한다.
            if (generateQueue.Count == 0)
                for (int i = 0; i < dungeonRoomList.Count; i++) generateQueue.Enqueue(dungeonRoomList[i]);

            // 생성 큐에서 방을 빼온다.
            Room room = generateQueue.Dequeue();
            // 해당 방 주변에 이웃 방들을 생성한다.
            List<Room> newRoomList = CreateNeighbourRooms(room);
            // 새로 생성한 방들을 생성 큐에 넣는다.
            for (int i = 0; i < newRoomList.Count; i++) generateQueue.Enqueue(newRoomList[i]);
        }

        // 던전 생성을 마무리한다.
        FinishDungeonGenerate();
    }
    
    // FinishDungeonGenerate: 던전 생성 작업을 마무리함.
    private void FinishDungeonGenerate() {
        for (int i = 0; i < dungeonRoomList.Count; i++) dungeonRoomList[i].DeactivateGates();
    }

    #region Save / Load

    // SaveDungeon: 생성된 던전을 저장한다.
    private void SaveDungeon() {
        List<RoomSaveData> dataList = new List<RoomSaveData>();
        for(int i = 0; i < dungeonRoomList.Count; i++) {
            dataList.Add(dungeonRoomList[i].SaveData);
        }
        SaveManager.Instance.SaveData("DungeonRoomDataList", dataList);
    }

    // LoadDungeon: 저장된 던전을 로드하여 생성한다.
    private void LoadDungeon() {
        // #1. 현재 세이브 데이터에서 던전 룸 데이터 리스트를 불러온다.
        List<RoomSaveData> dataList = SaveManager.CurrentData().LoadData("DungeonRoomDataList", new List<RoomSaveData>());
        
        // #2. 불러온 리스트가 비어있는지 확인한다. (비어있다면 로드 오류)
        if (dataList.Count == 0) {
            Debug.LogError("던전을 불러오는데 실패하였습니다.");
            return;
        }

        // #3. 불러온 데이터들을 바탕으로 방을 모두 생성한다.
        for (int i=0; i < dataList.Count; i++) {
            RoomSaveData data = dataList[i];
            Room roomPrefab = GetIDRoom(data.id);
            Vector2 roomPosition = new Vector2(data.x, data.y);
            CreateRoom(roomPrefab, roomPosition);
        }

        // #4. 던전 생성을 마무리한다.
        FinishDungeonGenerate();
    }

    #endregion

    #region RoomPrefab

    // LoadRooms: Room Prefab을 불러온다.
    private void LoadRooms() {
        Room[] rooms = Resources.LoadAll<Room>("Prefabs/Rooms");

        roomResourceList = rooms.ToList();
    }

    // GetIdRoom: 해당 ID를 가진 Room Prefab을 받는다.
    private Room GetIDRoom(int id) {
        for (int i = 0; i < roomResourceList.Count; i++)
            if (roomResourceList[i].id == id)
                return roomResourceList[i];
        return null;
    }

    // GetRandomRoomPrefab: 무작위의 Room Prefab을 받는다.
    private Room GetRandomRoomPrefab() => roomResourceList[Random.Range(0, roomResourceList.Count)];

    // GetRandomConnectedRoom: direction 방향으로 Gate가 존재하는 방 중에서 무작위의 Room Prefab을 받는다.
    private Room GetRandomConnectedRoom(int direction) {
        List<Room> roomList = new List<Room>();
        for (int i = 0; i < roomResourceList.Count; i++)
            if (roomResourceList[i].IsExistGate(direction))
                roomList.Add(roomResourceList[i]);
        return roomList[Random.Range(0, roomList.Count)];
    }

    #endregion

    #region DungeonRoom

    // GetRoom: 만들어진 방 중에서 해당 Room을 받는다.
    public Room GetRoom(int index) => dungeonRoomList[index];
    private Room GetRoom(int x, int y) {
        for (int i = 0; i < dungeonRoomList.Count; i++) {
            Room room = dungeonRoomList[i];
            if (room.x == x && room.y == y) return room;
        }
        return null;
    }
    // GetRoom: 해당 위치에 있는 Room을 받는다.
    public Room GetRoom(Vector2 position) {
        for (int i = 0; i < dungeonRoomList.Count; i++) {
            if (dungeonRoomList[i].IsRoomPosition(position)) return dungeonRoomList[i];
        }
        return null;
    }
    // GetNeighbourRoom: 만들어진 방 중에서 해당 Room의 direction 방향 이웃 방을 받는다.
    private Room GetNeighbourRoom(Room room, int direction) {
        int x = room.x;
        int y = room.y;
        switch (direction) {
            case 0: y += 1; break;
            case 1: x += 1; break;
            case 2: y -= 1; break;
            case 3: x -= 1; break;
        }
        return GetRoom(x, y);
    }

    #endregion

    #region Create Room

    // CreateRoom: position에 roomPrefab을 생성.
    private Room CreateRoom(Room roomPrefab, Vector2 roomPosition) {
        // 위치 정보.
        Vector2 position = new Vector2(roomPosition.x * maxWidth, roomPosition.y * maxHeight);
        // 새 Room 생성.
        Room newRoom = Instantiate(roomPrefab, position, Quaternion.identity, this.transform);
        // 새 Room 초기화 및 설정.
        newRoom.Initialize(dungeonRoomList.Count, (int)roomPosition.x, (int)roomPosition.y);
        // 새 Room과 이웃인 방을 모두 찾아 연결시킴.
        ConnectNeighbourRooms(newRoom);

        // 던전 Room List에 추가.
        dungeonRoomList.Add(newRoom);
        return newRoom;
    }

    // CreateNeighbourRooms: 해당 room의 이웃 방향 중, 생성 가능한 곳에 25% 확률로 Room 생성.
    private List<Room> CreateNeighbourRooms(Room room) {
        List<Room> newRoomList = new List<Room>();
        for (int i = 0; i < 4; i++) {
            // 먼저, 해당 Room의 i 방향에 새 Room을 생성할 수 있는지 확인한 후,
            if (room.CanCreateRoomDirection(i)) {
                // 그 위치에 Room이 없는지 확인한 후,
                if (GetNeighbourRoom(room, i) == null) {
                    // 25% 확률로 그 위치에 새 Room을 생성.
                    if (Random.Range(0, 4) == 0) {
                        newRoomList.Add(CreateNeighbourRoom(room, i));
                    }
                }
            }
        }
        return newRoomList;
    }

    // CreateNeighbourRoom: 해당 room의 direction 방향에 랜덤 Room 생성.
    private Room CreateNeighbourRoom(Room room, int direction) {
        // 위치 정하기.
        int x = room.x;
        int y = room.y;
        switch (direction) {
            case 0: y += 1; break;
            case 1: x += 1; break;
            case 2: y -= 1; break;
            case 3: x -= 1; break;
        }
        // 방 종류 (Prefab) 정하기.
        Room roomPrefab = GetRandomConnectedRoom(GetOppositeDirection(direction));

        return CreateRoom(roomPrefab, new Vector2(x, y));
    }

    #endregion

    #region Connect Room

    // ConnectNeighbourRooms: 해당 방과 이웃인 방을 모두 찾아 연결시킴.
    private void ConnectNeighbourRooms(Room room) {
        for (int i = 0; i < 4; i++) {
            // room의 i 방향에 Gate가 있다면,
            if (room.IsExistGate(i)) {
                // i 방향의 Room을 찾는다.
                Room neighbourRoom = GetNeighbourRoom(room, i);
                // i 방향의 Room이 있다면,
                if (neighbourRoom != null) {
                    // 이웃 Room이 room으로 향하는 Gate가 있다면,
                    if (neighbourRoom.IsExistGate(GetOppositeDirection(i))) {
                        // 두 방을 연결한다.
                        ConnectRoom(room, neighbourRoom);
                    }
                }
            }
        }
    }
    // ConnectRoom: 두 방을 연결시킴.
    private bool ConnectRoom(Room room1, Room room2) {
        if (room1.x == room2.x && room1.y == room2.y) return false; // 같은 방임.
        else if (room1.x != room2.x && room1.y != room2.y) return false; // 이웃이 아님.
        else if (room1.x == room2.x) { // 상하관계.
            Room upRoom, downRoom;
            if (room1.y > room2.y) { upRoom = room1; downRoom = room2; }
            else { upRoom = room2; downRoom = room1; }
            ConnectGate(upRoom.roomGates[2], downRoom.roomGates[0]);
            return true;
        }
        else if (room1.y == room2.y) { // 좌우관계.
            Room rightRoom, leftRoom;
            if (room1.x > room2.x) { rightRoom = room1; leftRoom = room2; }
            else { rightRoom = room2; leftRoom = room1; }
            ConnectGate(rightRoom.roomGates[3], leftRoom.roomGates[1]);
            return true;
        }
        return false;
    }
    // CoonectGate: 두 게이트를 연결시킴.
    private void ConnectGate(RoomGate gate1, RoomGate gate2) {
        if (gate1.IsConnected()) Debug.LogError(gate1.room.name + "방의 " + gate1.name + "은 이미 연결된 Gate가 있음에도 불구하고, 또 연결 요청이 들어옴.");
        gate1.ConnectGate(gate2);
        if (gate2.IsConnected()) Debug.LogError(gate2.room.name + "방의 " + gate2.name + "은 이미 연결된 Gate가 있음에도 불구하고, 또 연결 요청이 들어옴.");
        gate2.ConnectGate(gate1);
    }

    #endregion

    // GetOppositeDirection: 반대 방향의 방향 인덱스를 받는다. 0 <-> 2, 1 <-> 3.
    private int GetOppositeDirection(int direction) {
        switch (direction) {
            case 0: return 2;
            case 1: return 3;
            case 2: return 0;
            case 3: return 1;
            default: return -1;
        }
    }

    //// 안씀 TODO
    //public void MovePlayerRoom(Room room, RoomGate gate) {
    //    Player player = GameManager.Instance.player;

    //    // #1. 플레이어 방 이동.
    //    player.room = room;

    //    // #2. 플레이어 위치 이동.
    //    player.transform.position = gate.transform.position;

    //    // #3. 카메라 방 설정.
    //    GameCamera.instance.SetRoom(room);
    //}

    public void MoveCreatureRoom(Creature creature, RoomGate gate) {
        // #1. 크리쳐 방 이동.
        creature.room = gate.room;

        // #2. 크리쳐 위치 이동.
        creature.transform.position = gate.transform.position;

        // #3. 플레이이라면, 카메라 방 설정.
        if (creature.IsPlayer()) GameCamera.instance.SetRoom(gate.room);
    }
}