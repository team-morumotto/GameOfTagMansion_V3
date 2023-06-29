using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharactorPreviewScript : MonoBehaviour
{
    [SerializeField] GameObject[] charactorPreviewObject;
    [SerializeField] Text charactorPreviewNameText;
    [SerializeField] Text charactorPreviewSkillText;
    [SerializeField] Text charactorPreviewSpecText;
    [SerializeField] Text charactorPreviewOtherText;

    // Start is called before the first frame update
    void Start()
    {
        //初期化
        CharactorPreview(0);
    }
    /// <summary>
    /// キャラクターのプレビューを切り替える
    /// </summary>
    public void CharactorPreview(int cr){
        for(int i = 0; i < charactorPreviewObject.Length; i++){
                    charactorPreviewObject[i].SetActive(false);
                }
        switch(cr){
        case 0:
                charactorPreviewObject[0].SetActive(true);
                break;
        case 1:
                charactorPreviewObject[1].SetActive(true);
                break;
        case 2:
                charactorPreviewObject[2].SetActive(true);
                break;
        case 3:
                charactorPreviewObject[3].SetActive(true);
                break;
        case 4:
                charactorPreviewObject[4].SetActive(true);
                break;
        case 5:
                charactorPreviewObject[5].SetActive(true);
                break;
        case 6:
                charactorPreviewObject[6].SetActive(true);
                break;
        case 7:
                charactorPreviewObject[7].SetActive(true);
                break;
        case 8:
                charactorPreviewObject[8].SetActive(true);
                break;
        case 9:
                charactorPreviewObject[9].SetActive(true);
                break;
        }
    }
}
