using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MyNetManager : NetworkManager
{
    public NetworkClient m_client;
	public int m_clientId;
	public int m_currentRoom = -1; // 로컬 플레이어가 접속한 방. 로비는 -1임(방에 접속하지 않은 상태).
	public int m_roomCount = 0; // 방을 만들때마다 올라간다.

	public Text m_chatLog;  // 채팅 내역
    public GameObject m_startServer;
    public GameObject m_startClient;
    public Text m_netInfoPanel; // 네트워크 정보 메시지

    public Button m_chatRoomBtnPrfb; // 채팅방 버튼 프리팹
    public GameObject m_roomListContent; // 플레이어의 채팅방 리스트

    public Texture2D m_SndImage; // Test 64 64, 128 128, 256 256
    Texture2D m_recImage;
    public GameObject m_showImage;
    Sprite spr;
    private List<byte> imageBuffer = new List<byte>();
    
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
        NetworkServer.RegisterHandler(Message.MyMsgType.SendImageToServer, Message.OnMsgReceiveImageOnServer);

        Debug.Log("OnStartServer( )");

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

    // 기존 플레이어 오브젝트를 변경한다.
    /* 플레이어 오브젝트가 변경되면 플레이어 오브젝트가 가지고 있는 변수값들은 당연히 초기화된다. 유지해야할 정보가 있다면 여기서 넘겨주어야함. (현재로써는 캐릭터마다 다른 값을 갖는 정보는 없고 그나마 있는 것이 접속중인 채팅방인데 채팅방도 MyNetManager에서 실시간 동기화 되서 물려줄 필요 없음.) */
    public void RespawnPlayer(NetworkPlayer oldPlayer)
    {
        // spawnPrefabs[]은 MyNetworkManger의 SpawnInfo에 등록된 프리팹
        int randIndex = Random.Range(0, spawnPrefabs.Count);
        var conn = oldPlayer.connectionToClient;
        var newPlayer = Instantiate(spawnPrefabs[randIndex]) as GameObject;


        /* 클라이언트에 로컬플레이어가 사용하는 프리팹이 플레이어 오브젝트 하나라면 ReplacePlayerForConnection의 마지막 인수는 0임. */
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, 0);
        Destroy(oldPlayer.gameObject);
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
        client.RegisterHandler(Message.MyMsgType.SendImageToClient, Message.OnMsgReceiveImageOnClient);

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

        RefreshRoom();
    }

    // 채팅방 버튼 클릭시
    public void GotoRoom(int RoomNum)
    {
        // 이미 접속해 있다면
        if(m_currentRoom != -1)
        {
            m_netInfoPanel.text = "방에서 나간 후에 입장하세요. ";
            return;
        }

        // 입력한 방으로 접속
        Message.Msg_InAndOutChatRoom msg = new Message.Msg_InAndOutChatRoom();
        msg.roomNum = RoomNum;
        msg.clientId = m_clientId;
        m_client.Send(Message.MyMsgType.InAndOutChatRoom, msg);

        m_currentRoom = RoomNum;

        RefreshRoom();
    }

    // Exit 버튼
    public void ExitRoom(int RoomNum)
    {
        if(m_currentRoom == -1)
        {
            m_netInfoPanel.text = "입장중인 채팅방이 없습니다.";
            return;
        }

        Message.Msg_InAndOutChatRoom msg = new Message.Msg_InAndOutChatRoom();
        msg.roomNum = RoomNum;
        msg.clientId = -m_clientId;  // 클라이언트ID에 음수를 붙여 전달.
        m_client.Send(Message.MyMsgType.InAndOutChatRoom, msg);

        m_currentRoom = -1;

        RefreshRoom();
    }

    public void RefreshRoom()
    {
        // 채팅방 정보 요청
        Message.Msg_ChatRoomInfo msg = new Message.Msg_ChatRoomInfo();
        msg.clientId = m_clientId;
        m_client.Send(Message.MyMsgType.ChatRoomInfo, msg);
    }

    // 테스트 이미지 보내기
    public void TestSendImage(byte[] dataImage, string curState, int size)
    {
        Debug.Log("OnTestSendImage Call");

        Message.Msg_Image msg = new Message.Msg_Image();
        msg.roomNum = m_currentRoom;
        msg.clientId = this.m_clientId;
        msg.imageData = dataImage;
        msg.state = curState;
        msg.size = size;
        m_client.Send(Message.MyMsgType.SendImageToServer, msg);

        /*m_client.SendByChannel(Message.MyMsgType.SendImageToServer, msg, 2); 다른 채널 이용하는 방법. default채널은 Reliable Sequenced
         * 전송 속도를 위해 UnReliable Sequenced 채널을 이용하는 것도 생각해본다. */
    }

    // 테스트 이미지 받기
    public void TestReceiveImage(byte[] dataImage, string curState, int size)
    {
        Debug.Log("TestReceiveImage Call");

        // 순차적으로 버퍼에 저장
        for (int i = 0; i < size; i++)
            imageBuffer.Add(dataImage[i]);

        if (curState.Equals("FINISH"))
        {
            Debug.Log("FINISH");

            byte[] finalData = new byte[imageBuffer.Count];

            for (int i = 0; i < imageBuffer.Count; i++)
                finalData[i] = imageBuffer[i];

            imageBuffer.Clear();  // 다음 이미지를 위해 버퍼공간 초기화
            Debug.Log("Receive Data Size : " + finalData.Length);
            
            m_recImage = new Texture2D(m_SndImage.width, m_SndImage.height, TextureFormat.ARGB32, false);
            m_recImage.LoadImage(finalData);
            m_recImage.Apply();

            //m_recImage = new Texture2D(m_SndImage.width, m_SndImage.height, TextureFormat.RGBA32, false);
            //m_recImage.LoadRawTextureData(finalData);
            //m_recImage.Apply();

            spr = Sprite.Create(m_recImage, new Rect(0.0f, 0.0f, m_SndImage.width, m_SndImage.height), new Vector2(0.0f, 0.0f));
            m_showImage.GetComponent<Image>().sprite = spr;
        }
    }

    // 테스트 패킷 분할 전송
    public void OnSendData()
    {
        /* RawData Format
        Texture2D newTexture = new Texture2D(m_SndImage.width, m_SndImage.height, TextureFormat.RGBA32, false);
        newTexture.SetPixels32(m_SndImage.GetPixels32());
        newTexture.Apply();
        */

        // PNG Format Setting (PNG는 ARGB32텍스쳐에선 알파채널을 포함. RGB24에서는 미포함)
        Texture2D newTexture = new Texture2D(m_SndImage.width, m_SndImage.height, TextureFormat.ARGB32, false);
        // Apply Image
        newTexture.SetPixels(m_SndImage.GetPixels());
        newTexture.Apply();



        byte[] copyData = newTexture.EncodeToPNG();  // PNG로 인코딩
        //byte[] tempData = newTexture.GetRawTextureData();
        byte[] sndBuffer = new byte[1300];  // 패킷 크기를 1300으로 설정
        int pos = 0;
        int i = 0;
        int size = 0;
        int packet = 0;

        Debug.Log("Send Data Size : " + copyData.Length);

        // 반복 전송
        while (pos < copyData.Length)
        {
            sndBuffer[i] = copyData[pos];
            i++; pos++; size++;

            // 1300개를 받은 것이 마지막 패킷일경우
            if((i > sndBuffer.Length - 1) && (pos >= copyData.Length))
            {
                Debug.Log("마지막 패킷" + ++packet + "을 보냅니다.");
                TestSendImage(sndBuffer, "FINISH", size);  // 1300개 데이터 보낸다.
                break;
            }
            // 마지막 패킷은 아니지만 1300개까지 받았다면 
            else if (i > sndBuffer.Length - 1)
            {
                Debug.Log("패킷" + ++packet +  "을 보냅니다.");
                TestSendImage(sndBuffer, "SENDING", size);  // 1300개 데이터 보낸다.
                i = 0; size = 0;
            }
            // 1300개를 다 채우진 못햇지만 마지막 패킷이라면
            else if (pos >= copyData.Length)
            {
                Debug.Log("마지막 패킷" + ++packet + "을 보냅니다.");
                TestSendImage(sndBuffer, "FINISH", size); // 나머지 데이터를 보낸다.
                break;
            }
        }
    }

    #endregion
}

/* 한번에 얼만큼의 데이터를 보낼 수 있지? Maximum Transmission Unit : 1400 Byte
 * 그럼 1024 Byte씩 나누어서 보낸다. (추천 크기는 1300Byte )
   
     1024패킷을 다 채웠는가? -> 보낸다.
     1024패킷을 다 채우진 못했으나 마지막 데이터인가? -> 보낸다.*/