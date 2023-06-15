using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Photon.Pun;

public class EscapeNayu : PlayerEscape
{
    [Tooltip("カメラが注視するオブジェクト")]
    [SerializeField]
    public Transform lookat;
    //----------- Private変数 -----------//
    private GameObject offScreen; // ほかプレイヤーの位置を示すマーカーを管理するオブジェクト.
    private float sneakSpeed = 2.5f;   // スニーク状態のスピード.

    //----------- 変数宣言終了 -----------//
    void Start() {
        if(photonView.IsMine) {
            GetPlayers();
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
            Target.enabled = false; // 自分のカーソルを非表示に.

            itemDatabase = GameObject.Find("ItemList").GetComponent<ItemDatabase>();

            offScreen = mainCanvas.transform.Find("Panel_OffScreenIndicator").gameObject;

            PhotonMatchMaker.SetCustomProperty("c", false, 0); // 捕まったフラグを初期化.

            var cf = GameObject.Find("Vcam").GetComponent<CinemachineFreeLook>();
            cf.enabled = true;
            cf.Follow = this.transform;
            cf.LookAt = this.lookat;

            characterNumber = (int)character; // キャラクターの番号.

            //====== オブジェクトやコンポーネントの取得 ======//
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update () {
        // 自分のキャラクターでなければ処理をしない
        if(!photonView.IsMine) {
            return;
        }

        fps = (1.0f / Time.deltaTime).ToString();

        switch(gameState) {
            case GameState.ゲーム開始前:
                // 地面に接している.
                if(!isStan) {
                    if(isGround){
                        PlayerMove();
                        Sneak();
                    }
                }
                PlayNumber();

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
                    Performance();
                }
                    StartCoroutine(GameStartCountDown());
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

    protected override void Performance()
    {
        cp.StaminaHealBoost(); // スタミナ回復量をブーストする.
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
            RegenerativeStaminaHeal();
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
                RegenerativeStaminaHeal();
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
}
