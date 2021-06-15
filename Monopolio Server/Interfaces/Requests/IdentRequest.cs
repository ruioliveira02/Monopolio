using Monopolio;
using Monopolio_Server.Interfaces.Responses;
using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Requests
{
    /// <summary>
    /// Class for all the identification requests, i.e., the first request a client makes when connecting
    /// to the server
    /// </summary> 
    public class IdentRequest : Request, IIdentRequest
    {
        /// <summary>
        /// Executes the identification of the client
        /// </summary> 
        public override Response Execute()
        {
            Accepted = SenderID != null
                && SenderID != ""
                && SenderID.Length <= 16
                && !SenderID.Contains('"')
                && !SenderID.Contains('\n')
                && Server.ClientsList.Count < Server.MaxClients
                && !Server.ClientsList.ContainsKey(SenderID);

            return new IdentResponse(Accepted, SenderID, Server.State);
        }
        /// <summary>
        /// The message to display on the server's console to log this request
        /// </summary> 
        public override string Message()
        {
            return string.Format("{0} requested ID", SenderID);
        }
    }
}
