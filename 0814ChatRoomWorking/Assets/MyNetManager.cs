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

    public Button m_chatRoomBtnPrfb; // 채팅방 버튼 프리팹
    public GameObject m_roomListContent; // 플레이어의 채팅방 리스트
    
	public struct StructChatroom
	{
		public string roomName;
		public int roomNum;
        public List<int> member;

        /* ChatRoomInfo 메시지에서 채팅방 인원수를 활용하기 위한 변수임.
         * 오직 클라이언트에서 채팅방의 인원수를 알기위함이니 
         * 서버에서는 member.Count를 이용하면 됨. */
        public int memberCount;
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
        NetworkServer.RegisterHandler(Message.MyMsgType.SendChatToServer, Message.OnMsgReceiveChatOnServer);
        NetworkServer.RegisterHandler(Message.MyMsgType.InAndOutChatRoom, Message.OnMsgInAndOutChatRoom);
        NetworkServer.RegisterHandler(Message.MyMsgType.CreateRoom, Message.OnMsgCreateRoom);
        NetworkServer.RegisterHandler(Message.MyMsgType.ChatRoomInfo, Message.OnMsgChatRoomInfoOnServer);

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
        client.RegisterHandler(Message.MyMsgType.AssignClientId, Message.OnMsgAssignClientId);
        client.RegisterHandler(Message.MyMsgType.SendChatToClient, Message.OnMsgReceiveChatOnClient);
        client.RegisterHandler(Message.MyMsgType.InAndOutAlarm, Message.OnMsgReceiveInAndOutAlarm);
        client.RegisterHandler(Message.MyMsgType.ChatRoomInfo, Message.OnMsgChatRoomInfoOnClient);


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
        Message.Msg_CreateRoom msg = new Message.Msg_CreateRoom();
        msg.roomName = roomstr;
        msg.clientId = m_clientId;
        m_client.Send(Message.MyMsgType.CreateRoom, msg);

        /*해당 방으로 이동*/
        // 생성한 방번호를 서버에서 어떻게 가져올수있을까..?
        // 혹은 서버에서 해당 클라이언트에게 GotoRoom을 실행하도록 하는 간단한 방법이 있을까?
        Debug.Log("채팅방 생성");
        //RefreshRoom();
    }

    // Enter 버튼 클릭시 
    public void GotoRoom(int RoomNum)
    {
        // 입력한 방으로 접속
        Message.Msg_InAndOutChatRoom msg = new Message.Msg_InAndOutChatRoom();
        msg.roomNum = RoomNum;
        msg.clientId = m_clientId;
        m_client.Send(Message.MyMsgType.InAndOutChatRoom, msg);

        // 자신의 방 초기화 (CreateRoom의 경우에도 여기서 초기화됨.)
        /* 기존에는 CreateRoom에서 메시지를 통해 GotoRoom도 실행하도록 해서 자동으로 
           방에 참가하는 방식이었는데, 이유는 클라이언트가 생성된 방번호나 방목록을 알 수 없기때문이었음.
           여전히 지금도 방이만들어지면 클라이언트는 방번호를 입력해 참가해야하는 방식임. 여기서 필요한건 
           방 목록을 클라이언트에게 전송하는 메시지가 필요하다는것.*/
        m_currentRoom = RoomNum;
    }

    // Exit 버튼
    public void ExitRoom(int RoomNum)
    {
        Message.Msg_InAndOutChatRoom msg = new Message.Msg_InAndOutChatRoom();
        msg.roomNum = RoomNum;
        msg.clientId = -m_clientId;  // 클라이언트ID에 음수를 붙여 전달.
        m_client.Send(Message.MyMsgType.InAndOutChatRoom, msg);

        m_currentRoom = -1;
    }

    #endregion

    // 수정중
    /*
	public int GetRoomNum(string str){
		//Debug.Log ("Index : " + Chatroom.IndexOf(Chatroom.Find (StructChatroom => StructChatroom.RoomName == str)));
		return Chatroom.IndexOf(Chatroom.Find (StructChatroom => StructChatroom.roomName == str));
	}*/

    public void RefreshRoom()
    {
        // 채팅방 정보 요청
        Message.Msg_ChatRoomInfo msg = new Message.Msg_ChatRoomInfo();
        msg.clientId = m_clientId;
        m_client.Send(Message.MyMsgType.ChatRoomInfo, msg);
    }
   
}
