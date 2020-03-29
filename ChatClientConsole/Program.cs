using System;
using System.Threading;
using SystemUtility;
using ChatClientConsole.Model;
using NBitcoin;
using Newtonsoft.Json;

namespace ChatClientConsole
{
    class Program
    {
        private const string cryptobaronsAppId = "8c731da6-b4f2-4100-8dda-10585fd32ee3";
        private const string thanosUserEmail = "baronkingthanos@gmail.com";
        private const string thanosUsername = "Baron Thanos";
        private const string ronyUserEmail = "rony4d@gmail.com";
        private const string ronysername = "rony the 4th";

        private const string thanosGroupCreatorEmail = "baronkingthanos@gmail.com";
        static Thread _mainThread = new Thread(MainThread);
        static void MainOne(string[] args)
        {
            _mainThread.Name = "Main thread";
            Console.Title = "Chat Client " + Guid.NewGuid();
            Text.WriteLine($"Initializing {_mainThread.Name}", TextType.DEBUG);

            var network = Network.TestNet;
            
            _mainThread.Start();
        }

        static void MainThread()
        {
            General.InitClient();
            bool isChatting = true;
            //create chat user 
            //create thanos
            //ClientTcp.CreateChatUser(cryptobaronsAppId, thanosUserEmail, thanosUsername);

            //create rony
            ClientTcp.CreateChatUser(cryptobaronsAppId, ronyUserEmail, ronysername);
            //get the chat history if any
            ClientTcp.ChatHistory(cryptobaronsAppId);

            //get the type of chat the user wants to enter
            Text.WriteLine("Press 1 for general chat  ", TextType.INFO);
            Text.WriteLine("Press 2 for Group chat  ", TextType.INFO);

            //check validity of input
            bool isValid = int.TryParse(Console.ReadLine(), out int groupType);
            if (!isValid)
            {
                Text.WriteLine("Ehee!!!", TextType.WARNING);
            }
            string groupName = null;
            if (groupType == (int)GroupType.Private)
            {
                Text.WriteLine("Enter a valid group name like rony4d@gmail.com ", TextType.INFO);
                groupName = Console.ReadLine();
                Text.WriteLine("You are in the group chat module. Type your chat message...", TextType.INFO);

            }
            else if (groupType == (int)GroupType.General)
            {
                Text.WriteLine("You are in the general chat module. Type your chat message...", TextType.INFO);

            }

            while (isChatting)
            {
                string message = Console.ReadLine();

                switch (message)
                {
                    //Create group
                    case "creategroup":
                        ClientTcp.CreateGroup(cryptobaronsAppId, thanosGroupCreatorEmail);
                        break;
                    //Kill app
                    case "kill":
                        isChatting = false;
                        ClientTcp.DisconnectFromServer();
                        break;
                    case "joingroup":
                        //let rony join thano's group
                        ClientTcp.JoinGroup(cryptobaronsAppId, ronyUserEmail, thanosGroupCreatorEmail);
                        break;
                    //normal chat action
                    default:
                        string chatJsonString = GetChatMessageJsonString(message);
                        if (groupType == (int)GroupType.General)
                        {
                            ClientTcp.SendMessage(chatJsonString);
                        }
                        else if (groupType == (int)GroupType.Private)
                        {
                            ClientTcp.SendGroupMessage(chatJsonString, groupName, ronyUserEmail);
                        }
                        break;

                }
            }
        }

        static string GetChatMessageJsonString(string message)
        {
            ChatMessageViewModel model = new ChatMessageViewModel();
            model.Message = message;
            model.UserEmail = "rony4d@gmail.com";
            model.Username = "rony4d";
            model.AppId = cryptobaronsAppId;
            model.GroupType = (int) GroupType.General;
            return JsonConvert.SerializeObject(model);
        }

     
    }
}
