using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharactorPreviewScript : MonoBehaviour {
    [SerializeField] GameObject[] charactorPreviewObject; // キャラプレビュー(object)
    [SerializeField] Text charactorPreviewNameText;       // キャラ名前(text)
    [SerializeField] Text charactorPreviewSkillText;      // キャラスキル(text)
    [SerializeField] Text charactorPreviewSpecText;       // キャラ性能(text)
    [SerializeField] Text charactorPreviewOtherText;      // キャラ備考(text)

    // Start is called before the first frame update
    void Start() {
        //初期化
        CharactorPreview(0);
    }
    /// <summary>
    /// キャラクターのプレビューを切り替える
    /// 0: トラスとウェッジ
    /// 1: リルモア
    /// 2: 水鏡こよみ
    /// 3: NoranekoSeven
    /// 4: シャーロ Pure
    /// 5: ミュリシア
    /// 6: ウェンルイ
    /// 7: ミーシェ
    /// 8: 朝霞やのは
    /// 9: ナユ
    /// </summary>
    public void CharactorPreview(int cr) {
        for(int i = 0; i < charactorPreviewObject.Length; i++) {
            charactorPreviewObject[i].SetActive(false);
        }
        switch(cr){
            case 0:
                charactorPreviewObject[0].SetActive(true);
                charactorPreviewNameText.text = "トラスとウェッジ";
                break;
            case 1:
                charactorPreviewObject[1].SetActive(true);
                charactorPreviewNameText.text = "リルモア";
                break;
            case 2:
                charactorPreviewObject[2].SetActive(true);
                charactorPreviewNameText.text = "水鏡こよみ";
                break;
            case 3:
                charactorPreviewObject[3].SetActive(true);
                charactorPreviewNameText.text = "NoranekoSeven";
                break;
            case 4:
                charactorPreviewObject[4].SetActive(true);
                charactorPreviewNameText.text = "シャーロ Pure";
                break;
            case 5:
                charactorPreviewObject[5].SetActive(true);
                charactorPreviewNameText.text = "ミュリシア";
                break;
            case 6:
                charactorPreviewObject[6].SetActive(true);
                charactorPreviewNameText.text = "ウェンルイ";
                break;
            case 7:
                charactorPreviewObject[7].SetActive(true);
                charactorPreviewNameText.text = "ミーシェ";
                break;
            case 8:
                charactorPreviewObject[8].SetActive(true);
                charactorPreviewNameText.text = "朝霞やのは";
                break;
            case 9:
                charactorPreviewObject[9].SetActive(true);
                charactorPreviewNameText.text = "ナユ";
                break;
            case default:
                charactorPreviewNameText.text = "";
                charactorPreviewSkillText.text = "";
                charactorPreviewSpecText.text = "";
                charactorPreviewOtherText.text = "";
                break;
        }
    }
}
