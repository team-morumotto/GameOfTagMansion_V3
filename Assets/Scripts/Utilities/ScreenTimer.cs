using UnityEngine;
using Photon.Pun;

/*
    ### Screen Timer - Kashiwaya Rikuto ###
    2023/05/19 更新
    ・余分なコードを削減
    ・冗長なインポートを削除
    ・Player_Functionからtimerに変更

    2022/12/08 更新
    ・時間表示をPhotonNetwork.ServerTimestampを使用して表示するように変更
    ・開始時間、終了時間、その他Timestampを外部から取得できるように設定
*/

namespace Smile_waya {
    namespace GOM {
        namespace ScreenTimer {
            public class ScreenTimer {
                //------------- 変数宣言 -------------//

                //------------ Static変数 ------------//
                private int gameTimeLimit = 180;       // カウントダウンの時間(ローカル時間が引かれるため可変)

                //------------ Public変数 ------------//
                ///<summary>スポーンワールドの階層を入力したもの</summary>
                public int[] countTime = new int[3];     // 【カウントダウンの秒数格納】Minute, Second, Millisecond(1000ms)
                public int svTimeStart;     // ゲーム開始時のサーバーの時間を取得
                public int svTimeOver;      // ゲーム終了時のサーバーの時間を取得
                public int svTimeStamp;     // ゲーム開始時のサーバーのタイムスタンプを取得
                public int svTimeCountDown; // ゲーム中のサーバーのカウントダウン（制限時間からカウントダウン）
                public int svTimeCountUp;   // ゲーム中のサーバーのカウントアップ（0からカウントアップ）
                ///<summary>
                /// timeCountLocalDeltaは0.0fで宣言すると正常に動作しません。
                /// （プレイヤーが最初から0.0fでLoadされるためすぐ終了してしまう）
                /// そのため、初期値を1.0fにしています。別の場所でMasterConfigよりLoadしているので
                /// 上記初期値は特別な理由がなければ *変更を加えない* でください。
                ///</summary>
                public float timeCountLocalDelta = 1.0f; // 【カウントダウンの秒数格納】DeltaTime(Local)

                //----------- Private 変数 -----------//
                private bool hasCounterStartUp = false; // カウントダウン開始フラグ
                private bool isInit = false;

                //----------- 変数宣言終了 -----------//

                // 引数を複数返却(タプル式) 参考:https://www.create-forever.games/return-tuple/.
                ///<summary>UI上のカウントダウン処理</summary>
                public (string gameTimeStr, int gameTimeInt) GameTimeCounter() {
                    TimeCount();          // カウントダウン処理
                    CountTimer_StartUp(); // カウント起動管理フラグ
                    int LocalSecond = svTimeCountDown / 1000;

                    // 分数を求める カウントダウンの秒数を60で割った商
                    countTime[0] = (svTimeCountDown / 1000) / 60;

                    // 秒数を求める カウントダウンの秒数を60で割った余り 60秒以上の秒数がある場合は60*nで引く
                    countTime[1] = (svTimeCountDown / 1000) - (60 * countTime[0]);

                    // ミリ秒を求める カウントダウンの秒数を1000で掛けて割った余り
                    countTime[2] = svTimeCountDown - (LocalSecond * 1000);
                    if (isInit) {
                        // 00:00.000のString形式でReturnする
                        return (countTime[0].ToString("00") + ":" + countTime[1].ToString("00") + "." + countTime[2].ToString("000"),
                                svTimeCountDown);
                    }
                    else {
                        isInit = true;
                        return ("00:00.000", 120000);
                    }
                }

                public void TimeCount() {
                    svTimeStamp = PhotonNetwork.ServerTimestamp;      // サーバーの時間を取得
                    svTimeCountDown = (svTimeOver - svTimeStamp);     // カウントダウン処理
                    svTimeCountUp = (svTimeStart - svTimeStamp) * -1; // カウントアップ処理
                }

                //########## ここから先はFunction使用時の初期化用 ###########//
                // 使用時の初期化 起動時にどっかで叩いてもらわないと動けない //
                //###########################################################//
                void CountTimer_StartUp() {
                    if (hasCounterStartUp) return; // 既に起動していたら処理を終了する
                    hasCounterStartUp = true;     // 起動フラグを立てる

                    countTime[0] = 0;  // 分
                    countTime[1] = 0;  // 秒
                    countTime[2] = 0;  // ミリ秒
                    svTimeStart = PhotonNetwork.ServerTimestamp;                   // ゲーム開始時のサーバーの時間を取得
                    svTimeOver = svTimeStart + (gameTimeLimit * 1000); // ゲーム終了時のサーバーの時間を取得 *1000はミリ秒分かさ増しのため。
                }
            }
        }
    }
}