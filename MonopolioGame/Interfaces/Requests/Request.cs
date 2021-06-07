using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.Interfaces.Requests
{
    public abstract class Request : IRequest
    {
        public string SenderID { get; set; }
        public bool Accepted { get; set; }
    }
}
