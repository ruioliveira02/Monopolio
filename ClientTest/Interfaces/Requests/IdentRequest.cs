using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    public class IdentRequest : Request, IIdentRequest
    {
        public IdentRequest(string name)
        {
            SenderID = name;
            Accepted = false;
        }
    }
}
