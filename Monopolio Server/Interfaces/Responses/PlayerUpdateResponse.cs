using Monopolio;
using NetworkModel.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Responses
{
    public class PlayerUpdateResponse : Response, IPlayerUpdateResponse
    {
        public Player Player { get; set; }

        public PlayerUpdateResponse(Player p)
        {
            Player = p;
        }

        public override string Message()
            => string.Format("Updated player {0}", Player.name);
    }
}
