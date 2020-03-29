using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using SystemUtility;

namespace ClientServerConsole
{
    public class Client
    {
        //Assign the connection to this socket
        public TcpClient Socket;

        public NetworkStream ClientNetworkStream;

        public int ConnectionId;

        //Assign connections incoming data(Packet).
        private byte[] _clientReceiveBuffer;

        public ByteBuffer ByteBuffer;

        public Client(TcpClient socket, int connectionId)
        {
            try
            {
                if (socket == null)
                {
                    return;
                }

                Socket = socket;
                ConnectionId = connectionId;

                Socket.SendBufferSize = Constants.MAX_BUFFER_SIZE;
                Socket.ReceiveBufferSize = Constants.MAX_BUFFER_SIZE;
                ClientNetworkStream = socket.GetStream();

                Text.WriteLine($"Network stream initialized for client:{ConnectionId} ", TextType.INFO);

                _clientReceiveBuffer = new byte[Constants.MAX_BUFFER_SIZE];

                //Start listening to connections stream, to allow sending data over the network.
                ClientNetworkStream.BeginRead(_clientReceiveBuffer, Constants.NETWORK_STREAM_OFFSET,
                    Socket.ReceiveBufferSize, ReceiveBufferCallback, null);

                Text.WriteLine("Incoming connection from {0}", TextType.INFO, Socket.Client.RemoteEndPoint.ToString());

            }
            catch (Exception e)
            {
                Text.WriteLine($"Error occured in Client Constructor with message {e.Message}",TextType.ERROR);
                
            }
        }

        private void ReceiveBufferCallback(IAsyncResult result)
        {
            try
            {
                //Gets the length of the data packet from connection.
                int readBytes = ClientNetworkStream.EndRead(result);

                //If we are receiving nothing then the connection is closed.
                if (readBytes <= 0)
                {
                    //Properly close the connection from the server
                    CloseConnection();
                    return;
                }
                //resizing the byte array with the length of the received data.
                byte[] newBytesRead = new byte[readBytes];
                //copying the packet information with the received length to a new array 'newBytesRead'.
                Buffer.BlockCopy(_clientReceiveBuffer, Constants.NETWORK_STREAM_OFFSET,newBytesRead,Constants.NETWORK_STREAM_OFFSET,readBytes);
             
                //checking which 'packet' we got.
                ServerHandleData.HandleDataFromClient(ConnectionId, newBytesRead);
                //Start listening to connections stream, to allow getting other data over the network.
                ClientNetworkStream.BeginRead(_clientReceiveBuffer, Constants.NETWORK_STREAM_OFFSET, Socket.ReceiveBufferSize, ReceiveBufferCallback, null);


            }
            catch (Exception e)
            {
                //Properly close connection to the server
                CloseConnection();
                Text.WriteLine($"Error occured in ReceiveBufferCallback with message {e.Message}", TextType.ERROR);

            }
        }

        private void CloseConnection()
        {
            Text.WriteLine("Connection from {0} has been terminated", TextType.INFO, Socket.Client.RemoteEndPoint.ToString());
            //ToDo Disconnect player from server here
            Socket.Close();
            Socket = null;
        }
    }
}
