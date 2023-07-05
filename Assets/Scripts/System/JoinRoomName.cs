/*
    公開ルームに参加するボタンのスクリプト.

    2022/12/27 Atsuki Kobayashi
*/

using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomName : MonoBehaviourPunCallbacks
{
    private GameObject parent;         // このボタンの最上位親オブジェクトを取得する.
    private GameObject joinPanel;      // ルームリストを表示するパネル.
    private GameObject bgPanel;
    private Transform ListPanel;
    private PhotonMatchMaker PMM = new PhotonMatchMaker();

    void Start() {
        parent = GameObject.Find("Canvas_Main");                                // 最上位親オブジェクトを取得.
        joinPanel = GameObject.Find("Panel_RoomMatch");                          // ルームリストを表示するパネルを取得.
        ListPanel = joinPanel.transform.Find("Panel_RoomList").transform;
        bgPanel = parent.transform.Find("Panel_Background").transform.gameObject;
    }

    public void OnClick() {
        var objName = gameObject.name;      // ボタンの名前を取得.
        print("【Debug】: 公開ルームに参加");
        PMM.loadPanel.SetActive(true);
        PhotonNetwork.JoinRoom(objName);    // ルームに参加.
    }

    public override void OnJoinedRoom() {
        print("【Debug】: ルームに参加しました");
        PMM.loadPanel.SetActive(false);
        PanelBlind();
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        PanelBlind();
        PMM.gameErrorPanel.SetActive(true);
        var MatchErrorText = PMM.gameErrorPanel.transform.Find("Text_ErrorCode").GetComponent<Text>();
        MatchErrorText.text = "エラーコード :\n【" + returnCode + "】\nによりルームの参加に失敗しました。";
    }

    // 
    private void PanelBlind() {
        joinPanel.SetActive(false);         // ルームリストを非表示にする.
        bgPanel.SetActive(false);
    }
}