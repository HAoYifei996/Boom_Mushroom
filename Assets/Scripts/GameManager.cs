using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;


public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject readyButton;
    private bool is_Ready = false;
    static int escCount = 0;

    public GameObject escMenu;
    public Button btnResume;
    public Button btnExit;
    public bool is_clickEscMenu = false;
    
    
    // Start is called before the first frame update
    void Start()
    {
        Screen.fullScreen = true;
        
    }

    public void ReadyToPlay()
    {
        readyButton.SetActive(false);

        PhotonNetwork.Instantiate("Player", new Vector3(0, 3, -10), Quaternion.identity, 0);

        Cursor.lockState = CursorLockMode.Locked;//锁定指针到视图中心
        Cursor.visible = false;

        is_Ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(is_Ready)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                escCount++;
            }
            if (escCount % 2 == 0)
            {
                Cursor.lockState = CursorLockMode.Locked;//锁定指针到视图中心
                Cursor.visible = false;

                escMenu.SetActive(false);
                is_clickEscMenu = false;
            }
            else
            {

                Cursor.lockState = CursorLockMode.None;//锁定指针到视图中心
                Cursor.visible = true;

                escMenu.SetActive(true);
                is_clickEscMenu = true;
            }
        }
        
    }

    public void ClickExitButton()
    {
        Application.Quit();
    }

    public void ClickResumeButton()
    {
        escCount++;
    }


}
