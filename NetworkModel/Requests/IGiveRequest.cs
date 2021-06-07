using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IGiveRequest : IRequest
    {
        int Amount { get; set; }
        int DestinationPlayer { get; set; }
    }
}
