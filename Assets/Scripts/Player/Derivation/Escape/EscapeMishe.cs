using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeMishe : PlayerEscape
{
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(MisheES), RpcTarget.AllBuffered);
            }
            isAddhaveItem = true; // アイテムの複数個持ちが可能.
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
            isAddhaveItem = false;
        }else {
            isAddhaveItem = true;
        }
        BaseUpdate();
    }

    [PunRPC]
    private void MisheES() {
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
}
