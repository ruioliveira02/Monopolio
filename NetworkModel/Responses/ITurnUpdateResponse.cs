using Monopolio;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel.Responses
{
    public interface ITurnUpdateResponse : IResponse
    {
        State State { get; set; }
    }
}
