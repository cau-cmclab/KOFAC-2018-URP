using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;


// 방나가기(ExitRoom)가 제대로 구현 안된 파일에 작성되었음.
// 주석처리한 것 '내'가 이해하기 위해 작성한것이기 때문에 가독성을 위해 지우거나 수정하여도 무방.
// 변수명 대략적인 작성 방법은 조만간 세미나에서 통일하기로 했음. (이해하는데 어려움이 생기기 때문)

// 화요일에 맞춰봐야 하는것. 
// 1. MyMessage 클래스 이름
// 2. 메시지 클래스에 중복되는 변수들이 많은데 반드시 나누어야 하는가? (MyMessage2와 MyMessage3의 경우 비슷함, MyMessage안에 MyMessage3와 GotoChatRoom가 포함된 상태인데 용도로 인해 구분되어야 하나?)
// 3. 메시지 핸들러의 경우 클라이언트와 서버가 사용하는 메시지들이 다른데, 클라이언트와 서버 둘다 OnStartServer와 OnStartClient에서 불필요한 콜백함수까지 전부 등록하는것으로 보임. 
// (개인적으로 서버와 클라이언트가 MyNetManager안에 같이 작성되다보니까 너무 헷갈림; 서버함수인지 클라이언트함수인지 파악하는데 시간이 많이 잡아먹힘. 적어도 서버, 클라이언트가 다른 역할을 하기때문에 서버는 여기다 작성하되, 클라이언트를 위한 스크립트를 따로 작성하는게 맞다고 봄. 간편하게 하기위해 MyNetManager 인스턴스 하나로 클라이언트 처리까지 같이 하고싶은거라면 분류라도 꼭 해야함. 플레이어들이 적극적으로 사용할 것이기 때문에 섞여있으면 안됨.)

// 대소문자 변경한건 크게 의미는 없음. 개인성향에 달라지는 것이기때문에. 근데 일부 변경하다보니 그냥 통일성있게 하느라.. 변경해서 미안;;
public class MyNetManager : NetworkManager /* NetworkManager를 상속합니다. */
{
	public NetworkClient m_client;
    // m_clientId는 클라이언트 자신의 아이디
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
        // UserId 는 clientId와 동일한 내용임. clientId로 바꿔도 되지만 UserId라 쓴 이유가 있을것 같아서 놔두었음.
		public int userId;
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

    #region 콜백 함수

    public void OnMessage(NetworkMessage netMsg)
	{
		MyMessage msg = netMsg.ReadMessage<MyMessage> ();
		Debug.Log (msg.clientId + " / " + msg.strMsg);

        // 메시지1과 메시지3의 변수가 많이 겹치는데 굳이 재포장 해야하나? 그냥 변수를 사용 안하거나 정크값 주면 되는것 아닌가?
		MyMessage3 msg3 = new MyMessage3 ();
		msg3.clientId = msg.clientId;
		msg3.strMsg = msg.strMsg;

		for (int i = 0; i < ChatRoom[msg.roomNum - 1].Count; i++) {
			NetworkServer.SendToClient (ChatRoom [msg.roomNum - 1] [i], MyMsgType.CustomMsgType3, msg3);
			Debug.Log ("Send Success");
		}
	}

    // 서버로부터 클라이언트가 Msg2를 받으면 자신의 clientid를 얻어올 수 있다. m_clientId에 클라이언트 아이디 저장.
	public void OnMessage2(NetworkMessage netMsg)
	{
		MyMessage2 msg = netMsg.ReadMessage<MyMessage2> ();
		transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = "ID : " + msg.clientId.ToString ();
		m_clientId = msg.clientId;
		Debug.Log ("GetID : " + msg.clientId);
	}

	public void OnMessage3(NetworkMessage netMsg)
	{
		MyMessage3 msg = netMsg.ReadMessage<MyMessage3> ();
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + msg.clientId + " : " + msg.strMsg;
		Debug.Log ("Sender : " + msg.clientId + " /  Msg : " + msg.strMsg);
	}

    // 이 메서드는 서버에서 실행됨. 서버의 채팅방에 클라이언트 접속 처리.
	public void OnMessageGotoChatRoom(NetworkMessage netMsg)
	{
		MyMessage_GotoChatRoom msg = netMsg.ReadMessage<MyMessage_GotoChatRoom> ();
		Debug.Log ("111");

		if (msg.userId == -1) {
			for(int i=0;i<ChatRoom[msg.roomNum - 1].Count;i++){
				if(ChatRoom[msg.roomNum - 1][i] == m_clientId){
					ChatRoom [msg.roomNum - 1].RemoveAt (i);
					break;
				}
			}
			return;
		}

        // 서버에 클라이언트가 해당 방에 접속한것을 의미.
		ChatRoom [msg.roomNum - 1].Add (msg.userId);
		Debug.Log ("222");

        // 현재 방에 접속한 클라이언트 목록 출력.
        string temp = "";
        for (int k = 0; k < ChatRoom [msg.roomNum - 1].Count; k++)
			temp = temp + ChatRoom [msg.roomNum - 1] [k] + "/";
		Debug.Log (temp);
	}

    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server");
    }

    /// <summary>
    /// 서버 콜백함수 등록 및 채팅방 생성
    /// </summary>
    public override void OnStartServer()
	{
		base.OnStartServer ();
		NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType, OnMessage);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType2, OnMessage2);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType3, OnMessage3);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType4, OnMessageGotoChatRoom);
		Debug.Log("OnStartServer( )");

        // 채팅방 20개 생성
		for (int i = 0; i < 20; i++)
			ChatRoom.Add (new List<int> ());
    }

    /// <summary>
    /// 클라이언트 콜백함수 등록
    /// </summary>
    /// <param name="client"></param>
	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient (client);
		client.RegisterHandler(MyMsgType.CustomMsgType, OnMessage);
		client.RegisterHandler(MyMsgType.CustomMsgType2, OnMessage2);
		client.RegisterHandler(MyMsgType.CustomMsgType3, OnMessage3);
		client.RegisterHandler(MyMsgType.CustomMsgType4, OnMessageGotoChatRoom);
        
		m_client = client;
	}

    #endregion

    // Start Server Button
    public void SetupServer()
	{
		Debug.Log("SetupServer()");
		StartServer(); // OnStartServer 콜백함수 호출
	}

    // Start Client Button
	public void SetupClient()
	{
		Debug.Log("SetupClient()");
		StartClient(); // OnStartClient 콜백함수 호출
    }

    // player의 naeyong.text를 받아 서버로 전송.
    public void SendToServer (string strmsg){
		MyMessage msg = new MyMessage ();
		msg.roomNum = m_currentRoom;
		msg.clientId = m_clientId;
		msg.strMsg = strmsg;
		m_client.Send(MyMsgType.CustomMsgType, msg);
	}

    // connectionID에는 클라이언트의 connectionId가 전달됨.
    // 이 함수는 클라이언트로부터 clientId를 받고 다시 해당 클라이언트에 clientId를 전송함. (클라이언트가 자기 아이디를 알기 위함.)
	public void SendID(int connectionID){
		MyMessage2 msg = new MyMessage2 ();
		msg.clientId = connectionID;
		NetworkServer.SendToClient (connectionID, MyMsgType.CustomMsgType2, msg);
	}

	public void GotoRoom(int RoomNumber) {
		MyMessage_GotoChatRoom msg = new MyMessage_GotoChatRoom ();
		msg.roomNum = RoomNumber;
		msg.userId = m_clientId;
		m_client.Send(MyMsgType.CustomMsgType4, msg);

        // 클라이언트의 넷매니저 접속방 설정
		m_currentRoom = RoomNumber;
        Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + RoomNumber + " 번방에 입장\n";
	}

	public void ExitRoom(){
		MyMessage_GotoChatRoom msg = new MyMessage_GotoChatRoom ();
		msg.roomNum = m_currentRoom;
		msg.userId = -1;
		m_client.Send(MyMsgType.CustomMsgType4, msg);
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + m_currentRoom + " 번방에서 퇴장\n";
	}
}