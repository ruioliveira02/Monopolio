using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel.Responses
{
    public interface IMiddleMoneyResponse : IResponse
    {
        int MiddleMoney { get; set; }
    }
}
