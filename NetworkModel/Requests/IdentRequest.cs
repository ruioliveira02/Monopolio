using Network.Packets;
using System;

namespace NetworkModel
{
    public class IdentRequest : RequestPacket
    {
        public IdentRequest(string username)
        {
            Username = username;
        }

        public string Username { get; set; }
    }
}
