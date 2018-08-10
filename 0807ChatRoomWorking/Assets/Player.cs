using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
	public Text naeyong;
	public Text RoomNumber;
	public Text RoomName;
	int speed = 10;

	[SyncVar]
	public int CurrentRoom;

	void Start(){
		if (!isLocalPlayer)
			transform.GetChild (0).gameObject.SetActive (false);
		else
			Cmdgetid ();
	}

	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) {
			if (this.CurrentRoom != MyNetManager.instance.currentroom)
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
		MyNetManager.instance.SendID (this.connectionToClient.connectionId);
	}

	[Command]
	public void CmdsendMes(){
		MyNetManager.instance.SendToServer (naeyong.text);
	}

	[Command]
	public void CmdGotoRoom(){
		MyNetManager.instance.GotoRoom (int.Parse(RoomNumber.text));
		CmdSetMyRoom(int.Parse(RoomNumber.text));
	}

	[Command]
	public void CmdSetMyRoom (int roomnum){
		CurrentRoom = roomnum;
		RpcSetMyRoom (roomnum);
	}

	[ClientRpc]
	public void RpcSetMyRoom (int roomnum){
		CurrentRoom = roomnum;
	}

	[Command]
	public void CmdExitRoom(){
		MyNetManager.instance.ExitRoom (int.Parse(RoomNumber.text));
		CmdSetMyRoom (0);
	}

	[Command]
	public void CmdCreateRoom(){
		MyNetManager.instance.ExitRoom (int.Parse(RoomNumber.text));
		CmdSetMyRoom (0);
	}
}
