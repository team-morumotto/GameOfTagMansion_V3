/*
    朝霧やのはの鬼のスクリプト
    アイテム効果が50%増幅する想定
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChaserAsakaYanoha : PlayerChaser
{
    [SerializeField]
    GameObject RedCube;
    const int addamplification = 50; // アイテム効果増幅用の変数に加算する値.
    void Start() {
        if(photonView.IsMine) {
            // 自分が逃げなら.
            if(GoToChooseChara.GetPlayMode() == 0) {
                photonView.RPC(nameof(YanohaCS), RpcTarget.AllBuffered);
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
        if(isCanUseAbility) {
            amplification = 0;
        }else {
            amplification = 50;
        }
        BaseUpdate();
    }

    [PunRPC]
    private void YanohaCS() {
        RedCube.SetActive(false);
        Destroy(this); // 削除.
    }
}

//------ 以下、固有性能 ------//
