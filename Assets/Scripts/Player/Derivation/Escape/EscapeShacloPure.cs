using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeShacloPure : PlayerEscape
{
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(ShacloPureES), RpcTarget.AllBuffered);
            }
            isFrequency = true;
            Init(); // オブジェクトやコンポーネントの取得.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update () {
        if(!photonView.IsMine) {
            return;
        }
        if(Input.GetKeyDown(KeyCode.Space) && abilityUseAmount > 0) {
            avilityRiminingUpdate();
            ChaserTargetShow();
        }
        BaseUpdate();
    }

    [PunRPC]
    private void ShacloPureES() {
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
    public void ChaserTargetShow() {
        PhotonMatchMaker.SetCustomProperty("ct", true, 1);
    }
}
