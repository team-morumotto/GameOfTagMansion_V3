/*
    Created by Atsuki Kobayashi
*/
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Smile_waya.GOM.ScreenTimer;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerChaser : CharacterPerformance
{
    //------------ Public変数 ------------//
    [Tooltip("捕まえたキャラクターの表示")]
    public Text catch_text; //捕まえたプレイヤー名を表示するUI.

    //----------- Private 変数 -----------//
    private ScreenTimer ST = new ScreenTimer();
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
        particleSystem = playerCamera.transform.Find("Particle System").gameObject.GetComponent<ParticleSystem>();

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
        //====== オブジェクトやコンポーネントの取得 ======//
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        StatusGet(); // ステータスの取得.

        catch_text.enabled = false; // 非表示に.
    }

    void Update() {
        // 自分のキャラクターでなければ処理をしない
        if(!photonView.IsMine) {
            return;
        }

        // Tolassの場合.
        if(characterNumber == 0) {
            if(Input.GetKeyDown(KeyCode.G)) {
                instancedObstruct = PhotonNetwork.Instantiate("FrontObstructItem", transform.position + transform.forward * 2.0f , transform.rotation);
            }
        }

        switch(gameState) {
            case GameState.ゲーム開始前:
                // 地面に接している.
                if(isGround){
                    PlayerMove();
                }
                PlayNumber();

                /* 【Debug】
                if(Input.GetKeyDown(KeyCode.Z)) {
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
                    // キャラクターがナユの場合自分のみ回復力ブースト.
                    if((int)character == 9) {
                        staminaHealAmount += HealBoostAmount;
                    }
                    StartCoroutine(GameStartCountDown());
                }
            break;

            case GameState.ゲーム中:
                // 地面に接している.
                if(isGround){
                    PlayerMove();
                }
                UseItem();
                GameTimer();
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

        GetPlayersPos(this.transform.position);
        SneakEscapes();
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
                particleSystem.Play();     //パーティクルシステムをスタート
            }else {
                photonView.RPC(nameof(IsRunningChange), RpcTarget.All, false);
                MoveType(moveForward, walkSpeed, 1.0f);
                particleSystem.Stop();     //パーティクルシステムをストップ
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

    /// <summary>
    /// ルームに参加している逃げキャラのスニーク状態を取得する.
    /// </summary>
    private void SneakEscapes() {
        /*int j = 0;
        // 逃げが0人でないなら.
        if(players.Length != 0) {
            foreach(var player in players){
                bool isHide = (PhotonNetwork.PlayerList[i].CustomProperties["h"]is bool value) ? value : false;
                // 該当の逃げキャラがしゃがみ状態なら.
                if(isHide) {
                    players[j].GetComponent<Target>().enabled = false; // TargetスクリプトをOFFにする.
                }else{
                    players[j].GetComponent<Target>().enabled = true;
                }
                j++;
            }
        }*/
    }

    /// <summary>
    /// ゲームの制限時間カウント.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    private void GameTimer() {
        var gameTime = ST.GameTimeCounter();

        // テキストへ残り時間を表示
        gameTimer.text = gameTime.gameTimeStr;

        // 残り時間が5秒以下なら.
        if(gameTime.gameTimeInt <= 5000) {
            gameTimer.color = Color.red; // 赤色に指定.
        }

        // 時間切れになったら.
        if(gameTime.gameTimeInt < 0){
            resultWLText.text = "全員捕まえられなかった...";
            GameEnd(false);
        }

        // 時間切れ前に全員捕まえたら.
        if(escapeList.Count == 0) {
            resultWLText.text = "全員捕まえられた！\n" + ("残り時間 : " + gameTime.gameTimeStr);
            GameEnd(true);
        }
    }

    //--------------- コリジョン ---------------//
    private void OnCollisionEnter(Collision collision) {
        // 自分でない場合 or ゲームが開始されていない場合は処理を行わない
        if(!photonView.IsMine || !PhotonMatchMaker.GameStartFlg) {
            return;
        }

        // 接触したオブジェクトにPlayer_Escapeがあるかどうか
        if(collision.gameObject.GetComponent<PlayerEscape>()){
            var hashTable = new ExitGames.Client.Photon.Hashtable();
            hashTable["c"] = true;
            collision.gameObject.GetComponent<PhotonView>().Owner.SetCustomProperties(hashTable);

            catch_text.enabled = true;
            var pName = collision.gameObject.GetComponent<PhotonView>().Owner.NickName;// 接触した逃げキャラのプレイヤー名を取得
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

        int isHit = 0;

    void OnTriggerEnter(Collider collider) {
        if(instancedObstruct != collider.gameObject) {
            if(collider.CompareTag("Obstruct")) {
                isHit++;
                Destroy(collider.gameObject);
                SE.Call_SE(7);
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
        style.fontSize = 200;
        GUI.Label(new Rect(100, 200, 300, 300), isHit.ToString(), style);
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
        var i = 0;
        foreach(var property in propertiesThatChanged){
            var tmpKey = property.Key.ToString(); // Key.
            var tmpValue = property.Value; // Value.
            i++;

            // Keyで照合;
            switch(tmpKey) {
                default:
                    Debug.LogError("想定されていないキー【" + tmpKey + "】です");
                break;

                //--- 随時追加 ---//
            }
        }

        print("ルームプロパティ書き換え回数 : " + i);
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