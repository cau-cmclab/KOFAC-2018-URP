using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
	public Text m_chat;  // 보내려는 채팅 내용
    public Text m_roomName;
	public Text m_roomNum;

    // 활성화, 비활성화를 위한 변수
    public GameObject m_canvas;
    public GameObject m_player;  // cowboy object
    public GameObject m_chatField;  // 채팅 입력창
    public GameObject m_sendMsgButton;

	[SyncVar] // [SyncVar] : 서버에서 값을 변경하면 다른 클라이언트들에게 동기화 시켜준다.
	public int m_currentRoom;  // 로컬, 리모트 플레이어가 접속한 방

    int speed = 10;

    void Start(){
        // 리모트 플레이어라면 캔버스 비활성화.
		if (!isLocalPlayer)
			m_canvas.SetActive (false);
		else
			CmdGetId ();
	}

	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) {
			if (this.m_currentRoom != MyNetManager.instance.m_currentRoom)
				m_player.SetActive (false);
			else
				m_player.SetActive (true);
			return;
		}

        // 방에 입장하지 않은 상태라면 메시지 보내는 버튼 비활성화
		if (m_currentRoom == -1) {
			m_chatField.SetActive (false);
			m_sendMsgButton.SetActive (false);
		} else {
            m_chatField.SetActive(true);
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
		MyNetManager.instance.SendToServer (m_chat.text);
	}

    [Command]
    public void CmdSetMyRoom(int roomnum)
    {
        m_currentRoom = roomnum;
    }

    //[Command]
    public void CmdCreateRoom()
    {
        MyNetManager.instance.CreateRoom(m_roomName.text);
    }

    // Enter버튼 클릭시
    //[Command]
    public void CmdGotoRoom(){
		MyNetManager.instance.GotoRoom (int.Parse(m_roomNum.text));
	}
    
	//[Command]
	public void CmdExitRoom(){
		MyNetManager.instance.ExitRoom (int.Parse(m_roomNum.text));
	}


}
