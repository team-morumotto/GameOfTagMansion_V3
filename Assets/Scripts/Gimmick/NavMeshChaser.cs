//参考https://qiita.com/OKsaiyowa/items/6058106b13052a97219b
using UnityEngine;
using UnityEngine.AI;

public class NavMeshChaser : MonoBehaviour
{
    [SerializeField, Range(5, 50)] private float RunAwayDistance = 10f; //鬼との許容距離
    private NavMeshAgent MyAgent; //NavMeshAgent対象
    public GameObject[] NavPoint; //NavPointの配列
    private Vector3 SetPoint; //現在の目標地点記録用
    private GameObject NierNige; //逃げの位置入れる用
    enum Mode{  //状態管理用
        ゲーム開始前,
        逃げ発見,
        巡回中,
        ゲーム終了
    }
    private Mode mode = Mode.ゲーム開始前; //現在のモード
    //public int GamePlayerCount = 4; //ここに自分含めたプレイヤーの人数を入れてください
    void Start()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        MyAgent.autoBraking = false; //自動で減速しない
        NavPoint = GameObject.FindGameObjectsWithTag("NavPoint"); //代入できなかったらインスペクターで直に入れてください
    }

    void Update()
    {
        
        switch(mode){
            case Mode.ゲーム開始前:
                if(GameObject.FindGameObjectWithTag("Nigeru")){
                    GameObject nierObject = GameObject.FindGameObjectWithTag("Nigeru");
                    NierNige = nierObject;
                }
                //ここに開始前の待機時間とか突っ込んでから巡回中に変えてください
                if(NierNige != null){
                    mode = Mode.巡回中;
                    SetRandomPoint();
                }
                
                break;

            case Mode.巡回中:
                //プレイヤーとの距離が許容距離より近い時
                    if(RunAwayDistance>Vector3.Distance(transform.position,NierNige.transform.position)){
                        mode = Mode.逃げ発見;
                    }
                
                    //目標がないとき
                    if(SetPoint == this.transform.position||MyAgent == null){
                        SetRandomPoint();
                    }
                break;

            case Mode.逃げ発見:
                //Debug.Log("dis:" + Vector3.Distance(NavPoint[0].transform.position,transform.position));
                //プレイヤーとの距離が許容距離より近い時
                    if(RunAwayDistance>Vector3.Distance(transform.position,NierNige.transform.position)){
                        SetChasePoint(NierNige);
                        //Debug.Log("nununu");
                    }
                
                    else if(RunAwayDistance<Vector3.Distance(transform.position,NierNige.transform.position)){
                        mode = Mode.巡回中;
                        SetRandomPoint();
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

    //逃げ追跡
    private void SetChasePoint(GameObject go){
        MyAgent.SetDestination(go.transform.position);
        SetPoint = go.transform.position;
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
