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
        public static short AssignClientId = MsgType.Highest + 1;
        // Msg_Chat
        public static short SendChatToServer = MsgType.Highest + 2;
        // Msg_Chat
        public static short SendChatToClient = MsgType.Highest + 3;
        // 
        public static short CustomMsgType4 = MsgType.Highest + 4;

        public static short CustomMsgType_CreateRoom = MsgType.Highest + 5;

        public static short CustomMsgType_RoomInfo = MsgType.Highest + 6;
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

    public class MyMessage_GotoChatRoom : MessageBase
    {
        public int roomNum;
        public int clientId;
    }

    public class MyMessage_CreateRoom : MessageBase
    {
        public string roomName;
        public int clientId;
    }

    public class MyMessage_RoomInfo : MessageBase
    {
        public string roomName;
        public int roomNum;
    }

    #endregion

    







    /*서버가 처리하는 콜백함수.*/
    #region Server


    /// <summary>
    /// 클라이언트에게서 받은 메시지를 서버에서 다른 클라이언트들에게 재전송
    /// </summary>
    /// <param name="netMsg"></param>
    // SendChatToServer에 대한 콜백함수
    public static void OnMsgReceiveChatFromClient(NetworkMessage netMsg)
    {
        Msg_Chat msg = netMsg.ReadMessage<Msg_Chat>();

        // 메시지를 보낸 클라이언트와 같은 방에 접속한 클라이언트들에게 재전송
        for (int i = 0; i < MyNetManager.instance.Chatroom[msg.roomNum].member.Count; i++)
        {
            NetworkServer.SendToClient(MyNetManager.instance.Chatroom[msg.roomNum].member[i], MyMsgType.SendChatToClient, msg);
        }
    }

    public static void OnMessageCreateRoom(NetworkMessage netMsg)
    {
        MyMessage_CreateRoom msg = netMsg.ReadMessage<MyMessage_CreateRoom>();

        // 방 정보를 갖는 구조체를 만든다.
        MyNetManager.StructChatroom tmpRoom = new MyNetManager.StructChatroom();
        tmpRoom.roomName = msg.roomName;
        tmpRoom.roomNum = MyNetManager.instance.m_roomCount++; // 방 번호 부여
        tmpRoom.member = new List<int>();

        MyNetManager.instance.Chatroom.Add(tmpRoom); // 채팅방 목록에 추가

        // 방 정보 메시지를 만든다.
        MyMessage_RoomInfo msg_room = new MyMessage_RoomInfo();
        msg_room.roomName = msg.roomName;
        msg_room.roomNum = tmpRoom.roomNum;

        // 방을 만들었던 클라이언트에게 방정보 전송
        NetworkServer.SendToClient(msg.clientId, MyMsgType.CustomMsgType_RoomInfo, msg_room);

        Debug.Log(MyNetManager.instance.Chatroom[0].roomNum + "/" + tmpRoom.roomNum);
    }

    public static void OnMessageGotoChatRoom(NetworkMessage netMsg)
    {
        MyMessage_GotoChatRoom msg = netMsg.ReadMessage<MyMessage_GotoChatRoom>();

        // Exit에 대한 처리
        if (msg.clientId < 0)
        {
            MyNetManager.instance.Chatroom[msg.roomNum].member.Remove(-msg.clientId);
            return;
        }

        // Enter에 대한 처리
        MyNetManager.instance.Chatroom[msg.roomNum].member.Add(msg.clientId);

        MyMessage_RoomInfo msg_room = new MyMessage_RoomInfo();
        msg_room.roomName = MyNetManager.instance.Chatroom[msg.roomNum].roomName;
        msg_room.roomNum = MyNetManager.instance.Chatroom[msg.roomNum].roomNum;
        NetworkServer.SendToClient(msg.clientId, MyMsgType.CustomMsgType_RoomInfo, msg_room);

        string temp = "";
        for (int i = 0; i < MyNetManager.instance.Chatroom[msg.roomNum].member.Count; i++)
            temp = temp + MyNetManager.instance.Chatroom[msg.roomNum].member[i] + "/";
        Debug.Log(temp);
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
    public static void OnMsgReceiveChatFromServer(NetworkMessage netMsg)
    {
        Msg_Chat msg = netMsg.ReadMessage<Msg_Chat>();

        // 입장, 퇴장에 대해 
        if (msg.strMsg.Equals(":enter:"))
        {
            if (msg.clientId != MyNetManager.instance.m_clientId)
            {
                MyNetManager.instance.m_chatLog.text += "\n" + msg.clientId.ToString() + " 님이 입장했습니다.";
            }
            return;
        }
        else if (msg.strMsg.Equals(":out:"))
        {
            MyNetManager.instance.m_chatLog.text += "\n" + msg.clientId.ToString() + " 님이 퇴장했습니다.";
            return;
        }

        // 받은 메시지를 m_chatLog에 출력
        MyNetManager.instance.m_chatLog.text += "\n" + msg.clientId + " : " + msg.strMsg;
        Debug.Log("Sender : " + msg.clientId + " /  Msg : " + msg.strMsg);
    }

    // 
    public static void OnMessageRoomInfo(NetworkMessage netMsg)
    {
        MyMessage_RoomInfo msg = netMsg.ReadMessage<MyMessage_RoomInfo>();

        // 현재 m_currentRoom 초기화 하나때문에 메시지를 번복하는 꼴임.
        if (MyNetManager.instance.m_currentRoom != msg.roomNum)
        {
            MyNetManager.instance.GotoRoom(msg.roomNum);
            // MyNetManager.instance.m_currentRoom = msg.roomNum; // GotoRoom메서드에서 중복됨. 
        }
        else
        {
            Text RoomInfo = MyNetManager.instance.transform.GetChild(0).GetChild(0).GetComponent<Text>();
            RoomInfo.text = msg.roomNum + "번방 / " + msg.roomName;
            MyNetManager.instance.m_chatLog.text += "\n" + msg.roomName + "에 입장했습니다.";
        }
    }
    #endregion
}
