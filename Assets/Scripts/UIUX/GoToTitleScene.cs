using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GoToTitleScene : MonoBehaviourPunCallbacks
{
    public void GotoTitle() {
        SceneManager.LoadScene("Closed_TitleScene",LoadSceneMode.Single);
    }

    public static void exeend() {
        //エディタの場合
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
        //ビルドの場合
        #else
            Application.Quit();//ゲームプレイ終了
        //なんかあったとき
        #endif
    }
}