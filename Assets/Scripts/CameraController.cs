using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
    public class CameraController : MonoBehaviour
    {

        [Tooltip("The distance in the local x-z plane to the target")]
        [SerializeField]
        private float distance = 7.0f;

        [Tooltip("The height we want the camera to be above the target")]
        [SerializeField]
        private float height = 3.0f;

        [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
        [SerializeField]
        private Vector3 centerOffset = Vector3.zero;

        [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
        [SerializeField]
        private bool followOnStart = false;

        [Tooltip("The Smoothing for the camera to follow the target")]
        [SerializeField]
        private float smoothSpeed = 0.125f;

        // cached transform of the target
        public Transform cameraTransform;

        // maintain a flag internally to reconnect if target is lost or camera is switched
        bool isFollowing;

        // Cache for camera offset
        Vector3 cameraOffset = Vector3.zero;

        [SerializeField]
        private float mouseX, mouseY;//获取鼠标移动的值
        [SerializeField]
        private float mouseSensitivity = 200;//获取鼠标移动速度

        [SerializeField]
        private float xRotation, yRotation;
        [SerializeField]
        private int Tcount = 0;

        private Quaternion m_deltaQuaternion;
        private bool is_Fire = false;
        
        private Quaternion m_notShootGunPos;
        private Quaternion m_notShootCameraPos;

        private Transform gunTransform;

        private float t_shootUp = 0;
        
        private bool is_pressCtrl = false;

        private PlayerController pc;
        private void Awake()
        {

        }
        
        void Start()
        {

            m_deltaQuaternion = Quaternion.Euler(0, 0, 0);
            gunTransform = this.transform.GetChild(0);
            // Start following the target if wanted.
            if (followOnStart)
            {

                OnStartFollowing();
            }
            Tcount = 0;
            pc = this.transform.GetComponent<PlayerController>();
        }

        private void Update()
        {
            
        }

        void LateUpdate()
        {


            // The transform target may not destroy on level load, 
            // so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
            if (isFollowing)
            {
                OnStartFollowing();
            }

            if (cameraTransform != null && !pc.is_playerDead)
            {
                LookAsFirstPersonView();
            }


        }

        public void SetFolling()
        {
            isFollowing = true;
        }

        public void OnStartFollowing()
        {
            cameraTransform = Camera.main.transform;
            // we don't smooth anything, we go straight to the right camera shot
        }
        
        

        void LookAsFirstPersonView()
        {
            
            Vector3 highW, shortW;
            highW = this.transform.position + new Vector3(0, 0.8f, 0.1f);
            shortW = this.transform.position + new Vector3(0, 0.6f, 0.1f);
            if (!is_pressCtrl)
            {
                cameraTransform.position = this.transform.position + new Vector3(0, 0.8f, 0.1f);
            }
           else
            {
                cameraTransform.position = this.transform.position + new Vector3(0, 0.6f, 0.1f);
            }
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                is_pressCtrl = true;
                
                shrinkAndStand(highW,shortW);
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                shrinkAndStand(shortW,highW);
                is_pressCtrl = false;
            }

            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -50f, 50f); //限制相机俯仰高度（-50，50）
            yRotation += mouseX;

            this.transform.Rotate(Vector3.up * mouseX);
            

            if (Input.GetButton("Fire1") && !pc.is_OnLoading)
            {
                is_Fire = true;
                if (pc.B_flag == 0)
                {
                    xRotation += -10f * Time.deltaTime;
                }
                else
                {
                    xRotation += -20f * Time.deltaTime;
                }


                float xRanvalue = Random.value * 2 - 1;
                float yRanvalue = Random.value * 2 - 1;
                xRotation += xRanvalue * 0.5f;
                yRotation += yRanvalue * 0.5f;
            }
            
            gunTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);


        }


        void shrinkAndStand(Vector3 src, Vector3 dis)
        {
            System.Collections.Hashtable args = new System.Collections.Hashtable();
            args.Add("position", src);
            args.Add("path", new Vector3[] {src, dis});
            args.Add("time", 0.8f);
            
            iTween.MoveTo(cameraTransform.gameObject, args);
        }
    }
}