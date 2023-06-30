/*
*   Created by Kobayashi Atsuki;
*   鬼の水鏡こよみの専用スクリプト.
*   固有性能を用意できていない(企画)ので実質無効.
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
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(KoyomiCS), RpcTarget.AllBuffered);
            }
            //====== オブジェクトやコンポーネントの取得 ======//
            Init();
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
        }
        BaseUpdate();
    }

    [PunRPC]
    private void KoyomiCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }
}
