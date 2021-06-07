using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.Interfaces.Requests
{
    public class IdentRequest : Request, IIdentRequest
    {
        public IdentRequest(string user)
        {
            SenderID = user;
            Accepted = false;
        }
    }
}
