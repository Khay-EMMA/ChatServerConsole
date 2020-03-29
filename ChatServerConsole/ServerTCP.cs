using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SystemUtility;

namespace ClientServerConsole
{
    /* ----------------------------------------
    * |ServerTcp.cs Class by Ugochukwu Aronu © 2018|
    * ----------------------------------------
    * This class is needed to create the Server
    * itself. By using Sockets we allow multiple
    * Clients to connect to the Server and listen
    * to the stream to receive data.
    */
    public class ServerTcp
    {
        //TcpListener  Object to create the server itself.
        private static TcpListener _serverSocket = new TcpListener(IPAddress.Any,Constants.PORT);

        //Client array to allow a maximum number of connections.
        public static Client[] Clients = new Client[Constants.MAX_PLAYERS];


        public static void InitServer()
        {
            Text.WriteLine("Initializing server socket...", TextType.DEBUG);
            //Start the server socket
            _serverSocket.Start();
            //Accepting the connection and start a asynchronous Callback.
            _serverSocket.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);

        }

        private static void ClientConnectCallback(IAsyncResult result)
        {
            try
            {
                //Accepts the current connection and binds it to a temporary Socket(tcpClient Object) to allow other connections.
                TcpClient tcpClient = _serverSocket.EndAcceptTcpClient(result);
                //Accepting a new connection so multiple clients can connect.
                _serverSocket.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);

                //Looping through the client array.
                Text.WriteLine("New Client connection received", TextType.INFO);

                for (int i = 1; i < Constants.MAX_PLAYERS; i++)
                {
                    if (Clients[i].Socket == null)
                    {
                        Clients[i] = new Client(tcpClient,i);

                        //ToDo Add JoinMap
                        return;
                    }
                }

            }
            catch (Exception e)
            {
                Text.WriteLine($"Error occured in ServerTcp: ClientConnectCallback with message {e.Message} ",TextType.ERROR);
            }
        }

        public static void SendDataTo(int connectionID, byte[] data)
        {
            try
            {
                ByteBuffer byteBuffer = new ByteBuffer();
                byteBuffer.WriteInteger(data.GetUpperBound(0) - data.GetLowerBound(0) + 1);
                byteBuffer.WriteBytes(data);

                if (Clients[connectionID].ClientNetworkStream != null)
                {
                    Text.WriteLine("Client with connection Id" + connectionID + " has a network stream", TextType.DEBUG);

                    Clients[connectionID].ClientNetworkStream.BeginWrite(byteBuffer.ToArray(), Constants.NETWORK_STREAM_OFFSET, byteBuffer.ToArray().Length, null, null);

                }
                else
                {
                    Text.WriteLine("Client with connection Id" + connectionID + " has null network stream", TextType.ERROR);
                }
                //byteBuffer.Dispose();
            }
            catch (ObjectDisposedException ex)
            {
                Text.WriteLine("SendDataTo Object disposed exception: " + ex.Message, TextType.ERROR);

            }

        }
        public static void SendDataToAll(byte[] data)
        {
            try
            {
                for (int i = 1; i < Constants.MAX_PLAYERS; i++)
                {
                    if (Clients[i].Socket != null)
                    {

                        SendDataTo(i, data);
                        
                    }
                }
            }
            catch (ObjectDisposedException ex)
            {
                Text.WriteLine("SendDataToAll Object disposed exception: " + ex.Message, TextType.ERROR);

            }

        }
        private static void SendDataToAllBut(int connectionId, byte[] data)
        {
            try
            {
                for (int i = 1; i < Constants.MAX_PLAYERS; i++)
                {
                    if (i != connectionId)
                    {
                        if (Clients[i].Socket != null)
                        {
                            SendDataTo(i, data);
                        }
                    }

                }
            }
            catch (ObjectDisposedException ex)
            {
                Text.WriteLine("SendDataToAllBut Object disposed exception: " + ex.Message, TextType.ERROR);

            }

        }

        /// <summary>
        /// This function actually sends the chat message
        /// from the server to the clients.
        /// NOTE: This message was initially sent from a client and wants
        /// to get it across to other clients
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="msg"></param>
        internal static void SendChatMessageToClient(int connectionId, string msg)
        {
            //Todo: Note that we must read this data from the client the same way we sent it
            
            ByteBuffer byteBuffer = new ByteBuffer();
            byteBuffer.WriteInteger((int)ServerPackets.ServerChatMessage);
            byteBuffer.WriteString(msg);
            SendDataToAll(byteBuffer.ToArray());
        }
    }
}
