using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModel.Responses;

namespace Monopolio_Server.Interfaces.Responses
{
    public class MiddleMoneyResponse : Response, IMiddleMoneyResponse
    {
        public int MiddleMoney { get; set; }

        public MiddleMoneyResponse(int middleMoney)
        {
            MiddleMoney = middleMoney;
        }

        public override string Message()
            => string.Format("Middle money update to {0}", MiddleMoney);
    }
}
