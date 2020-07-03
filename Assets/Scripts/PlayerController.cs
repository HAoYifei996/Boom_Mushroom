using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine.UI;
using Photon.Chat;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerController : MonoBehaviourPun, IPunObservable, IChatClientListener
{
    public void Initialize(GameObject character)
    {
        m_animator = character.GetComponent<Animator>();
        m_rigidBody = character.GetComponent<Rigidbody>();
    }


    [SerializeField] private float m_moveSpeed = 2.0f;
    [SerializeField] private float m_jumpForce = 4;

    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;


    private float m_currentV = 0;
    private float m_currentH = 0;

    private readonly float m_interpolation = 10;
    private readonly float m_runScale = 2.0f;


    private bool m_wasGrounded;
    private Vector3 m_currentDirection = Vector3.zero;

    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;

    private bool m_isGrounded;
    
    private List<Collider> m_collisions = new List<Collider>();

    private float m_shootDamage = 6f;
    private float m_shootRange = 100f;
    public float m_playerHealth = 100f;
    public bool is_playerDead = false;
    private bool m_amIShoot = false;
    private bool m_amIShootHead = false;
    public bool m_isShooted = false;
    public bool m_isShootedHead = false;
    private float m_timerAmIShootHead = 0.2f;
    private float m_timerIsShooted = 1f;
    private GameObject m_headShootedPic;
    public float m_shootRate = 2f;

    private Transform targetTransform;
    private PlayerController targetPC;

    private GameObject m_gun;
    private ParticleSystem m_gunFlash;
    private GameObject m_beShootedPic;
    private float nextTimeToFire = 0f;

    public GameObject m_hitImpact;

    public string[] nameList;

    public int B_flag = 1;

    private float m_gunCapacity = 30f;
    private float m_gunNumber = 30f;
    public bool is_OnLoading = false;
    private float m_timerOnLoading = 3f;
    private bool hasBeenPressR = false;

    private float m_timerRevive = 5f;

    public Text m_nameText;
    public GameObject m_playerHealthBarHandle;

    private string whoKillMe;
    private string iKillWho;
    private bool is_iKillSomeone = false;
    private bool is_someoneKillMe = false;
    private float m_timerKillSomeone = 3f;

    private GameObject m_gameManager;


    public GameObject chatContentPrefab;
    private Transform gridLayout;
    public GameObject chatAreaPrefab;
    public GameObject chatInputField;
    private bool is_pressT = false;
    private bool is_hasChatArea = false;
    private string msg;

    public ChatClient client;
    private string AppID;
    private string AppVersion;

    void Awake()
    {
        if(!m_animator) { gameObject.GetComponent<Animator>(); }
        if(!m_rigidBody) { gameObject.GetComponent<Animator>(); }

        if(photonView.IsMine)
        {
            m_nameText.text = PhotonNetwork.NickName;
            this.transform.name = PhotonNetwork.NickName;
        }
        else
        {
            m_nameText.text = photonView.Owner.NickName;
            this.transform.name = photonView.Owner.NickName;
        }
    }

    

    void Start()
    {
        

        //nameList = new string[] { "Jack", "Rose", "Bigby", "Haoooo", "Sober" };
        //this.gameObject.name = findNextName();

        CameraController _cameraController = this.gameObject.GetComponent<CameraController>();
        m_playerHealth = 100f;
        m_gun = this.transform.Find("Gun").gameObject;
        m_gunFlash = m_gun.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
        m_beShootedPic = GameObject.Find("BeShooted").gameObject;
        m_headShootedPic = GameObject.Find("HeadQuasiCenter").gameObject;
        m_gameManager = GameObject.Find("GameManager").gameObject;

        client = new ChatClient(this);
        AppID = "3752229c-9c8d-4214-a556-ffbba1d8520f";
        AppVersion = "1";
        Debug.Log(AppID);
        Debug.Log("On connecting... ");
        client.Connect(AppID, AppVersion, new Photon.Chat.AuthenticationValues(this.name));

        if (_cameraController != null)
        {
            if (photonView.IsMine)
            {
                _cameraController.SetFolling();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

    }

    string findNextName()
    {
        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if(players.Length==1)
        {
            return nameList[0];
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                int j;
                for (j = 0; j < players.Length; j++)
                    if (nameList[i].Equals(players[j].name))
                        break;
                if(j == players.Length)
                    return nameList[i];
            }
            return null;
        }
      
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for(int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!m_collisions.Contains(collision.collider)) {
                    m_collisions.Add(collision.collider);
                }
                m_isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if(validSurfaceNormal)
        {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider))
            {
                m_collisions.Add(collision.collider);
            }
        } else
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(m_collisions.Contains(collision.collider))
        {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count == 0) { m_isGrounded = false; }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 我们拥有这个角色:把我们的数据发送给其他人
            if(m_amIShoot)
            {
                GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");
                foreach(GameObject per_target in targets)
                {
                    
                    PlayerController pc = per_target.transform.GetComponent<PlayerController>();

                    string sendStr = PhotonNetwork.NickName + "-" + per_target.transform.GetComponent<PhotonView>().ViewID + ":" + pc.m_playerHealth.ToString();
                    stream.SendNext(sendStr);
                    //Debug.Log("send: "+sendStr);

                }
                
            }
            m_amIShoot = false;
        }
        if(stream.IsReading)
        {
            
            // 网络角色，接收数据
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");
            

            for (int i= targets.Length-1; i>=0;i--)
            {
                string recvStr = (string)stream.ReceiveNext();
                //Debug.Log(recvStr);
                string[] recvArray = recvStr.Split(new char[2] { '-', ':' });
                string sendNickname = recvArray[0];
                int recvViewID = int.Parse(recvArray[1]);
                int recvHealth = int.Parse(recvArray[2]);

                for(int j=0;j<targets.Length;j++)
                {
                    PlayerController update_pc = targets[j].transform.GetComponent<PlayerController>();
                    if (recvViewID == targets[j].transform.GetComponent<PhotonView>().ViewID)
                    {
                        if(update_pc.m_playerHealth != recvHealth && this.transform.GetComponent<PhotonView>().ViewID == recvViewID)
                        {
                            m_isShooted = true;
                            m_timerIsShooted = 0.2f;
                        }
                        update_pc.m_playerHealth = recvHealth;

                        if(recvHealth <= 0)
                        {
                            whoKillMe = sendNickname;
                            is_someoneKillMe = true;
                            m_timerKillSomeone = 3f;
                        }
                        //Debug.Log("AfterUpdate: " + targets[j].transform.GetComponent<PhotonView>().ViewID.ToString() + ":" + recvHealth);
                    }
                }
            }
            

        }
    }

    void Update()
    {
        if (client != null)
        {
            client.Service();
        }

        // 被击中
        if (m_isShooted)
        {
            m_timerIsShooted -= Time.fixedDeltaTime;
            //受击红色
            m_beShootedPic.GetComponent<Image>().color = new Color(255f/255f, 158f / 255f, 158f / 255f, 94f / 255f);
            m_moveSpeed = 0.1f;
            if (m_timerIsShooted <= 0)
            {
                m_isShooted = false;
                m_beShootedPic.GetComponent<Image>().color = new Color(255f / 255f, 158f / 255f, 158f / 255f, 0f / 255f);
                m_moveSpeed = 2.0f;
            }
        }


        //爆头准星
        if (m_amIShootHead)
        {
            m_timerAmIShootHead -= Time.fixedDeltaTime;
            //受击红色
            m_headShootedPic.GetComponent<Image>().color = new Color(222f / 255f, 222f / 255f, 222f / 255f, 255f / 255f);
            if (m_timerAmIShootHead <= 0)
            {
                m_amIShootHead = false;
                m_headShootedPic.GetComponent<Image>().color = new Color(222f / 255f, 222f / 255f, 222f / 255f, 0f / 255f);
            }
        }


        //换弹
        if (is_OnLoading)
        {
            m_timerOnLoading -= Time.fixedDeltaTime;
            Text txtOnLoading = GameObject.Find("OnLoadingTip").GetComponent<Text>();
            txtOnLoading.text = "On Loading! ";

            //Debug.Log(m_timerOnLoading);
            if (m_timerOnLoading <= 0)
            {
                txtOnLoading.text = "";
                is_OnLoading = false;
                hasBeenPressR = false;
                this.m_gunNumber = 30f;
            }
        }

        //i消灭someone
        if (is_iKillSomeone)
        {
            m_timerKillSomeone -= Time.fixedDeltaTime;
            Text txtKill = GameObject.Find("KillTip").GetComponent<Text>();
            txtKill.text = "You Killed " + iKillWho;

            //Debug.Log(m_timerOnLoading);
            if (m_timerKillSomeone <= 0)
            {
                txtKill.text = "";
                iKillWho = "";
                is_iKillSomeone = false;
            }
        }

        //someone消灭me
        if (is_someoneKillMe)
        {
            m_timerKillSomeone -= Time.fixedDeltaTime;
            Text txtKill = GameObject.Find("KillTip").GetComponent<Text>();
            txtKill.text = "You were eliminated by " + whoKillMe;

            //Debug.Log(m_timerOnLoading);
            if (m_timerKillSomeone <= 0)
            {
                txtKill.text = "";
                whoKillMe = "";
                is_someoneKillMe = false;
            }
        }

        //死亡复活
        if (is_playerDead)
        {
            m_timerRevive -= Time.fixedDeltaTime;
            Text txxt = GameObject.Find("DeadTip").GetComponent<Text>();
            txxt.text = "You are Dead! Reviving ... ";

            Quaternion tmpQ = this.transform.rotation;
            this.transform.rotation = Quaternion.Euler(-90, tmpQ.eulerAngles.y, tmpQ.eulerAngles.z);
            //Debug.Log(whoKillMe);

            if (m_timerRevive <= 0)
            {
                txxt.text = "";
                this.m_playerHealth = 100f;
                this.transform.position = new Vector3(0, 3, -10);
                this.transform.rotation = Quaternion.identity;
                is_playerDead = false;
            }
        }

    }
    


	void FixedUpdate ()
    {
        

        //Debug.Log(PhotonNetwork.NickName);
        //避免误操作其他玩家
        if ( (!photonView.IsMine || is_playerDead || m_gameManager.GetComponent<GameManager>().is_clickEscMenu )
            && PhotonNetwork.IsConnected)
        {
            return;
        }

        // t 聊天
        if (!is_pressT && Input.GetKeyDown(KeyCode.T))
        {
            is_pressT = true;
            //client.PublishMessage("all", "Hello Everyone!");
            GameObject chatInput = Instantiate(chatInputField, chatInputField.transform.position, Quaternion.identity);
            chatInput.SetActive(true);
            chatInput.transform.SetParent(GameObject.Find("UI").transform);
            chatInput.transform.GetComponent<InputField>().ActivateInputField();

        }
        if (is_pressT && Input.GetKeyDown(KeyCode.Return))
        {
            GameObject chatInputText = GameObject.Find("ChatInputFieldText");
            GameObject chatInput = GameObject.Find("ChatInputField(Clone)");
            msg = chatInputText.transform.GetComponent<Text>().text;
            
            client.PublishMessage("all", msg);
            chatInput.SetActive(false);
            Debug.Log(chatInput.activeSelf);
            is_pressT = false;

        }


        Text txt = GameObject.Find("HP").GetComponent<Text>();
        
        txt.text = "HP :" +m_playerHealth.ToString();

        m_playerHealthBarHandle.GetComponent<RectTransform>().offsetMin = new Vector2( (100-m_playerHealth)/100*160 , 0);

        Text txtCapacity = GameObject.Find("Capacity").GetComponent<Text>();

        txtCapacity.text = "Capacity : " + Mathf.Floor(m_gunNumber).ToString() + 
            " / " + Mathf.Floor(m_gunCapacity).ToString();

        //Debug.Log(photonView.IsMine);
        m_animator.SetBool("Grounded", m_isGrounded);


        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        Transform camera = Camera.main.transform;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            v *= m_runScale;
            h *= m_runScale;
        }

        if(Input.GetButton("Fire1"))
        {
            v /= m_runScale;
            h /= m_runScale;
        }
        //Debug.Log(v);

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction = camera.forward * m_currentV + camera.right * m_currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;
            //Debug.Log(direction.magnitude / 2.0f);
            m_animator.SetFloat("MoveSpeed", direction.magnitude / 2.0f);
        }

        JumpingAndLanding();

        if (Input.GetKey(KeyCode.F))
        {
            m_animator.SetTrigger("Wave");
        }

        if (Input.GetKey(KeyCode.E))
        {
            m_animator.SetTrigger("Pickup");
        }

        if(Input.GetButton("Fire1") && !is_OnLoading && Time.time>=nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / m_shootRate;
            m_gunNumber--;
            Shoot();
            m_gunFlash.Play();
        }
        
        

        if (m_gunNumber <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            is_OnLoading = true;
            if(!hasBeenPressR)
            {
                m_timerOnLoading = 3f;
                hasBeenPressR = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            shrinkAndStand(0);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            shrinkAndStand(1);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Text txxxt = GameObject.Find("FastShoot").GetComponent<Text>();
            if (this.B_flag == 0)
            {
                this.B_flag = 1;
                txxxt.text = "Now Fast Shooting Mode";
                this.m_shootRate = 10f;
            }
            else
            {
                this.B_flag = 0;
                txxxt.text = "Now Tap Shooting Mode";
                this.m_shootRate = 2f;
            }
        }

        if (m_playerHealth<=0 || this.transform.position.y<-10f)
        {
            is_playerDead = true;
            m_timerRevive = 5.0f;
        }

        m_wasGrounded = m_isGrounded;
         
    }

    private void Shoot()
    {
        //GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");
        //foreach (GameObject per_target in targets)
        //{
        //    Debug.Log(per_target.GetComponent<PlayerController>().m_playerHealth);
        //}

        RaycastHit hit;
        Transform camera = Camera.main.transform;
        if(Physics.Raycast(camera.position, camera.forward, out hit, m_shootRange))
        {
            targetTransform = hit.transform;
            targetPC = targetTransform.GetComponent<PlayerController>();

            if (hit.transform.tag == "Mushroom")
            {
                ObjectHealthy mushroom = hit.transform.GetComponent<ObjectHealthy>();
                Debug.Log(mushroom.transform.position);
                m_playerHealth = mushroom.takeDamage(m_shootDamage, m_playerHealth);
            }

            if (hit.collider.name=="Head")
            {
                targetPC.takeDamage(m_shootDamage,true);
                m_amIShoot = true;
                m_amIShootHead = true;
                m_timerAmIShootHead = 0.2f;
            }
            else if (targetPC != null)
            {
                targetPC.takeDamage(m_shootDamage,false);
                m_amIShoot = true;
            }
            
            if(targetPC!=null && targetPC.m_playerHealth <= 0)
            {
                iKillWho = targetPC.transform.name;
                is_iKillSomeone = true;
                m_timerKillSomeone = 3f;
            }

            Instantiate(m_hitImpact, hit.point, Quaternion.LookRotation(hit.normal));
            GameObject[] to_destory = GameObject.FindGameObjectsWithTag("Smoke");
            foreach(GameObject to_des in to_destory)
            {
                Destroy(to_des, 5);
            }
            
        }
    }


    public void takeDamage(float amount,bool isHead)
    {
        if(m_playerHealth > 0f)
        {
            if(isHead)
            {
                m_playerHealth -= 2*amount;
            }
            else
            {
                m_playerHealth -= amount;
            }

            if(m_playerHealth <0)
            {
                m_playerHealth = 0;
            }
            
        }
        
        //if(m_playerHealth <= 0f)
        //{
        //    is_playerDead = true;
        //}
    }

    private void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && Input.GetKey(KeyCode.Space))
        {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }

        if (!m_wasGrounded && m_isGrounded)
        {
            m_animator.SetTrigger("Land");
        }

        if (!m_isGrounded && m_wasGrounded)
        {
            m_animator.SetTrigger("Jump");
        }
    }

    void shrinkAndStand(int flag)
    {
        float scale = 1f;
        if (flag == 0)
        {
            scale = 0.7f;
        }
        else
        {
            scale = 1f;
        }
        System.Collections.Hashtable args = new System.Collections.Hashtable();
        args.Add("scale", new Vector3(1, scale, 1));
        args.Add("x", 1);
        args.Add("y", scale);
        args.Add("z", 1);
        args.Add("time", 0.8f);
        
        
        iTween.ScaleTo(this.gameObject, args);
    }

    void IChatClientListener.DebugReturn(DebugLevel level, string message)
    {
    }

    void IChatClientListener.OnDisconnected()
    {
    }

    void IChatClientListener.OnConnected()
    {
        Debug.Log("Connected...");

        //下面这个方法用于订阅频道，只有订阅频道之后，才能接受到这个频道的信息，比如现在有人在工会聊天，你就可以接受到。
        client.Subscribe(new string[] { "all" });//这里我们订阅了世界聊天频道和工会聊天频道


        //这个方法用于向频道发送消息，这里我们向世界聊天频道发送了大家好，那么订阅了这个频道的人都能接受到这条消息。
        client.PublishMessage("all", "Hello Everyone!");

        //这个方法用于发送私人消息，这里我们向一个叫小小的用户发送了一条消息“你好”。如果这个用户在线的话，他将收到这条消息。
        //client.SendPrivateMessage("小小", "你好");
    }

    void IChatClientListener.OnChatStateChange(ChatState state)
    {
        Debug.Log("现在我的状态是：" + state);
    }

    void IChatClientListener.OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if(!is_hasChatArea)
        {
            GameObject chatArea = Instantiate(chatAreaPrefab, chatAreaPrefab.transform.position, Quaternion.identity);
            is_hasChatArea = true;
            chatArea.SetActive(true);
            chatArea.transform.SetParent(GameObject.Find("UI").transform);
        }
        
        
        gridLayout = GameObject.Find("ChatContent").transform;
        //chatAreaPrefab.SetActive(true);
        for (int i = 0; i < senders.Length; i++)
        {
            GameObject newChatContent = Instantiate(chatContentPrefab, gridLayout.position, Quaternion.identity);

            newChatContent.GetComponentInChildren<Text>().text = "CHANNEL : " + channelName + "  |  " + senders[0] + " : " + messages[0];

            newChatContent.transform.SetParent(gridLayout);
        }
    }

    void IChatClientListener.OnPrivateMessage(string sender, object message, string channelName)
    {
    }

    void IChatClientListener.OnSubscribed(string[] channels, bool[] results)
    {
    }

    void IChatClientListener.OnUnsubscribed(string[] channels)
    {
    }

    void IChatClientListener.OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    void IChatClientListener.OnUserSubscribed(string channel, string user)
    {
    }

    void IChatClientListener.OnUserUnsubscribed(string channel, string user)
    {
    }
}
