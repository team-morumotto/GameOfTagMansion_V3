/*
*   Created by Kobayashi Atsuki.
*   キャラの固有性能まとめ(キャラごとに扱えるものが違う).
*/

using UnityEngine;
using System.Collections;

public class CharacterPerformance : PlayerBase
{
    delegate void Performance();

    protected float HealBoostAmount = 3f;
    /// <summary>
    /// ルームに参加している逃げキャラ全員の体力回復速度常時ブースト.
    /// ※ナユの固有性能
    /// </summary>
    protected void StaminaHealBoost() {
        PhotonMatchMaker.SetCustomProperty("hb", HealBoostAmount, 1);
    }

    private float detectionRange = 10.0f; // 探知範囲.
    /// <summary>
    /// ほかキャラ相対位置を計算し、一定範囲内なら反応する.
    /// ※ミュリシア(鬼)の固有性能.
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

    private float nowAbilityTime = 0.0f; // 能力発動の経過時間.
    private float maxAbilityTime = 1.0f; // 能力の効果時間.
    private float scaleChangeAvirityTime = 2.0f; // 小さくなる能力の効果時間.
    private float reductionAmount = 0.5f; // 縮小後のサイズ.
    private float expansionAmount = 1.0f; // 拡大後のサイズ.
    private bool isAbility = false; //発動状態かどうか

    /// <summary>
    /// 水鏡こよみの固有能力.
    /// </summary>
    protected void MikagamiKoyomiAbility(){
        // 発動可能か.
        if(!isAbility){
            StartCoroutine(Cor());
        }
    }

    private IEnumerator Cor() {
        yield return ScaleChange(reductionAmount);
        yield return Delay(scaleChangeAvirityTime);
        yield return ScaleChange(expansionAmount);
        isAbility = false;
    }

    /// <summary>
    /// スケールを急激に変更する.
    /// </summary>
    /// <param name="easeAmount">最終的なスケール値</param>
    private IEnumerator ScaleChange(float easeAmount) {
        isAbility = true;
        nowAbilityTime = 0;
        while(nowAbilityTime <= maxAbilityTime) {
            nowAbilityTime += Time.deltaTime; // 経過時間.
            var a = nowAbilityTime / maxAbilityTime; // 経過時間 / 終了時間.
            var easea = easeOutElastic(a, easeAmount); // イージングを用いてスケール値を算出.
            var dd = transform.localScale;
            dd.x = easea;
            dd.y = easea;
            dd.z = easea;
            transform.localScale = dd;
            yield return null; // 1フレーム遅延.
        }
    }

    /// <summary>
    /// 任意の時間遅延.
    /// </summary>
    /// <param name="time">遅延する時間</param>
    private IEnumerator Delay(float time) {
        yield return new WaitForSeconds(time);
    }

    /// <summary>
    /// イージング用
    /// https://easings.net/ja#easeInElastic
    /// </summary>
    protected float easeOutElastic(float x, float addValue){
        float c4 = (Mathf.PI * 2) / 3;
        return x == 0? 0: x == 1? 1: Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10f - 0.75f) * c4) + addValue;
    }

    protected void HitObstruct() {
        StartCoroutine(Stan());
    }

    /// <summary>
    /// 10秒スタン
    /// </summary>
    /// <returns></returns>
    private IEnumerator Stan() {
        print("スタン中");
        isStan = true;
        yield return new WaitForSeconds(3.0f);
        isStan = false;
        print("スタン後");
    }
}