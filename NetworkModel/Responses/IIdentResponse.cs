using Monopolio;
using Network.Attributes;
using Network.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    public interface IIdentResponse : IResponse
    {
        string Username { get; set; }

        State State { get; set; }
        bool Accepted { get; set; }
    }
}
