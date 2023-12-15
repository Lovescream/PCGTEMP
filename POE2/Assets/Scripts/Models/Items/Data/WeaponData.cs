using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Items/Weapon", order = 1)]
public class WeaponData : ItemData {
    public HandType handType;

    public override ItemType Type {
        get => ItemType.Weapon; }
}