/*
*   Created by Kobayashi Atsuki.
*   キャラの固有性能まとめ(キャラごとに扱えるものが違う).
*/

using UnityEngine;
using System.Collections;

public class CharacterPerformance : PlayerBase
{
    protected delegate void Performance();
    protected Performance performance;

    protected void EscapeAbilitySet() {
        switch(characterNumber) {
            case 0: return;
            case 1: performance = FookShot; break;
            case 2: performance = CharacterScaleChange; break;
            case 3: break;
            case 4: performance = EscapeTargetShow; break;
            case 5: break;
            case 6: break;
            case 7: break;
            case 8: break;
            case 9: performance = StaminaHealBoost; break;
        }
    }

    protected void ChaserAbilitySet() {
        switch(characterNumber) {
            case 0: return;
            case 1: 
            case 2: performance = CharacterScaleChange; break;
            case 3: break;
            case 4: performance = ChaserTargetShow; break;
            case 5: break;
            case 6: performance = GetPlayersPos; break;
            case 7: break;
            case 8: break;
            case 9: performance = StaminaHealBoost; break;
        }
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
        anim.SetBool("Stan", true); // スタンアニメーション.
        yield return new WaitForSeconds(3.0f);
        isStan = false;
        anim.SetBool("Stan", false); // スタンアニメーション.
        print("スタン後");
    }

    protected void FookShot() {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            StartCoroutine(LinearMove(hit.point));
        }
    }

    float relativeDistance;
    float HitDistance = 1.0f;
    float speed = 30.0f; // 移動速度
    private IEnumerator LinearMove(Vector3 targetPos) {
        rb.useGravity = false;
        do {
            print("relative");
            var tmp = targetPos - transform.position;
            Vector3 direction = tmp.normalized; // 目標位置への方向ベクトルを計算
            relativeDistance = tmp.magnitude;
            float distance = speed * Time.deltaTime; // 目標位置への移動量を計算
            transform.position += direction * distance; // 目標位置に向かって移動

            //ベクトルの大きさが0.01以上の時に向きを変える処理をする
            if (relativeDistance > 0.01f) {
                transform.rotation = Quaternion.LookRotation(direction); //向きを変更する
            }

            yield return null; // 1フレーム遅延.
        } while(relativeDistance > HitDistance);

        isUseAvility = false;
        rb.useGravity = true;
    }

    private float nowAbilityTime = 0.0f; // 能力発動の経過時間.
    private float maxAbilityTime = 1.0f; // 能力の効果時間.
    private float scaleChangeAvirityTime = 2.0f; // 小さくなる能力の効果時間.
    private float reductionAmount = 0.5f; // 縮小後のサイズ.
    private float expansionAmount = 1.0f; // 拡大後のサイズ.

    /// <summary>
    /// 水鏡こよみの固有能力.
    /// </summary>
    protected void CharacterScaleChange(){
        // 発動可能か.
        StartCoroutine(Cor());
    }

    private IEnumerator Cor() {
        yield return ScaleChange(reductionAmount);
        yield return Delay(scaleChangeAvirityTime);
        yield return ScaleChange(expansionAmount);
        // 発動終了.
        isUseAvility = false;
    }

    /// <summary>
    /// スケールを急激に変更する.
    /// </summary>
    /// <param name="easeAmount">最終的なスケール値</param>
    private IEnumerator ScaleChange(float easeAmount) {
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
    /// イージング.
    /// https://easings.net/ja#easeInElastic
    /// </summary>
    protected float easeOutElastic(float x, float addValue){
        float c4 = (Mathf.PI * 2) / 3;
        return x == 0? 0: x == 1? 1: Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10f - 0.75f) * c4) + addValue;
    }

    protected void EscapeTargetShow() {
        PhotonMatchMaker.SetCustomProperty("et", true, 1);
    }

    protected void ChaserTargetShow() {
        PhotonMatchMaker.SetCustomProperty("ct", true, 1);
    }

    /// <summary>
    /// ルーム内のキャラクターのカーソルを表示.
    /// </summary>
    /// <param name="isEscape">呼び出し側が逃げキャラかどうか</param>

    protected IEnumerator TargetShow(bool isEscape) {
        if(isEscape) {
            chaserTarget.enabled = true;
            yield return new WaitForSeconds(10.0f);
            chaserTarget.enabled = false;
        }else{
            foreach(var targets in escapeTargetList) {
                targets.enabled = true;
            }
            yield return new WaitForSeconds(10.0f);
            foreach(var targets in escapeTargetList) {
                targets.enabled = false;
            }
        }
    }

    private float detectionRange = 10.0f; // 探知範囲.
    /// <summary>
    /// 自分とほかキャラとの相対位置を計算し、一定範囲内なら反応する.
    /// ※ミュリシア(鬼)の固有性能.
    /// </summary>
    protected void GetPlayersPos() {
        foreach(var players in playerList) {
            var tmpDistance = (players.transform.position - transform.position).magnitude; // 自分とほかキャラの相対位置を計算.
            if(tmpDistance < detectionRange) {
                print("【" + players + "】が探知範囲内です");
            }
        }
    }

    protected float HealBoostAmount = 3f;
    /// <summary>
    /// ルームに参加している逃げキャラ全員の体力回復速度常時ブースト.
    /// ※ナユの固有性能
    /// </summary>
    protected void StaminaHealBoost() {
        PhotonMatchMaker.SetCustomProperty("hb", HealBoostAmount, 1);
    }


    /// <summary>
    /// 任意の時間遅延.
    /// </summary>
    /// <param name="time">遅延する時間</param>
    private IEnumerator Delay(float time) {
        yield return new WaitForSeconds(time);
    }
}