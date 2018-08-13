using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
	public Text naeyong;
	public Text roomNumber;
	int speed = 10;
    public GameObject m_cowboy;
    public GameObject m_canvas;

	[SyncVar]
	public int m_currentRoom;

    // 순서: OnStartClient() 실행 후 Start()가 수행됨.
	void Start(){
        if (!isLocalPlayer)
            m_canvas.SetActive(false);
        else
            CmdGetid();
	}

	// Update is called once per frame
	void Update () {
        // 리모트 플레이어의 방이 로컬 플레이어와 다르면 캐릭터 비활성화.
		if (!isLocalPlayer) {
            if (this.m_currentRoom != MyNetManager.instance.m_currentRoom)
                m_cowboy.SetActive(false);
            else
                m_cowboy.SetActive(true);
                return;
		}
        m_cowboy.transform.Translate (Vector3.right * speed * Time.smoothDeltaTime * Input.GetAxis ("Horizontal"), Space.World);
        m_cowboy.transform.Translate (Vector3.forward * speed * Time.smoothDeltaTime * Input.GetAxis ("Vertical"), Space.World);

        if (Input.GetKeyDown("space"))
            InputKey();

    }

    void InputKey()
    {
        if (!isLocalPlayer)
            Debug.Log("리모트 플레이어입니다.");
        else
            Debug.Log("로컬 플레이어입니다.");
    }

    [Command]
	public void CmdGetid(){
		MyNetManager.instance.SendID (this.connectionToClient.connectionId);
	}


    /* m_client.Send()만으로 클라이언트에서 서버를 호출하게 됨.
       UNet은 데디케이트 서버 방식을 지원하는데 서버에서 제어한다는 의미는 클라이언트의 모든 처리를 서버가 맡아서 한다는 의미는 아니다.
       서버에서 처리해주어야하는 부분은 서버가, 클라이언트가 할 수 있는 부분은 클라이언트가 맡아서 하게 되는데
       클라이언트가 자신의 값을 임의로 변경하지 못하도록 값을 변경하는 부분은 보안을 위해 서버에서 담당한다.
       SendMes같은 경우 SendToServer에서 m_client.Send()를 통해 메시지를 포장과 전송을 클라이언트에서 수행해도 된다.
       서버는 Send를 통해 메시지를 받으며 기존과 동일한 작업을 수행한다.
       중요한것은 [Command]를 사용하게되면 클라이언트의 플레이어 오브젝트에서 수행할 행동을 서버의 해당 플레이어 오브젝트에서 실행하게 되어
       따로 로컬 플레이어와 리모트 플레이어를 구분하지 않아도 되는데, 단순 SendMes메서드로 사용하는 경우엔 클라이언트 내에서 Player오브젝트들을 구분할 수 없게되어
       모든 플레이어들에게서 SendMes()가 발생할 것으로 생각된다. 근데 현재 리모트 플레이어들에게서는 발생하지 않는다. 왜지???
       리모트 플레이어들의 캔버스를 비활성화 했기 때문은 아님. 아마도 버튼에 SendMes()함수를 연결할때 오브젝트가 연결되기 때문인 것 같음!
       많은 플레이어들이 생성되도 버튼에 연결된 함수의 주인은 Player오브젝트 하나로 되는듯 하다. */

    // SendMsg 버튼 클릭시 호출
    // [Command] 
    public void SendMes(){
        /*if (!isLocalPlayer) {
            Debug.Log("리모트플레이어에게서도 이 메서드가 발생하는가?"); -> 발생하지 않는다.
        }*/
		MyNetManager.instance.SendToServer (naeyong.text);
	}

    // Room 버튼 클릭시 호출
	[Command]
	public void CmdGotoRoom(){
        int roomNum = int.Parse(roomNumber.text);
        MyNetManager.instance.GotoRoom (roomNum);

		CmdSetMyRoom(roomNum);
	}

    // 서버에서 값 변경, [SyncVar]로 클라이언트에 반영. 
	[Command]
	public void CmdSetMyRoom (int roomnum){
		m_currentRoom = roomnum;
	}

	[Command]
	public void CmdExitRoom(){
        int roomNum = int.Parse(roomNumber.text);
        MyNetManager.instance.ExitRoom (roomNum);

		CmdSetMyRoom (0);
	}
}
