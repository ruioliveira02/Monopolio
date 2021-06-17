using MonopolioGame.Models;
using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.Interfaces.Responses
{
    public class ChatResponse : Response, IChatResponse
    {
        public string Player { get; set; }
        public string Msg { get; set; }

        public override void Execute(GameState game)
        {
            string playerText = (game.Player == Player) ? "Eu" : Player;
            game.Chat += string.Format("\n{0}: {1}", playerText, Msg);
        }
    }
}
