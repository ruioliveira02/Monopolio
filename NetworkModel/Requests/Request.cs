
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    /// <summary>
    /// Base class for all the requests from clients
    /// </summary> 
    public abstract class Request
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
