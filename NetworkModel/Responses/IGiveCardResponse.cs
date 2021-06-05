using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IGiveCardResponse : IResponse
    {
        string SourcePlayer { get; set; }
        string DestinationPlayer { get; set; }
        string Card { get; set; } //TODO:: Change to card type
    }
}
