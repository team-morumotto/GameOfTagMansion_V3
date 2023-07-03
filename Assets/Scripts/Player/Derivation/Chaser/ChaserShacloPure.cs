using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChaserShacloPure : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    void Start() {
        if(photonView.IsMine) {
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(ShacloPureCS), RpcTarget.AllBuffered);
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
            EscapeTargetShow();
        }
        BaseUpdate();
    }

    [PunRPC]
    private void ShacloPureCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//

    public void EscapeTargetShow() {
        PhotonMatchMaker.SetCustomProperty("et", true, 1);
    }
}
