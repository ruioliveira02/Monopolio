using NetworkModel.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Responses
{
    public class TurnUpdateResponse : Response, ITurnUpdateResponse
    {
        public int Turn { get; set; }

        public TurnUpdateResponse(int turn)
        {
            Turn = turn;
        }

        /*
        public override string Message()
            => string.Format("Begin turn of player {0} ({1})",
                Turn, Server.State.Players[Turn]);
        */

        public override string Message()
            => string.Format("Begin turn of player {0}", Turn);
    }
}
