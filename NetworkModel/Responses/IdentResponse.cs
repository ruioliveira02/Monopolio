using Network.Attributes;
using Network.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel
{
    [PacketRequest(typeof(IdentRequest))]
    public class IdentResponse : ResponsePacket
    {
        public IdentResponse(bool accepted, RequestPacket request)
            : base(request)
        {
            Accepted = accepted;
        }

        public bool Accepted { get; set; }
    }
}
