/*
    Created by Atsuki Kobayashi
*/
using UnityEngine;
using Smile_waya.GOM.ScreenTimer;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerEscape : PlayerBase {
    protected CharacterPerformance cp = new CharacterPerformance();
    protected int isHit = 0; // デバッグ用.
    protected string fps = "";
    private ScreenTimer ST = new ScreenTimer(); // プレイヤーの機能をまとめたクラス.

    /*if(Input.GetKeyDown(KeyCode.I)) {
        if(performance != null) {
            if(!isUseAvility) {
                print("能力使用");
                abilityUseAmount--; // 使用可能回数-1.
                isUseAvility = true; // 使用中.
            }
        }else{
            Debug.LogError("能力がセットされていません");
        }
    }*/

    // トラス用スクリプトが無いので保留
    /*
    [PunRPC]
    protected void FireObstruct(PhotonMessageInfo info) {
        if(info.Sender.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
            instanceObstructItem = Instantiate(obstructItem, transform.position + (-transform.forward * 2), transform.rotation); // リストに追加.
        }else {
            Instantiate(obstructItem, transform.position + (-transform.forward * 2), transform.rotation); // リストに追加.
        }
    }
    */

    /// <summary>
    /// 機能 : LeftShiftを押すとスニークを切り替え.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    protected void Sneak() {
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
    protected void GameTimer() {
        var gameTime = ST.GameTimeCounter();

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

    /// <summary>
    /// UGUI表示[デバッグ用]
    /// </summary>
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
                // HitObstruct();
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
                // スタミナの回復量のブースト.
                case "hb":
                    staminaHealAmount += float.Parse(tmpValue.ToString());
                    print("StaminaBoost");
                break;
                /*
                case "et": TargetShow(true); break; // 逃げのカーソルを表示.
                case "ct": TargetShow(false); break; // 鬼のカーソルを表示.
                */

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
    /// </summary>
    /// <param name="newPlayer">入室してきたプレイヤー</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        print("enter");
        Invoke("GetPlayers",1.0f); // 入室直後はキャラクターが生成されていないため遅延させる.
    }

    /// <summary>
    /// ルームからプレイヤーが退出した時.
    /// </summary>
    /// <param name="otherPlayer">退出したプレイヤー</param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        print("left");
        Invoke("GetPlayers",1.0f); // 入室直後はキャラクターが生成されていないため遅延させる.
    }
}