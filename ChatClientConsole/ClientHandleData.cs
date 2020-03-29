using System;
using System.Collections.Generic;
using System.Text;
using SystemUtility;
using ChatClientConsole.Model;
using Newtonsoft.Json;

namespace ChatClientConsole
{
    /* ----------------------------------------
    * |ClientHandleData.cs Class by Ugochukwu Aronu © 2018|
    * ----------------------------------------
    * This class is needed to handle all information
    * which get sent by the server to the client.
    * It checks which packet got sent, reads it
    * out and will executed the assigned code for it.
    */
    public class ClientHandleData
    {
        //makes sure that you need a index and packet(byte[]) to read out the dictionary
        public delegate void PacketDelegate(byte[] data);

        //dictionary filled with packets to listen to.
        private static Dictionary<int, PacketDelegate> _packetDictionary;

        public static int PacketLength;

        private static ByteBuffer _playerByteBuffer;
        //creates a new dictionary of packets to listen to so it executes the correct
        //method when the packet is arriving at the server.
        public static void InitPacketsFromServer()
        {
            _packetDictionary = new Dictionary<int, PacketDelegate>
            {
                //Add your packets in here, so the client knows which method to execute.
                { (int)ServerPackets.ServerChatMessage,HandleChatMessageFromServer},
                { (int)ServerPackets.ServerChatHistory,HandleChatHistoryFromServer},
                { (int)ServerPackets.ServerCreateGroup,HandleServerCreateGroupResponse},

            };
            Text.WriteLine("Server Packets Initialized...", TextType.DEBUG);

        }

        /// <summary>
        /// Handles response from the server when create group packet is sent
        /// </summary>
        /// <param name="data"></param>
        private static void HandleServerCreateGroupResponse(byte[] data)
        {
            try
            {
                //Creates a new instance of the buffer to read out the packet.
                ByteBuffer buffer = new ByteBuffer();
                //writes the packet into a list to make it available to read it out.
                buffer.WriteBytes(data);
                //Todo INFO: You always have to read out the data as you sent it. 
                //In this case you always have to first to read out the packet identifier.
                int packetIdentify = buffer.ReadInteger();
                //0 or 1 which maps to true or false.
                int status = buffer.ReadInteger();
                //In the server side you now send a string as next so you have to read out the string as next.
                string responseMessage = buffer.ReadString();

                Text.WriteLine(responseMessage, TextType.INFO);

            }
            catch (Exception e)
            {
                //todo: Log error in database
                Console.WriteLine(e);

            }
        }

        /// <summary>
        /// Displays chat history when the server responds
        /// </summary>
        /// <param name="data"></param>
        private static void HandleChatHistoryFromServer(byte[] data)
        {
            try
            {
                //Creates a new instance of the buffer to read out the packet.
                ByteBuffer buffer = new ByteBuffer();
                //writes the packet into a list to make it available to read it out.
                buffer.WriteBytes(data);
                //Todo INFO: You always have to read out the data as you sent it. 
                //In this case you always have to first to read out the packet identifier.
                int packetIdentify = buffer.ReadInteger();
                //In the server side you now send a string as next so you have to read out the string as next.
                string chatHistoryJsonString = buffer.ReadString();

                List<ChatMessageViewModel> chatHistory = JsonConvert.DeserializeObject<List<ChatMessageViewModel>>(chatHistoryJsonString);

                //print out chat history from server
                for (int i = 0; i < chatHistory.Count; i++)
                {
                    Text.WriteLine($"[{chatHistory[i].DateCreated}] {chatHistory[i].Username} : {chatHistory[i].Message}", TextType.INFO);
                }
            }
            catch (Exception e)
            {
                //todo: Log error in database
                Console.WriteLine(e);
            
            }
        }

        /// <summary>
        /// Accurately extract the data and assign
        /// the corresponding bytes to the Client's ByteBuffer
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="newBytesRead"></param>
        internal static void HandleDataFromServer(byte[] data)
        {
            byte[] buffer = (byte[])data.Clone();

            if (_playerByteBuffer == null)
            {
                //creating a new instance of 'Bytebuffer' to read out the packet.
                _playerByteBuffer = new ByteBuffer();
            }
            //writing incoming packet to the buffer.
            _playerByteBuffer.WriteBytes(buffer);

            if (_playerByteBuffer.Count() == 0)
            {
                _playerByteBuffer.Clear();
                return;
            }

            //Check if the byte buffer has data as big as an int size
            if (_playerByteBuffer.Count() >= Constants.INT_SIZE)
            {
                PacketLength = _playerByteBuffer.ReadInteger(false);
                if (PacketLength <= 0)
                {
                    //no packet exists in the bytebuffer
                    _playerByteBuffer.Clear();
                    return;
                }
            }

            while (PacketLength > 0 && PacketLength <= _playerByteBuffer.Length() - Constants.INT_SIZE)
            {
                if (PacketLength <= _playerByteBuffer.Length() - Constants.INT_SIZE)
                {
                    _playerByteBuffer.ReadInteger();

                    data = _playerByteBuffer.ReadBytes(PacketLength);

                    HandleDataPacketsFromServer(data);
                }


                PacketLength = 0;
                if (_playerByteBuffer.Length() >= Constants.INT_SIZE)
                {
                   // _playerByteBuffer.ReadInteger();

                    PacketLength = _playerByteBuffer.ReadInteger(false);

                    if (PacketLength <= 0)
                    {
                        _playerByteBuffer.Clear();
                        return;
                    }

                }

                if (PacketLength <= 1)
                {
                    _playerByteBuffer.Clear();
                }
            }
        }

        private static void HandleDataPacketsFromServer(byte[] data)
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
                packetDelegate.Invoke(data);
            }
        }

        private static void HandleChatMessageFromServer( byte[] data)
        {
            try
            {
                //Creates a new instance of the buffer to read out the packet.
                ByteBuffer buffer = new ByteBuffer();
                //writes the packet into a list to make it available to read it out.
                buffer.WriteBytes(data);
                //Todo INFO: You always have to read out the data as you sent it. 
                //In this case you always have to first to read out the packet identifier.
                int packetIdentify = buffer.ReadInteger();
                //In the server side you now send a string as next so you have to read out the string as next.
                string msg = buffer.ReadString();

                //print out the string msg you received from the server
                //Text.WriteLine($"Client Received a Packet with identifier {packetIdentify} with message: {msg} ", TextType.INFO);

                ChatMessageViewModel chatMessage = JsonConvert.DeserializeObject<ChatMessageViewModel>(msg);
                string messageToDisplay = $"App ID: {chatMessage.AppId} Message: {chatMessage.Message} " +
                                          $"User ID: {chatMessage.UserEmail} " +
                                          $"Group Type: {Enum.GetName(typeof(GroupType), chatMessage.GroupType) } " +
                                          $"Date: {DateTime.UtcNow}";
                Text.WriteLine(messageToDisplay, TextType.INFO);
            }
            catch (Exception e)
            {
                //todo: Log error to database
                Text.WriteLine($"Error occured in ServerHandleData:HandleChatMessageFromClient  with message {e.Message}", TextType.ERROR);

            }
        }

    }
}
