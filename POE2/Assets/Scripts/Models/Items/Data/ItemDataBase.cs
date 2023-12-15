using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataBase : MonoBehaviour {

    public List<ItemData> itemDataList = new List<ItemData>();

    private void BuildDataBase() {
        itemDataList = new List<ItemData>(

            );
    }

}