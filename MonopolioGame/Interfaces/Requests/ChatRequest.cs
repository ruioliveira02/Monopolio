using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.Interfaces.Requests
{
    public class ChatRequest : Request, IChatRequest
    {
        public string Msg { get; set; }

        public ChatRequest(string user, string message)
        {
            Msg = message;
            SenderID = user;
        }
    }
}
