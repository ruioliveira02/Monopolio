using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IGiveResponse : IResponse
    {
        string SourcePlayer { get; set; }
        string DestinationPlayer { get; set; }
        int Amount { get; set; }
    }
}
