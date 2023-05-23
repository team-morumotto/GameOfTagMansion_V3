using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;
public class PlayerBase : MonoBehaviourPunCallbacks
{
    //------------ 定数 ------------//
    public const string GAMECANVAS = "/Canvas_Main"; // Canvas_Mainの取得.
    //------------ Public変数 ------------//
    [Tooltip("キャラクターのステージのスポーン場所")] [FormerlySerializedAs("before")]               public GameObject[] userSpawnPoint;           // キャラクターのステージスポーン場所.
    [Tooltip("スピードアップアイテムのステージスポーン場所")] [FormerlySerializedAs("before")]       public GameObject[] itemSpawnPoint;         // アイテムのステージスポーン場所.
    [Tooltip("ゲームスタートまでのカウントダウン時間")] [FormerlySerializedAs("before")]             public int COUNTDOWN= 5;                   // ゲームスタートまでのカウントダウン
    public CharaState charaState = CharaState.ゲーム開始前; // ゲーム開始前で初期化.
    protected Animator anim;                 // アニメーション.
    protected ParticleSystem particleSystem; // パーティクルシステム.
    protected Camera playerCamera;           // プレイヤーを追尾するカメラ.
    protected Button_SE SE;
    protected BGM_Script BGM;
    protected Text countDownText;            // タイマー出力用.
    protected GameObject resultPanel;        // リザルトパネル.
    protected Text resultWLText;             // リザルトパネルの勝敗テキスト.
    protected Text resultWinLoseText;        // リザルトの勝敗.
    
    protected Rigidbody rb;                  // リジッドボディ.

    //------ bool型変数 ------//
    public static bool isHaveItem = false;   // アイテムを取得したかどうか.
    public static bool isUseItem = false;    // アイテムを使用したかどうか.
    protected bool isGameStart_CountDown = true; //ゲームスタートカウントダウンが終了したかどうか.
    protected bool isOnGui = false;              // GUIを表示しているかどうか.
    protected bool isGround = true;              // 地面に接地しているかどうか.
    protected bool isSneak = false;              // スニーク状態かどうか.

    public enum CharaState { // ゲームの進行状況.
        ゲーム開始前,
        カウントダウン,
        ゲーム中,
        ゲーム終了
    }

    // int型変数.
    protected int isGameStartTimer = 5;
    //----------- float変数 -----------//
    [SerializeField]
    protected float walkSpeed = 10.0f;    // 歩く速度.
    [SerializeField]
    protected float runSpeed = 20.0f;    // 走る速度.
    // float型変数.
    protected float sneakSpeed = 2.5f;   // スニーク状態のスピード.

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
                anim.SetFloat("Speed", 0f);                   //プレイヤーが移動してないときは走るアニメーションを止める
            }
            else{
                //スピードアップアイテムを取得しているときは走る速度を上げる
                if(isUseItem) {
                    particleSystem.Play();     //パーティクルシステムをスタート
                    anim.SetFloat("DashSpeed", 1.5f); //プレイヤーが移動しているときは走るアニメーションを再生する
                }else{
                    particleSystem.Stop();     //パーティクルシステムをストップ
                    anim.SetFloat("DashSpeed", 1.0f); //プレイヤーが移動しているときは走るアニメーションを再生する
                }
                anim.SetFloat("Speed", 1.0f); //プレイヤーが移動しているときは走るアニメーションを再生する
            }
            Vector3 cameraForward = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;// カメラの向きを取得
            Vector3 moveForward = cameraForward * inputVertical + playerCamera.transform.right * inputHorizontal;  // カメラの向きに合わせて移動方向を決定

            float nowspeed; // プレイヤーの移動速度

            if(isSneak) {
                nowspeed = sneakSpeed; // スニーク状態なら.
            }
            else if(isUseItem) {
                nowspeed = runSpeed;  // アイテムを使用しているなら走る
            }
            else {
                nowspeed = walkSpeed; // それ以外なら歩く
            }

            // プレイヤーの移動処理
            rb.velocity = moveForward * nowspeed + new Vector3(0, rb.velocity.y, 0);

            // カメラの向きが0でなければプレイヤーの向きをカメラの向きにする
            if (moveForward != Vector3.zero) {
                transform.rotation = Quaternion.LookRotation(moveForward);
            }
    }
    
    public void CharaPositionReset() {
        // ステージ外に落ちたときy座標が-100以下になったら自分のスポーン位置に戻る.
        if(gameObject.transform.position.y <= -300f) {
            PlayerSpawn();
        }
    }

    /// <summary>
    /// キャラクターがステージに移動.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    public void PlayerSpawn() {
        var actor = GoToChooseChara.actorNumber;//ルームに入ってきたプレイヤーの入室順番号を入手
        // 各プレイヤーの入室順番号によってスポーン位置を変更
        transform.position = userSpawnPoint[actor - 1].transform.position;
    }

    /// <summary>
    /// ルームに参加した際に、自動的に番号が割り振られる.
    /// ルームからほかプレイヤーが退出すると番号は詰められる.
    /// 戻り値 : なし.
    /// 引数 : なし.
    /// </summary>
    public void PlayNumber() {
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++) {
            // ルームに参加した順に数字が割り当てられる.
            if(PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer) {
                PhotonNetwork.LocalPlayer.NickName = $"{"Player"}({i + 1})";
                GoToChooseChara.actorNumber = i + 1; // 添字+1をすることで、スポーン位置の決定.
            }
        }
    }

    /// <summary>
    /// ゲーム終了.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    /// <param name="isWin">勝敗</param>
    public void GameEnd(bool isWin) {
        //勝った場合
        if(isWin) {
            SE.Call_SE(5);
        }
        //負けた場合
        else{
            SE.Call_SE(6);
        }
        resultPanel.SetActive(true);                  //パネルを表示
        countDownText.text = ("00:00.000").ToString(); //残り時間を0に上書きし表示
        PhotonNetwork.Destroy(gameObject);             //自分を全体から破棄
        PhotonNetwork.Disconnect();                    //ルームから退出
    }

    /// <summary>
    /// スピードアップアイテムを使用する.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    public void UseItem() {
        // マウスの右ボタンを押した && アイテムを持っている && スニーク状態でないなら.
        if(Input.GetMouseButton(1) && isHaveItem && !isSneak) {
            // ChangeSpeedコルーチンを発動
            StartCoroutine("ChangeSpeed");
            isUseItem = true;
        }
    }


    ///<summary> UGUI表示 </summary>
    private void OnGUI() {
        if(!photonView.IsMine || !isOnGui) {
            return;                                                      // 自分でない場合は処理を行わない.
        }

        var controlStyle = new GUIStyle(GUI.skin.label) { fontSize = 30 };                  // フォントサイズを設定.
        var rectY = Screen.height/14;
        // ゲームスタート前のカウントダウンを表示.
        controlStyle.fontSize = 300;
        GUI.Label(new Rect(Screen.width/2, rectY, 300, 300), isGameStartTimer.ToString(), controlStyle);
    }

    ///<summary>5秒間待ってゲームを開始する</summary>
    public IEnumerator GameStartCountDown() {
        BGM.Call_BGM_Stop(); // BGMを止める.
        SE.Call_SE(3);                          // カウントダウンの音を鳴らす.
        isGameStart_CountDown = false;
        // 5秒間カウントダウン.
        for(isGameStartTimer = COUNTDOWN; isGameStartTimer > 0; isGameStartTimer--) {
            isOnGui = true;                     // OnGuiを有効にする.
            yield return new WaitForSeconds(1f);
        }

        isOnGui = false;                        // OnGuiを無効にする.

        var BGMObject = GameObject.Find("BGM");
        BGMObject.GetComponent<BGM_Script>().Call_BGM(0);
        charaState = CharaState.ゲーム中;

        // アイテム生成ルーチンを動かす.
        if(PhotonNetwork.LocalPlayer.IsMasterClient) {
            StartCoroutine(ItemSpawn());
        }
    }

    ///<summary>自分がマスタークライアントならば、指定の座標にアイテムを出現させる</summary>
    public IEnumerator ItemSpawn() {
        while(true) {
            for(int i = 0; i < itemSpawnPoint.Length ; i++) {
                PhotonNetwork.Instantiate("Item", itemSpawnPoint[i].transform.position, Quaternion.identity);// Resorcesフォルダ内のItemを生成.
            }
            yield return new WaitForSeconds(10.0f);
        }
    }

    ///<summary> スピードアップアイテムを使用したときの処理 </summary>
    public IEnumerator ChangeSpeed() {
        // 5秒間スピードアップ
        yield return new WaitForSeconds(5.0f);
        // スピードアップ状態を解除
        isUseItem = false;
        isHaveItem = false;
    }
}