using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeWenrui : PlayerEscape
{
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(WenruiES), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update () {
        if(!photonView.IsMine) {
            return;
        }

        // 固有能力が使用可能か.
        if(isCanUseAbility) {
            if(Input.GetKeyDown(KeyCode.Space) && !isUseAvility && !isCoolTime) {
                isUseAvility = true;
                SE.CallAvilitySE(5); // SE.
                StartCoroutine(AvilityEffectLoop(EffectDatabase.avilityEffects[5]));
                StartCoroutine(BillCircle());
            }
        }
        BaseUpdate();
    }

    void OnTriggerEnter(Collider collider) {
        if(!photonView.IsMine) {
            return;
        }

        // すでにスタンしているなら処理しない.
        if(isStan) {
            print("スタン済み");
            return;
        }

        // 当たったオブジェクトが障害物なら.
        if(collider.CompareTag("Obstruct")) {
            isHit++;
            Destroy(collider.gameObject); // 破棄.
            StartCoroutine(Stan());
        }

        // 当たったオブジェクトが御札なら.
        if(collider.CompareTag("Bill")) {
            // 自分が生成した御札に触れてスタンするのを防ぐ.
            foreach(var bills in billList) {
                if(collider.gameObject == bills) {
                    return;
                }
            }

            if(!isSlow) {
                isSlow = true;
                StartCoroutine(DelayChangeFlg("Slow"));
            }
        }
    }

    [PunRPC]
    private void WenruiES() {
        Destroy(this); // 削除.
    }
}