using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "", menuName = "CreateItem")]
public class Item : ScriptableObject
{
    public string itemName; // アイテムの名前.
    public Sprite itemIcon; // アイテムのアイコン.
    public Sprite GetIcon() {
        return itemIcon;
    }

    public string GetName() {
        return itemName;
    }
}
