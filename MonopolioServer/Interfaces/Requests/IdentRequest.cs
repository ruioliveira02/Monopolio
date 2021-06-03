using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioServer
{

    /// <summary>
    /// Class that implements identification requests server-side, i.e., the first request a client makes when connecting
    /// to the server
    /// </summary> 
    public class IdentRequest : Request, IIdentRequest
    {
        /// <summary>
        /// Default constructor
        /// </summary> 
        public IdentRequest(string username)
        {
            SenderID = username;
        }

        /// <summary>
        /// Executes the identification of the client
        /// </summary> 
        public override IResponse Execute()
        {
            Accepted = true; //TODO:: Change to accomodate max number of players
            return new IdentResponse(Accepted, SenderID);
        }
        /// <summary>
        /// The message to display on the server's console to log this request
        /// </summary> 
        public override string Message()
        {
            return string.Format("{0}: {1} requested ID", DateTime.Now.ToString("dd-mm-yy HH:mm:ss"), SenderID);
        }
    }
}
