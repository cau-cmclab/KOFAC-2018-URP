using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;


public class MyNetManager : NetworkManager
{
    // 방입장, 퇴장 등 네트워크 특이사항을 출력하는 패널
    public Text m_netLogPanel;
    // StartServer, StartClient 버튼
    public GameObject m_startServer;
    public GameObject m_startClient;

    public NetworkClient m_client;
	public int m_clientId;
	public int m_currentRoom = -1;

	List<List<int>> ChatRoom = new List<List<int>>();


	private static MyNetManager mInstance;

	public static MyNetManager instance
	{
		get
		{
			if(mInstance == null)
			{
				mInstance = FindObjectOfType<MyNetManager> ();
			}
			return mInstance;
		}
	}

    #region 메시지 클래스
    public class MyMessage : MessageBase
	{
		public int roomNum;
		public int clientId;
		public string strMsg;
	}

	public class MyMessage2 : MessageBase
	{
		public int clientId;
	}

	public class MyMessage3 : MessageBase
	{
		public int clientId;
		public string strMsg;
	}

	public class MyMessage_GotoChatRoom : MessageBase
	{
		public int roomNum;
		public int clientId;
	}
    #endregion

    #region 메시지 타입
    public class MyMsgType
	{
        // MyMessage
        public const short CustomMsgType = MsgType.Highest + 1;
        // MyMessage2
        public const short CustomMsgType2 = MsgType.Highest + 2;
        // MyMessage3
        public const short CustomMsgType3 = MsgType.Highest + 3;
        // MyMsgType_GotoChatRoom
        public const short CustomMsgType4 = MsgType.Highest + 4;
    }
    #endregion

	public void OnMessage(NetworkMessage netMsg)
	{
		MyMessage msg = netMsg.ReadMessage<MyMessage> ();
		Debug.Log (msg.clientId + " 유저가 " + msg.strMsg + "를 보냈습니다.");

		MyMessage3 msg3 = new MyMessage3 ();
		msg3.clientId = msg.clientId;
		msg3.strMsg = msg.strMsg;

		for (int i = 0; i < ChatRoom[msg.roomNum - 1].Count; i++) {
			NetworkServer.SendToClient (ChatRoom [msg.roomNum - 1] [i], MyMsgType.CustomMsgType3, msg3);
		}
	}

    // 클라이언트가 자신의 아이디를 받는다.
	public void OnMessage2(NetworkMessage netMsg)
	{
        Debug.Log("OnMessage2 On");
		MyMessage2 msg = netMsg.ReadMessage<MyMessage2> ();
        /*
		transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = "ID : " + msg.clientId.ToString ();
		*/
        m_netLogPanel.text = "ID : " + msg.clientId.ToString();
        m_clientId = msg.clientId;
        Debug.Log ("GetID : " + msg.clientId);
    }

	public void OnMessage3(NetworkMessage netMsg)
	{
		MyMessage3 msg = netMsg.ReadMessage<MyMessage3> ();
        // 문자열 비교는 Equals 함수, ==는 주소 값을 비교해서 안됨.
		if (msg.strMsg.Equals(":ipjang:")) {
			if (msg.clientId != m_clientId) {
				Text entrytemp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
				entrytemp.text = entrytemp.text + "\n" + msg.clientId.ToString () + " 님이 입장했습니다.";
			}
			return;
		}
		else if (msg.strMsg.Equals(":taejang:")) {
			Text exittemp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
			exittemp.text = exittemp.text + "\n" + msg.clientId.ToString() + " 님이 퇴장했습니다.";
			return;
		}

		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + msg.clientId + " : " + msg.strMsg;
		Debug.Log ("Sender : " + msg.clientId + " /  Msg : " + msg.strMsg);
	}

	public void OnMessageGotoChatRoom(NetworkMessage netMsg)
	{
		Debug.Log ("Get GotoRoom Msg!");
		MyMessage_GotoChatRoom msg = netMsg.ReadMessage<MyMessage_GotoChatRoom> ();

		if (msg.clientId < 0){
			Debug.Log ("Try Exit!");
            // List에 012가 저장되어있다는 가정하에 Remove(1)을 사용하면 리스트는 02로 줄어듬. 인덱스도 동일하게 줄어듬.
            // 왜 -를 붙였지??
            ChatRoom[msg.roomNum - 1].Remove(-msg.clientId);
			Debug.Log ("REMOVE!");
			return;
		}

		ChatRoom [msg.roomNum - 1].Add (msg.clientId);

		string temp = "";
		for (int k = 0; k < ChatRoom [msg.roomNum - 1].Count; k++)
			temp = temp + ChatRoom [msg.roomNum - 1] [k] + "/";
		Debug.Log (temp);
	}

	public override void OnStartServer()
	{
		base.OnStartServer ();
		NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType, OnMessage);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType2, OnMessage2);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType3, OnMessage3);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType4, OnMessageGotoChatRoom);
		Debug.Log("OnStartServer( )");

        /*
		if (NetworkServer.active)
			transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = "Server is Working!";
        */
		for (int i = 0; i < 20; i++)
			ChatRoom.Add (new List<int> ());

		m_startServer.SetActive (false);
		m_startClient.SetActive (false);
	}

	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient (client);
		client.RegisterHandler(MyMsgType.CustomMsgType, OnMessage);
		client.RegisterHandler(MyMsgType.CustomMsgType2, OnMessage2);
		client.RegisterHandler(MyMsgType.CustomMsgType3, OnMessage3);
		client.RegisterHandler(MyMsgType.CustomMsgType4, OnMessageGotoChatRoom);
		m_client = client;

        m_startServer.SetActive(false);
        m_startClient.SetActive(false);
    }
		
	public void SetupServer()
	{
		Debug.Log("SetupServer()");
		StartServer();
	}

	public void SetupClient()
	{
		Debug.Log("SetupClient()");
		StartClient();    
	}

	public void SendToServer (string strMsg){
		MyMessage msg = new MyMessage ();
		msg.roomNum = m_currentRoom;
		msg.clientId = m_clientId;
		msg.strMsg = strMsg;
		m_client.Send(MyMsgType.CustomMsgType, msg);
	}

	public void SendID(int connectionID){
		MyMessage2 msg = new MyMessage2 ();
		msg.clientId = connectionID;
		NetworkServer.SendToClient (connectionID, MyMsgType.CustomMsgType2, msg);
	}

	public void OnConnected(NetworkMessage netMsg)
	{
		Debug.Log("Connected to server");
	}

	public void GotoRoom(int roomNumber) {
		MyMessage_GotoChatRoom msg = new MyMessage_GotoChatRoom ();
		msg.roomNum = roomNumber;
		msg.clientId = m_clientId;
		m_client.Send(MyMsgType.CustomMsgType4, msg);

        m_currentRoom = roomNumber;

        /*
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + roomNumber + " 번방에 입장했습니다.";
        */

        m_netLogPanel.text = "" + roomNumber + " 번 방에 입장했습니다.";

		MyMessage entryMsg = new MyMessage ();
		entryMsg.roomNum = m_currentRoom;
		entryMsg.clientId = m_clientId;
		entryMsg.strMsg = ":ipjang:";
		m_client.Send(MyMsgType.CustomMsgType, entryMsg);
	}

	public void ExitRoom(int roomNumber){
		MyMessage_GotoChatRoom msg = new MyMessage_GotoChatRoom ();
		msg.roomNum = roomNumber;
		msg.clientId = -m_clientId;
		m_client.Send(MyMsgType.CustomMsgType4, msg);
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + m_currentRoom + " 번방에서 퇴장했습니다.";

		MyMessage exitMsg = new MyMessage ();
		exitMsg.roomNum = m_currentRoom;
		exitMsg.clientId = m_clientId;
		exitMsg.strMsg = ":taejang:";
		m_client.Send(MyMsgType.CustomMsgType, exitMsg);

		m_currentRoom = 0;
	}
}