using System;

[Serializable]
public class RoomSaveData {

    public int id;
    public int x;
    public int y;

    private RoomSaveData() { }
    public RoomSaveData(int id, int x, int y) {
        this.id = id;
        this.x = x;
        this.y = y;
    }

}