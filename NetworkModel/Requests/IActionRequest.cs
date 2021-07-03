using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel.Requests
{
    public interface IActionRequest
    {
        Monopolio.Action Action { get; set; }
    }
}
