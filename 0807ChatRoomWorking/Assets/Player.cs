using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
	public Text naeyong;
	public Text roomNumber;
	int speed = 10;
    
    // 서버에서 CurrentRoom값이 변경되면 클라이언트에 동기화.
	[SyncVar]
	int currentRoom;

	void Start(){
        // 리모트 플레이어들의 캔버스 비활성화. transform.GetChild(0)은 canvas를 말함. 클라이언트가 연결되었을 때 버튼이 나오도록 하기위해 플레이어 오브젝트 아래에 넣음.
		if (!isLocalPlayer)
			transform.GetChild (0).gameObject.SetActive (false);
        // Player가 생성되었을때 로컬 플레이어의 정보를 서버에게 전달.
		else
			Cmdgetid ();
	}

	void Update () {
        // 리모트 플레이어들 중 MyNetManager의 currentroom(자신이 접속한 채팅방)과 다르다면 캐릭터 비활성화. 같은 방이면 활성화.
		if (!isLocalPlayer) {
			if (this.currentRoom != MyNetManager.instance.m_currentRoom)
				transform.GetChild (1).gameObject.SetActive (false);
			else
				transform.GetChild (1).gameObject.SetActive (true);
			return;
		}
        // 캐릭터 이동
		transform.GetChild(1).Translate (Vector3.right * speed * Time.smoothDeltaTime * Input.GetAxis ("Horizontal"), Space.World);
		transform.GetChild(1).Translate (Vector3.forward * speed * Time.smoothDeltaTime * Input.GetAxis ("Vertical"), Space.World);
	}

    // connectionToClient.connectionId는 서버에서만 사용가능하기 때문에 자신의 아이디를 알려면 메시지로 돌려받아야 하는 번거로운 과정을 가짐. Command명령으로 서버에서 실행하나 this.connectionToClient.connectionId는 클라이언트를 의미. (서버에서 호출되기 때문에 connectionToClient구나)
    // Command는 클라이언트에서 서버에게 Cmd함수를 호출해달라 명령. -> 서버는 CmdgetId 실행. 따라서 connectionToClient도 서버에서 접근함.
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
		MyNetManager.instance.GotoRoom (int.Parse(roomNumber.text));
		CmdSetMyRoom(int.Parse(roomNumber.text));
	}

    // 클라이언트의 CurrentRoom을 바꾸기 위해 서버의 해당 클라이언트 값을 변경. 서버에서만 변경하였으나 [SyncVar]를 통해 클라이언트들에서도 변경됨.
	[Command]
	public void CmdSetMyRoom (int roomnum){
        Debug.Log("CmdSetMyRoom On");
		currentRoom = roomnum;
        //RpcSetMyRoom (roomnum);
	}
    /*
	[ClientRpc]
	public void RpcSetMyRoom (int roomnum){
        CurrentRoom = roomnum;
	}
    */
	[Command]
	public void CmdExitRoom(){
		MyNetManager.instance.ExitRoom ();
	}
}
