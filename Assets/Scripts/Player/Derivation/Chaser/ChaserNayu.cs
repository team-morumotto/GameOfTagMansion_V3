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
    private float HealBoostAmount = 0.5f;
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
    protected void StaminaHealBoost() {
        staminaHealAmount += HealBoostAmount;
    }
}