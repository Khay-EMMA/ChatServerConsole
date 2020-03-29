using System;
using System.Collections.Generic;
using System.Text;
using SystemUtility;

namespace ChatClientConsole
{
    public class General
    {
        public static void InitClient()
        {
            // Initialize all Players
            //ToDO: Create InitPlayers();

            //Initialize the packet list to listen to from server
            ClientHandleData.InitPacketsFromServer();

            //Connect to the Server 
            ClientTcp.ConnectClientToServer(Constants.SERVER_IP, Constants.PORT);
        }
    }
}
