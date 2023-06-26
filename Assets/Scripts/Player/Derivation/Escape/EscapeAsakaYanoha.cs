/*
    朝霧やのはの逃げのスクリプト
    アイテム効果が50%増幅する想定
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EscapeAsakaYanoha : PlayerEscape
{
    const int addamplification = 50; // アイテム効果増幅用の変数に加算する値.
    void Start() {
        if(photonView.IsMine) {
            // 自分が鬼なら.
            if(GoToChooseChara.GetPlayMode() == 1) {
                photonView.RPC(nameof(YanohaES), RpcTarget.AllBuffered);
            }
            Init(); // オブジェクトやコンポーネントの取得.
             //アイテムの効果増幅用の変数に値を代入.(パーセンテージで増幅)
            amplification = addamplification;
        }
        characterDatabase = GameObject.Find("CharacterStatusList").GetComponent<CharacterDatabase>();
        GetStatus(); // ステータスの取得.
    }

    void Update () {
        if(!photonView.IsMine) {
            return;
        }
        if(Input.GetKeyDown(KeyCode.I)) {
            // 固有性能はここから使用する.
        }
        BaseUpdate();
    }

    [PunRPC]
    private void YanohaES() {
        Destroy(this); // 削除.
    }
}

//------ 以下、固有性能 ------//
