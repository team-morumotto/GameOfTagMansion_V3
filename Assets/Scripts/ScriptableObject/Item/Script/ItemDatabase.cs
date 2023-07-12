using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "ItemList", menuName = "CreateItemList")]
public class ItemDatabase : MonoBehaviour
{
    // アイテムのステータスをまとめたScriptableObjectをリストにまとめる.
    public List<Item> itemList = new List<Item>();
    public Sprite emptySprite; // 空白.

    /// <summary>
    /// Itemクラスのリストを参照し、引数に対応するアイテムのデータを返す.
    /// </summary>
    /// <param name="itemName">アイテムの名前</param>
    public Item GetItemData(string itemName) {
        foreach(var item in itemList) {
            if(item.itemName == itemName) {
                Debug.Log("itemNameが一致");
                return item; // 該当するアイテムのデータを返す.
            }
        }

        Debug.LogError("itemNameが一致しない");
        return null; // 何もヒットしなかった場合はnullを返す.
    }
}
