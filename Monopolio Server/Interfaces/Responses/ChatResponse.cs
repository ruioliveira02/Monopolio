using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monopolio;
using NetworkModel;

namespace Monopolio_Server.Interfaces.Responses
{
    public class ChatResponse : Response, IChatResponse
    {
        public string Player { get; set; }

        public string Msg { get; set; }


        public ChatResponse(string player, string msg)
        {
            Player = player;
            Msg = msg;
        }

        public override string Message()
            => string.Format("<{0}>{1}", Player, Msg);
    }
}
