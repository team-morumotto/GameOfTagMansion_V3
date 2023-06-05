/*
*   Created by Kobayashi Atsuki.
*   キャラの固有性能まとめ(キャラごとに扱えるものが違う).
*/

using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class CharacterPerformance : PlayerBase
{
    protected float HealBoostAmount = 3f;
    /// <summary>
    /// ルームに参加している逃げキャラ全員の体力回復速度をブースト(常時発動).
    /// </summary>
    protected void StaminaHealBoost() {
        PhotonMatchMaker.SetCustomProperty("hb", HealBoostAmount, 1);
    }

    protected void PassiveActivate() {
        staminaHealAmount += HealBoostAmount;
    }

    public static List<PlayerBase> playerBase = new List<PlayerBase>();
    protected List<GameObject> escapeList = new List<GameObject>();
    private List<GameObject> playerList = new List<GameObject>();
    /// <summary>
    /// ルーム内の全キャラを取得する.
    /// </summary>
    protected void GetPlayers() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        playerList = new List<GameObject>(); // キャラクターリストの初期化.
        escapeList = new List<GameObject>(); // 逃げキャラリストの初期化.

        if(players != null) {
            foreach(var player in players) {
                // 取得したオブジェクトが自分でない場合.
                if(player != this.gameObject) {
                    playerList.Add(player);
                    var tmp1 = player.GetComponent<PlayerBase>();
                    playerBase.Add(tmp1);
                }

                if(player.GetComponent<PlayerEscape>()) {
                    escapeList.Add(player);
                }
            }
        }
    }

    private float detectionRange = 10.0f;
    /// <summary>
    /// ほかキャラ相対位置を計算し、一定範囲内なら反応する.
    /// </summary>
    /// <param name="minePos">自分の位置</param>
    protected void GetPlayersPos(Vector3 minePos) {
        foreach(var players in playerList) {
            var tmpDistance = (players.transform.position - minePos).magnitude; // 自分とほかキャラの相対位置を計算.
            if(tmpDistance < detectionRange) {
                print("【" + players + "】が探知範囲内です");
            }
        }
    }
}