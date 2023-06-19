using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeTolass : PlayerEscape
{
    public GameObject obstructItem; // 障害物オブジェクト.
    private GameObject instanceObstructItem; // 生成した障害物.
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(TolassES), RpcTarget.AllBuffered);
            }
            Init(); // 初期化処理.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update () {
        if(!photonView.IsMine) {
            return;
        }
        if(Input.GetKeyDown(KeyCode.I) && abilityUseAmount > 0) {
            abilityUseAmount--;
            photonView.RPC(nameof(FireObstruct), RpcTarget.All);
        }
        BaseUpdate();
    }

    [PunRPC]
    private void TolassES() {
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
    [PunRPC]
    protected void FireObstruct(PhotonMessageInfo info) {
        Instantiate(obstructItem, transform.position + (-transform.forward * 2), transform.rotation); // リストに追加.
    }
}
