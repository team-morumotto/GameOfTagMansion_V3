/*
    2022/12/29 Atsuki Kobayashi

    --参考サイト--https://enia.hatenablog.com/entry/unity/introduction/20
                --https://zenn.dev/o8que/books/bdcb9af27bdd7d
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System;
using ExitGames.Client.Photon;

public class PhotonMatchMaker : MonoBehaviourPunCallbacks
{
    //------------ 定数 ------------//
    private const string GAMECANVAS = "Canvas_Main";
    //------------ static ------------//
    public GameObject gameDuringPanel;                                                               // ゲーム中のUIパネル.
    public GameObject gameLobbyPanel;                                                                // ゲーム中のボタンUIパネル.
    public GameObject gameErrorPanel;                                                                // ルームの作成/参加に失敗した際のエラー表示パネル.
    /* sneakUIとuseItemUIは緊急措置.
        本来は別のスクリプトで管理するべきなので今後変更予定.*/
    public GameObject sneakUI;
    public GameObject useItemUI;
    public static bool GameStartFlg = false;                                                         // ゲーム開始フラグ.

    //------------ public ------------//
    private string[] instantedRoom = new string[300];                                                // 生成済みのルームリスト(300個までルームのボタン生成が可能).
    private List<string> roomListName = new List<string>();                                          // ルームリストの各ルームの名前.
    [FormerlySerializedAs("before")] public GameObject[] OniObject = {null,null,null};	             // 鬼オブジェクト.
    [FormerlySerializedAs("before")] public GameObject[] PlayerObject = {null,null,null};            // プレイヤーオブジェクト.
    [FormerlySerializedAs("before")] public GameObject[] SpawnPoint;						         // キャラクタースポーンポイント.
    [FormerlySerializedAs("before")] public GameObject Instant;                                      // ルームリストのボタン.
    public GameObject BGPanel;
    public Transform roomScroll;                                                                     // ルームリストのスクロールビュー.
    public InputField inputCreateRoomName;                                                           // 作成するルーム名を入力するInputField.
    public InputField inputJoinRoomName;                                                             // 参加する非公開ルーム名を保存するInputField.
    public GameObject cursol;

    //------------ private ------------//
    private RoomList roomList = new RoomList();             // RoomListClassのインスタンスを生成.
    private Toggle roomVisible;
    private Text playerCountText;                            // ルーム内の逃げの残り人数.

    // int型変数
    private int number;										// 鬼側か逃げる側かを識別するナンバー.

    private int beforePlayers = 0;                          // 前フレームのルームの人数を格納する変数.

    // bool型変数
    private bool isConnect = true;                          // マスターサーバーに接続したか.
    private bool isStanby = false;                          // 待機中かどうか.
    private bool isCanStart = false;                        // ゲームスタートが可能な状態か.
    private bool isJoinRoom = false;                        // ルームに参加したかどうか.
    private bool isVisible = false;                         // 作成したルームが非公開可どうか.

    // string型変数
    private string createRoomName = "";                     // 作成するルーム名を保存する変数.
    private string joinRoomName = "";                       // 参加するルーム名を保存する変数.

    // ValueTuple変数
    ValueTuple<List<string>, List<int>, List<int>, List<bool>> roomData = new ValueTuple<List<string>, List<int>, List<int>, List<bool>>(); // RoomListクラスで更新されるルームのパラメータを格納する変数.

    void Start() {
        isConnect = true;       // フォトン接続フラグを初期化.
        GameStartFlg = false;   // ゲームスタートフラグを初期化.
        isJoinRoom = false;     // ルーム参加フラグを初期化.
        // 逃げなら.
        if(GoToChooseChara.GetPlayMode() == 0) {
            sneakUI.SetActive(true);
        }else{
            useItemUI.transform.position = sneakUI.transform.position;
            sneakUI.SetActive(false);
        }
    }

    void Update() {
        // 使用するキャラクターが選択された段階でフォトンに接続する関数を1回のみ動かす
        if(GoToChooseChara.GetIsEdit() && isConnect) {
            isConnect = false;
            Connect();
            print("Connecting");
        }

        // ルームに参加したなら.
        if(isJoinRoom) {
            // ESCキーを押したら.
            if(Input.GetKeyDown(KeyCode.Escape)) {
                MenuPanelBlind();
            }

            // 自分がマスタークライアントなら.
            if(PhotonNetwork.LocalPlayer.IsMasterClient) {
                // Eキーを押したら.
                if(Input.GetKeyDown(KeyCode.E)) {
                    if(GameStartError()) {
                        var HashTable = new ExitGames.Client.Photon.Hashtable();            // ハッシュテーブルを生成.
                        HashTable["on"] = true;                                             // ルームカスタムプロパティのonにtrueを代入.
                        PhotonNetwork.CurrentRoom.SetCustomProperties(HashTable);           // ルームのカスタムプロパティを更新.
                    }
                }
            }
        }
    }

    // ゲームスタートができるかどうか.
    private bool GameStartError() {
        var chasers = GameObject.FindGameObjectsWithTag("Oni");
        var walningText = gameLobbyPanel.transform.Find("Text_Caution").GetComponent<Text>();
        // 鬼が1人かつルームの人数がルームの最大参加人数と同じなら.
        if(PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers) {
            if(chasers.Length == 1){
                walningText.text = "";
                return true;
            }else{
                walningText.text = "【注意】鬼の数が一人ではありません。";
                return false;
            }
        }else{
            walningText.text = "【注意】ルームの参加人数が足りません。";
            return false;
        }
    }

    // メニューの表示/非表示.
    private void MenuPanelBlind() {
        // 鬼か逃げか.
        switch(GoToChooseChara.GetPlayMode()) {
            // 逃げなら.
            case 0:
                // メニューが表示されているなら.
                if(Player_Escape.isMenuOn) {
                    Player_Escape.isMenuOn = false;
                    gameLobbyPanel.SetActive(false);
                }else{
                    Player_Escape.isMenuOn = true;
                    gameLobbyPanel.SetActive(true);
                }
            break;

            // 鬼なら.
            case 1:
                // メニューが表示されているなら.
                if(Player_Chaser.isMenuOn) {
                    Player_Chaser.isMenuOn = false;
                    gameLobbyPanel.SetActive(false);
                }else{
                    Player_Chaser.isMenuOn = true;
                    gameLobbyPanel.SetActive(true);
                }
            break;
        }
    }

	// フォトンのマスターサーバーに接続.
    private void Connect() {
        number =GoToChooseChara.GetPlayMode();  // GotoChooseCharaから鬼か逃げる側かを識別するナンバーを受け取る.
        PhotonNetwork.ConnectUsingSettings();	// Photonネットワークへの接続処理部分(これがないとPhotonは使用できない). // ConnectedMaster()コールバック関数が呼ばれる
    }

    /// <summary>
    /// 機能 : 生成したキャラクターに追従カメラを設定する
    /// 仮引数：CinemachineFreeLookコンポーネント,メインカメラ,生成したキャラクター
    /// 戻り値：なし
    /// </summary>
    private void CreateCharacter(GameObject Character) {
        PivotColliderController.m_end = Character.transform.Find("LookAtObject").transform;                                     // プレイヤーのtransform.
        PivotColliderController.cameraPos = Character.transform.Find("Reset_CameraPosition").transform;                         // プレイヤーカメラのリセット位置オブジェクト.
        CameraRotator3rdPersonPov.playerObject = Character.transform.Find("LookAtObject").gameObject;                           // プレイヤーのオブジェクト.
    }

    /// <summary>
    /// 機能 : ルーム作成/参加時のパネルの表示非表示を切り替え.
    /// 仮引数 : 呼び出し側関数により値が変更される.CreatRoom関数→Panel_CreateRoom,JoinRoom関数→Panel_JoinRoom.
    /// 戻り値 : なし
    /// </summary>
    private void PanelBlind(GameObject panel) {
        panel.SetActive(false);                                         // 非表示.
        BGPanel.SetActive(false);                                       // BackGroundパネルを非表示.
        cursol.SetActive(false);                                        // カーソルを非表示.
        gameDuringPanel.SetActive(true);                                // DuringGameUIパネル表示.
    }

    //-------------------- フォトンのコールバック関数 --------------------//
    //この関数たちはこのスクリプト内で明示的に呼び出すことはありません//

    // ルームのカスタムプロパティが更新された場合.
    public override void OnRoomPropertiesUpdate(Hashtable roomProperties) {
        // ルームプロパティのonが更新された場合.
        if(roomProperties.ContainsKey("on")) {
            print("【Debug】 : ルームプロパティのonが更新されました");
            // ルームプロパティのonがtrueならゲーム開始.
            if((bool)roomProperties["on"] == true) {
                GameStartFlg = true;
                gameLobbyPanel.SetActive(false);
                Player_Chaser.isMenuOn = false; // 逃げのメニューを非表示.
                Player_Escape.isMenuOn = false; // 鬼のメニューを非表示.
            }
        }
    }

    
    public override void OnMasterClientSwitched(Player newMasterClient) {
        if(!GameStartFlg) {
            gameErrorPanel.SetActive(true); // エラー表示パネルを表示.
            var MatchErrorText = gameErrorPanel.transform.Find("Text_ErrorCode").GetComponent<Text>();
            MatchErrorText.text = "ホストが退出しました。\nルームが解体されました。";
        }
    }

    // ルームのリストが更新された場合.
    public override void OnRoomListUpdate(List<RoomInfo> changedRoomList) {
        print("【Debug】 : OnRoomListUpdate");

        roomData = new ValueTuple<List<string>, List<int>, List<int>, List<bool>>();                   // roomDataを初期化.
        roomData = roomList.Update(changedRoomList);                            // 更新されたルームの一覧を更新.
    }

    // マスターサーバーに接続した時.
    public override void OnConnectedToMaster() {
        print("【Debug】 : マスターサーバに接続した");
        PhotonNetwork.JoinLobby();	// ロビーへの入室処理.
    }

    // ロビーに入室した場合.
    public override void OnJoinedLobby() {
        print("【Debug】 ロビー入室");
        Invoke("ListUpdate" , 1.0f);            // GameSceneがロードされてから1秒後にListUpdate関数を実行.
    }

    // マスターサーバーへの接続に失敗した/切断した場合.
    public override void OnDisconnected(DisconnectCause cause) {
        print("【Debug】サーバー非接続状態 : "+cause);
    }

    // ルームから退室した場合.
    public override void OnLeftRoom()
    {
        print("【Debug】ルームから退室");
    }

    // ルームの作成に失敗した場合.
    /*
        エラーコード.
        32766 : 同じルーム名のルームが存在する.
    */
    public override void OnCreateRoomFailed(short returnCode, string message) {
        gameErrorPanel.SetActive(true); // エラー表示パネルを表示.
        var MatchErrorText = gameErrorPanel.transform.Find("Text_ErrorCode").GetComponent<Text>();
        MatchErrorText.text = "エラーコード :\n【" + returnCode + "】\nによりルームの作成に失敗しました。";
    }

	// ルームに参加できなかった場合.
    /*
        エラーコード.
        32758 : ルームが存在しない.
    */
    public override void OnJoinRoomFailed(short returnCode, string message) {
        gameErrorPanel.SetActive(true);
        var MatchErrorText = gameErrorPanel.transform.Find("Text_ErrorCode").GetComponent<Text>();
        MatchErrorText.text = "エラーコード :\n【" + returnCode + "】\nによりルームの参加に失敗しました。";
    }

	// フォトンのルームに参加したときに呼ばれます.
    public override void OnJoinedRoom() {
        isJoinRoom = true;              // ルームに参加した.
        gameDuringPanel.SetActive(true);
        gameLobbyPanel.SetActive(false); // ゲームメニューを非表示.

        // ルーム作成時のトグルがtrueなら.
        if(isVisible) {
            PhotonNetwork.CurrentRoom.IsVisible = false; // ルームを非公開に.
        }

        float x = UnityEngine.Random.Range(SpawnPoint[0].transform.position.x, SpawnPoint[1].transform.position.x);
        float y = UnityEngine.Random.Range(SpawnPoint[0].transform.position.y, SpawnPoint[1].transform.position.y);
        float z = UnityEngine.Random.Range(SpawnPoint[0].transform.position.z, SpawnPoint[1].transform.position.z);
        Vector3 spawnPos = new Vector3(x, y, z);                                                                        // プレイヤーの生成位置をLobby内のランダムな位置に決定.

        var hashTable = new ExitGames.Client.Photon.Hashtable();
        hashTable["gs"] = 1;
        hashTable["h"] = false;                                     // カスタムプロパティのハイドをfalseに設定.
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashTable);   // プレイヤープロパティをセット.

        switch(GoToChooseChara.GetPlayMode()) {
            case 0:
                    GameObject Player = PhotonNetwork.Instantiate(PlayerObject[GoToChooseChara.GetCharacters()].name,spawnPos,Quaternion.identity,0);// 逃げキャラを生成.
                    CreateCharacter(Player);
                break;
            case 1:
                    GameObject Oni = PhotonNetwork.Instantiate(OniObject[GoToChooseChara.GetCharacters()].name,spawnPos,Quaternion.identity,0);// 鬼キャラを生成.
                    CreateCharacter(Oni);
                break;
            default:
                    Debug.LogError("【Debug】 : キャラタイプ" + number + "は存在しません");     // 0,1以外は想定外の値なのでエラーを出す.
                break;
        }
        GameStartFlg = false;
    }
    //-------------------- フォトンのコールバック関数 --------------------//

    //----------- ボタン -----------//
    /// <summary>
    /// マスターサーバから切断し、Closed_GameSceneをリロードする。
    /// </summary>
    public void GameSceneReload() {
        isJoinRoom = false;
        PhotonNetwork.Disconnect();                                         // マスターサーバから切断.
        SceneManager.LoadScene("Closed_GameScene",LoadSceneMode.Single);    // ゲームシーンに遷移.
    }

    // ルームを作成する.
    public void CreateRoom(GameObject panel) {
        if(createRoomName == "" || createRoomName == null) {
            return;
        }
        var tmpObj = GameObject.Find(GAMECANVAS).transform.Find("Panel_CreateRoom");
        var Toggle = tmpObj.transform.Find("Toggle_PrivateMode").GetComponent<Toggle>();
        isVisible = Toggle.isOn;

        RoomOptions roomOptions = new RoomOptions();	                // RoomOptionをインスタンス化.
        roomOptions.MaxPlayers = (byte)RoomPlayerSet.GamePlayers;       // ルームの最大人数を設定.

        PhotonNetwork.CreateRoom(createRoomName, roomOptions);	        // ルームを作成.

        var hashTable = new ExitGames.Client.Photon.Hashtable();
        hashTable["gs"] = 1;                                            // ルームを作成した段階で準備完了状態にする.
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashTable);

        PanelBlind(panel);
    }

    // 非公開ルームに参加する.
    public void JoinPrivateRoom(GameObject panel) {
        if(joinRoomName == "" || joinRoomName == null) {
            return;
        }

        PhotonNetwork.JoinRoom(joinRoomName);                                       // ルームに参加.

        PanelBlind(panel);
    }

    /*
        公開ルームリストを更新する.
    */
    public void ListUpdate() {
        print("【Debug】 : ListUpdate");
        roomListName = roomData.Item1;        // 更新されたルームの名前を取得.
        var roomMemberCount = roomData.Item2; // 更新されたルームの人数を取得.
        var roomMaxPlayers = roomData.Item3;  // 更新されたルームの最大参加人数を取得.
        var roomIsVisible = roomData.Item4;   // 更新されたルームの公開状況を取得.

        if(roomListName.Count > 0) {
            for(int i = 0; i < roomListName.Count; i++) {
                print("[Debug] : " + roomListName[i] + " : " + roomMemberCount[i] + "人");
                // roomListNameの街灯番目がnullなら次の要素へ.
                if(roomListName[i] == null || roomListName[i] == "") {
                    print("[Debug] : null Or Space");
                    continue;
                }

                // すでにインスタンス化されているかチェック.
                for(int j = 0; j < instantedRoom.Length; j++) {
                    if(roomListName[i] == instantedRoom[j]) {
                        print("[Debug] : すでにボタンが生成されている");
                        goto NEXT; // 生成処理をスキップ.
                    }
                }

                // 公開ルームへの参加ボタンの生成処理.
                if(roomMemberCount[i] >= 1) {
                    // ルームの参加人数が最大人数と同じなら.
                    if(roomMemberCount[i] == roomMaxPlayers[i]) {
                        print("【Debug】: ルームの人数が最大なのでボタンを生成しません");
                        goto NEXT;
                    }

                    if(roomIsVisible[i] == false) {
                        print("【Debug】 : ルームが非公開なのでボタンの生成をしません");
                        goto NEXT;
                    }else {
                        for(int j = 0; j < instantedRoom.Length; j++) {
                            if(instantedRoom[j] == null || instantedRoom[j] == "") {
                                GameObject Room = Instantiate(Instant, roomScroll);             // ルーム参加ボタンを生成.
                                Room.GetComponentInChildren<Text>().text = roomListName[i];     // ルーム参加ボタンのテキストをルーム名にする.
                                Room.name = roomListName[i];                                    // ルーム参加ボタンのオブジェクト名をルーム名にする.
                                instantedRoom[j] = roomListName[i];                             // 生成されたルームのボタンををリストに追加.
                                print("[Debug] : " + roomListName[i] + "のボタンを生成しました");
                                break;
                            }
                        }
                    }
                }

                NEXT:;
            }
        }
    }

    // 「タイトルに戻る」ボタン押下時.
    public void GoToTitleScene() {
        // DontDestroy要素を初期化.
        GoToChooseChara.PlayMode = 0;
        GoToChooseChara.Characters = 0;
        GoToChooseChara.actorNumber = -1;

        PhotonNetwork.Disconnect();                                         // マスターサーバから切断.
        SceneManager.LoadScene("Closed_TitleScene",LoadSceneMode.Single);   // タイトルシーンに遷移.
    }
    //----------- ボタン -----------//

    //--------- InputField ---------//
    // 作成するルームの名前を保存
    public void SetCreateRoomName() {
        createRoomName = inputCreateRoomName.text;
    }

    // 参加する非公開ルームの名前
    public void SetJoinRoomName() {
        joinRoomName = inputJoinRoomName.text;
    }
    //--------- InputField ---------//
}