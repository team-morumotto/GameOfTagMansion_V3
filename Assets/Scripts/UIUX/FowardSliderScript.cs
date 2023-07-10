using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FowardSliderScript : MonoBehaviour
{
    [SerializeField] private Text Text_Contents; //表示するテキスト
    [SerializeField] private Button buttonNext; //次のページに進むボタン
    [SerializeField] private Button buttonPrevious; //前のページに戻るボタン
    public enum Mode{ //表示する内容
        鬼,
        逃げ
    }
    public List<Mode> modeList = new List<Mode>(); //表示される順番(インスペクターで入力)
    private int _MaxPageNumber; //最大ページ数
    private int _NowPageNumber = 1;
    [SerializeField] private GameObject nigeCharacterPalel;
    [SerializeField] private GameObject oniCharacterPalel;
    [SerializeField] private CharacterPreviewManager characterPreview;
    // Start is called before the first frame update
    void Start()
    {
        _MaxPageNumber = modeList.Count;
        ChangeContents(_NowPageNumber);
        buttonNext.onClick.AddListener(OnNextPaper);
        buttonPrevious.onClick.AddListener(OnPreviousPaper);
    }

    /// <summary>
    /// ページを変更する
    /// </summary>
    private void OnNextPaper()
    {
        if (_NowPageNumber != _MaxPageNumber)
        {
            ChangeContents(_NowPageNumber + 1);
        }
        else{
            ChangeContents(1);
        }
    }

    /// <summary>
    /// ページを戻る
    /// </summary>
    private void OnPreviousPaper()
    {
        if (_NowPageNumber != 1)
        {
            ChangeContents(_NowPageNumber - 1);
        }
        else{
            ChangeContents(_MaxPageNumber);
        }
    }

    /// <summary>
    /// ページの内容を変更する
    /// </summary>
    private void ChangeContents(int pageNumber)
    {
        _NowPageNumber = pageNumber;
        Text_Contents.text = modeList[_NowPageNumber - 1].ToString();
        if(_NowPageNumber == 1){
            nigeCharacterPalel.SetActive(false);
            oniCharacterPalel.SetActive(true);
            characterPreview.characterPreview(1);
        }
        else{
            nigeCharacterPalel.SetActive(true);
            oniCharacterPalel.SetActive(false);
            characterPreview.characterPreview(0);
        }
    }
}
