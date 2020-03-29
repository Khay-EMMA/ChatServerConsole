using System;
using System.Collections.Generic;
using System.Text;

namespace SystemUtility
{
    public enum ServerPackets
    {
        ServerChatMessage = 1,
        ServerCreateGroup = 2,
        ServerChatHistory = 3,
        ServerJoinGroup = 4,
        ServerLeaveGroup = 5,
        ServerGroupChatMessage = 6

    }

    public enum ClientPackets
    {

        ClientChatMessage = 1,
        ClientCreateGroup = 2,
        ClientChatHistory = 3,
        ClientJoinGroup = 4,
        ClientLeaveGroup = 5,
        ClientGroupChatMessage = 6,
        ClientCreateChatUser = 7

    }

    public enum GroupType
    {
        General = 1,
        Private = 2
    }
}
