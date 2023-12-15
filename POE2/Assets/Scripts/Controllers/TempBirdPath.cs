using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempBirdPath : MonoBehaviour {

    public LayerMask wallLayer;
    public Creature creature;

    public void Temp() {
        Room currentRoom = creature.room;

        //PathFinder pathFinder = new PathFinder(currentRoom, wallLayer).SetStartTile(1, 1).SetEndTile(10, 10);
        //AIPath path = pathFinder.GetPath();
    }
}