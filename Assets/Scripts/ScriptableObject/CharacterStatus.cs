using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "", menuName = "CreateCharaStatus")]
public class CharacterStatus : ScriptableObject
{
	public string charaName; // キャラクターネーム.
	public float walkSpeed; // 歩行速度.
	public float runSpeed; // 走行速度.
	public float stamina; // 体力.
	public bool overCome; // 乗り越え.
	public bool obstructive; // 邪魔者.
	public bool stealth; //ステルス.
	public bool special; // 特殊.
}