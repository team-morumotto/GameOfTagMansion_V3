//参考https://qiita.com/OKsaiyowa/items/6058106b13052a97219b
using UnityEngine;
using UnityEngine.AI;

public class NavMeshNige : MonoBehaviour
{
    //プレイヤーとの許容距離
    [SerializeField, Range(5, 50)] private float RunAwayDistance = 10f; //鬼との許容距離
    private NavMeshAgent MyAgent; //NavMeshAgent対象
    public GameObject[] NavPoint; //NavPointの配列
    private Vector3 SetPoint; //現在の目標地点
    private GameObject NierOni; //  鬼の位置入れる用
    private bool InitSet = false; //鬼が近くにいる状態での初期設定用
    enum Mode{  //状態管理用
        ゲーム開始前,
        鬼発見,
        巡回中,
        ゲーム終了
    }
    private Mode mode = Mode.ゲーム開始前; //現在のモード
    void Start()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        MyAgent.autoBraking = false; //自動で減速しない

    }

    void Update()
    {
        
        switch(mode){
            case Mode.ゲーム開始前:
                //念のため鬼がはいってるか確認
                if(NierOni == null){
                    GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Oni");
                    foreach (GameObject gameObj in gameObjects){
                        if(gameObj != this.gameObject){
                            NierOni = gameObj;
                        }
                        /*if(NierOni == this.gameObject){
                            NierOni = null;
                        }*/
                    }  
                }
                //ここに開始前の待機時間とか突っ込んでから巡回中に変えてください
                if(NierOni != null&&NierOni != this.gameObject){
                    mode = Mode.巡回中;
                    SetRandomPoint();
                }
                
                break;

            case Mode.巡回中:
                //プレイヤーとの距離が許容距離より近い時
                if(RunAwayDistance>Vector3.Distance(transform.position,NierOni.transform.position)){
                    mode = Mode.鬼発見;
                }
                //目標がないとき
                else if(SetPoint == this.transform.position||MyAgent == null){
                    SetRandomPoint();
                }
                break;

            case Mode.鬼発見:
                //Debug.Log("dis:" + Vector3.Distance(NavPoint[0].transform.position,transform.position));
                //プレイヤーとの距離が許容距離より近い時
                if(RunAwayDistance>Vector3.Distance(transform.position,NierOni.transform.position)){
                    if(!InitSet){
                        SetRandomRangePoint();
                        InitSet = true;
                    }
                    if(SetPoint == transform.position){
                        SetRandomRangePoint();
                    }
                    //Debug.Log("nununu");
                }
                else if(RunAwayDistance<Vector3.Distance(transform.position,NierOni.transform.position)){
                    mode = Mode.巡回中;
                    SetRandomPoint();
                    InitSet = false;
                }
                break;

            case Mode.ゲーム終了:
                //ここにつかまったときの処理書いてください
                break;
        }
    }

    //navpointからランダムな目標地点を設定
    private void SetRandomPoint(){
        int randomIndex = Random.Range(0, NavPoint.Length);
        MyAgent.SetDestination(NavPoint[randomIndex].transform.position);
        SetPoint = NavPoint[randomIndex].transform.position;
    }

    //ランダムな位置を目標地点に設定
    private void SetRandomRangePoint(){
        var Range = Random.Range(-1000, 1000);
        MyAgent.SetDestination(new Vector3(Range,transform.position.y,Range));
        SetPoint = new Vector3(Range,transform.position.y,Range);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        //巡回状態の時に目標地点についたら目標地点を変更
        if(other.gameObject.transform.position == SetPoint){
            SetRandomPoint();
        }

        //触れられたときにここに処理書いてください
        
    }

    
}
