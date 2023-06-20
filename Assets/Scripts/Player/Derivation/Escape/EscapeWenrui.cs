using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeWenrui : PlayerEscape
{
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(WenruiES), RpcTarget.AllBuffered);
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
        if(Input.GetKeyDown(KeyCode.I)) {
            // 固有性能はここから使用する.
        }
        BaseUpdate();
    }

    [PunRPC]
    private void WenruiES() {
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
}