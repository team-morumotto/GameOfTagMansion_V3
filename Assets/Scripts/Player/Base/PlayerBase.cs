using System;
using UnityEngine.Events;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Effekseer;

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
    public EffectDatabase EffectDatabase;
    public int characterNumber;
    public Animator anim;                 // アニメーション.
    public static Camera playerCamera;           // プレイヤーを追尾するカメラ.
    public Image SeenBy;                     // 相手に位置を見られているかのアイコン.
    public List<GameObject> playerList = new List<GameObject>(); // ルーム内の自分を除くキャラのリスト.
    public List<GameObject> escapeList = new List<GameObject>(); // ルーム内の逃げキャラのリスト.
    public List<Target> escapeTargetList = new List<Target>(); // ルーム内の逃げキャラのTargetコンポーネントのリスト.
    public Target chaserTarget; // ルーム内の鬼キャラのターゲットコンポーネント.
    public GameObject resultPanel;           // リザルトパネル.
    public Text avilityRiminingAmount;       // 固有能力の残り使用可能回数.
    public Image avilityRecastAmount;        // 固有能力のリキャスト時間を反映するUI.
    public EffekseerEmitter emitter;         // EffeKSeerEmitter.
    //---------- protected変数----------//
    protected Button_SE SE;
    protected BGM_Script BGM;
    protected Text gameTimer;                // タイマー出力用.
    protected Text resultWinLoseText;        // リザルトの勝敗.
    protected Rigidbody rb;                  // リジッドボディ.
    protected GameObject staminaParent;      // スタミナUIの親.
    protected Image staminaGuage;            // スタミナゲージ.
    protected Image avilityImage;            // 固有能力の画像.
    protected EffekseerEffectAsset avilityEffect; // 固有能力のエフェクト.
    protected Coroutine healBoostEffectCoroutine;  // スタミナ回復のブーストのエフェクトコルーチン.

    //------ int変数 ------//
    protected int abilityUseAmount = 3;      // 固有能力の使用可能回数(試験的に三回).
    protected int isHit = 0; // デバッグ用.
    protected float HealBoostAmount = 0.3f;  // スタミナ回復量のブースト量.
    private int countDownSeconds = 5;        // ゲームスタートまでのカウントダウン.

    //------ float変数 ------//
    protected float nowStamina;

    //------ bool変数 ------//
    public bool isRunning = false;               // 走っているか.
    protected bool isGameStarted = false;        //ゲームスタートしたか.
    protected bool isOnGui = false;              // GUIを表示しているか.
    protected bool isGround = true;              // 地面に接地しているか.
    protected bool isSneak = false;              // スニークしているか.
    protected bool isStaminaLoss = false;        // スタミナが切れているか.
    protected bool isStan = false;               // スタンしているか.
    protected bool isUseAvility = false;         // 固有能力を発動しているか.
    protected bool isCoolTime = false;           // 固有能力発動後のクールタイム中か.
    protected bool isSlow = false;               // 移動速度が低下しているか.
    protected bool isFrequency = false;          // 固有能力が回数制限性か.
    public static bool isDebug = false;              // デバッグか.

    //------ string変数 ------//
    protected string fps = "";

    /// <summary>
    /// プレイヤーの移動処理.
    /// </summary>
    /// <param name="moveForward">移動方向</param>
    /// <param name="moveSpeed">移動速度</param>
    /// <param name="animSpeed">アニメーション速度</param>
    protected void MoveType(Vector3 moveForward, float moveSpeed, float animSpeed,float horizontal,float vertical) {
        //print("f" + moveForward);
        // print(moveSpeed);
        rb.velocity = moveForward * moveSpeed + new Vector3(0, rb.velocity.y, 0); // 移動.
        // print("vel" + rb.velocity);
        anim.SetFloat("Speed", 1.0f); // 移動中は1.0.
        anim.SetFloat("SpeedX", horizontal);
        anim.SetFloat("SpeedY", vertical);
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
                staminaGuage.color = new Color(0, 255, 0);
                staminaParent.SetActive(false); // スタミナUI非表示.
            }
        }
    }

    /// <summary>
    /// キャラクターの座標をリセットする.
    /// </summary>
    public void CharaPositionReset() {
        // Bキーを押した時、最初のスポーン位置に転移する.
        if(Input.GetKeyDown(KeyCode.B)) {
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
    /// 引数 : state.
    /// 戻り値 : なし.
    /// </summary>
    /// <param name="state">0 : 敗北, 1 : 勝利, 2 : エラー</param>
    public void GameEnd(int state) {
        switch(state) {
            case 0:
                SE.CallButtonSE(6);
            break;

            case 1:
                SE.CallButtonSE(5);
            break;

            case 2:
                print("エラーによるゲーム終了です");
            break;

            default:
                Debug.LogError("想定されていないゲーム終了です");
            break;
        }

        Cursor.visible = true;                        // カーソルを表示.
        resultPanel.SetActive(true);                  // パネルを表示.
        gameTimer.text = ("00:00.000").ToString();    // 残り時間を0に上書きし表示.
        PhotonNetwork.Destroy(gameObject);            // 自分を全体から破棄.
        PhotonNetwork.Disconnect();                   // ルームから退出.
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
                if(photonView.IsMine) {
                    avilityImage.sprite = characterDatabase.statusList[tmp3].avilitySprite;
                }
            }
        }
        nowStamina = staminaAmount; // 現状のスタミナに最大スタミナを代入.
    }

    ///<summary>5秒間待ってゲームを開始する</summary>
    public IEnumerator GameStartCountDown() {
        BGM.audioSource.Stop();             // BGMを止める.
        isOnGui = true;                     // OnGuiを有効にする.

        int i = countDownSeconds;           // カウントダウン秒数を入れる.
        SE.CallButtonSE(3);                 // カウントダウンの音を鳴らす.
        while(i > 0) {
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
        List<GameObject> items = new List<GameObject>();
        while(true) {
            for(int i = 0; i < itemSpawnPoint.Length ; i++) {
                items.Add(PhotonNetwork.Instantiate("NewItem", itemSpawnPoint[i].transform.position, Quaternion.identity));// Resorcesフォルダ内のItemを生成.
            }
            yield return new WaitForSeconds(10.0f);
            foreach(var item in items) {
                if(item) {
                    print("削除");
                    PhotonNetwork.Destroy(item);
                }
            }
        }
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
    /// 固有性能のクールタイム.
    /// </summary>
    /// <param name="delay">遅延時間</param>
    protected IEnumerator AvillityCoolTime(float delay) {
        var delta = delay;
        isCoolTime = true; // クールタイム中.
        while(delta > 0) {
            print("delta : " + delta);
            avilityRecastAmount.fillAmount = delta / delay;
            yield return null; // 1フレーム遅延.
            delta -= Time.deltaTime; // クールタイム計算.
            avilityRiminingAmount.text = delta.ToString(); // クールタイムの残り時間を表示.
        }
        isCoolTime = false; // クールタイム解除.
        avilityRiminingAmount.text = ""; // 非表示処理は重いので空白を入れて不可視化.
    }

    /// <summary>
    /// 3秒スタン
    /// </summary>
    protected IEnumerator Stan() {
        isStan = true;
        anim.SetBool("Stan", true); // スタンアニメーション.
        yield return new WaitForSeconds(3.0f);
        isStan = false;
        anim.SetBool("Stan", false); // スタンアニメーション.
    }

    /// <summary>
    /// フラグの入れ替えを遅延して行う.
    /// </summary>
    /// <param name="flgName">フラグの名前(isは省略)</param>
    protected IEnumerator DelayChangeFlg(string flgName) {
        var time = 0.0f;
        switch(flgName) {
            case "CanUseAbility":
                time = 10.0f;
                StartCoroutine(TimeEffectLoop(EffectDatabase.itemEffects[1], time));
                yield return StartCoroutine(BooleanReverse(x => isCanUseAbility = x, !isCanUseAbility, time));
            break;
            case "CanUseMovement":
                time = 5.0f;
                anim.SetBool("Stan", true);
                StartCoroutine(TimeEffectLoop(EffectDatabase.itemEffects[1], time));
                yield return StartCoroutine(BooleanReverse(x => isCanUseMovement = x, !isCanUseMovement, time));
                anim.SetBool("Stan", false);
            break;
            case "Invincible":
                yield return StartCoroutine(BooleanReverse(x => isInvincible = x, !isInvincible, 10.0f));
            break;
            case "Slow":
                yield return StartCoroutine(BooleanReverse(x => isSlow = x, !isSlow, 5.0f));
            break;
                case "CanUseDash":
                time = 20.0f;
                StartCoroutine(TimeEffectLoop(EffectDatabase.itemEffects[0], time));
                yield return StartCoroutine(BooleanReverse(x => isCanUseDash = x, !isCanUseDash, time));
            break;
        }
    }

    /// <summary>
    /// 与えられたフラグをスイッチする.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="tmpBool">フラグ</param>
    /// <param name="delay">遅延時間</param>
    /// <returns></returns>
    private IEnumerator BooleanReverse(UnityAction<bool> callback, bool tmpBool, float delay) {
        var tmpReturn = tmpBool;
        yield return new WaitForSeconds(delay);
        if(callback != null)callback(tmpReturn);
    }

    private void OnGUI()
    {
        #if UNITY_EDITOR
            if(!photonView.IsMine) {
                return;
            }
            GUIStyle style = new GUIStyle();
            style.fontSize = 100;
            GUI.Label(new Rect(0, 100, style.fontSize, style.fontSize), "CoolTime" + isCoolTime.ToString(), style);
            GUI.Label(new Rect(0, 200, style.fontSize, style.fontSize), "useAvility" + isUseAvility.ToString(), style);
            GUI.Label(new Rect(0, 300, style.fontSize, style.fontSize), "Stan" + isStan.ToString(), style);
            GUI.Label(new Rect(0, 400, style.fontSize, style.fontSize), "Slow" + isSlow.ToString(), style);
        #endif
    }

    /// <summary>
    /// エフェクトを固有能力の発動中ループで再生.
    /// </summary>
    /// <param name="asset">エフェクト</param>
    protected IEnumerator AvilityEffectLoop(EffekseerEffectAsset asset) {
        EffekseerHandle handle = emitter.Play(asset); // 再生中のエフェクト.
        while(isUseAvility) {
            // 再生中のエフェクトが存在しているか.
            if(!handle.exists) {
                handle = emitter.Play(asset); // 再生.
            }
            yield return null;
        }
        handle.Stop();
    }

    /// <summary>
    /// エフェクトを時間制限付きでループ再生.
    /// </summary>
    /// <param name="asset">エフェクト</param>
    /// <param name="limitTime">制限時間</param>
    protected IEnumerator TimeEffectLoop(EffekseerEffectAsset asset, float limitTime) {
        EffekseerHandle handle = emitter.Play(asset); // 再生中のエフェクト.
        var deltaTime = 0.0f;

        while(deltaTime < limitTime) {
            deltaTime += Time.deltaTime;
            // 再生中のエフェクトが存在しているか.
            if(!handle.exists) {
                handle = emitter.Play(asset); // 再生.
            }
            yield return null;
        }
        handle.Stop();
    }

    public enum ItemName{
        // invincibleStar, //無敵スター
        // locationShuffle, //位置入れ替え
        abilityBlock, //アビリティ封印
        movementBinding, //移動封印
        drink, //小回復
        poteto, //中回復
        hamburger, //大回復
        disposableGrapnelGun, //使い捨てグラップルガン
        speedup,//スタミナ無限
    }
    public List<ItemName> haveItemList = new List<ItemName>(); // 所持アイテム記録用のリスト
    public List<Image> haveItemImageList = new List<Image>(); // 所持アイテムのイメージリスト.
    protected bool isCanUseAbility = true; //これがtrueならアビリティが使える(封印処理用)
    protected bool isCanUseMovement = true; //これがtrueなら移動が使える(封印処理用)
    protected bool isInvincible = false; //これがtrueなら無敵(無敵スター用)
    protected bool isAddhaveItem = false; //これがtrueならアイテムを二個保持できる(キャラクター用)
    protected bool isCanUseDash = false; //これがtrueならスタミナが減らない
    protected bool isRoomPropatiesUpdater; // 自分がルームプロパティのアップデートを、アイテムにより行ったか.

    /// <summary>
    /// アイテム関連の処理.
    /// </summary>
    protected void ItemUse(){
        // アイテムを持っていないなら処理しない.
        if(haveItemList.Count == 0) {
            return;
        }

        // Eキーが押されていたら.
        if(Input.GetKeyDown(KeyCode.E)){
            ItemName tmp = haveItemList[0]; // アイテム名を取得.
            switch(tmp){
                // case ItemName.invincibleStar:
                //     // 無敵スターを使用する.
                //     isInvincible = true;
                //     SE.CallItemSE(3);
                //     emitter.Play(EffectDatabase.itemEffects[0]); // エフェクト.
                //     StartCoroutine(DelayChangeFlg("Invincible"));
                //     break;
                case ItemName.abilityBlock:
                    // 固有能力封印を使用.
                    isRoomPropatiesUpdater = true;
                    SE.CallItemSE(1); // SE.
                    emitter.Play(EffectDatabase.itemEffects[1]); // エフェクト.
                    PhotonMatchMaker.SetCustomProperty("ab", true, 1);
                    break;
                case ItemName.movementBinding:
                    // 移動封印を使用.
                    isRoomPropatiesUpdater = true;
                    SE.CallItemSE(1); // SE.
                    emitter.Play(EffectDatabase.itemEffects[1]); // エフェクト.
                    PhotonMatchMaker.SetCustomProperty("mb", true, 1);
                    break;
                case ItemName.drink:
                    // 小回復を使用する.
                    SE.CallItemSE(3); // SE.
                    emitter.Play(EffectDatabase.itemEffects[2]); // エフェクト.
                    InstanceStaminaHeal(10);
                    break;
                case ItemName.poteto:
                    // 中回復を使用する.
                    SE.CallItemSE(3); // SE.
                    emitter.Play(EffectDatabase.itemEffects[2]); // エフェクト.
                    InstanceStaminaHeal(30);
                    break;
                case ItemName.hamburger:
                    // 大回復を使用する.
                    SE.CallItemSE(3); // SE.
                    emitter.Play(EffectDatabase.itemEffects[2]); // エフェクト.
                    InstanceStaminaHeal(50);
                    break;
                case ItemName.disposableGrapnelGun:
                    // 使い捨てグラップルガンを使用する.
                    HookShot();
                    break;
                case ItemName.speedup:
                    // スタミナの上限解放（無制限ダッシュ）
                    isCanUseDash = true;
                    isStaminaLoss = false;       // スタミナ切れ回復
                    nowStamina = staminaAmount;  // スタミナマックス.
                    staminaParent.SetActive(false); // スタミナゲージを隠す.
                    SE.CallItemSE(2); // SE.
                    emitter.Play(EffectDatabase.itemEffects[0]); // エフェクト.
                    StartCoroutine(DelayChangeFlg("CanUseDash"));
                    break;
            }

            // アイテムの複数持ちが可能なら.
            if(isAddhaveItem) {
                // アイテムの個数分繰り返す.
                for(int i = 0; i < haveItemList.Count; i++) {
                    // リストのスプライトがemptyでないなら.
                    if(haveItemImageList[i].sprite != itemDatabase.emptySprite) {
                        haveItemImageList[i].sprite = haveItemImageList[i + 1].sprite; // アイコンリストをずらす.
                        haveItemImageList[i + 1].sprite = itemDatabase.emptySprite; // アイコンリストをずらす.
                    }
                }
            }else {
                haveItemImageList[0].sprite = itemDatabase.emptySprite;
            }

            haveItemList.RemoveAt(0); // アイテムを消費.
        }
    }

    /// <summary>
    /// アイテムを入手する処理(アイテムから叩かせるのでpublicにしました)
    /// </summary>
    public void ItemGet(ItemName itemName){
        if(!photonView.IsMine) {
            return;
        }

        if(isAddhaveItem) {
            if(haveItemList.Count < 2) {
                print("追加しました");
                haveItemList.Add(itemName);
            }else {
                print("既に持っている");
                return;
            }
        }else{
            if(haveItemList.Count < 1) {
                print("追加しました");
                haveItemList.Add(itemName);
            }else {
                print("既に持っている");
                return;
            }
        }

        print("画像追加");

        var itemData = itemDatabase.GetItemData(itemName.ToString());
        if(isAddhaveItem) {
            // アイテムの個数分繰り返す.
            for(int i = 0; i < haveItemList.Count; i++) {
                if(haveItemImageList[i].sprite == itemDatabase.emptySprite) {
                    haveItemImageList[i].sprite = itemData.itemIcon;
                }
            }
        }else {
            haveItemImageList[0].sprite = itemData.itemIcon;
        }
    }

    protected float amplification = 0; // アイテムの効果量の増幅効果があるキャラが使用する変数
    /// <summary>
    /// 回復する割合を%で指定すると最大スタミナに合わせて回復する.
    /// </summary>
    protected void InstanceStaminaHeal(float healparsent){
        //回復したらスタミナがどのぐらいになるのか計算
        var healamount = (nowStamina/staminaAmount)+(healparsent+amplification/healparsent);
        //回復量が最大スタミナを超えたら最大スタミナにする
        if(healamount > staminaAmount) {
            staminaGuage.color = new Color(0, 255, 0); // 緑.
            staminaParent.SetActive(false);
            nowStamina = staminaAmount;
            isStaminaLoss = false;
        }
        //回復量が最大スタミナを超えなかったら回復後の値を代入
        else{
            nowStamina = healamount;
        }
    }

    /// <summary>
    /// 回数制限性の固有性能の残り使用可能回数の更新.
    /// </summary>
    protected void avilityRiminingUpdate() {
        abilityUseAmount--;
        avilityRiminingAmount.text = abilityUseAmount.ToString();
    }

    //------ 以下、固有性能(複数のスクリプトから呼び出しがある場合は基底クラスに.) ------//
    /// <summary>
    /// ルーム内のキャラクターのカーソルを表示.
    /// </summary>
    /// <param name="isEscape">呼び出し側が逃げキャラかどうか</param>
    protected IEnumerator TargetShow(bool isEscape) {
        if(photonView.IsMine) {
            // クールタイム中の場合は処理しない。
            if(!isCoolTime) {
                SE.CallAvilitySE(4); // SE.
                if(isEscape) {
                    chaserTarget.enabled = true;
                    yield return new WaitForSeconds(10.0f);
                    chaserTarget.enabled = false;
                    PhotonMatchMaker.SetCustomProperty("ct", false, 1);
                }else{
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

    protected float relativeDistance;
    protected float HitDistance = 2.0f;
    protected float speed = 30.0f; // 移動速度
    /// <summary>
    /// カメラの中心直線上にレイを飛ばし、当たったオブジェクトを取得する.
    /// </summary>
    protected void HookShot() {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            SE.CallAvilitySE(1); // SE.
            emitter.Play(EffectDatabase.avilityEffects[0]);
            anim.SetBool("HookShot", true);
            StartCoroutine(LinearMove(hit.point));
        }else {
            anim.SetBool("HookShot", false);
        }
    }

    /// <summary>
    /// 特定の位置に直線に向かう.
    /// </summary>
    /// <param name="targetPos">目標の位置</param>
    protected IEnumerator LinearMove(Vector3 targetPos) {
        rb.useGravity = false;
        do {
            var tmp = targetPos - transform.position;
            Vector3 direction = tmp.normalized; // 目標位置への単位ベクトルを計算.
            relativeDistance = tmp.magnitude;
            float distance = speed * Time.deltaTime; // 目標位置への移動量を計算.
            transform.position += direction * distance; // 目標位置に向かって移動.

            //ベクトルの大きさが0.01以上の時に向きを変える処理をする.
            if (relativeDistance > 0.01f) {
                transform.rotation = Quaternion.LookRotation(direction); //向きを変更する.
            }

            yield return null; // 1フレーム遅延.
        } while(relativeDistance > HitDistance);

        transform.position = targetPos;
        transform.rotation = Quaternion.Euler(0,transform.rotation.y, 0);

        anim.SetBool("HookShot", false);
        rb.useGravity = true;

        if(isUseAvility && characterNumber == 1){
            isUseAvility = false; // 発動終了.
            StartCoroutine(AvillityCoolTime(30.0f)); // クールタイム.
        }
    }

    protected List<GameObject> billList = new List<GameObject>(); // 御札のリスト.
    private int billAmount = 6; // 同時に展開する御札の枚数.
    private float radius = 5.0f; // 展開する距離.
    private float abilityTime = 20.0f; // 固有能力の発動時間.
    private float rotationSpeed = 100.0f; // 展開した御札が回転する速度.
    private float coolTime = 10.0f; // クールタイム.
    /// <summary>
    /// 自分の周囲に炎上に御札を展開する。
    /// </summary>
    protected IEnumerator BillCircle() {
        var deltaTime = 0.0f;
        billList = new List<GameObject>();

        for(int i = 0; i < billAmount; i++) {
            float angle = i * (360f / billAmount);  // 角度を計算

            // 角度に基づいて配置する位置を計算
            Vector3 position = transform.position + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 0.1f;

            // オブジェクトを配置
            var tmpBill = PhotonNetwork.Instantiate("Bill", position, Quaternion.identity);
            billList.Add(tmpBill);
        }

        var tmpRadius = 0.0f; // 展開距離.

        while(deltaTime < abilityTime) {
            // 指定の範囲まで展開されるまで広がる.
            if(tmpRadius < radius) {
                tmpRadius += 0.1f;
            }
            for(int i = 0; i < billAmount; i++) {
                var relative = billList[i].transform.position - transform.position; // 方向ベクトル.
                relative = relative.normalized; // 正規化.
                billList[i].transform.position = transform.position + (relative * tmpRadius); // 方向ベクトルに距離を乗算し展開.
                billList[i].transform.RotateAround(transform.position, Vector3.up, (i + 1) * rotationSpeed * Time.deltaTime); // 回転.
                var pos = billList[i].transform.position;
                pos.y = transform.position.y;
                billList[i].transform.position = pos;
            }
            deltaTime += Time.deltaTime;
            yield return null;
        }

        billDestroy();

        // 発動終了.
        isUseAvility = false;
        StartCoroutine(AvillityCoolTime(coolTime));
    }
    private void billDestroy() {
        if(photonView.IsMine) {
            foreach(var bill in billList) {
                PhotonNetwork.Destroy(bill); // 展開した御札を破棄.
            }
        }
    }
}