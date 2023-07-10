using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Effekseer;

[Serializable]
[CreateAssetMenu(fileName = "", menuName = "CreateCharaStatus")]
public class CharacterStatus : ScriptableObject
{
	public string charaName; // キャラクターネーム.
	public float walkSpeed; // 歩行速度.
	public float runSpeed; // 走行速度.
	public float staminaAmount; // スタミナ.
	public float staminaHealAmount; // スタミナ回復量.
	public bool overCome; // 乗り越え.
	public bool floating; // 浮遊.
	public Sprite avilitySprite; // 固有性能のアイコン.
}