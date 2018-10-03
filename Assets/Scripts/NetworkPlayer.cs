using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


/* 해야하는 기능
 * 1. 채팅을 입력하면 자신의 말풍선에 출력되야함. 둘째로, 상대방의 화면의 자신의 플레이어에도 출력되어야 한다. 몇 초뒤에 동시에 사라지게 해야한다.
 
   2.  이모티콘은 몇초뒤 사라져야한다. 현재 되지않고 있음.

 */


public class NetworkPlayer : NetworkBehaviour {

    public InputField m_newRoomName; // 생성하려는 방 이름
    public GameObject m_roomListContent; // 플레이어의 채팅방 리스트
    public GameObject m_roomScrollList;
    private bool m_isRSLActive = true;

    public GameObject m_mainCanvas;
    public GameObject m_player;  // malcom

    /* 방에 입장해야 활성화 되는 오브젝트 */
    public InputField m_chatField;  // 채팅 입력창
    public GameObject m_sendMsgButton;
    public GameObject m_GalleryButton;
    public GameObject m_panelForButtons;


    
    public Text speechBubbleText;
    public GameObject speechBubbleUI;
    public GameObject speechBubbleImage;

    [SyncVar]
    public string m_text;

    [SyncVar]
    public int m_ID; // 개인클라이언트 ID

    [SyncVar] // [SyncVar] : 서버에서 값을 변경하면 다른 클라이언트들에게 동기화 시켜준다
	public int m_currentRoom;  // 로컬, 리모트 플레이어가 접속한 방

    private GameObject arCamera;
    private float timer;
    private float waitingTime;
    void Start(){
        // 플레이어가 생성되면 ImageTarget 하위 오브젝트로 설정
        this.transform.SetParent(GameObject.FindGameObjectWithTag("ImageTarget").transform);
        arCamera = GameObject.Find("ARCamera");

        timer = 0;
        waitingTime = 5f;

        // 리모트 플레이어라면 MainCanvas 비활성화
        if (!isLocalPlayer)
        {
            m_mainCanvas.SetActive(false);
        }
        else
        {
            CmdGetId();
            MyNetManager.instance.m_roomListContent = this.m_roomListContent;
        }
	}

	void Update () {
        speechBubbleText.text = m_text;

        if(m_text == "")
            speechBubbleUI.SetActive(false);
        else{
            speechBubbleUI.SetActive(true);
            timer += Time.deltaTime;   
        }
        
        speechBubbleUI.transform.LookAt(arCamera.transform);

        if(timer > waitingTime)
        {
            CmdShareBubble("");
            //speechBubbleText.text = "";
            timer = 0.0f;
        }

        // 로컬플레이어와 리모트플레이어의 방이 다르다면 리모트플레이어 비활성화
		if (!isLocalPlayer) {
            /*
			if (m_currentRoom != MyNetManager.instance.m_currentRoom)
				m_player.SetActive (false);
			else
				m_player.SetActive (true);
		    */
            SetPlayerRender();
            return;
		}

        // 방에 입장하지 않은 상태라면 메시지 보내는 버튼 비활성화
		if (m_currentRoom == -1) {
			m_chatField.gameObject.SetActive (false);
			m_sendMsgButton.SetActive (false);
            m_GalleryButton.SetActive(false);
            m_panelForButtons.SetActive(false);
        } else {
            m_chatField.gameObject.SetActive(true);
            m_sendMsgButton.SetActive(true);
            m_GalleryButton.SetActive(true);
            m_panelForButtons.SetActive(true);
        }

        // player의 m_currentRoom과 MyNetManager의 m_currentRoom은 매순간 동기화된다
		CmdSetMyRoom (MyNetManager.instance.m_currentRoom);
        // player의 자신의 클라이언트ID도 매순간 동기화한다.
        CmdGetId();
	}

    /* 자신의 방과 다른 방에 접속한 플레이어들을 보이지 않는다.
     * 현재 Update에서 돌아가기 때문에 성능 저하가 있을 것으로 예상됨. 
     * 불가피하게 작성된 함수이므로 추후 변경하는 것이 좋음. */
    private void SetPlayerRender()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        if (m_currentRoom != MyNetManager.instance.m_currentRoom)
        {
            // Enable rendering:
            foreach (var component in rendererComponents)
                component.enabled = false;

            // Enable colliders:
            foreach (var component in colliderComponents)
                component.enabled = false;

            // Enable canvas':
            foreach (var component in canvasComponents)
                component.enabled = false;
        }
        else
        {     
            // Enable rendering:
            foreach (var component in rendererComponents)
                component.enabled = true;

            // Enable colliders:
            foreach (var component in colliderComponents)
                component.enabled = true;

            // Enable canvas':
            foreach (var component in canvasComponents)
                component.enabled = true;
        }
    }

    public void InputTextChatwindow(InputField ip)
        {
            //speechBubbleText.text = ip.text;
            CmdShareBubble(ip.text);
            // 몇 초뒤에 말풍선 초기화
            //StartCoroutine(MyWaitForSeconds(5f));
            timer = 0.0f;
        }


	[Command]
	public void CmdGetId(){
		MyNetManager.instance.SendID (this.connectionToClient.connectionId);
        this.m_ID = MyNetManager.instance.GetMyID();
        RpcGetid();
	}

    [ClientRpc]
    public void RpcGetid(){
        this.m_ID = MyNetManager.instance.GetMyID();
    }

    // 서버로 채팅 메시지 전송
	public void SendMes(){
        if (m_chatField.text.Equals(""))
            return;

        MyNetManager.instance.SendToServer(m_chatField.text);
        // 자신의 말풍선에 출력
        InputTextChatwindow(m_chatField);
        CmdShareBubble(m_chatField.text);
        m_chatField.text = "";
	}

    [Command]
    public void CmdShareBubble(string str){
        m_text = str;
    }

    [Command]
    public void CmdSetMyRoom(int roomnum)
    {
        m_currentRoom = roomnum;
    }

    public void CreateRoom()
    {
        if (m_newRoomName.text.Equals(""))
            return;

        MyNetManager.instance.CreateRoom(m_newRoomName.text);
        m_newRoomName.text = "";
    }
    
	
	public void ExitRoom(){
		MyNetManager.instance.ExitRoom (m_currentRoom);
	}

    // 방 목록 새로고침
    public void RefreshRoom()
    {
        MyNetManager.instance.RefreshRoom();
    }

    // 채팅방 목록 최소화
    public void MinimizeRSL()
    {
        m_isRSLActive = !m_isRSLActive;
        m_roomScrollList.SetActive(m_isRSLActive);
    }

}
