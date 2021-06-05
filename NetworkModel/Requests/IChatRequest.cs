using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IChatRequest : IRequest
    {
        string Message { get; set; }
    }
}
