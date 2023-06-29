using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FowardSliderScript : MonoBehaviour
{
    [SerializeField] private Text Text_Contents;
    [SerializeField] private Button _ButtonNext;
    [SerializeField] private Button _ButtonPrevious;
    public enum Mode{
        Oni,
        Nige
    }
    public List<Mode> modeList = new List<Mode>();
    private int _MaxPageNumber;
    private int _NowPageNumber = 1;
    // Start is called before the first frame update
    void Start()
    {
        _MaxPageNumber = modeList.Count;
        ChangeContents(_NowPageNumber);
        _ButtonNext.onClick.AddListener(OnNextPaper);
        _ButtonPrevious.onClick.AddListener(OnPreviousPaper);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     [System.Serializable]
    public class Blog
    {
        public string Title;
        [TextArea(1, 10)] public string Contents;


        public Blog(string title ,string contents)
        {
            Title = title;
            Contents = contents;
        }
     }

    private void OnNextPaper()
    {
        if (_NowPageNumber != _MaxPageNumber)
        {
            ChangeContents(_NowPageNumber + 1);
        }

    }

    private void OnPreviousPaper()
    {
        if (_NowPageNumber != 1)
        {
            ChangeContents(_NowPageNumber - 1);
        }

    }
    private void ChangeContents(int pageNumber)
    {
        _NowPageNumber = pageNumber;


    }
}
