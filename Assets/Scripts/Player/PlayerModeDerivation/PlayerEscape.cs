/*
    2022/12/29 Atsuki Kobayashi
*/
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Smile_waya.GOM.ScreenTimer;
using Photon.Realtime;

public class PlayerEscape : PlayerBase {

    //----------- protected変数 -----------//
    private GameObject offScreen; // ほかプレイヤーの位置を示すマーカーを管理するオブジェクト.
    //----------- Private変数 -----------//
    private ScreenTimer ST = new ScreenTimer(); // プレイヤーの機能をまとめたクラス.
    private float sneakSpeed = 2.5f;   // スニーク状態のスピード.
    //----------- 変数宣言終了 -----------//

    void Start() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        SE = GameObject.Find("Obj_SE").GetComponent<Button_SE>(); // SEコンポーネント取得.
        BGM = GameObject.Find("BGM").GetComponent<BGM_Script>(); // BGMコンポーネント取得.
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>(); // カメラ取得.
        particleSystem = playerCamera.transform.Find("Particle System").gameObject.GetComponent<ParticleSystem>();

        var mainCanvas = GameObject.Find(GAMECANVAS);

        var DuringUI = mainCanvas.transform.Find("Panel_DuringGameUI"); // ゲーム中の状況表示UI取得.
        gameTimer = DuringUI.transform.Find("Text_Time").GetComponent<Text>(); // 残り時間テキスト取得.
        staminaParent = DuringUI.transform.Find("Group_Stamina").gameObject;
        staminaGuage = staminaParent.transform.Find("Image_Gauge").GetComponent<Image>();

        var resultPanel = mainCanvas.transform.Find("Panel_ResultList").transform.gameObject;
        resultWinLoseText = resultPanel.transform.Find("Result_TextBox").GetComponent<Text>();

        var Target = GetComponent<Target>(); // 位置カーソルコンポーネント取得.
        Target.enabled = false; // 非表示に.

        characterDatabase = GameObject.Find("CharacterStatusLoad").GetComponent<CharacterDatabase>();
        StatusGet();

        offScreen = mainCanvas.transform.Find("Panel_OffScreenIndicator").gameObject;

        PhotonMatchMaker.SetCustomProperty("c", false, 0); // 捕まったフラグを初期化.
    }

    void Update () {
        // 自分のキャラクターでなければ処理をしない
        if(!photonView.IsMine) {
            return;
        }

        switch(gameState) {
            case GameState.ゲーム開始前:
                // 地面に接している.
                if(isGround){
                    PlayerMove();
                }
                Sneak();
                PlayNumber();
                UseItem();

                /*if(Input.GetKeyDown(KeyCode.Z)) {
                    PlayerSpawn(); // キャラクターのスポーン処理.
                    gameState = GameState.カウントダウン;
                }*/

                if(PhotonMatchMaker.GameStartFlg) {
                    PlayerSpawn(); // キャラクターのスポーン処理.
                    gameState = GameState.カウントダウン;
                }
            break;

            case GameState.カウントダウン:
                anim.SetFloat("DashSpeed", 0.0f); // アニメーションストップ.
                anim.SetFloat("Speed", 0.0f);     // アニメーションストップ.

                // カウントダウン.
                if(isGameStarted) {
                    StartCoroutine(GameStartCountDown());
                }
            break;

            case GameState.ゲーム中:
                // 地面に接している.
                if(isGround){
                    PlayerMove();
                }
                GameTimer();
                Sneak();
                UseItem();
                CharaPositionReset();
            break;
        }
    }

    //定期処理
    void FixedUpdate() {
        // 自分でない場合 or カウントダウンが終了していない場合は処理を行わない
        if(!photonView.IsMine) {
            return;
        }

        if(gameState == GameState.ゲーム中) {
            var a = (PhotonNetwork.LocalPlayer.CustomProperties["c"] is bool value) ? value : false; // 捕まったかどうかのプレイヤーカスタムプロパティを取得.
            if(a) {
                resultWLText.text = "捕まった！";
                GameEnd(false); // ゲーム終了.
            }
        }
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

                    PhotonMatchMaker.SetCustomProperty("h", false, 0);
                }
            break;

            case false:
                if(Input.GetKeyDown(KeyCode.LeftShift)) {
                    anim.SetBool("Sneak", true);
                    isSneak = true; // スニークフラグON.

                    PhotonMatchMaker.SetCustomProperty("h", true, 0);
            }
            break;
        }
    }

    /// <summary>
    /// ゲームの制限時間カウント.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    private void GameTimer() {
        var gameTime =ST.GameTimeCounter();

        // テキストへ残り時間を表示
        gameTimer.text = gameTime.gameTimeStr;

        // 残り時間が5秒以下なら.
        if(gameTime.gameTimeInt <= 5000) {
            gameTimer.color = Color.red; // 赤色に指定.
        }

        // 時間切れになったら.
        if(gameTime.gameTimeInt <= 0){
            resultWLText.text = "逃げ切った！";         //テキストを表示
            GameEnd(true);                             //ゲーム終了処理
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(PhotonNetwork.LocalPlayer == targetPlayer) {
            print(targetPlayer);
        }
        print(targetPlayer);
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
}