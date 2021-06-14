using Monopolio_Server.Interfaces.Responses;
using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Requests
{
    public class CommandRequest : Request, ICommandRequest
    {
        public string Command { get; set; }

        /// <summary>
        /// Executes the identification of the client
        /// </summary> 
        public override Response Execute()
        {
            Accepted = Server.RunCommand(Command);
            return null; //changes to the State are broadcasted directly by the server
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
