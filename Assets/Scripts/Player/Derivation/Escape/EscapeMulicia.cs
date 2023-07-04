using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeMulicia : PlayerEscape
{
    GameObject muliciaCoat; // ミュリシアのコート.
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(MuliciaES), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
        }

        // コートを取得
        muliciaCoat = GameObject.Find("mdl_c001_base_00/outer");
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update () {
        if(!photonView.IsMine) {
            return;
        }
        BaseUpdate();
        
        // 鬼と当たったらこれを実行してください（現状スポーンしたら非表示になるようになってるんでコートの取得は出来てます）
        muliciaCoat.SetActive(false);
    }

    [PunRPC]
    private void MuliciaES() {
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
}
