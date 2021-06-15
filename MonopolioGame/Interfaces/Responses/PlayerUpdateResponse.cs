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
    public class PlayerUpdateResponse : Response, IPlayerUpdateResponse
    {
        public Player Player { get; set; }

        public override void Execute(GameState game)
        {
            for(int i = 0; i < game.CurrentState.Players.Length; i++)
            {
                if(game.CurrentState.Players[i].name == Player.name)
                {
                    game.CurrentState.Players[i] = Player;
                    break;
                }
            }
        }
    }
}
