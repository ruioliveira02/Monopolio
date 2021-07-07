using Monopolio;
using MonopolioGame.Models;
using NetworkModel.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.Interfaces.Responses
{
    public class TurnUpdateResponse : Response, ITurnUpdateResponse
    {
        public int Turn { get; set; }

        public State State { get; set; }

        public TurnUpdateResponse(int turn, State state)
        {
            Turn = turn;
            State = state;
        }

        public override void Execute(GameState game)
        {
            game.CurrentState = State;
            game.Chat += string.Format("É a vez de {0}.\n", game.CurrentState.Players[Turn].name);
        }
    }
}
