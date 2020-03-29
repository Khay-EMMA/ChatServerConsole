using System;
using System.Collections.Generic;
using System.Text;
using SystemUtility;

namespace ClientServerConsole
{

    /* ----------------------------------------
    * |ServerHandleData.cs Class by Ugochukwu Aronu © 2018|
    * ----------------------------------------
    * This class is needed to handle all info
    * which gets sent by the client to the server.
    * It checks which packet got sent, reads it
    * out and will execute the assigned code for it.
    */
    public class ServerHandleData
    {
        //makes sure that you need a index and packet(byte[]) to read out the dictionary
        public delegate void PacketDelegate(int connectionId, byte[] data);
        
        //dictionary filled with packets to listen to.
        private static Dictionary<int, PacketDelegate> _packetDictionary;

        public static int PacketLength;

        //creates a new dictionary of packets to listen to so it executes the correct
        //method when the packet is arriving at the server.
        public static void InitPacketsFromClient()
        {
            _packetDictionary = new Dictionary<int, PacketDelegate>
            {
                //Add your packets in here, so the server knows which method to execute.
                //Add your packets in here, so the client knows which method to execute.
                { (int)ClientPackets.ClientChatMessage,HandleChatMessageFromClient},
            };
            Text.WriteLine("Client Packets Initialized...", TextType.DEBUG);

        }



        /// <summary>
        /// Accurately extract the data and assign
        /// the corresponding bytes to the Client's ByteBuffer
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="newBytesRead"></param>
        internal static void HandleDataFromClient(int connectionId, byte[] data)
        {
            byte[] buffer = (byte[]) data.Clone();

            if (ServerTcp.Clients[connectionId].ByteBuffer == null)
            {
                //creating a new instance of 'Bytebuffer' to read out the packet.
                ServerTcp.Clients[connectionId].ByteBuffer = new ByteBuffer();
            }
            //writing incoming packet to the buffer.
            ServerTcp.Clients[connectionId].ByteBuffer.WriteBytes(buffer);

            if (ServerTcp.Clients[connectionId].ByteBuffer.Count() == 0)
            {
                ServerTcp.Clients[connectionId].ByteBuffer.Clear();
                return;
            }

            //Check if the byte buffer has data as big as an int size
            if (ServerTcp.Clients[connectionId].ByteBuffer.Count() >= Constants.INT_SIZE)
            {
                PacketLength = ServerTcp.Clients[connectionId].ByteBuffer.ReadInteger(false);
                if (PacketLength <= 0)
                {
                    //no packet exists in the bytebuffer
                    ServerTcp.Clients[connectionId].ByteBuffer.Clear();
                    return;
                }
            }

            while (PacketLength > 0 && PacketLength <= ServerTcp.Clients[connectionId].ByteBuffer.Length() - Constants.INT_SIZE)
            {
                if (PacketLength <= ServerTcp.Clients[connectionId].ByteBuffer.Length() - Constants.INT_SIZE)
                {
                    ServerTcp.Clients[connectionId].ByteBuffer.ReadInteger();

                    data = ServerTcp.Clients[connectionId].ByteBuffer.ReadBytes(PacketLength);

                    HandleDataPacketsFromClient(connectionId, data);
                }


                PacketLength = 0;
                if (ServerTcp.Clients[connectionId].ByteBuffer.Length() >= Constants.INT_SIZE)
                {

                    PacketLength = ServerTcp.Clients[connectionId].ByteBuffer.ReadInteger(false);

                    if (PacketLength <= 0)
                    {
                        ServerTcp.Clients[connectionId].ByteBuffer.Clear();
                        return;
                    }

                }

                if (PacketLength <= 1)
                {
                    ServerTcp.Clients[connectionId].ByteBuffer.Clear();
                }
            }
        }

        /// <summary>
        /// Correctly check the packet sent and invoke the
        /// correct function(delegate)
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="data"></param>
        private static void HandleDataPacketsFromClient(int connectionId, byte[] data)
        {
            //creating a new instance of 'Bytebuffer' to read out the packet.
            ByteBuffer byteBuffer = new ByteBuffer();
            //writing incoming packet to the buffer.
            byteBuffer.WriteBytes(data);
            //reads out the packet to see which packet we got.
            int packet = byteBuffer.ReadInteger();
            //closes the buffer.
            byteBuffer.Dispose();
            //checking if we are listening to that packet in the _packets Dictionary.
            if (_packetDictionary.TryGetValue(packet, out PacketDelegate packetDelegate))
            {
                //checks which Method is assigned to the packet and executes it,
                //index: the socket which sends the data
                //data: the packet byte [] with the information.
                packetDelegate.Invoke(connectionId, data);
            }
        }

        private static void HandleChatMessageFromClient(int connectionId, byte[] data)
        {
            try
            {
                //Creates a new instance of the buffer to read out the packet.
                ByteBuffer buffer = new ByteBuffer();
                //writes the packet into a list to make it avaiable to read it out.
                buffer.WriteBytes(data);
                //Todo INFO: You always have to read out the data as you sent it. 
                //In this case you always have to first to read out the packet identifier.
                int packetIdentify = buffer.ReadInteger();
                //In the server side you now send a string as next so you have to read out the string as next.
                string msg = buffer.ReadString();

                ServerTcp.SendChatMessageToClient(connectionId, msg);

                //print out the string msg you did send from the server.
                Text.WriteLine($"Got Packet with identifier {packetIdentify} with message: {msg} from connection with connection id: {connectionId}", TextType.INFO);
            }
            catch (Exception e)
            {
                Text.WriteLine($"Error occured in ServerHandleData:HandleChatMessageFromClient  with message {e.Message}", TextType.ERROR);

            }
        }
    }
}
