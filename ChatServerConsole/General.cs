using System;
using System.Collections.Generic;
using System.Text;
using SystemUtility;

namespace ClientServerConsole
{
    public class General
    {
        //Get the current time in milliseconds from server
        public static int GetTickCount()
        {
            return Environment.TickCount;
        }

        public static void InitServer()
        {
            Text.WriteLine("Loading server ...", TextType.DEBUG);

            int start = GetTickCount();

            InitClients();
            ServerHandleData.InitPacketsFromClient();
            ServerTcp.InitServer();

            int end = GetTickCount();

            Text.WriteLine("Server loaded in {0} ms", TextType.DEBUG, end - start);
        }

        private static void InitClients()
        {
            for (int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                ServerTcp.Clients[i] = new Client(null,0);
                
            }
            Text.WriteLine($"{Constants.MAX_PLAYERS} Clients Initialization Complete..", TextType.DEBUG);

        }

    }
}
