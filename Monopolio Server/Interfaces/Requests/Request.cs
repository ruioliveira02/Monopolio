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
    /// Base class for all the requests from clients
    /// </summary> 
    public abstract class Request : IRequest
    {
        /// <summary>
        /// The name of the client who sent the request
        /// </summary> 
        public string SenderID { get; set; }

        /// <summary>
        /// Whether the request was accepted or not
        /// </summary> 
        public bool Accepted { get; set; }

        /// <summary>
        /// Executes the request server side
        /// </summary> 
        public abstract Response Execute();

        /// <summary>
        /// The message to display on the server's console to log this request
        /// </summary> 
        public abstract string Message();
    }
}
