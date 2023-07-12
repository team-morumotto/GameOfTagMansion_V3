/*
*   Created by Kobayashi Atsuki;
*   鬼のナユの専用スクリプト.
*/

using UnityEngine;
using Photon.Pun;

public class ChaserNayu : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    void Start()
    {
        if(photonView.IsMine) {
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(NayuCS), RpcTarget.AllBuffered);
            }
            //====== オブジェクトやコンポーネントの取得 ======//
            Init();
            StaminaHealBoost(); // 自動で使用.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        isGameStarted = false;
        GetStatus(); // ステータスの取得.
    }

    void Update() {
        if(!photonView.IsMine) {
            return;
        }
        if(isGameStarted) {
            isGameStarted = false;
            StaminaHealBoost();
        }
        BaseUpdate();
    }

    [PunRPC]
    private void NayuCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
    /// <summary>
    /// 体力の回復力上昇.
    /// </summary>
    private void StaminaHealBoost() {
        staminaHealAmount += HealBoostAmount;
        SE.CallAvilitySE(6); // SE.
        StartCoroutine(TimeEffectLoop(EffectDatabase.avilityEffects[6], 99999f));
    }
}