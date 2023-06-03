using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;

public class PlayerBase : MonoBehaviourPunCallbacks
{
    //=========== キャラクターステータス(変動しない) ===========//
    //----------- float変数 -----------//
	protected float walkSpeed; // 歩行速度.
	protected float runSpeed; // 走行速度.
	protected float staminaAmount; // スタミナ.
    protected float staminaHealAmount; // スタミナ回復量.

    //---------- bool変数 ----------//
	protected bool overCome; // 乗り越え.
	protected bool obstructive; // 邪魔者.
	protected bool stealth; //ステルス.
	protected bool special; // 特殊.
    //=========== キャラクターステータス ===========//

    //------ public static変数 ------//
    public static bool isHaveItem = false;   // アイテムを取得したかどうか.
    public static bool isUseItem = false;    // アイテムを使用したかどうか.
    //------------ 定数 ------------//
    public const string GAMECANVAS = "/Canvas_Main"; // Canvas_Mainの取得.

    //----------- public変数 -----------//
    [Tooltip("キャラクターのステージのスポーン場所")] [FormerlySerializedAs("before")]               public GameObject[] userSpawnPoint;           // キャラクターのステージスポーン場所.
    [Tooltip("スピードアップアイテムのステージスポーン場所")] [FormerlySerializedAs("before")]       public GameObject[] itemSpawnPoint;         // アイテムのステージスポーン場所.
    public enum GameState { // ゲームの進行状況.
        ゲーム開始前,
        カウントダウン,
        ゲーム中,
        ゲーム終了
    }
    public GameState gameState = GameState.ゲーム開始前; // ゲーム開始前で初期化.

    public enum Character {
        Tolass,
        Liloumois,
        MikagamiKoyomi,
        NoranekoSeven,
        Shaclo,
        Mulicia,
        Wenrui,
        Mishe,
        AsakaYanoha,
        Nayu
    }
    public Character character;
    public CharacterDatabase characterDatabase;
    //---------- protected変数----------//
    protected Animator anim;                 // アニメーション.
    protected ParticleSystem particleSystem; // パーティクルシステム.
    protected Camera playerCamera;           // プレイヤーを追尾するカメラ.
    protected Button_SE SE;
    protected BGM_Script BGM;
    protected Text gameTimer;            // タイマー出力用.
    protected GameObject resultPanel;        // リザルトパネル.
    protected Text resultWLText;             // リザルトパネルの勝敗テキスト.
    protected Text resultWinLoseText;        // リザルトの勝敗.
    protected Rigidbody rb;                  // リジッドボディ.
    protected GameObject staminaParent;      // スタミナUIの親.
    protected Image staminaGuage;            // スタミナゲージ.

    //------ int変数 ------//
    protected int isGameStartTimer = 5;
    private int countDown = 5;                   // ゲームスタートまでのカウントダウン

    //------ float変数 ------//
    protected float nowStamina;

    //------ bool変数 ------//
    protected bool isGameStarted = true;         //ゲームスタートしたか.
    protected bool isOnGui = false;              // GUIを表示しているか.
    protected bool isGround = true;              // 地面に接地しているか.
    protected bool isSneak = false;              // スニークしているか.
    private bool isStaminaLoss = false;          // スタミナが切れているか.

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

                MoveType(moveForward, runSpeed, 1.5f);
                particleSystem.Play();     //パーティクルシステムをスタート
            }else {
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

    private void MoveType(Vector3 moveForward, float moveSpeed, float animSpeed) {
        rb.velocity = moveForward * moveSpeed + new Vector3(0, rb.velocity.y, 0); // プレイヤーの走る処理.
        anim.SetFloat("Speed", 1.0f); // 移動中は1.0.
        anim.SetFloat("DashSpeed", animSpeed);
    }

    private void StaminaHeal() {
        // スタミナが減っていたら.
        if(nowStamina < staminaAmount) {
            if(isStaminaLoss) {
                nowStamina += staminaHealAmount * 0.3f; // スタミナ回復(スタミナ切れ中は回復量が減少).
            }else {
                nowStamina += staminaHealAmount;        // スタミナ回復.
            }

            // 回復後にスタミナが上限超過したら
            if(nowStamina >= staminaAmount) {
                nowStamina = staminaAmount; // スタミナはオーバーフローしない.
                isStaminaLoss = false; //スタミナ切れ解除.
                staminaParent.SetActive(false); // スタミナUI非表示.
            }
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
        gameTimer.text = ("00:00.000").ToString(); //残り時間を0に上書きし表示
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
            StartCoroutine(ChangeSpeed());
            isUseItem = true;
        }
    }

    /// <summary>
    /// キャラクターのステータスを取得する.
    /// </summary>
    public void StatusGet() {
        var tmp1 = Character.GetValues(typeof(Character)); // ScriptableObjectの個数取得.
        var tmp2 = (int)character; // キャラクター名の列挙体の番号を取得.
        foreach (var value in tmp1) {
            var tmp3 = (int)value;
            if(tmp2 == tmp3) {
                walkSpeed = characterDatabase.statusList[tmp3].walkSpeed;
                runSpeed = characterDatabase.statusList[tmp3].runSpeed;
                staminaAmount = characterDatabase.statusList[tmp3].staminaAmount;
                staminaHealAmount = characterDatabase.statusList[tmp3].staminaHealAmount;
                overCome = characterDatabase.statusList[tmp3].overCome;
                obstructive = characterDatabase.statusList[tmp3].obstructive;
                stealth = characterDatabase.statusList[tmp3].stealth;
                special = characterDatabase.statusList[tmp3].special;
            }
        }
        nowStamina = staminaAmount; // 現状のスタミナに最大スタミナを代入.
    }

    ///<summary>5秒間待ってゲームを開始する</summary>
    public IEnumerator GameStartCountDown() {
        BGM.Call_BGM_Stop(); // BGMを止める.
        SE.Call_SE(3);                          // カウントダウンの音を鳴らす.
        isGameStarted = false;
        // 5秒間カウントダウン.
        for(isGameStartTimer = countDown; isGameStartTimer > 0; isGameStartTimer--) {
            isOnGui = true;                     // OnGuiを有効にする.
            yield return new WaitForSeconds(1f);
        }

        isOnGui = false;                        // OnGuiを無効にする.

        var BGMObject = GameObject.Find("BGM");
        BGMObject.GetComponent<BGM_Script>().Call_BGM(0);
        gameState = GameState.ゲーム中;

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