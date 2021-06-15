using System;
using System.Collections.Generic;
using System.Text;
using Monopolio;

namespace NetworkModel.Responses
{
    public interface IPlayerUpdateResponse : IResponse
    {
        Player Player { get; set; }
    }
}
