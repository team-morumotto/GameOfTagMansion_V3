/*
    これはボタンを押した時に鳴らすSEを管理するスクリプト

    2022/12/25 Atsuki Kobayashiが編集
        ・Button押下時に与えられる引数に対応するSEを鳴らすButton_Selected関数を作成(バラバラだったものを統合)

*/
using UnityEngine;

public class Button_SE : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip[] buttonSE;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Button_Selected(string buttonType){
        switch(buttonType){
            case "Back":
                audioSource.PlayOneShot(buttonSE[0]); // 戻るSE
                break;
            case "Select":
                audioSource.PlayOneShot(buttonSE[1]); // 選ぶSE
                break;
            case "Member":
                audioSource.PlayOneShot(buttonSE[2]); // ルームの人数指定SE
                break;
            default:
            Debug.LogError("与えられた文字列【"+buttonType+"】に対応するSEがありません");
            break;
        }
    }

    public void Call_SE(int SEnumber){
        audioSource.PlayOneShot(buttonSE[SEnumber]);
    }
}
