using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IGivePropertyRequest : IRequest
    {
        int Property { get; set; }
        string DestinationPlayer { get; set; }
    }
}
