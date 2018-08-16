using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
	public Text Content;
	public Text RoomName;
	public Text RoomNum;

    // 활성화, 비활성화를 위한 변수
    public GameObject m_canvas;
    public GameObject m_player;  // cowboy object
    public GameObject m_contentUI;
    public GameObject m_sendMsgButton;

	[SyncVar]
	public int m_currentRoom;

    int speed = 10;

    void Start(){
        // 리모트 플레이어라면 캔버스 비활성화.
		if (!isLocalPlayer)
			m_canvas.SetActive (false);
		else
			Cmdgetid ();
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
			m_contentUI.SetActive (false);
			m_sendMsgButton.SetActive (false);
		} else {
            m_contentUI.SetActive(true);
            m_sendMsgButton.SetActive(true);
        }

		CmdSetMyRoom (MyNetManager.instance.m_currentRoom);

		m_player.transform.Translate (Vector3.right * speed * Time.smoothDeltaTime * Input.GetAxis ("Horizontal"), Space.World);
		m_player.transform.Translate (Vector3.forward * speed * Time.smoothDeltaTime * Input.GetAxis ("Vertical"), Space.World);
	}

	[Command]
	public void Cmdgetid(){
		MyNetManager.instance.SendID (this.connectionToClient.connectionId);
	}

	[Command]
	public void CmdsendMes(){
		MyNetManager.instance.SendToServer (Content.text);
	}

	//[Command]
	public void CmdGotoRoom(){
		MyNetManager.instance.GotoRoom (int.Parse(RoomNum.text));
	}

	[Command]
	public void CmdSetMyRoom (int roomnum){
		m_currentRoom = roomnum;
		RpcSetMyRoom (roomnum);
	}

	[ClientRpc]
	public void RpcSetMyRoom (int roomnum){
		m_currentRoom = roomnum;
	}

	//[Command]
	public void CmdExitRoom(){
		MyNetManager.instance.ExitRoom (int.Parse(RoomNum.text));
	}

	[Command]
	public void CmdCreateRoom(){
		MyNetManager.instance.CreateRoom (RoomName.text);
	}
}
