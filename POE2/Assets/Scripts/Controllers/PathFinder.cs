using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder {

    public float creatureSize;

    public LayerMask wallLayer;

    public PathTileList tileList;

    public PathTile startTile;
    public PathTile endTile;

    public Room room;

    #region Initializer

    protected PathFinder() { }
    public PathFinder(Room room, float creatureSize, LayerMask wallLayer) {
        this.room = room;
        this.creatureSize = creatureSize;
        this.wallLayer = wallLayer;

        Initialize();
    }

    public PathFinder SetStartTile(int x, int y) {
        startTile = tileList[x, y];
        return this;
    }
    public PathFinder SetEndTile(int x, int y) {
        endTile = tileList[x, y];
        return this;
    }

    private void Initialize() {
        tileList = room.pathTileList;
    }

    #endregion

    public AIPath GetPath(int startX, int startY, int endX, int endY) {
        SetStartTile(startX, startY);
        SetEndTile(endX, endY);
        return GetPath();
    }
    public AIPath GetPath() {
        if (FindPath())
            return new AIPath(startTile, endTile);
        else return null;
    }





    // FindPath: 길찾기.
    private bool FindPath() {
        // #1. 리스트 초기화.
        List<PathTile> openList = new List<PathTile>();
        List<PathTile> closeList = new List<PathTile>();

        // #2. 시작 타일을 openList에 넣기.
        openList.Add(startTile);
        //Debug.Log("[FindPath] StartTile: (" + startTile.x + ", " + startTile.y + ")");
        // #3. Path 탐색: 길을 찾거나, 더 이상 찾을 타일이 없으면 종료.
        bool isFound = false;
        while (openList.Count > 0) {
            // openList에서 fCost가 가장 작은 타일을 찾아 탐색. (open->close)
            PathTile tile = GetMinFCost(openList);
            //Debug.Log("[FindPath]   ThisTile: (" + tile.x + ", " + tile.y + ")");
            openList.Remove(tile);
            closeList.Add(tile);

            // 이 타일이 도착 지점이라면 길 찾기 성공 및 종료.
            if (tile.IsEqual(endTile)) {
                //Debug.Log("[FindPath]       ThisTile is EndTile!");
                isFound = true;
                break;
            }

            // 이 타일의 이웃 타일을 탐색하여 openList에 넣거나 갱신.
            foreach (PathTile neighbourTile in GetNeighbours(tile)) {
                // 이미 탐색을 마친 타일이라면 아무것도 하지 않음.
                if (closeList.Contains(neighbourTile)) continue;
                //Debug.Log("[FindPath]       ThisTile's Neighbours: (" + neighbourTile.x + ", " + neighbourTile.y + ")");
                // 새로운 비용 계산.
                int newCost = tile.g + tile.GetDistance(neighbourTile);
                // 새로운 비용이 더 작거나 openList에 없다면 비용 및 parent 갱신.
                if (newCost < neighbourTile.g || !openList.Contains(neighbourTile)) {
                    neighbourTile.g = newCost;
                    neighbourTile.h = neighbourTile.GetDistance(endTile);
                    neighbourTile.f = neighbourTile.g + neighbourTile.h;
                    neighbourTile.parent = tile;
                    //Debug.Log("[FindPath]       ThisTile's Neighbours: (" + neighbourTile.x + ", " + neighbourTile.y + ")");
                    if (!openList.Contains(neighbourTile)) {
                        openList.Add(neighbourTile);
                    }
                }
            }
        }

        return isFound;
    }

    // GetMinFCost: 타일 리스트에서 Cost가 가장 적은 타일 리턴.
    private PathTile GetMinFCost(List<PathTile> tileList) {
        PathTile tile = tileList[0];
        for (int i = 1; i < tileList.Count; i++) {
            if (tileList[i].f < tile.f || (tileList[i].f == tile.f && tileList[i].h < tile.h)) {
                tile = tileList[i];
            }
        }
        return tile;
    }

    // CanMove: start에서 end로 직선 경로 이동이 가능한지 여부.
    private bool CanMove(Vector2 start, Vector2 end) {
        Vector2 rayOrigin = start;
        Vector2 rayDirection = (end - start).normalized;
        float rayDistance = (end - start).magnitude;
        LayerMask layerMask = wallLayer;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, layerMask);

        if (hit) {
            return false;
        }
        else {
            return true;
        }
    }

    // GetTIle: (x, y)의 Tile을 tileList에서 받아온다.
    private PathTile GetTile(int x, int y) => tileList[new Vector2Int(x, y)];

    // GetNeighbours: 해당 타일의 이동 가능한 이웃 타일들을 받아온다.
    // 대각선 방향은 두 정방향이 모두 이동 가능해야 한다.
    private List<PathTile> GetNeighbours(PathTile tile) {
        List<PathTile> neighbours = new List<PathTile>();
        int[,] temp = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };     // top-right-bottom-left
        bool[] moveDirection = new bool[4];

        for (int i = 0; i < 4; i++) {
            int x = tile.x + temp[i, 0];
            int y = tile.y + temp[i, 1];
            PathTile neighbourTile = GetTile(x, y);
            if (tileList.IsExist(neighbourTile)) {
                if (tileList.CanMove(tile, neighbourTile, creatureSize)) {
                    moveDirection[i] = true;
                    neighbours.Add(neighbourTile);
                }
            }
        }
        for (int i = 0; i < 4; i++) {
            if (moveDirection[i] && moveDirection[(i + 1) % 4]) {
                int x = tile.x + temp[i, 0] + temp[(i + 1) % 4, 0];
                int y = tile.y + temp[i, 1] + temp[(i + 1) % 4, 1];
                PathTile neighbourTile = GetTile(x, y);
                if (tileList.IsExist(neighbourTile)) {
                    if (tileList.CanMove(tile, neighbourTile, creatureSize)) {
                        neighbours.Add(neighbourTile);
                    }
                }
            }
        }
        return neighbours;
    }
}

public class PathTile {
    public int x, y;
    public int f, g, h;
    public bool isWall;
    public bool isOneway;
    public bool onGround;   // 아래에 벽이 있는지 여부.
    public PathTile parent;
    public Vector2 position;
    public Room room;

    protected PathTile() { }
    public PathTile(Room room, int x, int y, bool isWall, bool isOneway) {
        this.room = room;
        this.x = x;
        this.y = y;
        this.isWall = isWall;
        this.isOneway = isOneway;

        // 실제 위치 계산.
        position = room.min + new Vector2(x, y) + new Vector2(0.5f, 0.5f);

        // 이 타일이 벽이나 단방향일 때: 위 타일이 비어있다면 onGround 설정.
        if (isWall || isOneway) {
            PathTile tile = room.pathTileList[x, y + 1];
            if (tile != null) if (!tile.isWall && !tile.isOneway) tile.onGround = true;
        }
        // 이 타일이 벽도 단방향도 아닐 때: 아래 타일이 벽이나 단방향이라면 onGround 설정.
        else {
            PathTile tile = room.pathTileList[x, y - 1];
            if (tile != null) 
                if (tile.isWall || tile.isOneway) this.onGround = true;
        }
    }

    public bool IsEqual(int x, int y) => this.x == x && this.y == y;
    public bool IsEqual(PathTile tile) {
        if (this.x == tile.x && this.y == tile.y) return true;
        else return false;
    }

    // GetDistance: 해당 Tile 까지의 거리.
    public int GetDistance(PathTile tile) {
        int x = Mathf.Abs(this.x - tile.x);
        int y = Mathf.Abs(this.y - tile.y);
        if (x > y) return 14 * y + 10 * (x - y);
        else return 14 * x + 10 * (y - x);
    }
}
public class PathTileList {

    public LayerMask wallLayer;
    public Vector2 worldPosition;
    public Dictionary<Vector2Int, PathTile> list;

    public int Count { get => list.Count; }

    protected PathTileList() { }
    public PathTileList(Vector2 worldPosition, LayerMask layerMask) {
        this.worldPosition = worldPosition;
        this.wallLayer = layerMask;

        list = new Dictionary<Vector2Int, PathTile>();
    }
    public PathTile this[Vector2Int pos] {
        get {
            if (list.ContainsKey(pos))
                return list[pos];
            else
                return null;
        }
        set {
            if (list.ContainsKey(pos))
                list[pos] = value;
            else
                list.Add(pos, value);
        }
    }
    public PathTile this[int x, int y] {
        get {
            if (list.ContainsKey(new Vector2Int(x, y))) return list[new Vector2Int(x, y)];
            else return null;
        }
        set {
            if (list.ContainsKey(new Vector2Int(x, y)))
                list[new Vector2Int(x, y)] = value;
            else
                list.Add(new Vector2Int(x, y), value);
        }
    }

    // IsExist: 해당 PathTile을 보유하고 있는지 검사.
    public bool IsExist(PathTile tile) {
        List<PathTile> tileList = list.Values.ToList();
        for(int i = 0; i < tileList.Count; i++) {
            if (tileList[i].IsEqual(tile)) return true;
        }
        return false;
    }

    // CanMove: tile1에서 tile2로 직선이동이 가능한지 레이캐스팅 검사.
    public bool CanMove(PathTile tile1, PathTile tile2, float size = 1) {
        Vector2 pos1 = tile1.position;
        Vector2 pos2 = tile2.position;

        Vector2 rayOrigin = pos1;
        Vector2 rayDirection = (pos2 - pos1).normalized;
        float rayDistance = (pos2 - pos1).magnitude;
        LayerMask layerMask = wallLayer;


        Collider2D tile1Collider = Physics2D.Raycast(rayOrigin, Vector2.zero, 0.1f, layerMask).collider;
        if (tile1Collider != null) tile1Collider.enabled = false;



        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, layerMask);
        RaycastHit2D right = Physics2D.Raycast(pos2, Vector2.right, size, layerMask);
        RaycastHit2D left = Physics2D.Raycast(pos2, Vector2.left, size, layerMask);
        //Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
        //Debug.DrawRay(pos2, Vector2.right * size, Color.white);
        //Debug.DrawRay(pos2, Vector2.left * size, Color.black);
        //UnityEditor.EditorApplication.isPaused = true;


        //if (hit) Debug.Log(hit.transform.name);
        //if (right) Debug.Log("오른쪽히트");
        //if (left) Debug.Log("왼쪽히트");

        if (tile1Collider != null) tile1Collider.enabled = true;

        return (!hit && !right && !left);
    }

    // GetOnGroundTileList: OnGround 타일들을 찾아 리턴.
    public List<PathTile> GetOnGroundTileList() {
        List<PathTile> tilelist = new List<PathTile>();
        foreach(PathTile pathTile in list.Values) {
            if (pathTile.onGround) tilelist.Add(pathTile);
        }
        return tilelist;
    }

    // GetNotWallTileList: 벽이 아닌 타일들을 찾아 리턴.
    public List<PathTile> GetNotWallTileList() {
        List<PathTile> tilelist = new List<PathTile>();
        foreach(PathTile pathTile in list.Values) {
            if (pathTile.isWall == false) tilelist.Add(pathTile);
        }
        return tilelist;
    }
}
public class AIPath {

    private int currentOrder;

    private List<PathTile> pathList;

    protected AIPath() { }
    public AIPath(List<PathTile> pathList) {
        this.pathList = pathList;
        currentOrder = 0;
    }
    public AIPath(PathTile startTile, PathTile endTile) {
        pathList = new List<PathTile>();
        PathTile currentTile = endTile;
        pathList.Add(currentTile);
        while (!currentTile.IsEqual(startTile)) {
            pathList.Add(currentTile.parent);
            currentTile = currentTile.parent;
        }
        pathList.Reverse();
    }

    // 다음 위치 반환. 다음 위치가 없다면 return -10491049.
    public Vector2 GetNextPath() {
        if (++currentOrder >= pathList.Count) return new Vector2(-10491049, -10491049);
        else return pathList[currentOrder].position;
    }


    // Simplify: Path 방향 간결화.
    public AIPath Simplify() {
        List<PathTile> simplifiedPath = new List<PathTile>();
        Vector2 oldDirection = Vector2.zero;

        for (int i = 1; i < pathList.Count; i++) {
            float x = pathList[i - 1].position.x - pathList[i].position.x;
            float y = pathList[i - 1].position.y - pathList[i].position.y;
            Vector2 newDirection = new Vector2(x, y).normalized;
            if (newDirection != oldDirection) simplifiedPath.Add(pathList[i - 1]);
            oldDirection = newDirection;
        }
        simplifiedPath.Add(pathList[pathList.Count-1]);

        pathList = simplifiedPath;

        return this;
    }
    // Simplify_Flight: Path 방향 간결화. (비행)
    public AIPath Simplify_Flight() {
        List<PathTile> simplifiedPath = new List<PathTile>();
        Vector2 oldDirection = Vector2.zero;

        bool flying = false;
        for (int i = 1; i < pathList.Count; i++) {
            float x = pathList[i - 1].position.x - pathList[i].position.x;
            float y = pathList[i - 1].position.y - pathList[i].position.y;
            Vector2 newDirection = new Vector2(x, y).normalized;
            // 수평 방향: 경로 간결화.
            if (Mathf.Abs(newDirection.y) < 0.1f) {
                if (newDirection != oldDirection) simplifiedPath.Add(pathList[i - 1]);
            }
            // 수직 방향: 비행모드 ON.
            else {
                flying = true;
            }

            // 비행 모드일 때: Ground를 만나면 리스트에 추가 후 비행모드 OFF.
            if (flying && pathList[i - 1].onGround) {
                simplifiedPath.Add(pathList[i - 1]);
                flying = false;
            }

            oldDirection = newDirection;
        }
        simplifiedPath.Add(pathList[pathList.Count - 1]);

        pathList = simplifiedPath;

        return this;
    }

    public void Debug_LogAllPath() {
        string log = "";
        for (int i = 0; i < pathList.Count; i++) {
            string _log = "(" + pathList[i].x + ", " + pathList[i].y + ")";

            log += ",";
            log += _log;
        }
        //Debug.Log(log);
    }
}