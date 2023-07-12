/*
*   Created by Kobayashi Atsuki

*   == 派生クラスの[PunRPC]について補足. ==
*   関数:キャラクター名ES()は自分が「鬼キャラ」だった場合「逃げ用のスクリプト」を削除するという働きをする.
*   [PunRPC]で定義された関数は自環境の自分と他環境の自分で同じ動作をさせるためのもの.
*   photonViewコンポーネントがアタッチされたゲームオブジェクトにアタッチしたスクリプトからでないと動作しないので、派生クラスごとに定義している.
*   https://zenn.dev/o8que/books/bdcb9af27bdd7d/viewer/2e3520
*   ==

*   == 定期処理について補足 ==
*   Initは派生先のStartで動かす.
*   BaseUpdateは派生先のUpdateで動かす.
*   また、BaseUpdateはトラスとリルモワのみoverrideにて上書きしているため、BaseUpdateを編集する場合は個別スクリプトにて書き換えが必要.
*/
using UnityEngine;
using System.Collections.Generic;
using Smile_waya.GOM.ScreenTimer;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using Cinemachine;
using Effekseer;

public class PlayerEscape : PlayerBase {
    //----------- public 変数 -----------//
    [Tooltip("カメラが注視するオブジェクト")]
    [SerializeField]
    public Transform lookat;

    //----------- private 変数 -----------//
    private ScreenTimer ST = new ScreenTimer(); // プレイヤーの機能をまとめたクラス.
    private GameObject offScreen; // ほかプレイヤーの位置を示すマーカーを管理するオブジェクト.
    private float sneakSpeed = 2.5f;   // スニーク状態のスピード.
    //----------- 変数宣言終了 -----------//

    protected void Init() {
        StartCoroutine(GetPlayers(1.0f));
        //====== オブジェクトやコンポーネントの取得 ======//
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        emitter = GetComponent<EffekseerEmitter>();
        SE = GameObject.Find("Obj_SE").GetComponent<Button_SE>(); // SEコンポーネント取得.
        BGM = GameObject.Find("BGM").GetComponent<BGM_Script>(); // BGMコンポーネント取得.
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>(); // カメラ取得.

        var mainCanvas = GameObject.Find(GAMECANVAS); // MainCanvas取得.

        //====== Panel_DuringGameUI下のオブジェクト ======//
        var DuringUI = mainCanvas.transform.Find("Panel_DuringGameUI"); // ゲーム中の状況表示UI取得.
        gameTimer = DuringUI.transform.Find("Text_Time").GetComponent<Text>(); // 残り時間テキスト取得.
        staminaParent = DuringUI.transform.Find("Group_Stamina").gameObject;
        staminaGuage = staminaParent.transform.Find("Image_Gauge").GetComponent<Image>();
        SeenBy = DuringUI.transform.Find("Image_SeenBy").GetComponent<Image>();
        var itemParent = DuringUI.Find("Group_Item");
        haveItemImageList.Add(itemParent.transform.Find("Image_Item1BG").transform.Find("Image_Item1").GetComponent<Image>());
        // アイテム複数持ちが可能なキャラなら.
        if(isAddhaveItem) {
            var tmpItem2 = itemParent.transform.Find("Image_Item2BG").gameObject;
            tmpItem2.SetActive(true);
            haveItemImageList.Add(tmpItem2.transform.Find("Image_Item2").GetComponent<Image>());
        }

        var recastParent = DuringUI.transform.Find("Group_Avility").gameObject;
        if(isFrequency) {
            var rimining = recastParent.transform.Find("Image_UseLimited").gameObject;
            avilityRiminingAmount = rimining.transform.Find("Text_UseAvilityAmount").GetComponent<Text>();
            avilityRiminingAmount.text = abilityUseAmount.ToString();
        }else {
            var recast = recastParent.transform.Find("Image_Recast").gameObject;
            recast.SetActive(true);
            avilityRecastAmount = recast.GetComponent<Image>();
            avilityRecastAmount.fillAmount = 0.0f;
            avilityRiminingAmount = recast.transform.Find("Text_UseAvilityAmount").GetComponent<Text>();
        }
        avilityImage = recastParent.transform.Find("Image_Avility").GetComponent<Image>();

        SeenBy.color = new Color(255, 255, 255, 0); // 非表示に.
        staminaParent.SetActive(false);
        //====== Panel_DuringGameUI下のオブジェクト ======//

        resultPanel = mainCanvas.transform.Find("Panel_ResultList").transform.gameObject;
        resultWinLoseText = resultPanel.transform.Find("Result_TextBox").GetComponent<Text>();

        var Target = GetComponent<Target>(); // 位置カーソルコンポーネント取得.
        Target.enabled = false; // 自分のカーソルを非表示に.

        itemDatabase = GameObject.Find("ItemList").GetComponent<ItemDatabase>();
        EffectDatabase = GameObject.Find("EffectList").GetComponent<EffectDatabase>();

        offScreen = mainCanvas.transform.Find("Panel_OffScreenIndicator").gameObject;

        PhotonMatchMaker.SetCustomProperty("c", false, 0); // 捕まったフラグを初期化.

        var cf = GameObject.Find("Vcam").GetComponent<CinemachineFreeLook>();
        cf.enabled = true;
        cf.Follow = this.transform;
        cf.LookAt = this.lookat;

        characterNumber = (int)character; // キャラクターの番号.

        //====== オブジェクトやコンポーネントの取得 ======//
    }

    /// <summary>
    /// Update処理のベース.
    /// 上書き予定なので仮想関数として定義.
    /// </summary>
    protected virtual void BaseUpdate() {
        // 自分のキャラクターでなければ処理をしない
        if(!photonView.IsMine) {
            return;
        }

        switch(gameState) {
            case GameState.ゲーム開始前:
                // 地面に接していてスタンしていない.
                if(!isStan && isGround && isCanUseMovement) {
                    PlayerMove();
                    Sneak();
                }
                ItemUse();
                PlayNumber();

                if(PhotonMatchMaker.GameStartFlg) {
                    PlayerSpawn(); // キャラクターのスポーン処理.
                    StartCoroutine(GameStartCountDown()); // カウントダウン開始
                    gameState = GameState.カウントダウン;
                }
            break;

            case GameState.カウントダウン:
                anim.SetFloat("DashSpeed", 0.0f); // アニメーションストップ.
                anim.SetFloat("Speed", 0.0f);     // アニメーションストップ.
            break;

            case GameState.ゲーム中:
                // 地面に接していてスタンしていない.
                if(!isStan && isGround && isCanUseMovement) {
                    PlayerMove();
                    Sneak();
                }
                ItemUse();
                GameTimer();
                CharaPositionReset();
            break;
        }
    }

    /// <summary>
    /// 機能 : プレイヤーの移動制御.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    protected virtual void PlayerMove() {
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
            if(nowStamina > 0 && Input.GetKey(KeyCode.LeftShift) && !isStaminaLoss) {
                // スタミナ無限でないなら.
                if(!isCanUseDash) {
                    nowStamina -= 0.1f;  // スタミナ減少.
                }

                if(nowStamina < 0) {
                    nowStamina = 0;  // スタミナはオーバーフローしない.
                    isStaminaLoss = true; // スタミナ切れに.
                    staminaGuage.color = new Color(255, 0, 0); // 赤.
                }

                if(isSlow) {
                    MoveType(moveForward, runSpeed * 0.8f, 1.5f,inputHorizontal,inputVertical);
                }else {
                    MoveType(moveForward, runSpeed, 1.5f,inputHorizontal,inputVertical);
                }
            }else {
                if(isSlow) {
                    MoveType(moveForward, walkSpeed * 0.8f, 1.0f,inputHorizontal,inputVertical);
                }else {
                    MoveType(moveForward, walkSpeed, 1.0f,inputHorizontal,inputVertical);
                }
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

    /// <summary>
    /// 機能 : LeftControlを押すとスニークを切り替え.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    protected void Sneak() {
        switch(isSneak) {
            case true:
                if(Input.GetKeyDown(KeyCode.LeftControl)) {
                    anim.SetBool("Sneak", false);
                    isSneak = false; // スニークフラグOFF.

                    PhotonMatchMaker.SetCustomProperty("h", false, 0);
                }
            break;

            case false:
                if(Input.GetKeyDown(KeyCode.LeftControl)) {
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
    protected void GameTimer() {
        if(isDebug) {
            return;
        }
        var gameTime = ST.GameTimeCounter();

        // テキストへ残り時間を表示
        gameTimer.text = gameTime.gameTimeStr;

        // 残り時間が5秒以下なら.
        if(gameTime.gameTimeInt <= 5000) {
            gameTimer.color = Color.red; // 赤色に指定.
        }else {
            if(!chaserTarget) {
                GameEnd(2);
                resultWinLoseText.text = "エラーが発生しました。";     //テキストを表示.
            }
        }

        // 時間切れになったら.
        if(gameTime.gameTimeInt <= 0){
            resultWinLoseText.text = "逃げ切った！";// テキストを表示.
            GameEnd(1);                             // ゲーム終了処理.
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

        // 当たったオブジェクトが障害物なら.
        if(collider.CompareTag("Obstruct")) {
            // すでにスタンしているなら処理しない.
            if(isStan) {
                print("スタン済み");
                return;
            }
            isHit++;
            Destroy(collider.gameObject); // 破棄.
            StartCoroutine(Stan());
        }

        // 当たったオブジェクトが御札なら.
        if(collider.CompareTag("Bill")) {
            if(!isSlow) {
                isSlow = true;
                StartCoroutine(DelayChangeFlg("Slow"));
            }
        }
    }
    //--------------- ここまでコリジョン ---------------//

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
                case "on":
                    if(isFrequency) {
                        abilityUseAmount = 3; //! マジックナンバー.
                        avilityRiminingAmount.text = abilityUseAmount.ToString();
                    }else {
                        avilityRecastAmount.fillAmount = 0.0f;
                    }

                    //== 各種パラメータの初期化 ==//
                    isUseAvility = false;
                    isCoolTime = false;
                    isCanUseMovement = true;
                    isCanUseAbility = true;
                    isStan = false;
                    anim.SetBool("Stan", false);
                    anim.SetBool("HookShot", false);
                    nowStamina = staminaAmount;
                    isStaminaLoss = false;
                    //== 各種パラメータの初期化 ==//

                    EffekseerSystem.StopAllEffects();
                    StopAllCoroutines();

                    // 所持アイテムリセット.
                    haveItemList = new List<ItemName>();
                    haveItemImageList[0].sprite = itemDatabase.emptySprite;
                    if(isAddhaveItem) {
                        haveItemImageList[1].sprite = itemDatabase.emptySprite;
                    }
                break;
                // スタミナの回復量のブースト.
                case "hb":
                    staminaHealAmount += float.Parse(tmpValue.ToString());
                    SE.CallAvilitySE(6); // SE.
                    healBoostEffectCoroutine = StartCoroutine(TimeEffectLoop(EffectDatabase.avilityEffects[6], 99999)); // コルーチンを発動とともに格納.
                    print("StaminaHealBoostRoom");
                break;
                case "et":
                    if((bool)tmpValue) {
                        print("aa");
                        SeenBy.color = new Color(255, 255, 255, 255);
                    }else {
                        SeenBy.color = new Color(255, 255, 255, 0);
                    }
                break;
                case "ct":
                    if((bool)tmpValue) {
                        print("bb");
                        StartCoroutine(TargetShow(true));
                    }
                break; // 逃げのカーソルを表示.

                case "ab":
                    if(isRoomPropatiesUpdater){
                        isRoomPropatiesUpdater = false;
                    }else {
                        if(!isCanUseAbility) {
                            return;
                        }
                        if(characterNumber == 9) {
                            amplification = 0; // アイテムの効果増幅無し.
                        }else if(characterNumber == 10) {
                            StopCoroutine(healBoostEffectCoroutine); // スタミナ回復ブーストのエフェクトをストップ.
                            staminaHealAmount -= HealBoostAmount;
                        }

                        SE.CallItemSE(1); // SE.
                        isCanUseAbility = false;
                        StartCoroutine(DelayChangeFlg("CanUseAbility"));
                    }
                break;

                case "mb":
                    if(isRoomPropatiesUpdater){
                        isRoomPropatiesUpdater = false;
                    }else {
                        if(!isCanUseMovement) {
                            return;
                        }
                        anim.SetBool("Stan", true);
                        SE.CallItemSE(1); // SE.
                        isCanUseMovement = false;
                        StartCoroutine(DelayChangeFlg("CanUseMovement"));
                    }
                break;

                //--- 随時追加 ---//
                default:
                    Debug.LogError("想定されていないキー【" + tmpKey + "】です");
                break;
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
        StartCoroutine(GetPlayers(2.0f)); // 入室直後はキャラクターが生成されていないため遅延させる.
    }

    /// <summary>
    /// ルームからプレイヤーが退出した時.
    /// </summary>
    /// <param name="otherPlayer">退出したプレイヤー</param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        StartCoroutine(GetPlayers(2.0f)); // 入室直後はキャラクターが生成されていないため遅延させる.
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
                    // 無敵状態でないなら.
                    if((bool)prop.Value) {
                        if(!isInvincible) {
                            resultWinLoseText.text = "捕まった！";
                            GameEnd(0); // ゲーム終了.
                        }else {
                            PhotonMatchMaker.SetCustomProperty("c", false, 0); // 捕まったフラグを初期化.
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