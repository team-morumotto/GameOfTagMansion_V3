using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToPlayerSelect : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject PlayerSelectPanel;
    public GameObject TitleMenuPanel;
    void Start() {
        PlayerSelectPanel.SetActive(false);
    }
    public void gotoplayerselect(){
        PlayerSelectPanel.SetActive(true);
        TitleMenuPanel.SetActive(false);
    }
}