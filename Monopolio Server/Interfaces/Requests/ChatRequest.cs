using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monopolio_Server.Interfaces.Responses;
using NetworkModel;

namespace Monopolio_Server.Interfaces.Requests
{
    public class ChatRequest : Request, IChatRequest
    {
        public string Msg { get; set; }

        public override string Message()
            => string.Format("{0} says {1}", SenderID, Msg);

        public override Response Execute()
        {
            Accepted = true;
            return new ChatResponse(SenderID, Msg);
        }
    }
}
