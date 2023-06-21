using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChaserWenrui : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    void Start() {
        if(photonView.IsMine) {
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(WenruiCS), RpcTarget.AllBuffered);
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
        if(Input.GetKeyDown(KeyCode.I) && abilityUseAmount > 0) {
            avilityRiminingUpdate();
            // 固有性能はここから使用する.
        }
        BaseUpdate();
    }

    [PunRPC]
    private void WenruiCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
}
