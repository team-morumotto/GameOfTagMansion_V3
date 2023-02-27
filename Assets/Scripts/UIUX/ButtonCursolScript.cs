using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonCursolScript : MonoBehaviour,ISelectHandler,IPointerEnterHandler,IPointerExitHandler,IDeselectHandler
{
    [SerializeField] GameObject arrowImage; // 矢印を表示するImage
    //[SerializeField] float buttonDistance = -300; //ボタンとの距離
    [SerializeField] bool isCursol = true; //カーソル機能が必要かどうか
    [SerializeField] bool isTitle = true; //このスクリプトがついているオブジェクトはタイトルシーンのボタンかどうか
    [SerializeField] GameObject SE; //SEを鳴らすオブジェクト
    /*enum cursolType{ //カーソルがどの方向に動くか
        Horizontal,
        Vertical
    }
    [SerializeField] cursolType cursoltype; //カーソル現在参照方向*/
    private Vector3 Initcursorposition = new Vector3(); //カーソルの初期位置
    private float CursolrecttransformX; //カーソルの幅格納用

    void Start(){
        //ボタンの存在するシーンによってSEのオブジェクトを変更
        if(SE == null) {
            if(isTitle){
                SE = GameObject.Find("Button_SE_Obj");
            }
            else{
                SE = GameObject.Find("Obj_SE");
            }
        }

        //カーソル用の画像がアタッチされてない時は自動的にカーソル機能をオフにする
        if(arrowImage == null) {
            isCursol = false;
        }else{
            //カーソルの幅を取得
            CursolrecttransformX = arrowImage.GetComponent<RectTransform>().sizeDelta.x;
            //ボタンの初期X座標を記録
            Initcursorposition = arrowImage.transform.position;
        }
        //arrowImage.SetActive(false);
    }
    // ボタンがセレクトされている際に呼ばれるイベントハンドラ.
    public void OnSelect(BaseEventData eventData) {
        /*
        // SEを鳴らす.
        var selecteObj = gameObject.name;
        // マウスポインターでセレクトしたオブジェクトが
        if(GoToChooseChara.beforeSelectButton != selecteObj) {
            SE.GetComponent<Button_SE>().Call_SE(4);
        }

        //カーソル機能がいるとき
        if(isCursol){

            //ボタンの幅を取得
            var rtX =this.gameObject.GetComponent<RectTransform>().sizeDelta.x;
            //自分のオブジェクトの位置を一旦記録
            //ボタンの大きさが違うと同じ分量だけx座標をずらしただけでは同じ位置関係でカーソルが移動してくれない
            Vector3 selected = this.transform.position;

            print(selected.x/4);

            arrowImage.transform.position = new Vector3(selected.x-(selected.x/4), selected.y, selected.z);
            arrowImage.SetActive(true);
        }
        */
    }

    // マウスポインターがボタンに乗った場合.
    public void OnPointerEnter(PointerEventData eventData) {
        //GoToChooseChara.SetBeforeSelectButton(gameObject.name);
        // SEを鳴らす.
        var selecteObj = gameObject.name;
        if(GoToChooseChara.beforeSelectButton != selecteObj) {
            SE.GetComponent<Button_SE>().Call_SE(4);
        }

        GoToChooseChara.beforeSelectButton = gameObject.name; // 前に選択したボタンをオブジェクトを記録.

        if(isCursol){
            /*
            //ボタンの幅を取得
            var rtX = this.gameObject.GetComponent<RectTransform>().sizeDelta.x;
            //自分のオブジェクトの位置を一旦記録.
            // ボタンの大きさが違うと同じ分量だけx座標をずらしただけでは同じ位置関係でカーソルが移動してくれない.
            Vector3 selected = this.transform.position;
            print(rtX);

            arrowImage.transform.position = new Vector3(selected.x-(rtX+(rtX/2)), selected.y, selected.z);
            */
            arrowImage.SetActive(true);
        }
    }
    public void OnPointerExit(PointerEventData eventData){
        if(isCursol){
            arrowImage.SetActive(false);
        }
    }
    public void OnDeselect(BaseEventData eventData){
        //カーソル機能がいるとき
        if(isCursol){
            arrowImage.SetActive(false);
        }
    }
}
