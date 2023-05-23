/*
    2022/12/29 Atsuki Kobayashi
*/
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using Smile_waya.GOM.ScreenTimer;
using UnityEngine.Serialization;
using System;

public class PlayerEscape : PlayerBase {
    //----------- protected変数 -----------//
    protected GameObject offScreen; // ほかプレイヤーの位置を示すマーカーを管理するオブジェクト.
    //----------- Private変数 -----------//
    private ScreenTimer ST = new ScreenTimer(); // プレイヤーの機能をまとめたクラス. 
    //----------- 変数宣言終了 -----------//

    /// <summary>
    /// 機能 : LeftShiftを押すとスニークを切り替え.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    protected void Sneak() {
        switch(isSneak) {
            case true:
                if(Input.GetKeyDown(KeyCode.LeftShift)) {
                    anim.SetBool("Sneak", false);
                    isSneak = false; // スニークフラグOFF.

                    PhotonMatchMaker.SetCustomProperty("h", false, 0);
                }
            break;

            case false:
                if(Input.GetKeyDown(KeyCode.LeftShift)) {
                    anim.SetBool("Sneak", true);
                    isSneak = true; // スニークフラグON.

                    PhotonMatchMaker.SetCustomProperty("h", true, 0);
            }
            break;
        }
    }

    /// <summary>
    /// ゲームの制限時間カウント.
    /// 引数 : なし.
    /// 戻り値 : なし.
    /// </summary>
    protected void GameTimer() {
        var gameTime =ST.GameTimeCounter();

        // テキストへ残り時間を表示
        countDownText.text = gameTime.gameTimeStr;

        // 残り時間が5秒以下なら.
        if(gameTime.gameTimeInt <= 5000) {
            countDownText.color = Color.red; // 赤色に指定.
        }

        // 時間切れになったら.
        if(gameTime.gameTimeInt <= 0){
            resultWinLoseText.text = "You Win!";
            resultWLText.text = "逃げ切った！";         //テキストを表示
            GameEnd(true);                             //ゲーム終了処理
        }
    }
}