using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
	public bool floating; // 浮遊.
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
    public ItemDatabase itemDatabase;
    public  int characterNumber;
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
    protected List<Sprite> itemImageList = new List<Sprite>();
    protected GameObject instancedObstruct;

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
    protected bool isStaminaLoss = false;          // スタミナが切れているか.
    public bool isRunning = false;            // 走っているか.

    protected void MoveType(Vector3 moveForward, float moveSpeed, float animSpeed) {
        rb.velocity = moveForward * moveSpeed + new Vector3(0, rb.velocity.y, 0); // プレイヤーの走る処理.
        anim.SetFloat("Speed", 1.0f); // 移動中は1.0.
        anim.SetFloat("DashSpeed", animSpeed);
    }

    protected void StaminaHeal() {
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
                floating = characterDatabase.statusList[tmp3].floating;
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

    public enum ItemName{
        invincibleStar, //無敵スター
        locationShuffle, //位置入れ替え
        abilityBlock, //アビリティ封印
        movementBinding, //移動封印
        drink, //小回復
        poteto, //中回復
        hamburger, //大回復
        disposableGrapnelGun, //使い捨てグラップルガン
    }; 
    private List<ItemName>[] haveItem = new List<ItemName>[2];
    protected bool isUseAbility = true; 
    /// <summary>
    /// アイテム関連の処理.
    /// </summary>
    void ItemUse(){
        // アイテムを持っているなら.
        if(Input.GetKey(KeyCode.I)){

            if(isHaveItem){
                ItemName tmp = haveItem[0][0]; // アイテム名を取得.
                switch(tmp){
                    case ItemName.invincibleStar:
                        // 無敵スターを使用する.
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.locationShuffle:
                        // 位置入れ替えを使用する.
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.abilityBlock:
                        // アビリティ封印を使用する.
                        isUseAbility = false;
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.movementBinding:
                        // 移動封印を使用する.
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.drink:
                        // 小回復を使用する.
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.poteto:
                        // 中回復を使用する.
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.hamburger:
                        // 大回復を使用する.
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.disposableGrapnelGun:
                        // 使い捨てグラップルガンを使用する.
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
               }
            }
        }
    }
    protected void ItemGet(ItemName itemName){
        haveItem[0].Add(itemName);
    }
}