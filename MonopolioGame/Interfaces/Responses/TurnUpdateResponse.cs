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

        public override void Execute(GameState game)
        {
            game.CurrentState.Turn = Turn;
        }
    }
}
