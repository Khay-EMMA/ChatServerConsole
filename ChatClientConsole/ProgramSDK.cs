using ChatClientConsole.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SystemUtility;

namespace ChatClientConsole
{

    /// <summary>
    /// This SDK will handle only General chat for now.
    /// 
    /// </summary>
    class ProgramSDK
    {
        private const string cryptobaronsAppId = "8c731da6-b4f2-4100-8dda-10585fd32ee3";
        private const string thanosUserEmail = "baronkingthanos@gmail.com";
        private const string thanosUsername = "Baron Thanos";
        private const string ronyUserEmail = "rony4d@gmail.com";
        private const string ronysername = "rony the 4th";

        private const string thanosGroupCreatorEmail = "baronkingthanos@gmail.com";
        static Thread _mainThread = new Thread(MainThread);
        static void Main(string[] args)
        {
            _mainThread.Name = "Main thread";
            Console.Title = "Chat Client " + Guid.NewGuid();
            Text.WriteLine($"Initializing {_mainThread.Name}", TextType.DEBUG);


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


           

            while (isChatting)
            {
                string message = Console.ReadLine();

                string chatJsonString = GetChatMessageJsonString(message);
                ClientTcp.SendMessage(chatJsonString);

            }
        }

        static string GetChatMessageJsonString(string message)
        {
            ChatMessageViewModel model = new ChatMessageViewModel();
            model.Message = message;
            model.UserEmail = "rony4d@gmail.com";
            model.Username = "rony4d";
            model.AppId = cryptobaronsAppId;
            model.GroupType = (int)GroupType.General;
            return JsonConvert.SerializeObject(model);
        }


    }
}
