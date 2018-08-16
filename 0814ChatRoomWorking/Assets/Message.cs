using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/* 메시지 타입, 메시지 정의, 메시지 이벤트 함수 처리 */
// MyNetManager클래스를 상속받음. 
public class Message : MyNetManager{

    //MyNetManager의 멤버에 접근하려면 'instance'를 사용하면 된다. (MyNetManager는 싱글턴임)

    #region 메시지 타입
    public class MyMsgType
    {
        public static short CustomMsgType = MsgType.Highest + 1;
        public static short AssignClientId = MsgType.Highest + 2;
        public static short CustomMsgType3 = MsgType.Highest + 3;
        public static short CustomMsgType4 = MsgType.Highest + 4;
        public static short CustomMsgType_CreateRoom = MsgType.Highest + 5;
        public static short CustomMsgType_RoomInfo = MsgType.Highest + 6;
    }
    #endregion

    #region 메시지 정의

    public class MyMessage : MessageBase
    {
        public int roomNum;
        public int clientId;
        public string strMsg;
    }

    // 서버가 클라이언트들에게 ID 배정
    public class Msg_AssignClientId : MessageBase
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

    public static void OnMessage(NetworkMessage netMsg)
    {
        MyMessage msg = netMsg.ReadMessage<MyMessage>();

        MyMessage3 msg3 = new MyMessage3();
        msg3.clientId = msg.clientId;
        msg3.strMsg = msg.strMsg;

        for (int i = 0; i < instance.Chatroom[msg.roomNum].member.Count; i++)
        {
            NetworkServer.SendToClient(instance.Chatroom[msg.roomNum].member[i], MyMsgType.CustomMsgType3, msg3);
        }
    }

    public static void OnMsgAssignClientId(NetworkMessage netMsg)
    {
        Msg_AssignClientId msg = netMsg.ReadMessage<Msg_AssignClientId>();
        //transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = "ID : " + msg.idd.ToString ();
        instance.m_clientId = msg.clientId;
        Debug.Log("GetID : " + msg.clientId);
    }

    public static void OnMessage3(NetworkMessage netMsg)
    {
        MyMessage3 msg = netMsg.ReadMessage<MyMessage3>();
        if (msg.strMsg.Equals(":enter:"))
        {
            if (msg.clientId != instance.m_clientId)
            {
                instance.ChatLog.text = instance.ChatLog.text + "\n" + msg.clientId.ToString() + " 님이 입장했습니다.";
            }
            return;
        }
        else if (msg.strMsg.Equals(":out:"))
        {
            instance.ChatLog.text = instance.ChatLog.text + "\n" + msg.clientId.ToString() + " 님이 퇴장했습니다.";
            return;
        }
        //??
        Text temp = instance.transform.GetChild(0).GetChild(1).GetComponent<Text>();
        temp.text = temp.text + "\n" + msg.clientId + " : " + msg.strMsg;
        Debug.Log("Sender : " + msg.clientId + " /  Msg : " + msg.strMsg);
    }

    public static void OnMessageGotoChatRoom(NetworkMessage netMsg)
    {
        MyMessage_GotoChatRoom msg = netMsg.ReadMessage<MyMessage_GotoChatRoom>();
        if (msg.clientId < 0)
        {
            instance.Chatroom[msg.roomNum].member.Remove(-msg.clientId);
            return;
        }
        instance.Chatroom[msg.roomNum].member.Add(msg.clientId);

        MyMessage_RoomInfo msg_room = new MyMessage_RoomInfo();
        msg_room.roomName = instance.Chatroom[msg.roomNum].roomName;
        msg_room.roomNum = instance.Chatroom[msg.roomNum].roomNum;
        NetworkServer.SendToClient(msg.clientId, MyMsgType.CustomMsgType_RoomInfo, msg_room);

        string temp = "";
        for (int k = 0; k < instance.Chatroom[msg.roomNum].member.Count; k++)
            temp = temp + instance.Chatroom[msg.roomNum].member[k] + "/";
        Debug.Log(temp);
    }

    public static void OnMessageCreateRoom(NetworkMessage netMsg)
    {
        MyMessage_CreateRoom msg = netMsg.ReadMessage<MyMessage_CreateRoom>();
        StructChatroom temp = new StructChatroom();
        temp.roomName = msg.roomName;
        temp.roomNum = instance.m_roomCount;
        instance.m_roomCount++;
        temp.member = new List<int>();
        instance.Chatroom.Add(temp);
        MyMessage_RoomInfo msg_room = new MyMessage_RoomInfo();
        msg_room.roomName = msg.roomName;
        msg_room.roomNum = temp.roomNum;
        NetworkServer.SendToClient(msg.clientId, MyMsgType.CustomMsgType_RoomInfo, msg_room);
        Debug.Log(instance.Chatroom[0].roomNum + "/" + temp.roomNum);
    }

    public static void OnMessageRoomInfo(NetworkMessage netMsg)
    {
        MyMessage_RoomInfo msg = netMsg.ReadMessage<MyMessage_RoomInfo>();

        if (instance.m_currentRoom != msg.roomNum)
        {
            instance.GotoRoom(msg.roomNum);
            instance.m_currentRoom = msg.roomNum;
        }
        else
        {
            Text RoomInfo = instance.transform.GetChild(0).GetChild(0).GetComponent<Text>();
            instance.ChatLog.text += "\n" + msg.roomName + "에 입장했습니다.";
            RoomInfo.text = msg.roomNum + "번방 / " + msg.roomName;
        }
    }
}
