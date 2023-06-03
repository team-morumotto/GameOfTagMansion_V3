/*
*   Created by Kobayashi Atsuki.
*   キャラの固有性能まとめ(キャラごとに扱えるものが違う).
*/

using UnityEngine;

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

    /// <summary>
    /// ルーム内の逃げキャラを取得する.
    /// </summary>
    protected GameObject[] players;
    protected void EscapeCount() {
        players = GameObject.FindGameObjectsWithTag("Nige");
    }

    private float detectionRange = 10.0f;
    /// <summary>
    /// 鬼と逃げとの相対位置を計算し、一定範囲内なら反応する.
    /// </summary>
    /// <param name="chaserPos">鬼の位置</param>
    protected void GetEscapesPos(Vector3 chaserPos) {
        // 逃げキャラの数分繰り返し.
        for(int i = 0; i < players.Length; i++) {
            var tmpDistance = (players[i].transform.position - chaserPos).magnitude; // 
            if(tmpDistance < detectionRange) {
                print("【" + players[i] + "】が探知範囲内です");
            }
        }
    }
}