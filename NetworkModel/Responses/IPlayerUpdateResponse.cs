using System;
using System.Collections.Generic;
using System.Text;
using Monopolio;

namespace NetworkModel.Responses
{
    interface IPlayerUpdateResponse : IResponse
    {
        Player Player { get; set; }
    }
}
