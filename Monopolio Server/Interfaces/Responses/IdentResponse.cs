using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Responses
{
    /// <summary>
    /// The server's response to a <cref>IdentRequest</cref>
    /// </summary> 
    public class IdentResponse : Response
    {
        /// <summary>
        /// Whether the user was accepted or not
        /// </summary> 
        public bool Accepted { get; set; }

        /// <summary>
        /// The name of the user who made the request
        /// </summary> 
        public string User { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary> 
        public IdentResponse(bool accepted, string user)
        {
            Accepted = accepted;
            User = user;
        }

        /// <summary>
        /// The message to display on the server's console to log this response
        /// </summary> 
        public override string Message()
        {
            return string.Format("{0} was {1}accepted", User, (Accepted ? "" : "not "));
            //TODO: if not accepted, why
        }
    }
}
