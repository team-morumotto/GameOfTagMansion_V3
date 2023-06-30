using System.Collections;
using UnityEngine;
using Photon.Pun;

public class EscapeLiloumois : PlayerEscape
{
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(LiloumoisES), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        itemDatabase = GameObject.Find("ItemList").GetComponent<ItemDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update() {
        if(!photonView.IsMine) {
            return;
        }
        if(Input.GetKeyDown(KeyCode.I) && !isUseAvility && !isCoolTime) {
            isUseAvility = true;
            anim.SetBool("HookShot", true);
            HookShot();
        }
        BaseUpdate();
    }

    protected override void BaseUpdate() {
        // 自分のキャラクターでなければ処理をしない
        if(!photonView.IsMine) {
            return;
        }

        fps = (1.0f / Time.deltaTime).ToString();

        switch(gameState) {
            case GameState.ゲーム開始前:
                if(!isStan && isGround && !isUseAvility) {
                    PlayerMove();
                }
                ItemUse();
                PlayNumber();

                if(PhotonMatchMaker.GameStartFlg) {
                    PlayerSpawn(); // キャラクターのスポーン処理.
                    StartCoroutine(GameStartCountDown()); // カウントダウン開始
                    gameState = GameState.カウントダウン;
                }
            break;

            case GameState.カウントダウン:
                anim.SetFloat("DashSpeed", 0.0f); // アニメーションストップ.
                anim.SetFloat("Speed", 0.0f);     // アニメーションストップ.
            break;

            case GameState.ゲーム中:
                if(!isStan && isGround && !isUseAvility) {
                    PlayerMove();
                }
                ItemUse();
                GameTimer();
                Sneak();
                CharaPositionReset();
            break;
        }
    }

    /// <summary>
    /// 機能 : プレイヤーの移動制御.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    protected override void PlayerMove() {
        //プレイヤーの向きを変える
        var inputHorizontal = Input.GetAxis("Horizontal"); // 入力デバイスの水平軸.
        var inputVertical = Input.GetAxis("Vertical");     // 入力デバイスの垂直軸.

        if(inputHorizontal == 0 && inputVertical == 0) {
            anim.SetFloat("Speed", 0f); // 移動していないので0.
            RegenerativeStaminaHeal();
        }
        else{
            Vector3 cameraForward = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;// カメラの向きを取得
            Vector3 moveForward = cameraForward * inputVertical + playerCamera.transform.right * inputHorizontal;  // カメラの向きに合わせて移動方向を決定

            // スタミナが残っていて走っている.
            if(nowStamina > 0 && Input.GetKey(KeyCode.LeftControl) && !isStaminaLoss) {
                // スタミナ無限でないなら.
                if(!isCanUseDash) {
                    nowStamina -= 0.1f;  // スタミナ減少.
                }

                if(nowStamina < 0) {
                    nowStamina = 0;  // スタミナはオーバーフローしない.
                    isStaminaLoss = true; // スタミナ切れに.
                }

                photonView.RPC(nameof(IsRunningChangeE), RpcTarget.All, true); // override追加項目.
                MoveType(moveForward , runSpeed, 1.5f);
            }else {
                photonView.RPC(nameof(IsRunningChangeE), RpcTarget.All, false); // override追加項目.
                MoveType(moveForward, walkSpeed, 1.0f);
                RegenerativeStaminaHeal();
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

    [PunRPC]
    private void LiloumoisES() {
        Destroy(this); // 削除.
    }

    [PunRPC]
    private void IsRunningChangeE(bool value) {
        isRunning = value;
    }

    /// <summary>
    /// カメラの中心直線上にレイを飛ばし、当たったオブジェクトを取得する.
    /// </summary>
    protected override void HookShot() {
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
    protected override IEnumerator LinearMove(Vector3 targetPos) {
        rb.useGravity = false;
        do {
            print("relative");
            var tmp = targetPos - transform.position;
            Vector3 direction = tmp.normalized; // 目標位置への方向ベクトルを計算
            relativeDistance = tmp.magnitude;
            float distance = speed * Time.deltaTime; // 目標位置への移動量を計算
            transform.position += direction * distance; // 目標位置に向かって移動

            //ベクトルの大きさが0.01以上の時に向きを変える処理をする
            if (relativeDistance > 0.01f) {
                transform.rotation = Quaternion.LookRotation(direction); //向きを変更する
            }

            yield return null; // 1フレーム遅延.
        } while(relativeDistance > HitDistance);

        anim.SetBool("HookShot", false);
        rb.useGravity = true;
        isUseAvility = false; // 発動終了. // override追加項目.

        StartCoroutine(AvillityCoolTime(10.0f)); // クールタイム. // override追加項目.
    }
}
