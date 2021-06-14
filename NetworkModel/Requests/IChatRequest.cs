using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IChatRequest : IRequest
    {
        string Msg { get; set; }
    }
}
