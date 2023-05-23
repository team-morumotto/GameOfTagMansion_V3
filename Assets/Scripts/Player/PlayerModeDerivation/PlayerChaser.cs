/*
    2022/12/29 Atsuki Kobayashi
*/
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using Smile_waya.GOM.ScreenTimer;
using UnityEngine.Serialization;
using Photon.Realtime;

public class PlayerChaser : PlayerBase
{
    //------------ Public変数 ------------//
    [Tooltip("捕まえたときに文字を出力する用（鬼専用）")]
    public Text catch_text;

    //----------- Private 変数 -----------//
    private ScreenTimer ST = new ScreenTimer();
    public GameObject[] players;
    private GUIStyle speedUpStyle;         // スピードアップ中のGUIテキストのスタイル.
    //----------- 変数宣言終了 -----------//

    void Start() {

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        SE = GameObject.Find("Obj_SE").GetComponent<Button_SE>();
        BGM = GameObject.Find("BGM").GetComponent<BGM_Script>();
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>(); // タグから CinemaChineManager オブジェクト用 MainCamera を取得

        var DuringUI = GameObject.Find(GAMECANVAS).transform.Find("Panel_DuringGameUI");
        countDownText = DuringUI.transform.Find("Text_Time").GetComponent<Text>();
        catch_text.enabled = false;

        resultPanel = GameObject.Find(GAMECANVAS).transform.Find("Panel_ResultList").transform.gameObject;
        resultWinLoseText = resultPanel.transform.Find("Result_TextBox").GetComponent<Text>();
        var resultScoreTable =  resultPanel.transform.Find("Text_ScoreTable").transform.gameObject;
        resultWLText = resultScoreTable.transform.Find("Score_TextBox").gameObject.GetComponentInChildren<Text>();

        particleSystem = playerCamera.transform.Find("Particle System").gameObject.GetComponent<ParticleSystem>();

        speedUpStyle = new GUIStyle();
        speedUpStyle.fontSize = 50;
        speedUpStyle.normal.textColor = Color.red;

        var Target = GetComponent<Target>();
        Target.enabled = false;

        EscapeCount();
    }

    void Update() {
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
                PlayNumber();

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
        SneakEscapes();
    }

    /// <summary>
    /// ルームに参加している逃げプレイヤーのスニーク状態を取得する.
    /// </summary>
    private void SneakEscapes() {
        /*int j = 0;
        // 逃げが0人でないなら.
        if(players.Length != 0) {
            foreach(var player in players){
                // 自分以外.
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
    /// ルーム内の逃げプレイヤーの数をカウントする.
    /// </summary>
    private void EscapeCount() {
        players = GameObject.FindGameObjectsWithTag("Nige");
    }

    /// <summary>
    /// ゲームの制限時間カウント.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    private void GameTimer() {
        var gameTime = ST.GameTimeCounter();

        
        // テキストへ残り時間を表示
        countDownText.text = gameTime.gameTimeStr;

        // 残り時間が5秒以下なら.
        if(gameTime.gameTimeInt <= 5000) {
            countDownText.color = Color.red; // 赤色に指定.
        }

        // 時間切れになったら.
        if(gameTime.gameTimeInt < 0){
            resultWinLoseText.text = "You Lose...";
            resultWLText.text = "全員捕まえられなかった...";
            GameEnd(false);
        }

        // 時間切れ前に全員捕まえたら.
        if(players.Length == 0) {
            resultWinLoseText.text = "You Win!";
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

            Invoke("EscapeCount",1.0f); // 逃げプレイヤーをカウント.
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

    private void OnTriggerEnter(Collider collider) {
        // 自分でない場合
        if(!photonView.IsMine) {
            return;
        }

        if(collider.gameObject.tag == "Item") {
            isHaveItem = true;      // アイテムを保持.
            SE.Call_SE(2);
        }
    }
    //--------------- ここまでコリジョン ---------------//


    //--------------- フォトンのコールバック ---------------//
    /// <summary>
    /// ルームにプレイヤーが入室してきたときのコールバック関数.
    /// 引数 : newPlayer. 
    /// 戻り値 : なし.
    /// </summary>
    /// <param name="newPlayer">入室してきたプレイヤー</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Invoke("EscapeCount",1.0f); // 入室直後はキャラクターが生成されていないため遅延させる.
    }
}