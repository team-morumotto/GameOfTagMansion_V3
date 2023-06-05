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
}
