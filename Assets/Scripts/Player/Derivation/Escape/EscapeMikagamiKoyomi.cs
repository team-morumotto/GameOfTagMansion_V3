/*
* 水鏡こよみのスクリプト.
* このキャラは逃げしか存在しない.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeMikagamiKoyomi : PlayerEscape
{
    private float nowAbilityTime = 0.0f; // 能力発動の経過時間.
    private float maxAbilityTime = 20.0f; // 能力の効果時間.
    private float reductionAmount = 0.5f; // 縮小後のサイズ.
    private float expansionAmount = 1.0f; // 拡大後のサイズ.
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(KoyomiES), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update () {
        if(!photonView.IsMine) {
            return;
        }

        // 固有能力が使用可能か.
        if(isCanUseAbility) {
            if(Input.GetKeyDown(KeyCode.Space) && !isUseAvility && !isCoolTime) {
                isUseAvility = true;
                SE.CallAvilitySE(2); // SE.
                StartCoroutine(CharacterScaleChange());
            }
        }
        BaseUpdate();
    }

    private IEnumerator CharacterScaleChange() {
        yield return ScaleChange(reductionAmount);
        yield return Delay(maxAbilityTime);
        yield return ScaleChange(expansionAmount);
        isUseAvility = false;
        StartCoroutine(AvillityCoolTime(10.0f));
        // 発動終了.
    }

    [PunRPC]
    private void KoyomiES() {
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
    /// <summary>
    /// スケールを急激に変更する.
    /// </summary>
    /// <param name="easeAmount">最終的なスケール値</param>
    private IEnumerator ScaleChange(float easeAmount) {
        emitter.Play(EffectDatabase.avilityEffects[1]);
        nowAbilityTime = 0;
        while(nowAbilityTime <= 1.0f) {
            var a = nowAbilityTime / 1.0f; // 経過時間 / 終了時間.
            var easea = easeOutElastic(a, easeAmount); // イージングを用いてスケール値を算出.
            var dd = transform.localScale;
            dd.x = easea;
            dd.y = easea;
            dd.z = easea;
            transform.localScale = dd;

            nowAbilityTime += Time.deltaTime; // 経過時間.
            yield return null; // 1フレーム遅延.
        }
    }

    /// <summary>
    /// イージング.
    /// https://easings.net/ja#easeInElastic
    /// </summary>
    public float easeOutElastic(float x, float addValue){
        float c4 = (Mathf.PI * 2) / 3;
        return x == 0? 0: x == 1? 1: Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10f - 0.75f) * c4) + addValue;
    }

    /// <summary>
    /// 任意の時間遅延.
    /// </summary>
    /// <param name="time">遅延する時間</param>
    private IEnumerator Delay(float time) {
        yield return new WaitForSeconds(time);
    }
}