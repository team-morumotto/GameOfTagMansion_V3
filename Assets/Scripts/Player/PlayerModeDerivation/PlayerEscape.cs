/*
    Created by Atsuki Kobayashi
*/
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Smile_waya.GOM.ScreenTimer;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Cinemachine;

public class PlayerEscape : CharacterPerformance {
    [Tooltip("カメラが注視するオブジェクト")]
    [SerializeField]
    public Transform lookat;
    //----------- Private変数 -----------//
    private GameObject offScreen; // ほかプレイヤーの位置を示すマーカーを管理するオブジェクト.
    private ScreenTimer ST = new ScreenTimer(); // プレイヤーの機能をまとめたクラス.
    private float sneakSpeed = 2.5f;   // スニーク状態のスピード.

    //----------- 変数宣言終了 -----------//

    void Start() {
        GetPlayers();
        if(photonView.IsMine) {
            //====== オブジェクトやコンポーネントの取得 ======//
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            SE = GameObject.Find("Obj_SE").GetComponent<Button_SE>(); // SEコンポーネント取得.
            BGM = GameObject.Find("BGM").GetComponent<BGM_Script>(); // BGMコンポーネント取得.
            playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>(); // カメラ取得.

            var mainCanvas = GameObject.Find(GAMECANVAS); // MainCanvas取得.

            var DuringUI = mainCanvas.transform.Find("Panel_DuringGameUI"); // ゲーム中の状況表示UI取得.
            gameTimer = DuringUI.transform.Find("Text_Time").GetComponent<Text>(); // 残り時間テキスト取得.
            staminaParent = DuringUI.transform.Find("Group_Stamina").gameObject;
            staminaGuage = staminaParent.transform.Find("Image_Gauge").GetComponent<Image>();
            staminaParent.SetActive(false);

            var resultPanel = mainCanvas.transform.Find("Panel_ResultList").transform.gameObject;
            resultWinLoseText = resultPanel.transform.Find("Result_TextBox").GetComponent<Text>();

            var Target = GetComponent<Target>(); // 位置カーソルコンポーネント取得.
            Target.enabled = false; // 非表示に.

            itemDatabase = GameObject.Find("ItemList").GetComponent<ItemDatabase>();

            offScreen = mainCanvas.transform.Find("Panel_OffScreenIndicator").gameObject;

            PhotonMatchMaker.SetCustomProperty("c", false, 0); // 捕まったフラグを初期化.

            var cf = GameObject.Find("Vcam").GetComponent<CinemachineFreeLook>();
            cf.enabled = true;
            cf.Follow = this.transform;
            cf.LookAt = this.lookat;

            //====== オブジェクトやコンポーネントの取得 ======//
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        StatusGet(); // ステータスの取得.

        characterNumber = (int)character; // キャラクターの番号.
    }

    string fps = "";

    void Update () {
        // 自分のキャラクターでなければ処理をしない
        if(!photonView.IsMine) {
            return;
        }

        fps = (1.0f / Time.deltaTime).ToString();

        // Tolassの場合.
        if(characterNumber == 0) {
            if(Input.GetKeyDown(KeyCode.G)) {
                photonView.RPC(nameof(FireObstruct), RpcTarget.All);
            }
        }

        if(characterNumber == 2) {
            if(Input.GetKeyDown(KeyCode.H)) {
                MikagamiKoyomiAbility();
            }
        }

        switch(gameState) {
            case GameState.ゲーム開始前:
                // 地面に接している.
                if(!isStan) {
                    if(isGround){
                        PlayerMove();
                    }
                }
                Sneak();
                PlayNumber();

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
                    // Nayuの場合.
                    if(characterNumber == 9) {
                        StaminaHealBoost(); // スタミナ回復量をブーストする.
                    }
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
    /// 機能 : プレイヤーの移動制御.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    public void PlayerMove() {
        //プレイヤーの向きを変える
        var inputHorizontal = Input.GetAxis("Horizontal"); // 入力デバイスの水平軸.
        var inputVertical = Input.GetAxis("Vertical");     // 入力デバイスの垂直軸.

        if(inputHorizontal == 0 && inputVertical == 0) {
            anim.SetFloat("Speed", 0f); // 移動していないので0.
            StaminaHeal();
        }
        else{
            Vector3 cameraForward = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;// カメラの向きを取得
            Vector3 moveForward = cameraForward * inputVertical + playerCamera.transform.right * inputHorizontal;  // カメラの向きに合わせて移動方向を決定

            // スタミナが残っていて走っている.
            if(nowStamina > 0 && Input.GetKey(KeyCode.LeftControl) && !isStaminaLoss) {
                nowStamina -= 0.1f;  // スタミナ減少.
                if(nowStamina < 0) {
                    nowStamina = 0;  // スタミナはオーバーフローしない.
                    isStaminaLoss = true; // スタミナ切れに.
                }

                photonView.RPC(nameof(IsRunningChange), RpcTarget.All, true);
                MoveType(moveForward, runSpeed, 1.5f);
            }else {
                photonView.RPC(nameof(IsRunningChange), RpcTarget.All, false);
                MoveType(moveForward, walkSpeed, 1.0f);
                StaminaHeal();
            }

            // カメラの向きが0でなければプレイヤーの向きをカメラの向きにする.
            if (moveForward != Vector3.zero) {
                transform.rotation = Quaternion.LookRotation(moveForward);
            }
        }

        // 走っているときはスタミナUI表示.
        if(nowStamina < staminaAmount && !staminaParent.activeSelf) {
            staminaParent.SetActive(true);
        }

        staminaGuage.fillAmount = nowStamina / staminaAmount; // 残りのスタミナをUIに反映.
    }

    [PunRPC]
    private void IsRunningChange(bool value) {
        isRunning = value;
    }

    [PunRPC]
    private void FireObstruct(PhotonMessageInfo info) {
        if(info.Sender.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
            instanceObstructItem = Instantiate(obstructItem, transform.position + (-transform.forward * 2), transform.rotation); // リストに追加.
            #if UNITY_EDITOR
                print("自分が生成した");
            #endif
        }else {
            Instantiate(obstructItem, transform.position + (-transform.forward * 2), transform.rotation); // リストに追加.
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

    int isHit = 0; // デバッグ用.

    void OnTriggerEnter(Collider collider) {
        // 当たったオブジェクトが障害物なら.
        if(collider.CompareTag("Obstruct")) {
            if(instanceObstructItem != collider.gameObject) {
                // すでにスタンしているなら処理しない.
                if(isStan) {
                    print("スタン済み");
                    return;
                }

                // 自分で生成した障害物でないなら.
                isHit++;
                Destroy(collider.gameObject); // 破壊.
                HitObstruct();
            }
        }
    }
    //--------------- ここまでコリジョン ---------------//

    ///<summary> UGUI表示 </summary>
    void OnGUI(){
        if(!photonView.IsMine) {
            return;
        }
        GUIStyle style = new GUIStyle();
        style.fontSize = 100;
        GUI.Label(new Rect(100, 100, 300, 300), "velocity:" + rb.velocity.ToString(), style);
        GUI.Label(new Rect(100, 200, 300, 300), "deltaTime:" + Time.deltaTime.ToString(), style);
        GUI.Label(new Rect(100, 300, 300, 300), "flameLate:" + fps.ToString(), style);
        GUI.Label(new Rect(100, 400, 300, 300), "isHit:" + isHit.ToString(), style);
    }

    //--------------- フォトンのコールバック ---------------//
    /// <summary>
    /// ルームのカスタムプロパティが変更された場合.
    /// </summary>
    /// <param name="propertiesThatChanged">変更されたカスタムプロパティ</param>
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        // 自分でない場合.
        if(!photonView.IsMine) {
            return;
        }
        foreach(var property in propertiesThatChanged){
            var tmpKey = property.Key.ToString(); // Key.
            var tmpValue = property.Value; // Value.

            // Keyで照合;
            switch(tmpKey) {
                // スタミナの回復量のブースト.
                case "hb":
                    staminaHealAmount += float.Parse(tmpValue.ToString());
                    print("StaminaBoost");
                break;

                default:
                    Debug.LogError("想定されていないキー【" + tmpKey + "】です");
                break;

                //--- 随時追加 ---//
            }
        }

        print("ルームプロパティ書き換え");
    }

    /// <summary>
    /// ルームにプレイヤーが入室してきたときのコールバック関数.
    /// 引数 : newPlayer.
    /// 戻り値 : なし.
    /// </summary>
    /// <param name="newPlayer">入室してきたプレイヤー</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Invoke("GetPlayers",1.0f); // 入室直後はキャラクターが生成されていないため遅延させる.
    }
}