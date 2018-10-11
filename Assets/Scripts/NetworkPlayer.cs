using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


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
    public GameObject m_SpeechButton;
    public GameObject m_panelForButtons;

    [SyncVar]
	public int m_currentRoom;  // 로컬, 리모트 플레이어가 접속한 방

    void Start(){
        // 플레이어가 생성되면 ImageTarget 하위 오브젝트로 설정
        this.transform.SetParent(GameObject.FindGameObjectWithTag("ImageTarget").transform);

        // 리모트 플레이어라면 MainCanvas 비활성화
        if (!isLocalPlayer)
        {
            m_mainCanvas.SetActive(false);
        }
        else
        {
            // MyNetManager에 로컬플레이어 오브젝트 저장.
            MyNetManager.instance.m_LocalPlayer = this;

            CmdGetId();
            MyNetManager.instance.m_roomListContent = this.m_roomListContent;
            ChangeMyObjName();
            CmdSetMyRoom(MyNetManager.instance.m_currentRoom);
        }
	}

	void Update () {

        if (!isLocalPlayer) {
            // 마커가 인식되는중에만 플레이어들의 렌더링 여부를 판단한다.
            if(MyNetManager.instance.isMarkerFound == true)
            {
                SetPlayerRender();
            }
            return;
		}
        
        // 방에 입장하지 않은 상태라면 메시지 보내는 버튼 비활성화
		if (m_currentRoom == -1) {
			m_chatField.gameObject.SetActive (false);
			m_sendMsgButton.SetActive (false);
            m_GalleryButton.SetActive(false);
            m_SpeechButton.SetActive(false);
            m_panelForButtons.SetActive(false);
        }
        else {
            m_chatField.gameObject.SetActive(true);
            m_sendMsgButton.SetActive(true);
            m_GalleryButton.SetActive(true);
            m_SpeechButton.SetActive(true);
            m_panelForButtons.SetActive(true);
        }
	}

    // 로컬플레이어와 리모트플레이어의 방이 다르다면 리모트플레이어 비활성화
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

	[Command]
	public void CmdGetId(){
		MyNetManager.instance.SendID (this.connectionToClient.connectionId);
	}

    // 서버로 채팅 메시지 전송
	public void SendMes(){
        if (m_chatField.text.Equals(""))
            return;

        MyNetManager.instance.SendToServer(m_chatField.text);
        // 자신의 말풍선에 출력
        m_player.GetComponentInChildren<SpeechBubbleControl>().InputTextChatwindow(m_chatField);
        m_chatField.text = "";
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
    
    public void GotoRoom(int RoomNum)
    {
        MyNetManager.instance.GotoRoom(RoomNum);

        CmdSetMyRoom(MyNetManager.instance.m_currentRoom);
    }
	
	public void ExitRoom(){
		MyNetManager.instance.ExitRoom (m_currentRoom);

        CmdSetMyRoom(MyNetManager.instance.m_currentRoom);
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
  
    /* 한가지 사실을 알았는데 Cmd함수를 버튼의 콜백함수로 사용하면 Cmd기능이 제대로 동작하지 않는다. ([Command]에 의해서 서버에서 실행되야하는데 그렇게 되지 않음.)
       따라서 버튼으로 호출하려면 버튼 콜백함수를 따로 만들어 그 안에서 호출해주어야 함. */
    // 캐릭터 변경 버튼
    public void OnChangePlayerButton()
    {
        if (!isLocalPlayer)
            return;
        CmdChangePlayer();
    }

    [Command]
    public void CmdChangePlayer()
    {
        MyNetManager.instance.RespawnPlayer(this);
    }
  
    public void ChangeMyObjName()
    {
        this.gameObject.name = this.gameObject.name + "_LOCALPLAYER";
    }
}
