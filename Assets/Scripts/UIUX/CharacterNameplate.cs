/*### CREATOR #####

2022/10/18 - Edit and modify by Rikuto Kashiwaya.
##### ｺｺﾏﾃﾞCREATOR ###*/

/*### README #####
PhotonNetwork.LocalPlayer.NickName ... ローカルプレイヤーのニックネーム
PhotonNetwork.NickName             ... プレイヤーのニックネーム情報格納
photonView.Owner.NickName          ... プレイヤーのニックネームを表示

Powered by トマシープが学ぶ Unity/VR/AR が好きなミーハー人間のメモ
https://bibinbaleo.hatenablog.com/entry/2019/09/06/131024
##### ｺｺﾏﾃﾞREADME ###*/

//2023/01/13 Atsuki Kobayashi

using UnityEngine;
using Photon.Pun;
using TMPro;

public class CharacterNameplate : MonoBehaviourPunCallbacks {
    private TextMeshProUGUI NickName;  // プレイヤーの名前.
    private Camera nameCamera;         // このオブジェクトを映すカメラ.
    void Start(){
        NickName = GetComponentInChildren<TextMeshProUGUI>();
        nameCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
    }

    void Update() {
        NickName.text = $"{photonView.Owner.NickName}";                       // PhotonNetwork.LocalPlayer.NickNameの値を公開.
        GetComponent<RectTransform>().transform.LookAt(nameCamera.transform); // 誰から見ても常に自分の名前が正面に見えるようにする.
    }
}