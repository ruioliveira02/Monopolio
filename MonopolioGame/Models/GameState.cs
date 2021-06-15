using Monopolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.Models
{
    public class GameState
    {
        public State? CurrentState { get; set; }

        public string Chat { get; set; }
        
        public string Player { get; set; }
        public bool Connected { get; set; }

        public bool ConnectionAttempt { get; set; }
        public GameState()
        {
            CurrentState = null;
            Chat = "";
            Player = "";
            Connected = false;
        }
        public GameState(State state)
        {
            CurrentState = state;
            Chat = "";
        }
    }
}
