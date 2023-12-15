using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Room : MonoBehaviour {

    private RoomSaveData saveData;
    public RoomSaveData SaveData {
        get {
            if (saveData == null) saveData = new RoomSaveData(id, x, y);
            return saveData;
        }
    }

    [Header("*방 ID")]
    public int id;
    [Header("방 번호")]
    public int index;
    public int x;
    public int y;
    [Header("*입구")]
    public RoomGate[] roomGates = new RoomGate[4]; // ↑→↓←
    [Space(20)]
    // 폭, 높이.
    public int width;
    public int height;
    // 경계 좌표.
    public Vector2 min;
    public Vector2 max;


    public PathTileList pathTileList;

    public Tilemap backgroundTilemap;
    public TilemapGroup tilemap;
    public Bounds roomBound;

    #region MonoBehaviours

    void Awake() {
        tilemap = this.GetComponent<TilemapGroup>();
        SetRoomBound();
    }

    void Start() {
        //DeactivateGates();
        OpenAllGate();
    }

    void Update() {

    }

    #endregion

    // Initialize: 초기화.
    public void Initialize(int index, int x, int y) {
        // 변수 설정.
        this.index = index;
        this.x = x;
        this.y = y;
        // 하이어라키 이름 설정.
        this.transform.name = "Room[" + index + "] (" + x + ", " + y + ")";
        // 모든 Gate 초기화.
        for (int i = 0; i < roomGates.Length; i++) if (IsExistGate(i)) roomGates[i].Initialize(this);

        TileListInitialize();
    }
    // SetRoomBound: 방 경계 정보 설정.
    private void SetRoomBound() {
        roomBound = tilemap[0].MapBounds;
        //roomBound = backgroundTilemap.cellBounds;

        width = (int)roomBound.size.x;
        height = (int)roomBound.size.y;

        min = this.transform.position;
        max = this.transform.position + roomBound.size;
    }
    //IsRoomPosition: 해당 위치가 이 방인지 아닌지 검사.
    public bool IsRoomPosition(Vector2 position) => (min.x < position.x && position.x < max.x) && (min.y < position.y && position.y < max.y);

    #region Gate

    // IsExistGate: 해당 방향으로 Gate가 존재하는지 여부.
    public bool IsExistGate(int direction) => roomGates[direction] != null;
    // IsEmptyGate: 해당 방향의 Gate가 비어있는지 여부. (= 연결되어있지 않음.) (= 이 방향으로 새 Room을 생성할 수 있음.)
    public bool IsEmptyGate(int direction) => !roomGates[direction].IsConnected();
    // CanCreateRoomDirection: 해당 방향으로 새 Room을 생성할 수 있는지 여부.
    public bool CanCreateRoomDirection(int direction) => IsExistGate(direction) && IsEmptyGate(direction);
    // GetRandomGate: 게이트 아무거나 받기.
    public RoomGate GetRandomGate() {
        RoomGate gate = null;
        for (int i = 0; i < 1000; i++) {
            gate = roomGates[Random.Range(0, roomGates.Length)];
            if (gate != null) break;
        }
        return gate;
    }
    // DeactiveGates: 연결되지 않은 모든 게이트 비활성화.
    public void DeactivateGates() {
        for (int i = 0; i < roomGates.Length; i++) {
            if (IsExistGate(i)) if (!roomGates[i].IsConnected()) roomGates[i].Deactivate();
        }
    }
    // OpenAllGate: 모든 게이트 열기.
    public void OpenAllGate() {
        for (int i = 0; i < roomGates.Length; i++) {
            if (roomGates[i] != null) roomGates[i].OpenGate();
        }
    }
    // CloseAllGate: 모든 게이트 닫기.
    public void CloseAllGate() {
        for (int i = 0; i < roomGates.Length; i++) {
            if (roomGates[i] != null) roomGates[i].CloseGate();
        }
    }

    #endregion

    // GetRandomOnGroundTile: 무작위의 OnGround 타일을 찾는다.
    public PathTile GetRandomOnGroundTile() {
        List<PathTile> tileList = pathTileList.GetOnGroundTileList();
        //Debug.Log("[OnGroundTile목록]");
        for (int i = 0; i < tileList.Count; i++) {
            //Debug.Log(tileList[i].x + ", " + tileList[i].y);
        }
        int index = Random.Range(0, tileList.Count);
        return tileList[index];
    }
    // GetRandomNotWallTile: 무작위의 벽이 아닌 타일을 찾는다.
    public PathTile GetRandomNotWallTile() {
        List<PathTile> tileList = pathTileList.GetNotWallTileList();
        int index = Random.Range(0, tileList.Count);
        return tileList[index];
    }

    // GetNearestOnGround: 해당 타일과 가장 가까운 OnGround를 찾는다. isBelow = true 이면 해당 타일보다 위에 있는 타일은 제외한다.
    public PathTile GetNearestOnGround(PathTile tile, bool isBelow = false) {
        List<PathTile> onGroundList = pathTileList.GetOnGroundTileList();

        PathTile nearestOnGroundTile = onGroundList[0];
        int nearestDistance = 100000;

        for (int i = 0; i < onGroundList.Count; i++) {
            if (isBelow && onGroundList[i].y > tile.y) continue;

            int distance = onGroundList[i].GetDistance(tile);
            if (distance < nearestDistance) {
                nearestOnGroundTile = onGroundList[i];
                nearestDistance = distance;
            }
        }

        return nearestOnGroundTile;
    }
    private void TileListInitialize() {
        LayerMask platformLayerMask = 1 << 10;
        LayerMask onewayLayerMask = 1 << 11;
        // DEBUG.
        //int tryCount = 0;
        //float gan = 100f;
        //while (tryCount < gan*gan) {
        //    float num = tryCount / gan;
        //    int yIndex = (int)num;

        //    float x = num - yIndex;
        //    float y = yIndex / gan;

        //    Vector2 min = this.min;
        //    Vector2 max = min + new Vector2(width - 1, height - 1);

        //    //x = Mathf.Lerp(min.x, max.x, x);
        //    //y = Mathf.Lerp(min.y, max.y, y);

        //    RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, y), Vector2.zero, Mathf.Infinity, layerMask);

        //    //if (hit) Debug.Log("(" + x + ", " + y + ")는 벽임.");
        //    //else Debug.Log("(" + x + ", " + y + ")");
        //    //if (hit) Debug.DrawRay(new Vector2(x, y), new Vector2(0.0025f, 0.0025f), Color.red);
        //    //else Debug.DrawRay(new Vector2(x, y), new Vector2(0.0025f, 0.0025f), Color.white);
        //    if (hit) Debug.Log(hit.transform.position.x+","+hit.transform.position.y);
        //    tryCount++;
        //}

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


        pathTileList = new PathTileList(this.transform.position, platformLayerMask);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector2Int index = new Vector2Int(x, y);
                bool isWall = false;
                bool isOneway = false;
                //Debug.Log("[" + x + ", " + y + "]");
                Vector2 rayOrigin = min + new Vector2(x, y) + new Vector2(0.5f, 0.5f);
                RaycastHit2D wallHit = Physics2D.Raycast(rayOrigin, Vector2.zero, Mathf.Infinity, platformLayerMask);
                RaycastHit2D onewayHit = Physics2D.Raycast(rayOrigin, Vector2.zero, Mathf.Infinity, onewayLayerMask);
                //RaycastHit2D hit_top = Physics2D.Raycast(rayOrigin, Vector2.up, 0.6f, layerMask);
                //Debug.DrawRay(rayOrigin, Vector2.up * 0.5f, Color.red); if (hit_top) Debug.Log("TOP");
                //RaycastHit2D hit_below = Physics2D.Raycast(rayOrigin, Vector2.down, 0.6f, layerMask);
                //Debug.DrawRay(rayOrigin, Vector2.down * 0.5f, Color.green); if (hit_below) Debug.Log("BELOW");
                //RaycastHit2D hit_left = Physics2D.Raycast(rayOrigin, Vector2.left, 0.6f, layerMask);
                //Debug.DrawRay(rayOrigin, Vector2.left * 0.5f, Color.blue); if (hit_left) Debug.Log("LEFT");
                //RaycastHit2D hit_right = Physics2D.Raycast(rayOrigin, Vector2.right, 0.6f, layerMask);
                //Debug.DrawRay(rayOrigin, Vector2.right * 0.5f, Color.white); if (hit_right) Debug.Log("RIGHT");

                //bool hit = hit_top && hit_below && hit_left && hit_right;
                //Debug.DrawRay(rayOrigin, Vector2.one*0.05f, Color.red);
                //UnityEditor.EditorApplication.isPaused = true;

                if (wallHit) isWall = true;
                if (onewayHit) isOneway = true;
                //if (hit) Debug.Log("(" + x + ", " + y + ")는 벽임.");
                //if (!hit) Debug.Log("(" + x + ", " + y + ")는 벽이 아님.");
                pathTileList[index] = new PathTile(this, x, y, isWall, isOneway);
            }
        }
    }
}