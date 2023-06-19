using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChaserMulicia : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    void Start() {
        if(photonView.IsMine) {
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(MuliciaCS), RpcTarget.AllBuffered);
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
        GetPlayersPos();
        BaseUpdate();
    }

    [PunRPC]
    private void MuliciaCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//

    private float detectionRange = 10.0f; // 探知範囲.
    /// <summary>
    /// 自分とほかキャラとの相対位置を計算し、一定範囲内なら反応する.
    /// ※ミュリシア(鬼)の固有性能.
    /// </summary>
    public void GetPlayersPos() {
        foreach(var players in escapeList) {
            var tmpDistance = (players.transform.position - transform.position).magnitude; // 自分とほかキャラの相対位置を計算.
            // 探知範囲内なら.
            if(tmpDistance < detectionRange) {
                print("【" + players + "】が探知範囲内です");
            }
        }
    }
}
