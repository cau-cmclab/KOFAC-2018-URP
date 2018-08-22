using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/* 메시지 타입, 메시지 정의, 메시지 이벤트 함수 처리 */
public class Message : MonoBehaviour{

    //MyNetManager의 멤버에 접근하려면 'MyNetManager.instance'를 사용.

    #region 메시지 타입
    public class MyMsgType
    {
        // Msg_AssignClientId
        public const short AssignClientId = MsgType.Highest + 1;
        // Msg_Chat
        public const short SendChatToServer = MsgType.Highest + 2;
        // Msg_Chat
        public const short SendChatToClient = MsgType.Highest + 3;
        // 
        public const short InAndOutChatRoom = MsgType.Highest + 4;

        public const short CreateRoom = MsgType.Highest + 5;

        public const short InAndOutAlarm = MsgType.Highest + 6;

        public const short ChatRoomInfo = MsgType.Highest + 7;

    }
    #endregion

    #region 메시지 정의

    // client와 server가 사용하는 채팅메시지는 동일함.
    public class Msg_Chat : MessageBase
    {
        public int roomNum;
        public int clientId;
        public string strMsg;
    }

    public class Msg_AssignClientId : MessageBase
    {
        public int clientId;
    }

    public class Msg_InAndOutChatRoom : MessageBase
    {
        public int roomNum;
        public int clientId;
    }

    public class Msg_CreateRoom : MessageBase
    {
        public string roomName;
        public int clientId;
    }

    public class Msg_InAndOutAlarm : MessageBase
    {
        public string roomName;
        public int roomNum;
        public int clientId;
    }

    /* 메시지에는 구조체나 배열은 포함가능, 하지만 List는 제네릭이라서 불가함.*/
    public class Msg_ChatRoomInfo : MessageBase
    {
        public string[] roomName;
        public int[] roomNum;
        public int[] memberCount;  // 인원수

        public int clientId;

        // 생성자
        public Msg_ChatRoomInfo()
        {
            // 채팅방 개수만큼 생성
            roomName = new string[MyNetManager.instance.Chatroom.Count];
            roomNum = new int[MyNetManager.instance.Chatroom.Count];
            memberCount = new int[MyNetManager.instance.Chatroom.Count];
        }
    }

    #endregion


    /*서버가 처리하는 콜백함수.*/
    #region Server


    /// <summary>
    /// 클라이언트에게서 받은 메시지를 서버에서 다른 클라이언트들에게 재전송
    /// </summary>
    /// <param name="netMsg"></param>
    // SendChatToServer에 대한 콜백함수
    public static void OnMsgReceiveChatOnServer(NetworkMessage netMsg)
    {
        Msg_Chat msg = netMsg.ReadMessage<Msg_Chat>();

        // 메시지를 보낸 클라이언트와 같은 방에 접속한 클라이언트들에게 재전송
        for (int i = 0; i < MyNetManager.instance.Chatroom[msg.roomNum].member.Count; i++)
        {
            NetworkServer.SendToClient(MyNetManager.instance.Chatroom[msg.roomNum].member[i], MyMsgType.SendChatToClient, msg);
        }
    }

    public static void OnMsgCreateRoom(NetworkMessage netMsg)
    {
        Msg_CreateRoom msg = netMsg.ReadMessage<Msg_CreateRoom>();

        // 방 정보를 갖는 구조체를 만든다.
        MyNetManager.StructChatroom tmpRoom = new MyNetManager.StructChatroom();
        tmpRoom.roomName = msg.roomName;
        tmpRoom.roomNum = MyNetManager.instance.m_roomCount++; // 방 번호 부여
        tmpRoom.member = new List<int>();

        MyNetManager.instance.Chatroom.Add(tmpRoom); // 채팅방 목록에 방 추가
    }

    // 채팅방 입,퇴장 관리  (멤버들에게 전달하는 함수를 새로 정의하면 깔끔해지긴 하지만 이해가 어려운건 아니니까. 더 나은 방법이 있다면 수정부탁)
    public static void OnMsgInAndOutChatRoom(NetworkMessage netMsg)
    {
        Msg_InAndOutChatRoom msg = netMsg.ReadMessage<Msg_InAndOutChatRoom>();

        // 해당 방에 접속한 모든 클라이언트에게 접속한 클라이언트를 알린다.
        Msg_InAndOutAlarm msg_room = new Msg_InAndOutAlarm();
        msg_room.roomName = MyNetManager.instance.Chatroom[msg.roomNum].roomName;
        msg_room.roomNum = msg.roomNum;
        msg_room.clientId = msg.clientId;
        
        // Out에 대한 처리 (퇴장하는 클라이언트는 음수)
        
        if (msg.clientId < 0)
        {
            // 퇴장한 클라이언트와 같은 방에 접속해있는 클라이언트들에게 알림
            for (int i = 0; i < MyNetManager.instance.Chatroom[msg.roomNum].member.Count; i++)
            {
                NetworkServer.SendToClient(MyNetManager.instance.Chatroom[msg.roomNum].member[i], MyMsgType.InAndOutAlarm, msg_room);
            }

            // 퇴장하는 클라이언트에게도 전달하기 위해 멤버를 나중에 삭제
            MyNetManager.instance.Chatroom[msg.roomNum].member.Remove(-msg.clientId); // 채팅방 멤버 삭제
        }
        else  // In에 대한 처리
        {
            MyNetManager.instance.Chatroom[msg.roomNum].member.Add(msg.clientId); // 채팅방 멤버 추가

            // 멤버 추가 후 전달
            for (int i = 0; i < MyNetManager.instance.Chatroom[msg.roomNum].member.Count; i++)
            {
                NetworkServer.SendToClient(MyNetManager.instance.Chatroom[msg.roomNum].member[i], MyMsgType.InAndOutAlarm, msg_room);
            }
        }
    }
    // 서버의 채팅방을 복사해 클라이언트에게 전달
    public static void OnMsgChatRoomInfoOnServer(NetworkMessage netMsg)
    {
        Msg_ChatRoomInfo msg = netMsg.ReadMessage<Msg_ChatRoomInfo>();

        Msg_ChatRoomInfo InfoMsg = new Msg_ChatRoomInfo();


        for (int i = 0; i < MyNetManager.instance.Chatroom.Count; i++) {
            InfoMsg.roomName[i] = MyNetManager.instance.Chatroom[i].roomName;
            InfoMsg.roomNum[i] = MyNetManager.instance.Chatroom[i].roomNum;
            InfoMsg.memberCount[i] = MyNetManager.instance.Chatroom[i].member.Count;
        }

        NetworkServer.SendToClient(msg.clientId, MyMsgType.ChatRoomInfo, InfoMsg);
    }

    #endregion


    /*클라이언트가 처리하는 콜백함수.*/
    #region Client

    // 서버에서 받은 clientId 저장
    public static void OnMsgAssignClientId(NetworkMessage netMsg)
    {
        Msg_AssignClientId msg = netMsg.ReadMessage<Msg_AssignClientId>();
        MyNetManager.instance.m_clientId = msg.clientId;

        Debug.Log("GetID : " + msg.clientId);
    }

    /// <summary>
    /// 서버로부터 받은 메시지 처리
    /// </summary>
    /// <param name="netMsg"></param>
    // SendChatToClient에 대한 콜백함수
    public static void OnMsgReceiveChatOnClient(NetworkMessage netMsg)
    {
        Msg_Chat msg = netMsg.ReadMessage<Msg_Chat>();

        // 받은 메시지를 m_chatLog에 출력
        MyNetManager.instance.m_chatLog.text += "\n" + msg.clientId + " : " + msg.strMsg;
        Debug.Log("Sender : " + msg.clientId + " /  Msg : " + msg.strMsg);
    }

    // 클라이언트가 방 정보(방이름, 방번호)를 받아 화면에 입,퇴장 메시지 출력
    public static void OnMsgReceiveInAndOutAlarm(NetworkMessage netMsg)
    {
        Msg_InAndOutAlarm msg = netMsg.ReadMessage<Msg_InAndOutAlarm>();

        if (msg.clientId < 0) // 퇴장하는 클라이언트라면
        {
            msg.clientId = -msg.clientId; // 양수로 전환

            // 퇴장하는 클라이언트가 본인이 아니라면
            if (msg.clientId != MyNetManager.instance.m_clientId)
            {
                MyNetManager.instance.m_chatLog.text += "\n" + msg.clientId.ToString() + " 님이 퇴장했습니다.";
            }
            else  // 퇴장하는 클라이언트 본인이라면
            {
                MyNetManager.instance.m_netInfoPanel.text = "";  /* 이 부분에 뭘 출력해야하지 ? */
                MyNetManager.instance.m_chatLog.text = ""; // chatLog 초기화
            }
        }
        else // 입장하는 클라이언트라면
        {
            // 입장하는 클라이언트가 본인이 아니라면
            if (msg.clientId != MyNetManager.instance.m_clientId)
            {
                MyNetManager.instance.m_chatLog.text += "\n" + msg.clientId.ToString() + " 님이 입장했습니다.";
            }
            else  // 입장하는 클라이언트 본인이라면
            {
                MyNetManager.instance.m_netInfoPanel.text = msg.roomNum + "번방 / " + msg.roomName;
                MyNetManager.instance.m_chatLog.text += "\n" + msg.roomName + "에 입장했습니다.";
            }
        }
    }

    // 클라이언트에서는 채팅방 목록, 이름, 접속인원수 만 관리한다. 
    public static void OnMsgChatRoomInfoOnClient(NetworkMessage netMsg)
    {
        Msg_ChatRoomInfo InfoMsg = netMsg.ReadMessage<Msg_ChatRoomInfo>();

        // 채팅방 정보를 새로 담기위해 초기화시킨다.
        MyNetManager.instance.Chatroom.Clear();

        // 개설된 방 개수 만큼 정보를 받는다.
        for (int i = 0; i < InfoMsg.roomNum.Length; i++)
        {
            // 방 정보를 갖는 구조체를 만든다.
            MyNetManager.StructChatroom roomInfo = new MyNetManager.StructChatroom();
            roomInfo.roomName = InfoMsg.roomName[i];
            roomInfo.roomNum = InfoMsg.roomNum[i];
            roomInfo.memberCount = InfoMsg.memberCount[i];  // 현재 방에 접속한 멤버가 누구인지 까지는 필요하지 않고 인원수만 가져온다.
                                                            /* 인원수만 가져오는 이유는 접속한 인원 목록까지 가져오고 싶지만 메시지로 List를 전달할 수 없어서 2차원배열을 사용하기보다 단순하게 사용하기 위해 인원수(1차원 배열)만 가져온다.
                                                             * 추후 변경하는 것도 고려해볼수있음 */
                                                                
            MyNetManager.instance.Chatroom.Add(roomInfo); // 채팅방 목록에 방 추가
        }

        /* 현재 개설된 방을 클라이언트 화면에 출력 */

        /*
        Debug.Log(MyNetManager.instance.Chatroom.Count + "개의 채팅방이 존재함. ");
        Debug.Log(MyNetManager.instance.Chatroom[0].roomNum + "번 채팅방의 이름은 " + MyNetManager.instance.Chatroom[0].roomName + "이며, " + MyNetManager.instance.Chatroom[0].memberCount + "명이 접속중임. ");
        */

        

        // 만들어진 방이 한개라도 있다면 모든 방 삭제
        while (MyNetManager.instance.m_roomListContent.transform.childCount > 0)
        {
            Destroy(MyNetManager.instance.m_roomListContent.transform.GetChild(0));
        }

        // 최신화된 채팅방 정보로 채팅방 생성
        for (int i = 0; i < MyNetManager.instance.Chatroom.Count; i++)
        {
            Button newChatRoom = Instantiate(MyNetManager.instance.m_chatRoomBtnPrfb);
            newChatRoom.transform.SetParent(MyNetManager.instance.m_roomListContent.transform);
        }
        
    }


    #endregion
}
