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
		public int RoomNum;
		public int User;
	}

	public class MyMsgType
	{
		public static short CustomMsgType = MsgType.Highest + 1;
	}

	public class MyMsgType2
	{
		public static short CustomMsgType2 = MsgType.Highest + 2;
	}

	public class MyMsgType3
	{
		public static short CustomMsgType3 = MsgType.Highest + 3;
	}

	public class MyMsgType_GotoChatRoom
	{
		public static short CustomMsgType4 = MsgType.Highest + 4;
	}

	public void OnMessage(NetworkMessage netMsg)
	{
		MyMessage msg = netMsg.ReadMessage<MyMessage> ();
		Debug.Log (msg.sender + " / " + msg.strmsg);

		MyMessage3 msg3 = new MyMessage3 ();
		msg3.sender = msg.sender;
		msg3.strmsg = msg.strmsg;

		int i = 0;
		for (i = 0; i < ChatRoom[msg.Roomnum - 1].Count; i++) {
			NetworkServer.SendToClient (ChatRoom [msg.Roomnum - 1] [i], MyMsgType3.CustomMsgType3, msg3);
		}
	}

	public void OnMessage2(NetworkMessage netMsg)
	{
		MyMessage2 msg = netMsg.ReadMessage<MyMessage2> ();
		transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = "ID : " + msg.idd.ToString ();
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
		Debug.Log ("Get GotoRoom Msg!");
		MyMessage_GotoChatRoom msg = netMsg.ReadMessage<MyMessage_GotoChatRoom> ();
		if (msg.User < 0){
			Debug.Log ("Try Exit!");
			ChatRoom [msg.RoomNum - 1].Remove (-msg.User);
			Debug.Log ("REMOVE!");
			return;
		}

		ChatRoom [msg.RoomNum - 1].Add (msg.User);
		string temp = "";
		int k;
		for (k = 0; k < ChatRoom [msg.RoomNum - 1].Count; k++)
			temp = temp + ChatRoom [msg.RoomNum - 1] [k] + "/";
		Debug.Log (temp);
	}
		

	public override void OnStartServer()
	{
		base.OnStartServer ();
		NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
		NetworkServer.RegisterHandler(MyMsgType.CustomMsgType, OnMessage);
		NetworkServer.RegisterHandler(MyMsgType2.CustomMsgType2, OnMessage2);
		NetworkServer.RegisterHandler(MyMsgType3.CustomMsgType3, OnMessage3);
		NetworkServer.RegisterHandler(MyMsgType_GotoChatRoom.CustomMsgType4, OnMessageGotoChatRoom);
		Debug.Log("OnStartServer( )");

		if (NetworkServer.active)
			transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = "Server is Working!";

		int i;
		for (i = 0; i < 20; i++)
			ChatRoom.Add (new List<int> ());

		transform.GetChild (0).GetChild (2).gameObject.SetActive (false);
		transform.GetChild (0).GetChild (3).gameObject.SetActive (false);
	}

	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient (client);
		client.RegisterHandler(MyMsgType.CustomMsgType, OnMessage);
		client.RegisterHandler(MyMsgType2.CustomMsgType2, OnMessage2);
		client.RegisterHandler(MyMsgType3.CustomMsgType3, OnMessage3);
		client.RegisterHandler(MyMsgType_GotoChatRoom.CustomMsgType4, OnMessageGotoChatRoom);
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
		NetworkServer.SendToClient (connectionID, MyMsgType2.CustomMsgType2, msg);
	}

	public void OnConnected(NetworkMessage netMsg)
	{
		Debug.Log("Connected to server");
	}


	public void GotoRoom(int RoomNumber) {
		MyMessage_GotoChatRoom msg = new MyMessage_GotoChatRoom ();
		msg.RoomNum = RoomNumber;
		msg.User = sender;
		mClient.Send(MyMsgType_GotoChatRoom.CustomMsgType4, msg);
		currentroom = RoomNumber;
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + RoomNumber + " 번방에 입장했습니다.";

		MyMessage hellomsg = new MyMessage ();
		hellomsg.Roomnum = currentroom;
		hellomsg.sender = sender;
		hellomsg.strmsg = ":ipjang:";
		mClient.Send(MyMsgType.CustomMsgType, hellomsg);
	}

	public void ExitRoom(int RoomNumber){
		MyMessage_GotoChatRoom msg = new MyMessage_GotoChatRoom ();
		msg.RoomNum = RoomNumber;
		msg.User = -sender;
		mClient.Send(MyMsgType_GotoChatRoom.CustomMsgType4, msg);
		Text temp = transform.GetChild (0).GetChild (1).GetComponent<Text> ();
		temp.text = temp.text + "\n" + currentroom + " 번방에서 퇴장했습니다.";

		MyMessage exitmsg = new MyMessage ();
		exitmsg.Roomnum = currentroom;
		exitmsg.sender = sender;
		exitmsg.strmsg = ":taejang:";
		mClient.Send(MyMsgType.CustomMsgType, exitmsg);

		currentroom = 0;
	}
}