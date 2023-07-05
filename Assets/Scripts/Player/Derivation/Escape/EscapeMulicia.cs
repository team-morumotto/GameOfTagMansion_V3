using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class EscapeMulicia : PlayerEscape
{
    private GameObject muliciaCoat; // ミュリシアのコート.
    private bool isAvoidingCapture = false; // 捕まりを回避したか.
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(MuliciaES), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
            // コートを取得
            muliciaCoat = GameObject.Find("mdl_c001_base_00/outer");
        }

        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update () {
        if(!photonView.IsMine) {
            return;
        }
        BaseUpdate();
    }

    [PunRPC]
    private void MuliciaES() {
        Destroy(this); // 削除.
    }

    /// <summary>
    /// プレイヤーのカスタムプロパティが変更された時.
    /// </summary>
    /// <param name="targetPlayer">変更があったプレイヤー</param>
    /// <param name="changedProps">変更されたプロパティ</param>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // 自分でない場合.
        if(!photonView.IsMine) {
            return;
        }

        if(targetPlayer == PhotonNetwork.LocalPlayer) {
            print("namename"+targetPlayer.NickName);
            foreach(var prop in changedProps) {
                var tmpKey = prop.Key.ToString();
                switch(tmpKey) {
                    case "c":
                        if((bool)prop.Value) {
                            // 回避したか.
                            if(!isAvoidingCapture) {
                                isAvoidingCapture = true;
                                muliciaCoat.SetActive(false); // コートを脱ぐ.
                                PhotonMatchMaker.SetCustomProperty("c", false, 0);
                            }else {
                                resultWinLoseText.text = "捕まった！";
                                GameEnd(false); // ゲーム終了.
                            }
                        }
                    break;
                }
            }
        }else{
            print("Invalid");
            print("namename"+targetPlayer.NickName);
        }
    }
}