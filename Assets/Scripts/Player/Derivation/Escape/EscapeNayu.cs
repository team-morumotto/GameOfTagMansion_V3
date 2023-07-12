using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Photon.Pun;

public class EscapeNayu : PlayerEscape
{
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(NayuES), RpcTarget.AllBuffered);
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
        if(isGameStarted) {
            isGameStarted = false;
            StaminaHealBoostRoom();
        }
        BaseUpdate();
    }

    [PunRPC]
    private void NayuES() {
        Destroy(this); // 削除.
    }

    //------ 以下、固有性能 ------//
    /// <summary>
    /// ルームに参加している逃げキャラ全員の体力回復速度常時ブースト.
    /// ※ナユの固有性能
    /// </summary>
    public void StaminaHealBoostRoom() {
        print("使用検知");
        PhotonMatchMaker.SetCustomProperty("hb", HealBoostAmount, 1);
    }
}