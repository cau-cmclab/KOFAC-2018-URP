using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
    public InputField m_chatField;  // 채팅 입력창. m_chatField.text는 채팅내용

    public InputField m_newRoomName; // 생성하려는 방 이름
    public GameObject m_roomListContent; // 플레이어의 채팅방 리스트

    public GameObject m_canvas;
    public GameObject m_player;  // cowboy object
    public GameObject m_sendMsgButton;

	[SyncVar] // [SyncVar] : 서버에서 값을 변경하면 다른 클라이언트들에게 동기화 시켜준다.
	public int m_currentRoom;  // 로컬, 리모트 플레이어가 접속한 방

    int speed = 10;

    void Start(){
        // 리모트 플레이어라면 캔버스 비활성화.
        if (!isLocalPlayer)
        {
            m_canvas.SetActive(false);
        }
        else
        {
            CmdGetId();
            MyNetManager.instance.m_roomListContent = this.m_roomListContent;
        }
	}

	void Update () {
        // 로컬플레이어와 리모트플레이어의 방이 다르다면 리모트플레이어 비활성화
		if (!isLocalPlayer) {
			if (m_currentRoom != MyNetManager.instance.m_currentRoom)
				m_player.SetActive (false);
			else
				m_player.SetActive (true);
			return;
		}

        // 방에 입장하지 않은 상태라면 메시지 보내는 버튼 비활성화
		if (m_currentRoom == -1) {
			m_chatField.gameObject.SetActive (false);
			m_sendMsgButton.SetActive (false);
		} else {
            m_chatField.gameObject.SetActive(true);
            m_sendMsgButton.SetActive(true);
        }

        // player의 m_currentRoom과 MyNetManager의 m_currentRoom은 매순간 동기화된다.
		CmdSetMyRoom (MyNetManager.instance.m_currentRoom);

		m_player.transform.Translate (Vector3.right * speed * Time.smoothDeltaTime * Input.GetAxis ("Horizontal"), Space.World);
		m_player.transform.Translate (Vector3.forward * speed * Time.smoothDeltaTime * Input.GetAxis ("Vertical"), Space.World);
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
    
	
	public void ExitRoom(){
		MyNetManager.instance.ExitRoom (m_currentRoom);
	}

    // 방 목록 새로고침
    public void RefreshRoom()
    {
        MyNetManager.instance.RefreshRoom();
    }

}
