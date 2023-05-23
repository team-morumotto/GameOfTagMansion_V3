using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Es_Koyomi : PlayerEscape
{
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

        PhotonMatchMaker.SetCustomProperty("c", false, 0); // 捕まったフラグを初期化.

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

                
                /*if(Input.GetKeyDown(KeyCode.Z)) {
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
