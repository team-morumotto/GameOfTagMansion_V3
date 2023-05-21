/*
    2022/12/29 Atsuki Kobayashi
*/
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using Smile_waya.GOM.ScreenTimer;
using UnityEngine.Serialization;
using System;

public class PlayerEscape : PlayerBase {
    //----------- Private変数 -----------//
    private ScreenTimer ST = new ScreenTimer();            // プレイヤーの機能をまとめたクラス.
    private Text resultWLText;             // リザルトパネルの勝敗テキスト.
    private Text resultWinLoseText;        // リザルトの勝敗.    
    private GameObject offScreen;          // ほかプレイヤーの位置を示すマーカーを管理するオブジェクト.
    //----------- 変数宣言終了 -----------//
    void Start () {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator> ();
        SE = GameObject.Find("Obj_SE").GetComponent<Button_SE>();
        BGM = GameObject.Find("BGM").GetComponent<BGM_Script>();
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>(); // タグから CinemaChineManager オブジェクト用 MainCamera を取得

        var DuringUI = GameObject.Find(GAMECANVAS).transform.Find("Panel_DuringGameUI");
        countDownText = DuringUI.transform.Find("Text_Time").GetComponent<Text>();

        resultPanel = GameObject.Find(GAMECANVAS).transform.Find("Panel_ResultList").transform.gameObject;
        resultWinLoseText = resultPanel.transform.Find("Result_TextBox").GetComponent<Text>();
        var resultScoreTable =  resultPanel.transform.Find("Text_ScoreTable").transform.gameObject;
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
                // 地面に接している.
                if(isGround){
                    PlayerMove();
                }
                Sneak();
                PlayNumber();
                UseItem();

                /* 【Debug】
                if(Input.GetKeyDown(KeyCode.Z)) {
                    PlayerSpawn(); // キャラクターのスポーン処理.
                    charaState = CharaState.カウントダウン;
                }*/

                if(PhotonMatchMaker.GameStartFlg) {
                    PlayerSpawn(); // キャラクターのスポーン処理.
                    charaState = CharaState.カウントダウン;
                }
            break;

            case CharaState.カウントダウン:
                anim.SetFloat("DashSpeed", 0.0f); // アニメーションストップ.
                anim.SetFloat("Speed", 0.0f);     // アニメーションストップ.

                // カウントダウン.
                if(isGameStart_CountDown) {
                    StartCoroutine(GameStartCountDown());
                }
            break;

            case CharaState.ゲーム中:
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

        switch(charaState) {
            case CharaState.ゲーム開始前:
                var hashTable = new ExitGames.Client.Photon.Hashtable();
                hashTable["c"] = false; // プレイヤーカスタムプロパティの捕まったフラグを初期化.
                GetComponent<PhotonView>().Owner.SetCustomProperties(hashTable);
            break;

            case CharaState.ゲーム中:
                var a = (PhotonNetwork.LocalPlayer.CustomProperties["c"] is bool value) ? value : false; // 捕まったかどうかのプレイヤーカスタムプロパティを取得.
                if(a) {
                    resultWinLoseText.text = "You Lose...";
                    resultWLText.text = "捕まった！";
                    GameEnd(false); // ゲーム終了.
                }
            break;
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
    /// ゲームの制限時間カウント.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    private void GameTimer() {
        var gameTime =ST.GameTimeCounter();

        // テキストへ残り時間を表示
        countDownText.text = gameTime.gameTimeStr;

        // 残り時間が5秒以下なら.
        if(gameTime.gameTimeInt <= 5000) {
            countDownText.color = Color.red; // 赤色に指定.
        }

        // 時間切れになったら.
        if(gameTime.gameTimeInt <= 0){
            resultWinLoseText.text = "You Win!";
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