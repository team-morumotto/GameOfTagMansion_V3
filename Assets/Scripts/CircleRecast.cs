/*
    参考サイト https://clrmemory.com/programming/unity/circle-gauge-meter-p1/

    アイテムを使用した際の効果時間やUIを、可視化/不可視化するためのスクリプト.
    2023/01/25 Atsuki Kobayashi.
*/

using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class CircleRecast : MonoBehaviourPunCallbacks
{
    public Image RecastImage; // サークルでのリキャスト表現を行う画像.
    public Image ItemImage;   // アイテムの画像.
    public float RecastTime;  // リキャストタイム.
    public GameObject itemHowUse;    // アイテムの使用.
    void Update() {
        // アイテムを持っていたら.
        if(PlayerChaser.isHaveItem || PlayerEscape.isHaveItem) {
            itemHowUse.SetActive(true);
            ItemImage.enabled = true; // アイテムの画像を可視化.
        }else{
            itemHowUse.SetActive(false);
            ItemImage.enabled = false; // アイテムの画像を不可視化.
        }

        // アイテムを使用したら.
        if(PlayerChaser.isUseItem || PlayerEscape.isUseItem) {
            RecastImage.fillAmount -= 1.0f / RecastTime * Time.deltaTime; // 円状のゲージを減少させていく.
            RecastImage.enabled = true; // ゲージの画像を可視化.
            itemHowUse.SetActive(false);
        }else{
            RecastImage.fillAmount = 1.0f; // ゲージの塗りつぶしをMaxで初期化.
            RecastImage.enabled = false; // ゲージの画像を不可視化.
        }
    }
}