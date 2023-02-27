/*
    2022/12/29 Atsuki Kobayashi
*/
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using Smile_waya.GOM.PF;
using UnityEngine.Serialization;
using System;

public class Player_Escape : MonoBehaviourPunCallbacks {
    //------------ 定数 ------------//
    private const string GAMECANVAS = "/Canvas_Main";
    [Tooltip("ゲームスタートまでのカウントダウン時間")] [FormerlySerializedAs("before")]             public int COUNTDOWN= 5;                   // ゲームスタートまでのカウントダウン
    //------------ Static変数 ------------//
    public static bool isMenuOn{get; set;}   // ゲームロビーでメニューを表示しているかどうか.
    public static bool isHaveItem = false;         // アイテムを取得したかどうか.
    public static bool isUseItem = false;    // アイテムを使用したかどうか.

    //------------ Public変数 ------------//
    [Tooltip("キャラクターのステージのスポーン場所")] [FormerlySerializedAs("before")]               public GameObject[] userSpawnPoint;           // キャラクターのステージスポーン場所.
    [Tooltip("スピードアップアイテムのステージスポーン場所")] [FormerlySerializedAs("before")]       public GameObject[] itemSpawnPoint;         // アイテムのステージスポーン場所.

    //----------- Private 変数 -----------//
    private Player_Function pf = new Player_Function();            // プレイヤーの機能をまとめたクラス.
    private Camera playerCamera;           // プレイヤーを追尾するカメラ.
    private Button_SE SE;
    private BGM_Script BGM;
    private Rigidbody rb;                  // リジッドボディ.
    private Animator anim;                 // アニメーション.
    private Text countDownText;            // タイマー出力用.
    private Text resultWLText;             // リザルトパネルの勝敗テキスト.
    private Text resultWinLoseText;        // リザルトの勝敗.
    private GameObject result_Panel;       // リザルトパネル.
    private GameObject offScreen;          // ほかプレイヤーの位置を示すマーカーを管理するオブジェクト.
    private ParticleSystem particleSystem; // パーティクルシステム.
    public enum CharaState { // ゲームの進行状況.
        ゲーム開始前,
        カウントダウン,
        ゲーム中,
        ゲーム終了
    }
    public CharaState charaState = CharaState.ゲーム開始前; // ゲーム開始前で初期化.

    // int型変数.
    private int isGameStartTimer = 5;  // メンバーが揃ってからゲーム開始までのカウント(初期値は5秒).

    // float型変数.
    private float sneakSpeed = 2.5f;   // スニーク状態のスピード.
    private float walkSpeed = 5.0f;    // 歩く速度.
    private float runSpeed = 10.0f;    // 走る速度.
    private float itemSpawnTime;       // アイテムスポーン時間.

    // bool型変数.
    private bool isGameStart_CountDown = true; //ゲームスタートカウントダウンが終了したかどうか.
    private bool isOnGui = false;              // GUIを表示しているかどうか.
    private bool isGround = true;              // 地面に接地しているかどうか.
    private bool isSneak = false;              // スニーク状態かどうか.
    private bool isInstantedItem = true;       // アイテムを生成したかどうか.

    private ValueTuple<string, int> gameTime = new ValueTuple<string, int>();
    //----------- 変数宣言終了 -----------//
    void Start () {

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator> ();
        SE = GameObject.Find("Obj_SE").GetComponent<Button_SE>();
        BGM = GameObject.Find("BGM").GetComponent<BGM_Script>();
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>(); // タグから CinemaChineManager オブジェクト用 MainCamera を取得

        var DuringUI = GameObject.Find(GAMECANVAS).transform.Find("Panel_DuringGameUI");
        countDownText = DuringUI.transform.Find("Text_Time").GetComponent<Text>();

        result_Panel = GameObject.Find(GAMECANVAS).transform.Find("Panel_ResultList").transform.gameObject;
        resultWinLoseText = result_Panel.transform.Find("Result_TextBox").GetComponent<Text>();
        var resultScoreTable =  result_Panel.transform.Find("Text_ScoreTable").transform.gameObject;
        resultWLText = resultScoreTable.transform.Find("Score_TextBox").gameObject.GetComponentInChildren<Text>();

        offScreen = GameObject.Find(GAMECANVAS).transform.Find("Panel_OffScreenIndicator").gameObject;
        particleSystem = playerCamera.transform.Find("Particle System").gameObject.GetComponent<ParticleSystem>();
        isMenuOn = false;

        var Target = GetComponent<Target>();
        Target.enabled = false;
    }

    void Update () {
        // 自分のキャラクターでなければ処理をしない
        if(!photonView.IsMine) {
            return;
        }

        switch(charaState) {
            case CharaState.ゲーム開始前:

                Sneak();
                PlayNumber();
                UseItem();

                if(PhotonMatchMaker.GameStartFlg) {
                    PlayerSpawn(); // キャラクターのスポーン処理.
                    charaState = CharaState.カウントダウン;
                }
            break;

            case CharaState.カウントダウン:
                anim.SetFloat("DashSpeed", 0.0f); // アニメーションストップ.
                anim.SetFloat("Speed", 0.0f);     // アニメーションストップ.
                // ゲームスタート前のカウントダウンのCoroutineを実行
                if(isGameStart_CountDown) {
                    StartCoroutine("GameStartCountDown");
                }
            break;

            case CharaState.ゲーム中:
                UseItem();
                GameTimer();

                Sneak();

                // 自分がマスタークライアント、アイテムの生成をできるなら.
                if(PhotonNetwork.LocalPlayer.IsMasterClient && isInstantedItem) {
                    StartCoroutine("ItemSpawn");
                }

                // ステージ外に落ちたときy座標が-100以下になったら自分のスポーン位置に戻る.
                if(gameObject.transform.position.y <= -300f) {
                    PlayerSpawn();
                }
            break;
        }
    }

    //定期処理
    void FixedUpdate() {
        // 自分でない場合 or カウントダウンが終了していない場合は処理を行わない
        if(!photonView.IsMine) {
            return;
        }

        switch(charaState) {
            case CharaState.ゲーム開始前:
                var hashTable = new ExitGames.Client.Photon.Hashtable();
                hashTable["c"] = false; // プレイヤーカスタムプロパティの捕まったフラグを初期化.
                GetComponent<PhotonView>().Owner.SetCustomProperties(hashTable);
                // 地面に接していてかつメニューを開いていなければ.
                if(isGround && !isMenuOn){
                    PlayerMove();
                }
            break;

            case CharaState.ゲーム中:
                // 地面に接していてかつメニューを開いていなければ.
                if(isGround && !isMenuOn){
                    PlayerMove();
                }

                var a = (PhotonNetwork.LocalPlayer.CustomProperties["c"] is bool value) ? value : false; // 捕まったかどうかのプレイヤーカスタムプロパティを取得.
                if(a){
                    resultWinLoseText.text = "You Lose...";
                    resultWLText.text = "捕まった！";
                    GameEnd(false); // ゲーム終了.
                }
            break;
        }
    }

    //--------------- コリジョン ---------------//
    void OnCollisionEnter(Collision collision) {
        // 自分でない場合 or ゲームが開始されていない場合は処理を行わない
        if(!photonView.IsMine || !PhotonMatchMaker.GameStartFlg) {
            return;
        }

        if(collision.gameObject.tag == "Floor") {
            isGround = true;
        }
    }

    void OnCollisionStay(Collision collision) {
        // 自分でない場合 or ゲームが開始されていない場合は処理を行わない
        if(!photonView.IsMine || !PhotonMatchMaker.GameStartFlg) {
            return;
        }

        if(collision.gameObject.tag == "Floor") {
            isGround = true;
        }
    }

    void OnCollisionExit(Collision collision) {
        // 自分でない場合 or ゲームが開始されていない場合は処理を行わない
        if(!photonView.IsMine || !PhotonMatchMaker.GameStartFlg) {
            return;
        }

        if(collision.gameObject.tag == "Floor") {
            isGround = false;
        }
    }

    void OnTriggerEnter(Collider collider) {
        if(!photonView.IsMine) {
            return;
        }

        if(collider.gameObject.tag == "Item") {
            // スピードアップ状態を発動
            isHaveItem = true;
            SE.Call_SE(2);
        }
    }
    //--------------- ここまでコリジョン ---------------//

    /// <summary>
    /// 機能 : プレイヤーの移動制御.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    private void PlayerMove() {
        //プレイヤーの向きを変える
            var inputHorizontal = Input.GetAxis("Horizontal"); // 入力デバイスの水平軸をhで定義
            var inputVertical = Input.GetAxis("Vertical");     // 入力デバイスの垂直軸をvで定義

            if(isMenuOn) {
                inputHorizontal = 0;
                inputVertical = 0;
            }

            if(inputHorizontal == 0 && inputVertical == 0) {
                anim.SetFloat("Speed", 0f);                   //プレイヤーが移動してないときは走るアニメーションを止める
            }
            else{
                //スピードアップアイテムを取得しているときは走る速度を上げる
                if(isUseItem) {
                    particleSystem.Play();     //パーティクルシステムをスタート
                    anim.SetFloat("DashSpeed", 1.5f); //プレイヤーが移動しているときは走るアニメーションを再生する
                    anim.SetFloat("Speed", 1.0f); //プレイヤーが移動しているときは走るアニメーションを再生する
                }else{
                    particleSystem.Stop();     //パーティクルシステムをストップ
                    anim.SetFloat("DashSpeed", 1.0f); //プレイヤーが移動しているときは走るアニメーションを再生する
                    anim.SetFloat("Speed", 1.0f); //プレイヤーが移動しているときは走るアニメーションを再生する
                }
            }
            Vector3 cameraForward = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;// カメラの向きを取得
            Vector3 moveForward = cameraForward * inputVertical + playerCamera.transform.right * inputHorizontal;  // カメラの向きに合わせて移動方向を決定

            float nowspeed; // プレイヤーの移動速度

            // スピードアップアイテムを取得したかどうか
            if(isSneak) {
                nowspeed = sneakSpeed; // スニーク状態なら.
            }
            else if(isUseItem) {
                nowspeed = runSpeed;  // 取得しているなら走る
            }
            else {
                nowspeed = walkSpeed; // 取得していないなら歩く
            }

            // プレイヤーの移動処理
            rb.velocity = moveForward * nowspeed + new Vector3(0, rb.velocity.y, 0);

            // カメラの向きが0でなければプレイヤーの向きをカメラの向きにする
            if (moveForward != Vector3.zero) {
                transform.rotation = Quaternion.LookRotation(moveForward);
            }
    }

    /// <summary>
    /// ゲーム終了.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    /// <param name="isWin">勝敗</param>
    private void GameEnd(bool isWin) {
        //勝った場合
        if(isWin) {
            SE.Call_SE(5);
        }
        //負けた場合
        else{
            SE.Call_SE(6);
        }
        result_Panel.SetActive(true);                  //パネルを表示
        countDownText.text = ("00:00.000").ToString(); //残り時間を0に上書きし表示
        PhotonNetwork.Destroy(gameObject);             //自分を全体から破棄
        PhotonNetwork.Disconnect();                    //ルームから退出
    }

    /// <summary>
    /// キャラクターがステージに移動.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    private void PlayerSpawn() {
        var actor = GoToChooseChara.actorNumber;//ルームに入ってきたプレイヤーの入室順番号を入手
        // 各プレイヤーの入室順番号によってスポーンポイントを変更
        transform.position = userSpawnPoint[actor - 1].transform.position;
    }

    /// <summary>
    /// 機能 : LeftShiftを押すとスニークを切り替え.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    private void Sneak() {
        switch(isSneak) {
            case true:
                if(Input.GetKeyDown(KeyCode.LeftShift)) {
                    anim.SetBool("Sneak", false);
                    isSneak = false; // スニークフラグOFF.

                    var hashTable = new ExitGames.Client.Photon.Hashtable();
                    hashTable["h"] = false;                                    // しゃがんでいる.
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hashTable);
                }
            break;

            case false:
                if(Input.GetKeyDown(KeyCode.LeftShift)) {
                    anim.SetBool("Sneak", true);
                    isSneak = true; // スニークフラグON.

                    var hashTable = new ExitGames.Client.Photon.Hashtable();
                    hashTable["h"] = true;                                     // しゃがんでない.
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hashTable);
            }
            break;
        }
    }

    /// <summary>
    /// ルームに参加した際に、自動的に番号が割り振られる.
    /// ルームからほかプレイヤーが退出すると番号は詰められる.
    /// 戻り値 : なし.
    /// 引数 : なし.
    /// </summary>
    private void PlayNumber() {
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++) {
            // ルームに参加した順に数字が割り当てられる.
            if(PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer) {
                PhotonNetwork.LocalPlayer.NickName = $"{"Player"}({i + 1})";
                GoToChooseChara.actorNumber = i + 1; // 添字+1をすることで、スポーン位置の決定.
            }
        }
    }

    /// <summary>
    /// ゲームの制限時間カウント.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    private void GameTimer() {
        gameTime = pf.GameTimeCounter();

        // 残り時間が5秒以下なら.
        if(gameTime.Item2 <= 5000) {
            countDownText.color = Color.red; // 赤色に指定.
        }

        // 残り時間が0秒以下なら.
        if(gameTime.Item2 <= 0){
            resultWinLoseText.text = "You Win!";
            resultWLText.text = "逃げ切った！";         //テキストを表示
            GameEnd(true);                                 //ゲーム終了処理
        }

        // テキストへ残り時間を表示
        countDownText.text = gameTime.Item1;
    }

    /// <summary>
    /// スピードアップアイテムを使用する.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    private void UseItem() {
        // マウスの右ボタンを押した && アイテムを持っている && スニーク状態でないなら.
        if(Input.GetMouseButton(1) && isHaveItem && !isSneak) {
            // ChangeSpeedコルーチンを発動
            StartCoroutine("ChangeSpeed");
            isUseItem = true;
        }
    }

    ///<summary> UGUI表示 </summary>
    private void OnGUI() {
        if(!photonView.IsMine) return;                                                      // 自分でない場合は処理を行わない.

        var controlStyle = new GUIStyle(GUI.skin.label) { fontSize = 30 };                  // フォントサイズを設定.
        var rectY = Screen.height/14;
        // ゲームスタート前のカウントダウンを表示.
        if(isOnGui) {
            controlStyle.fontSize = 300;
            GUI.Label(new Rect(Screen.width/2, rectY, 300, 300), isGameStartTimer.ToString(), controlStyle);
        }
    }

    //--------------- コルーチン（呼び出し専用） ---------------//
    ///<summary>自分がマスタークライアントならば、指定の座標にアイテムを出現させる</summary>
    private IEnumerator ItemSpawn() {
        for(int i = 0; i < itemSpawnPoint.Length ; i++) {
            PhotonNetwork.Instantiate("Item", itemSpawnPoint[i].transform.position, Quaternion.identity);// Resorcesフォルダ内のItemを生成.
        }
        isInstantedItem = false;
        yield return new WaitForSeconds(10);
        isInstantedItem = true;
    }

    ///<summary> スピードアップアイテムを取得したときの処理 </summary>
    private IEnumerator ChangeSpeed() {
        // 5秒間スピードアップ
        yield return new WaitForSeconds(5.0f);
        // スピードアップ状態を解除
        isUseItem = false;
        isHaveItem = false;
    }

    ///<summary>5秒間待ってゲームを開始する</summary>
    private IEnumerator GameStartCountDown() {
        BGM.Call_BGM_Stop();
        SE.Call_SE(3);                          // カウントダウンの音を鳴らす
        isGameStart_CountDown = false;
        //5秒間カウントダウン
        for(isGameStartTimer = COUNTDOWN; isGameStartTimer > 0; isGameStartTimer--) {
            isOnGui = true;                     // OnGuiを有効にする
            yield return new WaitForSeconds(1f);
        }

        isOnGui = false;                        // OnGuiを無効にする

        var BGMObject = GameObject.Find("BGM");
        BGMObject.GetComponent<BGM_Script>().Call_BGM(0);
        charaState = CharaState.ゲーム中;
    }
    //--------------- ここまでコルーチン ---------------//
}