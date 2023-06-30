/*
*   Created by Kobayashi atsuki.
*   2022/12/25 Atsuki Kobayashi.
*       ・Privateメンバ変数を返す関数を追加.
*       ・他クラスからは読み取り専用でアクセス可能.
*/

using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToChooseChara : MonoBehaviour {
    public static GoToChooseChara instance = null;
    public static int playMode = 1;
    public static int characters = 0;
    public static int actorNumber = -1;
    public static bool isEdit = false;
    public static string beforeSelectButton = "";

    void Awake() {
        // シングルトンである.
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    //------- ゲッター -------//
    public static int GetPlayMode() {
        return playMode;
    }
    public static int GetCharacters() {
        return characters;
    }
    public static bool GetIsEdit() {
        return isEdit;
    }
    //------- ゲッター -------//

    //------- ボタン -------//

    /// <summary>
    /// 逃げか鬼かを選択する.
    /// 仮引数 : 0 = 逃げ, 1 = 鬼.
    /// </summary>
    public void SetPlayMode() {
        if(playMode == 0) {
            playMode = 1;
        }else {
            playMode = 0;
        }
    }

    public void ResetPlayerMode() {
        playMode = 1;
    }

    /// <summary>
    /// キャラクターを選択する.
    /// 仮引数は左から0、1、2...
    /// 仮引数 : トラス、リルモワ、水鏡こよみ、NoranekoSeven、シャーロ、ミュリシア、ウェンルイ、ミーシェ、朝霞やのは、ナユ.
    /// </summary>
    public void setCharacters(int setCharacters) {
        characters = setCharacters;
    }

    // ルームの参加の自分の順番を記録.
    public static void ActorNumber(int value) {
        actorNumber = value;
    }

    // 最後にマウスポインターが乗っかったオブジェクト名を保存.
    public static void SetBeforeSelectButton(string value) {
        beforeSelectButton  = value;
    }

    public void LoadGameScene() {
        isEdit = true;
        SceneManager.LoadScene("Closed_GameScene",LoadSceneMode.Single);
    }

    /*public static void LoadSettingScene() {
        SceneManager.LoadScene("Setting_Scene",LoadSceneMode.Additive);
    }*/
}