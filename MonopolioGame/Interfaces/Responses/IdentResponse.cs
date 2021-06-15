using MonopolioGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.Interfaces.Responses
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

        public override void Execute(GameState state)
        {
            if (Accepted)
                state.Connected = true;
        }
    }
}
