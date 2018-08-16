using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MyNetManager : NetworkManager
{

    public NetworkClient m_client;
	public int m_clientId;
	public int m_currentRoom = -1; // 로컬 플레이어가 접속한 방
	public int m_roomCount = 0; // 방을 만들때마다 올라간다.

	public Text m_chatLog;  // 채팅 내역
    public GameObject m_startServer;
    public GameObject m_startClient;
    public Text m_netInfoPanel; // 네트워크 정보 메시지

	public struct StructChatroom
	{
		public string roomName;
		public int roomNum;
		public List<int> member;
	};

	public List<StructChatroom> Chatroom = new List<StructChatroom> ();


    private static MyNetManager mInstance;

    public static MyNetManager instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = FindObjectOfType<MyNetManager>();
            }
            return mInstance;
        }
    }



    // Exit 버튼
	public void ExitRoom(int RoomNum){
        Message.MyMessage_GotoChatRoom msg = new Message.MyMessage_GotoChatRoom ();
		msg.roomNum = RoomNum;
		msg.clientId = -m_clientId;
		m_client.Send(Message.MyMsgType.CustomMsgType4, msg);
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + m_currentRoom + " 번방에서 퇴장했습니다.";
		Text RoomInfo = transform.GetChild (0).GetChild (0).GetComponent<Text> ();
		RoomInfo.text = "로비";

        Message.Msg_Chat exitmsg = new Message.Msg_Chat ();
		exitmsg.roomNum = m_currentRoom;
		exitmsg.clientId = m_clientId;
		exitmsg.strMsg = ":out:";
		m_client.Send(Message.MyMsgType.SendChatToServer, exitmsg);

		m_currentRoom = -1;
	}



    // 어디서 호출??
	public int GetRoomNum(string str){
		//Debug.Log ("Index : " + Chatroom.IndexOf(Chatroom.Find (StructChatroom => StructChatroom.RoomName == str)));
		return Chatroom.IndexOf(Chatroom.Find (StructChatroom => StructChatroom.roomName == str));
	}


    #region Server

    // StartServer 버튼 클릭시 호출
    public void SetupServer()
    {
        Debug.Log("SetupServer()");
        StartServer();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(Message.MyMsgType.SendChatToServer, Message.OnMsgReceiveChatFromClient);
        //NetworkServer.RegisterHandler(Message.MyMsgType.AssignClientId, Message.OnMsgAssignClientId);
        //NetworkServer.RegisterHandler(Message.MyMsgType.SendChatToClient, Message.OnMsgReceiveChatFromServer);
        NetworkServer.RegisterHandler(Message.MyMsgType.CustomMsgType4, Message.OnMessageGotoChatRoom);
        NetworkServer.RegisterHandler(Message.MyMsgType.CustomMsgType_CreateRoom, Message.OnMessageCreateRoom);
        NetworkServer.RegisterHandler(Message.MyMsgType.CustomMsgType_RoomInfo, Message.OnMessageRoomInfo);
        Debug.Log("OnStartServer( )");

        /*if (NetworkServer.active)
            transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Server is Working!";
        */

        m_startServer.gameObject.SetActive(false);
        m_startClient.gameObject.SetActive(false);
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to Server");
    }

    // 클라이언트에게 ID 배정
    public void SendID(int connectionID)
    {
        Message.Msg_AssignClientId msg = new Message.Msg_AssignClientId();
        msg.clientId = connectionID;
        NetworkServer.SendToClient(connectionID, Message.MyMsgType.AssignClientId, msg);
    }

    #endregion



    #region Client

    // StartClient 클릭시 호출
    public void SetupClient()
    {
        Debug.Log("SetupClient()");
        StartClient();
    }

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
        //client.RegisterHandler(Message.MyMsgType.SendChatToServer, Message.OnMsgReceiveChatFromClient);
        client.RegisterHandler(Message.MyMsgType.AssignClientId, Message.OnMsgAssignClientId);
        client.RegisterHandler(Message.MyMsgType.SendChatToClient, Message.OnMsgReceiveChatFromServer);
        client.RegisterHandler(Message.MyMsgType.CustomMsgType4, Message.OnMessageGotoChatRoom);
        client.RegisterHandler(Message.MyMsgType.CustomMsgType_CreateRoom, Message.OnMessageCreateRoom);
        client.RegisterHandler(Message.MyMsgType.CustomMsgType_RoomInfo, Message.OnMessageRoomInfo);

        m_client = client;

        m_startServer.gameObject.SetActive(false);
        m_startClient.gameObject.SetActive(false);
    }

    // 클라이언트에서 서버로 채팅메시지 전송
    public void SendToServer(string strmsg)
    {
        Message.Msg_Chat msg = new Message.Msg_Chat();
        msg.roomNum = m_currentRoom;
        msg.clientId = m_clientId;
        msg.strMsg = strmsg;

        m_client.Send(Message.MyMsgType.SendChatToServer, msg);
    }

    // Create버튼 클릭시 호출
    /// <summary>
    /// 입력한 방이름으로 방 만들기, 방 번호는 개설된 순으로 부여
    /// </summary>
    /// <param name="roomstr"></param>
    public void CreateRoom(string roomstr)
    {
        Message.MyMessage_CreateRoom msg = new Message.MyMessage_CreateRoom();
        msg.roomName = roomstr;
        msg.clientId = m_clientId;
        m_client.Send(Message.MyMsgType.CustomMsgType_CreateRoom, msg);
    }

    // Enter 버튼 클릭시 
    public void GotoRoom(int RoomNum)
    {
        // 입력한 방으로 접속
        Message.MyMessage_GotoChatRoom msg = new Message.MyMessage_GotoChatRoom();
        msg.roomNum = RoomNum;
        msg.clientId = m_clientId;
        m_client.Send(Message.MyMsgType.CustomMsgType4, msg);

        // 자신의 방 초기화(클라이언트가 직접 자기 값을 수정함. 이를 다른 클라이언트가 어떻게 알수있는가?)
        m_currentRoom = RoomNum;

        Message.Msg_Chat hellomsg = new Message.Msg_Chat();
        hellomsg.roomNum = m_currentRoom;
        hellomsg.clientId = m_clientId;
        hellomsg.strMsg = ":enter:";
        m_client.Send(Message.MyMsgType.SendChatToServer, hellomsg);
    }



    #endregion
}
