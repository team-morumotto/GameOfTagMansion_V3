using System.Reflection;
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
    [Tooltip("キャラクターのステージのスポーン場所")]
    [FormerlySerializedAs("before")]
    public GameObject[] userSpawnPoint;           // キャラクターのステージスポーン場所.

    [Tooltip("スピードアップアイテムのステージスポーン場所")]
    [FormerlySerializedAs("before")]
    public GameObject[] itemSpawnPoint;         // アイテムのステージスポーン場所.

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
    public int characterNumber;
    //---------- protected変数----------//
    public Animator anim;                 // アニメーション.
    protected Camera playerCamera;           // プレイヤーを追尾するカメラ.
    protected Button_SE SE;
    protected BGM_Script BGM;
    protected Text gameTimer;                // タイマー出力用.
    public GameObject resultPanel;        // リザルトパネル.
    protected Text resultWinLoseText;        // リザルトの勝敗.
    protected Rigidbody rb;                  // リジッドボディ.
    protected GameObject staminaParent;      // スタミナUIの親.
    protected Image staminaGuage;            // スタミナゲージ.
    public List<GameObject> playerList = new List<GameObject>(); // ルーム内の自分を除くキャラのリスト.
    public List<GameObject> escapeList = new List<GameObject>(); // ルーム内の逃げキャラのリスト.
    public List<Target> escapeTargetList = new List<Target>(); // ルーム内の逃げキャラのTargetコンポーネントのリスト.
    public Target chaserTarget; // ルーム内の鬼キャラのターゲットコンポーネント.

    //------ int変数 ------//
    protected int isGameStartTimer = 5;
    protected int abilityUseAmount = 3; // 固有能力の使用可能回数(試験的に三回).
    private int countDownSeconds = 5;          // ゲームスタートまでのカウントダウン
    protected int isHit = 0; // デバッグ用.

    //------ float変数 ------//
    protected float nowStamina;

    //------ bool変数 ------//
    protected bool isGameStarted = false;         //ゲームスタートしたか.
    protected bool isOnGui = false;              // GUIを表示しているか.
    protected bool isGround = true;              // 地面に接地しているか.
    protected bool isSneak = false;              // スニークしているか.
    protected bool isStaminaLoss = false;        // スタミナが切れているか.
    protected bool isStan = false;               // スタンしているか.
    protected bool isUseAvility = false;         // 固有能力を発動しているか.
    public bool isRunning = false;               // 走っているか.

    //------ string変数 ------//
    protected string fps = "";

    /// <summary>
    /// プレイヤーの移動処理.
    /// </summary>
    /// <param name="moveForward">移動方向</param>
    /// <param name="moveSpeed">移動速度</param>
    /// <param name="animSpeed">アニメーション速度</param>
    protected void MoveType(Vector3 moveForward, float moveSpeed, float animSpeed) {
        //print("f" + moveForward);
        // print(moveSpeed);
        rb.velocity = moveForward * moveSpeed + new Vector3(0, rb.velocity.y, 0); // 移動.
        // print("vel" + rb.velocity);
        anim.SetFloat("Speed", 1.0f); // 移動中は1.0.
        anim.SetFloat("DashSpeed", animSpeed);
    }

    /// <summary>
    /// 一定時間ごとにスタミナ回復.
    /// </summary>
    protected void RegenerativeStaminaHeal() {
        // スタミナが減っていたら.
        if(nowStamina < staminaAmount) {
            if(isStaminaLoss) {
                nowStamina += staminaHealAmount * 0.3f; // スタミナ回復(スタミナ切れ中は回復量が減少).
            }else {
                nowStamina += staminaHealAmount;        // スタミナ回復.
            }

            // 回復後にスタミナが上限超過したら.
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
    public void GetStatus() {
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
        BGM.audioSource.Stop();             // BGMを止める.
        isOnGui = true;                     // OnGuiを有効にする.

        int i = countDownSeconds;           // カウントダウン秒数を入れる.
        while(i > 0) {
            SE.Call_SE(3);                          // カウントダウンの音を鳴らす.
            yield return new WaitForSeconds(1f);
            i--;
        }

        isGameStarted = true;
        isOnGui = false;                    // OnGuiを無効にする.

        BGM.Call_BGM(0);
        gameState = GameState.ゲーム中;

        // 自分がマスタークライアントなら.
        if(PhotonNetwork.LocalPlayer.IsMasterClient) {
            StartCoroutine(ItemSpawn());
        }
    }

    ///<summary>指定の座標にアイテムを出現させる</summary>
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

    /// <summary>
    /// ルーム内のキャラクターのオブジェクトやコンポーネントの取得.
    /// </summary>
    protected IEnumerator GetPlayers(float delay) {
        print("players");

        yield return new WaitForSeconds(delay); //
        print("Players");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        playerList = new List<GameObject>();   // 初期化.
        escapeList = new List<GameObject>();   // 初期化.
        escapeTargetList = new List<Target>(); // 初期化.
        chaserTarget = null;                   // 初期化.

        // nullなら処理しない.
        if(players == null) {
            print("Not Player");
            yield break;
        }

        foreach(var player in players) {
            // 取得したキャラクターが自分でない場合.
            if(player != this.gameObject) {
                playerList.Add(player); // 追加.

                if(player.GetComponent<PlayerEscape>()) {
                    escapeList.Add(player); // 追加.
                    var tmp2 = player.GetComponent<Target>();
                    escapeTargetList.Add(tmp2); // 追加.
                    // 鬼なら.
                    if(GoToChooseChara.GetPlayMode() == 1) {
                        tmp2.enabled = false;
                    }

                }else if(player.GetComponent<PlayerChaser>()) {
                    var tmp2 = player.GetComponent<Target>();
                    chaserTarget = tmp2; // 鬼キャラは一人固定なので一意。

                    // 逃げなら.
                    if(GoToChooseChara.GetPlayMode() == 0) {
                        chaserTarget.enabled = false;
                    }
                }
            }
        }

        print("players");
    }

    /// <summary>
    /// 与えられたフラグの値入れ替え.
    /// </summary>
    /// <param name="flg">フラグ</param>
    /// <param name="delay"></param>
    /// <returns></returns>
    protected IEnumerator ChangeFlg(bool flg, float delay) {
        yield return new WaitForSeconds(delay);
        yield return flg = !flg;
    }

    /// <summary>
    /// 3秒スタン
    /// </summary>
    /// <returns></returns>
    protected IEnumerator Stan() {
        print("スタン中");
        isStan = true;
        anim.SetBool("Stan", true); // スタンアニメーション.
        yield return new WaitForSeconds(3.0f);
        isStan = false;
        anim.SetBool("Stan", false); // スタンアニメーション.
        print("スタン後");
    }

    /*/// <summary>
    /// UGUI表示[デバッグ用]
    /// </summary>
    void OnGUI() {
        if(!photonView.IsMine) {
            return;
        }
        GUIStyle style = new GUIStyle();
        style.fontSize = 100;
        GUI.Label(new Rect(100, 100, 300, 300), "velocity:" + rb.velocity.ToString(), style);
        GUI.Label(new Rect(100, 200, 300, 300), "deltaTime:" + Time.deltaTime.ToString(), style);
        GUI.Label(new Rect(100, 400, 300, 300), "isHit:" + isHit.ToString(), style);
    }*/

    public enum ItemName{
        invincibleStar, //無敵スター
        locationShuffle, //位置入れ替え
        abilityBlock, //アビリティ封印
        movementBinding, //移動封印
        drink, //小回復
        poteto, //中回復
        hamburger, //大回復
        disposableGrapnelGun, //使い捨てグラップルガン
    }
    public List<ItemName>[] haveItem = new List<ItemName>[2];
    protected bool isCanUseAbility = true; //これがtrueならアビリティが使える(封印処理用)
    protected bool isCanUseMovement = true; //これがtrueなら移動が使える(封印処理用)
    protected bool isInvincible = false; //これがtrueなら無敵(無敵スター用)
    /// <summary>
    /// アイテム関連の処理.
    /// </summary>
    void ItemUse(){
        if(Input.GetKey(KeyCode.I)){
            // アイテムを持っているなら.
            if(isHaveItem){
                ItemName tmp = haveItem[0][0]; // アイテム名を取得.
                switch(tmp){
                    case ItemName.invincibleStar:
                        // 無敵スターを使用する.
                        isInvincible = true;
                        ChangeFlg(isInvincible, 10.0f);
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.locationShuffle:
                        // 位置入れ替えを使用する. まだできてない
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.abilityBlock:
                        // アビリティ封印を使用する. 現在自分にしかけしかけられない
                        isCanUseAbility = false;
                        ChangeFlg(isCanUseAbility, 10.0f);
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.movementBinding:
                        // 移動封印を使用する. 現在自分にしかけしかけられない
                        isCanUseMovement = false;
                        ChangeFlg(isCanUseMovement, 5.0f);
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;

                    case ItemName.drink:
                        // 小回復を使用する.
                        InstanceStaminaHeal(10);
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.poteto:
                        // 中回復を使用する.
                        InstanceStaminaHeal(30);
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.hamburger:
                        // 大回復を使用する.
                        InstanceStaminaHeal(50);
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                    case ItemName.disposableGrapnelGun:
                        // 使い捨てグラップルガンを使用する.  まだできてない
                        haveItem[0].RemoveAt(0); // アイテムを消費.
                        break;
                }
            }
        }
    }
    protected void ItemGet(ItemName itemName){
        haveItem[0].Add(itemName);
    }

    protected float amplification = 0; // アイテムの効果量の増幅値.

    ///<summary>
    /// 回復する割合を%で指定すると最大スタミナに合わせて回復する.
    ///</summary>
    protected void InstanceStaminaHeal(float healparsent){
        var healamount = (staminaAmount/100)*(healparsent+amplification);
        nowStamina += healamount;
    }

    //------ 以下、固有性能(複数のスクリプトから呼び出しがある場合は基底クラスに) ------//
    /// <summary>
    /// ルーム内のキャラクターのカーソルを表示.
    /// </summary>
    /// <param name="isEscape">呼び出し側が逃げキャラかどうか</param>
    protected IEnumerator TargetShow(bool isEscape) {
        print("TargetShow");
        if(photonView.IsMine) {
            if(isEscape) {
                chaserTarget.enabled = true;
                print("chaserTrue");
                yield return new WaitForSeconds(10.0f);
                chaserTarget.enabled = false;
                PhotonMatchMaker.SetCustomProperty("ct", false, 1);
            }else{
                print("EscapeTrue");
                foreach(var targets in escapeTargetList) {
                    targets.enabled = true;
                }
                yield return new WaitForSeconds(10.0f);
                foreach(var targets in escapeTargetList) {
                    targets.enabled = false;
                }
                PhotonMatchMaker.SetCustomProperty("et", false, 1);
            }
        }
    }
}