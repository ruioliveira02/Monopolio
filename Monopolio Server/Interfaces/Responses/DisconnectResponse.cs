using NetworkModel.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Responses
{
    public class DisconnectResponse : Response, IDisconnectResponse
    {
        public string Username { get; set; }

        public DisconnectResponse(string client)
        {
            Username = client;
        }

        public override string Message()
            => string.Format("{0} disconnected", Username);
    }
}
