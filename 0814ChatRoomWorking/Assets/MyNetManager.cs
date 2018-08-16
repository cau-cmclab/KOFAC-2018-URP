using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MyNetManager : NetworkManager
{
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

    public NetworkClient m_client;
	public int m_clientId;
	public int m_currentRoom = -1;
	public int m_roomCount = 0;

	public Text ChatLog;

	public struct StructChatroom
	{
		public string roomName;
		public int roomNum;
		public List<int> member;
	};

	public List<StructChatroom> Chatroom = new List<StructChatroom> ();


    public override void OnStartServer()
	{
		base.OnStartServer ();
		NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
		NetworkServer.RegisterHandler(Message.MyMsgType.CustomMsgType, Message.OnMessage);

		NetworkServer.RegisterHandler(Message.MyMsgType.AssignClientId, Message.OnMsgAssignClientId);

		NetworkServer.RegisterHandler(Message.MyMsgType.CustomMsgType3, Message.OnMessage3);
		NetworkServer.RegisterHandler(Message.MyMsgType.CustomMsgType4, Message.OnMessageGotoChatRoom);
		NetworkServer.RegisterHandler (Message.MyMsgType.CustomMsgType_CreateRoom, Message.OnMessageCreateRoom);
		NetworkServer.RegisterHandler (Message.MyMsgType.CustomMsgType_RoomInfo, Message.OnMessageRoomInfo);
		Debug.Log("OnStartServer( )");

		if (NetworkServer.active)
			transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = "Server is Working!";

		transform.GetChild (0).GetChild (2).gameObject.SetActive (false);
		transform.GetChild (0).GetChild (3).gameObject.SetActive (false);
	}

	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient (client);
		client.RegisterHandler(Message.MyMsgType.CustomMsgType, Message.OnMessage);

		client.RegisterHandler(Message.MyMsgType.AssignClientId, Message.OnMsgAssignClientId);

		client.RegisterHandler(Message.MyMsgType.CustomMsgType3, Message.OnMessage3);
		client.RegisterHandler(Message.MyMsgType.CustomMsgType4, Message.OnMessageGotoChatRoom);
		client.RegisterHandler (Message.MyMsgType.CustomMsgType_CreateRoom, Message.OnMessageCreateRoom);
		client.RegisterHandler (Message.MyMsgType.CustomMsgType_RoomInfo, Message.OnMessageRoomInfo);
		m_client = client;

		transform.GetChild (0).GetChild (2).gameObject.SetActive (false);
		transform.GetChild (0).GetChild (3).gameObject.SetActive (false);
	}

	public void SetupServer()
	{
		Debug.Log("SetupServer()");
		StartServer();

		//RoomNameList.Add ("EMPTY");
		//ChatRoom.Add (new List<int> ());
		//Chatroom.Add(new StructChatroom());
	}

	public void SetupClient()
	{
		Debug.Log("SetupClient()");
		StartClient();
	}

	public void SendToServer (string strmsg){
        Message.MyMessage msg = new Message.MyMessage ();
		msg.roomNum = m_currentRoom;
		msg.clientId = m_clientId;
		msg.strMsg = strmsg;
		m_client.Send(Message.MyMsgType.CustomMsgType, msg);
	}

	public void OnConnected(NetworkMessage netMsg)
	{
		Debug.Log("Connected to server");
	}

	public void GotoRoom(int RoomNum) {
        Message.MyMessage_GotoChatRoom msg = new Message.MyMessage_GotoChatRoom ();
		msg.roomNum = RoomNum;
		msg.clientId = m_clientId;
		m_client.Send(Message.MyMsgType.CustomMsgType4, msg);

		m_currentRoom = RoomNum;

        Message.MyMessage hellomsg = new Message.MyMessage ();
		hellomsg.roomNum = m_currentRoom;
		hellomsg.clientId = m_clientId;
		hellomsg.strMsg = ":enter:";
		m_client.Send(Message.MyMsgType.CustomMsgType, hellomsg);
	}

	public void ExitRoom(int RoomNum){
        Message.MyMessage_GotoChatRoom msg = new Message.MyMessage_GotoChatRoom ();
		msg.roomNum = RoomNum;
		msg.clientId = -m_clientId;
		m_client.Send(Message.MyMsgType.CustomMsgType4, msg);
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + m_currentRoom + " 번방에서 퇴장했습니다.";
		Text RoomInfo = transform.GetChild (0).GetChild (0).GetComponent<Text> ();
		RoomInfo.text = "로비";

        Message.MyMessage exitmsg = new Message.MyMessage ();
		exitmsg.roomNum = m_currentRoom;
		exitmsg.clientId = m_clientId;
		exitmsg.strMsg = ":out:";
		m_client.Send(Message.MyMsgType.CustomMsgType, exitmsg);

		m_currentRoom = -1;
	}

	public void CreateRoom(string roomstr){
        Message.MyMessage_CreateRoom msg = new Message.MyMessage_CreateRoom ();
		msg.roomName = roomstr;
		msg.clientId = m_clientId;
		m_client.Send (Message.MyMsgType.CustomMsgType_CreateRoom, msg);
	}

	public int getroomnum(string str){
		//Debug.Log ("Index : " + Chatroom.IndexOf(Chatroom.Find (StructChatroom => StructChatroom.RoomName == str)));
		return Chatroom.IndexOf(Chatroom.Find (StructChatroom => StructChatroom.roomName == str));
	}


    #region Server

    // 클라이언트에게 ID 배정
    public void SendID(int connectionID)
    {
        Message.Msg_AssignClientId msg = new Message.Msg_AssignClientId();
        msg.clientId = connectionID;
        NetworkServer.SendToClient(connectionID, Message.MyMsgType.AssignClientId, msg);
    }

    #endregion

    #region Client

    #endregion
}
