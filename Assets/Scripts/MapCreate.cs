using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCreate : MonoBehaviour
{

    [SerializeField]GameObject buildingObject; //建物
    [SerializeField]GameObject floorObject; //床
    [SerializeField]Vector3 objectSize = new Vector3(2, 2, 2); //オブジェクトのサイズ
    [SerializeField]Vector2 mapSize = new Vector2(10, 10); //マップのサイズ
    private int[,] map; //マップの配列
    [SerializeField]Vector2 roadCount = new Vector2(0, 0); //ランダムに作るうえでの道の数
    [SerializeField]int buildingHeight = 10; //建物をいくつまで積み上げるか 
    // Start is called before the first frame update
    void Start(){
        map = new int[(int)mapSize.x, (int)mapSize.y];
        roadCount = new Vector2(Random.Range(5,mapSize.x/10), Random.Range(5,mapSize.y/10));
        MapGenerator();
    }

    void Update(){
        
    }
    void MapGenerator(){
        //初期化
        for(int i = 0; i < mapSize.x; i++){
            for(int j = 0; j < mapSize.y; j++){
                map[i, j] = 1;
            }
        }

        int road =0;
        //縦の処理
        for(int i = 0; i < roadCount.x; i++){
            road = Random.Range(0, (int)mapSize.x);
            if(map[road,0] == 0){continue;}
            for(int j = 0; j < mapSize.y; j++){
                map[road, j] = 0;
            }
        }

        //横の処理
        for(int i = 0; i < roadCount.y; i++){
            road = Random.Range(0, (int)mapSize.y);
            if(map[0, road] == 0){continue;}
            for(int j = 0; j < mapSize.x; j++){
                map[j, road] = 0;
            }
        }


        //生成処理
        for(int i = 0; i < mapSize.x; i++){
            for(int j = 0; j < mapSize.y; j++){
                //マンションの生成
                if(map[i, j] == 1){
                    //高さをランダムに
                    for(int n=0;n<Random.Range(1, buildingHeight);n++){
                        GameObject obj = Instantiate(buildingObject, new Vector3(i * objectSize.x, 1+n*2, j * objectSize.z), Quaternion.identity);
                    }
                //道の生成
                }else if(map[i, j] == 0){

                    GameObject obj = Instantiate(floorObject, new Vector3(i * objectSize.x, 0, j * objectSize.z), Quaternion.identity);
                }
            }
        }

    }
}
