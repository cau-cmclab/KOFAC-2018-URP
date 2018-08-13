using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
	public Text naeyong;
	public Text roomNumber;
	int speed = 10;

	[SyncVar]
	public int currentRoom;

	void Start(){
        if (!isLocalPlayer)
            transform.GetChild(0).gameObject.SetActive(false);
        else
			Cmdgetid ();
	}

	// Update is called once per frame
	void Update () {
        // 리모트 플레이어의 방이 로컬 플레이어와 다르면 비활성화.
		if (!isLocalPlayer) {
			if (this.currentRoom != MyNetManager.instance.m_currentRoom)
				transform.GetChild (1).gameObject.SetActive (false);
			else
				transform.GetChild (1).gameObject.SetActive (true);
			return;
		}
		transform.GetChild(1).Translate (Vector3.right * speed * Time.smoothDeltaTime * Input.GetAxis ("Horizontal"), Space.World);
		transform.GetChild(1).Translate (Vector3.forward * speed * Time.smoothDeltaTime * Input.GetAxis ("Vertical"), Space.World);
	}

	[Command]
	public void Cmdgetid(){
        Debug.Log(this.connectionToClient.connectionId);
		MyNetManager.instance.SendID (this.connectionToClient.connectionId);
	}

	[Command]
	public void CmdsendMes(){
		MyNetManager.instance.SendToServer (naeyong.text);
	}

	[Command]
	public void CmdGotoRoom(){
        int roomNum = int.Parse(roomNumber.text);
        MyNetManager.instance.GotoRoom (roomNum);
		CmdSetMyRoom(roomNum);
	}

	[Command]
	public void CmdSetMyRoom (int roomnum){
		currentRoom = roomnum;
	}

	[Command]
	public void CmdExitRoom(){
		MyNetManager.instance.ExitRoom (int.Parse(roomNumber.text));
		CmdSetMyRoom (0);
	}
}
