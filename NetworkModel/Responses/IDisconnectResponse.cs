using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel.Responses
{
    public interface IDisconnectResponse
    {
        string Username { get; set; }

        //motivo?
    }
}
