/*
*   Created by Kobayashi Atsuki;
*   鬼のトラスの専用スクリプト.
*/

using UnityEngine;
using Photon.Pun;

public class ChaserTolass : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    [SerializeField]
    GameObject obstructItem; // 障害物オブジェクト.
    private GameObject instanceObstructItem; // 生成した障害物.
    void Start()
    {
        if(photonView.IsMine) {
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(TolassCS), RpcTarget.AllBuffered);
            }
            Init(); // 初期化処理.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update() {
        if(!photonView.IsMine) {
            return;
        }
        if(Input.GetKeyDown(KeyCode.I) && abilityUseAmount > 0) {
            avilityRiminingUpdate();
            photonView.RPC(nameof(FireObstruct), RpcTarget.All);
        }
        BaseUpdate();
    }

    [PunRPC]
    private void TolassCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
    /// <summary>
    /// 障害物を前方に生成する.
    /// </summary>
    [PunRPC]
    protected void FireObstruct(PhotonMessageInfo info) {
        Instantiate(obstructItem, transform.position + (transform.forward * 2), transform.rotation); // リストに追加.
    }
}
