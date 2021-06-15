using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel.Responses
{
    public interface ITurnUpdateResponse : IResponse
    {
        int Turn { get; set; }
    }
}
