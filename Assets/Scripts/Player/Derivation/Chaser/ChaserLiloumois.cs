using System.IO;
/*
*   Created by Kobayashi Atsuki;
*   鬼のリルモアの専用スクリプト.
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
        GetStatus(); // ステータスの取得.
    }

    void Update() {
        if(!photonView.IsMine) {
            return;
        }
        if(Input.GetKeyDown(KeyCode.I)) {
            FookShot();
        }
        PlayerMove();
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
                nowStamina -= 0.1f;  // スタミナ減少.
                if(nowStamina < 0) {
                    nowStamina = 0;  // スタミナはオーバーフローしない.
                    isStaminaLoss = true; // スタミナ切れに.
                }

                photonView.RPC(nameof(IsRunningChangeC), RpcTarget.All, true); // override追加項目.
                MoveType(moveForward , runSpeed, 1.5f);
            }else {
                photonView.RPC(nameof(IsRunningChangeC), RpcTarget.All, false); // override追加項目.
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
    private void LiloumoisCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }

     // override追加項目.
    [PunRPC]
    private void IsRunningChangeC(bool value) {
        isRunning = value;
    }


    //------ 以下、固有性能 ------//
    private float relativeDistance;
    private float HitDistance = 1.0f;
    private float speed = 30.0f; // 移動速度
    /// <summary>
    /// カメラの中心直線上にレイを飛ばし、当たったオブジェクトを取得する.
    /// </summary>
    protected void FookShot() {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            StartCoroutine(LinearMove(hit.point));
        }
    }

    /// <summary>
    /// 特定の位置に直線に向かう.
    /// </summary>
    /// <param name="targetPos">目標の位置</param>
    private IEnumerator LinearMove(Vector3 targetPos) {
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

        rb.useGravity = true;
    }
}
