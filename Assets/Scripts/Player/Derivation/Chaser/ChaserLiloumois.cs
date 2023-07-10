using System.IO;
/*
*   Created by Kobayashi Atsuki;
*   鬼のリルモワの専用スクリプト.
*/

using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ChaserLiloumois : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    void Start()
    {
        if(photonView.IsMine) {
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(LiloumoisCS), RpcTarget.AllBuffered);
            }
            //====== オブジェクトやコンポーネントの取得 ======//
            Init();
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        itemDatabase = GameObject.Find("ItemList").GetComponent<ItemDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update() {
        if(!photonView.IsMine) {
            return;
        }

        // 固有能力が使用可能か.
        if(isCanUseAbility) {
            if(Input.GetKeyDown(KeyCode.Space) && !isUseAvility && !isCoolTime) {
                isUseAvility = true;
                anim.SetBool("HookShot", true);
                HookShot();
            }
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
            if(nowStamina > 0 && Input.GetKey(KeyCode.LeftShift) && !isStaminaLoss) {
                // スタミナ無限でないなら.
                if(!isCanUseDash) {
                    nowStamina -= 0.1f;  // スタミナ減少.
                }

                if(nowStamina < 0) {
                    nowStamina = 0;  // スタミナはオーバーフローしない.
                    isStaminaLoss = true; // スタミナ切れに.
                }

                photonView.RPC(nameof(IsRunningChangeC), RpcTarget.All, true); // override追加項目.
                MoveType(moveForward , runSpeed, 1.5f,inputHorizontal, inputVertical);
            }else {
                photonView.RPC(nameof(IsRunningChangeC), RpcTarget.All, false); // override追加項目.
                MoveType(moveForward, walkSpeed, 1.0f,inputHorizontal, inputVertical);
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
    private void LiloumoisCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }

    [PunRPC]
    private void IsRunningChangeC(bool value) {
        isRunning = value;
    }
}
