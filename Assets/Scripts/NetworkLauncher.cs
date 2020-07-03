using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class NetworkLauncher : MonoBehaviourPunCallbacks
{
    public GameObject loginUI;
    public GameObject nameUI;
    public InputField roomName;
    public InputField playerName;
    public GameObject roomListUI;


    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); //using unity settings
    }

    private void Update()
    {
    }

    //connect to server
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connect to the Master");
        nameUI.SetActive(true);

        //加入游戏大厅
        PhotonNetwork.JoinLobby();

    }

    public void PlayButton()
    {
        if(playerName.text.Length ==0)
        {
            return;
        }
        nameUI.SetActive(false);
        PhotonNetwork.NickName = playerName.text;
        loginUI.SetActive(true);
        if(PhotonNetwork.InLobby)
        {
            roomListUI.SetActive(true);
        }
    }

    public void JoinOrCreateButton()
    {
        if (roomName.text.Length < 2)
            return;

        loginUI.SetActive(false);

        RoomOptions options = new RoomOptions { MaxPlayers = 10 };
        PhotonNetwork.JoinOrCreateRoom(roomName.text, options, default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        PhotonNetwork.LoadLevel(1);
    }
}
