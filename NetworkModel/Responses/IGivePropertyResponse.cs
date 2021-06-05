using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IGivePropertyResponse : IResponse
    {
        string SourcePlayer { get; set; }
        string DestinationPlayer { get; set; }
        int Property { get; set; }
    }
}
