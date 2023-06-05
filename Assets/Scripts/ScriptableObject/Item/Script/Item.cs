using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "", menuName = "CreateItem")]
public class Item : ScriptableObject
{
    public enum ItemType{
        obstacle, // 障害物系.
        ability // 能力系.
    }
    public ItemType itemType; // アイテムの種類.
    public string name; // アイテムの名前.
    public Sprite icon; // アイテムのアイコン.
    public GameObject instance; // インスタンス(プレハブ). ItemTypeがobstructのときのみ.

    public ItemType GetItemType() {
        return itemType;
    }

    public Sprite GetIcon() {
        return icon;
    }

    public string GetName() {
        return name;
    }
}
