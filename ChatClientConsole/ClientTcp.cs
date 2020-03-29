using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SystemUtility;

namespace ChatClientConsole
{
    /* ----------------------------------------
       * |ClientTcp.cs Class by Ugochukwu Aronu © 2017|
       * ----------------------------------------
       * This class is needed to create the Client
       * itself. By using Sockets we allow this application
       * to connect to the Server and listen
       * to the stream to receive data.
       */
    public class ClientTcp
    {
        private static TcpClient _clientSocket;
        private static NetworkStream _clientNetworkStream;
        private static byte[] _asyncBuffer;

        internal static void ConnectClientToServer(string serverIpAddress, int port)
        {
            try
            {
                _clientSocket = new TcpClient();
                _clientSocket.ReceiveBufferSize = Constants.MAX_BUFFER_SIZE;
                _clientSocket.SendBufferSize = Constants.MAX_BUFFER_SIZE;

                _asyncBuffer = new byte[Constants.MAX_BUFFER_SIZE * 2];
                _clientSocket.BeginConnect(serverIpAddress, port, ClientConnectCallback, _clientSocket);
            }
            catch
            {
               Text.WriteLine("Connecting to server...",TextType.INFO);

                Thread.Sleep(10000);

                ConnectClientToServer(Constants.SERVER_IP, Constants.PORT);
            }

        }

   

        private static void ClientConnectCallback(IAsyncResult result)
        {
            try
            {
                _clientSocket.EndConnect(result);
                if (_clientSocket.Connected == false)
                {
                    return;
                }
                else
                {
                    _clientSocket.NoDelay = true;
                    _clientNetworkStream = _clientSocket.GetStream();
                    // Constants.MAX_BUFFER_SIZE is multiplied 2 is multiplied to have a much large '
                    _clientNetworkStream?.BeginRead(_asyncBuffer, Constants.NETWORK_STREAM_OFFSET, Constants.MAX_BUFFER_SIZE * 2, ReceiveCallback, null);
              
                    Text.WriteLine("Successfully connected to server",TextType.INFO);

                    //request for chat history
                    ChatHistory("8c731da6-b4f2-4100-8dda-10585fd32ee3");
                }
            }
            catch
            {
                Text.WriteLine("Connecting to server...", TextType.INFO);

                Thread.Sleep(10000);

                ConnectClientToServer(Constants.SERVER_IP, Constants.PORT);

                //Text.WriteLine("ClientConnectCallback error: " + ex.Message, TextType.ERROR);

            }
        }

 

        private static void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int readByteSize = _clientNetworkStream.EndRead(result);
                if (readByteSize <= 0)
                {
                    return;
                }
                byte[] newBytes = new byte[readByteSize];
                Buffer.BlockCopy(_asyncBuffer, Constants.NETWORK_STREAM_OFFSET, newBytes, Constants.NETWORK_STREAM_OFFSET, readByteSize);

                ClientHandleData.HandleDataFromServer(newBytes);
                _clientNetworkStream?.BeginRead(_asyncBuffer, Constants.NETWORK_STREAM_OFFSET, Constants.MAX_BUFFER_SIZE * 2, ReceiveCallback, null);

            }
            catch
            {
                //todo: Write retry connection function
                ConnectClientToServer(Constants.SERVER_IP, Constants.PORT);

            }
        }

        public static void SendData(byte[] data)
        {
            try
            {
                ByteBuffer byteBuffer = new ByteBuffer();
                byteBuffer.WriteInteger(data.GetUpperBound(0) - data.GetLowerBound(0) + 1);
                byteBuffer.WriteBytes(data);
                _clientNetworkStream?.Write(byteBuffer.ToArray(), Constants.NETWORK_STREAM_OFFSET, byteBuffer.ToArray().Length);
                byteBuffer.Dispose();
            }
            catch (ObjectDisposedException ex)
            {
                Text.WriteLine("SendData disposed exception: " + ex.Message,TextType.ERROR);

            }
        }

        /// <summary>
        /// Creates the user on the server if user does not exist
        /// </summary>
        /// <param name="cryptobaronsAppId"></param>
        /// <param name="userEmail"></param>
        public static void CreateChatUser(string cryptobaronsAppId, string userEmail,string username)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ClientPackets.ClientCreateChatUser);
            buffer.WriteString(cryptobaronsAppId);
            buffer.WriteString(userEmail);
            buffer.WriteString(username);
            SendData(buffer.ToArray());
        }

        /// <summary>
        /// Sends the message to all connected clients
        /// </summary>
        /// <param name="message"></param>
        public static void SendMessage(string message)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ClientPackets.ClientChatMessage);
            buffer.WriteString(message);
            SendData(buffer.ToArray());
        }
        /// <summary>
        /// Send message to only clients in the group
        /// </summary>
        /// <param name="message"></param>
        /// <param name="groupName"></param>
        public static void SendGroupMessage(string message, string groupName,string userEmail)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ClientPackets.ClientGroupChatMessage);
            buffer.WriteString(message);
            buffer.WriteString(groupName);
            buffer.WriteString(userEmail);
            SendData(buffer.ToArray());
        }
        /// <summary>
        /// Gets the chat history for the connected app
        /// </summary>
        /// <param name="appId"></param>
        public static void ChatHistory(string appId)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ClientPackets.ClientChatHistory);
            buffer.WriteString(appId);
            SendData(buffer.ToArray());
        }

        /// <summary>
        /// Create a group on the server with appID and the user's email
        /// A user can create only one group
        /// A user can belong to only one group till he decided to leave the group
        /// Once a user creates a group, he is added to the group immediately
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userEmail"></param>
        public static void CreateGroup(string appId, string userEmail)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ClientPackets.ClientCreateGroup);
            buffer.WriteString(appId);
            buffer.WriteString(userEmail);
            SendData(buffer.ToArray());
        }
        public static void JoinGroup(string appId, string userEmail, string groupCreatorEmail)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ClientPackets.ClientJoinGroup);
            buffer.WriteString(appId);
            buffer.WriteString(userEmail);
            buffer.WriteString(groupCreatorEmail);
            SendData(buffer.ToArray());
        }
        public static void LeaveGroup(string appId, string userEmail,string groupCreatorEmail)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ClientPackets.ClientLeaveGroup);
            buffer.WriteString(appId);
            buffer.WriteString(userEmail);
            buffer.WriteString(groupCreatorEmail);
            SendData(buffer.ToArray());
        }

       
        public static void DisconnectFromServer()
        {
            _clientSocket.Close();
            _clientSocket = null;
        }
    }
}
