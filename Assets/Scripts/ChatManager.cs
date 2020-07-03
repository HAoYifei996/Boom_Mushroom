using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Chat;
using Photon.Realtime;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    public GameObject chatContentPrefab;
    private Transform gridLayout;
    private GameObject chatArea;

    public ChatClient client;
    private string AppID;           
    private string AppVersion;      
    public string userName = "!null";
    
    

    // Start is called before the first frame update
    void Start()
    {
        gridLayout = GameObject.Find("ChatContent").transform;
        chatArea = GameObject.Find("ChatArea");
        client = new ChatClient(this);
        AppID = ChatSettings.Load().AppId;
        AppVersion = "1";
        Debug.Log("On connecting... ");
        while(userName=="!null")
        {
            
        }
        client.Connect(AppID, AppVersion, new Photon.Chat.AuthenticationValues(userName));


    }

    // Update is called once per frame
    void Update()
    {
        if (client != null)   
        {
            client.Service();
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        throw new System.NotImplementedException();
    }

    public void OnChatStateChange(ChatState state)
    {
        throw new System.NotImplementedException();
    }

    public void OnConnected()
    {
        Debug.Log("Connected...");

        //下面这个方法用于订阅频道，只有订阅频道之后，才能接受到这个频道的信息，比如现在有人在工会聊天，你就可以接受到。
        client.Subscribe(new string[] { "all" });//这里我们订阅了世界聊天频道和工会聊天频道


        //这个方法用于向频道发送消息，这里我们向世界聊天频道发送了大家好，那么订阅了这个频道的人都能接受到这条消息。
        client.PublishMessage("all", "大家好");

        //这个方法用于发送私人消息，这里我们向一个叫小小的用户发送了一条消息“你好”。如果这个用户在线的话，他将收到这条消息。
        //client.SendPrivateMessage("小小", "你好");
    }

    public void OnDisconnected()
    {
        throw new System.NotImplementedException();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        chatArea.SetActive(true);
        Debug.Log("频道：" + channelName + ",发送者：" + senders[0] + ", 消息内容：" + messages[0]);
        for(int i =0;i<senders.Length;i++)
        {
            GameObject newChatContent = Instantiate(chatContentPrefab, gridLayout.position, Quaternion.identity);

            newChatContent.GetComponentInChildren<Text>().text = "频道：" + channelName + "发送者：" + senders[i] + ", 消息内容：" + messages[i];

            newChatContent.transform.SetParent(gridLayout);
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new System.NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        throw new System.NotImplementedException();
    }

    public void OnUnsubscribed(string[] channels)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

   
}
