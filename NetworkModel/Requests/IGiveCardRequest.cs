using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IGiveCardRequest : IRequest
    {
        string Card { get; set; } //TODO:: Change to Card type
        string DestinationPlayer { get; set; }
    }
}
