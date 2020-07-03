using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomListManager : MonoBehaviourPunCallbacks
{
    public GameObject roomNamePrefab;
    public Transform gridLayout;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        for(int i=0;i<gridLayout.childCount;i++)
        {
            if(gridLayout.GetChild(i).gameObject.GetComponentInChildren<Text>().text == roomList[i].Name)
            {
                Destroy(gridLayout.GetChild(i));

                if(roomList[i].PlayerCount == 0)
                {
                    roomList.Remove(roomList[i]);
                }
            }
        }
        foreach(var room in roomList)
        {
            GameObject newRoom = Instantiate(roomNamePrefab, gridLayout.position, Quaternion.identity);

            newRoom.GetComponentInChildren<Text>().text = room.Name + " ( Player Num: " + room.PlayerCount + " ) ";

            newRoom.transform.SetParent(gridLayout);
        }
    }
}
