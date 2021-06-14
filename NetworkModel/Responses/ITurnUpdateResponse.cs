using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel.Responses
{
    interface ITurnUpdateResponse : IResponse
    {
        int Turn { get; set; }
    }
}
