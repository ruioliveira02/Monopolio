using Monopolio_Server.Interfaces.Responses;
using NetworkModel.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopolio_Server.Interfaces.Requests
{
    public class ActionRequest : Request, IActionRequest
    {
        public Monopolio.Action Action { get; set; }

        public override Response Execute()
        {
            bool r = Server.State.Execute(Action, Server.State.GetPlayer(SenderID));
            return null;
        }

        public override string Message()
            => string.Format("{0} requests action {1}", SenderID, Action.ToString());
    }
}
