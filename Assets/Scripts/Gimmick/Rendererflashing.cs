//参考https://futabazemi.net/unity/alpha_change
using System.Collections;
using UnityEngine;
 
public class Rendererflashing : MonoBehaviour
{
  MeshRenderer mesh;
 
  void Start ()
  {
      mesh = GetComponent<MeshRenderer>();
      //StartCoroutine("Transparent");
  }
 
  IEnumerator Transparent()
  {
      //一瞬色を付ける
      mesh.material.color = new Color32(255,255,255,100);
      //徐々に透明度を下げる
      for ( int i = 0 ; i < 100 ; i++ ){
          mesh.material.color = mesh.material.color - new Color32(0,0,0,1);
          yield return new WaitForSeconds(0.01f);
      }
  }
  //当たったら発動
  void OnCollisionEnter(Collision collision){
    if(collision.gameObject.tag == "Oni"||collision.gameObject.tag == "Nigeru"){
        StartCoroutine("Transparent");
    }
  }   
}