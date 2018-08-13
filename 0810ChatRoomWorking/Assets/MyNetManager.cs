using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MyNetManager : NetworkManager
{
	public NetworkClient mClient;
	public int sender;
	public int currentroom = -1;
	public int RoomCount = 0;

	//List<List<int>> ChatRoom = new List<List<int>>();
	//List<string> RoomNameList = new List<string> ();

	public struct StructChatroom
	{
		public string RoomName;
		public List<int> Member;
	};

	public List<StructChatroom> Chatroom = new List<StructChatroom> ();

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

	public class MyMessage : MessageBase
	{
		public int Roomnum;
		public int sender;
		public string strmsg;
	}

	public class MyMessage2 : MessageBase
	{
		public int idd;
	}

	public class MyMessage3 : MessageBase
	{
		public int sender;
		public string strmsg;
	}

	public class MyMessage_GotoChatRoom : MessageBase
	{
		public string RoomName;
		public int User;
	}

	public class MyMessage_CreateRoom : MessageBase
	{
		public string RoomName;
		public int maker;
	}

	public class MyMessage_RoomInfo : MessageBase
	{
		public string RoomName;
		public int RoomNum;
	}

	public class MyMsgType
	{
		public static short CustomMsgType = MsgType.Highest + 1;
		public static short CustomMsgType2 = MsgType.Highest + 2;
		public static short CustomMsgType3 = MsgType.Highest + 3;
		public static short CustomMsgType4 = MsgType.Highest + 4;
		public static short CustomMsgType_CreateRoom = MsgType.Highest + 5;
		public static short CustomMsgType_RoomInfo = MsgType.Highest + 6;
	}

	public void OnMessage(NetworkMessage netMsg)
	{
		MyMessage msg = netMsg.ReadMessage<MyMessage> ();
		
		MyMessage3 msg3 = new MyMessage3 ();
		msg3.sender = msg.sender;
		msg3.strmsg = msg.strmsg;

		int i = 0;
		for (i = 0; i < Chatroom[msg.Roomnum].Member.Count; i++) {
			NetworkServer.SendToClient (Chatroom [msg.Roomnum].Member[i], MyMsgType.CustomMsgType3, msg3);
		}
	}

	public void OnMessage2(NetworkMessage netMsg)
	{
		MyMessage2 msg = netMsg.ReadMessage<MyMessage2> ();
		//transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = "ID : " + msg.idd.ToString ();
		sender = msg.idd;
		Debug.Log ("GetID : " + msg.idd);
	}

	public void OnMessage3(NetworkMessage netMsg)
	{
		MyMessage3 msg = netMsg.ReadMessage<MyMessage3> ();
		if (msg.strmsg == ":ipjang:") {
			if (msg.sender != sender) {
				Text entrytemp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
				entrytemp.text = entrytemp.text + "\n" + msg.sender.ToString () + " 님이 입장했습니다.";
			}
			return;
		}
		else if (msg.strmsg == ":taejang:") {
			Text exittemp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
			exittemp.text = exittemp.text + "\n" + msg.sender.ToString() + " 님이 퇴장했습니다.";
			return;
		}

		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + msg.sender + " : " + msg.strmsg;
		Debug.Log ("Sender : " + msg.sender + " /  Msg : " + msg.strmsg);
	}

	public void OnMessageGotoChatRoom(NetworkMessage netMsg)
	{
		Debug.Log ("5");
		MyMessage_GotoChatRoom msg = netMsg.ReadMessage<MyMessage_GotoChatRoom> ();
		if (msg.User < 0){
			Chatroom [getroomnum (msg.RoomName)].Member.Remove (-msg.User);
			return;
		}
		Debug.Log ("6");
		Chatroom [getroomnum (msg.RoomName)].Member.Add (msg.User);
		currentroom = getroomnum (msg.RoomName);
		//string temp = "";
		//int k;
		//for (k = 0; k < Chatroom [getroomnum (msg.RoomName)].Member.Count; k++)
		//	temp = temp + Chatroom [getroomnum (msg.RoomName)].Member [k] + "/";
		//Debug.Log (temp);
		Debug.Log ("7");
	}

	public void OnMessageCreateRoom(NetworkMessage netMsg)
	{
		Debug.Log ("2");
		MyMessage_CreateRoom msg = netMsg.ReadMessage<MyMessage_CreateRoom> ();
		StructChatroom temp = new StructChatroom ();
		temp.RoomName = msg.RoomName;
		temp.Member = new List<int> ();
		Chatroom.Add (temp);
		Debug.Log ("3");
		MyMessage_RoomInfo msg_room = new MyMessage_RoomInfo ();
		msg_room.RoomName = msg.RoomName;
		msg_room.RoomNum = getroomnum (msg.RoomName);
		NetworkServer.SendToClient (msg.maker, MyMsgType.CustomMsgType_RoomInfo, msg_room);
	}

	public void OnMessageRoomInfo(NetworkMessage netMsg)
	{
		Debug.Log ("4");
		MyMessage_RoomInfo msg = netMsg.ReadMessage<MyMessage_RoomInfo> ();
		Text RoomInfo = transform.GetChild (0).GetChild (0).GetComponent<Text> ();
		RoomInfo.text = msg.RoomNum + "번방 / " + msg.RoomName;
		GotoRoom(msg.RoomName);
		currentroom = msg.RoomNum;
		Debug.Log ("8");
	}


	public override void OnStartServer()
	{
		base.OnStartServer ();
		NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType, OnMessage);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType2, OnMessage2);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType3, OnMessage3);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType4, OnMessageGotoChatRoom);
		NetworkServer.RegisterHandler (MyMsgType.CustomMsgType_CreateRoom, OnMessageCreateRoom);
		NetworkServer.RegisterHandler (MyMsgType.CustomMsgType_RoomInfo, OnMessageRoomInfo);
		Debug.Log("OnStartServer( )");

		if (NetworkServer.active)
			transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = "Server is Working!";

		//int i;
		//for (i = 0; i < 20; i++)
		//	ChatRoom.Add (new List<int> ());

		transform.GetChild (0).GetChild (2).gameObject.SetActive (false);
		transform.GetChild (0).GetChild (3).gameObject.SetActive (false);
	}

	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient (client);
		client.RegisterHandler(MyMsgType.CustomMsgType, OnMessage);
		client.RegisterHandler(MyMsgType.CustomMsgType2, OnMessage2);
		client.RegisterHandler(MyMsgType.CustomMsgType3, OnMessage3);
		client.RegisterHandler(MyMsgType.CustomMsgType4, OnMessageGotoChatRoom);
		client.RegisterHandler (MyMsgType.CustomMsgType_CreateRoom, OnMessageCreateRoom);
		client.RegisterHandler (MyMsgType.CustomMsgType_RoomInfo, OnMessageRoomInfo);
		mClient = client;

		transform.GetChild (0).GetChild (2).gameObject.SetActive (false);
		transform.GetChild (0).GetChild (3).gameObject.SetActive (false);
	}

	public void SetupServer()
	{
		Debug.Log("SetupServer()");
		StartServer();
		//NetworkServer.Listen(7777);
		//NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
		//NetworkServer.RegisterHandler(MyMsgType.CustomMsgType, OnMessage);
		//mconnectid.text = NetworkServer.serverHostId.ToString ();

		//RoomNameList.Add ("EMPTY");
		//ChatRoom.Add (new List<int> ());
		//Chatroom.Add(new StructChatroom());
	}

	public void SetupClient()
	{
		Debug.Log("SetupClient()");
		StartClient();

		//myClient = new NetworkClient ();
		//myClient.Connect("127.0.0.1", 4444);
		//myClient.RegisterHandler (MyMsgType.CustomMsgType, OnMessage);
	}

	public void SendToServer (string strmsg){
		MyMessage msg = new MyMessage ();
		msg.Roomnum = currentroom;
		msg.sender = sender;
		msg.strmsg = strmsg;
		mClient.Send(MyMsgType.CustomMsgType, msg);
	}

	public void SendID(int connectionID){
		MyMessage2 msg = new MyMessage2 ();
		msg.idd = connectionID;
		NetworkServer.SendToClient (connectionID, MyMsgType.CustomMsgType2, msg);
	}

	public void OnConnected(NetworkMessage netMsg)
	{
		Debug.Log("Connected to server");
	}


	public void GotoRoom(string roomname) {
		MyMessage_GotoChatRoom msg = new MyMessage_GotoChatRoom ();
		msg.RoomName = roomname;
		msg.User = sender;
		mClient.Send(MyMsgType.CustomMsgType4, msg);
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + roomname + " 에 입장했습니다.";

		MyMessage hellomsg = new MyMessage ();
		hellomsg.Roomnum = currentroom;
		hellomsg.sender = sender;
		hellomsg.strmsg = ":ipjang:";
		mClient.Send(MyMsgType.CustomMsgType, hellomsg);
	}

	public void ExitRoom(int RoomNumber){
		MyMessage_GotoChatRoom msg = new MyMessage_GotoChatRoom ();
		//msg.RoomNum = RoomNumber;
		msg.User = -sender;
		mClient.Send(MyMsgType.CustomMsgType4, msg);
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + currentroom + " 번방에서 퇴장했습니다.";
		Text RoomInfo = transform.GetChild (0).GetChild (0).GetComponent<Text> ();
		RoomInfo.text = "로비";

		MyMessage exitmsg = new MyMessage ();
		exitmsg.Roomnum = currentroom;
		exitmsg.sender = sender;
		exitmsg.strmsg = ":taejang:";
		mClient.Send(MyMsgType.CustomMsgType, exitmsg);

		currentroom = 0;
	}

	public void CreateRoom(string roomstr){
		Debug.Log ("1");
		MyMessage_CreateRoom msg = new MyMessage_CreateRoom ();
		msg.RoomName = roomstr;
		msg.maker = sender;
		mClient.Send (MyMsgType.CustomMsgType_CreateRoom, msg);
	}

	public int getroomnum(string str){
		//Debug.Log ("Index : " + Chatroom.IndexOf(Chatroom.Find (StructChatroom => StructChatroom.RoomName == str)));
		return Chatroom.IndexOf(Chatroom.Find (StructChatroom => StructChatroom.RoomName == str));
	}
}
