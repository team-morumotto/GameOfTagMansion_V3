using UnityEngine;

public class CharacterTeleporter : MonoBehaviour
{
    public GameObject TeleportPoint; // テレポート先
    public bool isFloatingFloor = false;
    private GameObject RespownPoint; // こちらにテレポートしてきたときにスポーンする位置
    private GameObject FloatingFloor; // テレポータ―の上の浮いてる床部分
    private float PerlinNoisetime; // time加算用
    private float FloatingFloorY; // 浮いてる床の高さ

    void Start()
    {
        RespownPoint = transform.Find("RespownPoint").gameObject;

        //浮いている床があるとき
        if(isFloatingFloor){
            FloatingFloor = transform.Find("FloatingFloor").gameObject;
            FloatingFloorY = FloatingFloor.transform.position.y;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //時間の加算
        PerlinNoisetime += Time.deltaTime;

        //浮いている床があるとき
        if(isFloatingFloor){
            FloatingFloor.transform.position = new Vector3(FloatingFloor.transform.position.x, FlootingPerlinNoise(PerlinNoisetime,FloatingFloorY), FloatingFloor.transform.position.z);
        }
    }

    //ノイズの精製
    float FlootingPerlinNoise(float t,float floatingfloorY){
        return Mathf.PerlinNoise(t, 0) + floatingfloorY;
    }

    // キャラクターが触れたらTeleportPoint(テレポート先)に移動する
    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player")){
            collision.gameObject.transform.position = TeleportPoint.GetComponent<CharacterTeleporter>().RespownPoint.transform.position;
            GameObject.Find("Obj_SE").GetComponent<Button_SE>().CallButtonSE(1);
        }
    }
}


