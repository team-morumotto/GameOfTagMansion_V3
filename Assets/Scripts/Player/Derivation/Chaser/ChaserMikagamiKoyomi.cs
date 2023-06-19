/*
*   Created by Kobayashi Atsuki;
*   鬼の水鏡こよみの専用スクリプト.
*/

using System.Collections;
using UnityEngine;
using Photon.Pun;

public class ChaserMikagamiKoyomi : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    private float nowAbilityTime = 0.0f; // 能力発動の経過時間.
    private float maxAbilityTime = 1.0f; // 能力の効果時間.
    private float scaleChangeAvirityTime = 2.0f; // 小さくなる能力の効果時間.
    private float reductionAmount = 0.5f; // 縮小後のサイズ.
    private float expansionAmount = 1.0f; // 拡大後のサイズ.
    void Start()
    {
        if(photonView.IsMine) {
            // 自分が鬼なら.
// 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(KoyomiCS), RpcTarget.AllBuffered);
            }
            //====== オブジェクトやコンポーネントの取得 ======//
            Init();
            CharacterScaleChange(); // 自動で使用.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update() {
        if(!photonView.IsMine) {
            return;
        }
        if(Input.GetKeyDown(KeyCode.I) && !isUseAvility && !isCoolTime) {
            isUseAvility = true;
            CharacterScaleChange();
        }
        BaseUpdate();
    }

    private IEnumerator CharacterScaleChange() {
        yield return ScaleChange(reductionAmount);
        yield return Delay(scaleChangeAvirityTime);
        yield return ScaleChange(expansionAmount);
        isUseAvility = false;
        StartCoroutine(AvillityCoolTime(10.0f));
        // 発動終了.
    }

    [PunRPC]
    private void KoyomiCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
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
