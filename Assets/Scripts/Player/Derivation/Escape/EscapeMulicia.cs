using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Effekseer;

public class EscapeMulicia : PlayerEscape
{
    public Sprite escapeAvilityImage;
    private GameObject muliciaCoat; // ミュリシアのコート.
    private bool isAvoidingCapture = false; // 捕まりを回避したか.
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(MuliciaES), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
        }

        SE = GameObject.Find("Obj_SE").GetComponent<Button_SE>(); // SEコンポーネント取得.
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        emitter = GetComponent<EffekseerEmitter>();
        EffectDatabase = GameObject.Find("EffectList").GetComponent<EffectDatabase>();
        GetStatus(); // ステータスの取得.
        if(photonView.IsMine) {
            if(GoToChooseChara.GetPlayMode() == 0) {
                avilityImage.sprite = escapeAvilityImage;
            }
        }
        // コートを取得
        muliciaCoat = GameObject.Find("mdl_c001_base_00/outer");
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
                            if(isCanUseAbility && !isAvoidingCapture) {
                                isAvoidingCapture = true;
                                photonView.RPC(nameof(Protect),RpcTarget.All);
                                PhotonMatchMaker.SetCustomProperty("c", false, 0);
                            }else {
                                resultWinLoseText.text = "捕まった！";
                                GameEnd(0); // ゲーム終了.
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

    [PunRPC]
    private void Protect() {
        if(GoToChooseChara.GetPlayMode() == 0) {
            emitter.Play(EffectDatabase.avilityEffects[2]);
            SE.CallAvilitySE(4); // SE.
            muliciaCoat.SetActive(false); // コートを脱ぐ.
        }
    }
}