using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class NewItemScript : MonoBehaviour
{
    [SerializeField] private Vector3 rotateSpeed = new Vector3(5,5,5);
    [SerializeField] private GameObject rotateObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //まわす
        rotateObject.transform.Rotate(rotateSpeed);
        ItemLookCamera();
        

    }
    ///<sammary>
    ///ビルボードみたいにカメラに向ける
    ///</sammary>
    void ItemLookCamera(){
        //カメラに向ける
        var c = Camera.main.transform.position;
        var p = transform.position;
        c.y = p.y;
        transform.LookAt(2 * p - c);
    }
    ///<sammary>
    ///プレイヤーがアイテムを取得したときの処理
    ///</sammary>
    void OnCollisionEnter(Collision collision){
        //プレイヤー以外は無視
        if (collision.gameObject.tag != "Player")return;
        //アイテムの列挙型の最大値を取得
        var a = System.Enum.GetValues(typeof(PlayerBase.ItemName)).Length;
        //アイテムの列挙型の最大値の中からランダムでアイテムを取得
        var b = UnityEngine.Random.Range(0,a) ;
        //アイテムの名前を取得
        PlayerBase.ItemName ii = (PlayerBase.ItemName)Enum.ToObject(typeof(PlayerBase.ItemName), b);
        collision.gameObject.GetComponent<PlayerBase>().ItemGet(ii);
        Debug.Log("アイテムとれたよ");
        Destroy(this.gameObject);
        
    }
}
