using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChaserMishe : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    void Start() {
        if(photonView.IsMine) {
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(MisheCS), RpcTarget.AllBuffered);
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
    private void MisheCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }
}
