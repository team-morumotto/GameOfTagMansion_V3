/*
    2022/12/25 Atsuki Kobayashi
        ・Privateメンバ変数を返す関数を追加
        ・他クラスからは読み取り専用でアクセス可能
*/

using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToChooseChara : MonoBehaviour {
    public static GoToChooseChara instance = null;
    public static int PlayMode = 0;
    public static int Characters = 0;
    public static int actorNumber = -1;
    public static bool isSolo = false;
    public static bool isEdit = false;
    public static string beforeSelectButton = "";

    void Awake() {
        // シングルトンである.
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        Application.targetFrameRate = 60;
    }

    //------- ゲッター -------//
    public static int GetPlayMode() {
        return PlayMode;
    }
    public static int GetCharacters() {
        return Characters;
    }
    public static bool GetIsEdit() {
        return isEdit;
    }
    public static bool GetIsSolo() {
        return isSolo;
    }
    //------- ゲッター -------//

    //------- ボタン -------//
    /// <summary>
    /// 逃げか鬼かを選択する.
    /// 仮引数 : 0 = 逃げ, 1 = 鬼.
    /// 戻り値 : なし.
    /// </summary>
    public void SetPlayMode(int setPlayMode) {
        PlayMode = setPlayMode;
    }

    /// <summary>
    /// キャラクターを選択する.
    /// 仮引数 : 0 = おおとり, 1 = のらねこ, 2 = こよみ.
    /// 戻り値 : なし.
    /// </summary>
    public void setCharacters(int setCharacters) {
        Characters = setCharacters;
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

    public static void LoadSettingScene() {
        SceneManager.LoadScene("Setting_Scene",LoadSceneMode.Additive);
    }
}