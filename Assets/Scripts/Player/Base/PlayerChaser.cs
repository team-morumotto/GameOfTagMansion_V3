/*
*   Created by Kobayashi Atsuki

*   == 派生クラスの[PunRPC]について補足. ==
*   関数:キャラクター名CS()は自分が「逃げキャラ」だった場合自分の「RedCubeオブジェクトと鬼用のスクリプト」を削除するという働きをする.
*   [PunRPC]で定義された関数は自環境の自分と他環境の自分で同じ動作をさせるためのもの.
*   photonViewコンポーネントがアタッチされたゲームオブジェクトにアタッチしたスクリプトからでないと動作しないので、派生クラスごとに定義している.
*   https://zenn.dev/o8que/books/bdcb9af27bdd7d/viewer/2e3520
*   ==

*   == 定期処理について補足 ==
*   Initは派生先のStartで動かす.
*   BaseUpdateは派生先のUpdateで動かす.
*   また、BaseUpdateはリルモワのみoverrideにて上書きしているため、BaseUpdateを編集する場合は個別スクリプトにて書き換えが必要.
*/
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;
using Smile_waya.GOM.ScreenTimer;
using Photon.Realtime;
using Cinemachine;

public class PlayerChaser : PlayerBase
{
    [Tooltip("カメラが注視するオブジェクト")]
    [SerializeField]
    public Transform lookat;

    [Tooltip("捕まえたキャラクターの表示")]
    [SerializeField]
    protected Text catch_text; //捕まえたプレイヤー名を表示するUI.

    //----------- Private 変数 -----------//
    private ScreenTimer ST = new ScreenTimer();
    //----------- 変数宣言終了 -----------//

    protected void Init() {
        StartCoroutine(GetPlayers(1.0f));
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        SE = GameObject.Find("Obj_SE").GetComponent<Button_SE>(); // SEコンポーネント取得.
        BGM = GameObject.Find("BGM").GetComponent<BGM_Script>(); // BGMコンポーネント取得.
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>(); // カメラ取得.

        var mainCanvas = GameObject.Find(GAMECANVAS); // MainCanvas取得.

        //====== Panel_DuringGameUI下のオブジェクト ======//
        var DuringUI = mainCanvas.transform.Find("Panel_DuringGameUI"); // ゲーム中の状況表示UI取得.
        gameTimer = DuringUI.transform.Find("Text_Time").GetComponent<Text>(); // 残り時間テキスト取得.
        staminaParent = DuringUI.transform.Find("Group_Stamina").gameObject;
        staminaGuage = staminaParent.transform.Find("Image_Gauge").GetComponent<Image>();
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
            rimining.SetActive(true);
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

        catch_text = DuringUI.transform.Find("Text_PlayerCatch").GetComponent<Text>();
        SeenBy = DuringUI.transform.Find("Image_SeenBy").GetComponent<Image>();
        SeenBy.color = new Color(255, 255, 255, 0); // 非表示に.
        staminaParent.SetActive(false);

        resultPanel = mainCanvas.transform.Find("Panel_ResultList").transform.gameObject;
        resultWinLoseText = resultPanel.transform.Find("Result_TextBox").GetComponent<Text>();

        var Target = GetComponent<Target>(); // 位置カーソルコンポーネント取得.
        Target.enabled = false; // 自分のカーソルを非表示に.

        itemDatabase = GameObject.Find("ItemList").GetComponent<ItemDatabase>();

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

        fps = (1.0f / Time.deltaTime).ToString();

        switch(gameState) {
            case GameState.ゲーム開始前:
                // 地面に接していてスタンしていない.
                if(!isStan && isGround) {
                    PlayerMove();
                }
                ItemUse();
                PlayNumber();

                if(PhotonMatchMaker.GameStartFlg) {
                    PlayerSpawn(); // キャラクターのスポーン処理.
                    StartCoroutine(GameStartCountDown()); // カウントダウン開始.
                    gameState = GameState.カウントダウン;
                }
            break;

            case GameState.カウントダウン:
                anim.SetFloat("DashSpeed", 0.0f); // アニメーションストップ.
                anim.SetFloat("Speed", 0.0f);     // アニメーションストップ.
            break;

            case GameState.ゲーム中:
                // 地面に接していてスタンしていない.
                if(!isStan && isGround) {
                    PlayerMove();
                }
                ItemUse();
                GameTimer();
                CharaPositionReset();
            break;
        }
    }

    /// <summary>
    /// 機能 : プレイヤーの移動制御.
    /// 上書き予定なので仮想関数として定義.
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
                    }

                if(isSlow) {
                    MoveType(moveForward, runSpeed * 0.8f, 1.5f,inputHorizontal,inputVertical);
                }else {
                    MoveType(moveForward, runSpeed, 1.5f, inputHorizontal, inputVertical);
                }
            }else {
                if(isSlow) {
                    MoveType(moveForward, walkSpeed * 0.8f, 1.0f,inputHorizontal,inputVertical);
                }else {
                    MoveType(moveForward, walkSpeed, 1.0f, inputHorizontal, inputVertical);
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
        }

        // 時間切れになったら.
        if(gameTime.gameTimeInt < 0){
            resultWinLoseText.text = "全員捕まえられなかった...";
            GameEnd(false);
        }

        // 時間切れ前に全員捕まえたら.
        if(escapeList.Count == 0) {
            resultWinLoseText.text = "全員捕まえられた！\n" + ("残り時間 : " + gameTime.gameTimeStr);
            GameEnd(true);
        }
    }

    //--------------- コリジョン ---------------//
    private void OnCollisionEnter(Collision collision) {
        // 自分でない場合 or ゲームが開始されていない場合は処理を行わない
        if(!photonView.IsMine || !PhotonMatchMaker.GameStartFlg) {
            return;
        }

        // 逃げキャラを捕まえたとき.
        if(collision.gameObject.GetComponent<PlayerEscape>()){
            print("aa");
            var hashTable = new ExitGames.Client.Photon.Hashtable();
            hashTable["c"] = true;
            collision.gameObject.GetComponent<PhotonView>().Owner.SetCustomProperties(hashTable);

            catch_text.enabled = true;
            var pName = collision.gameObject.GetComponent<PhotonView>().Owner.NickName;// 接触した逃げキャラのプレイヤー名を取得
            print(pName);
            catch_text.text = pName + "を捕まえた！";
            SE.Call_SE(1);

            Invoke("EscapeCount",1.0f); // 逃げキャラをカウント.
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
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
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
                    abilityUseAmount = 3; //! マジックナンバー.
                    isUseAvility = false;
                    isCoolTime = false;

                    // 所持アイテムリセット.
                    haveItem = new List<ItemName>();
                    haveItemImageList[0].sprite = itemDatabase.emptySprite;
                    if(isAddhaveItem) {
                        haveItemImageList[1].sprite = itemDatabase.emptySprite;
                    }
                break;
                case "et":
                    if((bool)tmpValue) {
                        print("et");
                        StartCoroutine(TargetShow(false));
                    }
                break; // 逃げのカーソルを表示.
                case "ct":
                    if((bool)tmpValue) {
                        SeenBy.color = new Color(255, 255, 255, 255);
                    }else {
                        SeenBy.color = new Color(255, 255, 255, 0);
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
}