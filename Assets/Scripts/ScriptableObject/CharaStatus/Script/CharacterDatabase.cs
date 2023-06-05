using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "CharastatusList", menuName = "CreateCharastatusList")]
public class CharacterDatabase : MonoBehaviour
{
    // キャラクターごとのステータスをまとめたScriptableObjectをリストにまとめる.
    public List<CharacterStatus> statusList = new List<CharacterStatus>();
}