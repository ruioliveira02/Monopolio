using Monopolio;
using MonopolioGame.Models;
using NetworkModel;
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
    public class IdentResponse : Response, IIdentResponse
    {
        /// <summary>
        /// Whether the user was accepted or not
        /// </summary> 
        public bool Accepted { get; set; }

        /// <summary>
        /// The name of the user who made the request
        /// </summary> 
        public string Username { get; set; }

        /// <summary>
        /// The current state of the game
        /// </summary> 
        public State State { get; set; }


        public override void Execute(GameState state)
        {
            if (Username == state.Player)
                state.Connected = Accepted;

            state.CurrentState = State;
        }
    }
}
