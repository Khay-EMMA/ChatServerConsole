using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClientConsole.Model
{
    public class ChatMessageViewModel
    {
        public string MessageId { get; set; }
        public string AppId { get; set; }
        public string Username { get; set; }

        public string Message { get; set; }
        public int GroupType { get; set; } // private, general
        public string GroupId { get; set; }
        public DateTime DateCreated { get; set; }
        public int Status { get; set; } //deleted
        public string UserEmail { get; set; }

    }
}
