using System;
using UnityEngine.Events;
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
    public Animator anim;                 // アニメーション.
    public static Camera playerCamera;           // プレイヤーを追尾するカメラ.
    public Image SeenBy;                     // 相手に位置を見られているかのアイコン.
    public List<GameObject> playerList = new List<GameObject>(); // ルーム内の自分を除くキャラのリスト.
    public List<GameObject> escapeList = new List<GameObject>(); // ルーム内の逃げキャラのリスト.
    public List<Target> escapeTargetList = new List<Target>(); // ルーム内の逃げキャラのTargetコンポーネントのリスト.
    public Target chaserTarget; // ルーム内の鬼キャラのターゲットコンポーネント.
    //---------- protected変数----------//
    protected Button_SE SE;
    protected BGM_Script BGM;
    protected Text gameTimer;                // タイマー出力用.
    public GameObject resultPanel;           // リザルトパネル.
    protected Text resultWinLoseText;        // リザルトの勝敗.
    protected Rigidbody rb;                  // リジッドボディ.
    protected GameObject staminaParent;      // スタミナUIの親.
    protected Image staminaGuage;            // スタミナゲージ.
    public Text avilityRiminingAmount;       // 固有能力の残り使用可能回数.
    public Image avilityRecastAmount;        // 固有能力のリキャスト時間を反映するUI.
    protected Image avilityImage;            // 固有能力の画像.

    //------ int変数 ------//
    protected int abilityUseAmount = 3;      // 固有能力の使用可能回数(試験的に三回).
    private int countDownSeconds = 5;        // ゲームスタートまでのカウントダウン.
    protected int isHit = 0; // デバッグ用.

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
                SE.Call_SE(6);
            break;

            case 1:
                SE.Call_SE(5);
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
        SE.Call_SE(3);                      // カウントダウンの音を鳴らす.
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
        switch(flgName) {
            case "CanUseAbility ":
                yield return StartCoroutine(BooleanReverse(x => isCanUseAbility = x, !isCanUseAbility, 10.0f));
            break;
            case "CanUseMovement":
                yield return StartCoroutine(BooleanReverse(x => isCanUseMovement = x, !isCanUseMovement, 5.0f));
            break;
            case "Invincible":
                yield return StartCoroutine(BooleanReverse(x => isInvincible = x, !isInvincible, 10.0f));
            break;
            case "Slow":
                yield return StartCoroutine(BooleanReverse(x => isSlow = x, !isSlow, 5.0f));
            break;
                case "CanUseDash":
                print("無限ダッシュ");
                yield return StartCoroutine(BooleanReverse(x => isCanUseDash = x, !isCanUseDash, 20.0f));
                print("無限ダッシュ解除");
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

    public enum ItemName{
        invincibleStar, //無敵スター
        // locationShuffle, //位置入れ替え
        // abilityBlock, //アビリティ封印
        // movementBinding, //移動封印
        drink, //小回復
        poteto, //中回復
        hamburger, //大回復
        disposableGrapnelGun, //使い捨てグラップルガン
        speedup,//スタミナ無限
    }
    public List<ItemName> haveItem = new List<ItemName>(); // 所持アイテム記録用のリスト
    public List<Image> haveItemImageList = new List<Image>(); // 所持アイテムのイメージリスト.
    protected bool isCanUseAbility = true; //これがtrueならアビリティが使える(封印処理用)
    protected bool isCanUseMovement = true; //これがtrueなら移動が使える(封印処理用)
    protected bool isInvincible = false; //これがtrueなら無敵(無敵スター用)
    protected bool isAddhaveItem = false; //これがtrueならアイテムを二個保持できる(キャラクター用)
    protected bool isCanUseDash = false; //これがtrueならスタミナが減らない

    /// <summary>
    /// アイテム関連の処理.
    /// </summary>
    protected void ItemUse(){
        // アイテムを持っていないなら処理しない.
        if(haveItem.Count == 0) {
            return;
        }

        // Eキーが押されていたら.
        if(Input.GetKeyDown(KeyCode.E)){
            ItemName tmp = haveItem[0]; // アイテム名を取得.
            switch(tmp){
                case ItemName.invincibleStar:
                    // 無敵スターを使用する.
                    isInvincible = true;
                    DelayChangeFlg("Invincible");
                    print("Invincible");
                    break;
                case ItemName.drink:
                    // 小回復を使用する.
                    InstanceStaminaHeal(10);
                    break;
                case ItemName.poteto:
                    // 中回復を使用する.
                    InstanceStaminaHeal(30);
                    break;
                case ItemName.hamburger:
                    // 大回復を使用する.
                    InstanceStaminaHeal(50);
                    break;
                case ItemName.disposableGrapnelGun:
                    // 使い捨てグラップルガンを使用する.  まだできてない
                    HookShot();
                    break;
                case ItemName.speedup:
                    // スタミナの上限解放（無制限ダッシュ）
                    isCanUseDash = true;
                    isStaminaLoss = false; // スタミナ切れ回復
                    nowStamina = staminaAmount;  // スタミナマックス.
                    StartCoroutine(DelayChangeFlg("CanUseDash"));
                    print("UseDash");
                    break;
            }
            haveItem.RemoveAt(0); // アイテムを消費.

            // アイテムの複数持ちが可能なら.
            if(isAddhaveItem) {
                for(int i = 0; i < haveItemImageList.Count; i++) {
                    // リストのスプライトがemptyなら.
                    if(haveItemImageList[i].sprite != itemDatabase.emptySprite) {
                        haveItemImageList[i].sprite = haveItemImageList[i + 1].sprite; // リストをずらす.
                        haveItemImageList[i + 1].sprite = itemDatabase.emptySprite; // リストをずらす.
                    }
                }
            }else {
                haveItemImageList[0].sprite = itemDatabase.emptySprite;
            }
        }
    }

    /// <summary>
    /// アイテムを入手する処理(アイテムから叩かせるのでpublicにしました)
    /// </summary>
    public void ItemGet(ItemName itemName){
        if(!photonView.IsMine) {
            return;
        }
        var itemData = itemDatabase.GetItemData(itemName.ToString());
        if(isAddhaveItem) {
            for(int i = 0; i < haveItemImageList.Count; i++) {
                if(haveItemImageList[i].sprite == itemDatabase.emptySprite) {
                    // print("空きを発見");
                    haveItemImageList[i].sprite = itemData.itemIcon;
                    break;
                }
            }
        }else {
            haveItemImageList[0].sprite = itemData.itemIcon;
        }

        //ない時は無条件で追加
        if(haveItem.Count == 0){
            haveItem.Add(itemName);
        }

        //二個目のアイテムを持てるキャラなら
        else if(haveItem.Count == 1 && isAddhaveItem){
            haveItem.Add(itemName);
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
        print(abilityUseAmount);
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
            // print("relative");
            var tmp = targetPos - transform.position;
            Vector3 direction = tmp.normalized; // 目標位置への単位ベクトルを計算.
            relativeDistance = tmp.magnitude;
            float distance = speed * Time.deltaTime; // 目標位置への移動量を計算.
            transform.position += direction * distance; // 目標位置に向かって移動.

            print(relativeDistance);

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
        // isUseAvility = false; // 発動終了. // override追加項目.

        // StartCoroutine(AvillityCoolTime(10.0f)); // クールタイム. // override追加項目.
    }
}