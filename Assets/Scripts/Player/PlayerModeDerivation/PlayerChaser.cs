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

public class PlayerChaser : PlayerBase
{
    [Tooltip("捕まえたキャラクターの表示")]
    [SerializeField]
    protected Text catch_text; //捕まえたプレイヤー名を表示するUI.

    [Tooltip("カメラが注視するオブジェクト")]
    [SerializeField]
    public Transform lookat;

    protected string fps = "";

    //----------- Private 変数 -----------//
    private ScreenTimer ST = new ScreenTimer();
    private CinemachineFreeLook cf; // CinemaCHineFreeLook.
    //----------- 変数宣言終了 -----------//

    /*if(Input.GetKeyDown(KeyCode.I)) {
        if(performance != null) {
            if(!isUseAvility) {
                abilityUseAmount--; // 使用可能回数-1.
                print("能力使用");
                isUseAvility = true;
            }
        }else{
            Debug.LogError("能力がセットされていません");
        }
    }*/

    // トラス用スクリプトが無いので保留
    /*// Tolassの場合.
    if(characterNumber == 0) {
        if(Input.GetKeyDown(KeyCode.G)) {
            photonView.RPC(nameof(FireObstruct), RpcTarget.All);
        }
    }

    [PunRPC]
    private void FireObstruct() {
        Instantiate(obstructItem, transform.position + new Vector3(0, 0, 2), transform.rotation); // リストに追加.
    }
    */



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
                // HitObstruct();
            }
        }
    }
    //--------------- ここまでコリジョン ---------------//

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
    /// 引数 : newPlayer.
    /// 戻り値 : なし.
    /// </summary>
    /// <param name="newPlayer">入室してきたプレイヤー</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Invoke("GetPlayers",1.0f); // 入室直後はキャラクターが生成されていないため遅延させる.
    }

    /// <summary>
    /// ルームからプレイヤーが退出した時.
    /// </summary>
    /// <param name="otherPlayer">退出したプレイヤー</param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Invoke("GetPlayers",1.0f); // 入室直後はキャラクターが生成されていないため遅延させる.
    }
}